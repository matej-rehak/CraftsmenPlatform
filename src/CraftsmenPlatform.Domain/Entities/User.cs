using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Enums;

namespace CraftsmenPlatform.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; private set } = string.Empty;

    public string PasswordHash { get; private set } = string.Empty;

    public string FirstName { get; private set } = string.Empty;

    public string LastName { get; private set } = string.Empty;

    public string? Phone { get; private set } = string.Empty;

    public string? Address { get; private set } = string.Empty;

    public string? City { get; private set } = string.Empty;

    public string? State { get; private set } = string.Empty;

    public string? ZipCode { get; private set } = string.Empty;

    public string? Country { get; private set } = string.Empty;

    public string? AvatarUrl { get; private set } = string.Empty;

    public UserRole Role { get; private set } = UserRole.User;
    public bool IsEmailVerified { get; private set } = false;
    public DateTime? EmailVerifiedAt { get; private set } = null;
    public DateTime? LastLoginAt { get; private set } = null;
    public bool IsActive { get; private set } = true;
    public DateTime? DeactivatedAt { get; private set } = null;
    public string? DeactivatedReason { get; private set } = string.Empty;

}