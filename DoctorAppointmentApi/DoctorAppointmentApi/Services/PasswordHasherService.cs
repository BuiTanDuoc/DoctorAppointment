using Microsoft.AspNetCore.Identity;

namespace DoctorAppointmentApi.Services;

public interface IPasswordHasherService
{
    string Hash(string password);
    bool Verify(string hash, string providedPassword);
}

/// <summary>
/// Thin wrapper around ASP.NET Core Identity's PasswordHasher so we get its
/// battle-tested PBKDF2 hashing without pulling in the full Identity/EF store.
/// </summary>
public class PasswordHasherService : IPasswordHasherService
{
    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string password) => _hasher.HashPassword(new object(), password);

    public bool Verify(string hash, string providedPassword)
    {
        var result = _hasher.VerifyHashedPassword(new object(), hash, providedPassword);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
