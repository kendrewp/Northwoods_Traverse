---
story_id: NT-005
feature_id: NT-000
reviewer: code-review (automatic)
date: 2026-05-06
verdict: APPROVED
review_cycle: 1
---

# Code Review Findings — NT-005: Base Microservice Projects (API Stubs)

## Summary

NT-005 creates nine empty ASP.NET Core Web API projects (one per PRD module), registers
them in `Traverse.sln`, replaces nine 2-stage NT-004 Dockerfiles with 4-stage
production-ready images, and wires all six shared infrastructure libraries in each
`Program.cs`. The implementation is clean, correct, and aligned with the design document.
`dotnet build Traverse.sln` passes with 0 errors and 0 warnings across all 17 projects.

- **Must-fix:** 0
- **Should-fix:** 0
- **Suggestions:** 2 (both auto-fixed in review cycle 1)
- **Verdict:** APPROVED

---

## Must-Fix (block merge — resolved before review completes)

_None._

---

## Should-Fix (advisory)

_None._

---

## Suggestions (take or leave)

### SG-1 — Design doc §5.2 template shows 4-level ProjectReference path (implementation uses 3, which is correct)

- **What:** `design_api_stubs.md` §5.2 `.csproj` template showed `..\..\..\..\shared\` (4 levels up) and the path note stated "four levels to `src/shared/`". The actual implementation uses `..\..\..\\shared\` (3 levels), which is the correct path from `src/services/{svc}/{ProjectName}/` to `src/shared/`. Four levels would overshoot to `{repo-root}/shared/` (does not exist), causing MSB9008 at build time.
- **Why:** If a future developer references the design doc to add a new project reference in Phase 1, they would get a broken path. ADR-009 (Decision 1) correctly documents the 3-level path, but the design doc template itself was still wrong.
- **How:** Correct §5.2 template `ProjectReference` paths from 4-level to 3-level, and update the path note to explain the correct count with a step-by-step walkthrough.
- **Resolved:** [x] Yes
- **Resolved date:** 2026-05-06
- **Resolution note:** auto-fixed in review cycle 1: updated §5.2 ProjectReference paths from 4 levels (`..\..\..\..\`) to 3 levels (`..\..\..\\`) and rewrote path note with directory-level walkthrough. Tracker checksum updated.

---

### SG-2 — Design doc §7 `/health/ready` description inaccurate re: MassTransit auto-registration

- **What:** Design doc §7 stated "In Phase 0, no `"ready"`-tagged checks are registered. The readiness endpoint returns HTTP 200 with an empty entries object." This is incorrect: MassTransit 8.3.0 automatically registers a `"masstransit-bus"` health check tagged `["ready","masstransit"]` when `AddTraverseMessaging()` wires `AddMassTransit().UsingRabbitMq()`. The readiness endpoint therefore returns 503 when RabbitMQ is unreachable (correct readiness behaviour) and 200 with a non-empty entries object when RabbitMQ is reachable.
- **Why:** The incorrect description was already flagged as CG-001 by integration testing (step_8). If a Phase 1 developer reads the design doc expecting an empty readiness response, they may misinterpret a correct 503 as a bug, or write incorrect health check tests.
- **How:** Update §7 success/failure response rows and explanatory text to accurately describe the MassTransit auto-registered broker connectivity check.
- **Resolved:** [x] Yes
- **Resolved date:** 2026-05-06
- **Resolution note:** auto-fixed in review cycle 1: updated §7 `/health/ready` success/failure response rows and descriptive text to document MassTransit 8.3.0 auto-registration of `"masstransit-bus"` check. Added cross-reference to integration test finding CG-001.

---

## What's Good

- **Zero-warning build**: All 9 projects compile with `TreatWarningsAsErrors=true` against .NET 10. No `NU1510` (duplicate FrameworkReference), no `CS1591` (missing XML doc comments — correctly suppressed for Phase 0). This is production discipline applied from day one.

- **Clean architecture**: Dependency graph is strictly inward — 9 API projects depend on 6 shared libraries; shared libraries have zero references back. `Traverse.Domain.Primitives` has zero `ProjectReference` entries (pure domain). This is DIP enforced at the project-reference level, not just the namespace level.

- **YAGNI applied correctly**: `Traverse.AI.Abstractions` and `Traverse.AI.Providers` are absent from all 9 Phase 0 stubs, avoiding OpenAI/Anthropic/AWS SDK transitive NuGet inflation in 8 services that will never use AI. The AICopilot stub documents the intentional deferral in both its `.csproj` comment and `Program.cs` header.

- **Layer-cache-friendly Dockerfiles**: The 4-stage pattern (`restore` → `build` → `publish` → `runtime`) correctly copies only `.csproj` files in Stage 1, so a source-only change in Stage 2 does not bust the NuGet restore cache. This is a significant CI time saving at scale with 9 services.

- **Security by default**: All 9 Dockerfiles use `adduser --disabled-password --gecos ""` (non-root, passwordless, no GECOS fields), consistent with CIS container benchmark requirements. `ENTRYPOINT` uses exec form (`["dotnet", "..."]`) — signal forwarding is correct for graceful SIGTERM shutdown.

- **Decision log discipline**: All 3 deviations from the plan (3-level vs 4-level path, `Microsoft.AspNetCore.OpenApi` explicit package, full Dockerfile replacement) are documented in ADR-009 with rationale and alternatives considered. LL-009 (inject() in RxJS callbacks) was added as a cross-cutting lesson learned. `development_evolution.md` updated on the same day as implementation.

- **Middleware pipeline order is correct**: `CorrelationIdMiddleware` → `ExceptionHandler` → `Authentication` → `Authorization` → `MapHealthChecks`. Health endpoints bypass auth via `AllowAnonymous()`. `AddOpenApi()` is registered unconditionally but `MapOpenApi()` is guarded by `IsDevelopment()` — correct security posture.

- **Consistent service name substitution**: All 9 `Program.cs` files pass the correct `{ServiceName}` string from §5.1 substitution table to `AddTraverseObservability()`. All 9 `appsettings.json` use the correct unique `{DbName}` value. No copy-paste errors across 9 services.

---

## Verification

### Step 1: Identify commands
- `dotnet build Traverse.sln --no-incremental` — AC-1 solution build
- `dotnet sln Traverse.sln list` — project count
- `grep` checks for structural correctness (paths, stages, ENTRYPOINT, service names)

### Step 2: Execute

```
dotnet build Traverse.sln --no-incremental 2>&1 | tail -5
```

### Step 3: Review output

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:02.21
```

### Step 4: Confirm claims

- `dotnet sln list` → 17 projects (9 API stubs + 8 shared) ✓
- All `.csproj` files use `..\..\..\\shared\` (3-level) ✓ — confirmed by grep across all 9
- All Dockerfiles have exactly 4 `FROM` instructions ✓ — confirmed by grep
- All `ENTRYPOINT` entries match their project's `.dll` name ✓
- No `Directory.Build.props` references in any Dockerfile ✓
- `[Ll]ogs/` is in `.gitignore` — `src/services/workflow/Traverse.Workflow.Api/logs/` will not be committed ✓
- Dev credentials in `appsettings.json` are intentional for local dev — `appsettings.Development.json` is deferred to Phase 1 per design §10 Out of Scope ✓

### Step 5: Findings
- Build passes. All structural checks pass. Two documentation-only suggestions (SG-1, SG-2) auto-fixed.

---

## YAGNI Check

The following were evaluated and confirmed YAGNI-compliant:

- `Microsoft.AspNetCore.OpenApi` added to each `.csproj` — required for `AddOpenApi()`/`MapOpenApi()` calls that are in the design-specified `Program.cs` template. Not speculative. ✓
- `builder.Services.AddAuthorization()` in each `Program.cs` with no policies — required by `app.UseAuthorization()` below it. Without this registration, ASP.NET Core throws at startup. ✓
- Six `ProjectReference` entries even though only 3–4 are actively called at Phase 0 — explicitly in-scope per design §3: "Phase 0 is the wiring point — each Phase 1 story extends this project without needing to discover or add project references." The pre-wired references prevent repetitive boilerplate in 9 Phase 1 stories. ✓
- `Otel:Endpoint` empty in `appsettings.json` — `AddTraverseObservability` skips the OTLP exporter when absent, per design §5.4 notes. No-op, not dead code. ✓
- No `appsettings.Development.json` files — correctly deferred to Phase 1 per design §10 Out of Scope. ✓

---

## AutoMode Self-Remediation Log (Review Cycle 1)

| Finding | Severity | Auto-fix applied | Result |
|---------|----------|-----------------|--------|
| SG-1 | suggestion | Updated `design_api_stubs.md` §5.2 template paths from 4-level to 3-level; rewrote path note | Resolved |
| SG-2 | suggestion | Updated `design_api_stubs.md` §7 `/health/ready` table and text to document MassTransit auto-registration | Resolved |

Tracker artefact checksum for `design_api_stubs.md` updated to `sha256-004da9cdb2c20dff024385f0781c03cc6795ef6947afbc66037b111732fe20dc`.

---

<!-- Generated by skill: code-review v2.2.0 | 2026-05-06 -->
<!-- AutoMode: true — all suggestions auto-fixed in review cycle 1 -->
