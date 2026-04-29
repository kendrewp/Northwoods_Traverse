using Traverse.Domain.Primitives.Abstractions;
using Traverse.Domain.Primitives.Identity;

namespace Traverse.Domain.Primitives.Aggregates;

/// <summary>
/// Abstract aggregate root base for the WorkItem bounded context.
/// </summary>
/// <remarks>
/// <see cref="AssignedTo"/> stores either an email or another user identifier
/// chosen by the service. The 256-character upper bound is documented here and
/// enforced by EF Core configuration in service projects (the domain primitive
/// project does not depend on EF Core, so the constraint is split: domain
/// validates non-empty here, EF Core enforces length downstream).
/// </remarks>
public abstract class WorkItemBase : AggregateRoot
{
    /// <summary>Identity of this work item.</summary>
    public WorkItemId WorkItemId { get; protected init; }

    /// <summary>Identity of the case this work item belongs to.</summary>
    public CaseId CaseId { get; protected init; }

    private string _assignedTo = string.Empty;

    /// <summary>
    /// User identifier (typically email) of the assignee.
    /// Maximum length 256 — enforced via EF Core configuration in service projects.
    /// </summary>
    public string AssignedTo
    {
        get => _assignedTo;
        protected set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("AssignedTo cannot be null or whitespace", nameof(value));
            }

            _assignedTo = value;
        }
    }
}
