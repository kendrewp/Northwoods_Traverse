# Design: NT-005 — Base Microservice Projects (API Stubs)

> **Feature:** NT-000 — Phase 0 Scaffolding
> **Story:** NT-005 — Base Microservice Projects (API Stubs) — Phase 0e
> **Story short:** api_stubs
> **Author:** Kendrew Peacey (Dev Lead / PM / Stakeholder — single operator)
> **Status:** Approved (AutoMode — self-approved)
> **Date:** 2026-05-04
> **Skill:** design-feature v2.4.0
> **Architecture ref:** `../Northwoods_Traverse-feature-NT-000/docs/NT-000/architecture_phase0.md` (checksum verified: `sha256-5a122ac18c515fca56e4138941958286d540de20ce16dba145323aecaf28ae87`)

---

## Table of Contents

1. [Problem Statement](#1-problem-statement)
2. [Acceptance Criteria](#2-acceptance-criteria)
3. [Proposed Solution](#3-proposed-solution)
4. [Architecture](#4-architecture)
5. [Per-Project Specification](#5-per-project-specification)
6. [Data Model](#6-data-model)
7. [API Surface](#7-api-surface)
8. [Business Logic](#8-business-logic)
9. [Edge Cases](#9-edge-cases)
10. [Out of Scope](#10-out-of-scope)
11. [Effort Estimate (CU-Derived)](#11-effort-estimate-cu-derived)

---

## 1. Problem Statement

### Context

Phase 0 of the Northwoods Traverse platform creates the complete compilable skeleton before any feature work begins. NT-001 (Repository and Solution Structure) established the folder tree and solution file. NT-002 (Shared Backend Libraries) created the six shared infrastructure projects (`Traverse.Domain.Primitives`, `Traverse.Infrastructure.Persistence`, `Traverse.Infrastructure.Messaging`, `Traverse.Infrastructure.Auth`, `Traverse.Infrastructure.Http`, `Traverse.Infrastructure.Observability`). NT-004 (Docker Compose Local Dev Environment) defined docker-compose.yml with profile-gated API service stubs waiting for their Dockerfiles.

### Problem

As of NT-004 completion, the `docker-compose.yml` references nine `Dockerfile` paths (`docker/{service}/Dockerfile`) that do not yet exist. The `Traverse.sln` contains no service-layer projects — only the eight shared library projects. The nine service folders under `src/services/` are empty directories. This means:

1. `docker compose --profile api up` fails — the referenced Dockerfiles do not exist.
2. `dotnet build Traverse.sln` does not compile any service code — there is nothing to reference the shared libraries from.
3. Phase 1 stories (beginning with MOD-02 Workflow) have no project to extend — every Phase 1 story would need to first create its project from scratch, determine the correct DI wiring pattern, and invent its own `Program.cs` structure.

### Deliverable

NT-005 resolves all three problems by creating nine empty ASP.NET Core Web API projects — one per PRD module. Each project:

- References all six shared infrastructure libraries and wires them via their published `Add*` / `Use*` extension methods
- Exposes only `/health/live` and `/health/ready` endpoints (no business routes)
- Has a multi-stage `Dockerfile` in `docker/{service}/Dockerfile`
- Is registered in `Traverse.sln`
- Has a complete `appsettings.json` with connection string and RabbitMQ placeholders

The result is a fully compilable, fully runnable eight-layer platform skeleton. `docker compose --profile api up` succeeds. `dotnet build Traverse.sln` succeeds with zero warnings. Every Phase 1 story has a clean, correctly-wired project to extend.

---

## 2. Acceptance Criteria

### AC-1 — All 9 Projects Referenced in Traverse.sln

**Given** the Traverse.sln file in the repository root,
**When** `dotnet build Traverse.sln` is executed (with SDK 10.0.103 as pinned in `global.json`),
**Then** all 9 API projects compile successfully with zero errors and zero warnings (`TreatWarningsAsErrors` is enabled on all projects). Each project must appear as a `Project("{FAE04EC0...}")` entry in `Traverse.sln`, placed under the `services` solution folder.

**Verification:** `dotnet build Traverse.sln --no-incremental 2>&1 | grep -E "Error|Warning|Build succeeded"` — output must show `Build succeeded` with `0 Error(s)` and `0 Warning(s)`.

---

### AC-2 — Health Check Endpoints Respond on Correct Ports

**Given** all nine API containers started with `docker compose --profile api up`,
**When** `GET /health/live` is called on each service's mapped port,
**Then** each endpoint returns HTTP 200 with a JSON body that includes `"status": "Healthy"`.

| Service | Host Port |
|---------|-----------|
| `workflow-api` | 5001 |
| `kpi-api` | 5002 |
| `admin-api` | 5003 |
| `notifications-api` | 5004 |
| `reporting-api` | 5005 |
| `aicopilot-api` | 5006 |
| `compliance-api` | 5007 |
| `search-api` | 5008 |
| `calendar-api` | 5009 |

**Verification:** `curl -s http://localhost:500{N}/health/live` returns `{"status":"Healthy"}` for each port.

---

### AC-3 — Dockerfiles Present and Build Successfully

**Given** the repository root,
**When** `docker build -f docker/{service}/Dockerfile .` is run for each of the 9 services,
**Then** the image builds successfully to the final `runtime` stage — no build errors. Each `Dockerfile` must follow the 4-stage multi-stage pattern: `restore` → `build` → `publish` → `runtime`.

**Verification:** `docker build -f docker/workflow/Dockerfile . --target runtime --no-cache` (repeat for each service) exits with code 0.

---

### AC-4 — docker-compose.yml Service Entries Activated

**Given** the existing `docker-compose.yml` with profile-gated API service stubs (produced by NT-004),
**When** `docker compose --profile api up --build` is executed after NT-005 Dockerfiles are in place,
**Then** all 9 API containers start, pass their healthchecks (`/health/live` returns 200), and show `healthy` status in `docker compose ps`.

**Verification:** `docker compose --profile api up -d --build && sleep 45 && docker compose ps | grep -E "workflow|kpi|admin|notifications|reporting|aicopilot|compliance|search|calendar"` — all 9 rows show `(healthy)`.

---

## 3. Proposed Solution

### Approach: Single Shared Template, Per-Project Instantiation

All 9 projects are structurally identical at Phase 0. They differ only in:
- Project name and namespace
- Service name string (used for Serilog enrichment and OTel resource)
- Port number
- Database name in the connection string placeholder
- Docker context path (`docker/{service}/Dockerfile`)

The solution is to define **one canonical template** in this document and apply it to all 9 projects. A junior developer creates each project by substituting the service-specific values from the substitution table in §5.

### Template Pattern

Each API project is a `Microsoft.NET.Sdk.Web` project (not `Microsoft.NET.Sdk`) — this SDK type activates ASP.NET Core's `WebApplication` builder, implicit framework references (`Microsoft.AspNetCore.App`), and launchSettings support. The `FrameworkReference Include="Microsoft.AspNetCore.App"` is implicitly included by the Web SDK and must **not** be added explicitly to avoid NU1510 warnings.

The `Program.cs` calls the six shared infrastructure extension methods in a fixed order that mirrors the ASP.NET Core middleware pipeline contract:

**Registration phase (before `builder.Build()`):**
1. `builder.AddTraverseObservability(serviceName)` — Serilog + OTel
2. `builder.Services.AddHealthChecks().AddTraverseHealthChecks()` — liveness check
3. `builder.Services.AddTraverseHttp()` — correlation ID middleware + global exception handler registration
4. `builder.Services.AddTraverseAuth(builder.Configuration, builder.Environment)` — JWT Bearer stub
5. `builder.Services.AddTraverseMessaging(builder.Configuration)` — MassTransit/RabbitMQ stub
6. `builder.Services.AddOpenApi()` — Scalar/OpenAPI document generation

**Pipeline phase (after `app = builder.Build()`):**
1. `app.UseTraverseHttp()` — activates CorrelationIdMiddleware + UseExceptionHandler
2. `app.UseAuthentication()` — JWT validation pipeline
3. `app.UseAuthorization()` — authorisation pipeline
4. `app.MapTraverseHealthChecks()` — maps `/health/live` and `/health/ready`
5. `app.MapOpenApi()` (development-only guard) — maps `/openapi/v1.json`

### Dockerfile Pattern

Each `Dockerfile` uses the 4-stage multi-stage build pattern from `.NET Coding Standards §18`:

- **Stage 1 (`restore`):** SDK image; copy only `.csproj` files (layer-cache friendly); `dotnet restore`
- **Stage 2 (`build`):** SDK image; copy full source; `dotnet build -c Release --no-restore`
- **Stage 3 (`publish`):** SDK image; `dotnet publish -c Release --no-build /p:UseAppHost=false -o /app/publish`
- **Stage 4 (`runtime`):** ASP.NET runtime image; non-root user; copy from publish; `EXPOSE 8080`; `ENTRYPOINT`

The build context is always the repository root (`.`) so the `COPY` instructions can reference the shared library projects in `src/shared/`. All 9 Dockerfiles are structurally identical except for the project path.

---

## 4. Architecture

### Solution Structure After NT-005

```
Traverse.sln
├── [solution folder: shared]
│   ├── Traverse.AI.Abstractions
│   ├── Traverse.AI.Providers
│   ├── Traverse.Domain.Primitives
│   ├── Traverse.Infrastructure.Persistence
│   ├── Traverse.Infrastructure.Messaging
│   ├── Traverse.Infrastructure.Auth
│   ├── Traverse.Infrastructure.Http
│   └── Traverse.Infrastructure.Observability
└── [solution folder: services]
    ├── Traverse.Workflow.Api        (NEW — NT-005)
    ├── Traverse.KPI.Api             (NEW — NT-005)
    ├── Traverse.Admin.Api           (NEW — NT-005)
    ├── Traverse.Notifications.Api   (NEW — NT-005)
    ├── Traverse.Reporting.Api       (NEW — NT-005)
    ├── Traverse.AICopilot.Api       (NEW — NT-005)
    ├── Traverse.Compliance.Api      (NEW — NT-005)
    ├── Traverse.Search.Api          (NEW — NT-005)
    └── Traverse.Calendar.Api        (NEW — NT-005)
```

### Filesystem Layout

```
src/
├── shared/                               (unchanged — NT-002)
│   ├── Traverse.AI.Abstractions/
│   ├── Traverse.AI.Providers/
│   ├── Traverse.Domain.Primitives/
│   ├── Traverse.Infrastructure.Auth/
│   ├── Traverse.Infrastructure.Http/
│   ├── Traverse.Infrastructure.Messaging/
│   ├── Traverse.Infrastructure.Observability/
│   └── Traverse.Infrastructure.Persistence/
└── services/
    ├── workflow/
    │   └── Traverse.Workflow.Api/
    │       ├── Traverse.Workflow.Api.csproj
    │       ├── Program.cs
    │       └── appsettings.json
    ├── kpi/
    │   └── Traverse.KPI.Api/
    ├── admin/
    │   └── Traverse.Admin.Api/
    ├── notifications/
    │   └── Traverse.Notifications.Api/
    ├── reporting/
    │   └── Traverse.Reporting.Api/
    ├── aicopilot/
    │   └── Traverse.AICopilot.Api/
    ├── compliance/
    │   └── Traverse.Compliance.Api/
    ├── search/
    │   └── Traverse.Search.Api/
    └── calendar/
        └── Traverse.Calendar.Api/

docker/
├── workflow/
│   └── Dockerfile                        (NEW — NT-005)
├── kpi/
│   └── Dockerfile                        (NEW — NT-005)
├── admin/
│   └── Dockerfile                        (NEW — NT-005)
├── notifications/
│   └── Dockerfile                        (NEW — NT-005)
├── reporting/
│   └── Dockerfile                        (NEW — NT-005)
├── aicopilot/
│   └── Dockerfile                        (NEW — NT-005)
├── compliance/
│   └── Dockerfile                        (NEW — NT-005)
├── search/
│   └── Dockerfile                        (NEW — NT-005)
└── calendar/
    └── Dockerfile                        (NEW — NT-005)
```

### Dependency Graph

Each service API project depends on all six shared infrastructure libraries. The direction is inward — the API projects depend on shared libs; shared libs do not depend on API projects:

```
Traverse.{Module}.Api
    → Traverse.Domain.Primitives
    → Traverse.Infrastructure.Persistence
    → Traverse.Infrastructure.Messaging
    → Traverse.Infrastructure.Auth
    → Traverse.Infrastructure.Http
    → Traverse.Infrastructure.Observability
```

`Traverse.Infrastructure.Http` and `Traverse.Infrastructure.Persistence` transitively pull in `Traverse.Domain.Primitives`. The direct reference is still declared in each API project's `.csproj` for explicitness (per SOLID DIP: explicit dependencies).

### docker-compose.yml Relationship

The nine API service entries already exist in `docker-compose.yml` (produced by NT-004) with the `api` profile. NT-005 does not modify `docker-compose.yml` — it only creates the Dockerfiles that the existing entries reference. The docker-compose entries reference:

- `docker/workflow/Dockerfile` → created by NT-005
- `docker/kpi/Dockerfile` → created by NT-005
- `docker/admin/Dockerfile` → created by NT-005
- `docker/notifications/Dockerfile` → created by NT-005
- `docker/reporting/Dockerfile` → created by NT-005
- `docker/aicopilot/Dockerfile` → created by NT-005
- `docker/compliance/Dockerfile` → created by NT-005
- `docker/search/Dockerfile` → created by NT-005
- `docker/calendar/Dockerfile` → created by NT-005

---

## 5. Per-Project Specification

### 5.1 Substitution Table

All per-project variable values. Every template reference in §5.2–§5.5 uses these values.

| Variable | Workflow | KPI | Admin | Notifications | Reporting | AICopilot | Compliance | Search | Calendar |
|----------|----------|-----|-------|---------------|-----------|-----------|------------|--------|----------|
| `{ProjectName}` | `Traverse.Workflow.Api` | `Traverse.KPI.Api` | `Traverse.Admin.Api` | `Traverse.Notifications.Api` | `Traverse.Reporting.Api` | `Traverse.AICopilot.Api` | `Traverse.Compliance.Api` | `Traverse.Search.Api` | `Traverse.Calendar.Api` |
| `{Namespace}` | `Traverse.Workflow.Api` | `Traverse.KPI.Api` | `Traverse.Admin.Api` | `Traverse.Notifications.Api` | `Traverse.Reporting.Api` | `Traverse.AICopilot.Api` | `Traverse.Compliance.Api` | `Traverse.Search.Api` | `Traverse.Calendar.Api` |
| `{ServiceName}` | `workflow-service` | `kpi-service` | `admin-service` | `notifications-service` | `reporting-service` | `aicopilot-service` | `compliance-service` | `search-service` | `calendar-service` |
| `{DbName}` | `traverse_workflow` | `traverse_kpi` | `traverse_admin` | `traverse_notifications` | `traverse_reporting` | `traverse_aicopilot` | `traverse_compliance` | `traverse_search` | `traverse_calendar` |
| `{ServiceFolder}` | `workflow` | `kpi` | `admin` | `notifications` | `reporting` | `aicopilot` | `compliance` | `search` | `calendar` |
| `{SrcRelPath}` | `src/services/workflow/Traverse.Workflow.Api` | `src/services/kpi/Traverse.KPI.Api` | `src/services/admin/Traverse.Admin.Api` | `src/services/notifications/Traverse.Notifications.Api` | `src/services/reporting/Traverse.Reporting.Api` | `src/services/aicopilot/Traverse.AICopilot.Api` | `src/services/compliance/Traverse.Compliance.Api` | `src/services/search/Traverse.Search.Api` | `src/services/calendar/Traverse.Calendar.Api` |
| `{HostPort}` | `5001` | `5002` | `5003` | `5004` | `5005` | `5006` | `5007` | `5008` | `5009` |
| `{EnvPortVar}` | `WORKFLOW_PORT` | `KPI_PORT` | `ADMIN_PORT` | `NOTIFICATIONS_PORT` | `REPORTING_PORT` | `AICOPILOT_PORT` | `COMPLIANCE_PORT` | `SEARCH_PORT` | `CALENDAR_PORT` |

### 5.2 Project File Template

Create the file at `{SrcRelPath}/{ProjectName}.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <!--
      Microsoft.NET.Sdk.Web activates ASP.NET Core's WebApplication builder
      and implicitly includes the Microsoft.AspNetCore.App framework reference.
      DO NOT add <FrameworkReference Include="Microsoft.AspNetCore.App" /> — that
      would duplicate the framework reference and trigger NU1510 warnings.
    -->
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <!-- CS1591 = missing XML documentation comment. Suppressed for Phase 0 API
         stubs — controllers and route handlers are added by Phase 1 stories
         which are responsible for documenting their own endpoints. -->
    <NoWarn>CS1591</NoWarn>
    <LangVersion>latest</LangVersion>
    <RootNamespace>{Namespace}</RootNamespace>
    <AssemblyName>{ProjectName}</AssemblyName>
  </PropertyGroup>

  <ItemGroup>
    <!--
      All six shared infrastructure libraries are referenced unconditionally.
      Phase 0 is the wiring point — each Phase 1 story extends this project
      without needing to discover or add project references.

      Dependency graph (inward only — API projects depend on shared libs;
      shared libs never depend on API projects):

        {ProjectName}
          ├── Traverse.Domain.Primitives          (domain types, DomainException)
          ├── Traverse.Infrastructure.Auth         (JWT Bearer, ICurrentUser)
          ├── Traverse.Infrastructure.Http         (CorrelationIdMiddleware, GlobalExceptionHandler)
          ├── Traverse.Infrastructure.Messaging    (MassTransit/RabbitMQ, IEventBus)
          ├── Traverse.Infrastructure.Observability (Serilog, OpenTelemetry, health checks)
          └── Traverse.Infrastructure.Persistence  (TraverseDbContext base, AuditableEntity)

      Traverse.Infrastructure.Http and Traverse.Infrastructure.Persistence
      transitively include Traverse.Domain.Primitives; the direct reference
      here is kept for explicitness (DIP: explicit dependencies visible in csproj).
    -->
    <ProjectReference Include="..\..\..\shared\Traverse.Domain.Primitives\Traverse.Domain.Primitives.csproj" />
    <ProjectReference Include="..\..\..\shared\Traverse.Infrastructure.Auth\Traverse.Infrastructure.Auth.csproj" />
    <ProjectReference Include="..\..\..\shared\Traverse.Infrastructure.Http\Traverse.Infrastructure.Http.csproj" />
    <ProjectReference Include="..\..\..\shared\Traverse.Infrastructure.Messaging\Traverse.Infrastructure.Messaging.csproj" />
    <ProjectReference Include="..\..\..\shared\Traverse.Infrastructure.Observability\Traverse.Infrastructure.Observability.csproj" />
    <ProjectReference Include="..\..\..\shared\Traverse.Infrastructure.Persistence\Traverse.Infrastructure.Persistence.csproj" />
  </ItemGroup>

  <!--
    No additional NuGet packages are required at Phase 0.
    The six ProjectReferences above transitively supply all runtime dependencies:
      - Serilog.AspNetCore, Serilog.Formatting.Compact          (via Observability)
      - OpenTelemetry.*                                           (via Observability)
      - Microsoft.AspNetCore.Authentication.JwtBearer            (via Auth)
      - FluentValidation, Microsoft.Extensions.Http.Resilience   (via Http)
      - MassTransit, MassTransit.RabbitMQ                        (via Messaging)
      - Microsoft.EntityFrameworkCore, Npgsql.EntityFrameworkCore.PostgreSQL (via Persistence)

    Phase 1 stories add service-specific packages (e.g., AspNetCore.HealthChecks.Npgsql
    for the readiness check, MediatR for CQRS handlers) directly to this csproj.
  -->

</Project>
```

> **Path note:** The relative path `..\..\..\\shared\` navigates from
> `src/services/{service}/{ProjectName}/` up **three** levels to `src/`, then into `shared/`.
> (`..` → `src/services/{service}/` → `..` → `src/services/` → `..` → `src/` → `shared/`).
> Four levels up would overshoot to the repository root, producing a broken reference (MSB9008).
> This was confirmed by implementation (ADR-009 Decision 1). Verify the path resolves correctly
> by running `dotnet build` from the solution root.

### 5.3 Program.cs Template

Create the file at `{SrcRelPath}/Program.cs`:

```csharp
// ============================================================================
// {ProjectName} — Program.cs
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
builder.AddTraverseObservability("{ServiceName}");

// ---------------------------------------------------------------------------
// Health checks — register the "self" liveness check (tagged "live").
// Each Phase 1 story extends this by chaining additional checks tagged "ready"
// (e.g., .AddNpgsql(connectionString, tags: new[] { "ready" })).
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
// appsettings.json. Auth:Authority and Auth:Audience are null in Phase 0;
// JWT validation is not enforced until the Phase 1 Auth story wires the OIDC
// provider. Phase 0 health endpoints bypass auth via AllowAnonymous() in
// MapTraverseHealthChecks.
// ---------------------------------------------------------------------------
builder.Services.AddTraverseAuth(builder.Configuration, builder.Environment);

// ---------------------------------------------------------------------------
// Authorisation — required by app.UseAuthorization() below.
// No policies are defined in Phase 0; Phase 1 stories add resource-level
// policies (e.g., builder.Services.AddAuthorization(opts => { ... })).
// ---------------------------------------------------------------------------
builder.Services.AddAuthorization();

// ---------------------------------------------------------------------------
// Messaging — MassTransit/RabbitMQ with connection params from appsettings.
// Phase 0: no consumers are registered (configure: null). Phase 1 stories
// pass a configure delegate to register their consumers:
//   builder.Services.AddTraverseMessaging(builder.Configuration,
//       cfg => cfg.ReceiveEndpoint("my-queue", e => e.Consumer<MyConsumer>()));
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
```

### 5.4 appsettings.json Template

Create the file at `{SrcRelPath}/appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "Database": "Host=localhost;Port=5432;Database={DbName};Username=traverse;Password=traverse_dev"
  },
  "RabbitMq": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "traverse",
    "Password": "traverse_dev",
    "VirtualHost": "/"
  },
  "Auth": {
    "Authority": "",
    "Audience": "traverse-api"
  },
  "Otel": {
    "Endpoint": ""
  }
}
```

> **Key notes:**
> - `ConnectionStrings:Database` uses `localhost` for development runs outside Docker. When running inside Docker Compose the environment variable `ConnectionStrings__Database` (double-underscore = colon) overrides this with `Host=postgres;...` (set by docker-compose.yml).
> - `Auth:Authority` is intentionally empty in Phase 0. `AddTraverseAuth` registers JWT Bearer but does not enforce validation until a real OIDC provider is wired.
> - `Otel:Endpoint` is intentionally empty in Phase 0. `AddTraverseObservability` silently skips the OTLP exporter when this key is absent or empty.
> - `RabbitMq:Host` = `localhost` for dev; overridden to `rabbitmq` (Docker DNS name) by `RabbitMq__Host` environment variable in docker-compose.yml.

### 5.5 Dockerfile Template

Create the file at `docker/{ServiceFolder}/Dockerfile`:

```dockerfile
# ============================================================================
# {ProjectName} — Multi-stage Dockerfile
#
# Stage layout (per .NET Coding Standards §18):
#   restore  : copy only .csproj files and restore NuGet packages
#              (layer-cache friendly — changes to source code do not
#               invalidate the restore layer)
#   build    : copy full source; dotnet build in Release
#   publish  : dotnet publish in Release; output to /app/publish
#   runtime  : aspnet runtime image only; no SDK; non-root user; minimal image
#
# Build context: repository root (.)
# All COPY paths are relative to the repository root.
#
# Usage:
#   docker build -f docker/{ServiceFolder}/Dockerfile .
#   docker compose --profile api up --build
# ============================================================================

# ---------------------------------------------------------------------------
# Stage 1: restore
# Copy only project files first. NuGet restore is the slowest step — keeping
# it in a dedicated layer means Docker reuses it on every source-only change.
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /src

# Copy solution file and global.json — needed by dotnet restore to resolve
# all project references in the solution graph.
COPY global.json ./
COPY Traverse.sln ./

# Copy shared library .csproj files (dependencies of the API project)
COPY src/shared/Traverse.Domain.Primitives/Traverse.Domain.Primitives.csproj \
     src/shared/Traverse.Domain.Primitives/
COPY src/shared/Traverse.Infrastructure.Auth/Traverse.Infrastructure.Auth.csproj \
     src/shared/Traverse.Infrastructure.Auth/
COPY src/shared/Traverse.Infrastructure.Http/Traverse.Infrastructure.Http.csproj \
     src/shared/Traverse.Infrastructure.Http/
COPY src/shared/Traverse.Infrastructure.Messaging/Traverse.Infrastructure.Messaging.csproj \
     src/shared/Traverse.Infrastructure.Messaging/
COPY src/shared/Traverse.Infrastructure.Observability/Traverse.Infrastructure.Observability.csproj \
     src/shared/Traverse.Infrastructure.Observability/
COPY src/shared/Traverse.Infrastructure.Persistence/Traverse.Infrastructure.Persistence.csproj \
     src/shared/Traverse.Infrastructure.Persistence/

# Copy the API project file
COPY {SrcRelPath}/{ProjectName}.csproj \
     {SrcRelPath}/

# Restore only this project and its transitive dependencies. The --project
# flag scopes the restore to avoid pulling packages for unrelated projects.
RUN dotnet restore {SrcRelPath}/{ProjectName}.csproj

# ---------------------------------------------------------------------------
# Stage 2: build
# Copy all source files and compile in Release configuration.
# --no-restore reuses the packages downloaded in stage 1.
# ---------------------------------------------------------------------------
FROM restore AS build
WORKDIR /src

# Copy shared library source
COPY src/shared/ src/shared/

# Copy service project source
COPY {SrcRelPath}/ {SrcRelPath}/

RUN dotnet build {SrcRelPath}/{ProjectName}.csproj \
    -c Release \
    --no-restore

# ---------------------------------------------------------------------------
# Stage 3: publish
# Produce self-contained publish output. /p:UseAppHost=false suppresses the
# native executable wrapper — not needed inside a Linux container.
# ---------------------------------------------------------------------------
FROM build AS publish

RUN dotnet publish {SrcRelPath}/{ProjectName}.csproj \
    -c Release \
    --no-build \
    /p:UseAppHost=false \
    -o /app/publish

# ---------------------------------------------------------------------------
# Stage 4: runtime
# Final image: aspnet runtime only (no SDK). Approximately 200MB vs 800MB+ SDK.
# Non-root user for security (CIS Benchmark for containers).
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create a non-root user and group; chown the working directory.
# RUN adduser is preferred over useradd for Alpine-compatible images.
RUN adduser --disabled-password --gecos "" appuser \
    && chown -R appuser /app
USER appuser

# Copy published output from the publish stage.
COPY --from=publish /app/publish .

# Internal port the service listens on (overridden by ASPNETCORE_URLS in compose).
EXPOSE 8080

# ASPNETCORE_URLS tells Kestrel which address to bind to inside the container.
# The docker-compose.yml environment section sets this to http://+:8080.
ENV ASPNETCORE_URLS=http://+:8080

# Use the shell form to allow signal forwarding (SIGTERM for graceful shutdown).
ENTRYPOINT ["dotnet", "{ProjectName}.dll"]
```

### 5.6 Solution Registration

Each project must be added to `Traverse.sln` under the existing `services` solution folder. The solution folder GUID for `services` is `{5968FBED-DF69-4CE6-8CAF-364A27DD447A}` (from the NT-001 solution file).

Run the following command for each project (substitute `{SrcRelPath}` and `{ProjectName}` from the table in §5.1):

```bash
dotnet sln Traverse.sln add --solution-folder services \
    {SrcRelPath}/{ProjectName}.csproj
```

> **Important:** Run `dotnet sln add` from the repository root so that the relative path is recorded correctly in the `.sln` file. Never manually edit the `.sln` file's GUID entries — `dotnet sln add` generates them correctly.

After adding all 9 projects, verify:
```bash
dotnet sln Traverse.sln list
```
Expected output: all 17 projects listed (8 existing shared + 9 new API stubs).

---

## 6. Data Model

**N/A — Phase 0 API stubs introduce no database schema.**

No EF Core migrations are created. No `DbContext` subclasses are defined. The `Traverse.Infrastructure.Persistence` project is referenced (so Phase 1 stories can inherit from `TraverseDbContext` without adding a project reference), but no entity mappings are registered in Phase 0.

The `appsettings.json` connection string is a placeholder. The Docker Compose environment variable `ConnectionStrings__Database` overrides it at runtime. The PostgreSQL databases (`traverse_workflow`, `traverse_kpi`, etc.) were created empty by `docker/postgres/init.sql` in NT-004.

---

## 7. API Surface

Each of the 9 API stubs exposes exactly two endpoints. No other routes are mapped.

### GET /health/live

| Attribute | Value |
|-----------|-------|
| Method | `GET` |
| Path | `/health/live` |
| Auth | None (AllowAnonymous) |
| Tags | Health check tagged `"live"` |
| Success response | `200 OK` — body: `{"status":"Healthy"}` |
| Failure response | `503 Service Unavailable` — if the liveness check fails |
| Purpose | Kubernetes liveness probe; Docker Compose healthcheck |
| Registered by | `app.MapTraverseHealthChecks()` in `Program.cs` via `WebApplicationExtensions.MapTraverseHealthChecks` |

The liveness check runs only the `"self"` check (registered by `AddTraverseHealthChecks()`). It always returns `Healthy` as long as the process is alive. It is intentionally isolated from database or broker connectivity — those are readiness concerns.

### GET /health/ready

| Attribute | Value |
|-----------|-------|
| Method | `GET` |
| Path | `/health/ready` |
| Auth | None (AllowAnonymous) |
| Tags | Health checks tagged `"ready"` |
| Success response | `200 OK` — body: `{"status":"Healthy","entries":{"masstransit-bus":{"status":"Healthy",...}}}` (when RabbitMQ is reachable) |
| Failure response | `503 Service Unavailable` — body includes `"masstransit-bus"` entry with `"Unhealthy"` status (when RabbitMQ is unreachable) |
| Purpose | Kubernetes readiness probe |
| Registered by | `app.MapTraverseHealthChecks()` in `Program.cs` |

In Phase 0, MassTransit 8.3.0 automatically registers a `"masstransit-bus"` health check tagged `["ready","masstransit"]` when `AddMassTransit().UsingRabbitMq()` is called (via `AddTraverseMessaging()`). This means the readiness endpoint is **not** empty in Phase 0 — it tests broker connectivity. When RabbitMQ is available (e.g., via `docker compose --profile api up`), the endpoint returns HTTP 200 Healthy. When RabbitMQ is unreachable (e.g., running the API locally without Docker), it returns HTTP 503 Unhealthy with the masstransit-bus entry marked Unhealthy.

This is architecturally correct readiness behaviour: a service is not ready to accept traffic if its message broker is down. Phase 1 stories add additional `"ready"`-tagged checks (`.AddNpgsql(...)`) on top of the automatically registered MassTransit check.

> **Integration test finding CG-001:** This behaviour was confirmed by integration testing (step_8). The design initially stated "empty entries object" — this was incorrect and has been corrected here.

### GET /openapi/v1.json (development only)

| Attribute | Value |
|-----------|-------|
| Method | `GET` |
| Path | `/openapi/v1.json` |
| Auth | None |
| Availability | Development environment only (`ASPNETCORE_ENVIRONMENT=Development`) |
| Purpose | OpenAPI 3.0 document for Swagger UI / API exploration |
| Registered by | `app.MapOpenApi()` inside `if (app.Environment.IsDevelopment())` |

In Phase 0, the document describes no endpoints beyond the health checks.

---

## 8. Business Logic

**N/A — Phase 0 API stubs contain no business logic.**

No CQRS handlers, no MediatR commands or queries, no domain aggregates, no repository implementations, no event consumers, and no service classes are created. The sole purpose of each `Program.cs` is infrastructure wiring.

---

## 9. Edge Cases

### EC-1 — SDK Version Consistency

**Risk:** A developer's machine may have a different .NET SDK installed. The Web SDK (`Microsoft.NET.Sdk.Web`) is sensitive to the SDK version for implicit package resolution.

**Resolution:** `global.json` in the repository root pins the SDK to `10.0.103` with `rollForward: latestFeature`. This is enforced by the .NET SDK itself — running any `dotnet` command from within the repository root directory resolves to `10.0.103`. No action required in NT-005; this was established in NT-001.

**Verification:** Before creating any project, confirm: `dotnet --version` from the repository root returns `10.0.103` (or a compatible feature-band version if the exact patch is not installed and `latestFeature` rolls forward). If the output is a different major/minor version, the developer must install .NET SDK 10.0.103.

---

### EC-2 — Port Conflicts on Developer Machines

**Risk:** Ports 5001–5009 may be occupied by another process on the developer's machine, causing Docker Compose to fail with `address already in use`.

**Resolution:** The `docker-compose.yml` (NT-004) uses environment variable overrides with defaults: `${WORKFLOW_PORT:-5001}:8080`. Developers who experience a conflict copy `.env.example` to `.env` and override the conflicting port variable. The API projects themselves always listen on `8080` inside the container — only the host-side mapping changes.

**No action in NT-005:** This was handled by NT-004's `.env.example` and docker-compose environment variable pattern.

---

### EC-3 — Service Naming Convention Alignment

**Risk:** The service name strings used for Serilog enrichment, OTel resource attributes, and Docker container names must be consistent. A mismatch (e.g., `workflow_service` vs `workflow-service`) makes log filtering inconsistent across the platform.

**Resolution:** All service names use kebab-case per the Serilog enrichment convention from NT-002 (`ObservabilityServiceCollectionExtensions` uses a `serviceName` parameter). The `{ServiceName}` values in §5.1 are authoritative:

| Project | serviceName | Container name |
|---------|------------|----------------|
| `Traverse.Workflow.Api` | `workflow-service` | `traverse-workflow` |
| `Traverse.KPI.Api` | `kpi-service` | `traverse-kpi` |
| `Traverse.Admin.Api` | `admin-service` | `traverse-admin` |
| `Traverse.Notifications.Api` | `notifications-service` | `traverse-notifications` |
| `Traverse.Reporting.Api` | `reporting-service` | `traverse-reporting` |
| `Traverse.AICopilot.Api` | `aicopilot-service` | `traverse-aicopilot` |
| `Traverse.Compliance.Api` | `compliance-service` | `traverse-compliance` |
| `Traverse.Search.Api` | `search-service` | `traverse-search` |
| `Traverse.Calendar.Api` | `calendar-service` | `traverse-calendar` |

The container names are already set in docker-compose.yml (NT-004) and must not change.

---

### EC-4 — NU1510 / Duplicate Framework Reference

**Risk:** Adding `<FrameworkReference Include="Microsoft.AspNetCore.App" />` to a project using `Microsoft.NET.Sdk.Web` causes `NU1510` warning which is fatal under `TreatWarningsAsErrors`.

**Resolution:** The `.csproj` template in §5.2 uses `Microsoft.NET.Sdk.Web` and explicitly does **not** include any `<FrameworkReference>` element. The Web SDK provides the framework reference automatically. This is documented as a comment in the `.csproj` template.

---

### EC-5 — `Traverse.AI.Abstractions` and `Traverse.AI.Providers` Not Referenced

**Risk:** A developer may wonder why the two pre-existing AI shared projects are not referenced from the API stubs.

**Resolution:** This is by design. The AI projects are referenced only by `Traverse.AICopilot.Api` — and even that reference is added in Phase 1 (the AI Copilot feature story), not Phase 0. Referencing unused projects adds transitive NuGet dependencies (OpenAI SDK, Anthropic SDK, AWS Bedrock) to all 9 services, inflating startup time and bundle size without benefit. The clean-architecture principle of explicit, minimal dependencies applies.

---

### EC-6 — `dotnet restore` Before First Build

**Risk:** The `dotnet build Traverse.sln` AC-1 verification will fail if NuGet packages have not been restored. In CI or on a fresh clone, packages must be restored before building.

**Resolution:** Run `dotnet restore Traverse.sln` before `dotnet build Traverse.sln`. The `Dockerfile` handles this automatically in the `restore` stage (Stage 1). For local builds, developers typically run `dotnet build` which triggers a restore implicitly — this is sufficient.

---

## 10. Out of Scope

The following items are explicitly excluded from NT-005. Inclusion of any of these constitutes scope creep and must not be attempted:

| Item | Deferred To |
|------|------------|
| Domain logic, CQRS handlers, MediatR registrations | Phase 1 feature story for each module |
| EF Core `DbContext` subclass per service | Phase 1 — when the service owns its first entity |
| EF Core migrations (`dotnet ef migrations add InitialCreate`) | Phase 1 — after the service's `DbContext` is defined |
| Database readiness check (`.AddNpgsql(...)` tagged `"ready"`) | Phase 1 — requires a real connection string and schema |
| RabbitMQ readiness check (`.AddRabbitMQ(...)` tagged `"ready"`) | Phase 1 — requires consumers to be registered |
| JWT Bearer validation (real OIDC provider authority) | Phase 1 Auth story |
| Controller classes or route groups | Phase 1 — no business routes in Phase 0 |
| FluentValidation validators | Phase 1 — requires request types to validate |
| MassTransit consumers | Phase 1 — no message flows in Phase 0 |
| Integration tests | Phase 1 — nothing to test at Phase 0 |
| Kubernetes manifests or Helm charts | Post-Phase 0 infrastructure story |
| `appsettings.Development.json` overrides | Phase 1 — Phase 0 defaults are sufficient |
| Swagger UI HTML endpoint (`/swagger`) | Phase 1+ — `MapOpenApi()` provides the JSON spec only |
| `Traverse.AI.Abstractions` / `Traverse.AI.Providers` references on non-AI stubs | Phase 1 AI Copilot story only |

---

## 11. Effort Estimate (CU-Derived)

> **Recalculated at design completion** — matches the architecture-level estimate
> (design complexity is fully resolved: 9 identical template applications, 0 domain logic).

| Input | Value | Notes |
|-------|-------|-------|
| New components | 9 × 1.0 = 9.0 CU | One Web API project per service |
| Integration boundaries | 2 × 1.5 = 3.0 CU | PostgreSQL connection string placeholder; RabbitMQ connection string placeholder |
| Acceptance criteria | 4 × 0.5 = 2.0 CU | AC-1 (sln), AC-2 (health), AC-3 (Dockerfile), AC-4 (compose) |
| Data changes | 0 × 1.0 = 0.0 CU | No database schema; no EF migrations |
| NFR level | Low = 1.0 CU | `dotnet build` succeeds; health endpoints respond |
| **Base CU** | **15.0** | |
| Calibration factor | 1.4 | Default — 0 actuals logged for northwoods-traverse |
| Risk multiplier | 1.0 | Low — repetitive scaffolding with a clear template |
| Project weight | 2.5 | northwoods-traverse |
| **Adjusted CU** | **52.5** | 15.0 × 1.4 × 1.0 × 2.5 |

**Hours estimate (at 2.0 hrs/CU):** 105.0 hours
**Confidence (design gate, ±30–50%):** 52.5 – 157.5 hours
**Architecture-level estimate was:** 52.5 Adjusted CU (from `architecture_phase0.md` §10 — NT-005)

---

## Approval Record

> **AutoMode: true** — All roles are "self" (single operator). Requirements are fully specified. This is a pure scaffolding task with zero architectural ambiguity.

### Q1 — Dev Lead + PM Final Approval

**Role:** Acting as: PM + DevLead (same actor) — apply all lenses simultaneously

**Answer:** Approved. The design document is complete and clear enough for a junior developer to implement all 9 projects without asking questions. Socratic self-check:

- Every project has explicit NuGet package information (none at the project level — all come via ProjectReferences; transitive packages documented in §5.2 comments).
- Every `Program.cs` is fully specified (§5.3 — not "similar to previous").
- Every `Dockerfile` follows the 4-stage pattern from architecture_phase0.md §8 (§5.5).
- Every docker-compose service entry is already in place from NT-004 (§4 confirms NT-005 creates only Dockerfiles, not compose entries).
- All 4 ACs are verifiable from the document alone (§2 — each AC has an explicit verification command).
- Scope is tight: §10 Out of Scope explicitly excludes all Phase 1+ items.

**Date:** 2026-05-04

---

<!-- Generated by skill: design-feature v2.4.0 | 2026-05-04 09:00 -->
