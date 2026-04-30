using Microsoft.Extensions.Configuration;
using OpenTelemetry.Exporter;

namespace Traverse.Infrastructure.Observability.Extensions;

/// <summary>
/// Extension methods for registering Traverse observability infrastructure
/// (Serilog structured logging, OpenTelemetry tracing and metrics).
/// </summary>
public static class ObservabilityServiceCollectionExtensions
{
    /// <summary>
    /// Configures Serilog structured logging and OpenTelemetry distributed
    /// tracing and metrics for the provided host application builder.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Serilog:</strong> logs are emitted as compact JSON to the console
    /// (for container stdout collection) and to a rolling daily file under
    /// <c>logs/{serviceName}-YYYYMMDD.log</c>. The <c>Microsoft.*</c> namespace
    /// is overridden to <c>Warning</c> level to reduce framework noise. Every
    /// log event is enriched with a <c>Service</c> property equal to
    /// <paramref name="serviceName"/> so logs from multiple services in the same
    /// aggregator can be filtered by service name.
    /// </para>
    /// <para>
    /// <strong>OpenTelemetry:</strong> ASP.NET Core and HTTP client
    /// instrumentation are always registered. The OTLP exporter is registered
    /// conditionally — only when <c>Otel:Endpoint</c> is set in configuration.
    /// This allows services to run in local development without a collector,
    /// while production deployments automatically export traces and metrics to
    /// the configured collector endpoint (e.g., OpenTelemetry Collector, Jaeger,
    /// Grafana Tempo).
    /// </para>
    /// <para>
    /// <strong>Required appsettings.json structure (optional in dev):</strong>
    /// <code>
    /// {
    ///   "Otel": {
    ///     "Endpoint": "http://otel-collector:4317"
    ///   }
    /// }
    /// </code>
    /// When <c>Otel:Endpoint</c> is absent or empty, the OTLP exporter is
    /// silently skipped (graceful no-op). This is a deliberate design decision
    /// (AC-6) — requiring a collector in local dev would increase onboarding
    /// friction with no benefit.
    /// </para>
    /// <para>
    /// <strong>DIP:</strong> the method operates on <see cref="IHostApplicationBuilder"/>
    /// (an abstraction), not <c>WebApplicationBuilder</c> directly, so it can
    /// be used by worker services and minimal API hosts alike.
    /// </para>
    /// </remarks>
    /// <param name="builder">The host application builder to configure.</param>
    /// <param name="serviceName">
    /// Logical service name (e.g., <c>"workflow-service"</c>) used to tag log
    /// events and OpenTelemetry resource attributes. Should be kebab-case and
    /// stable across deployments.
    /// </param>
    /// <returns>The same <paramref name="builder"/> for chaining.</returns>
    public static IHostApplicationBuilder AddTraverseObservability(
        this IHostApplicationBuilder builder,
        string serviceName)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);

        ConfigureSerilog(builder, serviceName);
        ConfigureOpenTelemetry(builder, serviceName);

        return builder;
    }

    /// <summary>
    /// Configures Serilog as the logging provider for the application, writing
    /// structured JSON to both console and rolling daily files.
    /// </summary>
    private static void ConfigureSerilog(IHostApplicationBuilder builder, string serviceName)
    {
        // AddSerilog with a configuration action replaces the default
        // Microsoft.Extensions.Logging providers. The configuration action
        // receives a LoggerConfiguration on which we apply enrichers and sinks.
        builder.Services.AddSerilog(loggerConfig => loggerConfig
            .MinimumLevel.Information()
            // Suppress verbose Microsoft framework logs — they dominate output
            // and rarely represent business-relevant events in production.
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            // Enrich every log event with ambient properties pushed to the
            // Serilog log context (e.g., CorrelationId from the middleware).
            .Enrich.FromLogContext()
            // Tag every event with the service name so log aggregators can
            // filter by service without parsing the message text.
            .Enrich.WithProperty("Service", serviceName)
            // Console sink: compact JSON format for container stdout/stderr
            // collection by Docker, Kubernetes, or a log agent (Filebeat, etc.).
            .WriteTo.Console(new CompactJsonFormatter())
            // File sink: rolling daily files. Useful in non-containerised
            // environments and for local development diagnostics. The file
            // name includes a date suffix (e.g., workflow-service-20260428.log).
            .WriteTo.File(
                new CompactJsonFormatter(),
                path: $"logs/{serviceName}-.log",
                rollingInterval: RollingInterval.Day));
    }

    /// <summary>
    /// Configures OpenTelemetry tracing and metrics with ASP.NET Core and HTTP
    /// client instrumentation. OTLP export is added only when configured.
    /// </summary>
    private static void ConfigureOpenTelemetry(IHostApplicationBuilder builder, string serviceName)
    {
        var otlpEndpoint = builder.Configuration["Otel:Endpoint"];

        var otelBuilder = builder.Services
            .AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                // Conditionally add the OTLP exporter — see class-level remarks
                // on the graceful no-op behaviour in local development (AC-6).
                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    tracing.AddOtlpExporter(opt =>
                        opt.Endpoint = new Uri(otlpEndpoint));
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                // Same conditional pattern as tracing above.
                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    metrics.AddOtlpExporter(opt =>
                        opt.Endpoint = new Uri(otlpEndpoint));
                }
            });

        // Suppress the IDE warning about the variable being unused —
        // the builder is materialised when the host starts.
        _ = otelBuilder;
    }
}
