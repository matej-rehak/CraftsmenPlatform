using CraftsmenPlatform.Domain.Common.Interface;

namespace CraftsmenPlatform.Domain.Events;

public record UserRegisteredEvent(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
