using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Exceptions;

namespace CraftsmenPlatform.Domain.ValueObjects;

/// <summary>
/// Value Object pro hodnocení (1-10)
/// </summary>
public sealed class Rating : ValueObject
{
    public const int MinValue = 1;
    public const int MaxValue = 10;

    public int Value { get; }

    private Rating(int value)
    {
        Value = value;
    }

    /// <summary>
    /// Vytvoří nové hodnocení
    /// </summary>
    public static Rating Create(int value)
    {
        if (value < MinValue || value > MaxValue)
            throw new InvalidValueObjectException(
                nameof(Rating), 
                nameof(value), 
                $"Rating must be between {MinValue} and {MaxValue}");

        return new Rating(value);
    }

    /// <summary>
    /// Výpočet průměrného ratingu z kolekce
    /// </summary>
    public static decimal CalculateAverage(IEnumerable<Rating> ratings)
    {
        var ratingList = ratings.ToList();
        if (!ratingList.Any())
            return 0;

        return (decimal)ratingList.Average(r => r.Value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => $"{Value}/10";

    public static implicit operator int(Rating rating) => rating.Value;
}
