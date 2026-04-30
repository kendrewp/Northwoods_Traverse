# Architecture: Phase 0 — Scaffolding (NT-000)

> **Feature:** NT-000 — Phase 0 Scaffolding
> **Author:** Kendrew Peacey (Dev Lead / PM / Stakeholder — single operator)
> **Status:** Approved
> **Date:** 2026-04-26
> **Skill:** design-architecture v1.5.0

---

## Table of Contents

1. [Solution Overview](#1-solution-overview)
2. [Architecture Decisions](#2-architecture-decisions)
3. [Data Model](#3-data-model)
4. [API Surface](#4-api-surface)
5. [Component Boundaries](#5-component-boundaries)
6. [Cross-Story Dependencies](#6-cross-story-dependencies)
7. [Integration Points](#7-integration-points)
8. [Technology Decisions](#8-technology-decisions)
9. [Risk Assessment](#9-risk-assessment)
10. [Story-Level Estimates](#10-story-level-estimates)

---

## 1. Solution Overview

Phase 0 is pure infrastructure scaffolding — it creates the compilable, runnable skeleton that every Phase 1+ feature story builds on. No business logic is introduced. The deliverable is a monorepo containing:

- A .NET 10 solution (`Traverse.sln`) with eleven class library projects (six shared infrastructure libs plus the two pre-existing AI projects) and nine empty ASP.NET Core Web API stubs.
- An Angular workspace (`traverse-workspace`) with Material 3 theming, an app shell, persona navigation, auth stubs, and shared presentational components.
- A Docker Compose environment that brings up PostgreSQL, RabbitMQ, and n8n locally with a single `docker compose up`.

From the user's perspective, nothing is visible yet — Phase 0 produces the working foundation so that Phase 1 stories (Workflow, KPI, Admin, etc.) can be implemented without first fighting the toolchain. The two existing AI projects (`Traverse.AI.Abstractions`, `Traverse.AI.Providers`) are adopted into the solution as-is; their patterns (file-scoped namespaces, `TreatWarningsAsErrors`, primary constructors, nullable reference types) establish the conventions all new libraries follow.

The technical approach is deliberately additive-only: create structure, wire configuration, verify compilation. No placeholder business logic, no stub controllers with fake data, no TODO-driven hacks that need ripping out later.

---

## 2. Architecture Decisions

### Decision 1: Clean Architecture layering for shared libraries

**Decision:** The six shared backend libraries are organised by Clean Architecture layer: Domain (primitives), then Infrastructure sub-layers (Persistence, Messaging, Auth, Http, Observability). Domain has zero infrastructure dependencies; each infrastructure library depends only on Domain and relevant NuGet packages.

**Rationale:** Enforces the Dependency Inversion Principle (DIP) — higher-level domain types never import lower-level infrastructure concerns. Aligns with the .NET Coding Standards document and the pattern established in `Traverse.AI.Abstractions` (zero provider dependencies). Prevents the common antipattern of business types gaining transitive EF Core or MassTransit dependencies.

**Trade-offs:** Slightly more project references to manage. Gained: testability (domain tests have no infrastructure dependencies), clear ownership boundaries per future team.

**Alternatives considered:** A single `Traverse.Infrastructure` monolith project. Rejected because it forces all consumers to pull in all infrastructure NuGet packages regardless of need, which bloats service startup and creates hidden coupling.

---

### Decision 2: One ASP.NET Core Web API project per PRD module

**Decision:** Nine empty `{Module}.Api` projects are created, one per PRD module, each with its own `Program.cs`, `appsettings.json`, and `Dockerfile`. Port assignments are fixed at 5001–5009.

**Rationale:** Establishes the microservice boundary at scaffolding time so Phase 1 stories slot directly into the correct project without structural rework. Port assignments give the Angular proxy, Docker Compose service entries, and health checks a stable contract from Day 1. Follows the "one solution per microservice" principle from the .NET Coding Standards (adapted here to "one project per bounded context" within a single solution during early development).

**Trade-offs:** Nine project files to maintain. Gained: zero rework when Phase 1 begins; each story knows exactly which project it extends.

**Alternatives considered:** A single "Traverse.Api" monolith that routes to feature modules by convention. Rejected because it defeats the entire microservice decomposition intent of the PRD and would require a painful split later.

---

### Decision 3: Angular standalone components from the start, no NgModules

**Decision:** The Angular workspace is created with standalone components, pipes, and directives as the default. No `AppModule` is created. `app.config.ts` uses `bootstrapApplication`.

**Rationale:** Angular Coding Standards §1 mandates "Standalone-first: All new code uses standalone components." Starting with the module-based API and migrating later is far more disruptive than starting correctly. The `bootstrapApplication` path also enables more granular lazy loading.

**Trade-offs:** Standalone requires more explicit import declarations per component. Gained: smaller bundle sizes via better tree-shaking, cleaner component isolation, no circular module dependency risk.

**Alternatives considered:** NgModule-based app bootstrapped with `ng new` defaults. Rejected — the standards document explicitly prohibits this for new code.

---

### Decision 4: Material 3 design tokens, not M2 theme mixin

**Decision:** The Angular theme uses `mat.define-theme()` with M3 palettes and CSS custom properties (`--mat-sys-*`). The Traverse primary colour `#1e4d8c` is mapped to a custom M3 palette.

**Rationale:** Angular Material 3 is the current version; M2 is deprecated. M3 tokens guarantee accessible paired contrast ratios automatically. Starting on M3 now avoids a forced migration later.

**Trade-offs:** M3 palette generation requires using `mat.$azure-palette` as a seed or a custom Sass function — slightly more initial setup than M2's `mat.define-light-theme`. Gained: future-proof theming, built-in accessibility, token-based overrides that work across the entire component library.

**Alternatives considered:** Angular Material 2 theming. Rejected — deprecated and will be removed in a future Angular version.

---

### Decision 5: Single `docker-compose.yml` for all infrastructure, separate Dockerfiles per service

**Decision:** `docker-compose.yml` at repo root covers shared infrastructure (PostgreSQL, RabbitMQ, n8n) plus all nine API service containers. Each service has its own `Dockerfile` in `docker/{service}/`.

**Rationale:** A single compose file makes `docker compose up` the only command a developer needs to start the entire local environment. Per-service Dockerfiles follow the multi-stage pattern from the .NET Coding Standards §18, keeping production images small (aspnet runtime only) and layer-cache-friendly.

**Trade-offs:** The compose file grows with each new service. Gained: single command for the full stack, explicit per-service image control, alignment with Kubernetes deployment model (each service independently deployable).

**Alternatives considered:** Separate compose files per sub-group (infra vs. services). Rejected for Phase 0 — the added orchestration complexity has no benefit at this scale.

---

### Decision 6: Outbox pattern base in `Traverse.Infrastructure.Messaging` (skeleton only)

**Decision:** The outbox `OutboxMessage` entity and `OutboxProcessor` background service are scaffolded as abstract bases in `Traverse.Infrastructure.Messaging`. No concrete messaging flows are wired — those belong to Phase 1 stories.

**Rationale:** The outbox pattern is mandatory for at-least-once delivery guarantees across all PRD modules (per .NET Coding Standards §7). Establishing the base class in shared infrastructure means Phase 1 stories compose rather than independently invent their outbox implementations. Single Responsibility and Open/Closed principles — the base handles timing and retry; subclasses handle message type resolution.

**Trade-offs:** Slightly increases scope of NT-002. Gained: zero duplication across nine services; consistent reliability guarantees from the first Phase 1 story.

**Alternatives considered:** Defer outbox to Phase 1. Rejected because the first Phase 1 story that needs async messaging would need to define the outbox model in a service-specific project, breaking the shared infrastructure design intent.

---

## 3. Data Model

Phase 0 introduces no service-specific business schema. All database creation is deferred to Phase 1+ stories that own their respective service schemas. Phase 0 does establish the shared data infrastructure types that Phase 1 will build on.

### Shared Entity: `AuditableEntity` (base class — `Traverse.Infrastructure.Persistence`)

Used as the base for all EF Core entities in every service.

```
AuditableEntity (abstract base class, not a table — each service maps its own entities)
- CreatedAt    (DateTimeOffset, NOT NULL, default: UtcNow)
- CreatedBy    (string, NOT NULL, max 256)
- ModifiedAt   (DateTimeOffset, NULL — null until first update)
- ModifiedBy   (string, NULL — null until first update)
```

Relationships: inherited by all service entities (1-to-1 composition pattern).

---

### Shared Entity: `OutboxMessage` (table per service — `Traverse.Infrastructure.Messaging`)

Each service will create its own `OutboxMessages` table via EF Core migration in Phase 1. The entity definition lives in the shared library.

```
OutboxMessage
- Id              (Guid, PRIMARY KEY, default: Guid.NewGuid())
- MessageType     (string NOT NULL, max 512) — fully qualified CLR type name
- Content         (string NOT NULL) — JSON-serialised message payload
- CreatedOnUtc    (DateTime NOT NULL, default: UtcNow)
- PublishedOnUtc  (DateTime NULL) — null until background processor publishes
```

Relationships: no FK; standalone log table.

---

### Shared Entity: `AuditLog` (table per service — `Traverse.Infrastructure.Persistence`)

```
AuditLog
- Id           (Guid, PRIMARY KEY, default: Guid.NewGuid())
- EntityType   (string NOT NULL, max 256)
- EntityId     (string NOT NULL, max 128) — string to accommodate all ID types
- Action       (string NOT NULL, max 50) — e.g. "Created", "Updated", "Deleted"
- ChangedBy    (string NOT NULL, max 256)
- ChangedAt    (DateTimeOffset NOT NULL, default: UtcNow)
- OldValues    (string NULL) — JSON snapshot
- NewValues    (string NULL) — JSON snapshot
```

---

### Domain Primitives (value types — `Traverse.Domain.Primitives`)

These are records/structs, not EF Core entities. They are used as strongly-typed IDs and value objects by service entities.

| Type | Kind | Notes |
|---|---|---|
| `ClientId` | record struct (Guid) | Strongly-typed identity |
| `CaseId` | record struct (Guid) | Strongly-typed identity |
| `WorkItemId` | record struct (Guid) | Strongly-typed identity |
| `Address` | record | Value object: Street, City, Province, PostalCode, Country |
| `Money` | record | Value object: Amount (decimal), CurrencyCode (string, default "CAD") |
| `Client` | abstract class | AggregateRoot base — ClientId, Name |
| `Case` | abstract class | AggregateRoot base — CaseId, ClientId, CaseType, Status |
| `WorkItem` | abstract class | AggregateRoot base — WorkItemId, CaseId, AssignedTo |
| `User` | abstract class | AggregateRoot base — UserId (Guid), Email, Roles |
| `Program` | abstract class | AggregateRoot base — ProgramId (Guid), Name, CaseType |
| `CaseType` | enum | Defined per service; base type here |

---

## 4. API Surface

Phase 0 introduces no business endpoints. Every API stub exposes only:

### Health Check Endpoints (all 9 services)

**Endpoint: GET /health/live**
- Request: none
- Response (200): `{ "status": "Healthy" }` — or HealthReport JSON if detailed output configured
- Response (503): if liveness check fails
- Purpose: Kubernetes liveness probe

**Endpoint: GET /health/ready**
- Request: none
- Response (200): HealthReport JSON including database and messaging readiness
- Response (503): if database or RabbitMQ not reachable
- Purpose: Kubernetes readiness probe; Docker Compose healthcheck

**Endpoint: GET /openapi/v1.json** (Scalar / Swagger UI)
- Request: none
- Response (200): OpenAPI 3.0 JSON document for the service
- Present in development profile only (`builder.Environment.IsDevelopment()`)

No business routes are registered in Phase 0. Each `Program.cs` maps only the health check and OpenAPI endpoints. Business route groups are added by Phase 1 stories.

---

## 5. Component Boundaries

### Backend — Shared Libraries

**Component: `Traverse.Domain.Primitives`**
- Responsibility: Define domain-agnostic base types, strongly-typed IDs, and value objects that all service aggregates inherit from or compose.
- Layer: Domain
- Calls: nothing (zero dependencies — pure C# records and abstract classes)
- Called by: All service projects; `Traverse.Infrastructure.Persistence`; `Traverse.Infrastructure.Messaging`

---

**Component: `Traverse.Infrastructure.Persistence`**
- Responsibility: Provide the `ApplicationDbContext` base class, `AuditableEntity`, `AuditLog` entity, `IAuditLogService` interface, `IUnitOfWork` interface, and EF Core value converters for strongly-typed IDs.
- Layer: Infrastructure
- Calls: `Traverse.Domain.Primitives` (entity base types); EF Core NuGet packages
- Called by: All nine service API projects (each registers its own DbContext derived from `ApplicationDbContext`)

---

**Component: `Traverse.Infrastructure.Messaging`**
- Responsibility: Bootstrap MassTransit + RabbitMQ via an `AddTraverseMessaging` extension method; provide `IEventBus` abstraction; provide `OutboxMessage` entity and `OutboxProcessor` background service base.
- Layer: Infrastructure
- Calls: `Traverse.Domain.Primitives`; MassTransit NuGet packages
- Called by: All nine service API projects (messaging opt-in via `AddTraverseMessaging`)

---

**Component: `Traverse.Infrastructure.Auth`**
- Responsibility: Configure JWT bearer authentication via `AddTraverseAuth` extension; expose `ICurrentUser` interface; provide `HasRoleAttribute` for controller/endpoint authorisation.
- Layer: Infrastructure
- Calls: ASP.NET Core authentication NuGet packages
- Called by: All nine service API projects; Angular `authInterceptor` (HTTP header convention)

---

**Component: `Traverse.Infrastructure.Http`**
- Responsibility: Provide `CorrelationIdMiddleware`, `GlobalExceptionHandler` (ProblemDetails mapping), `correlationIdInterceptor` (conceptual — C# side middleware), and `errorInterceptor` extension.
- Layer: Infrastructure
- Calls: ASP.NET Core middleware pipeline; `Traverse.Domain.Primitives` (for domain exception types)
- Called by: All nine service API projects (wired in `Program.cs` via `UseTraverseHttp`)

---

**Component: `Traverse.Infrastructure.Observability`**
- Responsibility: Bootstrap Serilog structured logging and OpenTelemetry tracing + metrics via `AddTraverseObservability`; provide health check extension `AddTraverseHealthChecks`.
- Layer: Infrastructure
- Calls: Serilog NuGet packages; OpenTelemetry SDK; ASP.NET Core health checks
- Called by: All nine service API projects

---

### Backend — Service API Stubs

**Component: Each of the 9 `Traverse.{Module}.Api` projects**
- Responsibility: Host the ASP.NET Core web application for one bounded context. Wires all shared infrastructure libraries. Defines no business routes in Phase 0.
- Layer: Presentation
- Calls: All six shared infrastructure libraries (via DI extension methods in `Program.cs`)
- Called by: Angular frontend (via proxy); Docker Compose health checks; future Phase 1 story implementations

Port map:

| Service | Port |
|---|---|
| `Traverse.Workflow.Api` | 5001 |
| `Traverse.KPI.Api` | 5002 |
| `Traverse.Admin.Api` | 5003 |
| `Traverse.Notifications.Api` | 5004 |
| `Traverse.Reporting.Api` | 5005 |
| `Traverse.AICopilot.Api` | 5006 |
| `Traverse.Compliance.Api` | 5007 |
| `Traverse.Search.Api` | 5008 |
| `Traverse.Calendar.Api` | 5009 |

---

### Frontend — Angular Shell

**Component: `AppShellComponent`** (smart container — `app/core/shell/`)
- Responsibility: Render the top navigation bar and left sidebar (220px). Apply responsive breakpoints. Host the `<router-outlet>`.
- Layer: Presentation
- Calls: `AuthService` (to determine nav item visibility by role)
- Called by: Angular router (loaded as the layout component for the authenticated root route; `bootstrapApplication` boots `AppComponent`, which renders `<router-outlet>` that activates `AppShellComponent`)

---

**Component: `AuthService`** (`app/core/services/auth.service.ts`)
- Responsibility: Provide `isAuthenticated()`, `hasRole(role)`, and `getToken()` as stubs. Returns hardcoded dev values in Phase 0; replaced by real JWT logic in Phase 1 Auth story.
- Layer: Application (injectable singleton)
- Calls: nothing (stub returns static values)
- Called by: `authGuard`, `adminGuard`, `AppShellComponent`

---

**Component: `authGuard` / `adminGuard`** (`app/core/guards/`)
- Responsibility: Functional route guards. `authGuard` checks `AuthService.isAuthenticated()`. `adminGuard` additionally checks `hasRole('admin')`. Redirect to `/login` if not satisfied.
- Layer: Application
- Calls: `AuthService`; `Router`
- Called by: Route definitions in `app.routes.ts`

---

**Component: `GlobalErrorHandler`** (`app/core/handlers/`)
- Responsibility: Implement Angular `ErrorHandler`. Catch unhandled errors, map `ApiError` (from ProblemDetails) to a user-visible `MatSnackBar` message.
- Layer: Presentation (Angular DI root)
- Calls: `MatSnackBar`; `Router` (for 401 redirect)
- Called by: Angular's error pipeline; registered in `app.config.ts`

---

**Component: Shared Presentational Components** (`app/shared/components/`)
- `PageHeaderComponent` — displays page title and optional subtitle. No logic.
- `ErrorDisplayComponent` — renders an error message card. Accepts `error` input.
- `LoadingSpinnerComponent` — centred Material spinner overlay. Accepts `isLoading` input.
- `KpiStatusBadgeComponent` — coloured status chip using M3 paired tokens. Accepts `status` input.
- Responsibility: Each component has a single display responsibility, accepts typed `input()`, emits nothing (display-only).
- Layer: Presentation (shared)
- Calls: Angular Material modules only
- Called by: Feature page components in Phase 1+

---

### Infrastructure

**Component: `docker-compose.yml`**
- Responsibility: Declare all local dev services — PostgreSQL, RabbitMQ (management plugin), n8n, and all nine ASP.NET Core API containers. Provide health checks, port mappings, and volume mounts.
- Layer: Infrastructure / DevOps
- Calls: Each service's `Dockerfile`
- Called by: Developer (`docker compose up`)

---

## 6. Cross-Story Dependencies

**NT-001: Repository and Solution Structure** — No dependencies. Must start first. Creates the folder layout and solution file that all other stories slot into.

**NT-002: Shared Backend Libraries** — Depends on NT-001 (folder structure and solution file must exist). Creates the six shared projects referenced by NT-005.

**NT-003: Angular Frontend Shell** — Depends on NT-001 (frontend folder placeholder must exist). No dependency on NT-002 or NT-005 — the frontend and backend are developed independently in Phase 0.

**NT-004: Docker Compose Local Dev Environment** — Depends on NT-001 (repo root must exist for docker-compose.yml placement). Depends on NT-005 (all service container names/ports must be known). Can be developed in parallel with NT-002 and NT-003 but compose file is finalised after NT-005.

**NT-005: API Project Stubs** — Depends on NT-001 (solution file and service project folders must exist) and NT-002 (shared libraries must be referenceable). Must run after NT-002.

**Parallelisation summary:**
- Start: NT-001 (blocks all others)
- After NT-001: NT-002, NT-003 can run in parallel
- After NT-002: NT-005 can start
- After NT-001 + NT-005 ports confirmed: NT-004 is finalised
- All five stories are sequential or parallel as shown — no circular dependencies

```
NT-001
├── NT-002 ──► NT-005
│                └──► NT-004 (finalize)
└── NT-003
└── NT-004 (start docker-compose skeleton — complete after NT-005)
```

---

## 7. Integration Points

### Integration with `Traverse.AI.Abstractions` and `Traverse.AI.Providers`

- How we connect: Project reference from `Traverse.sln`. No API call — compile-time reference.
- What we depend on: These projects exist at `src/shared/Traverse.AI.Abstractions/` and `src/shared/Traverse.AI.Providers/`. They currently target `net9.0` with nullable enabled and TreatWarningsAsErrors; they will be retargeted to `net10.0` in NT-002 (see §8 Technology — .NET 10.0).
- Impact on existing components: None. We adopt their patterns (file-scoped namespaces, primary constructors, records for value objects) as the convention baseline. No modifications to either project in Phase 0.

---

### Integration with PostgreSQL

- How we connect: `Traverse.Infrastructure.Persistence` wraps EF Core with Npgsql provider. Each service's `appsettings.json` contains a placeholder connection string pointing to the Docker Compose postgres container.
- What we depend on: PostgreSQL container running at `localhost:5432` (Docker Compose). Database names per service created by `docker/postgres/init.sql`.
- Impact on existing components: None — PostgreSQL is new infrastructure.

---

### Integration with RabbitMQ

- How we connect: `Traverse.Infrastructure.Messaging` bootstraps MassTransit with the RabbitMQ transport. Connection credentials come from environment variables / `appsettings.json`.
- What we depend on: RabbitMQ container running at `localhost:5672` (Docker Compose) with management UI at `15672`.
- Impact on existing components: None — RabbitMQ is new infrastructure.

---

### Integration with n8n

- How we connect: n8n runs as a Docker Compose service with a mounted workflows directory. Phase 0 does not wire any n8n workflows to the API services — this is deferred to Phase 1 Workflow stories.
- What we depend on: n8n container accessible at `localhost:5678`.
- Impact on existing components: None.

---

### Integration with Angular proxy

- How we connect: `proxy.conf.json` in the Angular workspace routes `/api/workflow` → `http://localhost:5001`, `/api/kpi` → `http://localhost:5002`, and so on for all nine services.
- What we depend on: The nine service ports (5001–5009) are stable from Phase 0.
- Impact on existing components: None — Angular workspace is new.

---

### Integration with CI / Build

- How we connect: `Traverse.sln` is the single build target. `dotnet build Traverse.sln` must succeed with zero warnings (TreatWarningsAsErrors). Angular builds via `ng build --configuration production`.
- What we depend on: .NET 10 SDK 10.0.103 (pinned in `global.json`); Node/Angular CLI.
- Impact on existing components: The existing AI projects are already CI-clean — adopting them into the solution must not break this.

---

## 8. Technology Decisions

### Technology: .NET 10.0

- Purpose: Runtime and SDK for all backend projects.
- Justification: The developer's machine has only the .NET 10 SDK installed (`10.0.103`, confirmed by `global.json`). During NT-001 implementation it was discovered that `Traverse.AI.Providers` already required `Microsoft.Extensions.*.Version="10.0.*"` packages to satisfy transitive dependency constraints introduced by OpenAI SDK 2.x (via System.ClientModel 1.10.0). Pinning at .NET 9 caused NU1605 (Warning As Error) on the .NET 10 SDK. ADR-004 (2026-04-27) explicitly selected `net10.0` as the target framework for all new projects in this solution. The existing AI projects (`net9.0` TFM) continue to build cleanly on the .NET 10 SDK and will be retargeted to `net10.0` in NT-002 when the shared library projects are created. .NET 10 provides primary constructors (C# 12/13), the improved `IExceptionHandler` API, and `AddOpenTelemetry` SDK v1.x stability — all the same capabilities as .NET 9 plus forward compatibility.
- Constraints: SDK version pinned in `global.json` at `10.0.103` with `rollForward: latestFeature`. All new `.csproj` files use `<TargetFramework>net10.0</TargetFramework>`. The two pre-existing AI projects (`Traverse.AI.Abstractions`, `Traverse.AI.Providers`) currently target `net9.0`; they will be updated to `net10.0` in NT-002 to maintain a consistent TFM across the solution.

---

### Technology: Entity Framework Core (EF Core) with Npgsql provider

- Purpose: ORM for all service database access.
- Justification: EF Core migrations per service (per .NET Coding Standards §13 — "each microservice owns its database schema"). The Npgsql provider is the standard for PostgreSQL in .NET. Dapper used for read-side queries in Phase 1 where performance matters (per standards §13 Dapper section).
- Constraints: EF Core version aligned with .NET 10 (`10.x` / `9.x` depending on availability at NT-002 implementation time — use the latest stable EF Core that supports `net10.0`). Each service manages its own migrations directory.

---

### Technology: MassTransit with RabbitMQ transport

- Purpose: Async messaging between services; consumer registration; outbox pattern support.
- Justification: MassTransit is cited explicitly in the .NET Coding Standards §7. It abstracts the broker so the transport can be swapped to Azure Service Bus or AWS SQS for production without changing consumer code.
- Constraints: RabbitMQ 3.x (management plugin) in Docker Compose. MassTransit `8.x` (latest stable for .NET 10).

---

### Technology: Serilog + OpenTelemetry SDK

- Purpose: Structured logging (Serilog) and distributed tracing/metrics (OpenTelemetry).
- Justification: Mandated by .NET Coding Standards §10. Serilog's `CompactJsonFormatter` for console output and rolling file sink. OpenTelemetry OTLP exporter configured with a placeholder OTLP endpoint — deferred to production infrastructure decisions.
- Constraints: Serilog `4.x`. OTel SDK `1.x`. OTLP collector not deployed in Phase 0 — the exporter endpoint is configured but the collector container is not included in docker-compose.yml (added in a later infrastructure story).

---

### Technology: Angular (latest stable) with Angular Material 3

- Purpose: Frontend SPA framework and component library.
- Justification: Angular Coding Standards §1 mandates standalone-first, strict TypeScript, and Material 3. Angular Material 3 with M3 design tokens provides accessible paired contrast ratios and a future-proof theming API.
- Constraints: `ng new` with `--standalone --strict`. TypeScript strict mode. Angular Material `18.x` or later (whatever `npm install @angular/material@latest` resolves to at time of NT-003 execution). The `mat.define-theme()` API requires Sass.

---

### Technology: PostgreSQL (Docker Compose)

- Purpose: Relational database for all nine services.
- Justification: Standard relational store; well-supported by EF Core + Npgsql; strong JSONB support for flexible schema fields in case/workitem types.
- Constraints: `postgres:latest` image in Docker Compose for local dev. Production version to be pinned in a later infrastructure story.

---

### Technology: n8n (workflow automation)

- Purpose: Visual workflow engine for case management automation rules (Phase 1 Workflow stories).
- Justification: Specified in the PRD tech stack. Low-code workflow orchestration with REST webhook triggers compatible with the ASP.NET Core API surface.
- Constraints: `no-auth` mode for local dev only. Workflow persistence backed by the n8n default SQLite in the mounted volume.

---

## 9. Risk Assessment

### Risk 1: TreatWarningsAsErrors on a greenfield build

- Likelihood: Medium
- Impact: Medium — build failures block all downstream stories
- Mitigation: The existing AI projects already compile clean under this flag. Phase 0 stories must run `dotnet build Traverse.sln` as the final acceptance check before each PR. Address any warning as it appears — do not accumulate them.

---

### Risk 2: Angular Material 3 palette generation for a custom brand colour

- Likelihood: Medium
- Impact: Low — worst case is a temporary placeholder palette
- Mitigation: Use the Angular Material palette generation tool (https://material-foundation.github.io/material-theme-builder/) to generate a Sass-compatible palette from `#1e4d8c`. Commit the generated `_palette.scss` file. If the tool changes API, fall back to the nearest Azure palette from `mat.$azure-palette` as a temporary proxy.

---

### Risk 3: Docker Compose port conflicts on developer machines

- Likelihood: Low
- Impact: Low — local dev inconvenience only
- Mitigation: Document all port assignments in `README.md`. Provide an `.env.example` with overridable port variables. Use non-standard ports (5001–5009) that are unlikely to conflict with common local services.

---

### Risk 4: n8n licensing / version changes

- Likelihood: Low
- Impact: Low in Phase 0 (n8n is not wired to anything); Medium in Phase 1 Workflow stories
- Mitigation: Phase 0 only runs n8n as a container — no workflow configuration is committed. Phase 1 Workflow stories will evaluate the specific n8n version and enterprise license requirements before building on it.

---

### Risk 5: EF Core migration strategy across nine services

- Likelihood: Low (Phase 0 concern only — no migrations in Phase 0)
- Impact: High if the pattern is inconsistent (migration conflicts, missing `init.sql` databases)
- Mitigation: `docker/postgres/init.sql` creates one empty database per service in Phase 0 (e.g., `traverse_workflow`, `traverse_kpi`, etc.). Each Phase 1 story adds its own `dotnet ef migrations add InitialCreate` — never shared. Document the migration convention in `README.md`.

---

### Risk 6: Scope creep — adding business logic to Phase 0 stories

- Likelihood: Medium (natural developer instinct to "do a bit more")
- Impact: High — delays Phase 0 completion and introduces untested code with no acceptance criteria
- Mitigation: Architecture explicitly marks all business logic as out-of-scope. Story acceptance criteria are structural (compile, run, correct file layout) not functional. Dev Lead (self) enforces the boundary at code review.

---

## 10. Story-Level Estimates

> **Calibration:** 0 actuals logged for northwoods-traverse. Default calibration factor 1.4 in use — warm-up period active. Confidence bounds are wider than they will be once calibration data accumulates.

---

### NT-001: Repository and Solution Structure

| Component | Value | Notes |
|---|---|---|
| New components | 1 | × 1.0 = 1.0 CU |
| Integration boundaries | 0 | × 1.5 = 0.0 CU |
| Acceptance criteria | 8 | × 0.5 = 4.0 CU |
| Data changes | 0 | × 1.0 = 0.0 CU |
| NFR level | Low | weight = 1.0 CU |
| **Base CU** | **6.0** | |
| Calibration factor | 1.4 | default — 0 of 6 actuals |
| Risk multiplier | 1.0 | Low — well-known scaffolding, no unknowns |
| Project weight | 2.5 | northwoods-traverse |
| Sub-total (before penalty) | 21.0 | 6.0 × 1.4 × 1.0 × 2.5 |
| Ambiguity penalty | + 0.0 | None |
| **Adjusted CU** | **21.0** | |

**Hours estimate (at 2.0 hrs/CU):** 42.0 hours
**Confidence (architecture gate, ±50–100%):** 21.0 – 84.0 hrs

> **Warm-up period active** — 0 of 6 required actuals logged for northwoods-traverse. Default calibration factor 1.4 in use.

---

### NT-002: Shared Backend Libraries

| Component | Value | Notes |
|---|---|---|
| New components | 6 | × 1.0 = 6.0 CU — one class library project per shared concern |
| Integration boundaries | 3 | × 1.5 = 4.5 CU — EF Core, MassTransit/RabbitMQ, OpenTelemetry |
| Acceptance criteria | 6 | × 0.5 = 3.0 CU — one AC set per library |
| Data changes | 1 | × 1.0 = 1.0 CU — AuditLog entity, EF Core value converters |
| NFR level | Medium | weight = 2.0 CU — warnings-as-errors, clean compile |
| **Base CU** | **16.5** | |
| Calibration factor | 1.4 | default — 0 of 6 actuals |
| Risk multiplier | 1.0 | Low — existing AI libs provide the pattern; well-understood libraries |
| Project weight | 2.5 | northwoods-traverse |
| Sub-total (before penalty) | 57.75 | 16.5 × 1.4 × 1.0 × 2.5 |
| Ambiguity penalty | + 0.0 | None |
| **Adjusted CU** | **57.75** | |

**Hours estimate (at 2.0 hrs/CU):** 115.5 hours
**Confidence (architecture gate, ±50–100%):** 57.75 – 231.0 hrs

> **Warm-up period active** — 0 of 6 required actuals logged for northwoods-traverse. Default calibration factor 1.4 in use.

---

### NT-003: Angular Frontend Shell

| Component | Value | Notes |
|---|---|---|
| New components | 8 | × 1.0 = 8.0 CU — AppShell, AuthService, authGuard, adminGuard, 4 shared presentational components |
| Integration boundaries | 1 | × 1.5 = 1.5 CU — proxy.conf.json to backend API ports |
| Acceptance criteria | 11 | × 0.5 = 5.5 CU — from NT-003 acceptance criteria |
| Data changes | 0 | × 1.0 = 0.0 CU |
| NFR level | Medium | weight = 2.0 CU — strict TS, ng build prod, a11y Material tokens |
| **Base CU** | **17.0** | |
| Calibration factor | 1.4 | default — 0 of 6 actuals |
| Risk multiplier | 1.0 | Low — established Angular patterns, Material 3 well-documented |
| Project weight | 2.5 | northwoods-traverse |
| Sub-total (before penalty) | 59.5 | 17.0 × 1.4 × 1.0 × 2.5 |
| Ambiguity penalty | + 0.0 | None |
| **Adjusted CU** | **59.5** | |

**Hours estimate (at 2.0 hrs/CU):** 119.0 hours
**Confidence (architecture gate, ±50–100%):** 59.5 – 238.0 hrs

> **Warm-up period active** — 0 of 6 required actuals logged for northwoods-traverse. Default calibration factor 1.4 in use.

---

### NT-004: Docker Compose Local Dev Environment

| Component | Value | Notes |
|---|---|---|
| New components | 1 | × 1.0 = 1.0 CU — docker-compose.yml + init.sql + README update |
| Integration boundaries | 3 | × 1.5 = 4.5 CU — PostgreSQL, RabbitMQ, n8n |
| Acceptance criteria | 5 | × 0.5 = 2.5 CU — from NT-004 acceptance criteria |
| Data changes | 0 | × 1.0 = 0.0 CU |
| NFR level | Low | weight = 1.0 CU — docker compose up succeeds |
| **Base CU** | **9.0** | |
| Calibration factor | 1.4 | default — 0 of 6 actuals |
| Risk multiplier | 1.0 | Low — standard Docker Compose patterns |
| Project weight | 2.5 | northwoods-traverse |
| Sub-total (before penalty) | 31.5 | 9.0 × 1.4 × 1.0 × 2.5 |
| Ambiguity penalty | + 0.0 | None |
| **Adjusted CU** | **31.5** | |

**Hours estimate (at 2.0 hrs/CU):** 63.0 hours
**Confidence (architecture gate, ±50–100%):** 31.5 – 126.0 hrs

> **Warm-up period active** — 0 of 6 required actuals logged for northwoods-traverse. Default calibration factor 1.4 in use.

---

### NT-005: API Project Stubs (9 Services)

| Component | Value | Notes |
|---|---|---|
| New components | 9 | × 1.0 = 9.0 CU — one Web API project per service |
| Integration boundaries | 2 | × 1.5 = 3.0 CU — PostgreSQL connection string, RabbitMQ placeholder |
| Acceptance criteria | 4 | × 0.5 = 2.0 CU — Program.cs pattern, appsettings, Dockerfile, sln reference |
| Data changes | 0 | × 1.0 = 0.0 CU |
| NFR level | Low | weight = 1.0 CU — dotnet build succeeds |
| **Base CU** | **15.0** | |
| Calibration factor | 1.4 | default — 0 of 6 actuals |
| Risk multiplier | 1.0 | Low — repetitive scaffolding, clear template from NT-002 Program.cs pattern |
| Project weight | 2.5 | northwoods-traverse |
| Sub-total (before penalty) | 52.5 | 15.0 × 1.4 × 1.0 × 2.5 |
| Ambiguity penalty | + 0.0 | None |
| **Adjusted CU** | **52.5** | |

**Hours estimate (at 2.0 hrs/CU):** 105.0 hours
**Confidence (architecture gate, ±50–100%):** 52.5 – 210.0 hrs

> **Warm-up period active** — 0 of 6 required actuals logged for northwoods-traverse. Default calibration factor 1.4 in use.

---

### Estimation Summary

| Story | Adjusted CU | Hours | Min (−50%) | Max (+100%) |
|---|---|---|---|---|
| NT-001: Repository and Solution Structure | 21.0 | 42.0 hrs | 21.0 hrs | 84.0 hrs |
| NT-002: Shared Backend Libraries | 57.75 | 115.5 hrs | 57.75 hrs | 231.0 hrs |
| NT-003: Angular Frontend Shell | 59.5 | 119.0 hrs | 59.5 hrs | 238.0 hrs |
| NT-004: Docker Compose Local Dev Environment | 31.5 | 63.0 hrs | 31.5 hrs | 126.0 hrs |
| NT-005: API Project Stubs (9 Services) | 52.5 | 105.0 hrs | 52.5 hrs | 210.0 hrs |
| **Total** | **222.25** | **444.5 hrs** | **222.25 hrs** | **889.0 hrs** |

**Calibration:** 0 actuals logged for northwoods-traverse. Default factor 1.4 in use — warm-up period active.

**Key Assumptions:**
- All five stories follow the patterns established by the existing AI shared libraries (file-scoped namespaces, primary constructors, strict nullable, TreatWarningsAsErrors).
- The developer is the single operator (PM + Dev Lead + Stakeholder), eliminating review round-trip overhead.
- No external blockers (SSO, production infrastructure) affect Phase 0 timeline.
- NT-002 and NT-003 are estimated as independent parallel tracks — the wide confidence range accounts for possible sequencing constraints discovered during implementation.

**Risk Factors That Could Increase Effort:**
- Angular Material 3 palette customisation — captured in Risk Assessment §9 Risk 2.
- Serilog / OpenTelemetry NuGet package compatibility with .NET 10 at time of implementation.
- Warm-up period: calibration factor defaults to 1.4; actual project velocity unknown until first actuals are logged.

---

## Approval Record

> **AutoMode: true** — All roles are "self" (single operator). Phase 0 is pure infrastructure scaffolding with no domain design decisions. All three approval gates are self-approved.

### Q1 — Dev Lead Technical Approval

**Role:** Acting as: DevLead

**Answer:** Approved. The component boundaries are clear and respect the Dependency Inversion Principle. The six shared libraries follow Clean Architecture layering correctly (Domain has zero infrastructure dependencies). The nine API stubs establish stable bounded context boundaries from day one. The data model correctly defers all business schema to Phase 1. No circular dependencies exist in the cross-story dependency graph. Risks are identified and mitigated. Technical approach is sound.

**Date:** 2026-04-26

---

### Q2 — PM Story Coverage Confirmation

**Role:** Acting as: PM

**Answer:** Confirmed. All five sub-phases (0a–0e) from the requirements document are covered by NT-001 through NT-005. No acceptance criterion from the requirements is missing from the story list. The architecture addresses all structural deliverables: solution structure (NT-001), shared libraries (NT-002), Angular shell (NT-003), Docker environment (NT-004), API stubs (NT-005). The out-of-scope list correctly excludes business logic, real auth, and production deployment config.

**Date:** 2026-04-26

---

### Q3 — Stakeholder Estimation Sign-Off

**Role:** Acting as: Stakeholder

**Answer:** Approved to proceed. Total estimate of 444.5 hrs (~55.6 working days at 8hrs/day) for greenfield infrastructure scaffolding on a 9-service platform is within acceptable range given the confidence bounds during the warm-up period. Phase 0 is a one-time cost that enables all subsequent feature work. The risk of scope creep (Risk 6) is accepted with the architectural constraint that no business logic enters Phase 0 stories. Proceed with implementation.

**Date:** 2026-04-26

---

---

## Change Log

| Date | Author | Section(s) Changed | Reason |
|---|---|---|---|
| 2026-04-27 | Kendrew Peacey | §8 Technology Decisions — .NET Runtime, EF Core, MassTransit constraints | resume-pipeline review — NT-001 implementation (PR #1, commit ed98ce87) confirmed developer machine has .NET 10 SDK (10.0.103) only. `Traverse.AI.Providers` already required `Microsoft.Extensions.*.Version="10.0.*"` packages due to OpenAI SDK 2.x transitive dependency (NU1605 with net9.0 SDK). ADR-004 (2026-04-27) explicitly selected net10.0 for all new projects. Architecture technology section updated from ".NET 9.0" to ".NET 10.0" with full rationale and constraint on retargeting existing AI projects in NT-002. |
| 2026-04-30 | audit-docs skill (NT-003) | §1 Solution Overview, §7 Integration Points (AI libs), §7 Integration with CI/Build, §5 AppShellComponent, §5 Shared Presentational Components, §10 Risk Factors | NT-003 audit-docs pass found five residual documentation-debt issues: (1) §1 line 30 still said ".NET 9" — updated to ".NET 10"; (2) §7 AI libs still said `net9.0` as current TFM — clarified they target net9.0 now, retargeted to net10.0 in NT-002; (3) §7 CI/Build still said ".NET 9 SDK" — corrected to ".NET 10 SDK 10.0.103"; (4) §5 AppShellComponent `Called by: bootstrapApplication root` was imprecise — updated to reflect router-based activation; (5) §5 PageHeaderComponent described "optional breadcrumbs" — corrected to "optional subtitle" per NT-003 design AC-023. Also corrected §10 risk factor mentioning ".NET 9" to ".NET 10". All are doc-debt fixes; no architectural intent changed. |

<!-- Generated by skill: design-architecture v1.5.0 | 2026-04-26 09:00 -->
<!-- Updated by skill: resume-pipeline v2.6.0 | 2026-04-27 -->
<!-- Updated by skill: audit-docs (NT-003) | 2026-04-30 -->
