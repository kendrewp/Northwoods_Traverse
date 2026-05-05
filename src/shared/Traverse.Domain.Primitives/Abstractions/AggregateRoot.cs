using System.ComponentModel;

namespace Traverse.Domain.Primitives.Abstractions;

/// <summary>
/// Base class for all aggregate roots in the Traverse domain.
/// </summary>
/// <remarks>
/// <para>
/// An aggregate root is the only entry point through which external code may
/// mutate the entities in its consistency boundary. This base class adds
/// uniform support for collecting <see cref="DomainEvent"/> instances raised by
/// derived types so they can be dispatched after the unit-of-work commits
/// (see the persistence layer's post-commit dispatch pattern).
/// </para>
/// <para>
/// The events list is kept private and exposed only through a read-only view —
/// callers cannot mutate the collection externally; only derived aggregates
/// (via <see cref="AddDomainEvent"/>) and the persistence layer (via
/// <see cref="ClearDomainEvents"/>) participate in lifecycle management.
/// </para>
/// </remarks>
public abstract class AggregateRoot
{
    // Backing list is kept private and mutated only via AddDomainEvent so that
    // the encapsulation invariant ("events are append-only until dispatched")
    // is enforced by the type system rather than by convention.
    private readonly List<DomainEvent> _domainEvents = new();

    /// <summary>
    /// Read-only view of domain events raised by this aggregate since its last
    /// dispatch. Returned as <see cref="IReadOnlyList{T}"/> so callers cannot
    /// downcast to <see cref="List{T}"/> and mutate the collection.
    /// </summary>
    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Records a domain event for later dispatch. Visible only to derived
    /// aggregates so business invariants control when an event is raised.
    /// </summary>
    /// <param name="domainEvent">The event to enqueue. Must not be null.</param>
    protected void AddDomainEvent(DomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears the recorded domain events.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method is intended to be called by the persistence layer after the
    /// events have been dispatched in <c>SaveChangesAsync</c>. It is hidden
    /// from IntelliSense via <see cref="EditorBrowsableAttribute"/> to
    /// discourage application code from clearing events manually — doing so
    /// would silently drop side-effects that other parts of the system rely on.
    /// </para>
    /// <para>
    /// Public visibility (rather than <c>internal</c>) is required because the
    /// persistence project is in a separate assembly and cannot reach an
    /// <c>internal</c> member without <c>InternalsVisibleTo</c>, which would
    /// couple the domain to specific infrastructure assemblies.
    /// </para>
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void ClearDomainEvents() => _domainEvents.Clear();
}
