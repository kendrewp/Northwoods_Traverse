using Microsoft.AspNetCore.Hosting;
using Traverse.Infrastructure.Auth.Abstractions;
using Traverse.Infrastructure.Auth.Services;

namespace Traverse.Infrastructure.Auth.Extensions;

/// <summary>
/// <see cref="IServiceCollection"/> extension methods for registering Traverse
/// authentication and authorisation infrastructure.
/// </summary>
public static class AuthServiceCollectionExtensions
{
    /// <summary>
    /// Registers JWT Bearer authentication, the <see cref="ICurrentUser"/>
    /// abstraction, and HTTP context accessor with the DI container.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Required appsettings.json structure:</strong>
    /// <code>
    /// {
    ///   "Auth": {
    ///     "Authority": "https://your-oidc-provider.example.com",
    ///     "Audience":  "traverse-api"
    ///   }
    /// }
    /// </code>
    /// Both keys are Phase 0 placeholders — the OIDC provider URL and audience
    /// identifier are filled in during Phase 1 (Auth story). In Phase 0 the
    /// values are intentionally absent; the JWT handler will not validate tokens
    /// until a real OIDC provider is wired.
    /// </para>
    /// <para>
    /// <strong><see cref="ICurrentUser"/> lifetime:</strong> registered as
    /// <c>Scoped</c>. One instance is created per HTTP request and disposed
    /// when the request ends. This matches the lifetime of
    /// <see cref="IHttpContextAccessor"/> itself — registering as Singleton
    /// would cause the accessor to capture a stale <c>HttpContext</c> across
    /// requests.
    /// </para>
    /// <para>
    /// <strong>HTTPS metadata:</strong> <c>RequireHttpsMetadata</c> is
    /// <c>false</c> in development (allows plain-text OIDC discovery endpoints
    /// on localhost) and <c>true</c> in all other environments. Never deploy
    /// with metadata validation disabled in production.
    /// </para>
    /// </remarks>
    /// <param name="services">The DI service collection to register into.</param>
    /// <param name="configuration">Application configuration — should contain
    /// the <c>Auth</c> section.</param>
    /// <param name="env">Hosting environment — used to determine whether HTTPS
    /// metadata validation should be enforced.</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddTraverseAuth(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment env)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(env);

        // IHttpContextAccessor is required by CurrentUser to read the
        // ClaimsPrincipal from the current HTTP request. This call is
        // idempotent — calling it multiple times does not register duplicate
        // instances.
        services.AddHttpContextAccessor();

        // Register ICurrentUser as Scoped — per-request lifetime mirrors
        // the HttpContext lifetime. See class-level remarks on CurrentUser.
        services.AddScoped<ICurrentUser, CurrentUser>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Auth:Authority — the OIDC discovery endpoint base URL.
                // Null in Phase 0; the handler will not perform token
                // validation until the authority is configured (Phase 1).
                options.Authority = configuration["Auth:Authority"];

                // Auth:Audience — the expected 'aud' claim value.
                // Null in Phase 0; validation is deferred to Phase 1.
                options.Audience = configuration["Auth:Audience"];

                // Enforce HTTPS for OIDC discovery and token validation in
                // all non-development environments. In development, plain-text
                // localhost endpoints are permitted for local OIDC stubs.
                options.RequireHttpsMetadata = !env.IsDevelopment();
            });

        return services;
    }
}
