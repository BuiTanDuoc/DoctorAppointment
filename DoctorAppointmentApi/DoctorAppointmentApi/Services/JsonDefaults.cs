using System.Text.Json;

namespace DoctorAppointmentApi.Services;

/// <summary>
/// JsonSerializer.Deserialize&lt;T&gt;(json) uses case-*sensitive* property matching
/// by default (unlike ASP.NET Core's MVC pipeline, which applies camelCase +
/// case-insensitive matching automatically). Controllers that manually deserialize
/// a JSON string field from multipart/form-data (e.g. the "address" field posted
/// alongside a doctor/user photo) must pass these options explicitly, or a payload
/// like {"line1":"...","line2":"..."} silently fails to match the C# `Line1`/`Line2`
/// properties and every field ends up empty instead of throwing.
/// </summary>
public static class JsonDefaults
{
    public static readonly JsonSerializerOptions CaseInsensitive = new()
    {
        PropertyNameCaseInsensitive = true
    };
}
