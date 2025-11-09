using MediatR;

namespace SmartScheduler.Domain.Events;

/// <summary>
/// Base interface for domain events.
/// Domain events represent something significant that happened in the domain.
/// Inherits from INotification to work with MediatR for event publishing.
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// Gets the event ID.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Gets the timestamp when the event occurred.
    /// </summary>
    DateTime OccurredAt { get; }
}

