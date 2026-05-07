namespace Traverse.Domain.Primitives.Abstractions;

/// <summary>
/// Base type for every domain event in the Traverse platform.
/// </summary>
/// <remarks>
/// <para>
/// Modelled as an abstract <c>record</c> so concrete events get value equality,
/// non-destructive <c>with</c>-mutation, and concise declaration semantics for
/// free. The <c>abstract</c> modifier prevents direct instantiation — every
/// event must be a meaningfully named concrete type (e.g.,
/// <c>OrderCreatedEvent</c>) so that subscribers can pattern-match on the
/// runtime type.
/// </para>
/// <para>
/// <see cref="EventId"/> defaults to a fresh <see cref="Guid"/> per instance and
/// <see cref="OccurredOnUtc"/> captures the construction moment in UTC.
/// Subscribers must treat both as authoritative (do not re-stamp them) so the
/// causal record stays consistent end-to-end across processes.
/// </para>
/// </remarks>
public abstract record DomainEvent
{
    /// <summary>Unique identifier for this specific event occurrence.</summary>
    public Guid EventId { get; init; } = Guid.NewGuid();

    /// <summary>UTC timestamp captured at event construction time.</summary>
    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
}
