using CraftsmenPlatform.Domain.Common;

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
    public static Result<Rating> Create(int value)
    {
        if (value < MinValue || value > MaxValue)
            return Result<Rating>.Failure(
                "Rating must be between " + MinValue + " and " + MaxValue);

        return Result<Rating>.Success(new Rating(value));
    }

    /// <summary>
    /// Výpočet průměrného ratingu z kolekce
    /// </summary>
    public static Result<decimal> CalculateAverage(IEnumerable<Rating> ratings)
    {
        var ratingList = ratings.ToList();
        if (!ratingList.Any())
            return Result<decimal>.Failure("No ratings provided");

        return Result<decimal>.Success((decimal)ratingList.Average(r => r.Value));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => $"{Value}/10";

    public static implicit operator int(Rating rating) => rating.Value;
}
