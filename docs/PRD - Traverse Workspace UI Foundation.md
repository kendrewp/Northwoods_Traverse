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
# PRD — Traverse Workspace UI Foundation (MOD-01)

> **V-Model–Aligned PRD.** Structure follows `VModel_PRD_Template.docx` (v1.0). Sections marked ★ are mandatory before Approved status.
>
> **Module PRD.** Inherits master vision, personas, and NFRs from [[PRD - Northwoods Traverse Workspace]]. Source: §8 (End-User Workspace), §20 (KPI Tile Drill-Down), §23 (Active Cases Drill-Down), §24 (Column-Header Sorting).

---

## 1. Product Overview

| Field                        | Value                                                                                                                             |
| ---------------------------- | --------------------------------------------------------------------------------------------------------------------------------- |
| 📅 Target date               | [QX YYYY — Phase 2 of master launch plan]                                                                                         |
| 🟡 Document status           | Draft                                                                                                                             |
| 🧭 Team                      | PM: [Name] · Product: [Name] · UX: [Name] · Dev Lead: [Name] · Dev: [Names] · QA: [Name]                                          |
| 🗃️ Work tracker (Jira epic)  | [CAM-XXXXX]                                                                                                                       |
| 📎 Parent PRD                | [[PRD - Northwoods Traverse Workspace]]                                                                                           |
| 🔗 Resources                 | [[Northwoods-Traverse]] · `00 - Inbox/Northwoods_Traverse_PRD_v2.0.docx` (§§8, 20, 23, 24)                                         |

---

## 2. 🎯 Objective and Problem Alignment

### Overview

Build the operational cockpit shell for Social Workers, Supervisors, and Directors — three primary surfaces (Dashboard, Cases, Work Items) plus the cross-cutting drill-down, sub-screen, and sortable-table patterns that every other module reuses. This is the user-visible foundation: navigation, role-aware layouts, KPI status indicators, the Work Item sub-screen contract, and the inline drill-down + column-sort patterns.

### Problem Statement

Workers today juggle multiple screens and have no single reliable view of "what is critical now." Supervisors and Directors lack role-appropriate aggregations. Without a unified, predictable cockpit shell, every feature module (KPI, Workflow, AI, Calendar, Reporting, etc.) would invent its own surface — leading to inconsistent UX, accessibility regressions, and broken context as users move between work items.

### Importance

- Establishes the persistent navigation, sub-screen, and modal patterns every other module will reuse.
- Surfaces "what is critical" without searching — directly delivers the master goal of low cognitive load.
- Makes role-appropriate views the default — Workers see their caseload, Supervisors see their team, Directors see programs.
- Locks in WCAG 2.1 AA, keyboard-first, and non-color KPI indicators *at the foundation* so accessibility doesn't bolt on later.

### High-Level Approach

- Three-area persistent navigation: **Dashboard**, **Cases**, **Work Items**. Workspace is the default landing page (Admin-configurable).
- **Dashboard** layout: KPI summary tiles (clickable drill-down) → AI Briefing Card (rendered by MOD-06) → work-item list (sortable columns, default Breached → Red → Yellow → Green).
- **Cases view:** Sortable + filterable case list → Case Detail.
- **Work Items view:** Unified, sortable, filterable list across all cases.
- **Work Item Sub-Screen:** In-context completion (side panel or modal) — never navigates away.
- **Drill-down pattern:** Inline modals overlay the source view; Active-Cases tile supports deeper inline drill-down to Case Detail.
- **Column-header sorting:** Replaces the legacy "Sort by:" button bar everywhere work-item tables appear.

---

## 3. 👥 Stakeholders and Personas

- **P1 — Social Worker** — primary daily user of the Workspace cockpit.
- **P2 — Supervisor** — uses the team-oriented Dashboard and Team Workload View; toggles between team and personal views.
- **P3 — Director** — uses the read-only cross-program rollup Dashboard.
- **P4 — Admin** — configures default landing, persona-specific saved views, and which navigation items appear.

Cross-module dependents: every downstream module (MOD-02 through MOD-12) renders inside or alongside this shell.

---

## 4. 🎬 Target Use Cases

- **Worker login → triage:** Worker lands on Workspace → KPI tiles show "1 Breached, 4 At Risk" → clicks Breached tile → drill-down modal lists the breached items → opens the top item via sub-screen → completes → next sequential item activates.
- **Supervisor team triage:** Supervisor lands on team Dashboard → toggles to personal view → returns to team view → drills into a worker's list.
- **Director program view:** Director lands on cross-program rollup → clicks a program card → sees supervisor-level breakdown (read-only).
- **Sortable table workflow:** Worker on Work Items view clicks "Due" column → sorts ascending; clicks again → descending; "no-due-date" rows always sink to the bottom.
- **Active-Cases drill:** Worker clicks Active Cases tile → modal lists cases → clicks a case row → inline replaces with Case Detail (workflow progress, addresses, active-step Open buttons) → "← Back to Active Cases" returns to list.
- **Saved view:** Admin publishes a "Today's red items" persona default; Worker resets to default after personal experimentation.

---

## 5. 📊 Success Conditions and Metrics

### Goals

1. Workers, Supervisors, and Directors land on a role-appropriate cockpit on every login.
2. KPI status is visible, color- *and* label-coded, on every list surface.
3. All work-item tables share the same column-sort and filter contract.
4. Drill-downs stay in context — no full-page navigation away from the cockpit.
5. WCAG 2.1 AA verified at this foundation level.

### Non-Goals

- KPI computation logic (MOD-03).
- Workflow execution semantics (MOD-02).
- Notifications and alert delivery (MOD-04).
- AI Briefing Card content generation (MOD-06; this PRD only owns the slot).
- Reporting / analytics surfaces (MOD-05).

### Success Metrics

| Goal | Metric (quantified) |
| --- | --- |
| Cockpit landing adoption | % of in-scope users landing on Workspace as default within 30 days of rollout — target ≥ 95 % |
| Time-to-first-action | Median seconds from login to opening a work-item sub-screen — [target TBD] |
| Sortable table consistency | Zero column-sort UX regressions detected in cross-table audit at launch |
| Accessibility | Zero WCAG 2.1 AA blocker findings at launch audit |
| Drill-down stability | 0 cases of nested-modal stacking in QA (TILE-005 #6) |

### Guardrails

- No PII in URL parameters or browser history (cross-cuts every list view).
- No work-item completion path that requires navigating away from the cockpit.
- KPI status indicators must use both color and label/icon — color-only fails WCAG.
- Dashboard must load with placeholder cards even when downstream services (KPI engine, AI) are degraded.

---

## 6. 🤔 Assumptions

- The KPI Engine (MOD-03) returns risk status + explainability payload via a shared API. — ❓ unvalidated
- The AI Briefing Card (MOD-06) is rendered as a slot inside the Dashboard layout owned by this module. — ❓ unvalidated
- Saved-view storage and persona-default publishing are part of MOD-07 Admin Configuration. — ❓ unvalidated
- "Active case" semantics align with base Traverse case state. — ❓ unvalidated

---

## 7. 🔗 Dependencies and V-Model Gates

★ **V-MODEL MANDATORY**

| Dependency | Type | Status | Impact / Notes |
| --- | --- | --- | --- |
| MOD-03 KPI / SLA Policy Engine | Feature (internal) | DRAFT | Provides the risk status + explainability payload rendered everywhere. Hard blocker. |
| MOD-02 Workflow Execution Engine | Feature (internal) | DRAFT | Provides Work Item state used in lists + sub-screen. Hard blocker. |
| MOD-06 AI Copilot | Feature (internal) | DRAFT | Renders the AI Briefing Card content; this module owns the slot only. Soft dependency — fallback messaging used if absent. |
| MOD-07 Admin Configuration Suite | Feature (internal) | DRAFT | Owns default-landing, persona-default views, navigation visibility. Hard blocker for Admin-driven config. |
| MOD-08 Universal Search | Feature (internal) | DRAFT | Renders the persistent top search bar inside this shell. Slot only — soft dependency. |
| Identity Provider (SSO / RBAC) | External system | PENDING | RBAC drives navigation visibility (FR-WKS-001 #3). Hard blocker. |
| Dev Lead technical feasibility sign-off | V-Model gate | PENDING | ★ Required before Approval. |
| T-shirt estimate — Estimation Checkpoint 1 | V-Model gate | PENDING | ★ Required before Approval. |

---

## 8. 📋 Requirements

★ **V-MODEL MANDATORY**

| Priority | Job Story (INVEST) | Acceptance Criteria (Given / When / Then) | Jira | Notes / Source |
| --- | --- | --- | --- | --- |
| P0 | As an Admin, when I configure landing pages, I want to set Workspace as the default for personas / teams / org so that users land on the cockpit after login. | AC-1.1: *Given* an Admin has set Workspace as default for the Social Worker persona, *When* a Social Worker authenticates, *Then* they land on the Workspace Dashboard. AC-1.2: *Given* a user is on another module, *When* they click Workspace in primary nav, *Then* they return to Workspace without losing session context. AC-1.3: *Given* a user lacks a navigation permission, *When* the nav renders, *Then* the unpermitted item is hidden. | [CAM-XXXXX] | FR-WKS-001 |
| P0 | As any persona, when I navigate the cockpit, I want a persistent left nav with Dashboard, Cases, Work Items so that I always know where I am. | AC-2.1: *Given* I am on the Workspace, *When* the Dashboard is selected, *Then* it is visually indicated as active. AC-2.2: *Given* I tab through nav with the keyboard, *When* I reach each item, *Then* focus is visible and matches WCAG 2.1 AA. | [CAM-XXXXX] | FR-WKS-002 |
| P0 | As a Social Worker, when I land on the Dashboard, I want six KPI summary tiles so that I see real-time counts at a glance. | AC-3.1: *Given* I am on the Dashboard, *When* it renders, *Then* tiles for Today's Tasks, At Risk, Breached, Active Cases, Pending Actions, and Unread Alerts are displayed with count and color. AC-3.2: *Given* tile counts are out of date, *When* a relevant event occurs, *Then* counts update within 60 seconds. AC-3.3: *Given* I click a tile, *When* the click resolves, *Then* the work-item list filters to that category. AC-3.4: *Given* I am a Social Worker, *When* tiles render, *Then* counts respect my caseload scope. | [CAM-XXXXX] | FR-WKS-010 |
| P0 | As a Social Worker, when I land on the Dashboard, I want an AI Briefing Card slot so that I can read my start-of-day summary. | AC-4.1: *Given* AI is available, *When* the Dashboard loads, *Then* the AI Briefing Card renders below tiles and above the work-item list with a generated timestamp and rate-limited refresh control. AC-4.2: *Given* AI is unavailable, *When* the Dashboard loads, *Then* the card shows a non-blocking fallback message and standard summary counts remain visible. AC-4.3: *Given* an Admin has disabled the card for my persona/program, *Then* the slot is hidden. | [CAM-XXXXX] | FR-WKS-011 — slot only; content owned by MOD-06 |
| P0 | As any persona, when the work-item list renders, I want default sort by KPI risk status so that the most urgent item is first. | AC-5.1: *Given* the Dashboard / Work Items view loads, *When* no user sort is set, *Then* the list is ordered Breached → Red → Yellow → Green. AC-5.2: *Given* I select "By Case" or "By Due Date" sort, *Then* the list re-orders accordingly. AC-5.3: *Given* I change sort during a session, *Then* my preference persists for the session. AC-5.4: *Given* a row renders, *Then* it shows Work Item name, Case, Program Type, Case Type, KPI status (color + label), and time remaining / overdue. | [CAM-XXXXX] | FR-WKS-012 |
| P0 | As a Supervisor, when I land on the Dashboard, I want a team-oriented view with team KPI summary, supervisor-scoped briefing, and team work-item list so that I can manage at the team level. | AC-6.1: *Given* I am a Supervisor, *When* the Dashboard loads, *Then* tile counts aggregate across my assigned Social Workers. AC-6.2: *Given* I am on the team view, *When* I select a Social Worker, *Then* I drill into their work list within my permission scope. AC-6.3: *Given* I am also a Worker, *When* I toggle, *Then* I can switch between team and personal views. | [CAM-XXXXX] | FR-WKS-013 |
| P0 | As a Director, when I land on the Dashboard, I want a read-only cross-program rollup so that I can monitor compliance without seeing individual cases. | AC-7.1: *Given* I am a Director, *When* the Dashboard loads, *Then* I see one card/row per Program I oversee with active cases, at-risk count, breached count, on-time rate, and trend vs prior period. AC-7.2: *Given* I click a program card, *Then* I see supervisor-level breakdown. AC-7.3: *Given* I am a Director, *Then* no task actions are available — all data is read-only. | [CAM-XXXXX] | FR-WKS-014 |
| P0 | As any persona, when I open the Cases view, I want a sortable / filterable case list with case detail drill-in so that I can navigate my caseload. | AC-8.1: *Given* I open Cases, *Then* columns include Case ID, Client (masked per permissions), Program Type, Case Type, Assigned Worker (Supervisor view), Current Work Item, KPI Status, Case Open Date. AC-8.2: *Given* I click a column header, *Then* the table sorts ascending; *When* I click again, *Then* it sorts descending; *Then* an active sort indicator (↑/↓) is visible; *And* KPI Status sorts by urgency (Breached → Critical → At Risk → On Track). AC-8.3: *Given* an active sort + filter, *When* I change a filter, *Then* the sort is preserved. AC-8.4: *Given* I click a case row, *Then* the Case Detail view opens. | [CAM-XXXXX] | FR-WKS-020 + COL-001/002/003 alignment |
| P0 | As any persona, when I open the Work Items view, I want a unified work-item list with configurable columns and filters so that I can work across cases. | AC-9.1: *Given* I open Work Items, *Then* columns include Work Item Name, Case Name, Program Type, Case Type, KPI Status, Due Date/Time, Time Remaining/Overdue, Owner. AC-9.2: *Given* default sort, *Then* it is KPI risk status ascending (Breached at top). AC-9.3: *Given* advanced filtering, *Then* I can filter by KPI Status, Program Type, Case Type, Due Date window, and Owner. | [CAM-XXXXX] | FR-WKS-030 |
| P0 | As any persona, when a work item is rendered, I want a consistent KPI risk indicator (color + label) so that the status is unambiguous and accessible. | AC-10.1: *Given* a work item, *When* its risk status is evaluated, *Then* it is shown as Green / Yellow / Red / Breached using both color and text label/icon. AC-10.2: *Given* a relevant event occurs, *Then* the indicator updates within 60 seconds. AC-10.3: *Given* WCAG audit, *Then* contrast and screen-reader label requirements are met. | [CAM-XXXXX] | FR-WKS-031 |
| P0 | As a Social Worker, when I click an active work item, I want a sub-screen to complete it without leaving the cockpit so that I stay in flow. | AC-11.1: *Given* I click an active work item, *When* the sub-screen opens, *Then* its type matches the Work Item Library definition (notes / data collection / document upload). AC-11.2: *Given* required fields are filled and I confirm, *Then* the work item closes, the next sequential item activates, and the next KPI clock starts. AC-11.3: *Given* I close the sub-screen, *Then* I return to my prior position in the list. AC-11.4: *Given* I attempt completion with missing required fields, *Then* completion is blocked with field-level errors. AC-11.5: *Given* completion succeeds, *Then* an audit entry records who, when, and from which screen. | [CAM-XXXXX] | FR-WKS-032 |
| P0 | As any persona, when a work item has a KPI, I want a "Why is this red?" explainability panel so that I can audit the calculation. | AC-12.1: *Given* a work item with a KPI, *When* I open the explainability panel, *Then* it shows policy name/version, start time, due time, pause periods, time remaining, thresholds, and breach status. AC-12.2: *Given* the panel is open, *Then* the AI Plain-Language Explanation slot (FR-AI-002) is rendered. AC-12.3: *Given* the policy cannot be evaluated, *Then* a clear error state with guidance is shown. | [CAM-XXXXX] | FR-WKS-033 — slot for AI explanation in MOD-06 |
| P1 | As an Admin / user, when I work with filtered + sorted lists frequently, I want saved views with persona defaults so that I do not reconfigure every session. | AC-13.1: *Given* a current filter/sort combination, *When* I save a view with a name, *Then* it is available on next visit. AC-13.2: *Given* I am an Admin, *When* I mark a saved view as default for a persona, *Then* members of that persona land on it. AC-13.3: *Given* a user has personalised a view, *When* they reset to persona default, *Then* the persona default loads. | [CAM-XXXXX] | FR-WKS-034 |
| P0 | As a Supervisor, when I land on the team Dashboard, I want a Team Workload View with at-risk counts, breaches, distribution, and per-worker capacity so that I can balance proactively. | AC-14.1: *Given* the team workload view, *Then* it lists my assigned Social Workers with at-risk count, breach count, workload distribution, and capacity signal (where configured). AC-14.2: *Given* I drill in, *Then* I see the worker's individual work-item list within scope. AC-14.3: *Given* filters, *Then* I can filter by Program Type, Case Type, and KPI Status. | [CAM-XXXXX] | FR-WKS-040 |
| P0 | As a Supervisor, when team load shifts, I want to assign and reassign cases / work items so that I can rebalance. | AC-15.1: *Given* I (re)assign a case or work item, *Then* an audit entry records old owner, new owner, optional reason, and timestamp. AC-15.2: *Given* I am assigning, *Then* the system may surface AI candidate recommendations (MOD-06 FR-AI-006). AC-15.3: *Given* the team workload view or case/work-item view, *Then* I can perform assignment from either surface. | [CAM-XXXXX] | FR-WKS-041 — full action UX deepens in MOD-11 |
| P0 | As a Supervisor, when intake referrals arrive, I want to view + manage intake queues so that I can route correctly. | AC-16.1: *Given* I have permission for a queue, *Then* I see all items in my Program's intake queue. AC-16.2: *Given* an item, *Then* a Worker can claim it or I can assign it. AC-16.3: *Given* queue ordering rules (e.g., referral date, priority), *Then* items appear in the configured order. | [CAM-XXXXX] | FR-WKS-042 |
| P0 | As a Supervisor, when a qualifying event occurs on a case, I want to trigger a secondary workflow with confirm / override of KPI pause default so that policy is enforced and audited. | AC-17.1: *Given* I trigger a secondary workflow, *Then* the system shows the event, the secondary workflow, and the default KPI pause behavior. AC-17.2: *Given* the prompt, *Then* I can confirm default or override (pause / continue primary KPIs). AC-17.3: *Given* I confirm, *Then* the secondary's first work item activates and its KPI clock starts; an audit entry records actor, timestamp, event, and pause decision. | [CAM-XXXXX] | FR-WKS-043 — workflow semantics owned by MOD-02 |
| P0 | As a Supervisor, when an exception is needed, I want to approve KPI due-date overrides under Admin governance so that exceptions are explicit and audited. | AC-18.1: *Given* an override, *Then* the work item is visually labeled (not silent). AC-18.2: *Given* an override, *Then* an audit entry records actor, reason, original due, new due, and timestamp. AC-18.3: *Given* Admin governance restricts overrides by program / case type / work-item type, *Then* the action is gated accordingly. | [CAM-XXXXX] | FR-WKS-044 |
| P0 | As any persona using assistive tech, when I use the Workspace, I want WCAG 2.1 AA compliance so that the cockpit is usable with keyboard and screen reader. | AC-19.1: *Given* WCAG 2.1 AA audit, *Then* all interactive elements are keyboard reachable, contrast meets ratio, and KPI status uses label/icon in addition to color. AC-19.2: *Given* AI-generated content surfaces, *Then* it is screen-reader accessible. | [CAM-XXXXX] | FR-WKS-050 |
| P0 | As any persona, when I see due dates and times, I want my configured time zone applied with localised formats so that the data is correct for me. | AC-20.1: *Given* I have a time zone, *Then* all dues display in that time zone with explicit context. AC-20.2: *Given* KPI calculations, *Then* they apply consistent UTC-based rules. | [CAM-XXXXX] | FR-WKS-051 |
| P1 | As a Social Worker, when I click a KPI summary tile, I want a contextual drill-down modal so that I can investigate without losing the Dashboard. | AC-21.1: *Given* any of the six tiles, *Then* it has hover + cursor + drill-down icon affordances. AC-21.2: *Given* I click, *Then* a modal opens overlaying the Dashboard; the tile color is carried into the modal header. AC-21.3: *Given* the modal, *Then* it can be closed via X, Close, or backdrop click. AC-21.4: *Given* the modal header, *Then* it shows tile name and item count. AC-21.5: *Given* content overflow, *Then* the modal scrolls internally; Dashboard does not. AC-21.6: *Given* I open a Work Item from inside a tile modal, *Then* the tile modal closes before the Work Item sub-screen opens (no nested modals). AC-21.7: *Given* desktop ≥ 1280 px, *Then* the modal width fits without horizontal scroll. | [CAM-XXXXX] | TILE-001 + TILE-005 |
| P1 | As a Social Worker, when I click Today's Tasks / At Risk / Breached / Pending Actions tiles, I want a Work Item drill-down modal so that I see the matching items and can open them. | AC-22.1: *Given* Today's Tasks, *Then* the modal lists all my active work items. AC-22.2: *Given* At Risk, *Then* only Yellow KPI items. AC-22.3: *Given* Breached, *Then* only Breached items. AC-22.4: *Given* Pending Actions, *Then* Breached / Red / Yellow items. AC-22.5: *Given* a row, *Then* it shows Work Item name, Case name, Program, KPI badge, Due date. AC-22.6: *Given* an Open button or row click, *Then* the tile modal closes and the Work Item sub-screen opens. AC-22.7: *Given* an empty category, *Then* an "All clear" empty state is shown. | [CAM-XXXXX] | TILE-002 |
| P1 | As a Social Worker, when I click Active Cases, I want a Cases drill-down modal with a read-only summary table so that I see my caseload at a glance. | AC-23.1: *Given* the modal, *Then* columns are Case ID, Client, Program, Case Type, Current Work Item, KPI Status, Date Opened. AC-23.2: *Given* ordering, *Then* it matches the Cases List view. AC-23.3: *Given* any case, *Then* KPI Status uses the standard badge. AC-23.4: *Given* the modal, *Then* it is read-only — full case management lives in Cases view. | [CAM-XXXXX] | TILE-003 |
| P1 | As a Social Worker, when I click a row inside the Active Cases drill-down, I want inline navigation into Case Detail (no nested modal) so that I can see workflow progress and open active items. | AC-24.1: *Given* a row, *Then* it has cursor pointer + hover + a "View" button. AC-24.2: *Given* I click row or View, *Then* the Case Detail view replaces the case list inside the same modal — no second overlay. AC-24.3: *Given* Case Detail is open, *Then* the modal header shows client name, case ID, program, and KPI badge; "← Cases" breadcrumb appears in the header; "← Back to Active Cases" appears in the footer. AC-24.4: *Given* I close the modal from Case Detail, *Then* it exits entirely (does not return to list). | [CAM-XXXXX] | ACD-001 |
| P1 | As a Social Worker, when I view Case Detail, I want full workflow progress with active items directly openable so that I can pivot to action. | AC-25.1: *Given* Case Detail, *Then* a 3-column info strip shows Case Type, Assigned Worker, Date Opened. AC-25.2: *Given* a client address on file, *Then* it is displayed in a highlighted address panel above the workflow. AC-25.3: *Given* the workflow section, *Then* all work items are listed in sequence with step number, name, KPI badge (active items), due/due-label (active), completed date (completed), and lock indicator (locked). AC-25.4: *Given* states, *Then* completed steps show a green checkmark; active steps a color-coded circle matching KPI; locked steps a neutral gray circle. AC-25.5: *Given* an active item, *Then* an "Open →" button appears; clicking it closes the tile modal and opens the Work Item sub-screen with case context preserved. AC-25.6: *Given* the workflow header, *Then* a counter (e.g. "2/5 steps complete") is shown. AC-25.7: *Given* completed items, *Then* the first 80 chars of notes are shown as a preview. | [CAM-XXXXX] | ACD-002 |
| P1 | As a Social Worker, when I click Unread Alerts, I want a Notifications drill-down modal so that I can dismiss / triage in place. | AC-26.1: *Given* the modal, *Then* it lists only unread notifications with severity icon, message, timestamp. AC-26.2: *Given* an alert, *Then* a Dismiss button marks it read. AC-26.3: *Given* no unread alerts, *Then* an "All caught up" empty state is shown. AC-26.4: *Given* dismissed alerts, *Then* they are removed from the list within the same session. | [CAM-XXXXX] | TILE-004 |
| P1 | As any persona, when I sort a work-item table, I want column-header click sorting (replacing the legacy "Sort by:" button bar) so that interaction matches familiar table conventions. | AC-27.1: *Given* a sortable column, *Then* the header is interactive (pointer cursor, highlighted background when active). AC-27.2: *Given* the indicator, *Then* it shows ⇅ neutral, ↑ ascending, ↓ descending. AC-27.3: *Given* a non-active column, *Then* clicking it sets ascending. AC-27.4: *Given* the active column, *Then* clicking toggles direction. AC-27.5: *Given* the legacy "Sort by:" bar, *Then* it is removed from Dashboard and Work Items screens. AC-27.6: *Given* the filter row, *Then* a brief instructional hint is shown ("Click any column header to sort"). | [CAM-XXXXX] | COL-001 |
| P1 | As any persona, when I sort columns, I want sort logic appropriate to data type so that order matches semantic meaning. | AC-28.1: *Given* text columns (Work Item, Case, Program, Case Type, Type), *Then* sort is alphabetic case-insensitive. AC-28.2: *Given* KPI Status, *Then* ascending = Breached → Critical → At Risk → On Track → Completed → Locked. AC-28.3: *Given* the Due column, *Then* sort uses ISO date value, not display label. AC-28.4: *Given* a row with no due date, *Then* it sorts to bottom regardless of direction. AC-28.5: *Given* the initial page load, *Then* default sort is KPI Status ascending. | [CAM-XXXXX] | COL-002 |
| P1 | As any persona, when I combine filters and sorts, I want them to coexist so that filtering does not reset sorting. | AC-29.1: *Given* a Program filter, *Then* visible rows narrow accordingly. AC-29.2: *Given* a KPI Status filter, *Then* visible rows narrow by status (Breached, Critical, At Risk, On Track). AC-29.3: *Given* an active sort, *When* I change either filter, *Then* the sort column and direction are preserved. AC-29.4: *Given* the filter row, *Then* both dropdowns are present on Dashboard and Work Items, replacing the legacy non-functional "All KPI Status" placeholder. | [CAM-XXXXX] | COL-003 |

---

## 9. ⚙️ Non-Functional Requirements

★ **V-MODEL MANDATORY** — inherits master §9. Module-specific deltas:

**Performance:** Dashboard initial render ≤ 2 s (master). Tile drill-down modal opens ≤ 200 ms after click. Column-sort interaction renders ≤ 100 ms for ≤ 1 000 visible rows (paged thereafter).

**Security:** No PII in URL parameters. RBAC governs nav visibility per FR-WKS-001 #3. Field masking respected on every list and drill-down modal.

**Accessibility:** WCAG 2.1 AA mandatory at this foundation level. Keyboard reachability, screen-reader labels on KPI status, focus-visible states on all interactive elements, modal focus trap with Esc-to-close.

**Scale:** Tables degrade gracefully past 1 000 rows via paging or infinite scroll with performance safeguards (FR-WKS-012 #4).

**Compatibility:** Modern evergreen browsers. Responsive layout — desktop ≥ 1280 px is the supported baseline for tile modals (TILE-005 #7); tablet/mobile rendering is a Module 9 / future concern.

**Observability:** Telemetry events: dashboard_loaded, tile_clicked, work_item_subscreen_opened, work_item_completed (cockpit slice), drill_down_opened, drill_down_closed, column_sort_changed, filter_changed, saved_view_used.

---

## 10. 🎨 User Interaction and Design

### UI Regions / Panels

- **Persistent left navigation:** Dashboard · Cases · Work Items (plus Calendar / Reporting / Search slots from other modules).
- **Persistent top search bar (slot):** rendered by MOD-08.
- **Dashboard region:** KPI summary tiles row → AI Briefing Card slot (MOD-06) → work-item list with filter/sort row.
- **Cases view:** Filter row + sortable list → Case Detail.
- **Work Items view:** Filter row + sortable list.
- **Drill-down modals:** Inline overlay; single-active-modal contract (no nested stacks).
- **Work Item Sub-Screen:** Side panel or modal; in-context, never navigates away.
- **KPI Explainability Panel:** "Why is this red?" — surfaces in list rows and Case Detail.

### UX Principles

- Cockpit-first — work happens in this shell; downstream modules render inside it.
- Non-color KPI indicators required everywhere risk status is shown.
- Single drill-down modal at any time — opening a work item from a tile modal closes the tile modal first.
- Filters and sorts coexist — a filter change never resets a sort.
- Saved views are the personalisation surface; Admin owns persona defaults.

---

## 11. 🔄 Key Flows

### Flow 1: Worker login → triage breached item

**Trigger:** Worker authenticates.
**Preconditions:** Workspace is the configured default landing for the persona; KPI engine is reachable.
**Steps:**
1. System lands user on Workspace Dashboard.
2. Six tiles render with current counts; AI Briefing Card renders (or falls back).
3. Work-item list defaults to KPI risk sort; first row is a Breached item.
4. User clicks Breached tile → drill-down modal opens with all breached items.
5. User clicks a row → tile modal closes → Work Item sub-screen opens.
6. User completes required fields → confirms → next sequential item activates (MOD-02).
**Exit state:** Work item closed; user returned to prior position in the list with updated counts.
**Handoff:** MOD-02 advances workflow; MOD-03 recomputes KPI; MOD-04 may emit notification.

### Flow 2: Sort + filter on Work Items view

**Trigger:** User opens Work Items.
**Preconditions:** RBAC scope determined.
**Steps:**
1. List loads with default sort (KPI Status ascending).
2. User clicks Due column header → list resorts ascending; ↑ indicator appears; rows with no due date sink to bottom.
3. User selects Program filter → visible rows narrow; sort preserved.
4. User clicks Due column again → list flips to descending.
**Exit state:** User sees filtered + sorted list; preference persists for session.
**Handoff:** N/A.

### Flow 3: Active Cases tile → Case Detail inline

**Trigger:** Worker clicks Active Cases tile.
**Preconditions:** Worker has at least one active case.
**Steps:**
1. Drill-down modal opens with read-only case summary table.
2. Worker clicks a case row → modal content swaps inline to Case Detail (no nested modal).
3. Modal header updates (client, case ID, program, KPI badge); "← Cases" breadcrumb appears.
4. Worker clicks Open → on an active step → modal closes → Work Item sub-screen opens.
**Exit state:** User on the right work item with case context preserved.
**Handoff:** MOD-02 work-item completion path.

---

## 12. 🚫 Out of Scope and Deferred

### Out of Scope

- KPI evaluation / threshold logic — MOD-03.
- Workflow sequencing / pause-resume semantics — MOD-02.
- AI Briefing content — MOD-06.
- Notification delivery — MOD-04.
- Reporting analytics surfaces — MOD-05.
- Calendar surface — MOD-09.
- Universal search — MOD-08.

### Deferred

- Mobile-first responsive layouts beyond desktop ≥ 1280 px baseline — handled per future iteration / per MOD-09 for field-worker mobile.
- Per-user theme customisation — not in scope; tiles use system / accessibility palette.
- Cross-persona shared saved views — MOD-07 may introduce in a future cycle.

---

## 13. 🧬 V-Model Traceability Matrix

★ Structure required at Approval. Populated progressively.

| AC ID | Story / Jira | Acceptance Criterion (summary) | Gherkin file path | Test class | Verdict |
| --- | --- | --- | --- | --- | --- |
| AC-1.1 | [CAM-XXXXX] | Workspace is default landing | `docs/{FEATURE-ID}/gherkin/wks-001-default-landing.feature` | [Populated by dev] | --- |
| AC-3.1 | [CAM-XXXXX] | KPI summary tiles render | `docs/{FEATURE-ID}/gherkin/wks-010-kpi-tiles.feature` | --- | --- |
| AC-5.1 | [CAM-XXXXX] | Default work-item sort | `docs/{FEATURE-ID}/gherkin/wks-012-default-sort.feature` | --- | --- |
| AC-11.1 | [CAM-XXXXX] | Sub-screen opens in context | `docs/{FEATURE-ID}/gherkin/wks-032-subscreen.feature` | --- | --- |
| AC-19.1 | [CAM-XXXXX] | WCAG 2.1 AA compliance | `docs/{FEATURE-ID}/gherkin/wks-050-accessibility.feature` | --- | --- |
| AC-21.6 | [CAM-XXXXX] | Tile-modal then sub-screen — no nesting | `docs/{FEATURE-ID}/gherkin/tile-005-no-nesting.feature` | --- | --- |
| AC-24.2 | [CAM-XXXXX] | Active-Cases inline detail (no second overlay) | `docs/{FEATURE-ID}/gherkin/acd-001-inline-detail.feature` | --- | --- |
| AC-27.1 | [CAM-XXXXX] | Column-header sort interaction | `docs/{FEATURE-ID}/gherkin/col-001-header-sort.feature` | --- | --- |
| AC-28.4 | [CAM-XXXXX] | No-due-date rows sink to bottom | `docs/{FEATURE-ID}/gherkin/col-002-no-date-bottom.feature` | --- | --- |

(Remaining ACs to be populated during generate-gherkin.)

---

## 14. 🚀 Launch Plan

### Key Milestones

| Target date | Milestone | Description | Exit criteria |
| --- | --- | --- | --- |
| [YYYY-MM-DD] | ✅ UAT | Internal cockpit usability test with reference Workers / Supervisors / Directors | No P0/P1 bugs on a rolling 7-day basis |
| [YYYY-MM-DD] | 🛑 Early Access | Reference county pilot | At least one win per persona |
| [YYYY-MM-DD] | 🛑 Launch | All in-scope tenants | Success metrics in §5 trending positive |

### Operation Checklist

- [ ] Default landing flag wired through MOD-07 Admin
- [ ] RBAC permissions for nav items mapped
- [ ] Telemetry events wired (§9 Observability)
- [ ] WCAG 2.1 AA audit completed
- [ ] Persona default saved views published (MOD-07)
- [ ] Fallback messaging for KPI / AI degraded modes verified

---

## 15. 📣 Cross-Functional Impact Checklist

| Team | Prompt | Y/N | Action (if Yes) |
| --- | --- | --- | --- |
| Analytics | Telemetry for cockpit interactions? | Y | Wire events in §9 Observability. |
| Configuration | Persona default landing + saved views? | Y | Coordinate with MOD-07. |
| Sales | Sales enablement? | Y | Cockpit demo flows. |
| Marketing | Launch messaging? | Y | Phase 2 of master §14. |
| Customer Success | Training? | Y | Persona quick-start guides. |
| Permissions | New permissions? | Y | Nav-item permissions per persona. |
| Reporting | New reports? | N | Cockpit consumes; MOD-05 owns reporting. |

---

## 16. ⚠️ Risks and Mitigations

| Risk | Likelihood / Impact | Mitigation |
| --- | --- | --- |
| Cockpit rendering blocks on degraded KPI / AI services | M / H | Fallback states required at this module level (FR-WKS-011 #3); contracts with MOD-03 / MOD-06 enforce non-blocking failure modes. |
| Nested-modal stacking ships and degrades UX | M / M | Single-active-modal contract codified in TILE-005 #6 + ACD-001; QA gate before launch. |
| WCAG regressions creep in via downstream modules rendering inside the shell | M / H | Module-level a11y audit gate (master §9); shared component library and audit tooling. |
| Sort + filter interaction surprises (filter resets sort) | M / M | COL-003 explicitly mandates preservation; covered by Gherkin. |
| Saved-view storage / sync overruns Admin scope | L / M | Storage contract owned by MOD-07; this module consumes via shared API. |

---

## 17. ❓ Open Questions

★ Must be empty before Approved.

| Question | Impact / Blocks | Date raised | Owner / Needed by |
| --- | --- | --- | --- |
| Confirm KPI status enum + ordering across UI surfaces (Breached / Critical / At Risk / On Track / Completed / Locked). | Blocks COL-002 #2 implementation. | 2026-04-25 | Product + MOD-03 lead / before design-feature |
| Confirm whether saved-view storage is server-side per-user or local-only for sessions. | Affects FR-WKS-034 implementation surface. | 2026-04-25 | Dev Lead + MOD-07 lead |
| Confirm tablet / mobile responsive scope for cockpit at launch. | Affects component library + QA matrix. | 2026-04-25 | Product / before UAT |
| Confirm Notification dismiss semantics (read-only flag vs delete) for TILE-004. | Affects MOD-04 contract. | 2026-04-25 | MOD-04 lead |
| Confirm "capacity signal" data source for FR-WKS-040 #3 — is it derived from active-work-item count, KPI-load, or manual config? | Affects supervisor view + MOD-11 design. | 2026-04-25 | Product / before MOD-11 design |

---

## 18. 💬 FAQs

**Q:** Why does the AI Briefing Card live in this module if MOD-06 owns AI?
**A:** This module owns the *slot* (where the card renders, fallback behavior, Admin enable/disable), MOD-06 owns the *content* (generation, prompt, citations).

**Q:** Why do drill-down modals live here instead of in MOD-08 Search or MOD-04 Notifications?
**A:** The single-active-modal contract is a foundational UX rule that every module must comply with. Codifying it here prevents drift.

**Q:** What about Director / Supervisor "report" surfaces?
**A:** This PRD owns the *Dashboard rollup card* style for Directors (FR-WKS-014). Deeper analytics live in MOD-05.

---

## 19. 🧭 Decision Log

| # | Decision | Rationale / Alternatives | Date | Decided by |
| --- | --- | --- | --- | --- |
| 1 | Replace legacy "Sort by:" button bar with column-header sorting everywhere work items are listed. | Matches familiar table conventions and removes a redundant UI element (source §24). | 2026-04-25 | Product (per source PRD) |
| 2 | Active Cases drill-down uses *inline* navigation (case list ↔ case detail in one modal) rather than nested modal. | Avoids modal stacking and preserves a single-active-modal UX rule (ACD-001 #3). | 2026-04-25 | Product |
| 3 | Default work-item sort is KPI Status ascending (Breached at top). | Optimises for "what is critical now" — the primary cockpit goal. | 2026-04-25 | Product |
| 4 | Foundation owns the AI Briefing Card *slot*, not the content. | Decouples UX shell from AI service availability; supports degraded mode. | 2026-04-25 | Product |

---

## 20. 📝 Change Log

| Date | Changed by | Section(s) | Reason |
| --- | --- | --- | --- |
| 2026-04-25 | Kendrew Peacey | All | Initial draft — derived from source PRD §§8, 20, 23, 24. |

---

## 21. 🧾 Informed By (Knowledge Lineage)

★ **V-MODEL MANDATORY**

| Document | Path / link | Integrity (SHA-256 first 8 hex) |
| --- | --- | --- |
| Master PRD | [[PRD - Northwoods Traverse Workspace]] | sha256:[computed at approval] |
| Source PRD | `00 - Inbox/Northwoods_Traverse_PRD_v2.0.docx` (§§8, 20, 23, 24) | sha256:[computed at approval] |

---

## 22. ✅ Approval and Checksum

★ **V-MODEL MANDATORY**

### Approval checklist

- [ ] Problem statement clear and specific
- [ ] Every job story passes INVEST
- [ ] Every AC in Given / When / Then format
- [ ] Every AC traces to a story and Jira issue
- [ ] NFRs quantified (or explicit N/A + rationale)
- [ ] Out of Scope and Deferred explicit with rationale
- [ ] §17 Open Questions resolved
- [ ] Assumptions validated or logged as open questions
- [ ] Dev Lead technical feasibility sign-off IN PLACE
- [ ] Estimation Checkpoint 1 IN PLACE
- [ ] V-Model Traceability Matrix populated with AC + Story IDs
- [ ] Decision Log, Change Log, Informed By current

### Approval record

| Field | Value |
| --- | --- |
| Approved by | [PM name and role] |
| Approval date | [YYYY-MM-DD] |
| Document SHA-256 | sha256-[computed at approval] |
| Pipeline tracker entry | [Path to feature pipeline tracker] |

---

*Template version 1.0 · Camis V-Model–Aligned PRD · Compatible with `camis-v-model:draft-prd` skill v2.1.0*
