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
# PRD — Traverse Reporting and Analytics (MOD-05)

> **V-Model–Aligned PRD.** Inherits from [[PRD - Northwoods Traverse Workspace]]. Source: §12 (Reporting & Analytics) + §26 (Persona Reporting Surfaces).

---

## 1. Product Overview

| Field | Value |
| --- | --- |
| 📅 Target date | [QX YYYY — Phase 3 of master launch plan] |
| 🟡 Document status | Draft |
| 🧭 Team | PM / Product / Dev Lead / Dev / QA: [Names] |
| 🗃️ Work tracker (Jira epic) | [CAM-XXXXX] |
| 📎 Parent PRD | [[PRD - Northwoods Traverse Workspace]] |
| 🔗 Resources | `00 - Inbox/Northwoods_Traverse_PRD_v2.0.docx` (§§12, 26) · [[PRD - Traverse KPI SLA Policy Engine]] |

---

## 2. 🎯 Objective and Problem Alignment

### Overview

Persona-scoped reporting surfaces — **My Performance** (Worker), **Team Reports** (Supervisor), **Analytics Hub** (Director, three sub-views: Program Performance, KPI Trend Analysis, Staffing & Capacity) — plus controlled CSV/PDF export, scheduled distribution, and a versioned metric dictionary so KPI math is consistent everywhere.

### Problem Statement

Today there is no aggregated, real-time view of program-level KPI compliance. Supervisors and Directors run manual reports. Workers have no clear view of their own performance trend. Without persona-scoped, live, masking-aware reporting, leadership cannot manage by exception and Workers cannot self-manage.

### Importance

- Each persona gets a fit-for-purpose reporting surface as a top-level navigation item.
- Live data — derived from MOD-02 work items + MOD-03 KPI history; no separate data warehouse for v1.
- Versioned metric dictionary makes KPI calculations transparent and consistent across surfaces.
- Controlled export + scheduling lets compliance and leadership consume reports outside the app without leaking PHI.

### High-Level Approach

- **Worker — My Performance:** stat tiles, KPI status donut, 6-month monthly completion bars, upcoming-deadlines table.
- **Supervisor — Team Reports:** stat tiles, per-worker on-track horizontal bar chart, 6-month team trend line, worker detail table.
- **Director — Analytics Hub:** pill-tab hub with three sub-views (Program Performance, KPI Trend Analysis, Staffing & Capacity).
- **Export:** CSV + PDF with masking; admin-restricted; audited.
- **Scheduled reports:** Admin-managed recipient lists; respects quiet hours and persona scope.
- **Metric dictionary:** versioned definitions (formula, filters, source, format) used across surfaces.

---

## 3. 👥 Stakeholders and Personas

- **P1 — Social Worker** — My Performance.
- **P2 — Supervisor** — Team Reports + Supervisor Dashboard pack.
- **P3 — Director** — Analytics Hub + Cross-Program Dashboard.
- **P4 — Admin** — manages metric dictionary, scheduled reports, export restrictions, dashboard configuration via MOD-07.
- **P5 — QA / Compliance (placeholder)** — read-only reporting + export within masking.

---

## 4. 🎬 Target Use Cases

- **Worker self-check:** Worker opens My Performance → sees Active / Completed / On-Track / Breached tiles, KPI donut, 6-month monthly completions, upcoming-7-day deadlines.
- **Supervisor weekly review:** Supervisor opens Team Reports → identifies a worker with on-track < 80 % via the horizontal bar chart → drills in (cross-link to MOD-11 Supervisor case actions).
- **Director monthly:** Director opens Analytics → Program Performance tab → spots a program below target → drills into Program Detail → reviews 6-month trend.
- **Director KPI investigation:** Director opens KPI Trend Analysis → toggles only the two underperforming programs → reads the multi-line chart against the 90 % target line.
- **Director staffing:** Director opens Staffing & Capacity → sees one program at 92 % capacity → reviews staffing detail table.
- **Compliance export:** QA persona pulls a CSV for state audit; export is masked, audited, and signed off.
- **Scheduled distribution:** Admin sets weekly Director rollup PDF to a state-leadership email list; quiet hours respected.

---

## 5. 📊 Success Conditions and Metrics

### Goals

1. Each persona has a top-level reporting surface aligned to their accountability level.
2. KPI math is consistent across cockpit, reporting, AI, and exports — driven by the metric dictionary + MOD-03.
3. Exports + scheduled reports respect persona scope and masking by default.
4. Reports load on live data (no overnight ETL) for the v1 footprint.

### Non-Goals

- Standalone data-warehouse / BI platform — v1 reads live records.
- Pivot / drag-drop ad-hoc analytics — persona-scoped fixed views only.
- Custom report builder for end users — Admin-managed only.
- AI-generated narratives (delegated to MOD-06 — Program Health Narrative).

### Success Metrics

| Goal | Metric |
| --- | --- |
| Adoption | % of Supervisors using Team Reports weekly within 30 days — target ≥ 70 % |
| Data freshness | Reporting surfaces use data ≤ 60 s stale (master) |
| Export safety | 0 PHI-leak findings in audited exports |
| KPI consistency | Reporting on-track % matches MOD-03 explainability payload (audit sample) |

### Guardrails

- All exports are audited — actor, timestamp, scope, persona.
- Scheduled report recipient lists are Admin-managed; users cannot add arbitrary recipients.
- Metric definitions are versioned; changes are auditable.
- Director surfaces show no individual client data.

---

## 6. 🤔 Assumptions

- MOD-03 exposes KPI history sufficient to power 6-month trends. — ❓ unvalidated
- Capacity signal (Staffing & Capacity sub-view) is derivable from active-work-item counts + Admin-configured capacity per worker. — ❓ unvalidated
- Email distribution for scheduled reports uses the same provider as MOD-04. — ❓ unvalidated
- v1 reporting performance acceptable on live records (no pre-aggregation). — ❓ unvalidated

---

## 7. 🔗 Dependencies and V-Model Gates

★

| Dependency | Type | Status | Notes |
| --- | --- | --- | --- |
| MOD-03 KPI Engine | Internal | DRAFT | Hard blocker — KPI history + payloads. |
| MOD-02 Workflow Engine | Internal | DRAFT | Hard blocker — work-item completion data. |
| MOD-07 Admin Suite | Internal | DRAFT | Owns metric dictionary editor, scheduled-report scheduler, export restriction surface. |
| MOD-04 Notifications | Internal | DRAFT | Soft — shares email provider for scheduled reports. |
| Audit log | Shared | PENDING | Records every export and every dictionary change. |
| Dev Lead feasibility | V-Model gate | PENDING | ★ |
| Estimation | V-Model gate | PENDING | ★ |

---

## 8. 📋 Requirements

★

| Priority | Job Story | Acceptance Criteria (G/W/T) | Jira | Source |
| --- | --- | --- | --- | --- |
| P1 | As a Social Worker, when I review my own performance, I want a private personal KPI summary so that I can self-manage my caseload. | AC-1.1: *Given* the personal view, *Then* it is private unless explicitly shared by policy. AC-1.2: *Given* the time-window selector, *Then* I can choose 7 / 30 / 90 days. AC-1.3: *Given* on-time rate, *Then* it is computed from completed work items within their KPI window using the metric dictionary. | [CAM-XXXXX] | FR-RPT-001 |
| P0 | As a Supervisor, when I land on Supervisor Dashboard, I want a standard pack with team backlog, at-risk counts, breach counts, workload by worker, aging distribution, and trends so that I can manage by exception. | AC-2.1: *Given* a widget, *Then* I can drill into underlying work items. AC-2.2: *Given* my data scope, *Then* the dashboard respects team / program limits. AC-2.3: *Given* filters, *Then* I can filter by Program Type, Case Type, individual Social Worker. | [CAM-XXXXX] | FR-RPT-010 |
| P0 | As a Director, when I land on the Director dashboard, I want read-only KPI compliance by Program with breach counts, supervisor breakdowns, and trend lines so that I can identify systemic issues. | AC-3.1: *Given* the dashboard, *Then* it is read-only — no task actions. AC-3.2: *Given* a Program Type card, *Then* clicking opens supervisor-level breakdown. AC-3.3: *Given* trends, *Then* the dashboard shows current vs prior period. AC-3.4: *Given* time window, *Then* I can change it. | [CAM-XXXXX] | FR-RPT-020 |
| P0 | As any persona, when I export a report, I want CSV / PDF with masking and audit so that compliance is preserved. | AC-4.1: *Given* an export, *Then* an audit entry records actor, scope, format, timestamp. AC-4.2: *Given* Admin restriction, *Then* sensitive dashboards or programs may be export-disabled. AC-4.3: *Given* an export, *Then* field-level masking is applied per the exporting user's persona. | [CAM-XXXXX] | FR-RPT-030 |
| P1 | As an Admin / authorised user, when leadership needs regular distribution, I want scheduled reports with privacy / quiet-hours respect and admin-managed recipients so that the right people get the right data. | AC-5.1: *Given* a scheduled job, *Then* it respects quiet hours and privacy constraints. AC-5.2: *Given* a recipient list, *Then* it is Admin-managed; users cannot add arbitrary recipients. AC-5.3: *Given* a recipient, *Then* report content respects their persona-based scope. | [CAM-XXXXX] | FR-RPT-031 |
| P0 | As an Admin, when defining metrics, I want a versioned metric dictionary so that KPI math is explicit, consistent, and auditable. | AC-6.1: *Given* a metric, *Then* it stores name, definition, formula, filters, data source, display format. AC-6.2: *Given* a dictionary change, *Then* it is versioned and audit-logged. AC-6.3: *Given* a surface that consumes a metric, *Then* it references a specific version and resolves consistently. | [CAM-XXXXX] | FR-RPT-040 |
| P1 | As a Social Worker, when I open the My Performance view, I want stat tiles, KPI donut, 6-month monthly completions, and upcoming-7-day deadlines so that the surface is glanceable. | AC-7.1: *Given* the sidebar, *Then* a "My Performance" item with a chart icon links to this view. AC-7.2: *Given* the view, *Then* four stat tiles render: Active Work Items, Completed This Period (current month), On Track or Better %, Breached (red if > 0). AC-7.3: *Given* the donut, *Then* it shows on-track-or-better % with a legend (On Track, At Risk, Critical, Breached, Completed). AC-7.4: *Given* the bar chart, *Then* it shows 6 months of completed counts. AC-7.5: *Given* upcoming deadlines, *Then* it lists items due within 7 days sorted ascending by days remaining with Work Item, Case ID, Program, Due In ("Today" for 0-day), KPI badge. AC-7.6: *Given* zero items in 7 days, *Then* "No deadlines in the next 7 days" is shown. | [CAM-XXXXX] | RPT-001 |
| P1 | As a Supervisor, when I open Team Reports, I want stat tiles, per-worker on-track horizontal bars, 6-month team trend, and a worker detail table so that I can identify outliers fast. | AC-8.1: *Given* the sidebar, *Then* a "Team Reports" item links to the view. AC-8.2: *Given* stat tiles, *Then* they show Total Active Cases, Avg On-Track Rate (yellow if < 90 %), Total Breached (red if > 0), Completed This Period. AC-8.3: *Given* the per-worker bar chart, *Then* one bar per worker, descending by on-track %, with a vertical red 90 % target line; bars green ≥ 90 %, yellow 80–89 %, red < 80 %; bar shows worker, %, active count, breached count. AC-8.4: *Given* the trend, *Then* a 6-month line chart plots overall agency on-track % with the 90 % target line. AC-8.5: *Given* the detail table, *Then* it lists Worker, Active Cases, On-Track %, Breached, Completed, Status badge (On Track / Monitor / At Risk by threshold) with alternating rows. | [CAM-XXXXX] | RPT-002 |
| P1 | As a Director, when I open Analytics, I want a tabbed hub with Program Performance / KPI Trend / Staffing & Capacity so that all leadership lenses are in one place. | AC-9.1: *Given* the sidebar, *Then* an "Analytics" item links to the hub. AC-9.2: *Given* the hub, *Then* a pill-style tab bar shows three tabs; the active tab uses white background + blue text. AC-9.3: *Given* tab switching, *Then* only the content below the tab bar changes — no page navigation. AC-9.4: *Given* first load, *Then* Program Performance is the default. | [CAM-XXXXX] | RPT-003 |
| P1 | As a Director, when I open Program Performance, I want horizontal bars + per-program donuts + program drill-down so that I can spot under-target programs and investigate. | AC-10.1: *Given* the bar chart, *Then* each row shows program, current KPI %, target %, vertical red target line, with sub-text case count, worker count, breached count. AC-10.2: *Given* I click a bar (or its donut), *Then* a Program Detail drill-down replaces the bar chart inside the same sub-view. AC-10.3: *Given* the drill-down, *Then* it shows a "← All Programs" back link, a 3×2 grid of stat cards (Active Cases, Workers Assigned, Current KPI %, Target KPI %, Breached, vs. Target delta), and a 6-month trend line. AC-10.4: *Given* per-program donut tiles below the bar chart, *Then* each shows program name, current KPI %, case count; clicking opens the drill-down. AC-10.5: *Given* a donut, *Then* arc is green at/above target or yellow below. | [CAM-XXXXX] | RPT-004 |
| P1 | As a Director, when I open KPI Trend Analysis, I want a multi-line chart with toggleable program lines + an overall agency trend so that I can compare programs over time. | AC-11.1: *Given* the chart, *Then* one line per program (5 lines for 5 programs), each with a distinct color. AC-11.2: *Given* toggle buttons above the chart, *Then* I can show / hide individual program lines; active toggles have filled background in program color, inactive have white background + colored border. AC-11.3: *Given* the target, *Then* a dashed red horizontal line at 90 % is drawn. AC-11.4: *Given* a data point, *Then* it shows a dot and the KPI % label; month labels appear on x-axis. AC-11.5: *Given* the agency trend card, *Then* it shows a single line for combined agency on-track % over 6 months with the 90 % target line, dots, and labels. | [CAM-XXXXX] | RPT-005 |
| P1 | As a Director, when I open Staffing & Capacity, I want capacity utilisation per program with the 90 % alert threshold so that I can plan staffing. | AC-12.1: *Given* stat tiles, *Then* they show Total Workers, Avg Capacity Used %, Avg Cases/Worker, Over Capacity (red if > 0). AC-12.2: *Given* the bar chart, *Then* capacity per program with vertical red 90 % alert line; bars green < 75 %, yellow 75–89 %, red ≥ 90 %; bar sub-text shows worker count, avg cases/worker, max cases/worker. AC-12.3: *Given* the staffing detail table, *Then* it lists Program, Workers, Avg Cases/Worker, Max Cases/Worker, Capacity Used %, and an Alert column with "⚠ Over" badge for ≥ 90 % programs and a green checkmark for others; alternating row backgrounds. | [CAM-XXXXX] | RPT-006 |

---

## 9. ⚙️ NFRs

★ Inherits master §9. Module deltas:

**Performance:** Reporting surface initial load ≤ 3 s for typical persona scope. Drill-down ≤ 1 s. Chart rendering ≤ 200 ms after data resolution.

**Security:** Field-level masking on every report and export. RBAC enforced at query layer.

**Scale:** Live-record queries acceptable for v1 footprint. Pre-aggregation deferred — flag if performance degrades at large tenants.

**Observability:** Telemetry: report_view_loaded, drill_down_opened, export_generated, scheduled_report_emitted, dictionary_change_published.

---

## 10. 🎨 User Interaction and Design

- **Top-level nav:** "My Performance" (Worker), "Team Reports" (Supervisor), "Analytics" (Director).
- **Tabs / drill-down pattern:** matches MOD-01 inline-state-swap convention — drill-down replaces content, not nested modals.
- **Charts:** SVG donut, bar, line. Use accessibility-compliant color palette + label/legend on every chart.
- **Empty states:** "No deadlines", "All clear", "No data yet" wherever a chart would be empty.

---

## 11. 🔄 Key Flows

### Flow 1: Director monthly review

**Trigger:** Director opens Analytics.
**Steps:**
1. Default Program Performance loads.
2. Director clicks under-target program → drill-down replaces content.
3. Director switches to KPI Trend Analysis tab → multi-line chart loads.
4. Director toggles two underperforming programs → only those lines visible.
5. Director switches to Staffing & Capacity → spots a program at 92 %.
**Exit:** Director has identified bottleneck.

### Flow 2: Scheduled export

**Trigger:** Admin schedules a weekly Director rollup PDF.
**Steps:**
1. Scheduler runs at configured time.
2. Engine computes report for each recipient at their persona scope.
3. PDF rendered with masking applied.
4. Email delivered via shared provider; quiet hours respected.
5. Audit entries written.
**Exit:** Recipients receive masked report.

---

## 12. 🚫 Out of Scope and Deferred

### Out of Scope

- Ad-hoc / pivot / custom report builder.
- Standalone data warehouse.
- Embedded BI tooling.
- AI-generated narrative summaries (MOD-06).

### Deferred

- Pre-aggregation / OLAP cube — only if v1 live queries underperform.
- Custom dashboards (per-persona / per-program) beyond persona defaults.
- API for third-party reporting consumers.

---

## 13. 🧬 Traceability

| AC ID | Story | Summary | Gherkin | Test class | Verdict |
| --- | --- | --- | --- | --- | --- |
| AC-2.2 | [CAM-XXXXX] | Supervisor scope respected | `docs/{FEATURE-ID}/gherkin/rpt-010-supervisor-scope.feature` | --- | --- |
| AC-3.1 | [CAM-XXXXX] | Director read-only | `docs/{FEATURE-ID}/gherkin/rpt-020-director-readonly.feature` | --- | --- |
| AC-4.3 | [CAM-XXXXX] | Export masking | `docs/{FEATURE-ID}/gherkin/rpt-030-export-mask.feature` | --- | --- |
| AC-6.2 | [CAM-XXXXX] | Dictionary versioning | `docs/{FEATURE-ID}/gherkin/rpt-040-dictionary.feature` | --- | --- |
| AC-10.2 | [CAM-XXXXX] | Program drill-down inline | `docs/{FEATURE-ID}/gherkin/rpt-004-drilldown.feature` | --- | --- |
| AC-11.2 | [CAM-XXXXX] | KPI Trend toggles | `docs/{FEATURE-ID}/gherkin/rpt-005-toggles.feature` | --- | --- |

---

## 14. 🚀 Launch Plan

| Target | Milestone | Description | Exit |
| --- | --- | --- | --- |
| [YYYY-MM-DD] | ✅ UAT | Persona pilots | No P0/P1 chart / data defects |
| [YYYY-MM-DD] | 🛑 Early Access | Pilot tenant | Weekly Supervisor adoption ≥ 50 % |
| [YYYY-MM-DD] | 🛑 Launch | All tenants | Master metrics positive |

### Operation Checklist

- [ ] Metric dictionary seeded
- [ ] Scheduled email provider integrated
- [ ] Export audit visibility tested
- [ ] Persona-scope tests passing

---

## 15. 📣 Cross-Functional Impact Checklist

| Team | Y/N | Action |
| --- | --- | --- |
| Analytics | Y | Telemetry. |
| Configuration | Y | Metric dictionary + scheduled report seeding. |
| Permissions | Y | Export + dashboard RBAC. |
| Reporting (Camis internal) | Y | New persona surfaces. |

---

## 16. ⚠️ Risks

| Risk | L/I | Mitigation |
| --- | --- | --- |
| Live-query performance under large tenants | M / H | Telemetry + per-tenant load testing; pre-aggregation as a deferred fallback. |
| KPI math drift across surfaces | M / H | Single metric dictionary + MOD-03 explainability payload. |
| PHI in exports | L / H | Default-on masking; export disable-by-program switch. |
| Director sees individual case data accidentally | L / H | Director surfaces are aggregate-only by design + tested. |

---

## 17. ❓ Open Questions

| Question | Impact | Date | Owner |
| --- | --- | --- | --- |
| Confirm "capacity" data source (active-WI count, KPI-load, configured capacity, mix?). | Blocks RPT-006. | 2026-04-25 | Product |
| Confirm whether Worker self-share of My Performance is in v1. | Affects AC-1.1. | 2026-04-25 | Product |
| Confirm trend window default — 6 months hard-coded vs configurable? | Affects RPT-001/002/004/005. | 2026-04-25 | Product |
| Confirm scheduled-report failure / retry policy. | Affects AC-5.x. | 2026-04-25 | SRE |
| Confirm export PDF templating engine. | Blocks AC-4.x. | 2026-04-25 | Dev Lead |

---

## 18. 💬 FAQs

**Q:** Is there a separate data warehouse?
**A:** No — v1 reads live work-item + KPI history records. Pre-aggregation is a deferred mitigation if performance demands.

**Q:** Why are Worker views private?
**A:** Self-management without comparative pressure. Sharing is policy-driven only.

**Q:** Where do AI-generated narratives live?
**A:** Director Program Health Narrative is in MOD-06 (FR-AI-007), not here.

---

## 19. 🧭 Decision Log

| # | Decision | Rationale | Date | By |
| --- | --- | --- | --- | --- |
| 1 | Live-record reporting in v1; no warehouse. | Reduces v1 infrastructure scope; data freshness ≤ 60 s. | 2026-04-25 | Product |
| 2 | Director surfaces are aggregate-only, no individual case data. | Persona definition + privacy guardrails. | 2026-04-25 | Product |
| 3 | Worker view is private by default. | Self-management posture. | 2026-04-25 | Product |

---

## 20. 📝 Change Log

| Date | By | Sections | Reason |
| --- | --- | --- | --- |
| 2026-04-25 | Kendrew Peacey | All | Initial draft — derived from §§12 + 26. |

---

## 21. 🧾 Informed By

★

| Document | Path | SHA-256 |
| --- | --- | --- |
| Master PRD | [[PRD - Northwoods Traverse Workspace]] | sha256:[at approval] |
| Source PRD | `00 - Inbox/Northwoods_Traverse_PRD_v2.0.docx` (§§12, 26) | sha256:[at approval] |

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
