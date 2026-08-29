using System.Text.Json;

namespace DoctorAppointmentApi.Entities;

public class Doctor
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public string Image { get; set; } = string.Empty;
    public string Speciality { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string Experience { get; set; } = "1 Year";
    public string About { get; set; } = string.Empty;
    public bool Available { get; set; } = true;
    public decimal Fees { get; set; }

    public string AddressLine1 { get; set; } = string.Empty;
    public string AddressLine2 { get; set; } = string.Empty;

    /// <summary>
    /// Serialized JSON dictionary of "d_m_yyyy" -> list of booked "hh:mm AM/PM" slot strings,
    /// mirroring the slots_booked map the frontend already knows how to read.
    /// </summary>
    public string SlotsBookedJson { get; set; } = "{}";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public Dictionary<string, List<string>> GetSlotsBooked()
    {
        if (string.IsNullOrWhiteSpace(SlotsBookedJson))
        {
            return new Dictionary<string, List<string>>();
        }

        return JsonSerializer.Deserialize<Dictionary<string, List<string>>>(SlotsBookedJson)
               ?? new Dictionary<string, List<string>>();
    }

    public void SetSlotsBooked(Dictionary<string, List<string>> slots)
    {
        SlotsBookedJson = JsonSerializer.Serialize(slots);
    }
}
