using Microsoft.AspNetCore.Identity;

namespace XRayne.Infrastructure.Auth;

public sealed class IdentityPasswordHasher : IPasswordHasher
{
    private static readonly object User = new();
    private readonly PasswordHasher<object> passwordHasher = new();

    public string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        return passwordHasher.HashPassword(User, password);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        var result = passwordHasher.VerifyHashedPassword(User, passwordHash, password);

        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
