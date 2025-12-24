using CraftsmenPlatform.Application.Common.Interfaces;

namespace CraftsmenPlatform.Infrastructure.Services;

/// <summary>
/// Password hasher using BCrypt.Net
/// Install: dotnet add package BCrypt.Net-Next --version 4.0.3
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        // BCrypt automatically generates a salt and includes it in the hash
        // WorkFactor 12 = 2^12 iterations (good balance between security and performance)
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch
        {
            // Invalid hash format
            return false;
        }
    }
}