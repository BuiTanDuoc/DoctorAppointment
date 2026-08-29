using System.Text;
using DoctorAppointmentApi.Data;
using DoctorAppointmentApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ---------- Configuration ----------
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.Configure<AdminSettings>(builder.Configuration.GetSection(AdminSettings.SectionName));
builder.Services.Configure<PaymentSettings>(builder.Configuration.GetSection(PaymentSettings.SectionName));

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt configuration section is missing.");

// ---------- Database ----------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------- App services ----------
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IJwtService, JwtService>();
builder.Services.AddSingleton<IPasswordHasherService, PasswordHasherService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IPaymentGatewayService, MockPaymentGatewayService>();

// ---------- CORS (both React frontends run on their own dev-server origins) ----------
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:5173", "http://localhost:5174" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ---------- Authentication ----------
// Three schemes because the existing React apps send their JWTs in plain custom
// headers ("token" / "dToken" / "aToken") instead of "Authorization: Bearer ...".
// See Services/AuthConstants.cs.
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));

void ConfigureCommonValidation(JwtBearerOptions options, string expectedRole, string headerName)
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = signingKey,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1),
        RoleClaimType = AuthConstants.RoleClaimType
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Headers[headerName].FirstOrDefault();
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var role = context.Principal?.FindFirst(AuthConstants.RoleClaimType)?.Value;
            if (!string.Equals(role, expectedRole, StringComparison.Ordinal))
            {
                context.Fail("Token role does not match this endpoint.");
            }
            return Task.CompletedTask;
        }
    };
}

builder.Services.AddAuthentication()
    .AddJwtBearer(AuthConstants.UserScheme, options =>
        ConfigureCommonValidation(options, AuthConstants.UserRole, AuthConstants.UserHeader))
    .AddJwtBearer(AuthConstants.DoctorScheme, options =>
        ConfigureCommonValidation(options, AuthConstants.DoctorRole, AuthConstants.DoctorHeader))
    .AddJwtBearer(AuthConstants.AdminScheme, options =>
        ConfigureCommonValidation(options, AuthConstants.AdminRole, AuthConstants.AdminHeader));

builder.Services.AddAuthorization();

// ---------- MVC / Swagger ----------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles(); // serves wwwroot/uploads/{doctors,users}

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
