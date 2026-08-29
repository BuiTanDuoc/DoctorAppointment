namespace DoctorAppointmentApi.Dtos.Common;

// The frontend (unmodified) only ever checks `data.success` and `data.message`,
// plus whatever named payload property a given endpoint returns (e.g. data.doctors,
// data.userData, data.token...). Controllers build anonymous objects like
// new { success = true, token } so each endpoint's JSON shape matches exactly
// what the React apps already expect - no extra wrapper type needed here.
public class ErrorResponse
{
    public bool Success { get; set; } = false;
    public string Message { get; set; } = string.Empty;
}
