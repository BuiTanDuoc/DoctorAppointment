namespace DoctorAppointmentApi.Services;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryHours { get; set; } = 24;
}

/// <summary>
/// Used only to seed a default row in the Admins table on first run when it's
/// empty. Once an Admin row exists, login is validated against the database
/// (see AdminController.Login), not this config.
/// </summary>
public class AdminSettings
{
    public const string SectionName = "Admin";

    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
