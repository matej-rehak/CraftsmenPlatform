using MediatR;
using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Common.Interface;

namespace CraftsmenPlatform.Infrastructure.Events;

public class MediatRDomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IMediator _mediator;

    public MediatRDomainEventDispatcher(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await _mediator.Publish(domainEvent, cancellationToken);
    }
}