using CraftsmenPlatform.Domain.Common;

namespace CraftsmenPlatform.Domain.ValueObjects;

public sealed class BudgetRange : ValueObject
{
    public Money Min { get; }
    public Money Max { get; }

    private BudgetRange(Money min, Money max)
    {
        Min = min;
        Max = max;
    }

    public static Result<BudgetRange> Create(Money min, Money max)
    {
        if (min.IsGreaterThan(max))
            return Result<BudgetRange>.Failure("Min cannot exceed max");

        return Result<BudgetRange>.Success(new BudgetRange(min, max));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Min;
        yield return Max;
    }

    public override string ToString() => $"{Min} – {Max}";
}