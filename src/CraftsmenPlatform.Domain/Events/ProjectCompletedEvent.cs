using CraftsmenPlatform.Domain.Common;

namespace CraftsmenPlatform.Domain.Events;

public record ProjectCompletedEvent(
    Guid ProjectId,
    Guid CustomerId,
    Guid CraftsmanId) : DomainEventBase
{
}
