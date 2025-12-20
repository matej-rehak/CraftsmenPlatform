using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Exceptions;

namespace CraftsmenPlatform.Domain.ValueObjects;

/// <summary>
/// Value Object pro peníze s měnou
/// </summary>
public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    /// <summary>
    /// Vytvoří nový objekt Money
    /// </summary>
    public static Money Create(decimal amount, string currency = "CZK")
    {
        if (amount < 0)
            throw new InvalidValueObjectException(
                nameof(Money), 
                nameof(amount), 
                "Amount cannot be negative");

        if (string.IsNullOrWhiteSpace(currency))
            throw new InvalidValueObjectException(
                nameof(Money), 
                nameof(currency), 
                "Currency cannot be empty");

        return new Money(amount, currency.ToUpperInvariant());
    }

    public static Money Zero(string currency = "CZK") => new Money(0, currency);

    // Operace s penězi
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException(
                $"Cannot add money with different currencies: {Currency} and {other.Currency}");

        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException(
                $"Cannot subtract money with different currencies: {Currency} and {other.Currency}");

        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal factor)
    {
        return new Money(Amount * factor, Currency);
    }

    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);
    public static Money operator *(Money money, decimal factor) => money.Multiply(factor);

    public bool IsGreaterThan(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException(
                $"Cannot compare money with different currencies: {Currency} and {other.Currency}");

        return Amount > other.Amount;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:N2} {Currency}";
}
