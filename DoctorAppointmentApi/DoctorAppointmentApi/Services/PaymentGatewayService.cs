using Microsoft.Extensions.Options;

namespace DoctorAppointmentApi.Services;

public record RazorpayOrder(string Id, long Amount, string Currency, string Receipt);

public interface IPaymentGatewayService
{
    /// <summary>Creates a payment "order" for an appointment. Replace the body with a real
    /// Razorpay SDK call (Razorpay.Api.Order.Create) once you have live API keys.</summary>
    Task<RazorpayOrder> CreateRazorpayOrderAsync(int appointmentId, decimal amount);

    /// <summary>Builds the URL the frontend redirects to after "payment". Replace the body with a real
    /// Stripe.net Checkout Session (Stripe.Checkout.SessionService) once you have live API keys.</summary>
    Task<string> CreateStripeCheckoutUrlAsync(int appointmentId, decimal amount);
}

/// <summary>
/// Local-dev stand-in for the two payment gateways the original frontend integrates with.
/// It never talks to Razorpay/Stripe - it just returns shapes the existing React code already
/// knows how to consume, so the booking flow works end-to-end without live payment credentials.
/// Swap this implementation out (see the interface above) before going to production.
/// </summary>
public class MockPaymentGatewayService : IPaymentGatewayService
{
    private readonly PaymentSettings _settings;

    public MockPaymentGatewayService(IOptions<PaymentSettings> settings)
    {
        _settings = settings.Value;
    }

    public Task<RazorpayOrder> CreateRazorpayOrderAsync(int appointmentId, decimal amount)
    {
        // Razorpay expects amounts in the smallest currency unit (e.g. paise).
        var order = new RazorpayOrder(
            Id: $"order_mock_{Guid.NewGuid():N}",
            Amount: (long)(amount * 100),
            Currency: "INR",
            Receipt: appointmentId.ToString());

        return Task.FromResult(order);
    }

    public Task<string> CreateStripeCheckoutUrlAsync(int appointmentId, decimal amount)
    {
        var url = $"{_settings.FrontendUrl.TrimEnd('/')}/verify?success=true&appointmentId={appointmentId}";
        return Task.FromResult(url);
    }
}
