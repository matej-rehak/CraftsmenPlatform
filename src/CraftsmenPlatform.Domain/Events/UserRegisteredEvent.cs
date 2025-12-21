using CraftsmenPlatform.Domain.Common;

namespace CraftsmenPlatform.Domain.Events;

public record UserRegisteredEvent(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName) : DomainEventBase
{
}
