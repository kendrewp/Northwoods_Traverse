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
# PRD — Traverse KPI / SLA Policy Engine (MOD-03)

> **V-Model–Aligned PRD.** Inherits from [[PRD - Northwoods Traverse Workspace]]. Source: §10 (KPI / SLA Policy Engine).

---

## 1. Product Overview

| Field | Value |
| --- | --- |
| 📅 Target date | [QX YYYY — Phase 1 of master launch plan] |
| 🟡 Document status | Draft |
| 🧭 Team | PM / Product / Dev Lead / Dev / QA: [Names] |
| 🗃️ Work tracker (Jira epic) | [CAM-XXXXX] |
| 📎 Parent PRD | [[PRD - Northwoods Traverse Workspace]] |
| 🔗 Resources | [[Northwoods-Traverse]] · `00 - Inbox/Northwoods_Traverse_PRD_v2.0.docx` (§10) · [[PRD - Traverse Workflow Execution Engine]] · [[PRD - Traverse Compliance Framework]] |

---

## 2. 🎯 Objective and Problem Alignment

### Overview

Build the configurable engine that computes due times and risk states (Green / Yellow / Red / Breached) for every work item, using versioned policies scoped per Program × Case Type × Work Item, business-time calendars, and pause-aware elapsed-time accounting. Produces an **explainability payload** that powers the "Why is this red?" panel everywhere.

### Problem Statement

KPI policies today vary by program, case type, and state and are hard-coded — driving drift and mistrust. There is no auditable explanation for why a work item is at risk. Without a single, transparent engine, the cockpit, reporting, AI, and notifications all compute slightly different versions of the truth, and any state regulation change requires engineering.

### Importance

- One canonical engine = one canonical truth across cockpit, reporting, AI, and notifications.
- Configurable + versioned + scoped policies = Admin-driven (codeless) updates with no retroactive surprises.
- Business-time calendars = honest KPIs that respect working hours, weekends, holidays, DST.
- Explainability payload = every status answers "why is this red?" for users and auditors.

### High-Level Approach

- **KPI Policy Object** scoped to Program Type × Case Type × Work Item, versioned, with status (active/inactive) + effective start date.
- **Timing definition:** start condition (Case Opened / Prior Item Completed) → stop condition (Item Completed) → pause conditions (status-based with reason codes) → target duration → thresholds (yellow/red as time-remaining or %-consumed) → time basis (calendar vs business time).
- **Business-time calendars** per state/program: working hours, weekends, holidays; DST-stable; UTC-stored, locally displayed.
- **Explainability payload:** start_at, stop_at, total_elapsed, paused_duration, remaining_duration, due_at, yellow_at, red_at, breached_at, policy_id/version, breach flag, override indicator.
- **Manual override** with governance — visible label, audit, Admin restriction by program/case-type/work-item.
- **Misconfiguration detection** — policies that cannot evaluate produce a safe "KPI Unavailable" state and route an alert to Admin.

---

## 3. 👥 Stakeholders and Personas

- **MOD-02 Workflow Engine** — primary upstream; provides activation/completion/pause events.
- **MOD-01 Workspace UI** — primary downstream; renders status + explainability panel.
- **MOD-04 Notifications** — consumes threshold-crossing events.
- **MOD-05 Reporting** — consumes explainability payload + KPI history for analytics.
- **MOD-06 AI Copilot** — consumes payload for plain-language explanation, breach prediction, anomaly flagging.
- **MOD-07 Admin Configuration Suite** — Admins author policies; UI lives there.
- **MOD-10 Compliance Framework** — supplies federal/state rule layer that drives default policies and timing constraints.

---

## 4. 🎬 Target Use Cases

- **Standard policy:** Admin defines a 7-day target with yellow at 2 days remaining and red at 6 hours remaining for "Initial Contact" in Child Welfare Investigation.
- **Percentage-based thresholds:** Admin uses 70 %-elapsed for yellow and 90 %-elapsed for red on a long-running adult-aging case-plan review.
- **Business-time calendar:** A 48-hour SLA on a behavioral-health intake uses business hours only — clock pauses outside 0900–1700 weekdays and on state holidays.
- **Pause-aware accounting:** A work item enters "Waiting on Client" status — clock pauses; resumes when status changes back; paused_duration is excluded from total_elapsed.
- **Override:** Supervisor sets a justified due-date override on a single work item; UI shows the override badge; explainability payload includes the override indicator + new due_at.
- **Misconfiguration:** Admin publishes a policy with start ≥ due — engine detects and rejects; running work items show "KPI Unavailable" until fixed; alert routes to Admin exception queue.

---

## 5. 📊 Success Conditions and Metrics

### Goals

1. Single, canonical KPI status surfaces consistently across cockpit, reporting, AI, and notifications.
2. Explainability payload accompanies every KPI status — no mystery scores.
3. Policy changes are versioned and never retroactively alter in-flight items unless explicitly configured.
4. Misconfigured policies fail safe (no incorrect coloring shown to end users).

### Non-Goals

- Authoring UI for policies — owned by MOD-07.
- AI plain-language explanation — consumes the payload; owned by MOD-06.
- Notification delivery — owned by MOD-04 (this engine emits events only).
- Compliance rule library — owned by MOD-10; this engine consumes it.

### Success Metrics

| Goal | Metric (quantified) |
| --- | --- |
| KPI consistency | 0 cross-surface KPI mismatches in audit sample (cockpit vs reporting vs notification) |
| Explainability coverage | 100 % of work items with a KPI render an explainability payload |
| Recalculation latency | KPI status recalculation ≤ 60 s after triggering event (master §9) |
| Pause accuracy | Paused duration error ≤ 1 s vs ground truth in test cases |
| Misconfiguration safety | 0 in-production cases of incorrect coloring caused by misconfigured policies |

### Guardrails

- Stored timestamps are UTC; user time zones are display-only.
- Business-time calculations stable across DST.
- Policies versioned — no silent retroactive change.
- A policy that cannot evaluate must produce "KPI Unavailable", never a colored guess.

---

## 6. 🤔 Assumptions

- MOD-02 emits work-item lifecycle events (activated, paused, resumed, completed) with reliable timestamps. — ❓ unvalidated
- Business calendars per state/program are authored in MOD-07 and seeded with Ohio reference. — ❓ unvalidated
- MOD-10 supplies federal/state rule layer that bounds policy timing (state must meet or exceed federal minimums). — ❓ unvalidated
- "Status-based pauses" align with MOD-02 lifecycle states. — ❓ unvalidated

---

## 7. 🔗 Dependencies and V-Model Gates

★ **V-MODEL MANDATORY**

| Dependency | Type | Status | Impact / Notes |
| --- | --- | --- | --- |
| MOD-02 Workflow Engine | Feature (internal) | DRAFT | Hard blocker — provides lifecycle events. |
| MOD-07 Admin Configuration Suite | Feature (internal) | DRAFT | Hard blocker — authoring UI for policies + calendars. |
| MOD-10 Compliance Framework | Feature (internal) | DRAFT | Hard blocker for v1 multi-state — supplies rule layer. |
| MOD-04 Notifications | Feature (internal) | DRAFT | Soft — consumes threshold events. |
| Audit log infrastructure | Shared | PENDING | Records overrides + status transitions. |
| Dev Lead technical feasibility sign-off | V-Model gate | PENDING | ★ |
| T-shirt estimate — Estimation Checkpoint 1 | V-Model gate | PENDING | ★ |

---

## 8. 📋 Requirements

★ **V-MODEL MANDATORY**

| Priority | Job Story (INVEST) | Acceptance Criteria (G/W/T) | Jira | Notes / Source |
| --- | --- | --- | --- | --- |
| P0 | As an Admin, when I define a KPI policy, I want a configurable, versioned object scoped to Program × Case Type × Work Item so that policies can evolve without retroactive impact. | AC-1.1: *Given* a policy, *Then* it stores name, description, scope (Program / Case Type / Work Item), effective start date, status (active/inactive), and version. AC-1.2: *Given* timing, *Then* it stores start condition, target duration, threshold configuration (G/Y/R/breach), and time basis (calendar vs business). AC-1.3: *Given* a republish, *Then* in-flight work items keep their original policy version unless explicitly migrated. | [CAM-XXXXX] | FR-KPI-001 |
| P0 | As a system, when a work item activates, I want defined start / stop / pause conditions so that the clock behaves predictably. | AC-2.1: *Given* the first work item, *Then* start = On Case Opened. AC-2.2: *Given* subsequent items, *Then* start = On Prior Work Item Completed. AC-2.3: *Given* item completion, *Then* stop = On Work Item Completed. AC-2.4: *Given* configurable status-based pauses with reason codes (e.g., Waiting on Client, Waiting on External Provider), *Then* the clock pauses while the status is held; reason is captured. | [CAM-XXXXX] | FR-KPI-002 |
| P0 | As a system, when a policy has a target and thresholds, I want to compute due_at, yellow_at, red_at, breached_at so that the cockpit can render risk consistently. | AC-3.1: *Given* a target expressed in days or hours, *Then* due_at is computed correctly. AC-3.2: *Given* yellow as time-remaining (e.g., 2 days) or %-consumed (e.g., 70 %), *Then* yellow_at is derived. AC-3.3: *Given* red as time-remaining (e.g., 6 hours) or %-consumed (e.g., 90 %), *Then* red_at is derived. AC-3.4: *Given* a work item not completed by due_at, *Then* it is Breached. AC-3.5: *Given* threshold change while in flight, *Then* current items keep their original computed thresholds. | [CAM-XXXXX] | FR-KPI-003 |
| P0 | As a system, when computing elapsed time, I want business-time calendars so that working hours, weekends, holidays, and DST are honored. | AC-4.1: *Given* a calendar scoped per state / program, *Then* it stores working hours, weekends, holidays. AC-4.2: *Given* business-time policies, *Then* elapsed time excludes non-working periods. AC-4.3: *Given* DST transitions, *Then* business-time calculations remain stable. AC-4.4: *Given* storage, *Then* timestamps are UTC; UI converts to user time zone with explicit context. | [CAM-XXXXX] | FR-KPI-004 |
| P0 | As any persona, when I view a work item's KPI, I want a complete explainability payload so that "Why is this red?" has an evidence-backed answer. | AC-5.1: *Given* a work item with a KPI, *Then* the payload contains start_at, stop_at (if completed), total_elapsed, paused_duration, remaining_duration, due_at, yellow_at, red_at, breached_at, policy_id/version, breach flag, override indicator. AC-5.2: *Given* the cockpit explainability panel (FR-WKS-033), *Then* it renders the payload without further computation. AC-5.3: *Given* the AI plain-language explanation (FR-AI-002 in MOD-06), *Then* it consumes this payload. | [CAM-XXXXX] | FR-KPI-005 |
| P1 | As an authorised role, when an exception is justified, I want to override a work item's due date with audit + governance so that flexibility doesn't break compliance. | AC-6.1: *Given* an override, *Then* an indicator is visible in UI (FR-WKS-044) and included in the explainability payload. AC-6.2: *Given* Admin restriction settings, *Then* override availability is gated per program / case type / work-item type. AC-6.3: *Given* an override, *Then* an audit entry records actor, original due, new due, optional reason, and timestamp. | [CAM-XXXXX] | FR-KPI-006 |
| P0 | As an Admin, when a policy is misconfigured, I want detection that fails safe + routes a clear error so that no end user sees incorrect coloring. | AC-7.1: *Given* a misconfigured policy (e.g., negative duration, start ≥ due, contradictory thresholds), *Then* the engine refuses to evaluate it and presents an actionable Admin error. AC-7.2: *Given* an in-flight work item bound to a misconfigured policy, *Then* its UI status shows "KPI Unavailable" — never a guessed color. AC-7.3: *Given* the misconfiguration, *Then* an alert routes to the Admin exception queue. | [CAM-XXXXX] | FR-KPI-007 |

---

## 9. ⚙️ Non-Functional Requirements

★ Inherits master §9. Module deltas:

**Performance:** Status recalculation ≤ 60 s (master). Single-item explainability payload generation ≤ 200 ms (P95). Bulk recompute on calendar change must not block read-side queries.

**Reliability:** Idempotent recalculation. State recovery from MOD-02 lifecycle event log + policy snapshot. Calendar misconfigurations never crash recalculation — they fall back to "KPI Unavailable".

**Scalability:** Recalc volume scales with work-item churn — must not degrade list operations under millions of historical items (master §9 Scale). Indexed KPI status fields per master.

**Security:** Override capability behind RBAC. Reason codes captured for audit.

**Observability:** Telemetry: kpi_recalculated, threshold_crossed, policy_evaluated, override_applied, kpi_unavailable. Per-policy metrics — recompute rate, evaluation error rate, average evaluation time.

---

## 10. 🎨 User Interaction and Design

This module is a service. It does not own UI surfaces; it powers them.

- **KPI Status badges** rendered by MOD-01 everywhere.
- **Explainability Panel** rendered by MOD-01 (FR-WKS-033); content shape defined here.
- **"KPI Unavailable" state** must use the MOD-01 standard non-color label/icon and be visually distinct from Green/Yellow/Red/Breached.
- **Policy Editor** lives in MOD-07 — UX principle: simulate-before-publish (Admin can run policy against open cases to preview impact).

---

## 11. 🔄 Key Flows

### Flow 1: Work item activation → first KPI evaluation

**Trigger:** MOD-02 emits `work_item_activated`.
**Preconditions:** Bound policy exists and is active; calendar resolved.
**Steps:**
1. Engine fetches active policy version.
2. Engine computes start_at, due_at, yellow_at, red_at.
3. Engine writes initial status (Green) + explainability payload.
4. Engine emits `kpi_evaluated` for downstream consumers.
**Exit state:** Work item has KPI status and payload.

### Flow 2: Threshold crossing

**Trigger:** Time advances past yellow_at (or red_at, or due_at).
**Preconditions:** Work item is Active and not paused.
**Steps:**
1. Recalculation tick (or event-driven evaluation) detects crossing.
2. Status updates to Yellow / Red / Breached.
3. Engine emits `threshold_crossed` event with payload.
4. MOD-04 generates notification per FR-NOTIF-001.
**Exit state:** New status + payload visible across surfaces.

### Flow 3: Override

**Trigger:** Authorised actor submits override on a work item.
**Preconditions:** RBAC + Admin restriction permits.
**Steps:**
1. Engine validates new due_at vs policy bounds.
2. Engine records override (actor, original due, new due, reason, timestamp).
3. Status recomputes against new due_at; payload updated with override indicator.
4. Audit + telemetry events emitted.
**Exit state:** Work item shows override badge; explainability payload reflects.

### Flow 4: Misconfigured policy

**Trigger:** Policy publish in MOD-07 (or runtime evaluation discovers contradiction).
**Preconditions:** None.
**Steps:**
1. Engine validates policy on publish; rejects with actionable error if invalid.
2. For runtime cases (calendar removed, etc.), engine sets work-item status to "KPI Unavailable" and routes alert.
3. End-user UI shows safe state.
**Exit state:** Admin can fix; no incorrect coloring shown.

---

## 12. 🚫 Out of Scope and Deferred

### Out of Scope

- Authoring UI (MOD-07).
- AI plain-language explanation (MOD-06).
- Notification delivery (MOD-04).
- Cross-tenant rule sharing — MOD-10 owns rule library.

### Deferred

- Bulk policy migration tooling for in-flight cases (manual or per-case migration only in v1).
- Per-user policy preferences (none — policies are org / Admin-managed).
- Probabilistic / predictive KPI estimates beyond simple time-remaining (MOD-06 owns prediction).

---

## 13. 🧬 V-Model Traceability Matrix

| AC ID | Story / Jira | Summary | Gherkin file path | Test class | Verdict |
| --- | --- | --- | --- | --- | --- |
| AC-1.3 | [CAM-XXXXX] | No retroactive policy change | `docs/{FEATURE-ID}/gherkin/kpi-001-versioning.feature` | --- | --- |
| AC-2.4 | [CAM-XXXXX] | Status-based pauses with reason | `docs/{FEATURE-ID}/gherkin/kpi-002-pause.feature` | --- | --- |
| AC-3.4 | [CAM-XXXXX] | Breached on miss of due_at | `docs/{FEATURE-ID}/gherkin/kpi-003-breach.feature` | --- | --- |
| AC-4.3 | [CAM-XXXXX] | DST stability | `docs/{FEATURE-ID}/gherkin/kpi-004-dst.feature` | --- | --- |
| AC-5.1 | [CAM-XXXXX] | Explainability payload shape | `docs/{FEATURE-ID}/gherkin/kpi-005-payload.feature` | --- | --- |
| AC-7.2 | [CAM-XXXXX] | KPI Unavailable safe state | `docs/{FEATURE-ID}/gherkin/kpi-007-unavailable.feature` | --- | --- |

---

## 14. 🚀 Launch Plan

| Target | Milestone | Description | Exit |
| --- | --- | --- | --- |
| [YYYY-MM-DD] | ✅ UAT | Engine running on Ohio reference policies | No P0/P1 status / pause defects |
| [YYYY-MM-DD] | 🛑 Early Access | Pilot tenant | Cross-surface KPI consistency verified |
| [YYYY-MM-DD] | 🛑 Launch | All tenants | Master metrics trending positive |

### Operation Checklist

- [ ] Ohio reference policies seeded
- [ ] Business calendars seeded (Ohio + federal holidays)
- [ ] Override governance defaults set per program
- [ ] Misconfiguration alert routing wired to Admin queue
- [ ] Telemetry dashboards live for engine health

---

## 15. 📣 Cross-Functional Impact Checklist

| Team | Prompt | Y/N | Action |
| --- | --- | --- | --- |
| Analytics | Telemetry? | Y | Engine events + per-policy metrics. |
| Configuration | Reference policies? | Y | Coordinate Ohio dataset with MOD-07. |
| Permissions | Override roles? | Y | RBAC for override + Admin restriction surface. |
| Reporting | KPI history? | Y | Schema for MOD-05 to consume. |

---

## 16. ⚠️ Risks and Mitigations

| Risk | L/I | Mitigation |
| --- | --- | --- |
| Cross-surface KPI mismatch | M / H | Single canonical engine; consistency tests across UI / reporting / notifications. |
| DST / calendar drift | M / H | Test fixtures cover transitions; UTC-stored, locally-displayed convention. |
| Policy versioning surprises in-flight cases | M / H | Pin version at activation; explicit migration only. |
| Misconfigured policy escapes detection | L / H | FR-KPI-007 fail-safe + Admin simulate-before-publish in MOD-07. |
| Override abuse hides chronic breach | M / M | All overrides audited + reported in MOD-05; Admin restriction surface. |

---

## 17. ❓ Open Questions

| Question | Impact | Date | Owner |
| --- | --- | --- | --- |
| Confirm exact pause-reason taxonomy (MOD-02 status alignment). | Blocks AC-2.4. | 2026-04-25 | Product + MOD-02 lead |
| Confirm "KPI Unavailable" rendering style + accessibility treatment. | Blocks MOD-01 contract. | 2026-04-25 | UX lead |
| Confirm whether overrides may extend due_at beyond compliance ceiling (federal/state minimums in MOD-10). | Blocks AC-6.x. | 2026-04-25 | Compliance + Product |
| Confirm recalculation strategy: pure event-driven, periodic tick, or hybrid. | Blocks NFR §9 implementation. | 2026-04-25 | Dev Lead |
| Confirm calendar change propagation policy for in-flight items. | Blocks AC-3.5 vs calendar edge cases. | 2026-04-25 | Product |

---

## 18. 💬 FAQs

**Q:** Why does this engine emit events instead of calling notifications directly?
**A:** Separation of concerns — the engine is responsible for *truth*, MOD-04 owns *delivery*. Lets each module evolve independently.

**Q:** Why are policies pinned to version at work-item activation?
**A:** Avoids retroactive changes that would surprise workers and auditors mid-case.

**Q:** What is "KPI Unavailable"?
**A:** A safe degraded state shown when a policy cannot evaluate (misconfiguration, missing calendar, etc.). It is visually distinct from G/Y/R/breached and prevents incorrect coloring.

---

## 19. 🧭 Decision Log

| # | Decision | Rationale | Date | By |
| --- | --- | --- | --- | --- |
| 1 | Single canonical engine — no per-surface KPI logic. | Eliminates cross-surface drift. | 2026-04-25 | Product |
| 2 | Pin policy version at work-item activation. | Avoids retroactive surprises. | 2026-04-25 | Product |
| 3 | Misconfigurations fail safe to "KPI Unavailable", not to Green. | Master guardrails. | 2026-04-25 | Product |

---

## 20. 📝 Change Log

| Date | By | Sections | Reason |
| --- | --- | --- | --- |
| 2026-04-25 | Kendrew Peacey | All | Initial draft — derived from §10. |

---

## 21. 🧾 Informed By

★ **V-MODEL MANDATORY**

| Document | Path | SHA-256 |
| --- | --- | --- |
| Master PRD | [[PRD - Northwoods Traverse Workspace]] | sha256:[at approval] |
| Source PRD | `00 - Inbox/Northwoods_Traverse_PRD_v2.0.docx` (§10) | sha256:[at approval] |

---

## 22. ✅ Approval and Checksum

★ **V-MODEL MANDATORY**

### Approval checklist

- [ ] Problem statement clear
- [ ] INVEST + G/W/T satisfied
- [ ] NFRs quantified
- [ ] Out of Scope / Deferred explicit
- [ ] Open Questions resolved
- [ ] Dev Lead feasibility IN PLACE
- [ ] Estimation IN PLACE
- [ ] Traceability matrix populated

### Approval record

| Field | Value |
| --- | --- |
| Approved by | [Name] |
| Approval date | [YYYY-MM-DD] |
| SHA-256 | sha256-[at approval] |
| Pipeline tracker | [Path] |

---

*Template v1.0 · Camis V-Model–Aligned PRD · Compatible with `camis-v-model:draft-prd` skill v2.1.0*
