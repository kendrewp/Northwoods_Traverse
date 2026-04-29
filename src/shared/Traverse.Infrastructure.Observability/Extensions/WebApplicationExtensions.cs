using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;

namespace Traverse.Infrastructure.Observability.Extensions;

/// <summary>
/// Extension methods for mapping Traverse health check endpoints on a
/// <see cref="WebApplication"/>.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Maps the <c>/health/live</c> (liveness) and <c>/health/ready</c>
    /// (readiness) health check endpoints on the application.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Endpoint semantics:</strong>
    /// <list type="bullet">
    ///   <item><c>/health/live</c> — runs only checks tagged <c>"live"</c>
    ///   (currently just the <c>"self"</c> check registered by
    ///   <see cref="HealthCheckServiceCollectionExtensions.AddTraverseHealthChecks"/>).
    ///   A liveness failure causes Kubernetes to restart the pod.</item>
    ///   <item><c>/health/ready</c> — runs only checks tagged <c>"ready"</c>
    ///   (database, broker, external APIs — registered by each concrete service).
    ///   A readiness failure removes the pod from the load-balancer pool without
    ///   restarting it.</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Why <c>AllowAnonymous()</c>:</strong> Kubernetes (and most health
    /// check aggregators) issue HTTP GET requests to the probe endpoints without
    /// any authentication headers. Adding an <c>[Authorize]</c> requirement would
    /// cause probes to always fail with 401, eventually triggering pod eviction.
    /// These endpoints expose no sensitive data — they return only "Healthy" or
    /// "Unhealthy" — so anonymous access is safe and intentional.
    /// </para>
    /// <para>
    /// Both endpoints must be called after <c>app.UseRouting()</c> and
    /// <c>app.UseAuthentication()</c> / <c>app.UseAuthorization()</c> in the
    /// middleware pipeline. In ASP.NET Core minimal API applications using
    /// <c>app.MapXxx()</c>, ordering is handled automatically.
    /// </para>
    /// </remarks>
    /// <param name="app">The <see cref="WebApplication"/> to map endpoints on.</param>
    /// <returns>The same <paramref name="app"/> for chaining.</returns>
    public static WebApplication MapTraverseHealthChecks(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        // Liveness — process is running. No auth required: see remarks.
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            // Only include checks tagged "live" so liveness is not affected
            // by transient database or broker outages — those belong on
            // /health/ready.
            Predicate = r => r.Tags.Contains("live")
        }).AllowAnonymous();

        // Readiness — dependencies are reachable. No auth required: see remarks.
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            // Only include checks tagged "ready" so the readiness probe reflects
            // the service's ability to handle traffic, not its liveness status.
            Predicate = r => r.Tags.Contains("ready")
        }).AllowAnonymous();

        return app;
    }
}
