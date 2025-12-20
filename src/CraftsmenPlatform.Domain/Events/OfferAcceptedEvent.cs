using CraftsmenPlatform.Domain.Common.Interface;

namespace CraftsmenPlatform.Domain.Events;

public record OfferAcceptedEvent(
    Guid OfferId,
    Guid ProjectId,
    Guid CraftsmanId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
