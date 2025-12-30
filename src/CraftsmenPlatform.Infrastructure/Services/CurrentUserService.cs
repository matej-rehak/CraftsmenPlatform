using CraftsmenPlatform.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace CraftsmenPlatform.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string UserId => _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";
}