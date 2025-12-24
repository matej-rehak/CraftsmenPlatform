namespace CraftsmenPlatform.Application.Common.Interfaces;

/// <summary>
/// Služba pro hashování a ověřování hesel
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashovat heslo pomocí BCrypt
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// Ověřit heslo proti hashi
    /// </summary>
    bool VerifyPassword(string password, string passwordHash);
}