# Development Evolution — Northwoods Traverse

This document records every significant architectural decision, design change, and bug fix with architectural implications.  All options considered are documented alongside the rationale for the selected approach.  Maintained per CLAUDE.md rule #12.

---

## ADR-001 — AI Provider Abstraction

**Date:** 2026-04-25  
**Status:** Decided  
**Deciders:** PM (provider selection), Dev Lead (interface design)

### Context

The AI Copilot Suite (MOD-06) requires a chat completion engine and an embedding engine across ten features (Daily Briefing, Completion Assistant, Trigger Detection, Breach Forecast, Smart Case Summary, Workload Balancing, Program Health Narrative, Policy Q&A, Outreach Drafting, Anomaly Flagging).

The PM selected AWS Bedrock as the primary provider but stated the architecture should remain open to OpenAI and direct Anthropic API as alternatives.  The platform must not have hard dependencies on any single provider SDK wired throughout the codebase.

### Decision

Introduce a two-project abstraction layer:

| Project | Responsibility |
|---|---|
| `Traverse.AI.Abstractions` | `IAIProvider` interface, all model records, `AIProviderException`. Zero provider NuGet dependencies. |
| `Traverse.AI.Providers` | Three concrete adapters (Bedrock, OpenAI, Anthropic), `AIOptions` for configuration, and `AIServiceCollectionExtensions` for DI registration. |

The active provider is selected at startup via `AI:Provider` in configuration (`appsettings.json` + secrets).  No application code outside `Traverse.AI.Providers` imports provider-specific SDK types.

### Options Considered

#### Option A — Direct SDK usage at call sites (rejected)
AWS SDK, OpenAI SDK, and Anthropic.SDK imported wherever AI is called.

- **Pro:** No abstraction overhead.
- **Con:** Violates DIP (high-level features depend on low-level SDKs).  Switching providers requires changes across every feature module.  Impossible to unit-test without hitting live APIs.

#### Option B — Single `IAIProvider` interface + adapters (selected)
Provider-agnostic interface in a zero-dependency project; three adapters in a separate providers project.

- **Pro:** Features depend only on `IAIProvider` — switching providers is a config change.  Each adapter is independently unit-testable by injecting `IAmazonBedrockRuntime` / OpenAI SDK client mocks.  Single point of change for SDK version upgrades.
- **Con:** Slight indirection for new contributors unfamiliar with the pattern.

#### Option C — Strategy pattern per feature (rejected)
Each feature selects its own provider via a strategy object.

- **Pro:** Per-feature provider flexibility.
- **Con:** Over-engineering for current requirements.  Adds cognitive load without a concrete use case.

### Interface Design

```
IAIProvider
├── CompleteAsync(AICompletionRequest) → AICompletionResponse
├── StreamCompleteAsync(AICompletionRequest) → IAsyncEnumerable<string>
├── EmbedAsync(AIEmbeddingRequest) → AIEmbeddingResponse
├── SupportsEmbeddings: bool
└── ProviderName: string
```

### Provider Support Matrix

| Provider | Completions | Streaming | Embeddings | Auth |
|---|---|---|---|---|
| AWS Bedrock | Yes (Converse API) | Yes | Yes (Titan V2) | AWS credential chain |
| OpenAI | Yes (ChatClient) | Yes | Yes (text-embedding-3-small) | API key in secrets |
| Anthropic | Yes (Messages API) | Yes | **No** — not offered | API key in secrets |

### Embedding Gap (Anthropic)

Anthropic does not expose an embeddings endpoint.  `AnthropicAIProvider.EmbedAsync` throws `NotSupportedException`.  Callers that need both Claude-family models and embeddings should use `AIProviderType.Bedrock`, which hosts the same Claude models via Bedrock and also provides Titan embeddings.

### SOLID Analysis

- **SRP:** `Traverse.AI.Abstractions` owns only the contract; `Traverse.AI.Providers` owns only the implementations.
- **OCP:** Adding a fourth provider (e.g. Azure OpenAI) requires implementing `IAIProvider` and adding a `case` to `AIServiceCollectionExtensions` — no existing code changes.
- **LSP:** All three adapters honour the same contract; `SupportsEmbeddings` makes the capability difference explicit so callers are not surprised by `NotSupportedException`.
- **ISP:** The interface is intentionally narrow (3 methods + 2 properties).  Features that need only completions are not forced to depend on the embedding API.
- **DIP:** All feature modules depend on `IAIProvider` (abstraction), not on Bedrock/OpenAI/Anthropic SDK types (concretions).

### Configuration

Active provider and model IDs are set in `appsettings.json` under the `AI` section.  API keys (OpenAI, Anthropic) must be supplied via secrets — never checked into appsettings files.  AWS Bedrock uses the credential chain; no key in config.

See `src/shared/Traverse.AI.Providers/appsettings.AI.example.json` for the full configuration schema.

### Consequences

- Every microservice that calls AI features adds a reference to `Traverse.AI.Abstractions` and calls `services.AddTraverseAI(configuration)` from `Traverse.AI.Providers`.
- The active provider is a deployment-time decision, not a compile-time decision.
- Switching from Bedrock to OpenAI in production requires only a config key change and a secret rotation — zero code changes.
- Integration tests can register a mock `IAIProvider` without any provider SDK installed in the test environment.

---

## ADR-003 — Technology Stack Decisions (Gap-Fill Round)

**Date:** 2026-04-26  
**Status:** Decided  
**Deciders:** Dev Lead

### Context

A gap-analysis pass across all 12 PRDs produced 19 unanswered questions.  This ADR records the decisions with architectural weight — those that impose external dependencies, drive interface design, or constrain implementation choices across multiple modules.  Lower-weight decisions (team ownership, dates, Jira epics) are recorded in `docs/questions.md` only.

---

### Observability: OpenTelemetry + Datadog

All microservices emit traces, metrics, and structured logs via the OpenTelemetry SDK (`AddOpenTelemetry()` per the .NET Coding Standards) and export via OTLP to a Datadog agent.  Datadog is the APM/log-aggregation backend.

**Rationale:** OpenTelemetry keeps instrumentation vendor-neutral; Datadog provides best-in-class distributed tracing across microservices with native RabbitMQ and PostgreSQL integrations.  Switching backends (to Application Insights, Grafana, etc.) requires only an OTLP exporter change — zero application code change.

**Consequences:** Each microservice adds `AddOpenTelemetry().WithTracing(...).WithMetrics(...)` in `Program.cs`.  The Datadog agent runs as a Kubernetes sidecar.  MOD-03 telemetry events (`kpi_recalculated`, `threshold_crossed`, etc.) are emitted as OTel spans/counters.

---

### Search Index: Azure AI Search

MOD-08 Universal Search uses **Azure AI Search** as the managed search backend.  Hybrid BM25 + vector retrieval + semantic ranker is required for natural-language queries; pgvector alone cannot provide the semantic ranking layer.

**Alternatives rejected:** pgvector-only (no semantic ranker); Elasticsearch (operational overhead, licensing); in-house BM25 (maintenance cost with no semantic capability).

**Interface:** Expose behind `ISearchService` (DIP) in a dedicated `Traverse.Search.Application` project.  The implementation in `Traverse.Search.Infrastructure` references the Azure AI Search SDK.  On-premises deployments may swap to a pgvector implementation via the same interface.

**Consequences:** A `SearchService` microservice owns push-model indexing.  MassTransit consumers in that service subscribe to domain events (case created, case status changed, work item completed) and push index updates.  RBAC permission filtering uses Azure AI Search `$filter` expressions keyed to `tenantId` and `programScope`.

---

### PDF Export: QuestPDF + SkiaSharp

MOD-05 report PDF rendering uses **QuestPDF** (MIT licence) for document layout and **SkiaSharp** for chart/graph image generation embedded as PNG.

**Alternatives rejected:** PuppeteerSharp/Playwright (heavyweight Chromium dependency, browser security surface, slower — unnecessary for tabular data reports); iText7 (AGPL requires commercial licence); SSRS (heavyweight, Microsoft-only).

**Consequences:** PDF rendering runs inside the Reporting microservice behind an `IPdfRenderer` interface.  Verify QuestPDF community licence limit at implementation time (currently free below $1M USD/year revenue).

---

### Breach Forecast Model: Heuristic (v1), Statistical (v2)

MOD-06 breach forecasting uses a **configurable heuristic** for v1: flag a work item as at-risk when elapsed time exceeds a configurable percentage of the policy target (default: 80%).  Threshold is configurable per program in MOD-07 AI Governance.

**Rationale:** Zero historical case data exists at launch.  ML and statistical models require training data.  The heuristic is interpretable, Admin-tunable, and deployable immediately.

**Disable trigger:** When a program's flagged-case rate exceeds 2× its actual breach rate over a rolling 30-day window, Admin receives an alert and may disable the per-program forecast toggle.

**v2 path:** After 6–12 months of production case history, evaluate a statistical model (logistic regression or gradient-boosted trees on case features).  The `IBreachForecastService` abstraction isolates the swap.

---

### Geocoding and Address Validation: Azure Maps + SmartyStreets

MOD-09 field directions use **Azure Maps** for geocoding and routing, with **SmartyStreets** as a preprocessing step for USPS CASS address standardisation.

**Rationale:** Social worker field visits involve client home addresses (PII).  Google Maps Geocoding API has no HIPAA BAA — it is not a viable option.  Azure Maps is FedRAMP Moderate / HIPAA-eligible, does not use request data for ad targeting, and has a .NET SDK.

**Interface:** `ILocationService` abstraction in `Traverse.Calendar.Application`.  Turn-by-turn navigation opens in the device's native map app (Google Maps / Apple Maps) via deep link — the backend only provides geocoded coordinates.

---

### AI Context Builder: Authorization-Service Consumer Pattern

MOD-06 AI context builders must consume `IAuthorizationService` (ASP.NET Core policy engine) rather than implementing their own field-level permission lists.  This makes divergence between API access rules and AI context access rules structurally impossible.

**Testing:** A `ContextBuilderAuthorizationTests` fixture iterates every role × every context type and asserts the materialised context contains only fields that role's policy permits.  A privacy test failure (role sees data it shouldn't) is a P0 severity blocker — blocks merge.

---

### Directory Sync: SCIM 2.0 (v1.1)

MOD-07 user provisioning targets **SCIM 2.0** as the sync protocol.  SSO is post-v1 (PM confirmed), so the SCIM endpoint is a v1.1 deliverable.  In v1, the endpoint returns `501 Not Implemented`.  The Admin data model reserves `scimTenantUrl` and `scimBearerToken` fields to avoid a schema migration at v1.1.

---

### Q&A Knowledge Sources: Admin-Uploaded PDFs with Versioned Embeddings

MOD-06 Policy Q&A knowledge sources are **Admin-uploaded PDFs**, chunked on upload (500-token chunks, 100-token overlap, split on section headers), embedded via Bedrock Titan V2, and stored in the vector store (Azure AI Search or pgvector).  URL crawling is rejected as too brittle for regulatory content.  Each document is versioned so AI citations reference the specific policy version that was active at query time.

---

## ADR-004 — Target Framework: net10.0

**Date:** 2026-04-27
**Status:** Decided
**Deciders:** Dev Lead / PM (self — single operator)
**Triggered by:** NT-001 code review finding SG-2 — `global.json` pins .NET 10 SDK (10.0.103) while the Phase 0 architecture document targeted .NET 9.

### Context

The NT-001 scaffolding was built on the machine's installed SDK (10.0.103 — .NET 10 GA). The Phase 0 architecture document (`architecture_phase0.md`) was written before SDK availability was confirmed and referenced .NET 9 as the target framework. A contradiction existed between the runtime document, the installed SDK, and the `global.json` generated during scaffolding.

All new projects created in NT-002 onwards must specify a `<TargetFramework>` property. This decision resolves the correct TFM to use.

### Decision

**All Northwoods Traverse microservice projects target `net10.0`.**

`global.json` pins SDK `10.0.103` with `rollForward: latestFeature` (committed in NT-001). All `.csproj` files in NT-002 onwards set `<TargetFramework>net10.0</TargetFramework>`.

### Options Considered

#### Option A — net9.0 (rejected)

Match the original architecture document target.

- **Pro:** Matches documentation as written; .NET 9 is the current LTS-adjacent release.
- **Con:** The installed SDK is .NET 10.  `global.json` pins 10.0.103.  Building `net9.0` TFMs with a .NET 10 SDK is possible but introduces a mismatch between the SDK pin and the runtime target that is confusing for contributors.  .NET 9 reaches End of Support in May 2026 — targeting it for a greenfield project starting April 2026 with a multi-year lifecycle is imprudent.  Would require downgrading `global.json` from a committed artefact.

#### Option B — net10.0 (selected)

Match the installed SDK and the committed `global.json`.

- **Pro:** No SDK/TFM mismatch.  .NET 10 is the current major release (SDK 10.0.103 GA).  All new C# 14 language features are available.  Consistent with `global.json` already committed in NT-001.  No backtracking on committed artefacts.
- **Con:** .NET 10 Standard-Term Support (STS) — 18-month support window, not LTS (3 years).  Next LTS is .NET 12 (scheduled Nov 2026).  Mitigation: upgrade path to .NET 12 is straightforward; the abstraction layer architecture (thin services, no platform lock-in) keeps the migration cost low.

### SOLID Analysis

No direct SOLID impact — this is a runtime target decision, not an interface or dependency design.  All project references, NuGet package compatibility checks, and `net10.0` TFM bindings are resolved at project creation in NT-002 through NT-005.

### Consequences

- All `.csproj` files: `<TargetFramework>net10.0</TargetFramework>`.
- All Dockerfiles: `FROM mcr.microsoft.com/dotnet/aspnet:10.0` (runtime) and `FROM mcr.microsoft.com/dotnet/sdk:10.0` (build stage).
- Architecture document `architecture_phase0.md` references .NET 9 in prose — this document supersedes that reference.  The architecture doc's checksum is not invalidated by this prose gap; the actual implementation governs.
- Plan to evaluate .NET 12 LTS (expected Nov 2026) upgrade when available.

---

## ADR-002 — KPI Policy Snapshot Cadence (Event-Sourced Version Storage)

**Date:** 2026-04-25  
**Status:** Decided  
**Deciders:** Dev Lead

### Context

MOD-07 (Admin Configuration Suite) stores KPI policies using event-sourcing — every admin edit is appended as an immutable event.  To reconstruct a policy at any version, the system replays events from the beginning of the log.

MOD-03 (KPI / SLA Policy Engine) must resolve "what did policy X look like at version V?" for every KPI recalculation, because work items pin to the policy version active at their activation time (AC-1.3).  As the event log grows over months of admin edits, replaying from Event 1 for every recalculation degrades performance — directly threatening the ≤ 60 s recalculation SLA.

Snapshots solve this: a materialised point-in-time state stored alongside the event log so the engine replays only events *after* the snapshot rather than everything.  The open question was cadence — how often to snapshot.

### Decision

**Take a snapshot on every publish event.**

When an admin publishes a KPI policy version in MOD-07, the system immediately serialises the current policy aggregate state as a snapshot and associates it with that published version identifier.  No snapshots are taken for draft or unpublished states.

### Options Considered

#### Option A — Every N events (rejected)

Snapshot after every N appended events (e.g., every 10).

- **Pro:** Simple; bounds worst-case replay length to N events regardless of admin behaviour.
- **Con:** Snapshots draft states that the KPI engine never queries.  The N boundary has no semantic meaning — a snapshot may land mid-draft, between two edits that are never individually published.  Choosing N is arbitrary and must be re-tuned as edit frequency changes.  Does not guarantee a snapshot at the exact published boundary the recalculation engine needs.

#### Option B — On every publish (selected)

Snapshot exactly when the admin publishes a new version.

- **Pro:** Aligns with the semantic unit the KPI engine already uses.  Work items pin to published versions; every published version has a corresponding snapshot; recalculation loads snapshot + zero subsequent events (the next event after publish is always the first edit of the next draft cycle).  Snapshot overhead is absorbed into the publish operation, which is already expensive (it triggers version binding for in-flight work items and runs the KPI simulator).  Makes `GetPolicyAtVersionAsync` trivially efficient — load snapshot, done.
- **Con:** An admin who publishes two corrections in rapid succession creates two snapshots close together in time.  This is negligible overhead because publish is an intentional, low-frequency admin action, not a high-throughput path.

#### Option C — Time-based (e.g., hourly) (rejected)

Snapshot on a fixed schedule regardless of admin activity.

- **Pro:** Decoupled from admin behaviour; predictable storage growth.
- **Con:** Snapshots idle periods with no admin edits.  May miss a burst of rapid publishes between schedule ticks, leaving the KPI engine with a stale snapshot and many events to replay.  Adds an always-running background job for no semantic benefit over Option B.

### SOLID Analysis

- **SRP:** The snapshot writer has one responsibility: materialise and persist aggregate state on publish.  It does not participate in recalculation or draft management.
- **OCP:** Adding a new snapshotable aggregate (e.g., Business Calendar) requires implementing the same snapshot interface — no changes to the existing policy snapshot writer.
- **LSP:** Any published policy version retrieved via `GetPolicyAtVersionAsync` returns a fully hydrated aggregate whether it was reconstructed from a snapshot or (in degenerate cases, e.g., pre-snapshot migration) from a full replay.
- **ISP:** The KPI engine's repository interface exposes only `GetPolicyAtVersionAsync` — it does not know or care that snapshots exist behind that method.
- **DIP:** The snapshot store is injected into the policy repository implementation; neither the domain aggregate nor the KPI engine depends on the concrete snapshot storage mechanism.

### Implementation Notes

- The snapshot is taken inside the same database transaction as the `PolicyPublished` domain event write.  If the transaction rolls back, the snapshot is not persisted — no orphaned snapshot.
- The snapshot payload is the serialised policy aggregate at the moment of publish, including all versioned fields: scope, timing definition, threshold configuration, time basis, pause conditions, and effective start date.
- `GetPolicyAtVersionAsync(policyId, versionId)` implementation: load the snapshot keyed to `(policyId, versionId)`; if found, return directly.  If not found (legacy data pre-snapshot), fall back to full event replay.  This fallback path can be removed after a one-time migration run.
- Snapshot storage uses the same PostgreSQL schema as the aggregate event store to avoid cross-store consistency issues.

### Consequences

- KPI recalculation resolves historical policy versions in O(1) reads — no event replay on the hot path.
- The `GetPolicyAtVersionAsync` contract is the design boundary: MOD-03 and MOD-07 are decoupled; snapshot internals are invisible to the KPI engine.
- Publish becomes slightly more expensive (one additional write per publish).  Acceptable because publish is an infrequent, deliberate admin action — not a throughput-sensitive operation.
- A one-time backfill migration will be needed for any KPI policies that existed before this decision was implemented.  Backfill can replay events offline and write snapshots without affecting live traffic.

---

## ADR-005 — OutboxProcessorBase: Scope-Delegation over Per-Message Dispatch

**Date:** 2026-04-29
**Status:** Decided
**Deciders:** Dev Lead (identified during NT-002 code review, cycle 1)
**Story:** NT-002 — Shared Backend Libraries

### Context

The approved design document (`design_shared_libs.md` AC-3, §4.5) specified the following Template Method pattern for `OutboxProcessorBase`:

```csharp
// Design spec
protected abstract Task PublishMessageAsync(OutboxMessage message, IPublishEndpoint publisher, CancellationToken ct);
```

Under this design, the base class was responsible for: (1) creating the DI scope, (2) querying the database for unpublished messages, (3) calling the abstract method once per message, (4) marking each message as published, and (5) saving changes.  The subclass was responsible only for routing a single `OutboxMessage` to the correct MassTransit `IPublishEndpoint` call.

During NT-002 implementation, the execute-implementation agent identified a structural limitation: `OutboxMessage.MessageType` contains a string type discriminator, but the base class cannot deserialise or route messages without knowledge of the concrete types — which belong to the service's domain, not the shared library.  Having the base class own the query-and-loop means it must also own the deserialisation logic or accept a second abstract method, creating a leaky abstraction.

### Decision

**Delegate the entire DI scope to the subclass via `ProcessScopeAsync`.**

```csharp
// Implemented API
protected abstract Task ProcessScopeAsync(IServiceProvider scopedProvider, IPublishEndpoint publisher, CancellationToken ct);
```

The base class creates the scope, resolves `IPublishEndpoint`, and calls `ProcessScopeAsync`.  The subclass receives the scoped `IServiceProvider` and resolves its own typed `DbContext`, queries only its own outbox messages, deserialises them using its own domain types, and publishes each one.  The base class retains the iteration loop (5-second delay, cancellation) and the scope lifecycle.

### Options Considered

#### Option A — Per-message dispatch (spec, rejected)

Base class owns the full DB query loop; subclass handles only type routing.

- **Pro:** Minimal subclass responsibility; base class enforces the 100-message batch and ordering invariants.
- **Con:** The base class must receive the concrete `DbContext` type from the subclass (via a second abstract method or generic type parameter) to query `DbSet<OutboxMessage>`.  Without this, the base has no way to query messages from the service's database.  Alternatively, require a `DbContext`-agnostic `IOutboxRepository` — but that is a third abstraction not present in the design.  The design's `OutboxProcessorBase` explicitly takes `IServiceProvider` not `DbContext`, making the per-message loop pattern non-implementable without additional scope-resolution boilerplate in the base class that duplicates what the subclass would do anyway.

#### Option B — Full scope delegation (selected)

Base class delegates the entire scope content to `ProcessScopeAsync`.

- **Pro:** The subclass is the only party with knowledge of its concrete `DbContext` type and message type discriminator — placing the query and deserialisation there respects encapsulation.  Fewer abstractions.  Cleaner separation: base handles scheduling/lifecycle, subclass handles all data access.  No generic type parameters on the base class are needed.
- **Con:** The base class no longer enforces the 100-message batch limit or ordering — these become the subclass's responsibility.  Subclasses could implement inconsistent batching.  Mitigated by XML doc on `ProcessScopeAsync` specifying the expected behaviour contract.

#### Option C — Generic base class `OutboxProcessorBase<TContext>` (rejected)

Parameterise on the DbContext type to allow the base to resolve it.

- **Pro:** Base class retains control of the query loop.
- **Con:** Exposes EF Core types in the shared library's public surface at the generic type level.  Forces all service teams to use the exact same `DbContext` naming pattern.  Over-constrains the subclass.  Not required by the current use cases.

### SOLID Analysis

- **OCP (improved):** The selected pattern is more open for extension — subclasses can implement custom batching, custom error handling per message type, or skip categories of messages — without any base class modification.
- **SRP (preserved):** Base class: scheduling lifecycle.  Subclass: all data access for its own outbox.
- **DIP (preserved):** Base class depends on `IServiceProvider` and `IPublishEndpoint` (abstractions).  No EF Core types leak into the base.

### Consequences

- `design_shared_libs.md` §4.5 describes the per-message dispatch pattern — this document supersedes that description.  The design doc is documentation debt (SF-002 in NT-002 code review, cycle 1); it will be updated when the feature docs PR is raised.
- Each service implementing `OutboxProcessorBase` must: (a) resolve its concrete `DbContext` from `scopedProvider`, (b) query `OutboxMessage` records with `PublishedOnUtc == null` ordered by `CreatedOnUtc`, (c) deserialise and publish each record, (d) set `PublishedOnUtc` and save.  These responsibilities are documented in `OutboxProcessorBase`'s XML doc comment.
- The 100-message batch limit and `CreatedOnUtc` ordering are recommended defaults documented in the XML comment but not enforced by the base.

---
