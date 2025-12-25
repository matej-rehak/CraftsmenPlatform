using CraftsmenPlatform.Domain.Common.Interfaces;

namespace CraftsmenPlatform.Infrastructure.Events;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
