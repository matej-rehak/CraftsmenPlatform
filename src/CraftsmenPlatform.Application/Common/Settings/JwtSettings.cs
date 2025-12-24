namespace CraftsmenPlatform.Application.Common.Settings;

/// <summary>
/// JWT konfigurace z appsettings.json
/// </summary>
public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    /// <summary>
    /// Secret key pro podpis JWT tokenů (min 32 znaků)
    /// </summary>
    public string Secret { get; init; } = string.Empty;

    /// <summary>
    /// Issuer (kdo token vydal)
    /// </summary>
    public string Issuer { get; init; } = string.Empty;

    /// <summary>
    /// Audience (pro koho je token určen)
    /// </summary>
    public string Audience { get; init; } = string.Empty;

    /// <summary>
    /// Životnost access tokenu v minutách (default: 15 minut)
    /// </summary>
    public int AccessTokenExpirationMinutes { get; init; } = 15;

    /// <summary>
    /// Životnost refresh tokenu ve dnech (default: 7 dní)
    /// </summary>
    public int RefreshTokenExpirationDays { get; init; } = 7;
}