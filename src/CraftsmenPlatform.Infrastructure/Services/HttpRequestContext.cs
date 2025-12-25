using CraftsmenPlatform.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace CraftsmenPlatform.Infrastructure.Services;

/// <summary>
/// Implementace IRequestContext pro HTTP požadavky
/// </summary>
public class HttpRequestContext : IRequestContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpRequestContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetIpAddress()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        
        if (httpContext == null)
            return "unknown";

        // Kontrola za proxy (X-Forwarded-For header)
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // Vezmi první IP adresu (klient)
            return forwardedFor.Split(',')[0].Trim();
        }

        // Kontrola X-Real-IP header (některé proxy)
        var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Fallback na RemoteIpAddress
        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    public string? GetUserAgent()
    {
        return _httpContextAccessor.HttpContext?
            .Request.Headers["User-Agent"].FirstOrDefault();
    }
}