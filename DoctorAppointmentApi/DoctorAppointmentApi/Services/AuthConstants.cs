namespace DoctorAppointmentApi.Services;

/// <summary>
/// The React frontends (unmodified) send their tokens in bare custom headers -
/// "token" for patients, "dToken" for doctors, "aToken" for the admin - rather
/// than a standard "Authorization: Bearer ..." header. To keep those apps as-is,
/// we register three JWT bearer schemes, one per header/role, instead of one.
/// </summary>
public static class AuthConstants
{
    public const string UserScheme = "UserScheme";
    public const string DoctorScheme = "DoctorScheme";
    public const string AdminScheme = "AdminScheme";

    public const string UserHeader = "token";
    public const string DoctorHeader = "dToken";
    public const string AdminHeader = "aToken";

    public const string UserRole = "User";
    public const string DoctorRole = "Doctor";
    public const string AdminRole = "Admin";

    public const string RoleClaimType = "role";
}
