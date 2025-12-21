using CraftsmenPlatform.Domain.Common;

namespace CraftsmenPlatform.Domain.Events;

public record OfferAcceptedEvent(
    Guid OfferId,
    Guid ProjectId,
    Guid CraftsmanId) : DomainEventBase
{
}
