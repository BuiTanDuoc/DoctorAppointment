namespace DoctorAppointmentApi.Services;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryHours { get; set; } = 24;
}

public class AdminSettings
{
    public const string SectionName = "Admin";

    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
