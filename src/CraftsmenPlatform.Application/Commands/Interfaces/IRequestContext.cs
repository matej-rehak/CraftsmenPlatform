namespace CraftsmenPlatform.Application.Common.Interfaces;

/// <summary>
/// Abstrakce pro získání informací o aktuálním HTTP requestu
/// </summary>
public interface IRequestContext
{
    /// <summary>
    /// Získá IP adresu klienta
    /// </summary>
    string GetIpAddress();
    
    /// <summary>
    /// Získá User Agent
    /// </summary>
    string? GetUserAgent();
}