using System.Text.RegularExpressions;
using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Exceptions;

namespace CraftsmenPlatform.Domain.ValueObjects;

/// <summary>
/// Value Object pro telefonní číslo
/// </summary>
public sealed class PhoneNumber : ValueObject
{
    private static readonly Regex PhoneRegex = new(
        @"^\+?[1-9]\d{1,14}$",
        RegexOptions.Compiled);

    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Vytvoří nové telefonní číslo
    /// </summary>
    public static Result<PhoneNumber> Create(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return Result<PhoneNumber>.Failure("Phone number cannot be empty");

        // Odstranění mezer a pomlček
        var cleanedPhone = phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

        if (!PhoneRegex.IsMatch(cleanedPhone))
            return Result<PhoneNumber>.Failure("Invalid phone number format. Use international format (e.g., +420123456789)");

        return Result<PhoneNumber>.Success(new PhoneNumber(cleanedPhone));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(PhoneNumber phone) => phone.Value;
}
