using MediatR;

namespace CraftsmenPlatform.Domain.Common.Interfaces;

/// <summary>
/// Base interface pro všechny domain events.
/// Domain events reprezentují něco důležitého co se stalo v doméně.
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// Čas kdy event nastal
    /// </summary>
    DateTime OccurredOn { get; }
    Guid EventId { get; }
}
