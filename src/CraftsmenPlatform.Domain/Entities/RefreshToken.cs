using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Exceptions;

namespace CraftsmenPlatform.Domain.Entities;

/// <summary>
/// Refresh token pro JWT authentication
/// Umožňuje získat nový access token bez nutnosti znovu se přihlašovat
/// </summary>
public class RefreshToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? RevokedByIp { get; private set; }
    public string? ReplacedByToken { get; private set; }
    public string CreatedByIp { get; private set; } = string.Empty;

    // Navigation
    public User User { get; private set; } = null!;

    // Computed properties
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsActive => !IsRevoked && !IsExpired;

    // EF Core constructor
    private RefreshToken() { }

    // Business constructor
    private RefreshToken(
        Guid userId,
        string token,
        DateTime expiresAt,
        string createdByIp)
    {
        if (userId == Guid.Empty)
            throw new BusinessRuleValidationException(
                nameof(UserId),
                "UserId cannot be empty");

        if (string.IsNullOrWhiteSpace(token))
            throw new BusinessRuleValidationException(
                nameof(Token),
                "Token cannot be empty");

        if (expiresAt <= DateTime.UtcNow)
            throw new BusinessRuleValidationException(
                nameof(ExpiresAt),
                "ExpiresAt must be in the future");

        if (string.IsNullOrWhiteSpace(createdByIp))
            throw new BusinessRuleValidationException(
                nameof(CreatedByIp),
                "CreatedByIp cannot be empty");

        Id = Guid.NewGuid();
        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
        CreatedByIp = createdByIp;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Factory method pro vytvoření nového refresh tokenu
    /// </summary>
    public static RefreshToken Create(
        Guid userId,
        string token,
        DateTime expiresAt,
        string createdByIp)
    {
        return new RefreshToken(userId, token, expiresAt, createdByIp);
    }

    /// <summary>
    /// Revoke token - zneplatnění tokenu
    /// </summary>
    public Result Revoke(string revokedByIp, string? replacedByToken = null)
    {
        if (IsRevoked)
            return Result.Failure("Token is already revoked");

        if (IsExpired)
            return Result.Failure("Token is already expired");

        if (string.IsNullOrWhiteSpace(revokedByIp))
            return Result.Failure("RevokedByIp cannot be empty");

        RevokedAt = DateTime.UtcNow;
        RevokedByIp = revokedByIp;
        ReplacedByToken = replacedByToken;

        return Result.Success();
    }
}