# Design: NT-004 — Local Dev Environment

> **Story:** NT-004 — Docker Compose Local Dev Environment
> **Feature:** NT-000 — Phase 0 Scaffolding
> **Author:** Kendrew Peacey (Dev Lead / PM / Stakeholder — single operator, AutoMode)
> **Status:** Draft for Self-Approval
> **Date:** 2026-04-30
> **Skill:** design-feature v2.4.0

---

## Table of Contents

1. [Problem Statement](#1-problem-statement)
2. [User Stories / Acceptance Criteria](#2-user-stories--acceptance-criteria)
3. [Proposed Solution](#3-proposed-solution)
4. [Detailed Specifications](#4-detailed-specifications)
   - 4.1 [Architecture & Service Topology](#41-architecture--service-topology)
   - 4.2 [PostgreSQL Service](#42-postgresql-service)
   - 4.3 [RabbitMQ Service](#43-rabbitmq-service)
   - 4.4 [n8n Service](#44-n8n-service)
   - 4.5 [Backend Service Stubs (9 ASP.NET Core APIs)](#45-backend-service-stubs-9-aspnet-core-apis)
   - 4.6 [Database Init Script (`docker/postgres/init.sql`)](#46-database-init-script-dockerpostgresinitsql)
   - 4.7 [Port Assignments & Network Topology](#47-port-assignments--network-topology)
   - 4.8 [Volumes & Persistence](#48-volumes--persistence)
   - 4.9 [Credentials & `.env` Strategy](#49-credentials--env-strategy)
   - 4.10 [README Quickstart Outline](#410-readme-quickstart-outline)
5. [Edge Cases & Error Handling](#5-edge-cases--error-handling)
6. [Out of Scope](#6-out-of-scope)
7. [Open Questions](#7-open-questions)
8. [Architecture Conformance](#8-architecture-conformance)
9. [Effort Estimate (CU-Derived)](#9-effort-estimate-cu-derived)
10. [Decisions & Rationale Log](#10-decisions--rationale-log)
11. [Approval Record](#11-approval-record)

---

## 1. Problem Statement

Phase 0 leaves a developer with a buildable .NET solution (NT-001), shared libraries (NT-002), an Angular shell (NT-003), and nine empty API stubs (NT-005), but **no way to actually run anything end-to-end on a laptop**. Each service needs a PostgreSQL database, an event broker (RabbitMQ), and access to the workflow engine (n8n) — and a developer should not have to install or configure any of these by hand.

NT-004 closes this gap by producing a single-command local environment. After running `docker compose up`, the developer must have:

- PostgreSQL running with one empty database per service (database-per-service per .NET Coding Standards §13).
- RabbitMQ running with the management UI and credentials wired through environment variables.
- n8n running in no-auth mode with a mounted workflows directory.
- All nine ASP.NET Core API stubs running, listening on their assigned ports (5001–5009), and reachable from the Angular dev proxy.
- Health checks that allow Compose to report ready state for each service.
- A README that documents the quickstart, port map, and the `ng serve` instructions.

**Without this story:** Developers cannot integration-test cross-service flows, the Angular `proxy.conf.json` has nothing to proxy to, and Phase 1 stories cannot run an end-to-end "create case → emit event → consume in another service" flow. Every Phase 1 story implicitly depends on this environment.

**Critical scope constraint:** This is **infrastructure wiring only**. No business logic, no migrations, no real auth, no production deployment configuration. The deliverable is the Docker Compose definition, the database init script, and the README updates — nothing else.

---

## 2. User Stories / Acceptance Criteria

### User Story

> As a Northwoods Traverse developer, I want a single `docker compose up` command to start the entire local development environment (PostgreSQL, RabbitMQ, n8n, and all nine service APIs) so that I can run, debug, and integration-test the platform without manually installing or configuring any infrastructure component.

### Acceptance Criteria

#### AC-1: `docker compose up` brings the full stack online

**Given** a developer has cloned the repo and has Docker Desktop running,
**When** they run `docker compose up` from the repo root,
**Then** all containers (postgres, rabbitmq, n8n, 9 API stubs) reach the `running` state and pass their `healthcheck` within 90 seconds, with no manual intervention.

#### AC-2: Database-per-service is provisioned on first start

**Given** a fresh `postgres-data` volume,
**When** the postgres container starts for the first time,
**Then** `docker/postgres/init.sql` runs automatically and creates exactly nine databases — `traverse_workflow`, `traverse_kpi`, `traverse_admin`, `traverse_notifications`, `traverse_reporting`, `traverse_aicopilot`, `traverse_compliance`, `traverse_search`, `traverse_calendar` — each owned by the configured admin user.

#### AC-3: RabbitMQ management UI is reachable with documented credentials

**Given** the RabbitMQ container is healthy,
**When** the developer opens `http://localhost:15672`,
**Then** they can log in with the credentials defined in `.env` (default user `traverse`, default password `traverse_dev`) and see the default vhost `/`.

#### AC-4: n8n editor is reachable with no authentication

**Given** the n8n container is healthy,
**When** the developer opens `http://localhost:5678`,
**Then** the n8n editor loads without prompting for credentials, and a host-side `n8n/workflows/` directory is mounted into the container so workflow JSON files can be version-controlled in Phase 1.

#### AC-5: Service API stubs are reachable on assigned ports

**Given** the API stub containers are healthy,
**When** the developer hits `http://localhost:{5001..5009}/health/live`,
**Then** each service responds `200 OK` with `{"status":"Healthy"}`, the response is observable in the Compose log stream, and the Angular proxy at `proxy.conf.json` resolves `/api/{service}` correctly to each port.

#### AC-6: README quickstart is accurate and copy-pasteable

**Given** a developer reading `README.md` for the first time,
**When** they follow the "Quickstart" section verbatim,
**Then** they can clone, run `docker compose up`, run `ng serve` (in the Angular workspace), see the Angular shell load at `http://localhost:4200`, and the proxy successfully reaches the backend `/health/live` endpoints. The README also documents how to override default ports and credentials.

---

## 3. Proposed Solution

### Approach Selected: Single root `docker-compose.yml` + per-service Dockerfile (architecture_phase0.md Decision 5)

**How it works:**
- One `docker-compose.yml` at the repo root declares all 12 containers (postgres, rabbitmq, n8n, plus 9 API stubs).
- Each API stub has its own multi-stage `Dockerfile` at `docker/{service}/Dockerfile` (the folders already exist as placeholders from NT-001). The Dockerfile builds the corresponding `Traverse.{Module}.Api` project from the .NET solution.
- A single `init.sql` creates one database per service. Postgres runs it automatically because Docker mounts it into `/docker-entrypoint-initdb.d/`.
- Credentials and tunable values live in a committed `.env.example` (no secrets) with a developer-local `.env` file (gitignored) loaded by Compose via the standard `${VAR}` interpolation.
- Health checks gate `depends_on` so API stubs do not start until postgres and rabbitmq are ready.
- A custom bridge network `traverse-net` connects all containers; service names act as DNS hostnames (e.g., `postgres`, `rabbitmq`).
- README is updated with a quickstart section, the port map, the `ng serve` instructions, and a troubleshooting block.

**Alternatives considered (and rejected):**

1. **Separate compose files per group (infra vs services).** Rejected per architecture_phase0.md Decision 5 — the orchestration overhead has no benefit at this scale, and `docker compose up` becomes `docker compose -f infra.yml -f services.yml up` which contradicts the AC-1 single-command requirement.
2. **Skip per-service Dockerfiles, run all APIs in one container.** Rejected — defeats the microservice decomposition (architecture Decision 2), forces every service to share a process and crash boundary, and prevents per-service image control needed for production.
3. **Use a managed dev environment (Tilt, Skaffold, devcontainers).** Rejected — overkill for a single-developer Phase 0 scaffold; introduces a new tool the team must learn before any business value is delivered. Reconsider in a future infrastructure story when team size grows.

### YAGNI Pruning

The following capabilities were deliberately excluded as speculative for Phase 0:
- **OpenTelemetry collector container** (architecture_phase0.md §8 explicitly defers this).
- **Reverse proxy / API gateway** (Angular dev proxy handles routing; a real gateway is a Phase 4+ infrastructure decision).
- **Test data seed scripts** (databases are created empty; Phase 1 stories own their seed data).
- **Production-grade postgres tuning, replication, or backups** (local dev only).
- **Hot-reload / file watching** for the API stubs (the developer can use `dotnet run` outside Compose for tight iteration loops; Compose is for full-stack runs).
- **TLS / HTTPS termination** (HTTP only inside the Compose network; HTTPS is a deployment concern).

---

## 4. Detailed Specifications

### 4.1 Architecture & Service Topology

```
┌────────────────────────────────────────────────────────────────────────┐
│  Host machine (developer laptop)                                       │
│                                                                        │
│  Angular dev server (ng serve)  ──proxy──►  http://localhost:5001..9   │
│  (port 4200, run separately, not in compose)                           │
│                                                                        │
│  ┌──────────────────────────── Docker network: traverse-net ────────┐  │
│  │                                                                  │  │
│  │  ┌──────────┐   ┌──────────┐   ┌────┐                            │  │
│  │  │ postgres │   │ rabbitmq │   │ n8n│                            │  │
│  │  │  :5432   │   │ :5672    │   │:5678│                           │  │
│  │  │  init.sql│   │ :15672 UI│   │     │                           │  │
│  │  └──────────┘   └──────────┘   └────┘                            │  │
│  │       ▲                ▲                                         │  │
│  │       │                │                                         │  │
│  │  ┌────┴────────────────┴───────────────────────────────────┐     │  │
│  │  │ workflow-api  kpi-api  admin-api  notifications-api ... │     │  │
│  │  │   :5001        :5002    :5003       :5004       ...     │     │  │
│  │  │ (9 ASP.NET Core stubs, each from src/services/{name}/   │     │  │
│  │  │  Traverse.{Module}.Api/Dockerfile)                      │     │  │
│  │  └─────────────────────────────────────────────────────────┘     │  │
│  └──────────────────────────────────────────────────────────────────┘  │
└────────────────────────────────────────────────────────────────────────┘
```

**Network:** A single user-defined bridge network `traverse-net` is declared in compose. All containers attach to it. Service names act as DNS hostnames (e.g., the Workflow API uses `Host=postgres;Database=traverse_workflow;...`).

**Why a custom bridge instead of the default:** User-defined networks provide automatic DNS resolution by service name; the default bridge requires `--link` and is deprecated.

**Compose file version:** Omit the `version:` key (Docker Compose v2 ignores it and emits a deprecation warning when present).

---

### 4.2 PostgreSQL Service

| Property | Value |
|---|---|
| Image | `postgres:latest` (per §0d requirement) |
| Container name | `traverse-postgres` |
| Internal port | `5432` |
| Host port | `${POSTGRES_PORT:-5432}` |
| Environment | `POSTGRES_USER=${POSTGRES_USER:-traverse}`, `POSTGRES_PASSWORD=${POSTGRES_PASSWORD:-traverse_dev}`, `POSTGRES_DB=postgres` (default; service DBs created by init.sql) |
| Volume (data) | `postgres-data:/var/lib/postgresql/data` (named volume, persists across `docker compose down`) |
| Volume (init) | `./docker/postgres/init.sql:/docker-entrypoint-initdb.d/init.sql:ro` |
| Healthcheck | `CMD-SHELL pg_isready -U $${POSTGRES_USER}`, interval `10s`, timeout `5s`, retries `10`, start_period `15s` |
| Network | `traverse-net` |
| Restart policy | `unless-stopped` |

**Note on `postgres:latest`:** The §0d requirement specifies `postgres:latest`. This is acceptable for local dev (architecture Risk 5) but should be pinned to a specific major version (e.g., `postgres:16`) before any production deployment. A note to that effect is added to README troubleshooting.

**Note on init.sql idempotency:** Postgres runs `/docker-entrypoint-initdb.d/*.sql` **only on first initialisation of an empty data directory**. If a developer needs to re-run init (e.g., to add a new database after a service is added), they must `docker compose down -v` to delete the volume, or apply the change manually via `psql`. README documents this.

---

### 4.3 RabbitMQ Service

| Property | Value |
|---|---|
| Image | `rabbitmq:3-management` (per §0d requirement) |
| Container name | `traverse-rabbitmq` |
| Internal ports | `5672` (AMQP), `15672` (management UI) |
| Host ports | `${RABBITMQ_PORT:-5672}`, `${RABBITMQ_MGMT_PORT:-15672}` |
| Environment | `RABBITMQ_DEFAULT_USER=${RABBITMQ_USER:-traverse}`, `RABBITMQ_DEFAULT_PASS=${RABBITMQ_PASSWORD:-traverse_dev}`, `RABBITMQ_DEFAULT_VHOST=/` |
| Volume | `rabbitmq-data:/var/lib/rabbitmq` (persists queue definitions) |
| Healthcheck | `CMD rabbitmq-diagnostics -q ping`, interval `10s`, timeout `5s`, retries `10`, start_period `30s` |
| Network | `traverse-net` |
| Restart policy | `unless-stopped` |

**Default vhost:** `/` (RabbitMQ default). MassTransit will use this vhost from the `Traverse.Infrastructure.Messaging` library (NT-002).

---

### 4.4 n8n Service

| Property | Value |
|---|---|
| Image | `n8nio/n8n:latest` |
| Container name | `traverse-n8n` |
| Internal port | `5678` |
| Host port | `${N8N_PORT:-5678}` |
| Environment | `N8N_BASIC_AUTH_ACTIVE=false`, `N8N_HOST=localhost`, `N8N_PORT=5678`, `WEBHOOK_URL=http://localhost:5678/`, `GENERIC_TIMEZONE=America/Toronto` |
| Volume (data) | `n8n-data:/home/node/.n8n` (persists workflows + SQLite DB) |
| Volume (workflows) | `./n8n/workflows:/home/node/.n8n/workflows` (host-side directory for version control) |
| Healthcheck | `CMD wget -qO- http://localhost:5678/healthz` (n8n exposes `/healthz`), interval `15s`, timeout `5s`, retries `5`, start_period `30s` |
| Network | `traverse-net` |
| Restart policy | `unless-stopped` |

**No-auth mode:** `N8N_BASIC_AUTH_ACTIVE=false` per §0d. README explicitly warns this is local-dev-only and must be disabled in any non-local environment.

**Workflows directory:** A `n8n/workflows/` directory is created at the repo root with a `.gitkeep` so future workflow JSON files can be committed.

**Timezone:** `America/Toronto` matches the Northwoods customer base (Ontario social-services); overridable via `${TZ}` env var.

---

### 4.5 Backend Service Stubs (9 ASP.NET Core APIs)

Each of the nine API stubs has an identical Compose service shape, differing only by name, port, and database. The pattern is parameterised below; see also §4.7 for the port map.

| Property | Pattern Value | Per-service example (workflow) |
|---|---|---|
| Build context | `.` (repo root) | `.` |
| Dockerfile | `docker/{service-short}/Dockerfile` | `docker/workflow/Dockerfile` |
| Container name | `traverse-{service-short}` | `traverse-workflow` |
| Internal port | `8080` (ASP.NET Core default in Compose) | `8080` |
| Host port | `${{SERVICE}}_PORT:-{port}` | `${WORKFLOW_PORT:-5001}:8080` |
| Environment — connection | `ConnectionStrings__Database=Host=postgres;Port=5432;Database=traverse_{service};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}` | `Database=traverse_workflow` |
| Environment — messaging | `RabbitMq__Host=rabbitmq`, `RabbitMq__Username=${RABBITMQ_USER}`, `RabbitMq__Password=${RABBITMQ_PASSWORD}` | (same) |
| Environment — runtime | `ASPNETCORE_ENVIRONMENT=Development`, `ASPNETCORE_URLS=http://+:8080` | (same) |
| `depends_on` | `postgres: condition: service_healthy`, `rabbitmq: condition: service_healthy` | (same) |
| Healthcheck | `CMD wget -qO- http://localhost:8080/health/live \|\| exit 1`, interval `15s`, timeout `5s`, retries `10`, start_period `30s` | (same) |
| Network | `traverse-net` | `traverse-net` |
| Restart policy | `unless-stopped` | (same) |

**The nine services and their per-service overrides:**

| Service | DB name | Host port | Compose service name | Dockerfile |
|---|---|---|---|---|
| Workflow | `traverse_workflow` | 5001 | `workflow-api` | `docker/workflow/Dockerfile` |
| KPI | `traverse_kpi` | 5002 | `kpi-api` | `docker/kpi/Dockerfile` |
| Admin | `traverse_admin` | 5003 | `admin-api` | `docker/admin/Dockerfile` |
| Notifications | `traverse_notifications` | 5004 | `notifications-api` | `docker/notifications/Dockerfile` |
| Reporting | `traverse_reporting` | 5005 | `reporting-api` | `docker/reporting/Dockerfile` |
| AI Copilot | `traverse_aicopilot` | 5006 | `aicopilot-api` | `docker/aicopilot/Dockerfile` |
| Compliance | `traverse_compliance` | 5007 | `compliance-api` | `docker/compliance/Dockerfile` |
| Search | `traverse_search` | 5008 | `search-api` | `docker/search/Dockerfile` |
| Calendar | `traverse_calendar` | 5009 | `calendar-api` | `docker/calendar/Dockerfile` |

**Per-service Dockerfile contract (multi-stage, mirrors .NET Coding Standards §18):**

```
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Traverse.sln", "global.json", "Directory.Build.props", "./"]
COPY ["src/", "src/"]
RUN dotnet restore "src/services/{service}/Traverse.{Module}.Api/Traverse.{Module}.Api.csproj"
RUN dotnet publish "src/services/{service}/Traverse.{Module}.Api/Traverse.{Module}.Api.csproj" \
    -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
RUN useradd -m appuser && chown -R appuser /app
USER appuser
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
HEALTHCHECK --interval=30s --timeout=10s CMD wget -qO- http://localhost:8080/health/live || exit 1
ENTRYPOINT ["dotnet", "Traverse.{Module}.Api.dll"]
```

> **Note on Dockerfile production:** The Dockerfiles themselves are produced by NT-005 (which creates the actual `Traverse.{Module}.Api` projects). NT-004 produces the `docker-compose.yml` that *references* these Dockerfiles. Per the cross-story dependency in architecture §6, NT-004's compose file is finalised after NT-005. The compose file shape is fully defined here so NT-005 only has to drop the Dockerfiles into the existing `docker/{service}/` placeholder folders.

**Why HTTP only on 8080 inside the container:** ASP.NET Core 8+ defaults to HTTP-only when no certificate is configured. HTTPS termination is deferred to the production deployment story.

---

### 4.6 Database Init Script (`docker/postgres/init.sql`)

```sql
-- docker/postgres/init.sql
-- Runs once on first postgres container initialisation (empty data volume).
-- Creates one database per service following the database-per-service pattern
-- (.NET Coding Standards §13). Each service owns its own schema and migrations.

CREATE DATABASE traverse_workflow;
CREATE DATABASE traverse_kpi;
CREATE DATABASE traverse_admin;
CREATE DATABASE traverse_notifications;
CREATE DATABASE traverse_reporting;
CREATE DATABASE traverse_aicopilot;
CREATE DATABASE traverse_compliance;
CREATE DATABASE traverse_search;
CREATE DATABASE traverse_calendar;
```

**Notes:**
- Each `CREATE DATABASE` runs as the `POSTGRES_USER` set in the postgres container env, which makes that user the owner — sufficient for local dev where one user has full access to all service databases. Production will use per-service users with least-privilege grants (out of scope for Phase 0).
- The script does not create extensions, schemas, tables, or seed data. Each Phase 1 service owns those via EF Core migrations.
- The script uses plain SQL (not `psql`-specific commands) so it runs identically under any postgres image variant.
- No `IF NOT EXISTS` clauses — these databases must not exist on first init, and re-runs require a volume reset (documented in README).

---

### 4.7 Port Assignments & Network Topology

**Public host ports (developer-visible):**

| Component | Default | Env override |
|---|---|---|
| PostgreSQL | 5432 | `POSTGRES_PORT` |
| RabbitMQ AMQP | 5672 | `RABBITMQ_PORT` |
| RabbitMQ Mgmt UI | 15672 | `RABBITMQ_MGMT_PORT` |
| n8n editor | 5678 | `N8N_PORT` |
| Workflow API | 5001 | `WORKFLOW_PORT` |
| KPI API | 5002 | `KPI_PORT` |
| Admin API | 5003 | `ADMIN_PORT` |
| Notifications API | 5004 | `NOTIFICATIONS_PORT` |
| Reporting API | 5005 | `REPORTING_PORT` |
| AI Copilot API | 5006 | `AICOPILOT_PORT` |
| Compliance API | 5007 | `COMPLIANCE_PORT` |
| Search API | 5008 | `SEARCH_PORT` |
| Calendar API | 5009 | `CALENDAR_PORT` |

**Internal DNS (container ↔ container):**
- `postgres`, `rabbitmq`, `n8n`, `workflow-api`, `kpi-api`, `admin-api`, `notifications-api`, `reporting-api`, `aicopilot-api`, `compliance-api`, `search-api`, `calendar-api`.

**Frontend Angular dev server (NT-003):** Runs on port `4200` *outside* Compose using `ng serve`. Its `proxy.conf.json` forwards `/api/{service}` requests to `http://localhost:{5001..5009}` on the host (see NT-003 design).

---

### 4.8 Volumes & Persistence

| Volume name | Mount target | Purpose |
|---|---|---|
| `postgres-data` | `/var/lib/postgresql/data` | All nine service databases. Survives `down`; cleared by `down -v`. |
| `rabbitmq-data` | `/var/lib/rabbitmq` | Queue / exchange definitions. |
| `n8n-data` | `/home/node/.n8n` | n8n internal SQLite + credentials. |

**Bind mounts (host → container):**
- `./docker/postgres/init.sql` → `/docker-entrypoint-initdb.d/init.sql` (read-only, init only)
- `./n8n/workflows` → `/home/node/.n8n/workflows` (read-write, version-controlled)

**Resetting state:**
```bash
docker compose down -v    # destroys all volumes
docker compose up         # re-runs init.sql cleanly
```

---

### 4.9 Credentials & `.env` Strategy

**Two files at the repo root:**

1. `.env.example` — committed; contains only **non-sensitive defaults** for local dev.
2. `.env` — gitignored; copied from `.env.example` on first run; developers can override locally.

**`.env.example` content:**

```ini
# PostgreSQL
POSTGRES_USER=traverse
POSTGRES_PASSWORD=traverse_dev
POSTGRES_PORT=5432

# RabbitMQ
RABBITMQ_USER=traverse
RABBITMQ_PASSWORD=traverse_dev
RABBITMQ_PORT=5672
RABBITMQ_MGMT_PORT=15672

# n8n
N8N_PORT=5678

# Service ports (override only on conflicts)
WORKFLOW_PORT=5001
KPI_PORT=5002
ADMIN_PORT=5003
NOTIFICATIONS_PORT=5004
REPORTING_PORT=5005
AICOPILOT_PORT=5006
COMPLIANCE_PORT=5007
SEARCH_PORT=5008
CALENDAR_PORT=5009
```

**Why these are not real secrets:** They are static well-known dev passwords. They never leave the developer's machine. Production credentials come from Azure Key Vault / AWS Secrets Manager (per .NET Coding Standards §15) — out of scope for Phase 0.

**`.gitignore` update:** Add `.env` and `n8n/workflows/*.credentials.json` (n8n stores encrypted credentials in this format).

---

### 4.10 README Quickstart Outline

The existing README will be expanded in the following structure (showing only the sections that NT-004 adds or modifies):

```markdown
## Quickstart (Local Dev)

### Prerequisites
(unchanged — already present)

### One-time setup
```bash
git clone <repo>
cd Northwoods_Traverse
cp .env.example .env       # local credential file (gitignored)
```

### Start the full stack
```bash
docker compose up          # foreground; Ctrl-C to stop
docker compose up -d       # detached (background)
```

First run takes ~3–5 minutes (image pulls + database init). Subsequent runs are <30s.

### Verify it's working
```bash
docker compose ps          # all services should show "healthy"
curl http://localhost:5001/health/live   # any service: {"status":"Healthy"}
open http://localhost:15672              # RabbitMQ mgmt UI (traverse / traverse_dev)
open http://localhost:5678               # n8n editor (no auth)
```

### Run the Angular workspace
```bash
cd src/frontend/traverse-workspace
npm install                              # one-time
ng serve --proxy-config proxy.conf.json  # http://localhost:4200
```

### Stop and clean up
```bash
docker compose down        # stop containers, keep data
docker compose down -v     # stop + delete all volumes (resets databases)
```

### Port Map
(extended version of the existing table — see §4.7)

### Troubleshooting

| Symptom | Cause | Fix |
|---|---|---|
| Port 5432 already in use | Local Postgres install | Set `POSTGRES_PORT=5433` in `.env` |
| `init.sql` didn't run | Volume already had data | `docker compose down -v` then `up` |
| API stubs in restart loop | postgres/rabbitmq still starting | Wait 60s; check `docker compose logs` |
| n8n login prompt appears | `N8N_BASIC_AUTH_ACTIVE` enabled accidentally | Remove the env var; restart n8n |

### Security Note
The default credentials in `.env.example` are for **local development only**. Never reuse them in any non-local environment. n8n no-auth mode is also local-only — production deployments must enable authentication.
```

---

## 5. Edge Cases & Error Handling

### EC-1: Port conflict on developer machine

**Trigger:** Another local service (e.g., a system Postgres install) already binds to 5432.
**Behaviour:** Compose fails immediately with a clear error. The developer overrides via `.env` (e.g., `POSTGRES_PORT=5433`).
**Mitigation:** All ports are env-overridable; `.env.example` documents this; troubleshooting table covers it.

### EC-2: API stub starts before postgres is ready

**Trigger:** Without coordination, the stub container would crash on first DB connection attempt.
**Behaviour:** `depends_on: { postgres: { condition: service_healthy } }` blocks the stub until Postgres reports healthy.
**Mitigation:** Healthcheck uses `pg_isready` which only returns healthy after Postgres accepts TCP connections.

### EC-3: `init.sql` does not re-run on subsequent starts

**Trigger:** The Postgres image runs init scripts only on first init (empty data volume). If a developer adds a tenth service later, the init script change won't auto-apply.
**Behaviour:** The new database is missing; the new service's connection fails.
**Mitigation:** README troubleshooting documents `docker compose down -v` to reset, plus a manual `psql` workaround for keeping existing data.

### EC-4: RabbitMQ healthcheck false positive during slow boot

**Trigger:** RabbitMQ takes longer than the healthcheck `start_period` on a slow machine.
**Behaviour:** Compose marks rabbitmq unhealthy; dependent services do not start.
**Mitigation:** `start_period: 30s` and `retries: 10` give a 130-second total grace period (start + 10×10s). Sufficient for any reasonable dev machine.

### EC-5: n8n workflows directory permission mismatch

**Trigger:** n8n runs as user `node` (uid 1000). On Linux hosts where the developer's uid is not 1000, the bind-mounted `n8n/workflows` may be unwritable by the container.
**Behaviour:** Workflow saves from inside n8n fail silently; logged but not visible in UI.
**Mitigation:** README troubleshooting documents `chown 1000:1000 n8n/workflows` for Linux developers. macOS / Windows Docker Desktop handle UID translation automatically.

### EC-6: Developer runs `docker compose up` before NT-005 is complete

**Trigger:** The compose file references Dockerfiles that don't exist yet.
**Behaviour:** Compose fails on `build` step with a clear "Dockerfile not found" error.
**Mitigation:** This story (NT-004) lands ordered after NT-005 (architecture §6 sequencing). Until NT-005 is merged, the compose file's API services are gated behind a profile (see Decision D-7) so `docker compose up` brings up only postgres/rabbitmq/n8n. Once NT-005 lands, the profile gating is removed in the NT-004 final commit.

### EC-7: Stale image after `Dockerfile` change

**Trigger:** Developer modifies a Dockerfile, runs `docker compose up`, and Compose reuses the cached image.
**Behaviour:** Changes don't take effect.
**Mitigation:** README troubleshooting documents `docker compose up --build` to force rebuild.

---

## 6. Out of Scope

The following are deliberately **not** part of NT-004 and must not be added during implementation:

- **OpenTelemetry collector container.** Architecture explicitly defers (architecture §8); environment variables for the OTLP endpoint are placeholders only.
- **Production-grade Postgres configuration** (replication, WAL archiving, tuning, backups, pinned major version).
- **Production secrets management** — Azure Key Vault / AWS Secrets Manager wiring is a deployment-time concern.
- **TLS / HTTPS** between services or to the developer's browser.
- **Reverse proxy / API gateway** (nginx, Traefik, Envoy).
- **CI/CD pipeline definition** (GitHub Actions / Azure Pipelines yml) — separate future story.
- **Kubernetes manifests / Helm charts** — production deployment story.
- **Test database seed data** — each Phase 1 service owns its own seed scripts.
- **EF Core migrations** — every service handles its own migrations in Phase 1.
- **Real authentication** — n8n no-auth, JWT auth deferred to MOD-07/Identity story.
- **Hot reload / file watcher** containers (`dotnet watch`) — developers can run `dotnet run` outside Compose for tight loops.
- **Docker secrets** (`secrets:` block) — not needed when no real secrets exist.
- **Container resource limits** (`deploy.resources.limits`) — local dev only.
- **Multi-arch image builds** — Docker Desktop handles this transparently for the developer's machine.
- **Cross-platform shell scripts** for orchestration (e.g., `make up`, `bin/start.sh`) — `docker compose` is itself the entry point.

---

## 7. Open Questions

None. All §0d requirements are explicit and the architecture document resolves all approach questions. The Socratic refinement loop (Phase 4) found no unresolved ambiguities.

---

## 8. Architecture Conformance

Cross-checked against `architecture_phase0.md`:

| Architecture element | Conformance | Notes |
|---|---|---|
| Decision 5: Single root `docker-compose.yml` + per-service Dockerfiles | ✅ Conforms | §3 Approach Selected, §4.5 |
| Component: `docker-compose.yml` (§5) | ✅ Conforms | Implemented in this story; service list matches |
| Port map (§5) | ✅ Conforms | §4.7 reproduces the architecture port table verbatim |
| Integration: PostgreSQL (§7) | ✅ Conforms | postgres container at `localhost:5432`, init.sql per-service |
| Integration: RabbitMQ (§7) | ✅ Conforms | rabbitmq container at `localhost:5672`, mgmt at `15672` |
| Integration: n8n (§7) | ✅ Conforms | n8n container at `localhost:5678`, no-auth, mounted workflows |
| Integration: Angular proxy (§7) | ✅ Conforms | Port 4200, proxies to host ports 5001–5009 |
| Cross-story deps (§6) | ✅ Conforms | Story acknowledges dependency on NT-005 for Dockerfiles; profile-gated until NT-005 merges |
| Risk 3: Port conflicts | ✅ Mitigated | EC-1, env overrides, README troubleshooting |
| Risk 5: EF Core migrations across services | ✅ Mitigated | init.sql creates per-service DBs; migrations remain Phase 1 concern |
| .NET Coding Standards §13 (database-per-service) | ✅ Conforms | One DB per service in init.sql |
| .NET Coding Standards §18 (multi-stage Dockerfile, non-root user, health probe) | ✅ Conforms | §4.5 Dockerfile contract |

**No deviations from architecture.** Implementation is a faithful realisation of architecture §5 component definitions and §7 integration points.

---

## 9. Effort Estimate (CU-Derived)

> **Recalculated at design completion** — refined from architecture-level estimate (architecture_phase0.md §10).

### CU Inputs (Design Gate)

| Input | Value | Rationale |
|---|---|---|
| New components | 1 | One `docker-compose.yml` + one `init.sql` + README updates count as a single coordinated artefact set (no business logic, no class libraries). |
| Integration boundaries | 3 | PostgreSQL (init script + healthcheck), RabbitMQ (env wiring + mgmt UI), n8n (mounted workflows + no-auth config). |
| Acceptance criteria | 6 | AC-1 through AC-6. (One more than the architecture estimate of 5 — the README quickstart criterion was split out as its own AC during design.) |
| Data changes | 0 | No schema, no migrations, no seed data — only `CREATE DATABASE` statements that are pure structural provisioning. |
| NFR level | Low | Single quality goal: `docker compose up` succeeds end-to-end on first try. No performance, scalability, or security NFRs apply at the local-dev layer. |

### CU Calculation

| Component | Value | Notes |
|---|---|---|
| New components | 1 × 1.0 | 1.0 CU |
| Integration boundaries | 3 × 1.5 | 4.5 CU |
| Acceptance criteria | 6 × 0.5 | 3.0 CU |
| Data changes | 0 × 1.0 | 0.0 CU |
| NFR level | Low (1.0) | 1.0 CU |
| **Base CU** | | **9.5** |
| Calibration factor | 1.4 | default — 0 of 6 actuals logged for northwoods-traverse (warm-up) |
| Risk multiplier | 1.0 | Low — standard Compose patterns; per-service Dockerfile shape mirrors .NET Coding Standards §18 |
| Project weight | 2.5 | northwoods-traverse |
| Sub-total (before penalty) | 33.25 | 9.5 × 1.4 × 1.0 × 2.5 |
| Ambiguity penalty | + 0.0 | None — architecture is approved and §0d is explicit |
| **Adjusted CU** | | **33.25** |

**Hours estimate (at 2.0 hrs/CU):** 66.5 hours
**Confidence (design gate, ±30–50%):** 33.25 – 99.75 hrs
**Architecture estimate was:** 31.5 Adjusted CU (architecture_phase0.md §10) — design gate refined upward by ~5.5% because AC count grew from 5 to 6 (README quickstart split out).

> **Warm-up period active** — 0 of 6 required actuals logged for northwoods-traverse. Default calibration factor 1.4 in use.

---

## 10. Decisions & Rationale Log

| ID | Decision | Rationale |
|---|---|---|
| D-1 | **Single root `docker-compose.yml`** (no per-group split) | Conforms to architecture Decision 5; satisfies AC-1 single-command requirement. |
| D-2 | **`postgres:latest`** image (per §0d) with documented intent to pin before production | §0d explicit; production pin is out of scope; README documents the constraint. |
| D-3 | **`rabbitmq:3-management`** image | §0d explicit; mgmt plugin needed for developer self-service. |
| D-4 | **n8n no-auth mode** for local dev | §0d explicit; README warns about non-local environments. |
| D-5 | **`.env` + `.env.example`** pattern (instead of compose `secrets:`) | Local-only credentials, no real secrets, simpler developer workflow, override-friendly. |
| D-6 | **User-defined bridge network `traverse-net`** | Enables service-name DNS resolution; default bridge is deprecated for inter-container DNS. |
| D-7 | **Profile-gate API services until NT-005 lands** | Compose file references Dockerfiles produced by NT-005; profiles allow `docker compose up` to bring up only infra services until NT-005 merges, at which point the profile gates are removed in NT-004's final commit. Eliminates a coordination dance during merge. |
| D-8 | **Healthcheck-driven `depends_on`** with `condition: service_healthy` | Eliminates the classic race condition where APIs start before Postgres/Rabbit accept connections; replaces sleep-based hacks. |
| D-9 | **Named volumes** for postgres/rabbitmq/n8n data | Persists state across `down`; explicit `down -v` to wipe; clearer than anonymous volumes. |
| D-10 | **Bind mount `./n8n/workflows`** | Workflow JSON files become version-controlled artefacts in Phase 1. |
| D-11 | **Plain `CREATE DATABASE` statements** in init.sql (no schema, no extensions) | Database-per-service; each Phase 1 story owns its schema and extensions via EF Core migrations. |
| D-12 | **HTTP-only on port 8080 inside containers** | TLS is a deployment concern; ASP.NET Core 8+ default; consistent with multi-stage Dockerfile pattern in .NET Coding Standards §18. |
| D-13 | **Omit Compose `version:` key** | Docker Compose v2 ignores it; including it emits a deprecation warning. |
| D-14 | **Exclude OTel collector** from Phase 0 | Architecture §8 explicitly defers; configured endpoint values are placeholders only. |
| D-15 | **No reverse proxy** in local dev | Angular dev proxy handles routing; gateway is a Phase 4+ concern. |

---

## 11. Approval Record

| Gate | Approver | Role | Date | Outcome |
|---|---|---|---|---|
| design-feature-10 | Dev Lead + PM (self) | PM + DevLead (same actor) — all lenses applied simultaneously | 2026-04-30 | **APPROVED** |

**Questions document:** `skill_docs/design-feature_docs/questions_001.md`

**Design checksum (approved):** `sha256-67a68a3ef221aae6893946a70dd514e67a0778a91f6bd1a4ffcba8b4eee7f641`

---

<!-- Generated by skill: design-feature v2.4.0 | 2026-04-30 14:35 | Approved: 2026-04-30 -->
