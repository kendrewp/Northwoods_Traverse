// ============================================================================
// Traverse.Compliance.Api — Program.cs
// Phase 0 API stub: wires shared infrastructure and exposes health check
// endpoints only. No business routes are registered here — they are added by
// the Phase 1 story that owns this bounded context.
//
// Middleware pipeline order (enforced by ASP.NET Core conventions):
//   1. CorrelationIdMiddleware  — tag every request with a trace ID first
//   2. UseExceptionHandler      — catch all downstream exceptions (via UseTraverseHttp)
//   3. UseAuthentication        — validate JWT tokens
//   4. UseAuthorization         — enforce policy requirements
//   5. MapHealthChecks          — /health/live and /health/ready endpoints
//   6. MapOpenApi               — /openapi/v1.json (development only)
// ============================================================================

using Traverse.Infrastructure.Auth.Extensions;
using Traverse.Infrastructure.Http.Extensions;
using Traverse.Infrastructure.Messaging.Extensions;
using Traverse.Infrastructure.Observability.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Observability — FIRST: ensures Serilog captures startup logs and OTel
// instruments the full request pipeline from the earliest possible point.
// serviceName is used to tag all log events and OTel resource attributes.
// ---------------------------------------------------------------------------
builder.AddTraverseObservability("compliance-service");

// ---------------------------------------------------------------------------
// Health checks — register the "self" liveness check (tagged "live").
// Each Phase 1 story extends this by chaining additional checks tagged "ready".
// ---------------------------------------------------------------------------
builder.Services
    .AddHealthChecks()
    .AddTraverseHealthChecks();

// ---------------------------------------------------------------------------
// HTTP infrastructure — registers CorrelationIdMiddleware (transient) and
// GlobalExceptionHandler (IExceptionHandler), and AddProblemDetails.
// ---------------------------------------------------------------------------
builder.Services.AddTraverseHttp();

// ---------------------------------------------------------------------------
// Authentication — JWT Bearer with placeholder Authority/Audience from
// appsettings.json. Auth:Authority is empty in Phase 0; JWT validation is
// not enforced until the Phase 1 Auth story wires the OIDC provider.
// Phase 0 health endpoints bypass auth via AllowAnonymous() in
// MapTraverseHealthChecks.
// ---------------------------------------------------------------------------
builder.Services.AddTraverseAuth(builder.Configuration, builder.Environment);

// ---------------------------------------------------------------------------
// Authorisation — required by app.UseAuthorization() below.
// No policies are defined in Phase 0; Phase 1 stories add resource-level
// policies.
// ---------------------------------------------------------------------------
builder.Services.AddAuthorization();

// ---------------------------------------------------------------------------
// Messaging — MassTransit/RabbitMQ with connection params from appsettings.
// Phase 0: no consumers are registered. Phase 1 stories pass a configure
// delegate to register their consumers.
// ---------------------------------------------------------------------------
builder.Services.AddTraverseMessaging(builder.Configuration);

// ---------------------------------------------------------------------------
// OpenAPI — Scalar document generation. Registered unconditionally;
// the endpoint is exposed only in development (MapOpenApi guard below).
// ---------------------------------------------------------------------------
builder.Services.AddOpenApi();

// ---------------------------------------------------------------------------
// Build the application — all services must be registered before this call.
// ---------------------------------------------------------------------------
var app = builder.Build();

// ---------------------------------------------------------------------------
// Middleware pipeline — order matters. UseTraverseHttp activates
// CorrelationIdMiddleware (first) and UseExceptionHandler (second) so
// correlation IDs are present in all downstream logs including error responses.
// ---------------------------------------------------------------------------
app.UseTraverseHttp();

// UseAuthentication and UseAuthorization must appear after UseExceptionHandler
// and before any endpoint middleware that enforces [Authorize].
app.UseAuthentication();
app.UseAuthorization();

// ---------------------------------------------------------------------------
// Endpoint mapping — health checks and OpenAPI only in Phase 0.
// MapTraverseHealthChecks wires /health/live (tag: "live") and
// /health/ready (tag: "ready") with AllowAnonymous().
// ---------------------------------------------------------------------------
app.MapTraverseHealthChecks();

// Expose the OpenAPI document in development only. Production containers do
// not serve the spec endpoint — this reduces the attack surface and avoids
// unintentional API discovery in staging/prod environments.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// ---------------------------------------------------------------------------
// Start the application.
// ---------------------------------------------------------------------------
app.Run();
