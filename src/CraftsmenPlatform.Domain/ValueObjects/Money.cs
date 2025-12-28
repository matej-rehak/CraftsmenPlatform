using CraftsmenPlatform.Domain.Common;

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
    public static Result<Money> Create(decimal amount, string currency = "CZK")
    {
        if (amount < 0)
            return Result<Money>.Failure("Amount cannot be negative");

        if (string.IsNullOrWhiteSpace(currency))
            return Result<Money>.Failure("Currency cannot be empty");

        if (currency.Length != 3)
            return Result<Money>.Failure("Currency code must be 3 characters (ISO 4217)");

        return Result<Money>.Success(new Money(amount, currency.ToUpperInvariant()));
    }

    public static Money Zero(string currency = "CZK") => new Money(0, currency.ToUpperInvariant());

    // ========== Aritmetické operace (vrací Result) ==========
    
    public Result<Money> Add(Money? other)
    {
        if (other == null)
            return Result<Money>.Failure("Cannot add null money");

        if (Currency != other.Currency)
            return Result<Money>.Failure($"Cannot add money with different currencies: {Currency} and {other.Currency}");

        return Result<Money>.Success(new Money(Amount + other.Amount, Currency));
    }

    public Result<Money> Subtract(Money? other)
    {
        if (other == null)
            return Result<Money>.Failure("Cannot subtract null money");

        if (Currency != other.Currency)
            return Result<Money>.Failure($"Cannot subtract money with different currencies: {Currency} and {other.Currency}");

        var newAmount = Amount - other.Amount;
        if (newAmount < 0)
            return Result<Money>.Failure("Subtraction would result in negative amount");

        return Result<Money>.Success(new Money(newAmount, Currency));
    }

    public Result<Money> Multiply(decimal factor)
    {
        if (factor < 0)
            return Result<Money>.Failure("Cannot multiply by negative factor");

        return Result<Money>.Success(new Money(Amount * factor, Currency));
    }

    public Result<Money> Divide(decimal divisor)
    {
        if (divisor == 0)
            return Result<Money>.Failure("Cannot divide by zero");

        if (divisor < 0)
            return Result<Money>.Failure("Cannot divide by negative number");

        return Result<Money>.Success(new Money(Amount / divisor, Currency));
    }

    // ========== Porovnání (vrací bool, hází při nekompatibilitě) ==========
    
    public bool IsGreaterThan(Money? other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        EnsureSameCurrency(other);
        return Amount > other.Amount;
    }

    public bool IsGreaterThanOrEqual(Money? other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        EnsureSameCurrency(other);
        return Amount >= other.Amount;
    }

    public bool IsLessThan(Money? other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        EnsureSameCurrency(other);
        return Amount < other.Amount;
    }

    public bool IsLessThanOrEqual(Money? other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        EnsureSameCurrency(other);
        return Amount <= other.Amount;
    }

    public bool IsZero() => Amount == 0;
    public bool IsPositive() => Amount > 0;

    // ========== Pomocná metoda pro kontrolu měny ==========
    
    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException(
                $"Cannot compare money with different currencies: {Currency} and {other.Currency}");
    }

    // ========== Operátory (NE Result - musí být deterministické) ==========
    // Poznámka: Operátory NEMOHOU vracet Result, proto házejí výjimky při chybách
    
    public static Money operator +(Money left, Money right)
    {
        if (left == null) throw new ArgumentNullException(nameof(left));
        if (right == null) throw new ArgumentNullException(nameof(right));
        
        var result = left.Add(right);
        if (result.IsFailure)
            throw new InvalidOperationException(result.Error);
        
        return result.Value;
    }

    public static Money operator -(Money left, Money right)
    {
        if (left == null) throw new ArgumentNullException(nameof(left));
        if (right == null) throw new ArgumentNullException(nameof(right));
        
        var result = left.Subtract(right);
        if (result.IsFailure)
            throw new InvalidOperationException(result.Error);
        
        return result.Value;
    }

    public static Money operator *(Money money, decimal factor)
    {
        if (money == null) throw new ArgumentNullException(nameof(money));
        
        var result = money.Multiply(factor);
        if (result.IsFailure)
            throw new InvalidOperationException(result.Error);
        
        return result.Value;
    }

    public static Money operator /(Money money, decimal divisor)
    {
        if (money == null) throw new ArgumentNullException(nameof(money));
        
        var result = money.Divide(divisor);
        if (result.IsFailure)
            throw new InvalidOperationException(result.Error);
        
        return result.Value;
    }

    // ========== Comparison operátory ==========
    
    public static bool operator >(Money left, Money right)
    {
        if (left == null) throw new ArgumentNullException(nameof(left));
        if (right == null) throw new ArgumentNullException(nameof(right));
        
        return left.IsGreaterThan(right);
    }

    public static bool operator <(Money left, Money right)
    {
        if (left == null) throw new ArgumentNullException(nameof(left));
        if (right == null) throw new ArgumentNullException(nameof(right));
        
        return left.IsLessThan(right);
    }

    public static bool operator >=(Money left, Money right)
    {
        if (left == null) throw new ArgumentNullException(nameof(left));
        if (right == null) throw new ArgumentNullException(nameof(right));
        
        return left.IsGreaterThanOrEqual(right);
    }

    public static bool operator <=(Money left, Money right)
    {
        if (left == null) throw new ArgumentNullException(nameof(left));
        if (right == null) throw new ArgumentNullException(nameof(right));
        
        return left.IsLessThanOrEqual(right);
    }

    // ========== ValueObject implementation ==========
    
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Math.Round(Amount, 2); // Zaokrouhlení pro konzistentní porovnání
        yield return Currency;
    }

    public override string ToString() => $"{Amount:N2} {Currency}";
}
