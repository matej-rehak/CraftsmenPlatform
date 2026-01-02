using CraftsmenPlatform.Domain.Enums;
using CraftsmenPlatform.Domain.Common;

namespace CraftsmenPlatform.Domain.ValueObjects;

public sealed class Password : ValueObject
{
    public string Value { get; }

    // Minimal password length
    private const int MinLength = 8;
    // Maximal password length
    private const int MaxLength = 128;

    private Password(string value)
    {
        Value = value;
    }

    public static Result<Password> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<Password>.Failure("Password cannot be empty");

        if (value.Length < MinLength)
            return Result<Password>.Failure("Password must be at least 8 characters long");

        if (value.Length > MaxLength)
            return Result<Password>.Failure("Password must be at most 128 characters long");

        if (!HasUppercase(value))
            return Result<Password>.Failure("Password must contain at least one uppercase letter");

        if (!HasLowercase(value))
            return Result<Password>.Failure("Password must contain at least one lowercase letter");

        if (!HasDigit(value))
            return Result<Password>.Failure("Password must contain at least one digit");

        if (!HasSpecialCharacter(value))
            return Result<Password>.Failure("Password must contain at least one special character");

        return Result<Password>.Success(new Password(value));
    }

    // Method to calculate password strength
    public int CalculateStrength()
    {
            int strength = 0;

            // Délka (max 40 bodů)
            strength += Math.Min(Value.Length * 2, 40);

            // Různorodost znaků (max 40 bodů)
            if (HasUppercase(Value)) strength += 10;
            if (HasLowercase(Value)) strength += 10;
            if (HasDigit(Value)) strength += 10;
            if (HasSpecialCharacter(Value)) strength += 10;

            // Počet unikátních znaků (max 20 bodů)
            int uniqueChars = Value.Distinct().Count();
            strength += Math.Min(uniqueChars * 2, 20);

            return Math.Min(strength, 100);
    }

    public PasswordStrengthLevel GetStrengthLevel()
    {
        int strength = CalculateStrength();
        
        return strength switch
        {
                >= 80 => PasswordStrengthLevel.VeryStrong,
                >= 60 => PasswordStrengthLevel.Strong,
                >= 40 => PasswordStrengthLevel.Medium,
                >= 20 => PasswordStrengthLevel.Weak,
                _ => PasswordStrengthLevel.VeryWeak
            };
    }
    
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    private static bool HasUppercase(string value)
    {
        return value.Any(char.IsUpper);
    }

    private static bool HasLowercase(string value)
    {
        return value.Any(char.IsLower);
    }

    private static bool HasDigit(string value)
    {
        return value.Any(char.IsDigit);
    }

    private static bool HasSpecialCharacter(string value)
    {
        return value.Any(ch => !char.IsLetterOrDigit(ch));
    }
}