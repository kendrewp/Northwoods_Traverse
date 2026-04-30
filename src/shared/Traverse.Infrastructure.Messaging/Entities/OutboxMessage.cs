namespace Traverse.Infrastructure.Messaging.Entities;

/// <summary>
/// Persistent record of a domain/integration event that must be forwarded to
/// the message broker at least once.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Outbox pattern:</strong> producers write an <see cref="OutboxMessage"/>
/// in the same DB transaction as business data. A background processor
/// (<see cref="Processing.OutboxProcessorBase"/>) queries unpublished rows,
/// publishes each one to MassTransit, and stamps
/// <see cref="PublishedOnUtc"/>. This guarantees at-least-once delivery to the
/// broker without a distributed transaction.
/// </para>
/// <para>
/// <strong>EF Core mapping:</strong> each service includes
/// <c>DbSet&lt;OutboxMessage&gt;</c> in its concrete <c>DbContext</c> and
/// provides a corresponding <c>IEntityTypeConfiguration&lt;OutboxMessage&gt;</c>
/// to enforce max-length constraints at the database level (Phase 1). Max lengths
/// are documented below as comments — no Data Annotation attributes are used
/// because attribute-based config couples the entity to a specific validation
/// framework, while EF Fluent API is used uniformly in this codebase.
/// </para>
/// <para>
/// <strong>Why <c>DateTime</c> and not <c>DateTimeOffset</c>:</strong>
/// MassTransit serializes and compares scheduled-message timestamps using
/// <c>DateTime.UtcNow</c> internally. Using <c>DateTimeOffset</c> here would
/// introduce offset-awareness that MassTransit's SQL polling transport does not
/// expect, causing subtle ordering bugs on systems with non-UTC clocks.
/// Per design §4.2 — MassTransit convention.
/// </para>
/// </remarks>
public class OutboxMessage
{
    /// <summary>Primary key — auto-generated per row.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Assembly-qualified type name of the event payload (max 512 characters).
    /// Used by the processor to deserialise <see cref="Content"/> back to the
    /// correct CLR type before publishing.
    /// </summary>
    /// <remarks>
    /// Max length: 512 — enforced by EF Core Fluent API in the service project.
    /// </remarks>
    public required string MessageType { get; init; }

    /// <summary>
    /// JSON-serialised event payload. The processor deserialises this before
    /// passing the object to MassTransit's <c>IPublishEndpoint</c>.
    /// </summary>
    /// <remarks>
    /// Max length: unlimited (TEXT / nvarchar(max)) — a message body has no
    /// practical upper bound; size constraints are enforced by RabbitMQ frame
    /// limits, not the DB column.
    /// </remarks>
    public required string Content { get; init; }

    /// <summary>
    /// UTC wall-clock instant at which this row was inserted.
    /// Used to order unpublished rows (oldest first) so events arrive at the
    /// broker in the order they were persisted.
    /// </summary>
    // DateTime (not DateTimeOffset) — see class-level remarks.
    public DateTime CreatedOnUtc { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// UTC instant at which the processor successfully handed this message to
    /// MassTransit. <c>null</c> while the row is pending delivery.
    /// </summary>
    // DateTime (not DateTimeOffset) — see class-level remarks.
    public DateTime? PublishedOnUtc { get; set; }
}
