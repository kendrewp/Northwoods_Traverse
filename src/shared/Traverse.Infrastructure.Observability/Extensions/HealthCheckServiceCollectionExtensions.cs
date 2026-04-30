namespace Traverse.Infrastructure.Observability.Extensions;

/// <summary>
/// Extension methods for registering the Traverse platform's base health checks.
/// </summary>
public static class HealthCheckServiceCollectionExtensions
{
    /// <summary>
    /// Adds the <c>"self"</c> liveness health check to the provided
    /// <see cref="IHealthChecksBuilder"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <c>"self"</c> check always returns <see cref="HealthCheckResult.Healthy"/>
    /// and is tagged <c>"live"</c>. It is mapped to the <c>/health/live</c>
    /// Kubernetes liveness probe endpoint by
    /// <see cref="WebApplicationExtensions.MapTraverseHealthChecks"/>. A liveness
    /// probe that always passes indicates that the process is running and has not
    /// deadlocked — Kubernetes restarts the pod if this probe fails.
    /// </para>
    /// <para>
    /// <strong>Readiness checks</strong> (database connectivity, broker reachability,
    /// etc.) are added by each concrete service in its own <c>Program.cs</c> by
    /// chaining further calls on the returned <see cref="IHealthChecksBuilder"/>.
    /// Readiness checks should be tagged <c>"ready"</c> to appear under
    /// <c>/health/ready</c>.
    /// </para>
    /// <para>
    /// Example service usage:
    /// <code>
    /// builder.Services
    ///     .AddHealthChecks()
    ///     .AddTraverseHealthChecks()                     // adds "self" liveness check
    ///     .AddNpgsql(connectionString, tags: new[] { "ready" })  // readiness: DB
    ///     .AddRabbitMQ(rabbitUri, tags: new[] { "ready" });      // readiness: broker
    /// </code>
    /// </para>
    /// </remarks>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/> to add the
    /// check to. Returned by <c>services.AddHealthChecks()</c>.</param>
    /// <returns>The same <paramref name="builder"/> for chaining additional
    /// service-specific health checks.</returns>
    public static IHealthChecksBuilder AddTraverseHealthChecks(this IHealthChecksBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // "self" liveness check — always healthy so long as the process is alive.
        // Tagged "live" so it is picked up by the /health/live endpoint predicate
        // in WebApplicationExtensions.MapTraverseHealthChecks.
        builder.AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" });

        return builder;
    }
}
