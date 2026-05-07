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
# PRD — Traverse Deputy Director / Statewide Oversight (MOD-12)

> **V-Model–Aligned PRD.** Inherits from [[PRD - Northwoods Traverse Workspace]]. Source: §29.

---

## 1. Product Overview

| Field | Value |
| --- | --- |
| 📅 Target date | [QX YYYY — Phase 6 of master launch plan] |
| 🟡 Document status | Draft |
| 🧭 Team | PM / Product / Dev Lead / Dev / QA: [Names] |
| 🗃️ Work tracker (Jira epic) | [CAM-XXXXX] |
| 📎 Parent PRD | [[PRD - Northwoods Traverse Workspace]] |
| 🔗 Resources | `00 - Inbox/Northwoods_Traverse_PRD_v2.0.docx` (§29) |

---

## 2. 🎯 Objective and Problem Alignment

### Overview

A new top-level persona — **Deputy Director** — for tenants that operate at the state level over multiple county Directors. Gated by a per-tenant `stateManagement.enabled` flag in Admin Client Configuration. When enabled, the Deputy persona has a **read-only** statewide cockpit: Dashboard with County Overview + drill-down, Statewide Analytics hub (three sub-views), and a Cases view with a County column / filter. Work Items is intentionally not in the Deputy nav — Deputies access work items only by drilling into a case.

### Problem Statement

State agencies overseeing county-level operations today have no role-appropriate, read-only statewide view. Comparing counties, identifying outliers, and reviewing program-level performance state-wide requires manual roll-ups. Tenants that are county-only deployments must not see this persona at all.

### Importance

- Read-only safe oversight at the state level — accountability remains with Directors and Supervisors.
- Per-tenant gating respects deployment reality (county-only vs state+county).
- Reuses Director Analytics patterns at state aggregation — predictable UX.
- County drill-down stays inline (no modal) — consistent with master state-swap pattern.

### High-Level Approach

- **Gate:** `stateManagement.enabled` flag in Admin Client Configuration (DEP-001). When false, Deputy persona is *entirely absent* — no UI affordance hints at it.
- **Persona:** Deputy tab between Director and Admin in the persona switcher; sidebar contains Dashboard, Cases, Statewide Analytics. No Work Items in nav.
- **Dashboard:** Statewide AI Briefing → 4 stat tiles → County Overview table → Statewide Program Summary; clickable rows drill inline into County Detail.
- **Statewide Analytics:** same three sub-views as Director Analytics — Program Performance, KPI Trend Analysis, Staffing & Capacity — aggregated state-wide.
- **Cases:** County column + County filter, sortable; visible only to Deputy persona; case records carry a county field.

---

## 3. 👥 Stakeholders and Personas

- **Deputy Director (state-level)** — primary user.
- **P3 — Director (county-level)** — accountable below Deputy.
- **P4 — Admin** — toggles `stateManagement.enabled`.
- **MOD-01 Workspace UI** — hosts Cases / Dashboard surfaces.
- **MOD-05 Reporting & Analytics** — Statewide Analytics extends Director's hub.

Persona hierarchy in Traverse One: **Deputy Director (State) → Director (County) → Supervisor → Social Worker → Cases**.

---

## 4. 🎬 Target Use Cases

- **Onboarding (state tenant):** Admin enables State Management; Deputy persona tab appears in switcher.
- **Statewide briefing:** Deputy logs in → reads statewide AI Briefing → notes lowest-performing program → reviews 4 stat tiles + County Overview.
- **County drill-down:** Deputy clicks Tuscola County row → County Drill-Down replaces table inline → reviews program performance + workers/supervisors counts; "← County Overview" returns.
- **Statewide Analytics:** Deputy switches tabs Program / Trend / Staffing — aggregated at state level — to find systemic issues; in Trend, toggles individual program lines.
- **Cases inspection:** Deputy opens Cases → County column visible → filters to one county → sorts by county → drills into a specific case to view its workflow / work items.
- **County-only tenant:** Admin keeps `stateManagement.enabled = false` → Deputy persona never appears; no users in that tenant see any Deputy affordance.

---

## 5. 📊 Success Conditions and Metrics

### Goals

1. Deputy persona renders only where `stateManagement.enabled = true`.
2. Read-only at every surface — no case-level or supervisory actions reachable.
3. County drill-down stays inline (single-active-modal contract).
4. Cases view exposes County column + filter only to Deputies; other personas unchanged.

### Non-Goals

- Case-level actions for the Deputy persona.
- Multi-state Deputy support — out of scope; one state per tenant per master + MOD-10.
- Deputy-side configuration of programs / KPI / users — those remain Admin (MOD-07).
- Work Items as a top-level nav for Deputies.

### Success Metrics

| Goal | Metric |
| --- | --- |
| Gating correctness | 0 instances of Deputy UI rendered when `stateManagement.enabled = false` |
| Read-only correctness | 0 case mutations attributable to a Deputy account |
| Drill-down stability | 0 cases of nested-modal stacking from the County Overview |
| Cases column visibility | County column / filter visible only when Deputy persona is active |

### Guardrails

- Deputy persona is fully invisible when flag is off.
- Read-only label is prominent on every Deputy surface.
- Cases / Case Detail visible to Deputy are read-only.
- No PHI in URLs (master).

---

## 6. 🤔 Assumptions

- Each case carries a `county` field populated at case creation. — ❓ unvalidated
- The Director recipient per county is resolvable for the Deputy view header. — ❓ unvalidated
- Statewide aggregations can be computed from per-county data without separate ETL in v1. — ❓ unvalidated
- Persona switcher is component-driven and can include / exclude personas dynamically. — ❓ unvalidated

---

## 7. 🔗 Dependencies and V-Model Gates

★

| Dependency | Type | Status | Notes |
| --- | --- | --- | --- |
| MOD-01 Workspace UI | Internal | DRAFT | Hosts Deputy nav + Dashboard + Cases. |
| MOD-05 Reporting & Analytics | Internal | DRAFT | Statewide Analytics extends Director Analytics. |
| MOD-07 Admin Configuration Suite | Internal | DRAFT | Hosts `stateManagement.enabled` flag. |
| MOD-06 AI Copilot | Internal | DRAFT | Statewide AI Briefing content. |
| Audit log | Shared | PENDING | Records persona enable / disable. |
| Dev Lead feasibility | V-Model gate | PENDING | ★ |
| Estimation | V-Model gate | PENDING | ★ |

---

## 8. 📋 Requirements

★

| Priority | Job Story | Acceptance Criteria (G/W/T) | Jira | Source |
| --- | --- | --- | --- | --- |
| P1 | As an Admin, when my tenant has state-level management, I want to enable the Deputy Director persona via Client Configuration so that the persona becomes available. | AC-1.1: *Given* Admin Client Configuration, *Then* a "State-Level Management" card is shown. AC-1.2: *Given* the toggle, *Then* it has Enabled/Disabled, defaulting to Disabled. AC-1.3: *Given* enabled, *Then* the card shows State name, state code, state agency name, Deputy Director name. AC-1.4: *Given* enabled, *Then* a blue info banner says "Enabling this feature adds the Deputy Director persona tab for [name] with read-only statewide oversight across all county deployments." AC-1.5: *Given* the flag is read at runtime, *Then* the Deputy persona is conditionally included in the PERSONAS array. AC-1.6: *Given* `stateManagement.enabled = false`, *Then* the Deputy persona is entirely absent from the persona switcher; no UI affordance hints at its existence. | [CAM-XXXXX] | DEP-001 |
| P1 | As a Deputy Director, when the persona is enabled for my tenant, I want a Deputy tab + dedicated sidebar so that I have a fit-for-purpose oversight surface. | AC-2.1: *Given* `stateManagement.enabled = true`, *Then* the Deputy tab appears between Director and Admin in the persona switcher. AC-2.2: *Given* the Deputy persona, *Then* my header shows the deputy name, "Deputy Director — State of [State]" title, and "Statewide" as program context. AC-2.3: *Given* the sidebar, *Then* it has Dashboard, Cases, Statewide Analytics — Work Items is not present. AC-2.4: *Given* Cases, *Then* it includes a County column and a County filter dropdown. AC-2.5: *Given* the page title, *Then* it updates per active view ("Dashboard" or "Statewide Analytics"). | [CAM-XXXXX] | DEP-002 |
| P1 | As a Deputy Director, when I land on Dashboard, I want a statewide AI Briefing + 4 stat tiles + County Overview + Statewide Program Summary so that I can triage at the state level. | AC-3.1: *Given* the Dashboard, *Then* an AI briefing card at the top provides a statewide narrative including overall on-time rate, total active cases, highest-risk county, best-performing county, lowest-performing program statewide. AC-3.2: *Given* stat tiles, *Then* they show Total Active Cases (sum across counties), Counties (count), Avg On-Time Rate (yellow if < 90 %), Total Breached (red if > 0). AC-3.3: *Given* the County Overview table, *Then* columns are County name, Director, Active Cases, At Risk, Breached, On-Time Rate (mini progress bar + %), Capacity (% with color-coded badge), Trend (month-over-month %). AC-3.4: *Given* the Statewide Program Summary table below, *Then* columns are Program, Total Cases, On-Time Rate, Target, vs Target (delta), Breached Statewide. AC-3.5: *Given* the County Overview, *Then* clicking a row navigates to the County Drill-Down (DEP-004). | [CAM-XXXXX] | DEP-003 |
| P1 | As a Deputy Director, when I drill into a county, I want an inline read-only view replacing the table so that I see county-level KPIs without losing context. | AC-4.1: *Given* I click a county row, *Then* the County Overview is replaced inline by the County Drill-Down; no modal or page navigation; a "← County Overview" breadcrumb returns. AC-4.2: *Given* the drill-down header, *Then* it shows county name, director name, active case count, KPI summary badges, and a yellow "👁 Read Only — Deputy View" badge prominently. AC-4.3: *Given* stat tiles, *Then* they show Active Cases, At Risk, Breached, Workers, Supervisors for the county. AC-4.4: *Given* the program performance table, *Then* columns are Program, Active Cases, On-Time Rate (mini progress bar), Breached, Status badge (On Target / Monitor / At Risk). AC-4.5: *Given* the bottom of the drill-down, *Then* a read-only notice states "Read-only view. Case-level actions are managed by [Director name] and their supervision team." | [CAM-XXXXX] | DEP-004 |
| P1 | As a Deputy Director, when I want analytics, I want a Statewide Analytics hub with the same three sub-views as Director Analytics aggregated at state level so that lenses are consistent. | AC-5.1: *Given* the hub, *Then* it uses a pill-style tab bar with three tabs (Program Performance, KPI Trend Analysis, Staffing & Capacity). AC-5.2: *Given* Program Performance, *Then* horizontal bars show statewide KPI % vs target with a red target line; clicking a program opens a drill-down with on-time rate broken down by county; "← All Programs" returns. AC-5.3: *Given* KPI Trend Analysis, *Then* a 6-month multi-line SVG chart with toggleable program lines (same interaction as Director Analytics), plus an overall statewide trend line chart, plus a current-period county comparison bar chart ranked by on-time rate. AC-5.4: *Given* Staffing & Capacity, *Then* four summary tiles (Total Workers Statewide, Avg Capacity Used, Counties Over Capacity, Total Active Cases), a county capacity utilization bar chart with 90 % alert threshold, and a statewide staffing detail table per program. AC-5.5: *Given* every Statewide Analytics surface, *Then* it is read-only — no actions. | [CAM-XXXXX] | DEP-005 |
| P1 | As a Deputy Director, when I review Cases, I want a County column + filter and persona-scoped visibility so that I can narrow by county; other personas remain unaffected. | AC-6.1: *Given* the Deputy persona is active, *Then* the Cases list table includes a "County" column inserted between Client and Program; county name is displayed as a green pill badge. AC-6.2: *Given* the toolbar, *Then* an "All Counties" filter dropdown is present allowing filter to a single county. AC-6.3: *Given* the County header, *Then* clicking sorts ascending alphabetically; clicking again sorts descending — consistent with the column-sort pattern across the product. AC-6.4: *Given* any other persona (Worker / Supervisor / Director / Admin), *Then* the County column and County filter are absent. AC-6.5: *Given* the data model, *Then* each case carries a county field populated at creation. AC-6.6: *Given* a Deputy needs to view work items for a case, *Then* they drill in from Cases — Work Items is not in the Deputy nav. | [CAM-XXXXX] | DEP-006 |

---

## 9. ⚙️ NFRs

★ Inherits master §9. Module deltas:

**Performance:** Dashboard load ≤ 3 s for typical state. Drill-down ≤ 500 ms. Cases query with County filter ≤ 1 s for typical state caseload.

**Security:** Deputy is read-only — no mutating endpoints reachable from this persona. Persona-level RBAC enforced server-side, not just UI gating.

**Reliability:** Statewide aggregations recompute live; no separate ETL v1.

**Observability:** Telemetry: deputy_dashboard_loaded, county_drill_down_opened, statewide_analytics_tab_changed, deputy_cases_filter_changed, persona_switched_to_deputy.

---

## 10. 🎨 User Interaction and Design

- **Gating** — flag-driven; persona invisible when off.
- **Read-only badge** — yellow "👁 Read Only — Deputy View" prominent on Dashboard County Detail header.
- **Single-modal contract** — county drill-down replaces table content inline.
- **Cases column visibility** — purely persona-driven; no Admin choice.
- **Tabs and charts** — reuse Director Analytics components for state aggregation.

---

## 11. 🔄 Key Flows

### Flow 1: Enable persona

1. Admin opens Client Configuration → State-Level Management card.
2. Toggles Enabled → enters state details + Deputy name.
3. Audit entry written.
**Exit:** Deputy persona appears in switcher.

### Flow 2: County drill-down

1. Deputy lands on Dashboard.
2. Reviews tiles + County Overview.
3. Clicks a county row → drill-down replaces table.
4. Reviews county KPIs / programs.
5. Clicks "← County Overview" → returns to table.

### Flow 3: Cases by county

1. Deputy opens Cases.
2. Sorts or filters by County.
3. Drills into a case → Case Detail (read-only); work items visible via case detail, not via top-nav Work Items.

---

## 12. 🚫 Out of Scope and Deferred

### Out of Scope

- Any case-level or supervisory action by Deputy.
- Multi-state binding for Deputy.
- Deputy-side administration of users / KPI / workflows.
- Deputy-only Work Items navigation surface.

### Deferred

- Multi-Deputy hierarchy (e.g., Deputy + Assistant Deputy).
- Deputy-side AI tools beyond statewide briefing (additional MOD-06 features for Deputy).
- Statewide notification subscriptions tailored to Deputy.

---

## 13. 🧬 Traceability

| AC ID | Story | Summary | Gherkin | Test class | Verdict |
| --- | --- | --- | --- | --- | --- |
| AC-1.6 | [CAM-XXXXX] | Deputy invisible when flag off | `docs/{FEATURE-ID}/gherkin/dep-001-gated.feature` | --- | --- |
| AC-2.3 | [CAM-XXXXX] | No Work Items in Deputy nav | `docs/{FEATURE-ID}/gherkin/dep-002-nav.feature` | --- | --- |
| AC-3.5 | [CAM-XXXXX] | County row click → drill-down | `docs/{FEATURE-ID}/gherkin/dep-003-rowclick.feature` | --- | --- |
| AC-4.5 | [CAM-XXXXX] | Read-only notice present | `docs/{FEATURE-ID}/gherkin/dep-004-readonly.feature` | --- | --- |
| AC-5.5 | [CAM-XXXXX] | Statewide Analytics no actions | `docs/{FEATURE-ID}/gherkin/dep-005-actions.feature` | --- | --- |
| AC-6.4 | [CAM-XXXXX] | County column hidden for non-Deputy | `docs/{FEATURE-ID}/gherkin/dep-006-column.feature` | --- | --- |

---

## 14. 🚀 Launch Plan

| Target | Milestone | Description | Exit |
| --- | --- | --- | --- |
| [YYYY-MM-DD] | ✅ UAT | Pilot state tenant — Deputy persona enabled | Read-only verified end-to-end; gating verified |
| [YYYY-MM-DD] | 🛑 Early Access | One state tenant in production | All sub-views operational |
| [YYYY-MM-DD] | 🛑 Launch | All state tenants | Adoption + safety metrics positive |

### Operation Checklist

- [ ] `stateManagement.enabled` flag wired
- [ ] Persona switcher renders Deputy conditionally
- [ ] Cases county field present on all records
- [ ] Director recipient per county resolvable
- [ ] Statewide Analytics aggregations validated
- [ ] Read-only enforced server-side

---

## 15. 📣 Cross-Functional Impact Checklist

| Team | Y/N | Action |
| --- | --- | --- |
| Configuration | Y | State-level config + Deputy seed. |
| Permissions | Y | RBAC for Deputy (read-only). |
| Reporting | Y | Statewide aggregations alignment with MOD-05. |
| Customer Success | Y | Deputy training + read-only positioning. |

---

## 16. ⚠️ Risks

| Risk | L/I | Mitigation |
| --- | --- | --- |
| Deputy persona leaks UI when flag off | L / H | Gating + tests; persona absent at server, not just hidden. |
| Deputy attempts case action via API | L / H | Server-side read-only enforcement, not UI-only. |
| County data inconsistent across tenants | M / M | County field mandatory at creation; data validation. |
| Aggregation performance under large states | M / M | Indexed county aggregations; load tests. |
| Deputy name resolution missing | M / M | Required field in DEP-001; validation on enable. |

---

## 17. ❓ Open Questions

| Question | Impact | Date | Owner |
| --- | --- | --- | --- |
| Confirm county field source (base Traverse case vs added by Northwoods). | Blocks AC-6.5. | 2026-04-25 | Dev Lead |
| Confirm whether Deputy can export Statewide Analytics. | Affects MOD-05 cross-cutting export contract. | 2026-04-25 | Product |
| Confirm Deputy-AI scope — is Briefing the only AI feature for Deputy at launch? | Affects MOD-06 contract. | 2026-04-25 | Product |
| Confirm whether Director per county is stored on the Director user record or elsewhere. | Blocks Dashboard county column "Director" rendering. | 2026-04-25 | Dev Lead |
| Confirm Deputy persona's interaction with notifications (does Deputy get any?). | Cross-module. | 2026-04-25 | Product |

---

## 18. 💬 FAQs

**Q:** Why no Work Items in Deputy nav?
**A:** Source DEP-002. Deputies are oversight; work items are inspected through the Case context, not as a top-level surface.

**Q:** Can a Deputy escalate a case?
**A:** No. Read-only persona; escalation remains with Supervisors (MOD-11).

**Q:** What if a tenant has no Deputy Director role?
**A:** Admin keeps `stateManagement.enabled = false`. The persona is invisible to that tenant.

---

## 19. 🧭 Decision Log

| # | Decision | Rationale | Date | By |
| --- | --- | --- | --- | --- |
| 1 | Per-tenant flag (`stateManagement.enabled`) gates the entire persona. | Source §29 — county-only deployments must not see it. | 2026-04-25 | Product |
| 2 | Deputy is strictly read-only at every surface. | Accountability remains with Directors / Supervisors. | 2026-04-25 | Product |
| 3 | Reuse Director Analytics tab + chart patterns at state aggregation. | Predictable UX; reduced build cost. | 2026-04-25 | Product |
| 4 | County column is persona-scoped on Cases; other personas unaffected. | Avoid surfacing county data where not relevant. | 2026-04-25 | Product |

---

## 20. 📝 Change Log

| Date | By | Sections | Reason |
| --- | --- | --- | --- |
| 2026-04-25 | Kendrew Peacey | All | Initial draft — derived from §29. |

---

## 21. 🧾 Informed By

★

| Document | Path | SHA-256 |
| --- | --- | --- |
| Master PRD | [[PRD - Northwoods Traverse Workspace]] | sha256:[at approval] |
| Source PRD | `00 - Inbox/Northwoods_Traverse_PRD_v2.0.docx` (§29) | sha256:[at approval] |

---

## 22. ✅ Approval and Checksum

★

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
