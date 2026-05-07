namespace Traverse.Infrastructure.Http.Middleware;

/// <summary>
/// ASP.NET Core middleware that propagates a correlation ID through the HTTP
/// request/response cycle and enriches the Serilog log context with it.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Why <c>IMiddleware</c> instead of the <c>RequestDelegate</c>
/// constructor pattern:</strong> the <c>IMiddleware</c> interface requires the
/// middleware to be registered with the DI container (as <c>Transient</c>),
/// which means it can receive constructor-injected services with any lifetime
/// without creating captive-dependency bugs. The <c>RequestDelegate</c>
/// constructor pattern instantiates middleware as a Singleton with its
/// dependencies captured at startup — unsuitable for Scoped dependencies such
/// as <c>ICurrentUser</c>. Although this middleware has no Scoped dependencies
/// today, the <c>IMiddleware</c> pattern is the consistent choice for
/// discoverability and future safety.
/// </para>
/// <para>
/// <strong>Correlation ID semantics:</strong> if the incoming request carries
/// an <c>X-Correlation-Id</c> header, its value is preserved and echoed back
/// on the response — allowing upstream callers to trace a request across
/// services. If no header is present, a new <c>Guid</c> is generated and both
/// the response header and the log context are set with the new value.
/// </para>
/// <para>
/// <strong>Serilog log context:</strong> <see cref="LogContext.PushProperty"/>
/// enriches every log event emitted during the remainder of the request
/// pipeline with <c>CorrelationId</c>. If Serilog is not configured (e.g., in
/// unit tests using the default <c>Log.Logger</c>), <c>PushProperty</c> is a
/// no-op and no exception is thrown — the disposable returned by
/// <c>PushProperty</c> is always safe to dispose.
/// </para>
/// </remarks>
public sealed class CorrelationIdMiddleware : IMiddleware
{
    /// <summary>HTTP header name used to carry the correlation ID.</summary>
    private const string Header = "X-Correlation-Id";

    /// <inheritdoc />
    /// <remarks>
    /// Reads or generates the correlation ID, writes it to the response header,
    /// pushes it to the Serilog log context, then invokes the next middleware.
    /// </remarks>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        // Attempt to read the correlation ID forwarded by an upstream service
        // (API gateway, load balancer, or calling microservice). Fall back to
        // a new GUID for the first hop in a chain.
        var correlationId = context.Request.Headers.TryGetValue(Header, out var existing)
            ? existing.ToString()
            : Guid.NewGuid().ToString();

        // Echo the correlation ID on the response so callers can correlate
        // their request with server-side logs even when the value was
        // auto-generated here.
        context.Response.Headers[Header] = correlationId;

        // Enrich every Serilog log event for the remainder of this request
        // with the correlation ID. The using block ensures the property is
        // removed from the context after the request completes, preventing
        // bleed-over into the next request on the same thread.
        // This call is a no-op if Serilog is not configured — see remarks.
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next.Invoke(context).ConfigureAwait(false);
        }
    }
}
