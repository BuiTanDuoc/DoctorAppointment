namespace DoctorAppointmentApi.Entities;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public string Image { get; set; } = "https://ui-avatars.com/api/?name=User";

    // Stored as plain fields rather than an owned type so partial updates are simple.
    public string AddressLine1 { get; set; } = string.Empty;
    public string AddressLine2 { get; set; } = string.Empty;

    public string Gender { get; set; } = "Not Selected";
    public string Dob { get; set; } = "Not Selected"; // kept as string, e.g. "1998-05-02", to mirror the frontend's free-text field
    public string Phone { get; set; } = "0000000000";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
