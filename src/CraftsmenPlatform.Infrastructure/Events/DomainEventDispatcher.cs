using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using CraftsmenPlatform.Infrastructure.Events;

namespace CraftsmenPlatform.Infrastructure.Events;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IMediator _mediator;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(IMediator mediator, ILogger<DomainEventDispatcher> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var notification = new DomainEventNotification(domainEvent);
        _logger.LogInformation("Publishing domain event: {DomainEvent}", domainEvent.GetType().Name);
        await _mediator.Publish(notification, cancellationToken);
    }
}

public class DomainEventNotification : INotification
{
    public IDomainEvent DomainEvent { get; }

    public DomainEventNotification(IDomainEvent domainEvent)
    {
        DomainEvent = domainEvent;
    }
}


