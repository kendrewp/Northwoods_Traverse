using Traverse.Domain.Primitives.Abstractions;
using Traverse.Domain.Primitives.Enums;
using Traverse.Domain.Primitives.Identity;

namespace Traverse.Domain.Primitives.Aggregates;

/// <summary>
/// Abstract aggregate root base for the Program bounded context.
/// </summary>
/// <remarks>
/// A "program" groups cases under a single funding/policy umbrella. The
/// <see cref="Name"/> max length 512 is documented here and enforced by EF Core
/// configuration in service projects.
/// </remarks>
public abstract class ProgramBase : AggregateRoot
{
    /// <summary>Identity of this program.</summary>
    public ProgramId ProgramId { get; protected init; }

    /// <summary>Taxonomy classification — see <see cref="Enums.CaseType"/>.</summary>
    public CaseType CaseType { get; protected init; }

    private string _name = string.Empty;

    /// <summary>
    /// Display name. Maximum length 512 — enforced via EF Core configuration in
    /// service projects.
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
