using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace DoctorAppointmentApi.Services;

public interface IFileStorageService
{
    /// <summary>Resizes/crops the image to a fixed square, saves it under
    /// wwwroot/uploads/{folder}/ as a JPEG, and returns an absolute URL to it.</summary>
    Task<string> SaveAsync(IFormFile file, string folder);
}

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;
    private readonly IHttpContextAccessor _httpContextAccessor;

    // Every photo is normalized to this square size so doctor/user images render
    // at identical dimensions everywhere they're used (admin grid, admin list,
    // and the patient-facing site) - not just wherever CSS happens to crop them.
    private const int PhotoSize = 500;
    private const int JpegQuality = 85;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif"
    };

    public FileStorageService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
    {
        _env = env;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string> SaveAsync(IFormFile file, string folder)
    {
        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Unsupported image format. Use jpg, png, webp or gif.");
        }

        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var targetDir = Path.Combine(webRoot, "uploads", folder);
        Directory.CreateDirectory(targetDir);

        // Output is always .jpg regardless of the source format, so every stored
        // photo has the same dimensions AND the same encoding.
        var fileName = $"{Guid.NewGuid()}.jpg";
        var fullPath = Path.Combine(targetDir, fileName);

        try
        {
            await using var input = file.OpenReadStream();
            using var image = await Image.LoadAsync(input);

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Crop, // center-crop rather than squash/stretch
                Size = new Size(PhotoSize, PhotoSize),
                Position = AnchorPositionMode.Center
            }));

            await image.SaveAsJpegAsync(fullPath, new JpegEncoder { Quality = JpegQuality });
        }
        catch (UnknownImageFormatException)
        {
            throw new InvalidOperationException("The uploaded file isn't a valid image.");
        }

        var request = _httpContextAccessor.HttpContext?.Request;
        var baseUrl = request is null ? string.Empty : $"{request.Scheme}://{request.Host}";

        return $"{baseUrl}/uploads/{folder}/{fileName}";
    }
}

