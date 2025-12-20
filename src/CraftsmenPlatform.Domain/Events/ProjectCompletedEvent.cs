using CraftsmenPlatform.Domain.Common.Interface;

namespace CraftsmenPlatform.Domain.Events;

public record ProjectCompletedEvent(
    Guid ProjectId,
    Guid CustomerId,
    Guid CraftsmanId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
