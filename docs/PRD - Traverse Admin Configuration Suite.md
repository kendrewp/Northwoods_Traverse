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
# PRD — Traverse Admin Configuration Suite (MOD-07)

> **V-Model–Aligned PRD.** Inherits from [[PRD - Northwoods Traverse Workspace]]. Source: §14.

---

## 1. Product Overview

| Field | Value |
| --- | --- |
| 📅 Target date | [QX YYYY — Phase 1 of master launch plan] |
| 🟡 Document status | Draft |
| 🧭 Team | PM / Product / Dev Lead / Dev / QA: [Names] |
| 🗃️ Work tracker (Jira epic) | [CAM-XXXXX] |
| 📎 Parent PRD | [[PRD - Northwoods Traverse Workspace]] |
| 🔗 Resources | `00 - Inbox/Northwoods_Traverse_PRD_v2.0.docx` (§14) |

---

## 2. 🎯 Objective and Problem Alignment

### Overview

The codeless configuration plane for the entire Workspace. Admins (P4) define and manage Programs, Case Types, Work Item Library (governance only — work items themselves are developer-built), primary + secondary Workflows, KPI policies (with simulator), business calendars, users + SSO, notification rules + escalation, dashboard / reporting config, feature flags, AI governance, compliance binding, and internal threshold overrides — all under audit and versioning.

### Problem Statement

Admins today cannot adapt KPI policies, workflows, or work item libraries without engineering changes. Notification rules are noisy or absent. Compliance updates require code releases. Without a configurable, auditable, simulate-before-publish surface, every operational change becomes a delivery cycle and the system drifts from regulatory reality.

### Importance

- "Configurable without code" is a master design principle.
- Versioned + auditable = safe Admin self-service.
- Simulate-before-publish lets Admins verify policy / workflow changes against live cases before impact.
- Compliance binding (state package) drives KPI thresholds + citations system-wide as data, not code.
- AI governance is centralised here so trust posture is consistent across the 10 features.

### High-Level Approach

Nine functional areas, all under shared admin governance (RBAC, audit, versioning, rollback):

1. **Admin Access & Governance** — admin roles, audit log, configuration versioning + rollback, compliance binding, compliance library UX, internal threshold overrides, work calendar.
2. **Program & Case Type Management** — manage Program Types and Case Types per state.
3. **Work Item Library** — governance of developer-built work items (program applicability, generic/specific, status, sub-screen preview).
4. **Workflow Configurator** — drag-and-drop primary + secondary workflow builder with KPI assignment, triggering events, KPI Policy Simulator.
5. **User Management** — accounts, persona, program, supervisor, SSO, deactivation reassignment.
6. **Notification & Escalation Rule Configuration** — rule editor, quiet hours, digests.
7. **Reporting & Dashboard Configuration** — persona dashboard config + scheduled reports.
8. **Feature Flags & Rollout Controls** — pilot vs prod, per-user / team / program / environment flags.
9. **AI Governance** — per-feature × persona × program toggle, knowledge sources, audit retention, feedback queue, rate limits.

---

## 3. 👥 Stakeholders and Personas

- **P4 — Admin** — primary user; multiple Admin role flavors (System Admin, Program Admin, KPI Admin, Integration Admin) with least-privilege scope.
- **P5 — QA / Compliance (placeholder)** — read-only audit access.
- **MOD-02, MOD-03, MOD-04, MOD-05, MOD-06, MOD-10** — all consume configuration published here.

---

## 4. 🎬 Target Use Cases

- **Bind state compliance package:** Admin opens Client Configuration → reviews active state binding → changes to a new state, sees impact summary (case count, work items affected), confirms acknowledgment → KPI thresholds + citations update system-wide.
- **Set internal threshold override:** Admin tightens a yellow warning from legal 5 days to 7 days remaining on a 21-day rule; legal threshold remains visible; "OVR" badge shows on rule row.
- **Build a primary workflow:** Admin selects Program × Case Type → drags work items from library → assigns KPI per item → runs Simulator with hypothetical case start → publishes new version (prior version grandfathered).
- **Add a triggered workflow:** Admin opens primary workflow → adds "Failure to Pay" triggered workflow with Pause Primary KPIs default → builds work items inline → publishes.
- **Manage users:** Admin creates a Worker account, assigns persona + Program + Supervisor → SSO maps identity at login.
- **Deactivate user:** Admin deactivates departing Worker → prompted to reassign caseload → bulk reassign to a queue → confirm.
- **Edit notification rules:** Admin tweaks escalation ladder for Child Welfare, sets quiet hours for Adult Aging.
- **AI governance:** Admin enables Policy Q&A for Workers in pilot county; restricts all AI features for a sensitive program.
- **Feature flag pilot:** Admin enables Workspace cockpit for one team in production while the rest stay on legacy.

---

## 5. 📊 Success Conditions and Metrics

### Goals

1. Every operational behavior the source PRD calls out (workflows, KPI, notifications, AI) is Admin-configurable, audited, versioned.
2. Simulate-before-publish prevents bad-publish incidents.
3. State compliance changes are data-only, no code release.
4. AI is governed centrally with per-feature × persona × program control.
5. User deactivation always preserves audit history and reassigns caseload.

### Non-Goals

- New Work Item *creation* (developer-built; Admin only governs program applicability + status).
- KPI computation logic (MOD-03).
- Workflow runtime execution (MOD-02).
- Notification delivery (MOD-04).
- AI feature implementation (MOD-06; this is governance only).
- Reporting visualisations (MOD-05; this is configuration only).
- Compliance rule authoring (MOD-10 owns rule library; this Suite consumes via the Compliance Library UX).

### Success Metrics

| Goal | Metric |
| --- | --- |
| Codeless Admin coverage | % of operational changes shipped without dev cycle — target ≥ 95 % within 6 months |
| Bad-publish prevention | 0 in-production cases where simulator would have caught the issue |
| Compliance update lead time | Days from rule change to system update — target ≤ 5 days |
| Adoption | Admin tasks completed per tenant per month — target [TBD] |

### Guardrails

- All changes audited (actor, before/after, timestamp).
- Versioned configuration with rollback for KPI policies, workflows, work item library.
- Grandfathering — workflow / work-item changes apply only to cases opened after publish.
- Simulator is mandatory for KPI / workflow changes; UI does not allow publish without it being run.
- Compliance binding requires acknowledgment.
- AI features disabled by default; Admin opts in.

---

## 6. 🤔 Assumptions

- IdP supports SSO (SAML/OAuth) and optionally directory sync. — ❓ unvalidated
- Compliance Library data model is owned by MOD-10; this Suite consumes via shared API. — ❓ unvalidated
- Work calendars are tenant-global v1 (program-level overrides deferred). — ❓ unvalidated
- The "Northwoods implementation team" path for new work items is a known process. — ❓ unvalidated

---

## 7. 🔗 Dependencies and V-Model Gates

★

| Dependency | Type | Status | Notes |
| --- | --- | --- | --- |
| MOD-02 Workflow Engine | Internal | DRAFT | Consumes published workflows. |
| MOD-03 KPI Engine | Internal | DRAFT | Consumes published policies + calendars. |
| MOD-04 Notifications | Internal | DRAFT | Consumes notification rules + quiet hours. |
| MOD-06 AI Copilot | Internal | DRAFT | Consumes governance + knowledge sources. |
| MOD-10 Compliance Framework | Internal | DRAFT | Provides Compliance Library. |
| Audit log infrastructure | Shared | PENDING | Records every change. |
| Identity Provider (SSO) | External | PENDING | Hard blocker for ADM-041. |
| Dev Lead feasibility | V-Model gate | PENDING | ★ |
| Estimation | V-Model gate | PENDING | ★ |

---

## 8. 📋 Requirements

★

| Priority | Job Story | Acceptance Criteria (G/W/T) | Jira | Source |
| --- | --- | --- | --- | --- |
| P0 | As an Admin, when I access the Suite, I want least-privilege admin roles so that scope is contained and auditable. | AC-1.1: *Given* an Admin role (System / Program / KPI / Integration), *Then* I see only configuration areas I'm permitted to manage. AC-1.2: *Given* any Admin action, *Then* it is audit-logged with who/what/when. AC-1.3: *Given* applicable areas, *Then* roles can be scoped to specific Program Types. | [CAM-XXXXX] | ADM-001 |
| P0 | As an Admin, when I change configuration, I want a complete change audit log so that compliance can review every action. | AC-2.1: *Given* a change, *Then* the log stores before/after values, actor, timestamp, optional reason. AC-2.2: *Given* logs, *Then* they are searchable + exportable for compliance roles, tamper-evident, retained per jurisdiction. | [CAM-XXXXX] | ADM-002 |
| P1 | As an Admin, when a published change is wrong, I want to roll back to a prior version of KPI policies / workflows / work item library so that I can correct fast. | AC-3.1: *Given* rollback, *Then* it is permission-restricted and logged. AC-3.2: *Given* a target prior version, *Then* the system shows a diff before rollback is confirmed. | [CAM-XXXXX] | ADM-003 |
| P0 | As an Admin, when I bind a state compliance package, I want a transparent flow with impact summary + acknowledgment so that the system-wide effect is explicit. | AC-4.1: *Given* the Client Configuration screen, *Then* a Compliance Package section shows currently bound state name, governing agency, ruleset version, effective date, total rule count. AC-4.2: *Given* "Change Bound State", *Then* a modal lists all available state packages; the current state is marked and not re-selectable. AC-4.3: *Given* a new selection, *Then* an impact summary lists active cases recalculated, open work items with updated warning timers, and the new agency replacing the current. AC-4.4: *Given* a mandatory acknowledgment checkbox, *Then* the confirm button enables only when checked; the acknowledgment text states the change is immediate, replaces all KPI thresholds + warning timers + citations, is logged, and not auto-reversible. AC-4.5: *Given* confirmation, *Then* the binding updates immediately; an audit entry per ADM-002 is written; the Compliance Library updates its "Active Tenant" indicator. AC-4.6: *Given* the Compliance Library, *Then* it shows an "Active Tenant" badge and a "Manage in Client Config" shortcut. | [CAM-XXXXX] | ADM-004 |
| P0 | As an Admin, when I browse compliance rules, I want a sortable / searchable / read-only Library mirroring the Work Item Library UX so that I can audit rules without modifying them. | AC-5.1: *Given* the rules table, *Then* column-header sorting works on Work Item, Program, Timeframe, Yellow Warning, Red Alert, Federal, Version, with ↑/↓ active sort indicator. AC-5.2: *Given* a filter-as-you-type bar, *Then* it filters on work item name, program, citation simultaneously with a live count; state-tab and program dropdown filtering remain available. AC-5.3: *Given* rule content, *Then* it is read-only — no add/edit/delete of timeframe, citation, or federal flag. AC-5.4: *Given* each row, *Then* it provides Details (cross-state comparison) and Override (per ADM-006); rows with active overrides show "OVR" indicator. | [CAM-XXXXX] | ADM-005 |
| P1 | As an Admin, when I want a tighter internal alert than the legal minimum, I want internal threshold overrides without modifying legal values so that operations get a buffer while compliance is preserved. | AC-6.1: *Given* the Override modal, *Then* legal thresholds (timeframe, yellow, red) are read-only; internal yellow (days remaining) and red (hours remaining) are editable. AC-6.2: *Given* validation, *Then* internal yellow ≥ legal yellow days, internal red ≥ legal red hours; errors inline; save disabled while invalid. AC-6.3: *Given* blank fields, *Then* the legal default applies; both blanks = clear override. AC-6.4: *Given* an active override, *Then* an amber "OVR" indicator appears on Yellow / Red values; Override button styled amber. Legal citation + values remain visible. | [CAM-XXXXX] | ADM-006 |
| P0 | As an Admin, when I configure tenant working time, I want a Work Calendar (working days + observed holidays) so that all KPI due-date calculations honor business time. | AC-7.1: *Given* Client Configuration, *Then* a Work Calendar card shows the configured working days + holidays. AC-7.2: *Given* day toggles, *Then* I can set Mon–Sun on/off (default Mon–Fri working). AC-7.3: *Given* holidays, *Then* I can add (name + date) and remove; removal applies to cases opened after the change is saved. AC-7.4: *Given* this calendar, *Then* it applies globally across programs (program-level overrides deferred; UI notes this). AC-7.5: *Given* changes, *Then* they are audit-logged with summary. | [CAM-XXXXX] | ADM-007 |
| P0 | As an Admin, when I manage Program Types, I want versioned + auditable management with deactivation safety so that I cannot break running operations. | AC-8.1: *Given* Program Types, *Then* they are versioned + auditable. AC-8.2: *Given* permitted, *Then* I can add custom Program Types. AC-8.3: *Given* deactivation, *Then* it is blocked if active cases or workflows reference the Program Type. | [CAM-XXXXX] | ADM-010 |
| P0 | As an Admin, when I manage Case Types within a Program, I want versioned + state-restricted management with safe deactivation so that running cases keep their workflow. | AC-9.1: *Given* Case Types, *Then* they are versioned + auditable; changes have effective dates. AC-9.2: *Given* state restriction, *Then* a Case Type can be limited per state / jurisdiction. AC-9.3: *Given* base Traverse, *Then* the Admin UI maps + syncs rather than duplicating data. AC-9.4: *Given* deactivation, *Then* active cases continue with the existing workflow; new cases cannot use the deactivated type. | [CAM-XXXXX] | ADM-011 |
| P0 | As an Admin, when I manage the Work Item Library, I want sort / search / preview / governance (program applicability, generic/specific, status) so that I can govern without authoring. | AC-10.1: *Given* the table, *Then* column-header sorting works on Work Item Name, Sub-Screen Type, Program Applicability, Generic, Status with ↑/↓ indicator. AC-10.2: *Given* a filter-as-you-type bar, *Then* visible rows filter on name, sub-screen type, program applicability with live row count; no "New Work Item" action; a developer note explains additions go through Northwoods implementation. AC-10.3: *Given* Edit on a row, *Then* a modal exposes Program Applicability checkboxes (5 programs) and Status (Active / Inactive) with Save Changes; sub-screen content + work-item type are read-only. AC-10.4: *Given* Deactivate, *Then* it is forward-looking only — active cases grandfathered; workflow templates that reference the item are flagged for review; the item is removed from the workflow builder pick list immediately. AC-10.5: *Given* deactivation confirmation, *Then* the dialog shows count of active cases (grandfathered), count of templates flagged for review, and "Superseded by" replacement (or "None"). AC-10.6: *Given* Preview, *Then* a modal renders a read-only sub-screen template appropriate to the type (Notes / Form / Document Upload / Checklist) with a Close Preview action. | [CAM-XXXXX] | ADM-020 |
| P0 | As an Admin, when I configure a sub-screen for a work item, I want field-level configuration with versioned audit so that completion enforcement is explicit. | AC-11.1: *Given* sub-screen types, *Then* they are Notes Entry, Structured Data Collection, Document Upload, Checklist, Composite. AC-11.2: *Given* required fields, *Then* they are enforced at completion. AC-11.3: *Given* fields, *Then* I can configure label, type (text/date/dropdown/checkbox/file), required/optional. AC-11.4: *Given* changes, *Then* they are versioned and audit-logged. | [CAM-XXXXX] | ADM-021 |
| P0 | As an Admin, when I build a primary workflow, I want a drag-and-drop builder with KPI assignment and grandfathering so that I can publish safely. | AC-12.1: *Given* Program + Case Type dropdowns, *Then* the configurator loads any existing workflow for that combo with a status banner (green if exists, amber if not). AC-12.2: *Given* no workflow, *Then* an empty state with "Start Building This Workflow" initialises an empty workflow. AC-12.3: *Given* "+ Add Work Item", *Then* the picker shows only active library items applicable to the selected program; existing items are excluded; search filters by name. AC-12.4: *Given* a Compliance Library match, *Then* a "Compliance ref" badge appears in the picker; selecting pre-populates KPI fields with legal values. AC-12.5: *Given* Add to Workflow, *Then* it requires KPI Target (days), Yellow Alert threshold (day), Red Alert threshold (hours prior); button disabled until valid. AC-12.6: *Given* a row, *Then* Edit KPI opens a pre-filled modal with compliance reference if applicable. AC-12.7: *Given* Remove (×), *Then* a confirmation dialog states grandfathering policy. AC-12.8: *Given* reorder, *Then* drag handle (production via @dnd-kit/core) and ▲ Move Up / ▼ Move Down buttons both work; boundary buttons disabled. AC-12.9: *Given* Simulator, *Then* I can verify due/threshold calculations before publishing. AC-12.10: *Given* publish, *Then* the workflow is versioned; prior version retained for in-flight cases; only one active version per Program × Case Type. | [CAM-XXXXX] | ADM-030 |
| P0 | As an Admin, when I define triggered (secondary) workflows under a primary, I want an inline builder with the same patterns as primary so that I do not navigate away. | AC-13.1: *Given* the Workflow Configurator, *Then* a "Triggered Workflows" section is shown beneath the primary builder for the selected combo. AC-13.2: *Given* "Add Triggered Workflow", *Then* the modal collects name (required), triggering event from the predefined list (Emergency Safety Concern, Child Removed from Home, Court Order Received, Family Refuses Cooperation, Hospitalization, Alleged Abuse/Neglect Report, Case Escalation Request, Service Plan Non-Compliance), program scope (All / specific), KPI pause behavior (Pause Primary recommended / Continue Primary). AC-13.3: *Given* Save & Build, *Then* the inline builder panel opens immediately on the screen with a trigger context banner. AC-13.4: *Given* the builder, *Then* it uses the same picker / KPI fields / Edit KPI / hybrid reorder / Remove-with-grandfathering as ADM-030. AC-13.5: *Given* publish, *Then* it is versioned; structural changes (add / remove / reorder) and deletion apply to cases opened after the next publication. | [CAM-XXXXX] | ADM-031 |
| P0 | As an Admin, when I manage triggering events, I want an Admin-managed list per program so that secondary workflows have meaningful triggers. | AC-14.1: *Given* events, *Then* the list is configurable per Program Type. AC-14.2: *Given* events, *Then* each has name, description, program scope. AC-14.3: *Given* examples (Failure to Pay / Failed Home Visit / Court Order Received / Hospitalization), *Then* they exist as defaults. AC-14.4: *Given* an event, *Then* it can be activated/deactivated independently of workflows that reference it. | [CAM-XXXXX] | ADM-032 |
| P0 | As an Admin, when I want to verify policy correctness, I want a KPI Policy Simulator so that I can preview alerts and deadlines before publish. | AC-15.1: *Given* a hypothetical case start date + calendar mode (Business Days using Work Calendar / Calendar Days), *Then* "Run Simulation" calculates per work item: item start, due, yellow alert, red alert (due − redHours). AC-15.2: *Given* the summary bar, *Then* it shows case open, projected completion, total duration, warning count; inline warnings flag weekends, yellow ≥ due, red ≤ yellow. AC-15.3: *Given* the simulator, *Then* it runs without publishing; results are display-only and do not persist. | [CAM-XXXXX] | ADM-033 |
| P0 | As an Admin, when I manage users, I want persona, program, supervisor, and active/inactive controls so that data scope and escalation routing are explicit. | AC-16.1: *Given* a user, *Then* they have one primary persona; optional secondary personas if multi-role. AC-16.2: *Given* program assignment, *Then* it drives data visibility + escalation routing. AC-16.3: *Given* a supervisor relationship, *Then* it defines the escalation chain. AC-16.4: *Given* deactivation, *Then* it is preferred over deletion to preserve audit history; Admin is prompted to reassign active caseload before confirmation. | [CAM-XXXXX] | ADM-040 |
| P0 | As an Admin, when users authenticate, I want SSO / IdP integration with directory sync where supported so that credentials are not duplicated. | AC-17.1: *Given* SSO, *Then* it is the preferred authentication method. AC-17.2: *Given* provisioning, *Then* manual or directory sync where supported. AC-17.3: *Given* persona + program assignments, *Then* they live in this Suite, not the IdP. | [CAM-XXXXX] | ADM-041 |
| P0 | As an Admin, when I deactivate a user, I want a guided caseload reassignment so that no active work loses an owner. | AC-18.1: *Given* deactivation, *Then* the Admin sees all active cases + work items assigned to the user. AC-18.2: *Given* reassignment, *Then* I can bulk reassign to another user or queue. AC-18.3: *Given* reassignment, *Then* it is audited. | [CAM-XXXXX] | ADM-042 |
| P0 | As an Admin, when I configure notifications, I want a rule editor (event triggers, channels, recipients) per Program Type so that calibration is granular. | AC-19.1: *Given* the editor, *Then* the escalation ladder is configurable per Program × Case Type. AC-19.2: *Given* event triggers, *Then* I can enable/disable per persona + program. AC-19.3: *Given* rules, *Then* dedupe + reminder cadence are configurable. | [CAM-XXXXX] | ADM-050 |
| P1 | As an Admin, when off-hours behavior matters, I want quiet hours + digest config globally and per persona / program with critical-Breach bypass. | AC-20.1: *Given* configuration, *Then* I can define defaults globally and per persona + program. AC-20.2: *Given* Critical Breach, *Then* it can bypass quiet hours if configured. AC-20.3: *Given* users, *Then* they may adjust within Admin-defined bounds. | [CAM-XXXXX] | ADM-051 |
| P0 | As an Admin, when I configure persona dashboards, I want widget visibility + required/optional controls + persona defaults so that surfaces are consistent. | AC-21.1: *Given* widgets, *Then* I can mark them required (cannot be removed) or optional. AC-21.2: *Given* persona defaults, *Then* I can define them. AC-21.3: *Given* a user's dashboard, *Then* I can reset it to persona default. | [CAM-XXXXX] | ADM-060 |
| P1 | As an Admin, when I configure scheduled reports, I want content + recipients + cadence + format with persona-scope respect. | AC-22.1: *Given* recipient lists, *Then* they are Admin-managed. AC-22.2: *Given* a recipient, *Then* report content respects their persona scope. AC-22.3: *Given* a job, *Then* it respects quiet hours + masking. | [CAM-XXXXX] | ADM-061 |
| P1 | As an Admin, when I roll out features, I want feature flags scoped to user / team / program / environment so that pilots are safe. | AC-23.1: *Given* a flag change, *Then* it is auditable. AC-23.2: *Given* scope, *Then* flags can target specific users / teams / programs / environments. AC-23.3: *Given* AI features, *Then* each can be toggled independently per persona. | [CAM-XXXXX] | ADM-070 |
| P0 | As an Admin, when AI is in use, I want enable/disable per feature × persona × program × environment with audit so that AI posture is centrally governed. | AC-24.1: *Given* a change, *Then* it is audited. AC-24.2: *Given* sensitive programs / data categories, *Then* I can restrict AI. AC-24.3: *Given* the user UI, *Then* users can see whether AI is enabled and which features are available. | [CAM-XXXXX] | ADM-080 |
| P0 | As an Admin, when AI consumes knowledge sources, I want versioned + scoped source management so that AI cites only approved content. | AC-25.1: *Given* sources, *Then* they are versioned with effective dates. AC-25.2: *Given* scope, *Then* I can restrict by program / state / persona. AC-25.3: *Given* indexing, *Then* I can trigger re-index and view status. AC-25.4: *Given* deprecation, *Then* AI stops citing the source after re-index. | [CAM-XXXXX] | ADM-081 |
| P0 | As an Admin, when AI runs, I want configurable audit logging + retention so that compliance is satisfied per jurisdiction. | AC-26.1: *Given* logs, *Then* they include user ID, timestamp, feature, referenced records, sources cited, applied/saved flag. AC-26.2: *Given* retention, *Then* it is configurable per program. AC-26.3: *Given* logs, *Then* they are exportable for compliance review. | [CAM-XXXXX] | ADM-082 |
| P1 | As an Admin, when users flag AI outputs, I want a feedback review queue so that I can track and address recurring issues. | AC-27.1: *Given* feedback items, *Then* they include prompt, response, citations, user feedback reason. AC-27.2: *Given* an issue, *Then* I can mark it resolved with corrective action documented. AC-27.3: *Given* recurring issues, *Then* the system supports bulk analysis. | [CAM-XXXXX] | ADM-083 |
| P1 | As an Admin, when I want to manage AI cost / performance, I want rate limits per persona / environment with non-blocking fallback. | AC-28.1: *Given* rate limits, *Then* they are configurable by persona + environment. AC-28.2: *Given* a limit reached, *Then* the UI shows a clear message + fallback; no user workflow is blocked. AC-28.3: *Given* usage, *Then* metrics are available to Admins for capacity planning. | [CAM-XXXXX] | ADM-084 |

---

## 9. ⚙️ NFRs

★ Inherits master §9. Module deltas:

**Performance:** Configurator surfaces ≤ 2 s initial load. Simulator response ≤ 1 s for typical workflows. Library filter-as-you-type ≤ 100 ms.

**Security:** RBAC + audit on every action. Persona / program scope respected in every list / picker.

**Reliability:** Publish is atomic — partial publishes are not allowed. Versioning + rollback covers KPI policies, workflows, work item library.

**Observability:** Telemetry: admin_action, config_published, config_rolled_back, simulator_run, ai_governance_changed, compliance_binding_changed.

---

## 10. 🎨 User Interaction and Design

- **Top-level Admin home** with cards for each functional area (§2 nine areas).
- **Workflow Configurator** is a single screen: Program × Case Type selectors → primary builder → Triggered Workflows section beneath → Simulator panel.
- **Compliance Library** mirrors Work Item Library UX (column sort, filter-as-you-type, badges).
- **Override / Edit modals** show legal values as read-only reference next to editable fields.
- **Confirmation dialogs** for destructive / forward-looking changes show explicit grandfathering messaging.
- **Diff viewer** for rollback.

---

## 11. 🔄 Key Flows

### Flow 1: Publish a primary workflow

1. Admin selects Program × Case Type.
2. Configurator loads existing workflow + status banner.
3. Admin adds / edits / reorders work items; KPI fields validated; Compliance ref pre-populates from library.
4. Admin runs Simulator with hypothetical start; reviews outputs + warnings.
5. Admin clicks Publish → version created; prior version remains for in-flight cases.
6. Audit entry written.
**Exit:** New version active for new cases; old version active for in-flight.

### Flow 2: Bind a new state compliance package

1. Admin opens Client Configuration → Compliance Package section.
2. Clicks "Change Bound State" → modal lists available states with current marked.
3. Selects new state → impact summary computes affected cases + work items.
4. Admin checks acknowledgment → confirm.
5. System updates binding; KPI thresholds + citations update; "Active Tenant" updates in Library.
6. Audit entry.
**Exit:** New compliance posture in effect.

### Flow 3: Deactivate user

1. Admin opens user record → Deactivate.
2. System lists active cases + work items.
3. Admin bulk reassigns to another user / queue.
4. Admin confirms deactivation.
5. Audit entry.
**Exit:** User inactive; caseload reassigned; audit history preserved.

---

## 12. 🚫 Out of Scope and Deferred

### Out of Scope

- New work-item creation (developer-built).
- Compliance rule authoring (MOD-10).
- Notification delivery (MOD-04).
- KPI computation (MOD-03).
- Workflow execution (MOD-02).

### Deferred

- Program-level work calendar overrides.
- Bulk migration of in-flight cases to a newer workflow version.
- Customer-facing AI feedback queue (Admin-only at launch).
- Self-service data dictionary editor for end users.

---

## 13. 🧬 Traceability

| AC ID | Story | Summary | Gherkin | Test class | Verdict |
| --- | --- | --- | --- | --- | --- |
| AC-4.5 | [CAM-XXXXX] | Compliance binding immediate effect | `docs/{FEATURE-ID}/gherkin/adm-004-binding.feature` | --- | --- |
| AC-6.2 | [CAM-XXXXX] | Override validation ≥ legal | `docs/{FEATURE-ID}/gherkin/adm-006-override.feature` | --- | --- |
| AC-10.4 | [CAM-XXXXX] | Work item deactivation forward-looking | `docs/{FEATURE-ID}/gherkin/adm-020-deactivate.feature` | --- | --- |
| AC-12.10 | [CAM-XXXXX] | Workflow grandfathering on publish | `docs/{FEATURE-ID}/gherkin/adm-030-publish.feature` | --- | --- |
| AC-13.5 | [CAM-XXXXX] | Triggered workflow grandfathering | `docs/{FEATURE-ID}/gherkin/adm-031-triggered.feature` | --- | --- |
| AC-15.3 | [CAM-XXXXX] | Simulator non-persistent | `docs/{FEATURE-ID}/gherkin/adm-033-simulator.feature` | --- | --- |
| AC-18.2 | [CAM-XXXXX] | Reassign on deactivate | `docs/{FEATURE-ID}/gherkin/adm-042-reassign.feature` | --- | --- |
| AC-25.4 | [CAM-XXXXX] | Deprecated source no longer cited | `docs/{FEATURE-ID}/gherkin/adm-081-source.feature` | --- | --- |

---

## 14. 🚀 Launch Plan

| Target | Milestone | Description | Exit |
| --- | --- | --- | --- |
| [YYYY-MM-DD] | ✅ UAT | Admin pilot with reference Ohio data | Simulator + publish flows verified |
| [YYYY-MM-DD] | 🛑 Early Access | Pilot tenant | All 9 functional areas operational |
| [YYYY-MM-DD] | 🛑 Launch | All tenants | Codeless coverage metric trending positive |

### Operation Checklist

- [ ] Ohio reference Programs + Case Types seeded
- [ ] Default Work Calendar (Mon–Fri + US federal holidays) seeded
- [ ] Compliance Library wired (MOD-10)
- [ ] SSO mapping verified per tenant
- [ ] AI Governance defaults safe-off
- [ ] Audit + version + rollback wired end-to-end

---

## 15. 📣 Cross-Functional Impact Checklist

| Team | Y/N | Action |
| --- | --- | --- |
| Configuration | Y | Reference dataset + Admin onboarding. |
| Permissions | Y | Admin role definitions + RBAC. |
| Customer Success | Y | Admin Configurator handbook + training. |
| Legal | Y | AI no-training + audit retention review. |
| Reporting | Y | Persona dashboard config aligned with MOD-05. |

---

## 16. ⚠️ Risks

| Risk | L/I | Mitigation |
| --- | --- | --- |
| Bad-publish breaks production workflows | M / H | Simulator + version + rollback + grandfathering. |
| Override drifts from legal compliance | L / H | Validation ≥ legal; legal values always visible; audit. |
| Compliance binding change surprise | L / H | Mandatory acknowledgment + impact summary + audit. |
| Deactivation orphans caseload | L / H | Forced reassignment flow before confirmation (ADM-042). |
| AI governance scattered across modules | M / M | Centralised here; downstream consumes config. |
| Admin role explosion | M / M | Four well-scoped roles (System / Program / KPI / Integration). |

---

## 17. ❓ Open Questions

| Question | Impact | Date | Owner |
| --- | --- | --- | --- |
| Confirm which Admin roles are needed at launch + per-tenant role customization scope. | Affects ADM-001 design. | 2026-04-25 | Product |
| Confirm directory-sync target (SCIM, AAD, Okta?) for ADM-041. | Hard blocker for SSO. | 2026-04-25 | Security |
| Confirm bulk reassignment performance + UI for users with very large caseloads. | Affects ADM-042. | 2026-04-25 | Dev Lead |
| Confirm storage / version model for KPI policies + workflows + work item library (event-sourced vs versioned doc). | Blocks rollback design. | 2026-04-25 | Dev Lead |
| Confirm "Northwoods implementation team" intake process for new work items. | Operational dependency. | 2026-04-25 | Product |

---

## 18. 💬 FAQs

**Q:** Why can't Admins create new work items?
**A:** Each work item has a corresponding sub-screen built in code. Admins govern (program applicability, status); the build path is via the Northwoods implementation team.

**Q:** Why does compliance binding immediately recalculate KPI thresholds?
**A:** Source PRD ADM-004 defines this as an explicit, acknowledged, system-wide action — the alternative would be drift between legal and applied thresholds.

**Q:** Are KPI overrides the same as compliance overrides?
**A:** No — KPI overrides (MOD-03 FR-KPI-006) are per-work-item runtime overrides by an authorised role; compliance overrides (ADM-006) are tighter internal *thresholds* on top of legal minimums.

---

## 19. 🧭 Decision Log

| # | Decision | Rationale | Date | By |
| --- | --- | --- | --- | --- |
| 1 | Forward-looking only deactivation + grandfathering for work items + workflows. | Source ADM-020 + ADM-030; protects in-flight cases. | 2026-04-25 | Product |
| 2 | Simulator mandatory before publish (UX). | Master "configurable without code" + bad-publish prevention. | 2026-04-25 | Product |
| 3 | Compliance binding requires acknowledgment with impact summary. | Source ADM-004; matches master security guardrails. | 2026-04-25 | Product |
| 4 | AI features default off; Admin opts in per persona × program. | Master AI principles. | 2026-04-25 | Product |

---

## 20. 📝 Change Log

| Date | By | Sections | Reason |
| --- | --- | --- | --- |
| 2026-04-25 | Kendrew Peacey | All | Initial draft — derived from §14. |

---

## 21. 🧾 Informed By

★

| Document | Path | SHA-256 |
| --- | --- | --- |
| Master PRD | [[PRD - Northwoods Traverse Workspace]] | sha256:[at approval] |
| Source PRD | `00 - Inbox/Northwoods_Traverse_PRD_v2.0.docx` (§14) | sha256:[at approval] |

---

## 22. ✅ Approval and Checksum

★

### Approval checklist

- [ ] Problem statement clear
- [ ] INVEST + G/W/T satisfied across all 9 functional areas
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
