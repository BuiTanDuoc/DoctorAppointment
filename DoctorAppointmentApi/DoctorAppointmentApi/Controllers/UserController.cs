using System.Security.Claims;
using System.Text.Json;
using DoctorAppointmentApi.Data;
using DoctorAppointmentApi.Dtos.Appointment;
using DoctorAppointmentApi.Dtos.Auth;
using DoctorAppointmentApi.Dtos.Common;
using DoctorAppointmentApi.Dtos.User;
using DoctorAppointmentApi.Entities;
using DoctorAppointmentApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoctorAppointmentApi.Controllers;

[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly IFileStorageService _fileStorage;
    private readonly IPaymentGatewayService _paymentGateway;

    public UserController(
        ApplicationDbContext db,
        IJwtService jwtService,
        IPasswordHasherService passwordHasher,
        IFileStorageService fileStorage,
        IPaymentGatewayService paymentGateway)
    {
        _db = db;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _fileStorage = fileStorage;
        _paymentGateway = paymentGateway;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.Claims.First(c => c.Type == "sub").Value);

    // POST /api/user/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return Ok(new ErrorResponse { Message = "Please enter a valid name, email and a password of at least 8 characters" });
        }

        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
        {
            return Ok(new ErrorResponse { Message = "An account with this email already exists" });
        }

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = _jwtService.GenerateToken(user.Id.ToString(), user.Email, AuthConstants.UserRole);
        return Ok(new { success = true, token });
    }

    // POST /api/user/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user is null || !_passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            return Ok(new ErrorResponse { Message = "Invalid credentials" });
        }

        var token = _jwtService.GenerateToken(user.Id.ToString(), user.Email, AuthConstants.UserRole);
        return Ok(new { success = true, token });
    }

    // GET /api/user/get-profile
    [HttpGet("get-profile")]
    [Authorize(AuthenticationSchemes = AuthConstants.UserScheme)]
    public async Task<IActionResult> GetProfile()
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == CurrentUserId);
        if (user is null)
        {
            return Ok(new ErrorResponse { Message = "User not found" });
        }

        return Ok(new { success = true, userData = user.ToDto() });
    }

    // POST /api/user/update-profile (multipart/form-data)
    [HttpPost("update-profile")]
    [Authorize(AuthenticationSchemes = AuthConstants.UserScheme)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateUserProfileRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == CurrentUserId);
        if (user is null)
        {
            return Ok(new ErrorResponse { Message = "User not found" });
        }

        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Phone) || string.IsNullOrWhiteSpace(request.Address))
        {
            return Ok(new ErrorResponse { Message = "Name, phone and address are required" });
        }

        var address = JsonSerializer.Deserialize<Dtos.Common.AddressDto>(request.Address) ?? new Dtos.Common.AddressDto();

        user.Name = request.Name;
        user.Phone = request.Phone;
        user.AddressLine1 = address.Line1;
        user.AddressLine2 = address.Line2;
        user.Gender = request.Gender;
        user.Dob = request.Dob;

        if (request.Image is not null)
        {
            try
            {
                user.Image = await _fileStorage.SaveAsync(request.Image, "users");
            }
            catch (InvalidOperationException ex)
            {
                return Ok(new ErrorResponse { Message = ex.Message });
            }
        }

        await _db.SaveChangesAsync();

        return Ok(new { success = true, message = "Profile updated" });
    }

    // POST /api/user/book-appointment
    [HttpPost("book-appointment")]
    [Authorize(AuthenticationSchemes = AuthConstants.UserScheme)]
    public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentRequest request)
    {
        if (!int.TryParse(request.DocId, out var docId))
        {
            return Ok(new ErrorResponse { Message = "Invalid doctor id" });
        }

        var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.Id == docId);
        if (doctor is null)
        {
            return Ok(new ErrorResponse { Message = "Doctor not found" });
        }

        if (!doctor.Available)
        {
            return Ok(new ErrorResponse { Message = "Doctor is not available" });
        }

        if (AdminSlotHelper.IsBooked(doctor, request.SlotDate, request.SlotTime))
        {
            return Ok(new ErrorResponse { Message = "This slot is no longer available" });
        }

        var appointment = new Appointment
        {
            UserId = CurrentUserId,
            DoctorId = docId,
            SlotDate = request.SlotDate,
            SlotTime = request.SlotTime,
            Amount = doctor.Fees
        };

        AdminSlotHelper.Book(doctor, request.SlotDate, request.SlotTime);

        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync();

        return Ok(new { success = true, message = "Appointment booked" });
    }

    // GET /api/user/appointments
    [HttpGet("appointments")]
    [Authorize(AuthenticationSchemes = AuthConstants.UserScheme)]
    public async Task<IActionResult> Appointments()
    {
        var appointments = await _db.Appointments
            .Include(a => a.User)
            .Include(a => a.Doctor)
            .Where(a => a.UserId == CurrentUserId)
            .OrderByDescending(a => a.Id)
            .ToListAsync();

        return Ok(new { success = true, appointments = appointments.Select(a => a.ToDto()) });
    }

    // POST /api/user/cancel-appointment
    [HttpPost("cancel-appointment")]
    [Authorize(AuthenticationSchemes = AuthConstants.UserScheme)]
    public async Task<IActionResult> CancelAppointment([FromBody] AppointmentIdRequest request)
    {
        if (!int.TryParse(request.AppointmentId, out var appointmentId))
        {
            return Ok(new ErrorResponse { Message = "Invalid appointment id" });
        }

        var appointment = await _db.Appointments.Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.UserId == CurrentUserId);

        if (appointment is null)
        {
            return Ok(new ErrorResponse { Message = "Appointment not found" });
        }

        appointment.Cancelled = true;
        AdminSlotHelper.Release(appointment);
        await _db.SaveChangesAsync();

        return Ok(new { success = true, message = "Appointment cancelled" });
    }

    // POST /api/user/payment-razorpay
    [HttpPost("payment-razorpay")]
    [Authorize(AuthenticationSchemes = AuthConstants.UserScheme)]
    public async Task<IActionResult> PaymentRazorpay([FromBody] AppointmentIdRequest request)
    {
        var appointment = await ValidatePayableAppointment(request.AppointmentId);
        if (appointment is null)
        {
            return Ok(new ErrorResponse { Message = "Appointment cancelled or not found" });
        }

        var order = await _paymentGateway.CreateRazorpayOrderAsync(appointment.Id, appointment.Amount);

        appointment.PaymentMethod = "razorpay";
        appointment.PaymentReference = order.Id;
        await _db.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            order = new { id = order.Id, amount = order.Amount, currency = order.Currency, receipt = order.Receipt }
        });
    }

    // POST /api/user/verifyRazorpay
    [HttpPost("verifyRazorpay")]
    [Authorize(AuthenticationSchemes = AuthConstants.UserScheme)]
    public async Task<IActionResult> VerifyRazorpay([FromBody] RazorpayVerifyRequest request)
    {
        var appointment = await _db.Appointments.FirstOrDefaultAsync(a =>
            a.UserId == CurrentUserId && a.PaymentReference == request.RazorpayOrderId);

        if (appointment is null)
        {
            return Ok(new ErrorResponse { Message = "Payment record not found" });
        }

        appointment.Payment = true;
        await _db.SaveChangesAsync();

        return Ok(new { success = true, message = "Payment successful" });
    }

    // POST /api/user/payment-stripe
    [HttpPost("payment-stripe")]
    [Authorize(AuthenticationSchemes = AuthConstants.UserScheme)]
    public async Task<IActionResult> PaymentStripe([FromBody] AppointmentIdRequest request)
    {
        var appointment = await ValidatePayableAppointment(request.AppointmentId);
        if (appointment is null)
        {
            return Ok(new ErrorResponse { Message = "Appointment cancelled or not found" });
        }

        var sessionUrl = await _paymentGateway.CreateStripeCheckoutUrlAsync(appointment.Id, appointment.Amount);

        appointment.PaymentMethod = "stripe";
        await _db.SaveChangesAsync();

        return Ok(new { success = true, session_url = sessionUrl });
    }

    // POST /api/user/verifyStripe
    [HttpPost("verifyStripe")]
    [Authorize(AuthenticationSchemes = AuthConstants.UserScheme)]
    public async Task<IActionResult> VerifyStripe([FromBody] StripeVerifyRequest request)
    {
        if (!int.TryParse(request.AppointmentId, out var appointmentId))
        {
            return Ok(new ErrorResponse { Message = "Invalid appointment id" });
        }

        var appointment = await _db.Appointments.FirstOrDefaultAsync(a =>
            a.Id == appointmentId && a.UserId == CurrentUserId);

        if (appointment is null)
        {
            return Ok(new ErrorResponse { Message = "Appointment not found" });
        }

        if (request.Success)
        {
            appointment.Payment = true;
            await _db.SaveChangesAsync();
            return Ok(new { success = true, message = "Payment successful" });
        }

        return Ok(new { success = false, message = "Payment failed" });
    }

    private async Task<Appointment?> ValidatePayableAppointment(string appointmentIdRaw)
    {
        if (!int.TryParse(appointmentIdRaw, out var appointmentId))
        {
            return null;
        }

        var appointment = await _db.Appointments.FirstOrDefaultAsync(a =>
            a.Id == appointmentId && a.UserId == CurrentUserId && !a.Cancelled);

        return appointment;
    }
}
