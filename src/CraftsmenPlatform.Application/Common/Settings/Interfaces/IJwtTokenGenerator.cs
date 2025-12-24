using CraftsmenPlatform.Domain.Entities;

namespace CraftsmenPlatform.Application.Common.Interfaces;

/// <summary>
/// Generátor JWT tokenů
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// Vygenerovat access token pro uživatele
    /// </summary>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Vygenerovat refresh token
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Validovat refresh token
    /// </summary>
    bool ValidateRefreshToken(string token);
}