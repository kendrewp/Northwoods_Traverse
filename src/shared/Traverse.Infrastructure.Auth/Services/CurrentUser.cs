using Traverse.Infrastructure.Auth.Abstractions;

namespace Traverse.Infrastructure.Auth.Services;

/// <summary>
/// ASP.NET Core implementation of <see cref="ICurrentUser"/> that reads JWT
/// claims from the current HTTP request's <see cref="ClaimsPrincipal"/>.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Thread-safety:</strong> this class is registered as Scoped — one
/// instance per HTTP request. It holds no mutable state of its own; every
/// property read delegates to <see cref="IHttpContextAccessor.HttpContext"/>,
/// which is itself per-request. There is no shared state between requests, so
/// no synchronisation is needed.
/// </para>
/// <para>
/// <strong>Unauthenticated fallback:</strong> all properties return safe empty
/// values (<see cref="string.Empty"/>, empty list, <c>false</c>) when the
/// request is anonymous or the accessor has no <c>HttpContext</c>. This
/// prevents null-reference exceptions in code paths that handle both
/// authenticated and unauthenticated scenarios.
/// </para>
/// <para>
/// <strong>Claim type mapping:</strong>
/// <list type="bullet">
///   <item><description><c>UserId</c> — prefers the <c>sub</c> claim (OIDC standard)
///   and falls back to <c>ClaimTypes.NameIdentifier</c> for legacy Windows/cookie
///   identity tokens.</description></item>
///   <item><description><c>Email</c> — reads <c>ClaimTypes.Email</c> which maps to
///   the <c>email</c> JWT claim after ASP.NET Core's JWT handler applies its default
///   claim type mapping.</description></item>
///   <item><description><c>Roles</c> — collects all <c>ClaimTypes.Role</c> values
///   because the <c>role</c> claim can be multi-valued in JWTs.</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    /// <summary>
    /// Constructs a <see cref="CurrentUser"/> backed by the supplied
    /// <paramref name="accessor"/>.
    /// </summary>
    /// <param name="accessor">ASP.NET Core HTTP context accessor — injected by DI.</param>
    public CurrentUser(IHttpContextAccessor accessor)
    {
        ArgumentNullException.ThrowIfNull(accessor);
        _accessor = accessor;
    }

    /// <inheritdoc />
    public string UserId =>
        // Prefer the OIDC standard 'sub' claim; fall back to the legacy
        // NameIdentifier claim type used by cookie and Windows auth.
        _accessor.HttpContext?.User.FindFirstValue("sub")
        ?? _accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? string.Empty;

    /// <inheritdoc />
    public string Email =>
        // ClaimTypes.Email maps to the 'email' JWT claim after ASP.NET Core's
        // default inbound claim type mapping is applied by the JWT handler.
        _accessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email)
        ?? string.Empty;

    /// <inheritdoc />
    public IReadOnlyList<string> Roles =>
        // The 'role' JWT claim is multi-valued — collect all occurrences so
        // callers get the full set of roles even when the token carries several.
        _accessor.HttpContext?.User
            .FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList()
        ?? (IReadOnlyList<string>)Array.Empty<string>();

    /// <inheritdoc />
    public bool IsAuthenticated =>
        _accessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}
