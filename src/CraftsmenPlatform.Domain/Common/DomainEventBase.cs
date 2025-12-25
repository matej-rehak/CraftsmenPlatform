using CraftsmenPlatform.Domain.Common.Interfaces;

namespace CraftsmenPlatform.Domain.Common;

public abstract record DomainEventBase : IDomainEvent
{
    public DateTime OccurredOn { get; }
    public Guid EventId { get; }

    protected DomainEventBase()
    {
        OccurredOn = DateTime.UtcNow;
        EventId = Guid.NewGuid();
    }
}