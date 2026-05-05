using Microsoft.AspNetCore.Builder;
using Traverse.Infrastructure.Http.Handlers;
using Traverse.Infrastructure.Http.Middleware;

namespace Traverse.Infrastructure.Http.Extensions;

/// <summary>
/// Extension methods for registering and activating Traverse HTTP infrastructure
/// (correlation ID middleware and global exception handler).
/// </summary>
public static class HttpServiceCollectionExtensions
{
    /// <summary>
    /// Registers Traverse HTTP infrastructure services with the DI container.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Must be called in <c>Program.cs</c> before <c>app.Build()</c> so that
    /// all services are available when the middleware pipeline is constructed.
    /// </para>
    /// <para>
    /// Services registered:
    /// <list type="bullet">
    ///   <item><see cref="Middleware.CorrelationIdMiddleware"/> — <c>Transient</c>.
    ///   Required by the <c>IMiddleware</c> pattern; ASP.NET Core resolves a new
    ///   instance per request from the DI container.</item>
    ///   <item><see cref="Handlers.GlobalExceptionHandler"/> — registered via
    ///   <c>AddExceptionHandler&lt;T&gt;</c> which wires it into ASP.NET Core's
    ///   <c>IExceptionHandler</c> pipeline.</item>
    ///   <item><c>ProblemDetails</c> services — required by
    ///   <c>WriteAsJsonAsync(ProblemDetails)</c> to set the correct
    ///   <c>application/problem+json</c> content type.</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <param name="services">The DI service collection to register into.</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddTraverseHttp(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // IMiddleware implementations must be registered in the DI container.
        // Transient lifetime is correct — each request resolution is independent.
        services.AddTransient<CorrelationIdMiddleware>();

        // AddExceptionHandler<T> registers GlobalExceptionHandler as the
        // IExceptionHandler implementation invoked by UseExceptionHandler().
        services.AddExceptionHandler<GlobalExceptionHandler>();

        // Required by WriteAsJsonAsync to emit application/problem+json content
        // type on ProblemDetails responses.
        services.AddProblemDetails();

        return services;
    }

    /// <summary>
    /// Adds Traverse HTTP middleware to the ASP.NET Core request pipeline.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Must be called in <c>Program.cs</c> after <c>app.Build()</c> and
    /// <strong>before</strong> any routing or endpoint middleware so that
    /// correlation IDs are propagated to all downstream log events and
    /// exceptions from all middleware are captured.
    /// </para>
    /// <para>
    /// Middleware activated in order:
    /// <list type="number">
    ///   <item><see cref="Middleware.CorrelationIdMiddleware"/> — propagates or
    ///   generates the <c>X-Correlation-Id</c> header and enriches Serilog
    ///   context.</item>
    ///   <item><c>UseExceptionHandler()</c> — activates ASP.NET Core's built-in
    ///   exception handler middleware which calls <see cref="Handlers.GlobalExceptionHandler"/>
    ///   for every unhandled exception. The no-argument overload requires
    ///   <see cref="Handlers.GlobalExceptionHandler"/> to be registered via
    ///   <c>AddExceptionHandler&lt;T&gt;</c> (done in
    ///   <see cref="AddTraverseHttp"/>).</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <param name="app">The application builder to configure.</param>
    /// <returns>The same <paramref name="app"/> for chaining.</returns>
    public static IApplicationBuilder UseTraverseHttp(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        // Correlation ID first — ensures every subsequent log event (including
        // those from the exception handler) is enriched with the correlation ID.
        app.UseMiddleware<CorrelationIdMiddleware>();

        // Exception handler after correlation ID but before routing — catches
        // exceptions from the entire downstream pipeline and maps them to
        // ProblemDetails responses. The no-argument overload delegates to
        // the registered IExceptionHandler implementations.
        app.UseExceptionHandler();

        return app;
    }
}
