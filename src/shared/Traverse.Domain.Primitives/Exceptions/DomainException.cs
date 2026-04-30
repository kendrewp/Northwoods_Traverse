namespace Traverse.Domain.Primitives.Exceptions;

/// <summary>
/// Base class for all domain rule violations in the Traverse platform.
/// </summary>
/// <remarks>
/// <para>
/// Domain exceptions live in the domain project — not the HTTP project — so
/// the domain layer can throw rich, structured errors without taking a
/// dependency on infrastructure (DIP). The HTTP layer knows how to map a
/// <see cref="DomainException"/> to <c>ProblemDetails</c>; the domain layer
/// itself never has to know what an HTTP response looks like.
/// </para>
/// <para>
/// <see cref="ErrorCode"/> is a stable, machine-readable identifier (e.g.,
/// <c>"order_not_found"</c>) that callers and clients pattern-match on.
/// <see cref="StatusCode"/> defaults to 400 (Bad Request) because most domain
/// rule violations are caller errors; concrete subclasses override it for
/// not-found (404), conflict (409), and similar cases.
/// </para>
/// </remarks>
/// <param name="message">Human-readable description.</param>
/// <param name="errorCode">Machine-readable error identifier (snake_case by convention).</param>
/// <param name="statusCode">HTTP status code that maps this error to a response.</param>
public abstract class DomainException(string message, string errorCode, int statusCode = 400)
    : Exception(message)
{
    /// <summary>Machine-readable error code (snake_case) for client-side error handling.</summary>
    public string ErrorCode { get; } = errorCode;

    /// <summary>HTTP status code that maps this domain error to an HTTP response.</summary>
    public int StatusCode { get; } = statusCode;
}
