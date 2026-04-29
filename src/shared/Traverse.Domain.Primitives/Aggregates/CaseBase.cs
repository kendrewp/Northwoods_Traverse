using Traverse.Domain.Primitives.Abstractions;
using Traverse.Domain.Primitives.Enums;
using Traverse.Domain.Primitives.Identity;

namespace Traverse.Domain.Primitives.Aggregates;

/// <summary>
/// Abstract aggregate root base for the Case bounded context.
/// </summary>
/// <remarks>
/// Concrete service implementations derive from this type. The <see cref="Status"/>
/// field is intentionally a <see cref="string"/> — each service has its own
/// status state machine, so an enum at the base level would impose a
/// premature taxonomy on every consumer.
/// </remarks>
public abstract class CaseBase : AggregateRoot
{
    /// <summary>Identity of this case.</summary>
    public CaseId CaseId { get; protected init; }

    /// <summary>Identity of the client this case belongs to.</summary>
    public ClientId ClientId { get; protected init; }

    /// <summary>Taxonomy classification — see <see cref="Enums.CaseType"/>.</summary>
    public CaseType CaseType { get; protected init; }

    private string _status = string.Empty;

    /// <summary>
    /// Service-defined status string. Validated non-null/non-whitespace on
    /// every set so the invariant survives derived-type assignments.
    /// </summary>
    public string Status
    {
        get => _status;
        protected set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Status cannot be null or whitespace", nameof(value));
            }

            _status = value;
        }
    }
}
