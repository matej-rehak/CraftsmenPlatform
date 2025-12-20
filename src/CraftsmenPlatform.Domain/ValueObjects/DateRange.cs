using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Exceptions;

namespace CraftsmenPlatform.Domain.ValueObjects;

/// <summary>
/// Value Object pro časové rozpětí
/// </summary>
public sealed class DateRange : ValueObject
{
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }

    private DateRange(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }

    /// <summary>
    /// Vytvoří nové časové rozpětí
    /// </summary>
    public static DateRange Create(DateTime startDate, DateTime endDate)
    {
        if (startDate > endDate)
            throw new InvalidValueObjectException(
                nameof(DateRange), 
                nameof(startDate), 
                "Start date cannot be after end date");

        return new DateRange(startDate, endDate);
    }

    /// <summary>
    /// Počet dní v rozpětí
    /// </summary>
    public int DurationInDays => (EndDate - StartDate).Days;

    /// <summary>
    /// Kontrola zda datum spadá do rozpětí
    /// </summary>
    public bool Contains(DateTime date)
    {
        return date >= StartDate && date <= EndDate;
    }

    /// <summary>
    /// Kontrola zda se rozpětí překrývá s jiným rozpětím
    /// </summary>
    public bool OverlapsWith(DateRange other)
    {
        return StartDate <= other.EndDate && EndDate >= other.StartDate;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return StartDate;
        yield return EndDate;
    }

    public override string ToString() 
        => $"{StartDate:yyyy-MM-dd} - {EndDate:yyyy-MM-dd} ({DurationInDays} days)";
}
