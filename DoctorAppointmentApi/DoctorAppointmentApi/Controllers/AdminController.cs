using System.Text.Json;
using DoctorAppointmentApi.Data;
using DoctorAppointmentApi.Dtos.Admin;
using DoctorAppointmentApi.Dtos.Common;
using DoctorAppointmentApi.Dtos.Doctor;
using DoctorAppointmentApi.Entities;
using DoctorAppointmentApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DoctorAppointmentApi.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly IFileStorageService _fileStorage;
    private readonly AdminSettings _adminSettings;

    public AdminController(
        ApplicationDbContext db,
        IJwtService jwtService,
        IPasswordHasherService passwordHasher,
        IFileStorageService fileStorage,
        IOptions<AdminSettings> adminSettings)
    {
        _db = db;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _fileStorage = fileStorage;
        _adminSettings = adminSettings.Value;
    }

    // POST /api/admin/login
    [HttpPost("login")]
    public IActionResult Login([FromBody] AdminLoginRequest request)
    {
        if (!string.Equals(request.Email, _adminSettings.Email, StringComparison.OrdinalIgnoreCase) ||
            request.Password != _adminSettings.Password)
        {
            return Ok(new ErrorResponse { Message = "Invalid credentials" });
        }

        var token = _jwtService.GenerateToken("admin", _adminSettings.Email, AuthConstants.AdminRole);
        return Ok(new { success = true, token });
    }

    // POST /api/admin/add-doctor  (multipart/form-data)
    [HttpPost("add-doctor")]
    [Authorize(AuthenticationSchemes = AuthConstants.AdminScheme)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AddDoctor([FromForm] AddDoctorRequest request)
    {
        if (!ModelState.IsValid)
        {
            return Ok(new ErrorResponse { Message = "Missing required doctor details" });
        }

        if (await _db.Doctors.AnyAsync(d => d.Email == request.Email))
        {
            return Ok(new ErrorResponse { Message = "A doctor with this email already exists" });
        }

        if (request.Password.Length < 8)
        {
            return Ok(new ErrorResponse { Message = "Please enter a strong password (min 8 characters)" });
        }

        string imageUrl;
        try
        {
            imageUrl = request.Image is not null
                ? await _fileStorage.SaveAsync(request.Image, "doctors")
                : string.Empty;
        }
        catch (InvalidOperationException ex)
        {
            return Ok(new ErrorResponse { Message = ex.Message });
        }

        var address = JsonSerializer.Deserialize<Dtos.Common.AddressDto>(request.Address) ?? new Dtos.Common.AddressDto();

        var doctor = new Doctor
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Image = imageUrl,
            Speciality = request.Speciality,
            Degree = request.Degree,
            Experience = request.Experience,
            About = request.About,
            Fees = request.Fees,
            AddressLine1 = address.Line1,
            AddressLine2 = address.Line2,
            Available = true
        };

        _db.Doctors.Add(doctor);
        await _db.SaveChangesAsync();

        return Ok(new { success = true, message = "Doctor added" });
    }

    // GET /api/admin/all-doctors
    [HttpGet("all-doctors")]
    [Authorize(AuthenticationSchemes = AuthConstants.AdminScheme)]
    public async Task<IActionResult> AllDoctors()
    {
        var doctors = await _db.Doctors.AsNoTracking().OrderByDescending(d => d.Id).ToListAsync();
        return Ok(new { success = true, doctors = doctors.Select(d => d.ToDto()) });
    }

    // POST /api/admin/change-availability
    [HttpPost("change-availability")]
    [Authorize(AuthenticationSchemes = AuthConstants.AdminScheme)]
    public async Task<IActionResult> ChangeAvailability([FromBody] ChangeAvailabilityRequest request)
    {
        if (!int.TryParse(request.DocId, out var docId))
        {
            return Ok(new ErrorResponse { Message = "Invalid doctor id" });
        }

        var doctor = await _db.Doctors.FindAsync(docId);
        if (doctor is null)
        {
            return Ok(new ErrorResponse { Message = "Doctor not found" });
        }

        doctor.Available = !doctor.Available;
        await _db.SaveChangesAsync();

        return Ok(new { success = true, message = "Availability changed" });
    }

    // GET /api/admin/appointments
    [HttpGet("appointments")]
    [Authorize(AuthenticationSchemes = AuthConstants.AdminScheme)]
    public async Task<IActionResult> AllAppointments()
    {
        var appointments = await _db.Appointments
            .Include(a => a.User)
            .Include(a => a.Doctor)
            .OrderByDescending(a => a.Id)
            .ToListAsync();

        return Ok(new { success = true, appointments = appointments.Select(a => a.ToDto()) });
    }

    // POST /api/admin/cancel-appointment
    [HttpPost("cancel-appointment")]
    [Authorize(AuthenticationSchemes = AuthConstants.AdminScheme)]
    public async Task<IActionResult> CancelAppointment([FromBody] Dtos.Appointment.AppointmentIdRequest request)
    {
        if (!int.TryParse(request.AppointmentId, out var appointmentId))
        {
            return Ok(new ErrorResponse { Message = "Invalid appointment id" });
        }

        var appointment = await _db.Appointments.Include(a => a.Doctor).FirstOrDefaultAsync(a => a.Id == appointmentId);
        if (appointment is null)
        {
            return Ok(new ErrorResponse { Message = "Appointment not found" });
        }

        appointment.Cancelled = true;
        AdminSlotHelper.Release(appointment);
        await _db.SaveChangesAsync();

        return Ok(new { success = true, message = "Appointment cancelled" });
    }

    // GET /api/admin/dashboard
    [HttpGet("dashboard")]
    [Authorize(AuthenticationSchemes = AuthConstants.AdminScheme)]
    public async Task<IActionResult> Dashboard()
    {
        var doctorCount = await _db.Doctors.CountAsync();
        var patientCount = await _db.Users.CountAsync();

        var appointments = await _db.Appointments
            .Include(a => a.User)
            .Include(a => a.Doctor)
            .OrderByDescending(a => a.Id)
            .ToListAsync();

        var dashData = new AdminDashboardDto
        {
            Doctors = doctorCount,
            Patients = patientCount,
            Appointments = appointments.Count,
            LatestAppointments = appointments.Take(5).Select(a => a.ToDto()).ToList()
        };

        return Ok(new { success = true, dashData });
    }
}
