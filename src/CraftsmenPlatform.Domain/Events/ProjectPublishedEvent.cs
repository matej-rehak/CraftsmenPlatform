using CraftsmenPlatform.Domain.Common;

namespace CraftsmenPlatform.Domain.Events;

public record ProjectPublishedEvent(
    Guid ProjectId,
    Guid CustomerId,
    string Title) : DomainEventBase
{
}
