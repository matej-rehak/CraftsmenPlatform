namespace CraftsmenPlatform.Domain.Exceptions;

/// <summary>
/// Exception vyhazovaná při porušení business pravidel
/// </summary>
public class BusinessRuleValidationException : DomainException
{
    public string RuleName { get; }

    public BusinessRuleValidationException(string ruleName, string message) 
        : base(message)
    {
        RuleName = ruleName;
    }
}
