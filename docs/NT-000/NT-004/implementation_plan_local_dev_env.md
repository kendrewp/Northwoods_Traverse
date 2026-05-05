# Implementation Plan: NT-004 — Local Dev Environment (Docker Compose)

> **Story:** NT-004 — Docker Compose Local Dev Environment
> **Feature:** NT-000 — Phase 0 Scaffolding
> **Author:** Claude Code (plan-implementation v3.4.0)
> **Date:** 2026-04-30
> **Status:** Awaiting Dev Lead Approval
> **Design document:** `docs/NT-000/NT-004/design_local_dev_env.md` (sha256-2ea8a2543b58afb7230042c96f42e88c8edae53ee79598e2cef40b7ab9894a80)

---

## Summary

This plan implements a single-command Docker Compose local development environment for the Northwoods Traverse platform. It spans six ordered phases — each independently verifiable — producing: a `.env.example`, the root `docker-compose.yml`, a postgres `init.sql`, an `n8n/workflows/.gitkeep`, per-service stub `Dockerfile`s (profile-gated until NT-005 merges), and an expanded README quickstart. No business logic, no migrations, no authentication.

**Total phases:** 6
**Estimated total hours:** 66.5 hrs (Adjusted CU 33.25 × 2.0 hrs/CU)
**Confidence range (Checkpoint 3):** 46.55 – 86.45 hrs

> **Warm-up period active** — calibration data is still accumulating (0 of 6 required actuals logged for northwoods-traverse). Actual bounds may be wider than ±20–30% until calibration stabilises.

---

## Risk Assessment

| # | Risk | Category | Phase(s) | Mitigation |
|---|------|----------|----------|------------|
| R-1 | Compose file references Dockerfiles not yet produced by NT-005; running `docker compose up --profile api` fails until NT-005 merges | Technical | Ph-3 | API services are profile-gated (`--profile api`); default `up` brings only infra services |
| R-2 | `init.sql` only runs on empty volume; re-running after adding a service requires `docker compose down -v` | Functional | Ph-2 | README documents the `down -v` pattern explicitly in both quickstart and troubleshooting |
| R-3 | RabbitMQ slow-start on weak laptops can cause premature healthcheck failure | Technical | Ph-3 | `start_period: 30s` + `retries: 10` gives 130-second grace window |
| R-4 | n8n bind-mount UID mismatch on Linux hosts (n8n runs as uid 1000) | Technical | Ph-4 | README troubleshooting documents `chown 1000:1000 n8n/workflows` for Linux devs |
| R-5 | Port conflicts with system services (local Postgres, etc.) | Functional | Ph-1 | All ports env-overridable via `.env`; troubleshooting table covers each conflict scenario |
| R-6 | `.env` accidentally committed (exposes even non-sensitive dev credentials) | Technical | Ph-1 | `.env` already in `.gitignore`; Phase 1 Step 1 verifies this explicitly |

---

## Phase Allocation (CU-Derived Effort)

| Phase | Description | Estimated Hours | Notes |
|-------|-------------|----------------|-------|
| Ph-1 | Credentials & `.env` pattern | 4 hrs | Simple files, already gitignored |
| Ph-2 | PostgreSQL — init.sql + service config | 8 hrs | Init script is the highest functional correctness risk |
| Ph-3 | RabbitMQ & n8n Compose services | 8 hrs | Healthcheck tuning is the main effort |
| Ph-4 | API stub services (profile-gated) | 16 hrs | 9 repetitive services; Dockerfile pattern per-service |
| Ph-5 | Network, volumes, & Compose wiring | 10 hrs | Integration of all services; end-to-end `compose up` test |
| Ph-6 | README quickstart expansion | 8 hrs | Prose, port map, troubleshooting table |
| **Total** | | **54 hrs core** (66.5 total includes buffer) | |

---

## Phase 1 — Credentials & `.env` Pattern

### What

Create `.env.example` at the repo root with all tunable variables and their documented dev defaults. Verify `.env` is in `.gitignore`. Add `n8n/workflows/*.credentials.json` to `.gitignore`.

### Why (from design)

Design §4.9 (D-5): The `.env` / `.env.example` pattern is the chosen credential strategy — non-sensitive dev defaults committed, developer-local `.env` gitignored. This must be the first phase because every subsequent phase references the env vars by name.

### Steps

**Step 1.1 — Verify `.gitignore` covers `.env` and n8n credentials**

- Read the current `.gitignore`
- Confirm `.env` is already present (it is — verified during planning)
- Add `n8n/workflows/*.credentials.json` to `.gitignore` if absent

**Test:** `git check-ignore -v .env` returns a match; `git check-ignore -v "n8n/workflows/test.credentials.json"` returns a match.

**Step 1.2 — Write `.env.example`**

Create `.env.example` at repo root containing all variables from design §4.9 with dev-safe defaults.

**Test:** File exists at repo root; all 14 variables from design §4.9 are present; no passwords, tokens, or real secrets appear in git diff for this file.

### Files touched

| File | Change |
|------|--------|
| `.gitignore` | Add `n8n/workflows/*.credentials.json` (if not present) |
| `.env.example` | **Create** — all 14 env vars with dev defaults |

### Not changed

- `.env` — this is developer-local, gitignored; only `.env.example` is committed.
- Any source code files — this phase is configuration only.

### Success criteria

- [ ] `.env.example` committed at repo root, contains all 14 variables matching design §4.9
- [ ] `.env` is in `.gitignore` (verified, not created)
- [ ] `n8n/workflows/*.credentials.json` is in `.gitignore`
- [ ] `git status` shows no `.env` file tracked
- [ ] No credentials appear in git diff for committed files

### Estimated hours: 4 hrs

---

## Phase 2 — PostgreSQL Init Script

### What

Create `docker/postgres/init.sql` with nine `CREATE DATABASE` statements — one per service. Remove the `docker/postgres/.gitkeep` placeholder.

### Why (from design)

Design §4.6 (D-11): The Postgres Docker image runs `/docker-entrypoint-initdb.d/*.sql` exactly once on first initialisation of an empty data volume. This is the only step needed to provision database-per-service (`.NET Coding Standards §13`). No schema, no extensions, no seed data.

### Steps

**Step 2.1 — Write `docker/postgres/init.sql`**

Create the file with nine `CREATE DATABASE` statements in the exact order from design §4.6. Plain SQL only (no `\c` or `psql`-specific meta-commands). No `IF NOT EXISTS` (design explicitly excludes it — re-runs must fail fast to force a `down -v` reset).

**Test (manual):** Run `docker compose up` with a fresh volume; connect with `psql -h localhost -U traverse -l`; confirm exactly nine service databases plus the default `postgres` database are listed.

**Automated test (Gherkin `gherkin_local_dev_env_database_provisioning.feature`):**
- Scenario: "init.sql creates all nine service databases on a fresh volume"
- Scenario Outline: "Each service database is individually reachable" (9 rows)
- Scenario: "init.sql does not re-run when the data volume already exists"

**Step 2.2 — Remove `docker/postgres/.gitkeep`**

Delete the placeholder `.gitkeep` now that a real file exists in `docker/postgres/`.

**Test:** `ls docker/postgres/` shows `init.sql` only; no `.gitkeep`.

### Files touched

| File | Change |
|------|--------|
| `docker/postgres/init.sql` | **Create** — 9 `CREATE DATABASE` statements |
| `docker/postgres/.gitkeep` | **Delete** — replaced by real file |

### Not changed

- Any Compose file (Phase 5 wires the volume mount).
- Any service source code.
- No schema or migration files — EF Core migrations are Phase 1 service concerns.

### Dummy code / placeholder handling

`docker/postgres/.gitkeep` is the placeholder removed by Step 2.2.

### Success criteria

- [ ] `docker/postgres/init.sql` exists with exactly 9 `CREATE DATABASE` statements
- [ ] Database names match design §4.6 exactly (case-sensitive)
- [ ] No `IF NOT EXISTS`, no schema creation, no table creation in the script
- [ ] `docker/postgres/.gitkeep` is removed from the repository
- [ ] Manual: `psql -h localhost -U traverse -l` after fresh `docker compose up` shows all 9 databases

### Risks / Compatibility

- **R-2 (init.sql one-time run):** README (Phase 6) documents the `down -v` workaround. No backward-compatibility concern — this is a new file on a new volume.

### Estimated hours: 8 hrs

---

## Phase 3 — RabbitMQ & n8n Compose Services

### What

Add `traverse-rabbitmq` and `traverse-n8n` services to `docker-compose.yml` (creating the file). Also create the `n8n/workflows/` directory with a `.gitkeep`.

### Why (from design)

Design §4.3 and §4.4 define these two infrastructure services. They are grouped in a single phase because they have no dependency on each other and neither depends on Postgres. Creating the compose file in this phase (rather than Phase 5) allows incremental `compose up` testing of the infra core before adding API stubs.

### Steps

**Step 3.1 — Create `n8n/workflows/.gitkeep`**

Create directory `n8n/workflows/` at repo root with a `.gitkeep` so git tracks it. This is the bind-mount target for n8n's workflow directory.

**Test:** `git ls-files n8n/workflows/.gitkeep` returns the path.

**Step 3.2 — Create `docker-compose.yml` with RabbitMQ and n8n services**

Create `docker-compose.yml` at repo root. Include:
- Top-level `networks: traverse-net: driver: bridge`
- Top-level `volumes: rabbitmq-data:`, `n8n-data:`
- `rabbitmq` service per design §4.3: image, ports, env vars, volume, healthcheck, network, restart policy
- `n8n` service per design §4.4: image, ports, env vars, named volume, bind mount `./n8n/workflows`, healthcheck, network, restart policy
- **No `version:` key** (D-13 — Docker Compose v2 deprecation warning)

**Test:** `docker compose config` exits 0 (validates YAML syntax and variable interpolation with env defaults).

**Step 3.3 — Verify RabbitMQ service manually**

**Tests (Gherkin `gherkin_local_dev_env_rabbitmq.feature`):**
- `docker compose up rabbitmq` (subset start)
- HTTP GET `http://localhost:15672/api/overview` with credentials `traverse:traverse_dev` → 200 OK
- TCP connect to `localhost:5672` → accepted
- Verify management dashboard vhost list shows `/`

**Step 3.4 — Verify n8n service manually**

**Tests (Gherkin `gherkin_local_dev_env_n8n.feature`):**
- `docker compose up n8n rabbitmq` (n8n requires neither postgres nor rabbitmq, but test in context)
- HTTP GET `http://localhost:5678/healthz` → 200 OK
- HTTP GET `http://localhost:5678` (browser): no login prompt visible
- `ls n8n/workflows/` on host → directory exists

### Files touched

| File | Change |
|------|--------|
| `docker-compose.yml` | **Create** — initial version with rabbitmq + n8n services, networks, volumes |
| `n8n/workflows/.gitkeep` | **Create** — empty placeholder so git tracks the directory |

### Not changed

- Postgres service (Phase 2 init.sql created; Compose wiring is Phase 5).
- API stub services (Phase 4).
- Source code.

### Success criteria

- [ ] `docker compose config` exits 0
- [ ] `docker compose up rabbitmq n8n` reaches `healthy` for both containers
- [ ] `http://localhost:15672` responds 200 with `traverse:traverse_dev`
- [ ] `http://localhost:5678/healthz` responds 200
- [ ] `n8n/workflows/.gitkeep` is committed
- [ ] No `version:` key in `docker-compose.yml`
- [ ] `traverse-net` custom bridge network defined
- [ ] Named volumes `rabbitmq-data` and `n8n-data` declared at top level

### Risks / Compatibility

- **R-3 (RabbitMQ slow-start):** Healthcheck uses `start_period: 30s`, `retries: 10` per design §4.3.
- **R-4 (n8n UID mismatch):** Deferred to README (Phase 6).

### Estimated hours: 8 hrs

---

## Phase 4 — API Stub Services (Profile-Gated)

### What

Add the nine API stub Compose service definitions to `docker-compose.yml` under the `api` profile. Create stub `Dockerfile`s for all nine services in their `docker/{service}/` directories. Remove the nine `.gitkeep` placeholder files from `docker/` service directories.

### Why (from design)

Design §4.5 and D-7: API services reference Dockerfiles that are fully produced by NT-005. Profile-gating with `--profile api` lets `docker compose up` succeed immediately (infra only) while `docker compose --profile api up` enables the full stack once NT-005 lands. The Dockerfile shape is fully specified in design §4.5 — this story authors them so NT-005 only needs to populate the corresponding `src/services/{service}/` projects.

### Steps

**Step 4.1 — Author per-service Dockerfiles (9 files)**

Create `docker/{service}/Dockerfile` for each of the nine services using the multi-stage pattern from design §4.5. Replace the `.gitkeep` placeholder in each directory. Services: workflow, kpi, admin, notifications, reporting, aicopilot, compliance, search, calendar.

Each Dockerfile:
- Stage 1 (`build`): `mcr.microsoft.com/dotnet/sdk:10.0`, copies `Traverse.sln`, `global.json`, `Directory.Build.props`, `src/`, runs `dotnet restore` then `dotnet publish -c Release` for the specific `.Api.csproj`
- Stage 2 (`runtime`): `mcr.microsoft.com/dotnet/aspnet:10.0`, non-root user `appuser`, `EXPOSE 8080`, `ENV ASPNETCORE_URLS=http://+:8080`, `HEALTHCHECK --interval=30s --timeout=10s CMD wget -qO- http://localhost:8080/health/live || exit 1`

**Test:** Each Dockerfile passes `docker compose --profile api config` (syntax validation). Actual build test is deferred to post-NT-005 (Dockerfiles cannot build until `.Api.csproj` files exist).

**Step 4.2 — Add nine API stub services to `docker-compose.yml`**

For each service, add a Compose service definition under `profiles: [api]` per design §4.5:
- `build.context: .`, `build.dockerfile: docker/{service}/Dockerfile`
- `container_name: traverse-{service}`
- `ports: ["{PORT}:8080"]`
- Connection string env var, RabbitMQ env vars, `ASPNETCORE_ENVIRONMENT: Development`
- `depends_on: { postgres: { condition: service_healthy }, rabbitmq: { condition: service_healthy } }`
- Healthcheck: `wget -qO- http://localhost:8080/health/live || exit 1`, interval `15s`, timeout `5s`, retries `10`, start_period `30s`
- `networks: [traverse-net]`
- `restart: unless-stopped`

**Test:** `docker compose --profile api config` exits 0.

**Step 4.3 — Remove nine `.gitkeep` files from `docker/{service}/`**

Each service directory gets a real `Dockerfile`, making the `.gitkeep` redundant.

**Test:** `git ls-files docker/workflow/.gitkeep` returns empty (removed from index).

### Files touched

| File | Change |
|------|--------|
| `docker/workflow/Dockerfile` | **Create** (multi-stage, Traverse.Workflow.Api) |
| `docker/kpi/Dockerfile` | **Create** (multi-stage, Traverse.KPI.Api) |
| `docker/admin/Dockerfile` | **Create** (multi-stage, Traverse.Admin.Api) |
| `docker/notifications/Dockerfile` | **Create** (multi-stage, Traverse.Notifications.Api) |
| `docker/reporting/Dockerfile` | **Create** (multi-stage, Traverse.Reporting.Api) |
| `docker/aicopilot/Dockerfile` | **Create** (multi-stage, Traverse.AICopilot.Api) |
| `docker/compliance/Dockerfile` | **Create** (multi-stage, Traverse.Compliance.Api) |
| `docker/search/Dockerfile` | **Create** (multi-stage, Traverse.Search.Api) |
| `docker/calendar/Dockerfile` | **Create** (multi-stage, Traverse.Calendar.Api) |
| `docker/{service}/.gitkeep` (×9) | **Delete** — replaced by real Dockerfile in each directory |
| `docker-compose.yml` | **Update** — append 9 profile-gated API stub services |

### Not changed

- `src/services/{service}/` directories — still empty placeholders (NT-005 fills them).
- `docker/postgres/` — not touched (handled in Phase 2).
- Infrastructure services (rabbitmq, n8n) in compose — already defined in Phase 3.

### Dummy code / placeholder handling

The nine `docker/{service}/.gitkeep` files are placeholder artefacts from NT-001. Each is removed when the real Dockerfile is created in Step 4.3.

### Success criteria

- [ ] `docker compose --profile api config` exits 0
- [ ] `docker compose config` (no profile) exits 0 and does NOT include API stub services
- [ ] All 9 Dockerfiles exist at `docker/{service}/Dockerfile`
- [ ] All 9 `docker/{service}/.gitkeep` files removed from repository
- [ ] Each API service definition has `profiles: [api]`
- [ ] Each API service has `depends_on` with `service_healthy` conditions for postgres and rabbitmq
- [ ] All 9 Dockerfiles use `.NET 10` SDK and runtime images (`mcr.microsoft.com/dotnet/sdk:10.0`, `mcr.microsoft.com/dotnet/aspnet:10.0`)
- [ ] All 9 Dockerfiles run as non-root user `appuser`

### Risks / Compatibility

- **R-1 (Dockerfiles without .Api.csproj):** Build test (`docker compose --profile api up`) deferred to post-NT-005. `docker compose config` (syntax only) can and must pass now.
- The Dockerfile `COPY ["src/", "src/"]` pattern copies entire `src/` tree — this is intentional since each service's `.csproj` may reference shared libraries; the full restore context is needed.

### Estimated hours: 16 hrs

---

## Phase 5 — PostgreSQL Service, Network, Volumes & Full Compose Wiring

### What

Add the `postgres` service definition to `docker-compose.yml`, declare the `postgres-data` named volume, and wire the postgres volume mount for `init.sql`. Then perform an end-to-end `docker compose up` (infra only) to validate the full composition.

### Why (from design)

Design §4.2, §4.1, §4.8 (D-6, D-8, D-9): The postgres service is the dependency anchor for the API stubs (all stubs `depends_on: postgres: service_healthy`). It is added after the init script (Phase 2) and after rabbitmq/n8n are validated (Phase 3) so integration is incremental. Named volumes for postgres-data are declared at the top level with the others.

### Steps

**Step 5.1 — Add postgres service to `docker-compose.yml`**

Add `postgres` service per design §4.2:
- Image: `postgres:latest`
- `container_name: traverse-postgres`
- Ports: `${POSTGRES_PORT:-5432}:5432`
- Env vars: `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_DB=postgres`
- Volume mounts: `postgres-data:/var/lib/postgresql/data` and `./docker/postgres/init.sql:/docker-entrypoint-initdb.d/init.sql:ro`
- Healthcheck: `CMD-SHELL pg_isready -U $${POSTGRES_USER}`, interval `10s`, timeout `5s`, retries `10`, start_period `15s`
- `networks: [traverse-net]`
- `restart: unless-stopped`

Declare `postgres-data:` in top-level `volumes:`.

**Test:** `docker compose config` exits 0. Service name `postgres` resolves as DNS within `traverse-net`.

**Step 5.2 — End-to-end infra `docker compose up` test**

With a fresh volume, run `docker compose up -d` (no `--profile api`). Verify:
- All 3 infra containers (`traverse-postgres`, `traverse-rabbitmq`, `traverse-n8n`) reach `healthy`
- API stubs are NOT started (profile-gated)
- `docker compose ps` shows 3 running services, no exited containers

**Tests (Gherkin `gherkin_local_dev_env_stack_startup.feature`):**
- Scenario: "docker compose up brings all containers to a healthy state within 90 seconds" (infra subset)

**Step 5.3 — Validate database provisioning**

After the infra-only `up`:
- Connect: `psql -h localhost -U traverse -l` → exactly 9 service databases + `postgres` default
- Each database: `psql -h localhost -U traverse -d traverse_workflow -c '\conninfo'` → succeeds

**Tests (Gherkin `gherkin_local_dev_env_database_provisioning.feature` — all 3 scenarios).**

**Step 5.4 — Validate env-override pattern**

With a `.env` that overrides `POSTGRES_PORT=5433`:
- `docker compose down -v && docker compose up -d`
- Verify postgres binds on 5433: `nc -zv localhost 5433`

**Test:** Port override works without any compose file change.

### Files touched

| File | Change |
|------|--------|
| `docker-compose.yml` | **Update** — add postgres service + `postgres-data` named volume |

### Not changed

- `docker/postgres/init.sql` — created in Phase 2; only mounted here.
- API stub service definitions — already added in Phase 4 (profile-gated).
- Source code.

### Success criteria

- [ ] `docker compose config` exits 0 (all services, all volumes, all networks declared)
- [ ] `docker compose up -d` (no profile): exactly 3 containers start and reach `healthy`
- [ ] `docker compose ps` shows `traverse-postgres`, `traverse-rabbitmq`, `traverse-n8n` as `running (healthy)`
- [ ] API stub containers are absent from `docker compose ps` (profile not activated)
- [ ] After fresh `up`: `psql -h localhost -U traverse -l` shows all 9 service databases
- [ ] `POSTGRES_PORT=5433` override binds postgres on 5433

### Risks / Compatibility

- **R-3 (RabbitMQ slow-start):** Healthcheck `start_period: 30s` already set in Phase 3.
- **R-5 (port conflicts):** All ports env-overridable; tested in Step 5.4.

### Estimated hours: 10 hrs

---

## Phase 6 — README Quickstart Expansion

### What

Replace the placeholder "Run Locally / Docker Compose setup is added in NT-004" section in `README.md` with the full quickstart documented in design §4.10. Extend the Port Map table. Add a Troubleshooting section.

### Why (from design)

AC-6 and design §4.10: A new developer must be able to clone, follow the README verbatim, and reach a working environment without any out-of-band knowledge. The README is a first-class deliverable for this story.

### Steps

**Step 6.1 — Write "Quickstart (Local Dev)" section**

Replace the existing placeholder prose under "Run Locally" with the full quickstart block from design §4.10:
- One-time setup (`cp .env.example .env`)
- Stack startup (`docker compose up` / `docker compose up -d`)
- Verification commands (`docker compose ps`, `curl`, browser links)
- Angular workspace commands (`npm install`, `ng serve --proxy-config proxy.conf.json`)
- Stop & clean-up commands (`down` vs `down -v`) with distinction explained

**Test:** README renders correctly; all commands are copy-pasteable verbatim.

**Step 6.2 — Update Port Map table**

Replace the existing Port Map table (which has placeholder "NT-005" status column) with the full port map from design §4.7, including env override column and both infra and API service rows.

**Test:** All 13 services listed; each has default port and `ENV_VAR` override name.

**Step 6.3 — Add Troubleshooting section**

Add the troubleshooting table from design §4.10 covering:
- Port 5432 already in use → `POSTGRES_PORT=5433`
- `init.sql` didn't run → `docker compose down -v`
- API stubs in restart loop → wait or check logs
- n8n login prompt appears → check `N8N_BASIC_AUTH_ACTIVE`
- Linux UID mismatch on n8n/workflows → `chown 1000:1000`
- Stale image → `docker compose up --build`

Also add the Security Note from design §4.10 (dev credentials local-only; n8n no-auth is local-only).

**Test:** Section exists; contains all 6 troubleshooting rows; Security Note present.

**Step 6.4 — Validate README scenarios**

**Tests (Gherkin `gherkin_local_dev_env_readme_quickstart.feature` — all 5 scenarios):**
- Scenario: "A developer following the README quickstart can start the full stack"
- Scenario: "A developer following the README quickstart can verify the stack is working"
- Scenario: "The Angular shell is reachable after following the ng serve instructions"
- Scenario: "The README documents how to stop and clean up the environment"
- Scenario: "The README documents how to override default ports and credentials"

Human-validation note: prose clarity reviewed by Dev Lead at code review (Step 10).

### Files touched

| File | Change |
|------|--------|
| `README.md` | **Update** — replace placeholder quickstart section; update Port Map; add Troubleshooting + Security Note |

### Not changed

- "Prerequisites" section — already accurate (Docker Desktop entry present).
- "Build the Backend" section — unchanged.
- "Project Layout" section — update only `docker/` description to reflect real Dockerfiles replacing `.gitkeep`.

### Success criteria

- [ ] README "Quickstart (Local Dev)" section contains all commands from design §4.10 verbatim
- [ ] Port Map table lists all 13 services (4 infra + 9 API) with default ports and env override names
- [ ] Troubleshooting table covers all 6 symptom rows from design §4.10
- [ ] Security Note present warning about dev credentials and n8n no-auth
- [ ] Linux UID mismatch mitigation documented (`chown 1000:1000 n8n/workflows`)
- [ ] `down` vs `down -v` distinction clearly explained
- [ ] All verification commands in README produce documented output when run against the live stack

---

## Gherkin → Phase Traceability

| Gherkin Feature File | AC | Phase(s) | Scenarios |
|---------------------|-----|----------|-----------|
| `gherkin_local_dev_env_stack_startup.feature` | AC-1 | Ph-5 (Step 5.2) | 1 |
| `gherkin_local_dev_env_database_provisioning.feature` | AC-2 | Ph-2 (Step 2.1), Ph-5 (Step 5.3) | 3 |
| `gherkin_local_dev_env_rabbitmq.feature` | AC-3 | Ph-3 (Steps 3.3–3.4) | 3 |
| `gherkin_local_dev_env_n8n.feature` | AC-4 | Ph-3 (Step 3.4) | 3 |
| `gherkin_local_dev_env_api_stubs.feature` | AC-5 | Ph-4 (Step 4.2), Ph-5 (Step 5.2) | 4 |
| `gherkin_local_dev_env_readme_quickstart.feature` | AC-6 | Ph-6 (Step 6.4) | 5 |

---

## Files Created / Modified Summary

| File | Phase | Action |
|------|-------|--------|
| `.gitignore` | Ph-1 | Update — add `n8n/workflows/*.credentials.json` |
| `.env.example` | Ph-1 | **Create** |
| `docker/postgres/init.sql` | Ph-2 | **Create** |
| `docker/postgres/.gitkeep` | Ph-2 | **Delete** |
| `docker-compose.yml` | Ph-3, Ph-4, Ph-5 | **Create** (Ph-3), Update (Ph-4, Ph-5) |
| `n8n/workflows/.gitkeep` | Ph-3 | **Create** |
| `docker/workflow/Dockerfile` | Ph-4 | **Create** |
| `docker/kpi/Dockerfile` | Ph-4 | **Create** |
| `docker/admin/Dockerfile` | Ph-4 | **Create** |
| `docker/notifications/Dockerfile` | Ph-4 | **Create** |
| `docker/reporting/Dockerfile` | Ph-4 | **Create** |
| `docker/aicopilot/Dockerfile` | Ph-4 | **Create** |
| `docker/compliance/Dockerfile` | Ph-4 | **Create** |
| `docker/search/Dockerfile` | Ph-4 | **Create** |
| `docker/calendar/Dockerfile` | Ph-4 | **Create** |
| `docker/{service}/.gitkeep` (×9) | Ph-4 | **Delete** |
| `README.md` | Ph-6 | Update |

**Total placeholder files removed:** 10 (1 postgres `.gitkeep` + 9 service `.gitkeep`s)

---

## SOLID Principles Applied

| Principle | Where Applied |
|-----------|--------------|
| **SRP** | Each phase does exactly one thing: credentials, database init, infra messaging/workflow services, API stub scaffolding, compose wiring, documentation |
| **OCP** | The compose profile pattern (`--profile api`) means infra services require no modification when API stubs are added/removed |
| **ISP** | Per-service Dockerfiles are independent artefacts — adding or changing one service's Dockerfile has no effect on any other service |
| **DIP** | API stubs depend on `postgres` and `rabbitmq` by service name (DNS abstraction), not by IP — the infra implementation is swappable |

(LSP is not directly applicable to Docker Compose / infrastructure configuration.)

---

## Clean Architecture Conformance

This story operates below the Clean Architecture boundary — it is infrastructure wiring only. No domain logic, application services, or repositories are touched. The compose file and Dockerfiles are infrastructure-layer artefacts that enable all Clean Architecture layers to run locally.

---

## Effort Estimate Detail (Checkpoint 3)

| | Value |
|--|--|
| Adjusted CU (from design doc §9) | 33.25 |
| hours_per_cu (project-baseline.json) | 2.0 |
| Total estimated hours | **66.5 hrs** |
| Confidence min (×0.70) | 46.55 hrs |
| Confidence max (×1.30) | 86.45 hrs |
| Actuals logged | 0 of 6 required (warm-up active) |

> **Warm-up period active** — calibration data is still accumulating (0 of 6 required actuals logged for northwoods-traverse). Actual bounds may be wider than ±20–30% until calibration stabilises.

---

<!-- Generated by skill: plan-implementation v3.4.0 | 2026-04-30 16:00 -->
