using CraftsmenPlatform.Domain.Common.Interface;

namespace CraftsmenPlatform.Domain.Events;

public record ReviewPublishedEvent(
    Guid ReviewId,
    Guid CraftsmanId,
    Guid ProjectId,
    int Rating) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
