---
categories:
  - "[[Product Requirements]]"
subjects:
  - "[[Software Development]]"
  - "[[V-Model PDLC]]"
  - "[[Northwoods]]"
status: draft
created: 2026-04-25
---
# PRD — Traverse Workflow Execution Engine (MOD-02)

> **V-Model–Aligned PRD.** Inherits from [[PRD - Northwoods Traverse Workspace]]. Source: §9 (Work Items + Workflow Execution).

---

## 1. Product Overview

| Field | Value |
| --- | --- |
| 📅 Target date | [QX YYYY — Phase 1 of master launch plan] |
| 🟡 Document status | Draft |
| 🧭 Team | PM: [Name] · Product: [Name] · Dev Lead: [Name] · Dev: [Names] · QA: [Name] |
| 🗃️ Work tracker (Jira epic) | [CAM-XXXXX] |
| 📎 Parent PRD | [[PRD - Northwoods Traverse Workspace]] |
| 🔗 Resources | [[Northwoods-Traverse]] · `00 - Inbox/Northwoods_Traverse_PRD_v2.0.docx` (§9) |

---

## 2. 🎯 Objective and Problem Alignment

### Overview

Build the deterministic engine that runs cases through Admin-defined sequences of work items — primary workflow per Case Type, secondary (triggered) workflows that run alongside one at a time, and the pause/resume contract with the KPI engine. This is the spine that everything else (cockpit, KPI status, notifications, AI) depends on.

### Problem Statement

Today work items inside a case are not consistently sequenced or tracked. Workers rely on memory and habit; missed steps lead to KPI breach and compliance gaps. Without a deterministic engine that activates the next item only when the prior is complete, the cockpit cannot show "what is next," KPI status is unreliable, and audit trails are incomplete. Triggered events (e.g., Failure to Pay) currently have no formal model — they live in supervisor heads, not the system.

### Importance

- Sequential by design — workers never need to remember the next step.
- Deterministic state means the KPI engine, notifications, and AI all read from a single source of truth.
- Auditable — every status transition + secondary trigger + pause decision is logged with actor, timestamp, and rationale.
- Configurable without code — Admins (MOD-07) define and version both primary and secondary workflows.

### High-Level Approach

- **Work Item entity** with full lifecycle (Not Started → Active → In Progress → Paused → Completed / Cancelled), sub-screen type, sequence position, parent-workflow reference.
- **Primary Workflow** auto-activates on case open; first item becomes Active and KPI clock starts.
- **Secondary Workflow** triggered by Supervisor on a qualifying event; only one active at a time; pause-or-continue primary KPIs per Admin default with Supervisor override.
- **Return contract:** when secondary completes, primary resumes at the point it left off; paused duration is factored into KPI calculation by MOD-03.
- **Versioning:** primary and secondary workflows are versioned and auditable; running cases stay on their current version unless migrated.

---

## 3. 👥 Stakeholders and Personas

- **P1 — Social Worker** — operates on Active work items.
- **P2 — Supervisor** — triggers secondary workflows; confirms or overrides KPI pause; approves overrides.
- **P4 — Admin** — defines primary + secondary workflows in MOD-07 (this module is the runtime executor).
- **MOD-03 KPI Engine** — consumes lifecycle events; returns due dates / risk status.
- **MOD-04 Notifications** — consumes activation, completion, and resumption events.
- **MOD-06 AI Copilot** — consumes lifecycle data for completion suggestions, trigger detection, anomaly flagging.

---

## 4. 🎬 Target Use Cases

- **New case opens:** Case opens with Program + Case Type → engine selects active primary workflow version → first work item enters Active → KPI clock starts.
- **Sequential progression:** Worker completes Active item → engine activates the next sequenced item → its KPI clock starts.
- **Secondary trigger (with pause):** Supervisor triggers a secondary workflow on Failure to Pay → confirms Admin default "pause primary" → primary KPIs pause; secondary's first item activates.
- **Secondary trigger (with continue):** Same scenario, but Supervisor overrides default to "continue primary" → primary KPI clocks keep running while secondary executes.
- **Secondary completes → primary resumes:** Last secondary item closes → engine reactivates the next incomplete primary item; if paused, primary resumes with paused duration factored in. Worker + Supervisor are notified.
- **Workflow not defined:** Case opens for a Case Type with no published workflow → engine creates the case without work items and shows a clear Admin warning (FR-WF-001 #2).

---

## 5. 📊 Success Conditions and Metrics

### Goals

1. Every active case has a deterministic "next work item" at all times.
2. Secondary workflows run alongside primary without ambiguity (only one active; pause/continue is explicit and audited).
3. Paused duration is correctly excluded from KPI elapsed time.
4. Versioning lets Admins change workflows without breaking running cases.

### Non-Goals

- KPI threshold computation (MOD-03).
- Workflow authoring UI (MOD-07 Workflow Configurator).
- Notification delivery (MOD-04).
- AI trigger detection — assistive recommendation only; the trigger action is human-driven (MOD-06 FR-AI-003).

### Success Metrics

| Goal | Metric (quantified) |
| --- | --- |
| Sequential integrity | 0 cases observed with multiple Active primary work items |
| Trigger correctness | 0 cases observed with multiple Active secondary workflows |
| Pause accuracy | KPI elapsed time error ≤ 1 second for cases with pause periods (audit sample) |
| Activation latency | Next-item activation ≤ 1 second after prior-item completion (P95) |

### Guardrails

- Locked work items are not openable or actionable — engine enforcement, not UI suppression.
- No silent pause / resume — every pause and resume writes an audit entry with actor, reason, and timestamp.
- Engine never bypasses required-field enforcement on sub-screen completion.

---

## 6. 🤔 Assumptions

- MOD-07 publishes primary workflows + secondary workflows as versioned, immutable definitions. — ❓ unvalidated
- A case's Program Type + Case Type are stable within a case lifecycle. — ❓ unvalidated
- Sub-screen field validation is enforced at the UI + API layer; the engine consumes a "completion verified" signal. — ❓ unvalidated
- Workflow migration of running cases is out of scope for v1. — ❓ unvalidated

---

## 7. 🔗 Dependencies and V-Model Gates

★ **V-MODEL MANDATORY**

| Dependency | Type | Status | Impact / Notes |
| --- | --- | --- | --- |
| MOD-03 KPI / SLA Policy Engine | Feature (internal) | DRAFT | Receives lifecycle events; returns due dates / status. Hard blocker. |
| MOD-07 Admin Configuration Suite | Feature (internal) | DRAFT | Publishes workflow definitions. Hard blocker for runtime. |
| MOD-04 Notifications & Escalations | Feature (internal) | DRAFT | Consumes activation / completion / resumption events. Soft dependency. |
| Base Traverse Case + Client primitives | Feature (internal) | IN PROGRESS | Hard blocker. |
| Audit log infrastructure | External / shared | PENDING | Receives every status transition + trigger event. |
| Dev Lead technical feasibility sign-off | V-Model gate | PENDING | ★ |
| T-shirt estimate — Estimation Checkpoint 1 | V-Model gate | PENDING | ★ |

---

## 8. 📋 Requirements

★ **V-MODEL MANDATORY**

| Priority | Job Story (INVEST) | Acceptance Criteria (Given / When / Then) | Jira | Notes / Source |
| --- | --- | --- | --- | --- |
| P0 | As a system, when storing a work item, I want a complete standard field set so that downstream consumers (UI, KPI, notifications, AI) read from one model. | AC-1.1: *Given* a work item, *Then* it persists Work Item ID, Title, Description, Type, Status, Owner (user/team/queue), Related Case, Program Type, Case Type, Sequence Position, Parent Work Item ID (for secondary), Created / Activated / Completed Date-Time, Due Date-Time (computed), KPI Status, KPI Policy ID/Version, and Source (manual/workflow/system). AC-1.2: *Given* sensitive fields, *Then* masking rules are applied at API + UI. AC-1.3: *Given* a work item, *Then* its Type determines the sub-screen presented. AC-1.4: *Given* a workflow, *Then* sequence position drives activation order. | [CAM-XXXXX] | FR-WI-001 |
| P0 | As a system, when a work item changes state, I want a defined lifecycle so that transitions are explicit and auditable. | AC-2.1: *Given* the lifecycle, *Then* allowed states are Not Started (locked), Active, In Progress, Paused (with reason), Completed, Cancelled. AC-2.2: *Given* a sequence, *Then* only the Active item can be acted upon — locked items cannot be opened. AC-2.3: *Given* any transition, *Then* an audit entry records actor + timestamp. AC-2.4: *Given* Paused state, *Then* whether the KPI clock pauses follows Admin configuration on the secondary workflow / pause cause. | [CAM-XXXXX] | FR-WI-002 |
| P0 | As an Admin, when I define a work item, I want to choose a sub-screen type so that the worker experiences a fit-for-purpose completion UI. | AC-3.1: *Given* the library, *Then* allowed sub-screen types are Notes Entry, Structured Data Collection (form), Document Upload, Checklist, Composite. AC-3.2: *Given* a sub-screen, *Then* required fields are configurable in MOD-07 and enforced at completion. AC-3.3: *Given* missing required fields, *Then* completion is blocked with field-level errors. | [CAM-XXXXX] | FR-WI-003 — definition lives in MOD-07; runtime enforcement here |
| P0 | As a system, when a workflow runs, I want sequential activation so that no item runs out of order. | AC-4.1: *Given* a sequence, *Then* a locked work item cannot be opened or acted upon. AC-4.2: *Given* completion of the active item, *Then* the next sequential item activates and its KPI clock starts within ≤ 1 second (P95). AC-4.3: *Given* completed items, *Then* they remain visible in workflow history for audit + context. | [CAM-XXXXX] | FR-WI-004 |
| P0 | As a system, when a case opens with a Case Type, I want to auto-select the active primary workflow and activate the first item so that work begins without human selection. | AC-5.1: *Given* a case opens with Program + Case Type, *Then* the most current active workflow version is selected. AC-5.2: *Given* no published workflow for that Case Type, *Then* the case is created without work items and a clear warning is surfaced to Admin (and to the assigned Supervisor). AC-5.3: *Given* the first work item, *Then* it is immediately visible and actionable by the assigned Worker; KPI clock starts at case open. | [CAM-XXXXX] | FR-WF-001 |
| P0 | As any persona, when viewing a case, I want a workflow progress view so that I can see completed, active, and upcoming items. | AC-6.1: *Given* Case Detail (rendered by MOD-01), *Then* progress shows the ordered sequence with completed, active, and upcoming items. AC-6.2: *Given* a completed item, *Then* completion date and actor are shown. AC-6.3: *Given* an upcoming item, *Then* its name and estimated KPI window are shown based on current progress. | [CAM-XXXXX] | FR-WF-002 — UI rendered by MOD-01; data owned here |
| P0 | As an Admin, when defining workflows, I want each Case Type to support one or more secondary workflows so that real-world case events have a structured response. | AC-7.1: *Given* a Case Type, *Then* zero or more secondary workflows may exist. AC-7.2: *Given* a secondary workflow, *Then* it requires a name, description, triggering event (from an Admin-managed event list), default KPI pause behavior, and an ordered work-item list. AC-7.3: *Given* the Work Item Library + KPI configurator, *Then* secondary workflows reuse the same tools as primary. AC-7.4: *Given* a published secondary workflow, *Then* it is versioned and auditable. | [CAM-XXXXX] | FR-WF-010 |
| P0 | As a Supervisor, when a qualifying event occurs, I want to trigger the corresponding secondary workflow under explicit pause / continue confirmation so that the case responds correctly without ambiguity. | AC-8.1: *Given* I trigger a secondary workflow, *Then* the system shows the workflow name, triggering event, work-item list, and default KPI pause behavior. AC-8.2: *Given* the prompt, *Then* I confirm or override the KPI pause default. AC-8.3: *Given* I confirm, *Then* the secondary's first work item activates and its KPI clock starts. AC-8.4: *Given* the decision, *Then* the primary workflow pauses or continues per the confirmed value. AC-8.5: *Given* only one secondary workflow may be active at a time, *Then* attempting to trigger a second is blocked with a clear message. AC-8.6: *Given* a trigger, *Then* event, actor, KPI pause decision, and timestamp are audited. | [CAM-XXXXX] | FR-WF-011 |
| P0 | As a system, when a secondary workflow completes, I want the case to return to the primary workflow at the point it left off so that the case continues seamlessly. | AC-9.1: *Given* all secondary work items are closed, *Then* the next incomplete primary work item activates. AC-9.2: *Given* primary KPIs were paused, *Then* they resume from the pause point; paused duration is factored into KPI calculation by MOD-03. AC-9.3: *Given* completion + resumption, *Then* both events are logged with timestamps. AC-9.4: *Given* resumption, *Then* the assigned Social Worker and Supervisor are notified (MOD-04). | [CAM-XXXXX] | FR-WF-012 |
| P0 | As an Admin, when defining a secondary workflow, I want to set a default KPI pause behavior so that operations behave consistently unless a Supervisor explicitly overrides. | AC-10.1: *Given* secondary workflow config, *Then* default = pause or continue. AC-10.2: *Given* a trigger, *Then* the Supervisor sees the default with an explicit override step. AC-10.3: *Given* a pause, *Then* paused duration is stored and excluded from KPI elapsed time. AC-10.4: *Given* KPI explainability, *Then* pause periods + reasons are reflected in the payload (MOD-03). | [CAM-XXXXX] | FR-WF-013 |

---

## 9. ⚙️ Non-Functional Requirements

★ Inherits master §9. Module deltas:

**Performance:** Next-item activation latency ≤ 1 s (P95) after prior-item completion. Secondary trigger end-to-end (confirm → activate + pause + audit) ≤ 2 s.

**Reliability:** Idempotent activation on retry — never produces duplicate Active items in a sequence. Engine state is recoverable from the audit log.

**Scalability:** Engine handles concurrent workflows across millions of cases historically without degrading single-case response.

**Observability:** Telemetry events: work_item_activated, work_item_started, work_item_completed, work_item_paused, work_item_resumed, secondary_workflow_triggered, secondary_workflow_completed, primary_resumed.

**Security:** RBAC enforced at engine API — only Supervisors may trigger secondary workflows on cases in their scope. All transitions audited.

---

## 10. 🎨 User Interaction and Design

This module is mostly server-side. UI surfaces it provides hooks into:

- **Workflow progress section** (rendered by MOD-01 Case Detail).
- **Secondary trigger confirmation modal** — owned by MOD-01 (Supervisor surface) but powered by this engine (work-item list, default pause, audit hook).
- **Pause indicators** — paused state must be visible on work items rendered in MOD-01 and in KPI explainability (MOD-03).

UX Principle: never silently advance or pause — every transition has an audit-visible reason.

---

## 11. 🔄 Key Flows

### Flow 1: Case open → primary activation

**Trigger:** Case opened with Program + Case Type.
**Preconditions:** A published primary workflow exists for that Program × Case Type.
**Steps:**
1. Engine selects most current active workflow version.
2. Engine instantiates work items per definition (sequence positions assigned).
3. First item enters Active; remainder Locked.
4. Engine emits `work_item_activated` to MOD-03 (start KPI clock) and MOD-04 (assignment notification).
**Exit state:** Case has one Active work item; all others Locked.
**Handoff:** Worker proceeds via MOD-01 sub-screen.

### Flow 2: Secondary trigger with pause

**Trigger:** Supervisor invokes secondary trigger UI on an active case.
**Preconditions:** Secondary workflow defined for the Case Type; no other secondary currently active.
**Steps:**
1. Engine returns workflow name, work items, default pause behavior, triggering event.
2. Supervisor confirms (or overrides) pause default.
3. Engine pauses primary KPIs (if pause), activates secondary's first item, starts secondary KPI clock.
4. Engine writes audit entry: actor, timestamp, event, pause decision.
**Exit state:** One Active secondary work item; primary KPIs paused or continuing per decision.
**Handoff:** MOD-04 notifies Worker; MOD-03 reflects paused state in explainability.

### Flow 3: Secondary completes → primary resumes

**Trigger:** Last secondary work item completed.
**Preconditions:** Secondary workflow has no remaining incomplete items.
**Steps:**
1. Engine marks secondary complete.
2. Engine reactivates next incomplete primary item.
3. If primary KPIs were paused, they resume; engine sends MOD-03 the paused-duration delta to factor into elapsed time.
4. Engine emits resumption event; MOD-04 notifies Worker + Supervisor.
**Exit state:** Primary workflow active; secondary closed.
**Handoff:** Continue Flow 1 sequential progression.

---

## 12. 🚫 Out of Scope and Deferred

### Out of Scope

- Workflow authoring UI (MOD-07).
- KPI computation (MOD-03).
- AI-driven trigger *detection* — recommendation only (MOD-06); the human action remains the trigger.
- Migrating running cases to a newer workflow version.

### Deferred

- Parallel work items inside a workflow (current model is strictly sequential).
- Multiple concurrent secondary workflows (model allows only one active).
- Worker-initiated secondary triggers (currently Supervisor-only).

---

## 13. 🧬 V-Model Traceability Matrix

★ Structure required at Approval; populated progressively.

| AC ID | Story / Jira | Acceptance Criterion (summary) | Gherkin file path | Test class | Verdict |
| --- | --- | --- | --- | --- | --- |
| AC-2.2 | [CAM-XXXXX] | Locked items cannot be opened | `docs/{FEATURE-ID}/gherkin/wi-002-lock.feature` | --- | --- |
| AC-4.2 | [CAM-XXXXX] | Next-item activation on completion | `docs/{FEATURE-ID}/gherkin/wi-004-sequential.feature` | --- | --- |
| AC-5.2 | [CAM-XXXXX] | No-workflow case creates with warning | `docs/{FEATURE-ID}/gherkin/wf-001-no-workflow.feature` | --- | --- |
| AC-8.5 | [CAM-XXXXX] | Only one secondary at a time | `docs/{FEATURE-ID}/gherkin/wf-011-single-secondary.feature` | --- | --- |
| AC-9.2 | [CAM-XXXXX] | Pause-aware resumption | `docs/{FEATURE-ID}/gherkin/wf-012-resume.feature` | --- | --- |
| AC-10.3 | [CAM-XXXXX] | Paused duration excluded from KPI elapsed | `docs/{FEATURE-ID}/gherkin/wf-013-pause-accounting.feature` | --- | --- |

---

## 14. 🚀 Launch Plan

| Target date | Milestone | Description | Exit criteria |
| --- | --- | --- | --- |
| [YYYY-MM-DD] | ✅ UAT | Internal pilot on reference Ohio Case Types | No P0/P1 sequencing or pause defects on rolling 7-day basis |
| [YYYY-MM-DD] | 🛑 Early Access | Pilot tenant + reference workflows | Successful primary + secondary execution end-to-end |
| [YYYY-MM-DD] | 🛑 Launch | All in-scope tenants | Sequencing + pause accuracy metrics in §5 met |

### Operation Checklist

- [ ] Workflow definitions seeded (Ohio reference)
- [ ] Audit log integration verified for all state transitions
- [ ] MOD-03 contract for paused-duration delta agreed
- [ ] MOD-04 notification contracts verified
- [ ] Backfill plan for in-flight base-Traverse cases (out of scope; assumed greenfield)

---

## 15. 📣 Cross-Functional Impact Checklist

| Team | Prompt | Y/N | Action |
| --- | --- | --- | --- |
| Analytics | Telemetry for engine events? | Y | Wire §9 events. |
| Configuration | Workflow seed data? | Y | Coordinate with MOD-07 for Ohio reference. |
| Permissions | New permissions? | Y | Supervisor secondary-trigger permission. |
| Reporting | New reports? | Y | MOD-05 will consume engine data. |

---

## 16. ⚠️ Risks and Mitigations

| Risk | Likelihood / Impact | Mitigation |
| --- | --- | --- |
| Race conditions activating next item create duplicate Active items | M / H | Idempotent activation; integration tests under concurrency. |
| Pause accounting drift across module boundaries | M / H | Single canonical pause-period record consumed by MOD-03; reconciliation tests. |
| Workflow definition change while a case is mid-flight | M / M | Cases pinned to definition version at instantiation; migration deferred. |
| Misuse of secondary trigger by Supervisors | L / M | Required reason on KPI pause override; audit + MOD-05 reporting visibility. |
| Engine becomes a hidden source-of-truth divergent from base Traverse case state | M / H | Engine writes back to base case state where it owns the field; explicit ownership contract. |

---

## 17. ❓ Open Questions

| Question | Impact / Blocks | Date raised | Owner / Needed by |
| --- | --- | --- | --- |
| Confirm exact write-back contract between engine and base Traverse case state. | Blocks AC-1.1 implementation. | 2026-04-25 | Dev Lead / before design-feature |
| Confirm whether a Supervisor can cancel an active secondary workflow mid-flight, and the pause/resume semantics if so. | Edge case not covered in source. | 2026-04-25 | Product |
| Confirm idempotency keys for engine APIs (e.g., workflow-instance ID + sequence position). | Blocks reliability tests. | 2026-04-25 | Dev Lead |
| Confirm migration policy for running cases when a workflow version is republished. | Affects v1 vs v2 scope. | 2026-04-25 | Product |
| Confirm whether "Paused (with reason)" requires a structured reason taxonomy or freeform text. | Affects audit + MOD-05 reporting. | 2026-04-25 | Product + MOD-05 lead |

---

## 18. 💬 FAQs

**Q:** Why only one active secondary workflow at a time?
**A:** Source PRD §7.3 + §9.3 enforce this to keep the case state model deterministic and predictable for users and auditors. Multiple concurrent secondaries are deferred.

**Q:** Who chooses the primary workflow version on a new case?
**A:** The engine — it auto-selects the most current active version for the Program × Case Type at case-open time (FR-WF-001).

**Q:** What happens if KPI engine is unavailable when an item activates?
**A:** Master §9 mandates graceful degradation — the engine still activates, the work item is workable, and KPI status renders as a clear degraded state until MOD-03 catches up.

---

## 19. 🧭 Decision Log

| # | Decision | Rationale / Alternatives | Date | Decided by |
| --- | --- | --- | --- | --- |
| 1 | Strict sequential model — no parallel work items in v1. | Matches source §7.3 + §9.1 + reduces KPI ambiguity. Parallel deferred. | 2026-04-25 | Product |
| 2 | One active secondary at a time. | Source §9.3; simplifies pause accounting and UX. | 2026-04-25 | Product |
| 3 | Cases pin to workflow definition version at instantiation. | Avoids destabilising in-flight cases on Admin republish. | 2026-04-25 | Product |

---

## 20. 📝 Change Log

| Date | Changed by | Section(s) | Reason |
| --- | --- | --- | --- |
| 2026-04-25 | Kendrew Peacey | All | Initial draft — derived from source PRD §9. |

---

## 21. 🧾 Informed By (Knowledge Lineage)

★ **V-MODEL MANDATORY**

| Document | Path / link | Integrity (SHA-256 first 8 hex) |
| --- | --- | --- |
| Master PRD | [[PRD - Northwoods Traverse Workspace]] | sha256:[computed at approval] |
| Source PRD | `00 - Inbox/Northwoods_Traverse_PRD_v2.0.docx` (§9) | sha256:[computed at approval] |

---

## 22. ✅ Approval and Checksum

★ **V-MODEL MANDATORY**

### Approval checklist

- [ ] Problem statement clear
- [ ] Every job story passes INVEST
- [ ] Every AC in Given / When / Then
- [ ] NFRs quantified
- [ ] Out of Scope / Deferred explicit
- [ ] §17 Open Questions resolved
- [ ] Dev Lead feasibility sign-off IN PLACE
- [ ] Estimation Checkpoint 1 IN PLACE
- [ ] Traceability matrix populated
- [ ] Decision / Change / Informed By current

### Approval record

| Field | Value |
| --- | --- |
| Approved by | [PM name and role] |
| Approval date | [YYYY-MM-DD] |
| Document SHA-256 | sha256-[computed at approval] |
| Pipeline tracker entry | [Path] |

---

*Template version 1.0 · Camis V-Model–Aligned PRD · Compatible with `camis-v-model:draft-prd` skill v2.1.0*
