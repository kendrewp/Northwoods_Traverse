namespace Traverse.Infrastructure.Messaging.Abstractions;

/// <summary>
/// Abstraction over the platform's event bus (MassTransit/RabbitMQ).
/// </summary>
/// <remarks>
/// <para>
/// Application layer code depends on this interface rather than on
/// <see cref="IPublishEndpoint"/> directly, which keeps MassTransit as an
/// implementation detail (DIP). In practice, the registered implementation
/// delegates to <see cref="IPublishEndpoint.Publish{T}"/> — no adaption logic
/// is needed.
/// </para>
/// <para>
/// For in-process domain events (same-request side-effects) prefer MediatR
/// <c>IPublisher</c>. Use <see cref="IEventBus"/> for events that cross service
/// boundaries and must transit RabbitMQ.
/// </para>
/// </remarks>
public interface IEventBus
{
    /// <summary>
    /// Publishes <paramref name="event"/> to the message broker for delivery to
    /// all registered consumers.
    /// </summary>
    /// <typeparam name="TEvent">Event type. Must be a reference type so MassTransit
    /// can serialize it via its configured serializer.</typeparam>
    /// <param name="event">The event payload to publish.</param>
    /// <param name="ct">Token to cancel the publish operation.</param>
    /// <returns>A <see cref="Task"/> that completes when the broker has accepted
    /// the message (not when consumers have processed it).</returns>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct) where TEvent : class;
}
