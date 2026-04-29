namespace Traverse.Infrastructure.Auth.Attributes;

/// <summary>
/// Convenience attribute that restricts an endpoint or controller to one or
/// more named roles, expressed as readable string arguments rather than a
/// bare comma-separated string.
/// </summary>
/// <remarks>
/// <para>
/// This attribute is a thin wrapper over <see cref="AuthorizeAttribute.Roles"/>.
/// It exists because the built-in usage —
/// <c>[Authorize(Roles = "CaseWorker,Supervisor")]</c> — is brittle: typos in
/// the string are not caught at compile time, and long role lists are hard to
/// read. <see cref="HasRoleAttribute"/> makes the same constraint explicit:
/// <c>[HasRole("CaseWorker", "Supervisor")]</c>.
/// </para>
/// <para>
/// <strong>ASP.NET Core behaviour:</strong> <see cref="AuthorizeAttribute.Roles"/>
/// is a comma-separated string where the framework grants access when the
/// principal holds <em>any</em> of the listed roles (OR semantics). This
/// attribute mirrors that behaviour by joining the provided role names with a
/// comma.
/// </para>
/// <para>
/// <strong>AND semantics (all roles required):</strong> stack multiple
/// <c>[HasRole]</c> attributes — each attribute represents an independent
/// authorization requirement and they are AND-composed by the framework.
/// </para>
/// </remarks>
/// <param name="roles">One or more role names that the principal must hold
/// (OR semantics within a single attribute; AND semantics across stacked
/// attributes).</param>
/// <example>
/// <code>
/// // Permit CaseWorkers OR Supervisors (OR within one attribute):
/// [HasRole("CaseWorker", "Supervisor")]
/// public IActionResult GetCase(Guid id) { ... }
///
/// // Require both Admin AND Supervisor (AND across stacked attributes):
/// [HasRole("Admin")]
/// [HasRole("Supervisor")]
/// public IActionResult AdminAction() { ... }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class HasRoleAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Initialises the attribute and sets the base <see cref="AuthorizeAttribute.Roles"/>
    /// property to a comma-joined string of the supplied role names.
    /// </summary>
    /// <param name="roles">One or more role names (OR semantics within one attribute).</param>
    /// <remarks>
    /// Uses a regular constructor (not a primary constructor) so that
    /// <c>base.Roles</c> can be assigned. <see cref="AuthorizeAttribute.Roles"/> is a
    /// non-virtual property — a <c>new</c> shadow property on the derived class would be
    /// invisible to the ASP.NET Core authorization middleware, which casts to
    /// <see cref="AuthorizeAttribute"/> and reads the base-class property directly.
    /// Assigning <c>Roles</c> in the constructor body is the only way to populate the
    /// property the framework actually consults.
    /// See: https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles
    /// </remarks>
    public HasRoleAttribute(params string[] roles)
    {
        // Join role names with a comma — this is the format that AuthorizeAttribute
        // parses to produce the OR-semantics role policy. The framework reads the
        // base Roles property during authorization evaluation.
        Roles = string.Join(',', roles);
    }
}
