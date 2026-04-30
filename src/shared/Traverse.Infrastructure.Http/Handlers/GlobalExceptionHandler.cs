using Microsoft.AspNetCore.Mvc;

namespace Traverse.Infrastructure.Http.Handlers;

/// <summary>
/// Global exception handler that maps all unhandled exceptions to RFC 7807
/// <see cref="ProblemDetails"/> responses.
/// </summary>
/// <remarks>
/// <para>
/// Registered via <c>services.AddExceptionHandler&lt;GlobalExceptionHandler&gt;()</c>
/// and activated by <c>app.UseExceptionHandler()</c> (no-argument overload).
/// ASP.NET Core invokes <see cref="TryHandleAsync"/> for every unhandled
/// exception that reaches the exception handler middleware.
/// </para>
/// <para>
/// <strong>Exception priority (most specific first):</strong>
/// <list type="number">
///   <item><see cref="DomainException"/> — maps to the exception's own
///   <c>StatusCode</c> and <c>ErrorCode</c>. This is the primary mapping for
///   business-rule violations (400 Bad Request, 404 Not Found, 409 Conflict,
///   etc.).</item>
///   <item><see cref="ValidationException"/> — maps to 400 Bad Request with a
///   structured <c>errors</c> extension that groups validation messages by
///   property name. This is the standard format expected by Angular form
///   controls (<c>mat-error</c>).</item>
///   <item>All other <see cref="Exception"/> types — maps to 500 Internal
///   Server Error with a generic user-facing message. The original exception
///   is logged at <c>Error</c> level for server-side diagnosis.</item>
/// </list>
/// </para>
/// <para>
/// <strong>OCP (Open/Closed Principle):</strong> new exception types can be
/// handled by adding cases to the switch expression without modifying existing
/// cases.
/// </para>
/// </remarks>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    /// <summary>
    /// Constructs the handler with a logger for server-side exception recording.
    /// </summary>
    /// <param name="logger">Logger used to record unhandled exceptions.</param>
    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Always returns <c>true</c> — this handler claims every exception and
    /// writes a ProblemDetails JSON response. Returning <c>false</c> would
    /// allow the exception to propagate to the next exception handler (or
    /// produce a 500 with no body), which is not the desired behaviour for a
    /// global catch-all.
    /// </remarks>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(exception);

        // Log before writing the response so the log event is always captured
        // regardless of whether WriteAsJsonAsync succeeds.
        _logger.LogError(exception, "Unhandled exception occurred. Path: {Path}", httpContext.Request.Path);

        var problem = exception switch
        {
            // Domain rule violations — use the exception's structured metadata
            // so the HTTP response mirrors the domain's intent exactly.
            DomainException d => new ProblemDetails
            {
                Status = d.StatusCode,
                Title = d.ErrorCode,
                Detail = d.Message
            },

            // FluentValidation errors — produce a structured 'errors' extension
            // that maps property names to their validation messages.
            // This format aligns with Angular Material's mat-error binding pattern
            // and the RFC 7807 'validation_error' convention.
            ValidationException v => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "validation_error",
                Detail = "One or more validation errors occurred.",
                Extensions =
                {
                    ["errors"] = v.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray())
                }
            },

            // Catch-all — never expose internal details to the client.
            // The full stack trace is preserved in the log above.
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "internal_error",
                Detail = "An unexpected error occurred."
            }
        };

        httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;

        // WriteAsJsonAsync sets Content-Type: application/problem+json when
        // the object type is ProblemDetails or a subtype.
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken).ConfigureAwait(false);

        // Return true to signal that the exception has been handled and no
        // further exception-handler middleware should be invoked.
        return true;
    }
}
