using System.Security.Claims;
using DoctorAppointmentApi.Data;
using DoctorAppointmentApi.Dtos.Appointment;
using DoctorAppointmentApi.Dtos.Common;
using DoctorAppointmentApi.Dtos.Doctor;
using DoctorAppointmentApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoctorAppointmentApi.Controllers;

[ApiController]
[Route("api/doctor")]
public class DoctorController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasherService _passwordHasher;

    public DoctorController(ApplicationDbContext db, IJwtService jwtService, IPasswordHasherService passwordHasher)
    {
        _db = db;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
    }

    private int CurrentDoctorId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.Claims.First(c => c.Type == "sub").Value);

    // POST /api/doctor/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] DoctorLoginRequest request)
    {
        var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.Email == request.Email);
        if (doctor is null || !_passwordHasher.Verify(doctor.PasswordHash, request.Password))
        {
            return Ok(new ErrorResponse { Message = "Invalid credentials" });
        }

        var token = _jwtService.GenerateToken(doctor.Id.ToString(), doctor.Email, AuthConstants.DoctorRole);
        return Ok(new { success = true, token });
    }

    // GET /api/doctor/list (public)
    [HttpGet("list")]
    [AllowAnonymous]
    public async Task<IActionResult> List()
    {
        var doctors = await _db.Doctors.AsNoTracking().ToListAsync();
        var dtos = doctors.Select(d =>
        {
            var dto = d.ToDto();
            dto.Email = string.Empty; // public listing hides contact details, mirroring the original app
            return dto;
        });

        return Ok(new { success = true, doctors = dtos });
    }

    // GET /api/doctor/appointments
    [HttpGet("appointments")]
    [Authorize(AuthenticationSchemes = AuthConstants.DoctorScheme)]
    public async Task<IActionResult> Appointments()
    {
        var appointments = await _db.Appointments
            .Include(a => a.User)
            .Include(a => a.Doctor)
            .Where(a => a.DoctorId == CurrentDoctorId)
            .OrderByDescending(a => a.Id)
            .ToListAsync();

        return Ok(new { success = true, appointments = appointments.Select(a => a.ToDto()) });
    }

    // POST /api/doctor/cancel-appointment
    [HttpPost("cancel-appointment")]
    [Authorize(AuthenticationSchemes = AuthConstants.DoctorScheme)]
    public async Task<IActionResult> CancelAppointment([FromBody] AppointmentIdRequest request)
    {
        if (!int.TryParse(request.AppointmentId, out var appointmentId))
        {
            return Ok(new ErrorResponse { Message = "Invalid appointment id" });
        }

        var appointment = await _db.Appointments.Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.DoctorId == CurrentDoctorId);

        if (appointment is null)
        {
            return Ok(new ErrorResponse { Message = "Appointment not found" });
        }

        appointment.Cancelled = true;
        AdminSlotHelper.Release(appointment);
        await _db.SaveChangesAsync();

        return Ok(new { success = true, message = "Appointment cancelled" });
    }

    // POST /api/doctor/complete-appointment
    [HttpPost("complete-appointment")]
    [Authorize(AuthenticationSchemes = AuthConstants.DoctorScheme)]
    public async Task<IActionResult> CompleteAppointment([FromBody] AppointmentIdRequest request)
    {
        if (!int.TryParse(request.AppointmentId, out var appointmentId))
        {
            return Ok(new ErrorResponse { Message = "Invalid appointment id" });
        }

        var appointment = await _db.Appointments
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.DoctorId == CurrentDoctorId);

        if (appointment is null)
        {
            return Ok(new ErrorResponse { Message = "Appointment not found" });
        }

        appointment.IsCompleted = true;
        await _db.SaveChangesAsync();

        return Ok(new { success = true, message = "Appointment completed" });
    }

    // GET /api/doctor/dashboard
    [HttpGet("dashboard")]
    [Authorize(AuthenticationSchemes = AuthConstants.DoctorScheme)]
    public async Task<IActionResult> Dashboard()
    {
        var appointments = await _db.Appointments
            .Include(a => a.User)
            .Include(a => a.Doctor)
            .Where(a => a.DoctorId == CurrentDoctorId)
            .OrderByDescending(a => a.Id)
            .ToListAsync();

        var earnings = appointments.Where(a => a.IsCompleted || a.Payment).Sum(a => a.Amount);
        var uniquePatients = appointments.Select(a => a.UserId).Distinct().Count();

        var dashData = new DoctorDashboardDto
        {
            Earnings = earnings,
            Appointments = appointments.Count,
            Patients = uniquePatients,
            LatestAppointments = appointments.Take(5).Select(a => a.ToDto()).ToList()
        };

        return Ok(new { success = true, dashData });
    }

    // GET /api/doctor/profile
    [HttpGet("profile")]
    [Authorize(AuthenticationSchemes = AuthConstants.DoctorScheme)]
    public async Task<IActionResult> Profile()
    {
        var doctor = await _db.Doctors.AsNoTracking().FirstOrDefaultAsync(d => d.Id == CurrentDoctorId);
        if (doctor is null)
        {
            return Ok(new ErrorResponse { Message = "Doctor not found" });
        }

        return Ok(new { success = true, profileData = doctor.ToDto() });
    }

    // POST /api/doctor/update-profile
    [HttpPost("update-profile")]
    [Authorize(AuthenticationSchemes = AuthConstants.DoctorScheme)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateDoctorProfileRequest request)
    {
        var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.Id == CurrentDoctorId);
        if (doctor is null)
        {
            return Ok(new ErrorResponse { Message = "Doctor not found" });
        }

        doctor.Fees = request.Fees;
        doctor.About = request.About;
        doctor.Available = request.Available;
        doctor.AddressLine1 = request.Address.Line1;
        doctor.AddressLine2 = request.Address.Line2;

        await _db.SaveChangesAsync();

        return Ok(new { success = true, message = "Profile updated" });
    }
}
