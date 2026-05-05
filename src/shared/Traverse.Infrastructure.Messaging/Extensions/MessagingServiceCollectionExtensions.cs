namespace Traverse.Infrastructure.Messaging.Extensions;

/// <summary>
/// <see cref="IServiceCollection"/> extension methods for registering Traverse
/// messaging infrastructure (MassTransit + RabbitMQ).
/// </summary>
public static class MessagingServiceCollectionExtensions
{
    /// <summary>
    /// Registers MassTransit with the RabbitMQ transport, configured from the
    /// <c>RabbitMq</c> section of <paramref name="configuration"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Required appsettings.json structure:</strong>
    /// <code>
    /// {
    ///   "RabbitMq": {
    ///     "Host":        "localhost",
    ///     "Port":        5672,
    ///     "Username":    "guest",
    ///     "Password":    "guest",
    ///     "VirtualHost": "/"
    ///   }
    /// }
    /// </code>
    /// All four keys are required. A missing or null <c>Host</c> value will
    /// cause MassTransit to throw at startup — this is intentional: a service
    /// with messaging enabled must have a broker configured. Fail-fast at
    /// startup is preferable to silent message loss at runtime.
    /// </para>
    /// <para>
    /// <strong>Optional <paramref name="configure"/> delegate:</strong> callers
    /// can pass an action to customise the RabbitMQ bus factory after the base
    /// host/port/credentials are applied — e.g., to register consumer endpoints
    /// for a specific service. This keeps service-specific routing out of this
    /// shared library (OCP).
    /// </para>
    /// <para>
    /// Consumers are discovered automatically via
    /// <c>ConfigureEndpoints(context)</c>, which uses MassTransit's convention-
    /// based endpoint naming. Pass the <paramref name="configure"/> action to
    /// override endpoint names or add service-specific configuration.
    /// </para>
    /// </remarks>
    /// <param name="services">The DI service collection to register into.</param>
    /// <param name="configuration">Application configuration — must contain the
    /// <c>RabbitMq</c> section.</param>
    /// <param name="configure">Optional delegate to apply additional RabbitMQ bus
    /// factory configuration after the transport is initialised.</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddTraverseMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IRabbitMqBusFactoryConfigurator>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var host = configuration["RabbitMq:Host"];
        var portRaw = configuration["RabbitMq:Port"];
        var username = configuration["RabbitMq:Username"];
        var password = configuration["RabbitMq:Password"];

        // Parse the port; default to 5672 (AMQP standard) if the key is
        // absent or cannot be parsed, so local dev environments work without
        // requiring the port key to be set explicitly.
        var port = ushort.TryParse(portRaw, out var parsedPort) ? parsedPort : (ushort)5672;

        // Virtual host defaults to "/" (RabbitMQ default vhost). Services
        // that require isolation between environments can override via the
        // optional configure delegate.
        var virtualHost = configuration["RabbitMq:VirtualHost"] ?? "/";

        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((ctx, cfg) =>
            {
                // MassTransit Host(host, port, virtualHost, configurator) overload.
                // A null host here means RabbitMq:Host is missing from
                // configuration — the resulting startup exception is the
                // intended fast-fail behaviour (see remarks).
                cfg.Host(host, port, virtualHost, h =>
                {
                    h.Username(username ?? string.Empty);
                    h.Password(password ?? string.Empty);
                });

                // Convention-based endpoint registration — MassTransit
                // derives queue names from consumer type names.
                cfg.ConfigureEndpoints(ctx);

                // Allow the caller to add service-specific configuration
                // (additional endpoints, retry policies, etc.) without
                // modifying this shared library.
                configure?.Invoke(cfg);
            });
        });

        return services;
    }
}
