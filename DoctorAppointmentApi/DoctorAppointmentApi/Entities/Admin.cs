namespace DoctorAppointmentApi.Entities;

public class Admin
{
    public int Id { get; set; }
    public string Name { get; set; } = "Admin";
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
