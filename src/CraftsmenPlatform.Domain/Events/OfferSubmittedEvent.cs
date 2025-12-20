using CraftsmenPlatform.Domain.Common.Interface;

namespace CraftsmenPlatform.Domain.Events;

public record OfferSubmittedEvent(
    Guid OfferId,
    Guid ProjectId,
    Guid CraftsmanId,
    decimal Price) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
