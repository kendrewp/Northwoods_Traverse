namespace Traverse.Domain.Primitives.Abstractions;

/// <summary>
/// Base class for entities — domain objects whose identity is defined by their
/// <typeparamref name="TId"/> rather than the equality of their attribute set.
/// </summary>
/// <typeparam name="TId">
/// Strongly-typed identifier (typically a <c>readonly record struct</c> wrapping
/// a <see cref="Guid"/>). Constraining to <c>notnull</c> prevents accidental use
/// of nullable identity types, which would invalidate the equality contract.
/// </typeparam>
/// <remarks>
/// <para>
/// Equality is defined exclusively by <see cref="Id"/>. Two entities of the
/// same concrete type with identical IDs are considered equal even if other
/// fields differ — this matches DDD entity semantics (the persistent
/// identity is what makes the entity the "same" thing across time).
/// </para>
/// <para>
/// <see cref="Id"/> is <c>protected init</c> so derived constructors and
/// factory methods set the ID exactly once at construction; afterwards the
/// identity is immutable, which is what entity equality assumes.
/// </para>
/// </remarks>
public abstract class Entity<TId>
    where TId : notnull
{
    /// <summary>Identity of this entity. Set once at construction, then immutable.</summary>
    public TId Id { get; protected init; } = default!;

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        // Reference-identity short-circuit (cheapest path).
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        // Concrete-type guard: an Order with id X is not equal to a Customer
        // with id X even if both wrap the same Guid value. We compare on
        // GetType() rather than `is Entity<TId>` to support inheritance
        // hierarchies safely.
        if (obj is not Entity<TId> other || other.GetType() != GetType())
        {
            return false;
        }

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    /// <inheritdoc />
    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    /// Typed equality operator — preferred over the generic
    /// <see cref="object"/> overload because it lets the compiler short-circuit
    /// type checks at the call site.
    /// </summary>
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        if (left is null)
        {
            return right is null;
        }

        return left.Equals(right);
    }

    /// <summary>Negation of <see cref="op_Equality(Entity{TId}, Entity{TId})"/>.</summary>
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !(left == right);
}
