# Lessons Learned — Northwoods Traverse

Each entry documents a past mistake and the concrete pattern to avoid repeating it. Read this file at the start of every session (CLAUDE.md rule #19). Add entries immediately after discovering a mistake, not retrospectively.

---

## LL-001 — Starting the Feature Pipeline Without an Implementation Order

**Session:** 2026-04-26
**Mistake:** Invoked `orchestrate-feature` before any sequenced implementation plan existed. On a 12-module greenfield project with hard cross-module dependencies, the pipeline immediately stalled because there was no Feature ID, no sequencing rationale, and no agreement on what Phase 1 meant.

**Root cause:** Treated a multi-module program-level effort as if it were a single-feature task. The feature pipeline is designed for one story at a time inside an already-sequenced backlog — not for deciding what to build first.

**Pattern to avoid:** On any project with more than 3 modules or explicit cross-module dependencies, create `docs/plans/implementation-order.md` (with phase definitions, dependency rationale, and circular-dependency resolutions) *before* starting the `orchestrate-feature` pipeline. The plan is the input to the pipeline, not something produced inside it.

---

## LL-002 — Architecture Document TFM Mismatch with Installed SDK

**Session:** 2026-04-27 (discovered during NT-001 code review, SG-2 finding)
**Mistake:** `architecture_phase0.md` was written referencing .NET 9 as the target framework. The machine's installed SDK was .NET 10 (10.0.103 GA), and `global.json` committed in NT-001 pinned SDK 10.0.103. This created a three-way contradiction: doc says net9.0, global.json pins .NET 10 SDK, new projects defaulted to net10.0.

**Root cause:** Architecture document was drafted before SDK availability was confirmed on the development machine. The author wrote from knowledge of .NET 9 (then-current at the time of initial design) without checking `dotnet --version`.

**Pattern to avoid:** Before writing any TFM (`net9.0`, `net10.0`, etc.) into an architecture document, run `dotnet --version` and check the committed `global.json`. The SDK pin governs; all architecture prose must match it. Required correction spawned ADR-004 and updated all `.csproj` files.

---

## LL-003 — Phase 0 Scaffolding Omitted from Initial Implementation Plan

**Session:** 2026-04-26
**Mistake:** The first draft of `docs/plans/implementation-order.md` jumped straight to PRD module phases (Phase 1 = MOD-02 + MOD-03 + MOD-07). There was no Angular project, no .NET solution file, no Docker Compose, no shared backend libraries, and no domain primitives — nothing any PRD phase could build on. The user caught the omission before the plan was used.

**Root cause:** Planning focused on the 12 PRD modules (the visible deliverables) and skipped the infrastructure that enables them. Greenfield projects require an explicit scaffolding phase that is easy to overlook when thinking in terms of features.

**Pattern to avoid:** Any implementation plan for a greenfield project must include an explicit Phase 0 (or equivalent) that covers: repository and solution structure, shared library projects, frontend application shell, local development environment (Docker Compose), and empty service stubs. Phase 0 must be a distinct phase — never folded into the first PRD module phase.

---

## LL-004 — NuGet Version Wildcards Instead of Pinned Versions

**Session:** 2026-04-25
**Mistake:** The `Traverse.AI.Providers.csproj` referenced provider SDKs with wildcard versions: `AWSSDK.BedrockRuntime 3.7.*`, `OpenAI 2.*`, `Anthropic.SDK 3.*`. Wildcards allow NuGet to resolve to any patch/minor within the range, which means the build is non-deterministic and may break silently after a package publisher releases a breaking change within the "safe" range.

**Root cause:** Wildcards were used as a placeholder because the exact latest version was not verified at the time of writing. This is a shortcut that defers the problem.

**Pattern to avoid:** After writing any `.csproj`, run `dotnet restore` and pin every wildcard to the exact resolved version in the file. If offline or in a planning context where restore cannot run, mark the package reference with a `TODO: pin after restore` comment and resolve it as the first action before the project is used by any other project.

---

## LL-005 — Unverified SDK API Assumptions in Provider Implementations

**Session:** 2026-04-25
**Mistake:** Three specific SDK type/property names were assumed from documentation rather than verified against the installed package: (1) Anthropic.SDK `ContentBlockDeltaEvent` and `TextDelta` types in streaming; (2) OpenAI SDK v2 `StreamingChatCompletionUpdate.ContentUpdate` property name; (3) OpenAI `EmbeddingClient` per-item token count access path. These may cause compile errors or runtime failures on first build.

**Root cause:** SDK documentation and installed packages can diverge, especially for pre-release or rapidly-iterating packages. Writing provider adapters from docs without a compile cycle is speculative code.

**Pattern to avoid:** Provider adapters must be compile-verified before being merged or referenced by any other project. The correct sequence is: write the adapter → `dotnet build` → fix all type errors → commit. Never treat a provider adapter as "done" if it has not compiled successfully. Unverified implementations must be tagged with `// UNVERIFIED — compile before use` at the top of the file.

---

## LL-006 — Referenced Docs That Did Not Exist at Time of Reference

**Session:** 2026-04-29 (noticed at session pickup)
**Mistake:** CLAUDE.md rules #14 and #19 reference `@docs/session-log-protocol.md` and `@docs/LessonsLearned.md` respectively. Both files were absent. Rule #14 requires logging STARTED/COMPLETED against a protocol document that does not exist. Rule #19 requires reading a LessonsLearned file that does not exist. This creates silent protocol drift — the rules appear mandatory but are unenforceable without the referenced artefacts.

**Root cause:** CLAUDE.md rules were written aspirationally, referencing documents that were planned but not yet created. The gap was not caught during CLAUDE.md authoring.

**Pattern to avoid:** Any CLAUDE.md rule that references a specific file must include creating that file as a prerequisite, or note it as a known gap with a tracking item. When adding a new rule that references a document, create the document in the same commit. This file (LessonsLearned.md) was created on 2026-04-29 to close the LL-006 gap. `docs/session-log-protocol.md` remains outstanding.

---

## LL-007 — Circular Dependencies Identified Late in Planning

**Session:** 2026-04-26
**Mistake:** The circular dependency between MOD-02 (Workflow Execution Engine), MOD-03 (KPI/SLA Policy Engine), and MOD-07 (Admin Configuration Suite) was not identified until the full implementation-order pass. MOD-02 needs KPI thresholds from MOD-03. MOD-03 needs workflow events from MOD-02. Both need configuration from MOD-07. Without explicit resolution, the plan had no valid starting point.

**Root cause:** Circular dependency analysis was not performed as an explicit step before sequencing. The dependency graph was built implicitly by reading PRDs in isolation, not by mapping cross-module data flows first.

**Pattern to avoid:** Before sequencing any multi-module plan, produce an explicit dependency matrix (who depends on whom for what data/event). Run a cycle-detection pass on the matrix before assigning phases. Resolution strategies (internal schema first → integration second; capability deferral; layered build order) must be documented in the plan, not discovered mid-implementation. See `docs/plans/implementation-order.md` §Circular Dependency Resolutions for the applied resolution pattern.

---

## LL-008 — UTF-16 Encoded Input Files

**Session:** 2026-04-25
**Mistake:** `docs/questions from pm.md` was encoded in UTF-16 (saved from a Windows application). Standard `cat`, `Read`, and text tools interpreted it as binary or produced garbled output, requiring a charset-detection step before the file contents could be merged.

**Root cause:** File was provided by the PM directly from a Windows-native editor without specifying encoding. No encoding check was performed before attempting to read it.

**Pattern to avoid:** When reading any user-provided file that produces unexpected output (garbled characters, `^@` null bytes, binary-looking content), check encoding first: `file <path>` or `iconv -f utf-16 -t utf-8 <path>`. Do not attempt to parse the raw bytes as UTF-8 before confirming encoding.

---

## LL-009 — inject() Called Inside RxJS Operator Callbacks in Functional Interceptors

**Session:** 2026-04-30 (discovered during NT-003 integration testing)
**Mistake:** `errorInterceptor` called `inject(Router)` inside a `catchError()` callback. This throws NG0203 (`inject() must be called from an injection context`) at runtime because RxJS operator callbacks execute asynchronously, outside Angular's synchronous injection context. The defect was not caught during Phase 9 smoke testing because no 401 response was triggered manually.

**Root cause:** The author knew that `inject()` is valid in functional interceptors but did not know that this validity applies only to the synchronous function body — not to any async callbacks returned from it (RxJS operators, Promise `.then()`, `setTimeout`, etc.).

**Pattern to avoid:** Never call `inject()` inside RxJS operator callbacks (`catchError`, `tap`, `map`, `switchMap`, `mergeMap`, etc.) in functional interceptors, guards, or resolvers. Capture the DI token at the function-body level (synchronous injection context) and use it via closure inside the callback:

```typescript
// WRONG — throws NG0203 at runtime
export function myInterceptor(req, next) {
  return next(req).pipe(
    catchError(err => { inject(Router).navigate(['/login']); ... })
  );
}

// CORRECT — capture at function-body level, use via closure
export function myInterceptor(req, next) {
  const router = inject(Router);  // synchronous injection context: valid
  return next(req).pipe(
    catchError(err => { router.navigate(['/login']); ... })  // closure: valid
  );
}
```

This pattern applies to all `inject()` calls that need DI tokens in async contexts within functional Angular constructs.

---
