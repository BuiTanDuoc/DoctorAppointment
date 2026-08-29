namespace DoctorAppointmentApi.Services;

public class PaymentSettings
{
    public const string SectionName = "Payments";

    /// <summary>Base URL of the patient-facing frontend, used to build the Stripe-style redirect link.</summary>
    public string FrontendUrl { get; set; } = "http://localhost:5173";

    public string Currency { get; set; } = "vnd";
}
