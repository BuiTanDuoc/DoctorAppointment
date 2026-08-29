using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DoctorAppointmentApi.Dtos.Common;

namespace DoctorAppointmentApi.Dtos.Doctor;

/// <summary>
/// Public-facing doctor shape used by /api/doctor/list, /api/admin/all-doctors,
/// and the doctor's own /api/doctor/profile. Property names/casing match what
/// AppContext.jsx, Appointment.jsx, DoctorProfile.jsx etc. already read.
/// </summary>
public class DoctorDto
{
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string Speciality { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string Experience { get; set; } = string.Empty;
    public string About { get; set; } = string.Empty;
    public bool Available { get; set; }
    public decimal Fees { get; set; }
    public AddressDto Address { get; set; } = new();

    [JsonPropertyName("slots_booked")]
    public Dictionary<string, List<string>> SlotsBooked { get; set; } = new();
}

public class DoctorLoginRequest
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}

/// <summary>Bound from multipart/form-data posted by AddDoctor.jsx.</summary>
public class AddDoctorRequest
{
    [Required] public string Name { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, MinLength(8)] public string Password { get; set; } = string.Empty;
    [Required] public string Experience { get; set; } = string.Empty;
    [Required] public decimal Fees { get; set; }
    [Required] public string About { get; set; } = string.Empty;
    [Required] public string Speciality { get; set; } = string.Empty;
    [Required] public string Degree { get; set; } = string.Empty;

    /// <summary>JSON string: {"line1":"...","line2":"..."} - sent that way by AddDoctor.jsx.</summary>
    [Required] public string Address { get; set; } = string.Empty;

    public IFormFile? Image { get; set; }
}

/// <summary>Posted as JSON by DoctorProfile.jsx's updateProfile().</summary>
public class UpdateDoctorProfileRequest
{
    public AddressDto Address { get; set; } = new();
    public decimal Fees { get; set; }
    public string About { get; set; } = string.Empty;
    public bool Available { get; set; }
}

/// <summary>Bound from multipart/form-data posted by the admin's EditDoctor page.
/// Unlike UpdateDoctorProfileRequest (doctor's own self-service update), this lets
/// the admin change identity/listing fields too - everything except email/password,
/// which stay tied to the doctor's own login.</summary>
public class UpdateDoctorAdminRequest
{
    [Required] public string DocId { get; set; } = string.Empty;
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string Speciality { get; set; } = string.Empty;
    [Required] public string Degree { get; set; } = string.Empty;
    [Required] public string Experience { get; set; } = string.Empty;
    [Required] public decimal Fees { get; set; }
    [Required] public string About { get; set; } = string.Empty;

    /// <summary>JSON string: {"line1":"...","line2":"..."}</summary>
    [Required] public string Address { get; set; } = string.Empty;
    public bool Available { get; set; }

    /// <summary>Optional - only sent when the admin picks a new photo.</summary>
    public IFormFile? Image { get; set; }
}

public class DoctorDashboardDto
{
    public decimal Earnings { get; set; }
    public int Appointments { get; set; }
    public int Patients { get; set; }
    public List<Appointment.AppointmentDto> LatestAppointments { get; set; } = new();
}
