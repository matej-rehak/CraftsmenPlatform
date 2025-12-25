using CraftsmenPlatform.Domain.Common;

namespace CraftsmenPlatform.Domain.Common.Interface;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
