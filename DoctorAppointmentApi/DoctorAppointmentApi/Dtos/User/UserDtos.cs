using System.Text.Json.Serialization;
using DoctorAppointmentApi.Dtos.Common;

namespace DoctorAppointmentApi.Dtos.User;

public class UserDto
{
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public AddressDto Address { get; set; } = new();
    public string Gender { get; set; } = string.Empty;
    public string Dob { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

/// <summary>Bound from multipart/form-data posted by MyProfile.jsx's updateUserProfileData().</summary>
public class UpdateUserProfileRequest
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    /// <summary>JSON string: {"line1":"...","line2":"..."}</summary>
    public string Address { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string Dob { get; set; } = string.Empty;
    public IFormFile? Image { get; set; }
}
