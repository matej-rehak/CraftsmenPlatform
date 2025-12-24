namespace CraftsmenPlatform.Application.DTOs.Authentication;

public record RefreshTokenRequest
{
    public string RefreshToken { get; init; } = string.Empty;
}