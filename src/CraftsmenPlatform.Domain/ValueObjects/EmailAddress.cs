using System.Text.RegularExpressions;
using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Exceptions;

namespace CraftsmenPlatform.Domain.ValueObjects;

/// <summary>
/// Value Object pro emailovou adresu
/// </summary>
public sealed class EmailAddress : ValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private EmailAddress(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Vytvoří novou emailovou adresu
    /// </summary>
    public static Result<EmailAddress> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result<EmailAddress>.Failure(
                "Email cannot be empty");

        email = email.Trim().ToLowerInvariant();

        if (email.Length > 255)
            return Result<EmailAddress>.Failure(
                "Email cannot be longer than 255 characters");

        if (!EmailRegex.IsMatch(email))
            return Result<EmailAddress>.Failure(
                "Invalid email format");

        return Result<EmailAddress>.Success(new EmailAddress(email));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(EmailAddress email) => email.Value;
}
