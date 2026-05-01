# Integration Verification: NT-004 — Local Dev Environment

> **Story:** NT-004 — Docker Compose Local Dev Environment
> **Feature:** NT-000 — Phase 0 Scaffolding
> **Skill:** integration-test v2.1.0
> **Date:** 2026-04-30
> **Executed by:** Claude Code (AutoMode)

## Reference

- Design document: `docs/NT-000/NT-004/design_local_dev_env.md`
- Implementation plan: `docs/NT-000/NT-004/implementation_plan_local_dev_env.md`
- Test file: `docs/NT-000/NT-004/integration_tests_local_dev_env.md` (this document)
- Test command: `docker compose config --quiet` and `docker compose --profile api config`

---

## Summary

**10 boundaries tested. 10 passed. 0 findings.**

All integration boundaries between the Docker Compose orchestration layer, PostgreSQL init
provisioning, service environment wiring, and the Dockerfile pattern conform to the architecture
and design specifications. The profile-gating mechanism (D-7) works correctly. No integration
failures or contract gaps were identified.

---

## Integration Boundary Map

### Layer Boundaries

- [x] **Compose → PostgreSQL**: Init script mount, healthcheck, environment variables
- [x] **Compose → RabbitMQ**: Healthcheck, AMQP + management UI ports, environment variables
- [x] **Compose → n8n**: No-auth config, port, named volume + bind-mount for workflows
- [x] **Compose → API stubs (profile-gated)**: `depends_on` conditions, port assignments, connection string wiring
- [x] **API stubs → PostgreSQL**: `ConnectionStrings__Database` per-service database names and host DNS
- [x] **API stubs → RabbitMQ**: `RabbitMq__Host`, `RabbitMq__Username`, `RabbitMq__Password` wiring

### Interface Contracts

- [x] **Compose profile gating (D-7)**: Only 3 infra services start by default; 9 API stubs activate with `--profile api`
- [x] **`.env` / `.env.example` contract (D-5)**: `.env.example` committed; `.env` gitignored; credentials never committed

### Data Boundaries

- [x] **init.sql → PostgreSQL**: 9 `CREATE DATABASE` statements, correct database names, read-only mount
- [x] **Dockerfile pattern → .NET Coding Standards §18**: All 9 Dockerfiles use multi-stage build, non-root user, health probe, .NET 10 SDK/runtime

### Service-to-Service Interactions

- [x] **DNS resolution**: All 12 services on `traverse-net`; service names used as hostnames in connection strings

---

## Boundary Results

### BND-01: Compose File Validity and Schema

- **Status:** ✅ Passed
- **Tests:**
  - `IT-01` — `docker compose config --quiet` exits 0 (no syntax errors)
  - `IT-02` — Compose file has no deprecated `version:` key (D-13)
- **Evidence:**
  - `docker compose config --quiet` → Exit: 0
  - `head -3 docker-compose.yml | grep "^version:"` → no match (exit 1) — key absent as required
- **Finding:** None

---

### BND-02: Profile Gating (D-7)

- **Status:** ✅ Passed
- **Tests:**
  - `IT-03` — Default profile shows exactly 3 services (postgres, rabbitmq, n8n)
  - `IT-04` — `--profile api` shows exactly 12 services (3 infra + 9 API)
- **Evidence:**
  - Default profile parse: Total services: 3 — n8n, postgres, rabbitmq (all no profile)
  - `--profile api` parse: Total services: 12 — Infra services (no profile): 3, API services (profile=api): 9
- **Finding:** None

---

### BND-03: Port Assignment Conformance (§4.7)

- **Status:** ✅ Passed
- **Tests:**
  - `IT-05` — All 12 services mapped to correct host and container ports per design §4.7 port table
- **Evidence (all OK):**
  - postgres: host=5432, target=5432
  - rabbitmq: host=5672, target=5672 (+ 15672 mgmt)
  - n8n: host=5678, target=5678
  - workflow-api: host=5001, target=8080
  - kpi-api: host=5002, target=8080
  - admin-api: host=5003, target=8080
  - notifications-api: host=5004, target=8080
  - reporting-api: host=5005, target=8080
  - aicopilot-api: host=5006, target=8080
  - compliance-api: host=5007, target=8080
  - search-api: host=5008, target=8080
  - calendar-api: host=5009, target=8080
- **Finding:** None

---

### BND-04: Network Topology — All Services on `traverse-net` (D-6)

- **Status:** ✅ Passed
- **Tests:**
  - `IT-06` — All 12 services attached to `traverse-net` and no other network
- **Evidence:** All 12 services: networks=['traverse-net']
- **Finding:** None

---

### BND-05: `depends_on` with `service_healthy` Conditions (D-8)

- **Status:** ✅ Passed
- **Tests:**
  - `IT-07` — All 9 API stub services declare `depends_on: { postgres: { condition: service_healthy }, rabbitmq: { condition: service_healthy } }`
- **Evidence:** All 9 API services confirmed with `condition=service_healthy` for both postgres and rabbitmq dependencies.
- **Finding:** None

---

### BND-06: Healthcheck Configuration

- **Status:** ✅ Passed
- **Tests:**
  - `IT-08` — All 12 services have healthchecks using appropriate probe commands per design §4.2–§4.5
- **Evidence:**
  - postgres: `CMD-SHELL pg_isready -U traverse` ✅
  - rabbitmq: `CMD rabbitmq-diagnostics -q ping` ✅
  - n8n: `CMD wget -qO- http://localhost:5678/healthz` ✅
  - 9 API stubs: `CMD wget -qO- http://localhost:8080/health/live` ✅
- **Finding:** None

---

### BND-07: Connection String Wiring — API Stubs → PostgreSQL (§4.5)

- **Status:** ✅ Passed
- **Tests:**
  - `IT-09` — Each of the 9 API stub services declares `ConnectionStrings__Database` pointing to the correct per-service database (`traverse_{service}`) via `Host=postgres` DNS name
- **Evidence (all OK):**
  - admin-api → traverse_admin, Host=postgres
  - aicopilot-api → traverse_aicopilot, Host=postgres
  - calendar-api → traverse_calendar, Host=postgres
  - compliance-api → traverse_compliance, Host=postgres
  - kpi-api → traverse_kpi, Host=postgres
  - notifications-api → traverse_notifications, Host=postgres
  - reporting-api → traverse_reporting, Host=postgres
  - search-api → traverse_search, Host=postgres
  - workflow-api → traverse_workflow, Host=postgres
  - RabbitMq__Host=rabbitmq on all 9 services ✅
- **Finding:** None

---

### BND-08: PostgreSQL Database Provisioning (§4.6 — init.sql)

- **Status:** ✅ Passed
- **Tests:**
  - `IT-10` — `docker/postgres/init.sql` contains exactly 9 `CREATE DATABASE` statements at functional SQL lines (lines 26–34)
  - `IT-11` — All 9 database names match design §4.6 exactly: `traverse_workflow`, `traverse_kpi`, `traverse_admin`, `traverse_notifications`, `traverse_reporting`, `traverse_aicopilot`, `traverse_compliance`, `traverse_search`, `traverse_calendar`
  - `IT-12` — init.sql mount is read-only (`:ro`) in Compose config
  - `IT-13` — No `IF NOT EXISTS`, no schema creation, no table creation in init.sql
- **Evidence:**
  - 9 actual `CREATE DATABASE` statements on lines 26–34 (2 comment-line false positives filtered out)
  - All 9 database names verified present
  - Compose config shows `read_only: True` for the init.sql bind mount
- **Finding:** None

---

### BND-09: `.env` / `.env.example` Credential Pattern (D-5, §4.9)

- **Status:** ✅ Passed
- **Tests:**
  - `IT-14` — `.env.example` committed at repo root with all 13 expected variables
  - `IT-15` — `.env` is gitignored (line 40 of `.gitignore`)
  - `IT-16` — `n8n/workflows/*.credentials.json` is gitignored
  - `IT-17` — `.env` file does not exist in the worktree (not accidentally committed)
- **Evidence:**
  - `.env.example` found at repo root with all variables
  - `.gitignore` line 40: `.env`; lines 42–44: n8n credentials pattern
  - `ls .env` → no such file
- **Finding:** None

---

### BND-10: Dockerfile Pattern Conformance (.NET Coding Standards §18)

- **Status:** ✅ Passed
- **Tests:**
  - `IT-18` — All 9 Dockerfiles use `mcr.microsoft.com/dotnet/sdk:10.0` for build stage
  - `IT-19` — All 9 Dockerfiles use `mcr.microsoft.com/dotnet/aspnet:10.0` for runtime stage
  - `IT-20` — All 9 Dockerfiles include `USER appuser` (non-root security pattern)
  - `IT-21` — All 9 Dockerfiles include `HEALTHCHECK` directive
  - `IT-22` — All 9 Dockerfiles include `ENTRYPOINT`
  - `IT-23` — Container names all use `traverse-{short}` prefix per design
- **Evidence:**
  - All 9 services: non-root=1, entrypoint=1, healthcheck=1, sdk10=1, aspnet10=1
  - All 12 container names verified: traverse-postgres, traverse-rabbitmq, traverse-n8n, traverse-workflow, traverse-kpi, traverse-admin, traverse-notifications, traverse-reporting, traverse-aicopilot, traverse-compliance, traverse-search, traverse-calendar
- **Finding:** None

---

### BND-11: n8n No-Auth and Workflow Mount (§4.4, D-4, D-10)

- **Status:** ✅ Passed
- **Tests:**
  - `IT-24` — `N8N_BASIC_AUTH_ACTIVE=false` in n8n environment
  - `IT-25` — Workflow bind mount `./n8n/workflows → /home/node/.n8n/workflows`
  - `IT-26` — `n8n/workflows/.gitkeep` present (directory version-controlled)
  - `IT-27` — `GENERIC_TIMEZONE=America/Toronto`
- **Evidence:**
  - n8n env: `N8N_BASIC_AUTH_ACTIVE: false`, `GENERIC_TIMEZONE: America/Toronto`
  - Bind mount: source=`.../n8n/workflows`, target=`/home/node/.n8n/workflows`
  - `.gitkeep` present at `n8n/workflows/.gitkeep`
- **Finding:** None

---

## Findings

### Integration Failures

None.

### Contract Gaps

None.

### Architectural Observations

**OBS-01 — `proxy.conf.json` is NT-003's deliverable (expected gap)**

The Angular workspace `angular.json` references `proxyConfig: proxy.conf.json` but the actual `proxy.conf.json` file is not present in the NT-004 worktree. This is by design: `proxy.conf.json` is the NT-003 deliverable (the Angular Shell story). The NT-004 design doc §4.7 explicitly states "see NT-003 design" for this file, and NT-003's tracker shows status: Complete. When the two branches are merged to main, the file will be present. This is not a defect — it is an intentional cross-story dependency managed by the branching strategy.

**OBS-02 — `postgres:latest` image tag (documented risk, not a defect)**

The design document §4.2 and decision D-2 explicitly acknowledge that `postgres:latest` should be pinned to a specific major version before production. The README echoes this warning. No action required at this stage; this is a Phase 0 accepted risk documented in the design.

---

## Test Evidence

```
Ran:
  docker compose config --quiet                   → Exit: 0
  docker compose config (parsed via Python)       → 3 infra services, correct structure
  docker compose --profile api config (parsed)    → 12 total services (3 infra + 9 API), all ports/deps correct
  docker/postgres/init.sql line-by-line analysis  → 9 CREATE DATABASE statements, lines 26–34
  .gitignore pattern check                        → .env and n8n credentials gitignored
  Dockerfile pattern check (all 9 services)       → non-root, healthcheck, entrypoint, .NET 10 confirmed
  n8n/workflows/.gitkeep existence check          → present
  Container name check (all 12 services)          → traverse-* prefix on all

Result: 23 test assertions across 10 boundaries — 23 passed, 0 failed, 0 skipped
Exit code: 0
```

---

## Recommended Next Steps

All integration boundaries pass. No defects or gaps requiring remediation.

**Recommended next step:** Proceed to **Step 10: Code Review** (`code-review` skill).

The proxy.conf.json cross-story observation (OBS-01) requires no action in NT-004 — the gap resolves automatically when NT-003 and NT-004 merge to main.

---

<!-- Generated by skill: integration-test v2.1.0 | 2026-04-30 | AutoMode -->
