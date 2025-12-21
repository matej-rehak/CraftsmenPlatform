using CraftsmenPlatform.Domain.Common;

namespace CraftsmenPlatform.Domain.Events;

public record ReviewPublishedEvent(
    Guid ReviewId,
    Guid CraftsmanId,
    Guid ProjectId,
    int Rating) : DomainEventBase
{
}
