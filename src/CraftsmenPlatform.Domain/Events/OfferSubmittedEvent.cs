using CraftsmenPlatform.Domain.Common;

namespace CraftsmenPlatform.Domain.Events;

public record OfferSubmittedEvent(
    Guid OfferId,
    Guid ProjectId,
    Guid CraftsmanId,
    decimal Price) : DomainEventBase
{
}
