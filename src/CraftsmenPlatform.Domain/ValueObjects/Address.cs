using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Exceptions;

namespace CraftsmenPlatform.Domain.ValueObjects;

/// <summary>
/// Value Object pro adresu
/// </summary>
public sealed class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string? State { get; }
    public string ZipCode { get; }
    public string Country { get; }

    private Address(string street, string city, string zipCode, string country, string? state = null)
    {
        Street = street;
        City = city;
        State = state;
        ZipCode = zipCode;
        Country = country;
    }

    /// <summary>
    /// Vytvoří novou adresu
    /// </summary>
    public static Address Create(string street, string city, string zipCode, string country, string? state = null)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new InvalidValueObjectException(nameof(Address), nameof(street), "Street cannot be empty");

        if (string.IsNullOrWhiteSpace(city))
            throw new InvalidValueObjectException(nameof(Address), nameof(city), "City cannot be empty");

        if (string.IsNullOrWhiteSpace(zipCode))
            throw new InvalidValueObjectException(nameof(Address), nameof(zipCode), "ZipCode cannot be empty");

        if (string.IsNullOrWhiteSpace(country))
            throw new InvalidValueObjectException(nameof(Address), nameof(country), "Country cannot be empty");

        return new Address(
            street.Trim(), 
            city.Trim(), 
            zipCode.Trim(), 
            country.Trim(), 
            state?.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return ZipCode;
        yield return Country;
    }

    public override string ToString()
    {
        var statePart = !string.IsNullOrEmpty(State) ? $", {State}" : "";
        return $"{Street}, {City}{statePart}, {ZipCode}, {Country}";
    }
}
