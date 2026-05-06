# Implementation Plan: NT-005 — API Service Stubs + Dockerfiles

## Overview

Creates nine empty ASP.NET Core Web API projects (one per PRD module), registers them in `Traverse.sln`, replaces the nine 2-stage NT-004 Dockerfiles with the 4-stage pattern required by AC-3, and verifies that `dotnet build Traverse.sln` succeeds and that `docker compose --profile api up` starts all nine services healthy.

## Reference

- Design document: `design_api_stubs.md`
- Gherkin specs: `gherkin_api_stubs_solution_build.feature`, `gherkin_api_stubs_health_endpoints.feature`, `gherkin_api_stubs_dockerfiles.feature`, `gherkin_api_stubs_compose.feature`
- Architecture: `../Northwoods_Traverse-feature-NT-000/docs/NT-000/architecture_phase0.md`
- Feature worktree: `/Users/kendrewpeacey/Projects/Northwoods_Traverse-feature-NT-000/`

## Risk Assessment

| Risk Category | Risk | Affected Phases | Mitigation |
|---|---|---|---|
| **Technical** | `AddTraverseAuth(builder.Configuration, builder.Environment)` takes `IWebHostEnvironment` — the `Program.cs` template passes `builder.Environment` which is `IWebHostEnvironment`; verify type compatibility | Phase 2 | Auth extension verified against live source — signature matches. Low residual risk. |
| **Technical** | NT-004 Dockerfiles contain `COPY ["Directory.Build.props", "./"]` referencing a file that does not exist in the repository | Phase 3 | NT-005 replaces all 9 Dockerfiles — this defect is automatically corrected by the replacement. Document explicitly. |
| **Technical** | `dotnet sln add` must run from the repository root so relative paths are recorded correctly — running from subdirectory produces wrong paths in `.sln` | Phase 1 | Each step instruction calls `dotnet sln add` from the feature worktree root explicitly. |
| **Functional** | `TreatWarningsAsErrors` on all projects means any inadvertent `FrameworkReference` duplicate (NU1510) or missing `using` causes a CI break | Phase 2 | `.csproj` template explicitly omits `FrameworkReference`; verified against EC-4 in the design. |
| **Blast radius** | The nine projects and Dockerfiles are independent units — a problem in one does not affect the others | All phases | Phases are decomposed per-service in groups so partial completion is verifiable. |
| **Compatibility** | `docker compose --profile api up` was defined by NT-004 — NT-005 must not modify `docker-compose.yml` | Phase 3 | `docker-compose.yml` is explicitly excluded from all phases. The Dockerfiles it references are created only. |

## Effort Estimate (Checkpoint 3)

| Input | Value |
|---|---|
| Adjusted CU (from `design_api_stubs.md` §11) | 52.5 |
| `hours_per_cu` (from `project-baseline.json`) | 2.0 |
| **Total story hours** | **105.0 hours** |
| Confidence range (±30%) | 73.5 – 136.5 hours |

> **Warm-up period active** — calibration data is still accumulating (0 of 6 required actuals logged for northwoods-traverse). Actual bounds may be wider than ±30% until calibration stabilises.

### Phase Allocation

| Phase | Description | Allocated Hours | Basis |
|---|---|---|---|
| Phase 1 | Create 9 × `.csproj` files + register in solution | 25 h | 9 new projects; moderate complexity (ProjectReferences, sln registration) |
| Phase 2 | Create 9 × `Program.cs` + `appsettings.json` | 25 h | Template application; infrastructure wiring per project |
| Phase 3 | Replace 9 × NT-004 Dockerfiles with 4-stage template | 20 h | Replace existing files; 4-stage pattern per .NET Coding Standards §18 |
| Phase 4 | Solution build verification + full smoke test | 35 h | `dotnet build`; `docker compose` smoke test; AC-1/2/3/4 verification |
| **Total** | | **105 h** | |

---

## Phases

---

### Phase 1: Create Nine API Project Files and Register in Solution

**What:** For each of the nine services, create the project folder structure, the `.csproj` file referencing all six shared infrastructure libraries, and register the project in `Traverse.sln` under the `services` solution folder.

**Why:** Without `.csproj` files, `dotnet build Traverse.sln` does not build any service code (AC-1). Without `dotnet sln add`, the projects are invisible to the solution. The six `ProjectReference` entries wire the shared library dependency graph so every Phase 1 story inherits infrastructure without discovering it.

**Steps** (smallest testable units):

1. Create `src/services/workflow/Traverse.Workflow.Api/Traverse.Workflow.Api.csproj` using the template from design §5.2 with Workflow substitutions from §5.1.
2. Run `dotnet sln Traverse.sln add --solution-folder services src/services/workflow/Traverse.Workflow.Api/Traverse.Workflow.Api.csproj` from the feature worktree root.
3. Verify: `dotnet sln Traverse.sln list` includes `Traverse.Workflow.Api`.
4. Repeat steps 1–3 for KPI, Admin, Notifications, Reporting, AICopilot, Compliance, Search, Calendar (substituting service-specific values from design §5.1 for each).
5. After all 9 projects are added, verify: `dotnet sln Traverse.sln list` shows all 17 projects (8 shared + 9 API stubs).
6. Run `dotnet restore Traverse.sln` from the feature worktree root to confirm all ProjectReference paths resolve and NuGet packages restore without error.

**Files touched:**

| File | Change |
|---|---|
| `src/services/workflow/Traverse.Workflow.Api/Traverse.Workflow.Api.csproj` | **NEW** — `Microsoft.NET.Sdk.Web`; `net10.0`; `Nullable=enable`; `TreatWarningsAsErrors=true`; `NoWarn=CS1591`; 6 × `ProjectReference` to `../../../../shared/` |
| `src/services/kpi/Traverse.KPI.Api/Traverse.KPI.Api.csproj` | **NEW** — same template, KPI substitutions |
| `src/services/admin/Traverse.Admin.Api/Traverse.Admin.Api.csproj` | **NEW** — same template, Admin substitutions |
| `src/services/notifications/Traverse.Notifications.Api/Traverse.Notifications.Api.csproj` | **NEW** — same template, Notifications substitutions |
| `src/services/reporting/Traverse.Reporting.Api/Traverse.Reporting.Api.csproj` | **NEW** — same template, Reporting substitutions |
| `src/services/aicopilot/Traverse.AICopilot.Api/Traverse.AICopilot.Api.csproj` | **NEW** — same template, AICopilot substitutions |
| `src/services/compliance/Traverse.Compliance.Api/Traverse.Compliance.Api.csproj` | **NEW** — same template, Compliance substitutions |
| `src/services/search/Traverse.Search.Api/Traverse.Search.Api.csproj` | **NEW** — same template, Search substitutions |
| `src/services/calendar/Traverse.Calendar.Api/Traverse.Calendar.Api.csproj` | **NEW** — same template, Calendar substitutions |
| `Traverse.sln` | **MODIFIED** — 9 × `Project(...)` entries added under the `services` solution folder GUID `{5968FBED-DF69-4CE6-8CAF-364A27DD447A}` via `dotnet sln add` |

**Not changed:**
- `src/shared/` — all six shared infrastructure projects exist and compile correctly; Phase 1 only adds consumers, never modifies providers.
- `docker-compose.yml` — NT-004 owns this file; service entries already exist and are not modified by NT-005.
- `src/services/{service}/.gitkeep` — removed implicitly when the project subdirectory is created (git will untrack the empty placeholder).

**Tests (per step):**

- **Step 3 (after each project):** `dotnet sln Traverse.sln list | grep "{ProjectName}"` — project appears in the list.
- **Step 5 (all 9 added):** `dotnet sln Traverse.sln list | wc -l` — output shows 17 projects.
- **Step 6 (restore):** `dotnet restore Traverse.sln 2>&1 | grep -E "error|Error"` — zero errors; all packages restore.

Happy path: all six `ProjectReference` relative paths resolve to `src/shared/` libs.
Edge case: `Directory.Build.props` does not exist in the repository — the `.csproj` template does not reference it, so this is not a risk in Phase 1 (it is a defect only in the NT-004 Dockerfiles, corrected in Phase 3).
Error case: if `dotnet restore` produces `NU1008` (missing project), the relative path `..\..\..\..\shared\` is incorrect — recount directory levels from `src/services/{service}/{ProjectName}/` to `src/shared/` (4 levels up).

**Database migrations:** None.

**Success criteria:**
- [ ] All 9 `.csproj` files exist at the paths listed in design §5.1 `{SrcRelPath}/{ProjectName}.csproj`
- [ ] `dotnet sln Traverse.sln list` shows exactly 17 projects
- [ ] `dotnet restore Traverse.sln` exits with code 0 and zero errors
- [ ] No `Directory.Build.props` is referenced in any `.csproj` file (it does not exist in the repository)
- [ ] No `<FrameworkReference Include="Microsoft.AspNetCore.App" />` present in any `.csproj` (NU1510 prevention, EC-4)

**Risks/Compatibility:**
- `dotnet sln add` modifies `Traverse.sln` in-place. Running from a subdirectory produces wrong paths. All `dotnet sln add` commands must be run from the feature worktree root (`/Users/kendrewpeacey/Projects/Northwoods_Traverse-feature-NT-000/`).
- The `services` solution folder GUID `{5968FBED-DF69-4CE6-8CAF-364A27DD447A}` is already present in `Traverse.sln` — `dotnet sln add --solution-folder services` will locate it automatically.

**Dummy code handling:** None in this phase — `.gitkeep` files are removed by git when the service subdirectories gain real content.

---

### Phase 2: Create Program.cs and appsettings.json for All Nine Services

**What:** For each of the nine services, create `Program.cs` (wiring the six shared infrastructure extension methods in the correct pipeline order) and `appsettings.json` (connection string and RabbitMQ placeholders).

**Why:** Without `Program.cs`, the Web SDK's implicit entry point requirement is unmet — `dotnet build` will emit a CS5001 error (no `Main` method). Without `appsettings.json`, `AddTraverseAuth` and `AddTraverseMessaging` will fail at startup when the configuration sections are absent. These two files together produce a compilable, startable service stub.

**Steps** (smallest testable units):

1. Create `src/services/workflow/Traverse.Workflow.Api/Program.cs` using the template from design §5.3, substituting `{ServiceName}` = `workflow-service` and the correct namespace from design §5.1.
2. Create `src/services/workflow/Traverse.Workflow.Api/appsettings.json` using the template from design §5.4, substituting `{DbName}` = `traverse_workflow`.
3. Verify partial build: `dotnet build src/services/workflow/Traverse.Workflow.Api/Traverse.Workflow.Api.csproj --no-restore 2>&1 | grep -E "error|warning|succeeded"` — must report `Build succeeded` with 0 errors and 0 warnings.
4. Repeat steps 1–3 for KPI, Admin, Notifications, Reporting, AICopilot, Compliance, Search, Calendar (substituting service-specific values for each).
5. After all 9 projects have `Program.cs` and `appsettings.json`, run `dotnet build Traverse.sln --no-incremental 2>&1 | grep -E "Error|Warning|Build succeeded"` — must show `Build succeeded`, `0 Error(s)`, `0 Warning(s)`.

**Files touched:**

| File | Change |
|---|---|
| `src/services/workflow/Traverse.Workflow.Api/Program.cs` | **NEW** — infrastructure wiring; `{ServiceName}` = `workflow-service` |
| `src/services/workflow/Traverse.Workflow.Api/appsettings.json` | **NEW** — `{DbName}` = `traverse_workflow`; RabbitMQ placeholders |
| `src/services/kpi/Traverse.KPI.Api/Program.cs` | **NEW** — `{ServiceName}` = `kpi-service` |
| `src/services/kpi/Traverse.KPI.Api/appsettings.json` | **NEW** — `{DbName}` = `traverse_kpi` |
| `src/services/admin/Traverse.Admin.Api/Program.cs` | **NEW** — `{ServiceName}` = `admin-service` |
| `src/services/admin/Traverse.Admin.Api/appsettings.json` | **NEW** — `{DbName}` = `traverse_admin` |
| `src/services/notifications/Traverse.Notifications.Api/Program.cs` | **NEW** — `{ServiceName}` = `notifications-service` |
| `src/services/notifications/Traverse.Notifications.Api/appsettings.json` | **NEW** — `{DbName}` = `traverse_notifications` |
| `src/services/reporting/Traverse.Reporting.Api/Program.cs` | **NEW** — `{ServiceName}` = `reporting-service` |
| `src/services/reporting/Traverse.Reporting.Api/appsettings.json` | **NEW** — `{DbName}` = `traverse_reporting` |
| `src/services/aicopilot/Traverse.AICopilot.Api/Program.cs` | **NEW** — `{ServiceName}` = `aicopilot-service` |
| `src/services/aicopilot/Traverse.AICopilot.Api/appsettings.json` | **NEW** — `{DbName}` = `traverse_aicopilot` |
| `src/services/compliance/Traverse.Compliance.Api/Program.cs` | **NEW** — `{ServiceName}` = `compliance-service` |
| `src/services/compliance/Traverse.Compliance.Api/appsettings.json` | **NEW** — `{DbName}` = `traverse_compliance` |
| `src/services/search/Traverse.Search.Api/Program.cs` | **NEW** — `{ServiceName}` = `search-service` |
| `src/services/search/Traverse.Search.Api/appsettings.json` | **NEW** — `{DbName}` = `traverse_search` |
| `src/services/calendar/Traverse.Calendar.Api/Program.cs` | **NEW** — `{ServiceName}` = `calendar-service` |
| `src/services/calendar/Traverse.Calendar.Api/appsettings.json` | **NEW** — `{DbName}` = `traverse_calendar` |

**Not changed:**
- `src/shared/` — all shared extension methods are consumed as-is; no modifications.
- `Traverse.sln` — project registrations were completed in Phase 1; no further `.sln` changes.
- `docker-compose.yml` — NT-004 owns this file; not modified.

**Middleware pipeline order (mandatory — enforced per design §3):**

The `Program.cs` template must register and activate middleware in this exact order to comply with ASP.NET Core conventions and the design specification:

**Registration phase (before `builder.Build()`):**
1. `builder.AddTraverseObservability("{ServiceName}")` — Serilog + OTel (FIRST: captures startup logs)
2. `builder.Services.AddHealthChecks().AddTraverseHealthChecks()` — liveness check
3. `builder.Services.AddTraverseHttp()` — correlation ID middleware + global exception handler
4. `builder.Services.AddTraverseAuth(builder.Configuration, builder.Environment)` — JWT Bearer stub
5. `builder.Services.AddAuthorization()` — required by `app.UseAuthorization()`
6. `builder.Services.AddTraverseMessaging(builder.Configuration)` — MassTransit/RabbitMQ
7. `builder.Services.AddOpenApi()` — Scalar/OpenAPI document

**Pipeline phase (after `app = builder.Build()`):**
1. `app.UseTraverseHttp()` — CorrelationIdMiddleware + UseExceptionHandler (FIRST: tag all downstream logs)
2. `app.UseAuthentication()` — JWT validation (after exception handler, before auth)
3. `app.UseAuthorization()` — policy enforcement
4. `app.MapTraverseHealthChecks()` — `/health/live` and `/health/ready` with AllowAnonymous
5. `if (app.Environment.IsDevelopment()) { app.MapOpenApi(); }` — development-only OpenAPI spec

**LL-009 Compliance Note:** No `inject()` calls are present in this `Program.cs` — it is a non-Angular .NET file. This note is for completeness; the LL-009 lesson applies to Angular interceptors only.

**Tests (per step):**

- **Step 3 (per project — partial build):** `dotnet build {SrcRelPath}/{ProjectName}.csproj --no-restore` — `Build succeeded`, `0 Error(s)`, `0 Warning(s)`.
- **Step 5 (full solution build):** `dotnet build Traverse.sln --no-incremental` — `Build succeeded`, `0 Error(s)`, `0 Warning(s)`.

Happy path: all 9 `Program.cs` files compile against the shared extension methods; method signatures match.
Edge case: `AddTraverseAuth` parameter type — design §3 shows `builder.Services.AddTraverseAuth(builder.Configuration, builder.Environment)`. Verified against live source: `AddTraverseAuth(IServiceCollection, IConfiguration, IWebHostEnvironment)`. `builder.Environment` is `IWebHostEnvironment`. Types match — no issue.
Error case: CS0246 (type not found) — indicates a missing `using` directive. The `Program.cs` template includes explicit `using` directives for all four extension namespaces (`Traverse.Infrastructure.Auth.Extensions`, `Traverse.Infrastructure.Http.Extensions`, `Traverse.Infrastructure.Messaging.Extensions`, `Traverse.Infrastructure.Observability.Extensions`).

**Database migrations:** None.

**Success criteria:**
- [ ] All 9 `Program.cs` files exist at `{SrcRelPath}/Program.cs`
- [ ] All 9 `appsettings.json` files exist at `{SrcRelPath}/appsettings.json`
- [ ] Per-project partial build (`dotnet build {csproj} --no-restore`) succeeds for each of the 9 projects: `Build succeeded`, `0 Error(s)`, `0 Warning(s)`
- [ ] Full solution build (`dotnet build Traverse.sln --no-incremental`) reports `Build succeeded`, `0 Error(s)`, `0 Warning(s)` — satisfies AC-1
- [ ] Middleware registration order matches design §3 exactly (verified by code review)
- [ ] `Auth:Authority` is empty string in every `appsettings.json` (Phase 0 placeholder, per design §5.4 notes)
- [ ] `Otel:Endpoint` is empty string in every `appsettings.json` (Phase 0 placeholder)
- [ ] No `Traverse.AI.Abstractions` or `Traverse.AI.Providers` is referenced in any `Program.cs` or `.csproj` (EC-5, YAGNI)

**Risks/Compatibility:**
- `TreatWarningsAsErrors=true` is set on all 9 projects. Any inadvertent warning is a build error. The most likely warnings are CS1591 (XML documentation) — suppressed by `<NoWarn>CS1591</NoWarn>` in the `.csproj` template. No other warnings are expected in a template-only `Program.cs`.
- The `AddAuthorization()` call is a plain ASP.NET Core call (not from a shared library); it requires `Microsoft.AspNetCore.Authorization` which is part of the `Microsoft.AspNetCore.App` framework reference. The Web SDK provides this automatically via `FrameworkReference` — no explicit package needed.

**Dummy code handling:** None — no placeholder or TODO code is introduced. Every extension method called in `Program.cs` has a real implementation in the shared infrastructure libraries.

---

### Phase 3: Replace NT-004 Dockerfiles with 4-Stage Template

**What:** Replace all 9 existing NT-004 2-stage Dockerfiles (at `docker/{service}/Dockerfile`) with the 4-stage multi-stage build template from design §5.5. The NT-004 Dockerfiles are pre-release stubs — they must be fully replaced, not patched.

**Why (Audit Finding):** NT-004 created 9 stub Dockerfiles with a 2-stage pattern (`build` → `runtime`) to allow `docker compose config` validation before NT-005 existed. Per design §5.5 and Gherkin AC-3, each Dockerfile must define exactly four stages: `restore` → `build` → `publish` → `runtime`. The NT-004 stubs also contain a defect: `COPY ["Directory.Build.props", "./"]` references a file that does not exist in the repository — this causes `docker build` to fail with `COPY failed: file not found`. Replacing the Dockerfiles corrects this defect automatically. See decision log entry #7 in the story tracker.

**Defect Detail — NT-004 Dockerfile:**
```dockerfile
# NT-004 stub (DEFECTIVE — 2 stages, references non-existent Directory.Build.props)
COPY ["Traverse.sln", "global.json", "Directory.Build.props", "./"]  # ← Directory.Build.props does not exist
```
The NT-005 4-stage template does not include `Directory.Build.props` — it copies only `global.json` and `Traverse.sln`.

**Steps** (smallest testable units):

1. Overwrite `docker/workflow/Dockerfile` with the 4-stage template from design §5.5, substituting `{SrcRelPath}` = `src/services/workflow/Traverse.Workflow.Api`, `{ProjectName}` = `Traverse.Workflow.Api`, `{ServiceFolder}` = `workflow`.
2. Verify: inspect `docker/workflow/Dockerfile` — confirms `FROM ... AS restore`, `FROM restore AS build`, `FROM build AS publish`, `FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime` are present (4 stages).
3. Repeat step 1 for KPI, Admin, Notifications, Reporting, AICopilot, Compliance, Search, Calendar (substituting service-specific values from design §5.1 for each).
4. Verify all 9: `grep -l "AS restore" docker/*/Dockerfile | wc -l` — output is `9`.
5. Verify `Directory.Build.props` is not referenced: `grep -r "Directory.Build.props" docker/` — empty output (no matches).
6. Verify the `ENTRYPOINT` for each Dockerfile uses the correct DLL name: e.g., `docker/workflow/Dockerfile` ends with `ENTRYPOINT ["dotnet", "Traverse.Workflow.Api.dll"]`.

**Files touched:**

| File | Change |
|---|---|
| `docker/workflow/Dockerfile` | **REPLACE** — 2-stage NT-004 stub → 4-stage NT-005 template (design §5.5) |
| `docker/kpi/Dockerfile` | **REPLACE** — same |
| `docker/admin/Dockerfile` | **REPLACE** — same |
| `docker/notifications/Dockerfile` | **REPLACE** — same |
| `docker/reporting/Dockerfile` | **REPLACE** — same |
| `docker/aicopilot/Dockerfile` | **REPLACE** — same |
| `docker/compliance/Dockerfile` | **REPLACE** — same |
| `docker/search/Dockerfile` | **REPLACE** — same |
| `docker/calendar/Dockerfile` | **REPLACE** — same |

**Not changed:**
- `docker-compose.yml` — NT-004 owns this file. The compose entries reference the Dockerfile paths created here; the entries themselves do not change. This is a firm scope boundary per design §4 and the decision log.
- `docker/postgres/init.sql` — database init script created by NT-004; not touched.
- `src/` — Dockerfiles reference source paths; source is not modified by Dockerfile changes.

**Dockerfile 4-stage structure (per design §5.5 — mandatory):**

```
Stage 1 (restore):  FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
                    COPY global.json, Traverse.sln
                    COPY all 6 shared .csproj files
                    COPY {SrcRelPath}/{ProjectName}.csproj
                    RUN dotnet restore {SrcRelPath}/{ProjectName}.csproj

Stage 2 (build):    FROM restore AS build
                    COPY src/shared/ src/shared/
                    COPY {SrcRelPath}/ {SrcRelPath}/
                    RUN dotnet build -c Release --no-restore

Stage 3 (publish):  FROM build AS publish
                    RUN dotnet publish -c Release --no-build /p:UseAppHost=false -o /app/publish

Stage 4 (runtime):  FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
                    RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
                    USER appuser
                    COPY --from=publish /app/publish .
                    EXPOSE 8080
                    ENV ASPNETCORE_URLS=http://+:8080
                    ENTRYPOINT ["dotnet", "{ProjectName}.dll"]
```

**Tests (per step):**

- **Step 2 (per Dockerfile — stage count check):** `grep "^FROM" docker/workflow/Dockerfile | wc -l` — output is `4`.
- **Step 4 (all 9):** `grep -l "AS restore" docker/*/Dockerfile | wc -l` — output is `9`.
- **Step 5 (defect correction check):** `grep -r "Directory.Build.props" docker/` — empty (no match confirms defect is corrected).

Happy path: each 4-stage Dockerfile references the correct `{SrcRelPath}` and `{ProjectName}` from design §5.1.
Edge case: the `adduser` vs `useradd` command — design §5.5 uses `adduser --disabled-password --gecos ""` (Alpine-compatible). The `mcr.microsoft.com/dotnet/aspnet:10.0` base image is Debian/Ubuntu-based and supports both. The design's `adduser` form is used as specified.
Error case: if `docker build` fails with "COPY failed", the `{SrcRelPath}` substitution in the Dockerfile is wrong — verify against the table in design §5.1.

**Note:** Actual `docker build` is not executed in Phase 3 — it is deferred to Phase 4 smoke testing. The verification steps in Phase 3 are structural checks only (grep-based). The full AC-3 verification requires Docker daemon access and is performed in Phase 4.

**Database migrations:** None.

**Success criteria:**
- [ ] All 9 Dockerfiles define exactly 4 stages (`FROM ... AS restore`, `FROM restore AS build`, `FROM build AS publish`, `FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime`)
- [ ] `grep -r "Directory.Build.props" docker/` returns empty output (NT-004 defect corrected)
- [ ] Each Dockerfile `ENTRYPOINT` references the correct `.dll` name for its service (e.g., `Traverse.Workflow.Api.dll`)
- [ ] `docker-compose.yml` is unchanged from its NT-004 state
- [ ] Each Dockerfile copies from the correct `{SrcRelPath}` per design §5.1

**Risks/Compatibility:**
- The NT-004 Dockerfiles are pre-release stubs explicitly designed to be replaced by NT-005. No other phase or story depends on the 2-stage stub format. The replacement is a clean overwrite with no compatibility concern.
- Docker cache: if a developer has previously run `docker build` with the NT-004 stub, their local cache will be invalidated by the Dockerfile change. This is expected and correct behaviour — `--no-cache` is specified in AC-3 verification.

**Dummy code handling:** The NT-004 2-stage Dockerfiles are explicitly dummy/pre-release code that NT-005 was always intended to replace. The full replacement (not patching) is the correct approach per the design §1 problem statement.

---

### Phase 4: Solution Build Verification and Smoke Testing

**What:** Run the full `dotnet build Traverse.sln` to satisfy AC-1, then execute `docker compose --profile api up` to verify all nine containers start healthy (AC-2, AC-3, AC-4).

**Why:** Phases 1–3 create all the files but do not verify the complete integration: that all 9 projects compile as a solution, that the Docker images build correctly from the 4-stage Dockerfiles, that the services start and pass health checks, and that the `docker-compose.yml` correctly wires the Dockerfiles to running containers. Phase 4 is the acceptance verification gate.

**Steps** (smallest testable units — in dependency order):

1. **Solution build (AC-1):** From the feature worktree root, run `dotnet build Traverse.sln --no-incremental`. Verify: output contains `Build succeeded`, `0 Error(s)`, `0 Warning(s)`.
2. **Solution project count (AC-1):** `dotnet sln Traverse.sln list | wc -l` — output is `17`.
3. **Single Dockerfile build (AC-3 sample):** `docker build -f docker/workflow/Dockerfile . --target runtime --no-cache` from the feature worktree root. Verify: exit code 0, no build errors. (Sample one service to validate the 4-stage pattern before running all 9.)
4. **All 9 Dockerfile builds (AC-3):** Repeat `docker build -f docker/{service}/Dockerfile . --target runtime --no-cache` for KPI, Admin, Notifications, Reporting, AICopilot, Compliance, Search, Calendar. Verify: all exit with code 0.
5. **Compose start (AC-2 + AC-4):** `docker compose --profile api up -d --build` from the feature worktree root. Wait 45 seconds for health checks to pass.
6. **Container health (AC-4):** `docker compose ps | grep -E "workflow|kpi|admin|notifications|reporting|aicopilot|compliance|search|calendar"` — all 9 rows show `(healthy)`.
7. **Health endpoint spot-check (AC-2 — liveness):** `for port in 5001 5002 5003 5004 5005 5006 5007 5008 5009; do curl -s http://localhost:$port/health/live; done` — each response contains `"status":"Healthy"`.
8. **Health endpoint spot-check (AC-2 — readiness):** `for port in 5001 5002 5003 5004 5005 5006 5007 5008 5009; do curl -s http://localhost:$port/health/ready; done` — each response contains `"status":"Healthy"`.
9. **Teardown:** `docker compose --profile api down` — clean up containers after verification.

**Files touched:** None — Phase 4 is verification only. No files are created or modified.

**Not changed:** All files created in Phases 1–3 are read but not modified during Phase 4.

**Tests (per step):**

- **Step 1:** `dotnet build Traverse.sln --no-incremental 2>&1 | grep -E "Error\(s\)|Warning\(s\)|Build succeeded"` — output: `Build succeeded`, `0 Error(s)`, `0 Warning(s)`.
- **Step 2:** `dotnet sln Traverse.sln list` — 17 entries.
- **Steps 3–4:** Exit code 0 for each `docker build` command.
- **Step 6:** All 9 service rows in `docker compose ps` show `(healthy)`.
- **Steps 7–8:** JSON response `{"status":"Healthy"}` (or containing `"Healthy"`) from each of the 9 ports.

Happy path: all 9 services start, register the liveness check, and respond with 200 from `/health/live`.
Edge case: Port conflicts (EC-2 from design) — if any of ports 5001–5009 is occupied, `docker compose up` fails. Resolution: copy `.env.example` to `.env` and override the conflicting port variable. The service still listens on `8080` inside the container; only the host port mapping changes.
Error case: Container exits immediately (`exited` status in `docker compose ps`) — indicates a startup error. Check `docker compose logs {service}-api` to identify the cause. Most likely causes: missing `appsettings.json` (resolved in Phase 2) or incorrect `ENTRYPOINT` DLL name in the Dockerfile (resolved in Phase 3).
Edge case: `curl` not available — use `wget -qO- http://localhost:{port}/health/live` as the `aspnet` base image includes `wget` but not `curl`.

**Database migrations:** None.

**Success criteria:**
- [ ] `dotnet build Traverse.sln --no-incremental` reports `Build succeeded`, `0 Error(s)`, `0 Warning(s)` — AC-1 satisfied
- [ ] `dotnet sln Traverse.sln list` shows 17 projects (8 shared + 9 service stubs) — AC-1 satisfied
- [ ] `docker build -f docker/{service}/Dockerfile . --target runtime --no-cache` exits with code 0 for all 9 services — AC-3 satisfied
- [ ] `docker compose --profile api up -d --build` starts all 9 containers without error
- [ ] `docker compose ps` shows all 9 API service containers with status `(healthy)` — AC-4 satisfied
- [ ] `GET http://localhost:{port}/health/live` returns `200 OK` with body containing `"Healthy"` for all 9 ports (5001–5009) — AC-2 satisfied
- [ ] `GET http://localhost:{port}/health/ready` returns `200 OK` with body containing `"Healthy"` for all 9 ports — AC-2 satisfied
- [ ] `docker compose --profile api down` completes cleanly

**Risks/Compatibility:**
- Docker daemon must be running locally for Phase 4 steps 3–9. Steps 1–2 (`dotnet build`, `dotnet sln list`) do not require Docker.
- The infrastructure containers (PostgreSQL, RabbitMQ) must be running for `docker compose --profile api up` to succeed, because the API services depend on them via `depends_on` in `docker-compose.yml`. Start infrastructure first: `docker compose up -d` (no profile) before running `docker compose --profile api up -d --build`.
- AC-4 specifies waiting 45 seconds for health checks. The `aspnet` container starts in ~3–5 seconds; the 45-second wait is for health check polling intervals from `docker-compose.yml`. If any container is still `starting` after 45 seconds, increase the wait or check `docker compose logs`.

**Dummy code handling:** None — Phase 4 is verification only.

---

## Files Not Changed (Story-Wide)

| File/Directory | Reason Not Changed |
|---|---|
| `docker-compose.yml` | NT-004 owns this file. All 9 API service entries already exist. NT-005 creates only the Dockerfiles referenced by those entries. Firm scope boundary per design §4 and decision log entry #5. |
| `src/shared/` | All 6 shared infrastructure projects exist and are complete from NT-002. NT-005 only adds consumers (API projects) — it does not modify providers. |
| `src/shared/Traverse.AI.Abstractions/` | Not referenced from any Phase 0 API stub per EC-5/decision log entry #6. YAGNI: referenced only by AICopilot in Phase 1. |
| `src/shared/Traverse.AI.Providers/` | Same rationale as `Traverse.AI.Abstractions`. |
| `docker/postgres/init.sql` | Created by NT-004. Empty databases are already created. NT-005 does not define schema. |
| `global.json` | SDK pin `10.0.103` established by NT-001. No change needed (EC-1). |

---

## Substitution Quick Reference

From design §5.1 — used by all phases above:

| Service | `{ProjectName}` | `{ServiceName}` | `{DbName}` | `{SrcRelPath}` | `{ServiceFolder}` | `{HostPort}` |
|---------|----------------|----------------|-----------|----------------|------------------|------------|
| Workflow | `Traverse.Workflow.Api` | `workflow-service` | `traverse_workflow` | `src/services/workflow/Traverse.Workflow.Api` | `workflow` | `5001` |
| KPI | `Traverse.KPI.Api` | `kpi-service` | `traverse_kpi` | `src/services/kpi/Traverse.KPI.Api` | `kpi` | `5002` |
| Admin | `Traverse.Admin.Api` | `admin-service` | `traverse_admin` | `src/services/admin/Traverse.Admin.Api` | `admin` | `5003` |
| Notifications | `Traverse.Notifications.Api` | `notifications-service` | `traverse_notifications` | `src/services/notifications/Traverse.Notifications.Api` | `notifications` | `5004` |
| Reporting | `Traverse.Reporting.Api` | `reporting-service` | `traverse_reporting` | `src/services/reporting/Traverse.Reporting.Api` | `reporting` | `5005` |
| AICopilot | `Traverse.AICopilot.Api` | `aicopilot-service` | `traverse_aicopilot` | `src/services/aicopilot/Traverse.AICopilot.Api` | `aicopilot` | `5006` |
| Compliance | `Traverse.Compliance.Api` | `compliance-service` | `traverse_compliance` | `src/services/compliance/Traverse.Compliance.Api` | `compliance` | `5007` |
| Search | `Traverse.Search.Api` | `search-service` | `traverse_search` | `src/services/search/Traverse.Search.Api` | `search` | `5008` |
| Calendar | `Traverse.Calendar.Api` | `calendar-service` | `traverse_calendar` | `src/services/calendar/Traverse.Calendar.Api` | `calendar` | `5009` |

---

## Gherkin Traceability

| Gherkin File | AC Tag | Satisfied By |
|---|---|---|
| `gherkin_api_stubs_solution_build.feature` | AC-1 | Phase 1 (project creation + sln registration) + Phase 2 (Program.cs) + Phase 4 Step 1–2 (build verification) |
| `gherkin_api_stubs_health_endpoints.feature` | AC-2 | Phase 2 (MapTraverseHealthChecks in Program.cs) + Phase 4 Steps 5–8 (container health check) |
| `gherkin_api_stubs_dockerfiles.feature` | AC-3 | Phase 3 (4-stage Dockerfile replacement) + Phase 4 Steps 3–4 (docker build verification) |
| `gherkin_api_stubs_compose.feature` | AC-4 | Phase 3 (Dockerfiles referenced by compose) + Phase 4 Steps 5–6 (docker compose ps health) |

---

## NT-004 Dockerfile Defect Documentation

The NT-004 Dockerfiles (created as pre-release stubs before the API projects existed) contain the following defect:

```dockerfile
COPY ["Traverse.sln", "global.json", "Directory.Build.props", "./"]
```

`Directory.Build.props` does not exist in the Northwoods Traverse repository. Running `docker build` against any NT-004 Dockerfile produces:

```
COPY failed: stat Directory.Build.props: no such file or directory
```

This defect is automatically and completely corrected when NT-005 replaces all 9 Dockerfiles with the 4-stage template from design §5.5. The NT-005 template copies only `global.json` and `Traverse.sln` — `Directory.Build.props` is not referenced.

No separate defect story is required. The correction is inherent to NT-005's scope (decision log entry #7).

---

<!-- Generated by skill: plan-implementation v3.4.0 | 2026-05-04 12:00 -->
