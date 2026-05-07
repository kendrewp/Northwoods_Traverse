using Traverse.Domain.Primitives.Abstractions;
using Traverse.Domain.Primitives.Identity;

namespace Traverse.Domain.Primitives.Aggregates;

/// <summary>
/// Abstract aggregate root base for the User bounded context.
/// </summary>
/// <remarks>
/// <para>
/// Roles are exposed as <see cref="IReadOnlyList{T}"/> so callers cannot mutate
/// the underlying collection — derived types replace the whole list to change
/// roles, which makes the change auditable.
/// </para>
/// </remarks>
public abstract class UserBase : AggregateRoot
{
    /// <summary>Identity of this user.</summary>
    public UserId UserId { get; protected init; }

    private string _email = string.Empty;

    /// <summary>
    /// Email address. Maximum length 512 — enforced via EF Core configuration
    /// in service projects.
    /// </summary>
    public string Email
    {
        get => _email;
        protected set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Email cannot be null or whitespace", nameof(value));
            }

            _email = value;
        }
    }

    // Backing list so we can return a read-only view without exposing the
    // mutable collection. Derived types call the protected setter to swap the
    // entire list rather than mutating an existing one.
    private List<string> _roles = new();

    /// <summary>
    /// Roles assigned to this user. Read-only view — derived aggregates assign
    /// a new collection to change roles, which keeps the change atomic and
    /// auditable.
    /// </summary>
    public IReadOnlyList<string> Roles
    {
        get => _roles.AsReadOnly();
        protected set
        {
            ArgumentNullException.ThrowIfNull(value);
            _roles = new List<string>(value);
        }
    }
}
