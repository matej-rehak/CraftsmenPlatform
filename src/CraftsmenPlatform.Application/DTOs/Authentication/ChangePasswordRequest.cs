namespace CraftsmenPlatform.Application.DTOs.Authentication;

public record ChangePasswordRequest
{
    public string CurrentPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}