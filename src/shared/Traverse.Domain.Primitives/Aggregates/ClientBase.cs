using Traverse.Domain.Primitives.Abstractions;
using Traverse.Domain.Primitives.Identity;

namespace Traverse.Domain.Primitives.Aggregates;

/// <summary>
/// Abstract aggregate root base for the Client bounded context.
/// </summary>
/// <remarks>
/// Concrete service implementations derive from this type and add their own
/// invariants, business methods, and domain events. The <c>Base</c> suffix
/// distinguishes the cross-service base from each service's concrete
/// <c>Client</c> aggregate (per audit finding AF-002).
/// </remarks>
public abstract class ClientBase : AggregateRoot
{
    /// <summary>Identity of this client. Set once via factory/constructor in derived types.</summary>
    public ClientId ClientId { get; protected init; }

    private string _name = string.Empty;

    /// <summary>
    /// Display name for the client. Validated non-null/non-whitespace on every
    /// assignment so the invariant is upheld even if a derived type sets the
    /// property outside the constructor.
    /// </summary>
    public string Name
    {
        get => _name;
        protected set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Name cannot be null or whitespace", nameof(value));
            }

            _name = value;
        }
    }
}
