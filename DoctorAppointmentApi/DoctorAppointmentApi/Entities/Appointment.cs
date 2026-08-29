namespace DoctorAppointmentApi.Entities;

public class Appointment
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public int DoctorId { get; set; }
    public Doctor? Doctor { get; set; }

    /// <summary>Format "d_m_yyyy", e.g. "5_8_2026" - matches the frontend's slot key format.</summary>
    public string SlotDate { get; set; } = string.Empty;

    /// <summary>Format "hh:mm AM/PM", e.g. "10:30 AM".</summary>
    public string SlotTime { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public bool Cancelled { get; set; }
    public bool Payment { get; set; }
    public bool IsCompleted { get; set; }

    /// <summary>"razorpay" or "stripe", set once a payment session/order is created.</summary>
    public string? PaymentMethod { get; set; }
    public string? PaymentReference { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
