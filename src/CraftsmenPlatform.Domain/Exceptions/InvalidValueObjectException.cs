namespace CraftsmenPlatform.Domain.Exceptions;

/// <summary>
/// Exception vyhazovaná při vytváření nevalidních Value Objects
/// </summary>
public class InvalidValueObjectException : DomainException
{
    public string ValueObjectType { get; }
    public string PropertyName { get; }

    public InvalidValueObjectException(string valueObjectType, string propertyName, string message) 
        : base($"{valueObjectType}.{propertyName}: {message}")
    {
        ValueObjectType = valueObjectType;
        PropertyName = propertyName;
    }
}
