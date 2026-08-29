using System.ComponentModel.DataAnnotations;

namespace DoctorAppointmentApi.Dtos.Admin;

public class AdminLoginRequest
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}

public class ChangeAvailabilityRequest
{
    [Required] public string DocId { get; set; } = string.Empty;
}

public class AdminDashboardDto
{
    public int Doctors { get; set; }
    public int Appointments { get; set; }
    public int Patients { get; set; }
    public List<Appointment.AppointmentDto> LatestAppointments { get; set; } = new();
}
