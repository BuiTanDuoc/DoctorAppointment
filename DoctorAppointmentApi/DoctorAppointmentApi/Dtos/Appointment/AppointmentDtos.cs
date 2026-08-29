using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DoctorAppointmentApi.Dtos.Doctor;
using DoctorAppointmentApi.Dtos.User;

namespace DoctorAppointmentApi.Dtos.Appointment;

/// <summary>
/// Matches item.docData / item.userData / item.slotDate / item.slotTime / item.amount /
/// item.cancelled / item.payment / item.isCompleted as read by MyAppointments.jsx,
/// AllAppointments.jsx and DoctorAppointments.jsx.
/// </summary>
public class AppointmentDto
{
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;

    public string SlotDate { get; set; } = string.Empty;
    public string SlotTime { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool Cancelled { get; set; }
    public bool Payment { get; set; }
    public bool IsCompleted { get; set; }

    public UserDto UserData { get; set; } = new();
    public DoctorDto DocData { get; set; } = new();
}

public class BookAppointmentRequest
{
    [Required] public string DocId { get; set; } = string.Empty;
    [Required] public string SlotDate { get; set; } = string.Empty;
    [Required] public string SlotTime { get; set; } = string.Empty;
}

public class AppointmentIdRequest
{
    [Required] public string AppointmentId { get; set; } = string.Empty;
}
