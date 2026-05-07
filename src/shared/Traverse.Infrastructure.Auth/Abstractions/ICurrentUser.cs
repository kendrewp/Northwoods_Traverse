namespace Traverse.Infrastructure.Auth.Abstractions;

/// <summary>
/// Provides strongly-typed access to the identity of the current HTTP request's
/// authenticated user.
/// </summary>
/// <remarks>
/// <para>
/// Application layer code (commands, queries) depends on this interface rather
/// than on <see cref="IHttpContextAccessor"/> or <c>ClaimsPrincipal</c> directly,
/// keeping infrastructure details out of the domain (DIP). The single registered
/// implementation, <see cref="Services.CurrentUser"/>, reads from
/// <see cref="IHttpContextAccessor"/> at call time — so values are always
/// fresh for the current request.
/// </para>
/// <para>
/// <strong>ISP applied:</strong> only the four properties callers actually need
/// are exposed. Callers that require raw claims can inject
/// <see cref="IHttpContextAccessor"/> directly — they are not forced through
/// this interface.
/// </para>
/// </remarks>
public interface ICurrentUser
{
    /// <summary>
    /// Subject identifier of the authenticated user (the <c>sub</c> JWT claim,
    /// falling back to <c>nameidentifier</c> for legacy tokens).
    /// Returns <see cref="string.Empty"/> when the request is unauthenticated.
    /// </summary>
    string UserId { get; }

    /// <summary>
    /// Email address of the authenticated user (the <c>email</c> JWT claim).
    /// Returns <see cref="string.Empty"/> when the claim is absent or the
    /// request is unauthenticated.
    /// </summary>
    string Email { get; }

    /// <summary>
    /// All role claim values for the authenticated user (the <c>role</c> JWT
    /// claim, which may be multi-valued). Returns an empty list when the
    /// request is unauthenticated.
    /// </summary>
    IReadOnlyList<string> Roles { get; }

    /// <summary>
    /// <c>true</c> when the current request carries a valid, authenticated
    /// identity; <c>false</c> otherwise (anonymous or unauthenticated).
    /// </summary>
    bool IsAuthenticated { get; }
}
