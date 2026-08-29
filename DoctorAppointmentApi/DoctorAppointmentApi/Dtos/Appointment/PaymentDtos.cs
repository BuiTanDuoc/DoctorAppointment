using System.Text.Json.Serialization;

namespace DoctorAppointmentApi.Dtos.Appointment;

/// <summary>Shape of the object Razorpay Checkout hands back to the frontend's handler,
/// forwarded as-is to /api/user/verifyRazorpay.</summary>
public class RazorpayVerifyRequest
{
    [JsonPropertyName("razorpay_order_id")]
    public string RazorpayOrderId { get; set; } = string.Empty;

    [JsonPropertyName("razorpay_payment_id")]
    public string? RazorpayPaymentId { get; set; }

    [JsonPropertyName("razorpay_signature")]
    public string? RazorpaySignature { get; set; }
}

public class StripeVerifyRequest
{
    public bool Success { get; set; }
    public string AppointmentId { get; set; } = string.Empty;
}
