using MediatR;
using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Common.Interface;

namespace CraftsmenPlatform.Infrastructure.Events;

// ==================== IDomainEventDispatcher ====================
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
