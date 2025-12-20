using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Common.Interface;
using CraftsmenPlatform.Domain.Enums;
using CraftsmenPlatform.Domain.Events;
using CraftsmenPlatform.Domain.Exceptions;
using CraftsmenPlatform.Domain.ValueObjects;

namespace CraftsmenPlatform.Domain.Entities;

/// <summary>
/// Agregát User - reprezentuje uživatele v systému
/// </summary>
public class User : BaseEntity, IAggregateRoot
{
    public EmailAddress Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public PhoneNumber? Phone { get; private set; }
    public Address? Address { get; private set; }
    public string? AvatarUrl { get; private set; }

    public UserRole Role { get; private set; } = UserRole.User;
    public bool IsEmailVerified { get; private set; } = false;
    public DateTime? EmailVerifiedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime? DeactivatedAt { get; private set; }
    public string? DeactivatedReason { get; private set; }

    public string FullName => $"{FirstName} {LastName}";

    // Private constructor pro EF Core
    private User() { }

    private User(
        Guid id,
        EmailAddress email,
        string passwordHash,
        string firstName,
        string lastName,
        UserRole role)
    {
        Id = id;
        Email = email ?? throw new ArgumentNullException(nameof(email));
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        FirstName = firstName?.Trim() ?? throw new ArgumentNullException(nameof(firstName));
        LastName = lastName?.Trim() ?? throw new ArgumentNullException(nameof(lastName));
        Role = role;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;

        if (string.IsNullOrWhiteSpace(firstName))
            throw new BusinessRuleValidationException(nameof(FirstName), "First name cannot be empty");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new BusinessRuleValidationException(nameof(LastName), "Last name cannot be empty");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new BusinessRuleValidationException(nameof(PasswordHash), "Password hash cannot be empty");
    }

    /// <summary>
    /// Factory metoda pro vytvoření běžného uživatele
    /// </summary>
    public static User CreateUser(
        string email,
        string passwordHash,
        string firstName,
        string lastName)
    {
        var emailAddress = EmailAddress.Create(email);
        var user = new User(Guid.NewGuid(), emailAddress, passwordHash, firstName, lastName, UserRole.User);
        
        user.AddDomainEvent(new UserRegisteredEvent(
            user.Id,
            emailAddress,
            firstName,
            lastName));

        return user;
    }

    /// <summary>
    /// Factory metoda pro vytvoření řemeslníka
    /// </summary>
    public static User CreateCraftsman(
        string email,
        string passwordHash,
        string firstName,
        string lastName)
    {
        var emailAddress = EmailAddress.Create(email);
        var user = new User(Guid.NewGuid(), emailAddress, passwordHash, firstName, lastName, UserRole.Craftsman);
        
        user.AddDomainEvent(new UserRegisteredEvent(
            user.Id,
            emailAddress,
            firstName,
            lastName));

        return user;
    }

    /// <summary>
    /// Factory metoda pro vytvoření zákazníka
    /// </summary>
    public static User CreateCustomer(
        string email,
        string passwordHash,
        string firstName,
        string lastName)
    {
        var emailAddress = EmailAddress.Create(email);
        var user = new User(Guid.NewGuid(), emailAddress, passwordHash, firstName, lastName, UserRole.User);
        
        user.AddDomainEvent(new UserRegisteredEvent(
            user.Id,
            emailAddress,
            firstName,
            lastName));

        return user;
    }

    /// <summary>
    /// Verifikace emailu
    /// </summary>
    public Result VerifyEmail()
    {
        if (IsEmailVerified)
            return Result.Failure("Email is already verified");

        IsEmailVerified = true;
        EmailVerifiedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Aktualizace profilu
    /// </summary>
    public void UpdateProfile(
        string? firstName = null,
        string? lastName = null,
        string? phoneNumber = null,
        Address? address = null,
        string? avatarUrl = null)
    {
        if (!string.IsNullOrWhiteSpace(firstName))
        {
            FirstName = firstName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(lastName))
        {
            LastName = lastName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            Phone = PhoneNumber.Create(phoneNumber);
        }

        if (address != null)
        {
            Address = address;
        }

        if (!string.IsNullOrWhiteSpace(avatarUrl))
        {
            AvatarUrl = avatarUrl;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Změna hesla
    /// </summary>
    public Result ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            return Result.Failure("Password hash cannot be empty");

        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Deaktivace účtu
    /// </summary>
    public Result Deactivate(string reason)
    {
        if (!IsActive)
            return Result.Failure("User is already deactivated");

        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure("Deactivation reason cannot be empty");

        IsActive = false;
        DeactivatedAt = DateTime.UtcNow;
        DeactivatedReason = reason;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Aktivace účtu
    /// </summary>
    public Result Activate()
    {
        if (IsActive)
            return Result.Failure("User is already active");

        IsActive = true;
        DeactivatedAt = null;
        DeactivatedReason = null;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Zaznamenání přihlášení
    /// </summary>
    public Result RecordLogin()
    {
        if (!IsActive)
            return Result.Failure("Cannot login with deactivated account");

        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Změna role
    /// </summary>
    public void ChangeRole(UserRole newRole)
    {
        if (Role == newRole)
            return;

        Role = newRole;
        UpdatedAt = DateTime.UtcNow;
    }
}