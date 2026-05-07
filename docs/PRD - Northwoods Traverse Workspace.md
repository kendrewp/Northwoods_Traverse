---
categories:
  - "[[Product Requirements]]"
subjects:
  - "[[Software Development]]"
  - "[[V-Model PDLC]]"
  - "[[AI]]"
  - "[[Northwoods]]"
status: draft
created: 2026-04-25
---
# PRD — Northwoods Traverse Workspace (Master)

> **V-Model–Aligned PRD.** Structure follows `VModel_PRD_Template.docx` (v1.0). Sections marked ★ are mandatory before Approved status.
>
> **Master / parent PRD.** This document captures the cross-cutting vision, personas, hierarchy, scope, NFRs, security, and conceptual data model for the Traverse Operational Dashboard / Cockpit + Admin Configuration Suite. Detailed functional requirements live in the **module PRDs** indexed in §20 below. Source: `Northwoods_Traverse_PRD_v2.0.docx` (Northwoods, S. Ryczek, April 2026).

---

## 1. Product Overview

| Field                        | Value                                                                                                                                |
| ---------------------------- | ------------------------------------------------------------------------------------------------------------------------------------ |
| 📅 Target date               | [QX YYYY — phased per module]                                                                                                        |
| 🟡 Document status           | Draft — for product design and development planning                                                                                  |
| 🧭 Team                      | PM: S. Ryczek (Northwoods) · Product: [Name] · UX: [Name] · Dev Lead: [Name] · Dev: [Names] · QA: [Name]                             |
| 🗃️ Work tracker (Jira epic)  | [CAM-XXXXX]                                                                                                                          |
| 📎 Parent PRD                | Root — supersedes BRD v1.2 (2026-02-12)                                                                                              |
| 🔗 Resources                 | [[Northwoods-Traverse]] · `00 - Inbox/Northwoods_Traverse_PRD_v2.0.docx` · [[Northwoods]] · [[Banyan Software]]                       |

---

## 2. 🎯 Objective and Problem Alignment

### Overview

Traverse is a role-based **Workspace** for human-services frontline teams that surfaces the right work at the right time, drives sequential KPI-tracked workflows, and embeds AI assistance behind human-in-the-loop guardrails. Two capability areas: an **End-User Workspace** for Social Workers, Supervisors, and Directors, and an **Admin Configuration Suite** for codeless program / workflow / KPI / user / AI governance.

### Problem Statement

Human-services workers manage high-volume, high-stakes caseloads where missed time-based KPIs directly impact client outcomes, funding compliance, and staff performance. Today they juggle multiple screens, manual spreadsheets, and memory-based tracking; supervisors learn about issues *after* KPIs miss; directors lack real-time program rollups; admins cannot adapt KPI policies or workflow definitions without engineering changes; notifications are noisy or absent; and there is no auditable explanation for *why* a work item is at risk.

### Importance

- Lifts frontline productivity by replacing search-for-what's-next with a prioritized cockpit.
- Reduces missed KPIs through structured sequential workflows + proactive risk awareness.
- Gives Supervisors real-time capacity / KPI-risk signals to balance workload before breach.
- Gives Directors program-level compliance rollups without manual reporting.
- Enables Admins to configure programs, workflows, and KPI policies without dev cycles.
- Embeds AI assistance that is permission-aware, evidence-grounded, and user-controlled.

### High-Level Approach

- Three-tier hierarchy (Director → Supervisor → Social Worker) plus an out-of-hierarchy Admin and a future Deputy Director / state tier.
- Sequential **primary workflow** per Case Type, with event-triggered **secondary workflows** that run alongside one at a time.
- KPI policies are codeless, scoped, versioned, and explainable — every status answers "why is this red?".
- Ten AI features embedded across personas, governed by Admin AI controls and degraded-mode fallback.
- Multi-tenant and multi-state ready, with Ohio (ODJFS) as the reference state.

---

## 3. 👥 Stakeholders and Personas

The Workspace is built around a three-tier operational hierarchy plus a configuration persona and two future / gated personas.

| Tier | Persona | Scope |
| --- | --- | --- |
| 1 | **P3 — Director** | Oversees multiple Programs and Supervisors. Read-only consumer of cross-program KPI rollups. |
| 2 | **P2 — Supervisor** | Manages one or more Programs. Assigns work, balances workload, monitors KPI risk, handles escalations, manages intake queues, approves exceptions and KPI overrides. |
| 3 | **P1 — Social Worker / Caseworker** | Owns a caseload within a single Program. Executes sequential work items, documents interactions, tracks personal KPI compliance. |
| — | **P4 — Admin** | Configures Programs, Case Types, Work Item Library, Workflows, KPI policies, users, notifications, AI governance. Outside the operational hierarchy. |
| — | **P5 — QA / Compliance (placeholder)** | Future. Read-only audit / compliance access with masking and export. |
| State (gated) | **Deputy Director** | State-level oversight where `stateManagement.enabled = true`. Read-only statewide view across counties. |

Detailed persona responsibilities and key capabilities: see source PRD §5.2 and Module 12 (Deputy Director).

---

## 4. 🎬 Target Use Cases

- **Social Worker daily start:** Worker lands on Workspace → KPI summary tiles + AI Briefing Card → top-of-list breached work item → completes via embedded sub-screen without navigating away.
- **Supervisor mid-day rebalance:** Supervisor sees a worker trending red on capacity + KPI risk → opens AI Workload Balancing recommendation → reassigns two cases inline.
- **Secondary workflow trigger:** Failure-to-Pay event detected mid-case → Supervisor triggers secondary workflow → Admin's default pause behavior applied (Supervisor may override) → primary KPI clocks pause.
- **Director monthly review:** Director opens cross-program rollup → drills program → reads AI Program Health Narrative → identifies a Supervisor with elevated breach rate.
- **Admin policy update:** Admin updates a KPI threshold via Configurator → simulates impact on open cases → publishes new policy version with audit trail.
- **Deputy Director statewide view:** Deputy lands on statewide dashboard → drills a county that is trending red → reviews Director-level analytics (read-only).
- **Field visit:** Social Worker opens calendar event for a Home Visit → taps Get Directions → device hands off to Apple/Google/Waze with destination pre-populated.
- **Compliance audit:** QA persona pulls audit log + KPI history for a closed case to demonstrate state regulatory compliance.

---

## 5. 📊 Success Conditions and Metrics

### Goals

1. Workers see "what is critical today" on landing without searching.
2. Sequential workflows drive on-time KPI compliance across all in-scope Programs.
3. Supervisors detect and rebalance at-risk workers *before* breach.
4. Directors get program-level rollups without manual reporting.
5. Admins adapt programs / workflows / KPI policies without engineering involvement.
6. AI assists across personas without trust loss or compliance incidents.

### Non-Goals

- Replacing core case-management features (Client, Case, Task primitives are assumed to exist in base Traverse).
- Autonomous AI decisioning for eligibility / clinical determination.
- Offline-first mobile.
- Full content-authoring / LMS.
- Integration with every external system — only defined APIs are in scope.
- Full QA/Compliance persona feature set (placeholder only in initial release).

### Success Metrics

| Goal | Metric (quantified) |
| --- | --- |
| Reduce overdue work items | −25% overdue work-item rate within 90 days of rollout |
| Improve on-time KPI completion | +15% on-time completion rate for KPI-tracked work items |
| Faster time-to-first-action | Reduction in average time-to-first-action on newly opened cases [target TBD] |
| Reduce daily prioritization time | Telemetry-measured reduction in time spent finding/prioritizing work [target TBD] |
| Supervisor time saved | Reduction in workload-balancing + escalation handling time [target TBD] |
| Workspace satisfaction | CSAT > 4.2 / 5 post-rollout |

### Guardrails

- AI must never auto-send external communications or auto-write records without explicit user confirmation.
- AI must not reference records/fields outside the user's RBAC / row-level / field-level permissions.
- KPI engine unavailability must not block Workspace load — degraded UI required.
- No PII in URLs, browser history, or default email subject lines.
- Notification volume must not exceed configured per-persona caps (alert fatigue ceiling — see Module 4).

---

## 6. 🤔 Assumptions

- Base Traverse provides core entities (Client, Case, Task / Work Item, User, status tracking). — ❓ unvalidated
- A work item has a lifecycle and can be assigned to user or queue. — ❓ unvalidated
- Cases carry Program Type + Case Type attributes that drive workflow selection and KPI scope. — ❓ unvalidated
- The platform can store and evaluate time-based KPI policies including pause/resume semantics. — ❓ unvalidated
- Users authenticate via existing IdP (internal auth or SSO) and can be assigned roles + personas. — ❓ unvalidated
- Telemetry events can be captured for adoption + performance measurement. — ❓ unvalidated
- Ohio (ODJFS) is the reference state for the initial release; per-state expansion is incremental and configuration-driven. — ❓ unvalidated

---

## 7. 🔗 Dependencies and V-Model Gates

★ **V-MODEL MANDATORY**

| Dependency | Type | Status | Impact / Notes |
| --- | --- | --- | --- |
| Base Traverse domain entities (Client / Case / Work Item / User) | Feature (internal) | IN PROGRESS | Hard blocker for every module — Workspace overlays these primitives. |
| Existing Identity Provider (SSO / internal auth) | External system | PENDING | Drives RBAC + persona assignment. Required by Module 7 (Admin) and Module 1 (Workspace). |
| Telemetry / observability platform | External system | PENDING | Required for §5 success metrics + Module 5 reporting. |
| AI service provider (model + safety stack) | External system | PENDING | Required by Module 6 (AI Copilot). PHI/PII redaction agreement and no-training contract clause required. |
| Ohio ODJFS reference dataset (default Program/Case Types + KPI policies) | Content | PENDING | Seeds Module 7 default config. |
| Dev Lead technical feasibility sign-off | V-Model gate | PENDING | ★ Required before Approval. Run design-feature feasibility review (Phase 1.5). |
| T-shirt estimate — Estimation Checkpoint 1 | V-Model gate | PENDING | ★ Required before Approval per module. Run manage-estimate per child PRD. |

---

## 8. 📋 Requirements

★ **V-MODEL MANDATORY** — at master level, requirements are stated as **module-level outcomes**. Detailed job stories with Given/When/Then acceptance criteria live in each module PRD (§20 index).

| Priority | Module-Level Outcome (master) | Owning Module PRD |
| --- | --- | --- |
| P0 | As a Social Worker / Supervisor / Director, when I log in, I want a role-appropriate operational cockpit so that I can act on prioritized work without searching. | Module 1 — Workspace UI Foundation |
| P0 | As a Social Worker, when I work a case, I want my work items to activate in sequence so that I can never miss the next required step. | Module 2 — Workflow Execution Engine |
| P0 | As any persona, when I view a work item, I want a transparent KPI status with explainability so that I can trust and audit "why is this red?". | Module 3 — KPI / SLA Policy Engine |
| P0 | As a worker / supervisor / director, when a KPI threshold crosses, I want a calibrated notification (with escalation ladder) so that issues surface before breach. | Module 4 — Notifications & Escalations |
| P1 | As any persona, when I need to inspect performance, I want a persona-scoped reporting surface with export/scheduled-report support. | Module 5 — Reporting & Analytics |
| P1 | As any persona, when I am working, I want AI assistance (briefings, drafts, Q&A, recommendations) under human-in-the-loop control so that my work is faster but never automated past my judgement. | Module 6 — AI Copilot Suite |
| P0 | As an Admin, when policy or workflow needs to change, I want a codeless configurator so that I can ship updates without engineering. | Module 7 — Admin Configuration Suite |
| P1 | As any persona, when I need to find a case / work item / person, I want NL + keyword search with conversational results so that I get answers, not lists. | Module 8 — Universal Search |
| P1 | As a Social Worker, when I plan field work, I want a calendar with visit-aware events and turn-by-turn directions so that I can run my day. | Module 9 — Calendar & Field Directions |
| P0 | As an Admin / Compliance officer, when state or federal rules change, I want a versioned compliance framework that drives the KPI engine so that updates are data-only and fully audited. | Module 10 — Compliance Framework |
| P1 | As a Supervisor, when I act on a worker's case, I want inline drill-down + reassign + escalate (and Grove transfer where contracted). | Module 11 — Supervisor Case Actions |
| P2 | As a Deputy Director (where state management is enabled), I want statewide read-only oversight across counties. | Module 12 — Deputy Director / Statewide Oversight |

---

## 9. ⚙️ Non-Functional Requirements

★ **V-MODEL MANDATORY**

**Performance:** Workspace initial load ≤ 2 s for typical users. Work-item list refresh / filter ≤ 1 s. KPI status recalculation ≤ 60 s after triggering event. AI feature response ≤ 5 s for common interactions (briefing, note draft) under typical load — show progress indicator if longer. Bulk actions show progress and complete in time-proportional bounds.

**Security:** RBAC + row-level + field-level masking enforced at API and UI layers. PHI/PII minimization for AI prompts (allowlist + denylist). All AI output labeled "AI Draft" until user-saved. No silent writes. No customer data used for AI training unless explicitly contracted. Comprehensive audit logging (login, sensitive-record access, status change, KPI override, reassignment, escalation, secondary-workflow trigger, all Admin config changes). Tamper-evident, retained per policy.

**Accessibility:** WCAG 2.1 AA across Workspace and Admin surfaces. Keyboard-first workflows. AI-generated content screen-reader friendly.

**Scale:** Horizontal scaling for Workspace API and KPI evaluation. Support organizations with millions of historical work items without degrading operational queries. Indexed/cached KPI status fields for fast filtering ("show me all red items"). Concurrent users sized to customer base.

**Compatibility:** Modern evergreen browsers (Chrome, Edge, Safari, Firefox). Mobile-responsive web (Calendar / Directions are mobile-first). Locales: en-US at launch (per-state localisation deferred). Multi-tenant, multi-state.

**Availability & Reliability:** ≥ 99.9 % production availability (customer-specific SLA). Graceful degradation when KPI engine or AI service is unavailable — Workspace still loads with clear state messaging. Idempotent event processing for KPI recalculation and notifications.

**Observability:** Telemetry events for open / complete / status-change / reassign / secondary-trigger / view-KPI-explanation / use-AI-feature. Admin/IT monitoring dashboards (error rates, latency, notification backlog, integration failures). Configurable system-health alerting (separate from end-user alerts).

---

## 10. 🎨 User Interaction and Design

### UI Regions / Panels (master)

- **Persistent left navigation:** Dashboard, Cases, Work Items (end-user); Calendar, Reporting, Search bar (per module). Admin Suite is a separate top-level surface.
- **Persistent top search bar:** AI-powered universal search (Module 8) on every non-Admin screen.
- **Dashboard region:** KPI summary tiles → AI Briefing Card → work-item list (sort default: Breached → Red → Yellow → Green).
- **Drill-down modals:** Inline overlay pattern — content stays in context, no navigation away.
- **Sub-screen pattern:** Work-item completion happens in an embedded sub-screen, never a separate page.
- **Persona switcher:** Surfaced where a user has multiple personas (e.g., Supervisor + Worker).

### UX Principles

- **Sequential by design** — workflows enforce order; users do not need to remember sequence.
- **In-the-flow** — work happens in-context; no navigating away from the cockpit.
- **Trustworthy** — KPI status explainable, auditable, consistent across surfaces.
- **Configurable without code** — Admins ship change without engineering.
- **Low cognitive load** — clarity over density; "what do I do next" beats reporting depth on the cockpit.
- **Evidence-first AI** — output grounded in visible record data and approved sources; no mystery scores.
- **Human-in-the-loop** — AI may draft and recommend; users confirm before any state change.
- **Accessibility** — WCAG 2.1 AA, inclusive by default.

---

## 11. 🔄 Key Flows

### Flow 1: Sequential workflow execution (cross-cutting)

**Trigger:** Case opens (Program + Case Type known).
**Preconditions:** Primary Workflow exists for the Program / Case Type; KPI policies bound; assigned worker has access.
**Steps:**
1. System instantiates work items per Primary Workflow definition; first item enters **Active**, KPI clock starts.
2. Worker completes the active item via embedded sub-screen.
3. On completion, next item activates and its KPI clock starts.
4. (Optional) Qualifying event triggers a Secondary Workflow → only one active at a time → Admin's pause-default applied to primary KPI clocks (Supervisor may override).
5. Secondary completes → primary resumes at the point it left off.
**Exit state:** All work items in completed / closed status; case remains visible in history.
**Handoff:** Audit log entries written; reporting + AI features consume completion telemetry.

### Flow 2: KPI threshold crossing → escalation (cross-cutting)

**Trigger:** KPI engine evaluates a work item and computes a threshold change.
**Preconditions:** Notification rules + escalation ladder configured for the policy.
**Steps:**
1. Status moves Green → Yellow → Worker notified per channel preferences (de-duped within window).
2. Status crosses to Red → Supervisor added to escalation chain.
3. Status crosses to Breach → Director added; breach event recorded in audit log.
4. Any actor may override with reason → override audited, KPI flag set.
**Exit state:** All required parties notified or override recorded.
**Handoff:** Reporting consumes breach + override events; AI Anomaly Flagging may surface patterns.

Module-specific flows live in each child PRD.

---

## 12. 🚫 Out of Scope and Deferred

### Out of Scope (initial release)

- Full case-management replacement — base Traverse provides the primitives.
- Autonomous AI decisioning (eligibility / clinical determination).
- Offline-first mobile capability.
- Full content authoring / LMS.
- Integration with every external system — only defined and supported APIs are in scope.
- QA / Compliance persona full feature set — placeholder only.

### Deferred (future iterations)

- Per-state localisation beyond Ohio (ODJFS) reference dataset.
- Additional AI features beyond the ten in Module 6.
- Deputy Director / state management — gated behind `stateManagement.enabled`; not all tenants will use it (Module 12).
- Grove Managed Services transfer — gated behind per-tenant managed-services flag (Module 11).

---

## 13. 🧬 V-Model Traceability Matrix

★ **V-MODEL MANDATORY** (structure required at Approval; contents progressive).

At master level, traceability is **module-indexed**: each module PRD owns its own AC ↔ Story ↔ Gherkin ↔ Test ↔ Verdict matrix. The master holds the inter-module rollup once child PRDs are approved.

| Module ID | Module PRD | Master AC reference | Status |
| --- | --- | --- | --- |
| MOD-01 | [[PRD - Traverse Workspace UI Foundation]] | §8 P0 (cockpit landing) | DRAFT |
| MOD-02 | [[PRD - Traverse Workflow Execution Engine]] | §8 P0 (sequential workflows) | DRAFT |
| MOD-03 | [[PRD - Traverse KPI SLA Policy Engine]] | §8 P0 (explainable KPI) | DRAFT |
| MOD-04 | [[PRD - Traverse Notifications and Escalations]] | §8 P0 (escalation ladder) | DRAFT |
| MOD-05 | [[PRD - Traverse Reporting and Analytics]] | §8 P1 (persona reporting) | DRAFT |
| MOD-06 | [[PRD - Traverse AI Copilot Suite]] | §8 P1 (AI assistance) | DRAFT |
| MOD-07 | [[PRD - Traverse Admin Configuration Suite]] | §8 P0 (codeless admin) | DRAFT |
| MOD-08 | [[PRD - Traverse Universal Search]] | §8 P1 (search + NL) | DRAFT |
| MOD-09 | [[PRD - Traverse Calendar and Field Directions]] | §8 P1 (calendar + directions) | DRAFT |
| MOD-10 | [[PRD - Traverse Compliance Framework]] | §8 P0 (compliance framework) | DRAFT |
| MOD-11 | [[PRD - Traverse Supervisor Case Actions]] | §8 P1 (supervisor actions) | DRAFT |
| MOD-12 | [[PRD - Traverse Deputy Director Statewide Oversight]] | §8 P2 (statewide) | DRAFT |

---

## 14. 🚀 Launch Plan

### Suggested phased release (sequenced by dependency)

| Phase | Modules | Rationale |
| --- | --- | --- |
| 1 — Foundations | MOD-02 Workflow · MOD-03 KPI Engine · MOD-07 Admin (core) | Without these, the cockpit has nothing to surface. |
| 2 — Cockpit | MOD-01 Workspace UI · MOD-04 Notifications | Lights up the operational surface. |
| 3 — Insight | MOD-05 Reporting · MOD-08 Search | Adds analytics + retrieval. |
| 4 — AI | MOD-06 AI Copilot | Layered on top of the deterministic surface. |
| 5 — Field & Compliance | MOD-09 Calendar · MOD-10 Compliance Framework | Field-worker UX + multi-state readiness. |
| 6 — Optional / gated | MOD-11 Supervisor Actions (Grove) · MOD-12 Deputy Director | Per-tenant gated capabilities. |

### Operation Checklist (master)

- [ ] Feature-flag matrix defined per module, owned by Admin (Module 7).
- [ ] Telemetry plan signed off (success metrics in §5 verifiable).
- [ ] AI governance + redaction policy configured (Module 6 + §9 Security).
- [ ] Audit retention + export contracts validated per state (Module 10).
- [ ] Rollout training materials (per persona) scheduled with Customer Success.
- [ ] Runbook + on-call rota for KPI engine + AI service published.

---

## 15. 📣 Cross-Functional Impact Checklist

| Team | Prompt | Y/N | Action (if Yes) |
| --- | --- | --- | --- |
| Analytics | Does this need instrumentation to measure §5 metrics? | Y | Work with [Analytics Lead] on telemetry events listed in §9 Observability. |
| Configuration | Does this require a config specialist to onboard tenants? | Y | Module 7 + Module 10 tenant binding require config services. |
| Sales | Sales enablement materials required? | Y | Persona narratives + Grove Managed Services positioning (MOD-11). |
| Marketing | Impact on shared KPIs? | Y | Launch messaging per phase. |
| Customer Success | Support content / training updates required? | Y | Per-persona training, Admin Configurator handbook. |
| Product Marketing | GTM plan required? | Y | Phased GTM mirrors §14 launch plan. |
| Partners | External partner impact? | Y | State agencies (ODJFS reference) + AI service provider. |
| Permissions | New permission set or role required? | Y | Persona model (P1–P5 + Admin + Deputy) requires RBAC updates. |
| Reporting | New reports or impacts to existing reports? | Y | Module 5 introduces persona-scoped reporting. |

---

## 16. ⚠️ Risks and Mitigations

| Risk | Likelihood / Impact | Mitigation |
| --- | --- | --- |
| AI hallucination or unsafe recommendation breaks user trust | M / H | Evidence-grounded outputs only; "AI Draft" labels; human-in-the-loop gating; report-unsafe action; per-tenant AI governance (MOD-06, MOD-07). |
| KPI engine drift between Workspace, reporting, and AI surfaces | M / H | Single KPI engine + explainability payload reused everywhere; consistency tests in MOD-03. |
| Admin misconfiguration breaks production workflows | M / H | Versioned configurator with simulate-before-publish, audit trail, and rollback (MOD-07). |
| State regulatory changes without code change capacity | M / H | Compliance Framework (MOD-10) — rules as data, federal/state layering, versioning + audit. |
| Notification fatigue defeats the purpose of escalation | M / M | Quiet hours, dedupe, per-persona caps, AI anomaly summary instead of per-event spam (MOD-04). |
| Performance regressions as work-item history grows | L / H | Indexing + caching strategy for KPI status fields (§9 Scale); load-test gate per module. |
| Multi-tenant data leakage via AI context | L / H | Permission-aware AI context builder mirroring API rules (MOD-06 + §9 Security). |

---

## 17. ❓ Open Questions

★ **V-MODEL MANDATORY** — must be empty before Approved.

| Question | Impact / Blocks | Date raised | Owner / Needed by |
| --- | --- | --- | --- |
| Confirm base Traverse provides Client / Case / Work Item primitives at the granularity assumed here. | Blocks every module — workflow and KPI engines depend on these entities. | 2026-04-25 | Dev Lead / before MOD-02 design |
| Confirm IdP / SSO target and persona-mapping scheme. | Blocks MOD-01 + MOD-07 RBAC design. | 2026-04-25 | Security / before MOD-07 design |
| Confirm AI service provider, redaction posture, and no-training contractual position. | Blocks MOD-06 architecture. | 2026-04-25 | Legal + Security / before MOD-06 design |
| Confirm reference dataset for Ohio ODJFS Programs / Case Types / KPI policies. | Seeds MOD-07 default config + MOD-10 federal/state layering. | 2026-04-25 | Product / before MOD-07 design |
| Confirm tenant model: county-only vs state+county for Deputy Director gating. | Drives MOD-12 feasibility. | 2026-04-25 | Product / before MOD-12 design |
| Confirm Grove Managed Services contractual model and per-tenant flag schema. | Drives MOD-11 feasibility. | 2026-04-25 | Product / before MOD-11 design |
| Confirm telemetry / observability platform target. | Required for §5 success metrics + MOD-05 reporting. | 2026-04-25 | SRE / before MOD-05 design |
| Confirm SLA targets per customer (≥ 99.9 % is the master default). | Drives reliability budget. | 2026-04-25 | Product / before MOD-04 + MOD-03 design |

---

## 18. 💬 FAQs

**Q:** Why split into 12 modules instead of one PRD?
**A:** The source document is 5,954 lines / 29 sections — too large for a single estimable + reviewable artefact. Each module is independently estimable, independently shippable in most cases, and individually traceable through the V-Model.

**Q:** Where do cross-cutting NFRs live?
**A:** Master PRD §9 sets the baseline. Each module PRD inherits and *only* states module-specific deltas.

**Q:** What is the relationship between Primary and Secondary workflows?
**A:** Each Case Type has exactly one Primary Workflow. Secondary Workflows are event-triggered, run sequentially one at a time, and may pause primary KPI clocks per Admin default (Supervisor may override). Detail in MOD-02.

**Q:** Who owns AI governance?
**A:** Admin (P4) per Module 7. AI features can be enabled / disabled per persona + per program; redaction allowlist/denylist + log retention controlled centrally.

**Q:** Is the Deputy Director persona always available?
**A:** No. It is gated by the per-tenant `stateManagement.enabled` flag. Where disabled, the persona switcher hides it entirely. See MOD-12.

**Q:** Is Grove Managed Services transfer always available?
**A:** No. It is gated by a per-tenant managed-services flag. Where disabled, the Reassign panel does not show the Northwoods Transfer option. See MOD-11.

---

## 19. 🧭 Decision Log

| # | Decision | Rationale / Alternatives | Date | Decided by |
| --- | --- | --- | --- | --- |
| 1 | Split source PRD into 1 master + 12 module PRDs. | Source is too large to estimate or review as one artefact. Alternative (single PRD) rejected for the same reason. | 2026-04-25 | Product (Kendrew Peacey, Camis) |
| 2 | Bundle Grove Managed Services Transfer (§28) into MOD-11 Supervisor Case Actions. | Both share the Reassign panel UI surface; splitting would force coordinated UI changes across two PRDs. Alternative (separate PRD) rejected. | 2026-04-25 | Product |
| 3 | Bundle KPI tile drill-down (§20), Active Cases drill-down (§23), and column-header sorting (§24) into MOD-01 Workspace UI Foundation. | These are UI refinements of the §8 surface, not new subsystems. Alternative (separate per-feature PRDs) rejected. | 2026-04-25 | Product |
| 4 | Use V-Model PRD template (`VModel_PRD_Template.docx` v1.0) markdown form. | Vault convention; matches existing PRDs (Workflow Engine / Scheduling Service / Agentic Harness). | 2026-04-25 | Product |
| 5 | Default to skeleton-with-placeholders pre-filled from source PRD; genuine unknowns logged as Open Questions. | Source PRD is rich enough to pre-fill ~80 % of each module. | 2026-04-25 | Product |

---

## 20. 📚 Module Index

Each module PRD inherits master vision, personas, and NFRs. Module PRDs are estimable independently and shippable per the §14 phased plan.

| # | Module PRD | Source §s | Scope summary |
| --- | --- | --- | --- |
| 1 | [[PRD - Traverse Workspace UI Foundation]] | 8, 20, 23, 24 | Navigation; role-aware Dashboards; Cases & Work Items views; KPI tile drill-downs; Active Cases drill-down; column-header sorting; accessibility. |
| 2 | [[PRD - Traverse Workflow Execution Engine]] | 9 | Work Item model; primary workflow sequencing; secondary (triggered) workflows; pause / resume semantics. |
| 3 | [[PRD - Traverse KPI SLA Policy Engine]] | 10 | KPI policy model; business-time calendars; threshold computation; explainability payload. |
| 4 | [[PRD - Traverse Notifications and Escalations]] | 11 | Triggers; escalation ladder; channels & preferences; quiet hours; dedupe. |
| 5 | [[PRD - Traverse Reporting and Analytics]] | 12, 26 | Persona reports (My Performance / Team / Analytics Hub); export & scheduled reports; metric dictionary. |
| 6 | [[PRD - Traverse AI Copilot Suite]] | 13 | All ten AI features (Briefing, Completion, Trigger Detection, Breach Prediction, Smart Summary, Workload Balance, Program Health, Policy Q&A, Outreach, Anomaly). |
| 7 | [[PRD - Traverse Admin Configuration Suite]] | 14 | Program / Case Type; Work Item Library; Workflow Configurator; User Mgmt; Notification Rules; Feature Flags; AI Governance. |
| 8 | [[PRD - Traverse Universal Search]] | 19 | NL query; autocomplete; federated search; conversational AI response layer. |
| 9 | [[PRD - Traverse Calendar and Field Directions]] | 21, 22 | Calendar screen; work-item events; manual appointments; Today's Schedule strip; event popup; directions handoff. |
| 10 | [[PRD - Traverse Compliance Framework]] | 25 | Tenant-state binding; federal / state layering; compliance library; update notification & audit. |
| 11 | [[PRD - Traverse Supervisor Case Actions]] | 27, 28 | Worker drill-down; reassign; escalate; Grove Managed Services transfer (gated). |
| 12 | [[PRD - Traverse Deputy Director Statewide Oversight]] | 29 | Deputy persona (gated); statewide dashboard; county drill-down; statewide analytics. |

---

## 21. 🗃️ Conceptual Data Model (master)

| Entity | Description |
| --- | --- |
| **User** | Persona, team, program scope, supervisor relationship, active status. |
| **Persona** | Role concept driving Workspace profile, permissions, feature access. |
| **ProgramType** | Top-level service domain (Child Welfare, Child Support, Adult Aging, Behavioral Health, Economic Assistance). |
| **CaseType** | Classification within a Program Type; determines Primary Workflow; may have Secondary Workflows. |
| **Client** | Person served; identifiers + risk flags subject to masking/consent. |
| **Case** | Service episode linked to a Client; carries Program Type, Case Type, assigned worker, open date, workflow state. |
| **WorkItemDefinition** | Admin-managed library entry; sub-screen config, required fields, program applicability, instructions. |
| **PrimaryWorkflow** | Admin-defined ordered work-item sequence per Program × Case Type; versioned + publishable. |
| **SecondaryWorkflow** | Admin-defined event-triggered ordered sequence; default pause behavior. |
| **TriggeringEvent** | Admin-managed event type that activates a secondary workflow (e.g., "Failure to Pay"). |
| **WorkItem (Instance)** | Linked to definition + case; status, owner, sequence, activated/completed timestamps, KPI policy ref, KPI status, explainability payload. |
| **KpiPolicy** | Configurable; scope, timing rules, green/yellow/red/breach thresholds, time basis, version. |
| **KpiExplainabilityPayload** | start_at, stop_at, total_elapsed, paused_duration, remaining_duration, due_at, yellow_at, red_at, breached_at, policy_id/version, breach flag, override indicator. |
| **BusinessCalendar** | Working hours, weekends, holidays for business-time KPIs; scoped per state/program. |
| **Queue** | Shared pool of work items / cases (e.g., intake) supporting claim/pull. |
| **Notification** | Event-driven record; type, recipients, channel, timestamp, delivery status. |
| **AuditLogEntry** | Immutable; actor, timestamp, entity, action, before/after, source. |
| **AIInteractionLog** | User, feature, referenced records, sources cited, output, applied/saved flag. |
| **ComplianceRule** | (MOD-10) Rule-as-data with federal/state layer, version, citations, effective dates. |
| **TenantConfig** | Per-tenant feature-flag set (managedServices.enabled, stateManagement.enabled, AI governance, retention windows). |

Detailed schema design lives in module-level technical specs.

---

## 22. 📝 Change Log

| Date | Changed by | Section(s) | Reason |
| --- | --- | --- | --- |
| 2026-04-25 | Kendrew Peacey | All | Initial master PRD creation — derived from `Northwoods_Traverse_PRD_v2.0.docx` (Northwoods, S. Ryczek, April 2026). 12 module PRDs spawned and indexed in §20. |

---

## 23. 🧾 Informed By (Knowledge Lineage)

★ **V-MODEL MANDATORY**

**Option A — Informed by existing artefacts:**

| Document | Path / link | Integrity (SHA-256 first 8 hex) |
| --- | --- | --- |
| Northwoods Traverse PRD v2.0 (source) | `00 - Inbox/Northwoods_Traverse_PRD_v2.0.docx` | sha256:[computed at approval] |
| Northwoods-Traverse project note | [[Northwoods-Traverse]] | sha256:[computed at approval] |

This master is a **derived artefact** — sourced from the Northwoods PRD v2.0 and decomposed into the module PRDs listed in §20.

---

## 24. ✅ Approval and Checksum

★ **V-MODEL MANDATORY**

### Approval checklist

- [ ] Problem statement is clear and specific
- [ ] Module-level outcomes (§8) traceable to persona needs and source PRD requirements
- [ ] Non-Functional Requirements have quantified targets (or explicit N/A + rationale)
- [ ] Out of Scope and Deferred items are explicit with rationale
- [ ] §17 Open Questions resolved
- [ ] All §6 assumptions validated or logged as open questions
- [ ] Dev Lead technical feasibility sign-off IN PLACE (per master + per module)
- [ ] Estimation Checkpoint 1 IN PLACE per module
- [ ] V-Model Traceability Matrix structure populated (master + per module)
- [ ] Decision Log, Change Log, Informed By sections current
- [ ] All 12 module PRDs reach Approved status

### Approval record

| Field | Value |
| --- | --- |
| Approved by | [PM name and role] |
| Approval date | [YYYY-MM-DD] |
| Document SHA-256 | sha256-[computed at approval] |
| Pipeline tracker entry | [Path to feature pipeline tracker that references this master + child PRDs] |

---

*Template version 1.0 · Camis V-Model–Aligned PRD · Compatible with `camis-v-model:draft-prd` skill v2.1.0*
