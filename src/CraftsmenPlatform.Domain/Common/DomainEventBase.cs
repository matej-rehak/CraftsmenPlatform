using CraftsmenPlatform.Domain.Common.Interface;

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