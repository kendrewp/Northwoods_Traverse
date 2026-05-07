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
# PRD — Traverse Supervisor Case Actions (MOD-11)

> **V-Model–Aligned PRD.** Inherits from [[PRD - Northwoods Traverse Workspace]]. Source: §27 (Supervisor Worker Drill-Down & Case Actions) + §28 (Grove Managed Services Case Transfer).

---

## 1. Product Overview

| Field | Value |
| --- | --- |
| 📅 Target date | [QX YYYY — Phase 6 of master launch plan] |
| 🟡 Document status | Draft |
| 🧭 Team | PM / Product / Dev Lead / Dev / QA: [Names] |
| 🗃️ Work tracker (Jira epic) | [CAM-XXXXX] |
| 📎 Parent PRD | [[PRD - Northwoods Traverse Workspace]] |
| 🔗 Resources | `00 - Inbox/Northwoods_Traverse_PRD_v2.0.docx` (§§27, 28) |

---

## 2. 🎯 Objective and Problem Alignment

### Overview

Supervisor capabilities to drill from Team Workload → an individual worker's case list → take inline structured case actions: **Reassign** (to another worker in the same program), **Escalate** (with reason / priority / compliance record), and (where contracted) **Transfer to Grove Managed Services Team** as a tenant-flag-gated external option in the Reassign panel.

### Problem Statement

Today supervisors discover a worker is at risk in the Team Workload View but have no inline path to act — they must navigate elsewhere, lose context, or rely on email. There is no structured Escalate path (reason / priority / compliance record). Tenants with managed-services contracts cannot transfer cases to Grove without an out-of-band process. Without inline structured actions, supervisor intervention is slow, inconsistent, and unaudited.

### Importance

- Inline state-swap (no nested modal) keeps supervisors in flow.
- Structured Reason / Priority on escalations turns ad-hoc nudges into auditable compliance records.
- Per-tenant managed-services flag respects contract reality — invisible to non-contracted tenants.
- Post-transfer visibility preserves supervisor accountability without giving false action affordances.

### High-Level Approach

- **Drill-down** — clicking a worker in Team Workload replaces table inline with worker case list (no modal). Breadcrumb returns to team view.
- **Reassign** — inline panel below the case row, blue left border; lists program peers under capacity; optional Reason; success banner.
- **Escalate** — inline panel below the case row, red left border; required Reason + Priority + optional Notes; notification preview shows recipient Director and program; success banner.
- **Mutual exclusivity** — opening Reassign closes Escalate and vice versa.
- **Grove transfer (gated)** — when `managedServices.enabled = true`, the Reassign panel adds an "or transfer externally" divider + Grove card with acknowledgment + "Transfer to Grove" CTA; logs and notifies Northwoods.
- **Post-transfer visibility** — case stays in supervisor drill-down list as read-only with "🤝 Managed by Grove" badge; Reassign / Escalate hidden; banner confirms transfer.

---

## 3. 👥 Stakeholders and Personas

- **P2 — Supervisor** — primary user.
- **P3 — Director** — recipient of escalations.
- **P4 — Admin** — toggles `managedServices.enabled` per tenant.
- **Northwoods Grove Managed Services Team** — recipient of transfer notifications.
- **MOD-01 Workspace UI** — hosts Team Workload View + drill-down container.
- **MOD-02 Workflow Engine** — receives reassignment + transfer events.
- **MOD-04 Notifications** — delivers escalation + transfer notifications.

---

## 4. 🎬 Target Use Cases

- **Worker drill-down:** Supervisor sees a worker trending red → clicks worker row → case list replaces team table inline → KPI summary badges visible in header.
- **Reassign:** Supervisor opens a case row → Reassign panel expands → selects an Available peer → adds Reason → Confirm → success banner; case in target worker's caseload.
- **Escalate:** Supervisor opens a case row → Escalate panel expands → picks "Safety Concern", Priority "Urgent", adds Notes → Submit → notification to Director Lisa Park; compliance record created; success banner.
- **Grove transfer (contracted tenant):** Supervisor opens Reassign panel → scrolls past internal workers → selects Grove card → checks acknowledgment → "Transfer to Grove" → notification email sent; case shows "🤝 Managed by Grove" with Read Only badge.
- **Hidden by flag:** Same supervisor on a non-contracted tenant → no Grove card or divider rendered at all.

---

## 5. 📊 Success Conditions and Metrics

### Goals

1. Supervisors can act on at-risk cases without leaving the dashboard.
2. Escalations always carry a structured Reason + Priority and produce an auditable compliance record.
3. Grove transfer is invisible to non-contracted tenants and explicit (acknowledgment + audit) where contracted.
4. Post-transfer accountability is preserved without misleading action affordances.

### Non-Goals

- Bulk reassignment beyond row-level Reassign / "Reassign All" on the workload row (already exists).
- Director-side acceptance / triage of escalations (lives in Director surfaces in MOD-05 / MOD-12).
- Editing Grove transfers after submission.
- Inline KPI override (lives in MOD-03 + MOD-01 Supervisor surface).

### Success Metrics

| Goal | Metric |
| --- | --- |
| Inline action adoption | % of weekly Supervisor sessions using Reassign or Escalate inline — target ≥ 70 % |
| Escalation completeness | 100 % of escalations carry Reason + Priority |
| Grove gating | 0 instances of Grove UI rendered when `managedServices.enabled = false` |
| Audit completeness | 100 % of reassignments + escalations + transfers have full audit entries |

### Guardrails

- Mutual exclusivity between Reassign and Escalate panels per row.
- Reassign target must be in the same program AND under maximum caseload.
- Grove transfer requires the tenant flag enabled, the Northwoods card selected, AND the acknowledgment checkbox.
- Transferred cases hide Reassign / Escalate buttons.

---

## 6. 🤔 Assumptions

- Capacity per program is configurable; v1 reference threshold = 17. — ❓ unvalidated
- A Director recipient is resolvable per program for escalation notifications. — ❓ unvalidated
- Grove notification email address is configured per tenant in MST-001. — ❓ unvalidated
- Compliance record schema for escalations aligns with MOD-04 audit storage. — ❓ unvalidated

---

## 7. 🔗 Dependencies and V-Model Gates

★

| Dependency | Type | Status | Notes |
| --- | --- | --- | --- |
| MOD-01 Workspace UI | Internal | DRAFT | Hosts Team Workload View + inline drill-down. |
| MOD-02 Workflow Engine | Internal | DRAFT | Hard blocker for reassignment effects. |
| MOD-04 Notifications | Internal | DRAFT | Hard blocker for escalation + transfer notifications. |
| MOD-07 Admin Configuration Suite | Internal | DRAFT | Hosts `managedServices.enabled` toggle (MST-001). |
| Audit log | Shared | PENDING | Records reassign / escalate / transfer. |
| Dev Lead feasibility | V-Model gate | PENDING | ★ |
| Estimation | V-Model gate | PENDING | ★ |

---

## 8. 📋 Requirements

★

| Priority | Job Story | Acceptance Criteria (G/W/T) | Jira | Source |
| --- | --- | --- | --- | --- |
| P1 | As a Supervisor, when I see a worker on the Team Workload View, I want to drill inline into their cases without navigating away. | AC-1.1: *Given* the Team Workload table, *Then* each row is clickable (cursor pointer) and shows a "Click to view cases →" sub-label under the worker's name. AC-1.2: *Given* I click a worker row, *Then* the workload table is replaced inline by the worker case detail view — no modal or page navigation. AC-1.3: *Given* the case detail header, *Then* it shows a "← Team Workload" breadcrumb returning to the full table; the worker's avatar, name, program, and active case count; and KPI summary badges (Breached, At Risk, On-Time Rate). AC-1.4: *Given* the cases table, *Then* columns are Case ID, Client, Program/Case Type, Current Work Item, Due (with due label), KPI status badge, Actions. AC-1.5: *Given* the workload table row, *Then* worker-level "Reassign All" and "Escalate" remain available as bulk/shortcut actions. | [CAM-XXXXX] | SUP-001 |
| P1 | As a Supervisor, when I want to reassign a case, I want an inline panel below the case row with eligible peers + reason so that I rebalance fast. | AC-2.1: *Given* a case row, *Then* a "⇄ Reassign" button is present. AC-2.2: *Given* I click it, *Then* an inline panel expands below the row with a blue left border — no modal overlay. AC-2.3: *Given* the panel, *Then* it shows the case ID + client name for context; an "Assign To" section listing program peers under capacity (< maximum caseload) with name, current case count, on-time rate, and capacity badge (Available / Near Capacity); plus an optional Reason / Note field. AC-2.4: *Given* worker presentation, *Then* radio button cards with the selected card highlighted (blue border + background). AC-2.5: *Given* "Confirm Reassignment", *Then* it is disabled until a target is selected. AC-2.6: *Given* confirmation, *Then* the panel closes and a success banner identifies case ID + new owner. AC-2.7: *Given* no eligible workers in the same program, *Then* a "No available workers in [program] with capacity" message is shown. AC-2.8: *Given* the Reassign button clicked again or Cancel, *Then* the panel closes without saving. | [CAM-XXXXX] | SUP-002 |
| P1 | As a Supervisor, when a case warrants Director attention, I want a structured inline Escalate form that creates a compliance record so that escalations are auditable. | AC-3.1: *Given* a case row, *Then* an "↑ Escalate" button is present. AC-3.2: *Given* I click it, *Then* an inline panel expands below the row with a red left border. AC-3.3: *Given* the form, *Then* fields are: required Escalation Reason dropdown (Safety Concern, Policy / Compliance Breach, Capacity / Workload, Court Deadline at Risk, Family Crisis, Other); required Priority toggle ("Urgent" 🚨 / "Standard" 📋, default Standard); optional Notes. AC-3.4: *Given* a notification preview panel, *Then* it states which Director and program will be notified, e.g., "Escalation will notify: Director Lisa Park and create a compliance record." AC-3.5: *Given* "Submit Escalation", *Then* it is disabled until a Reason is selected. AC-3.6: *Given* submission, *Then* the panel closes and a red confirmation banner identifies case ID + reason. AC-3.7: *Given* Cancel, *Then* the panel closes without saving. AC-3.8: *Given* Reassign and Escalate panels per row, *Then* opening one closes the other. | [CAM-XXXXX] | SUP-003 |
| P1 | As a system, when populating Team Workload + drill-down data, I want a realistic worker × cases dataset so that the surface is meaningful in prototype + production. | AC-4.1: *Given* WORKERS, *Then* each is associated with a non-empty WORKER_CASES set distributed across G/Y/R/Breached. AC-4.2: *Given* each case, *Then* it stores Case ID, Client name, Program, Case Type, Opened date, KPI status, Current Work Item, Due date, Due label. AC-4.3: *Given* eligibility filter for reassignment targets, *Then* it returns workers in the same program, not the current owner, with caseload < maximum (17 reference). AC-4.4: *Given* production, *Then* eligibility consumes actual assignments + configurable capacity per program. | [CAM-XXXXX] | SUP-004 |
| P1 | As an Admin, when my tenant has a managed-services contract, I want to enable Grove Managed Services in Client Configuration so that supervisors can transfer cases. | AC-5.1: *Given* Client Configuration, *Then* a "Grove Managed Services" card appears below the Tenant Card. AC-5.2: *Given* the toggle, *Then* it has Enabled / Disabled states; default = disabled; enable allowed only with a valid managed-services agreement. AC-5.3: *Given* enabled, *Then* the card expands to show Contract Reference, Contract Start, Contract Renewal, supported programs (all eligible), and the Northwoods notification email. AC-5.4: *Given* state, *Then* it is stored as `managedServices.enabled` on tenant config and read at runtime by Supervisor Dashboard. AC-5.5: *Given* enable / disable, *Then* the action is logged. | [CAM-XXXXX] | MST-001 |
| P1 | As a Supervisor on a contracted tenant, when I open the Reassign panel, I want a Grove transfer option visually distinct from internal workers so that external transfer is explicit. | AC-6.1: *Given* `managedServices.enabled = true`, *Then* the Reassign panel renders a horizontal divider labeled "or transfer externally" below the internal worker list with a single radio card representing Grove. AC-6.2: *Given* the Grove card, *Then* it shows a 🤝 icon, "Grove Managed Services Team" label, contract reference number tag, and sub-label "Case will be transferred to Northwoods. You retain read-only visibility and accountability." AC-6.3: *Given* styling, *Then* the Grove card is visually distinct from internal worker cards. AC-6.4: *Given* eligibility, *Then* all programs and case types are eligible; eligibility at work-item level is supervisor judgment — no programmatic block. AC-6.5: *Given* `managedServices.enabled = false`, *Then* the divider and the Grove card are not rendered. | [CAM-XXXXX] | MST-002 |
| P1 | As a Supervisor, when I select Grove transfer, I want explicit acknowledgment + a distinct confirm CTA so that external transfer is intentional. | AC-7.1: *Given* the Grove radio card is selected, *Then* an acknowledgment checkbox appears below it: "I confirm this case is eligible for external transfer under contract [contract reference] and understand that Northwoods will be notified at [northwoods email]. This action is logged." AC-7.2: *Given* selection, *Then* the confirm button label changes to "Transfer to Grove" (vs "Confirm Reassignment" for internal). AC-7.3: *Given* the confirm button, *Then* it remains disabled until both the Grove option is selected AND the acknowledgment is checked. AC-7.4: *Given* confirmation, *Then* a notification is sent to the Grove email address; the action is logged with supervisor identity, timestamp, case ID, and contract reference. | [CAM-XXXXX] | MST-003 |
| P1 | As a Supervisor, after Grove transfer, I want post-transfer visibility with read-only state + clear "Managed by Grove" indicator so that I retain accountability without misleading affordances. | AC-8.1: *Given* a transferred case, *Then* it remains in the worker's case list — not removed. AC-8.2: *Given* the row, *Then* a "🤝 Managed by Grove" badge appears inline with the case ID. AC-8.3: *Given* the row background, *Then* it is differentiated (light blue tint). AC-8.4: *Given* the KPI column, *Then* it displays a "Read Only" badge in place of the KPI status chip. AC-8.5: *Given* the due label, *Then* it is replaced with a dash (—); the current work item is shown muted/italic. AC-8.6: *Given* the Actions column, *Then* it shows "No actions available" in muted text; Reassign + Escalate buttons are hidden. AC-8.7: *Given* a successful transfer, *Then* a blue dismissible banner displays "🤝 [Case ID] — Transferred to Grove Managed Services Team". | [CAM-XXXXX] | MST-004 |

---

## 9. ⚙️ NFRs

★ Inherits master §9. Module deltas:

**Performance:** Drill-down render ≤ 500 ms after click. Inline panel open ≤ 150 ms. Confirm action ≤ 1 s end-to-end (audit + notification dispatch).

**Security:** Audit on every reassignment / escalation / transfer. Escalation creates a compliance record. Grove transfer is fully audited (supervisor, timestamp, case ID, contract reference).

**Reliability:** Inline state-swap is non-destructive — Cancel + close paths never cause partial saves.

**Observability:** Telemetry: worker_drill_down_opened, reassign_panel_opened, reassign_confirmed, escalate_panel_opened, escalate_submitted, grove_transfer_confirmed.

---

## 10. 🎨 User Interaction and Design

- **Inline state-swap** — workload table ↔ worker case detail in same content area.
- **Inline panels** — Reassign (blue left border) and Escalate (red left border) below the row.
- **Mutual exclusivity** — opening one closes the other for the same row.
- **Grove card** — distinct styling within the Reassign panel; only rendered when feature flag is on.
- **Success / error banners** — at top of case list — green for reassignment, red for escalation, blue for Grove transfer; dismissible.

---

## 11. 🔄 Key Flows

### Flow 1: Drill-down → reassign

1. Supervisor on Team Workload → clicks worker row.
2. Inline replace with worker case detail view (header + cases table).
3. On a case row → "⇄ Reassign" → inline panel expands.
4. Supervisor selects peer → adds optional Reason → Confirm.
5. Audit + MOD-02 reassignment effect; banner shown.

### Flow 2: Drill-down → escalate

1. Same drill-down state.
2. Click "↑ Escalate" → inline form expands; Reassign panel auto-closes if open.
3. Reason + Priority + optional Notes → notification preview shows Director.
4. Submit → audit + compliance record + MOD-04 notification.
5. Confirmation banner (red).

### Flow 3: Grove transfer (gated)

1. Admin enables Managed Services with valid contract → `managedServices.enabled = true` (MST-001).
2. Supervisor opens Reassign on a case → divider + Grove card render below internal workers.
3. Supervisor selects Grove → acknowledgment checkbox appears → confirms.
4. Notification email to Grove; audit; case row swaps to read-only with badges.
5. Banner.

---

## 12. 🚫 Out of Scope and Deferred

### Out of Scope

- Director-side escalation triage UI (see MOD-05 / MOD-12 surfaces).
- Editing or revoking a Grove transfer.
- Bulk multi-case escalation / transfer.
- Cross-program reassignment.

### Deferred

- AI candidate-suggestion ranking inside Reassign (MOD-06 FR-AI-006 surfaced here).
- SLA on Director-side escalation acknowledgment.
- Two-way Grove status updates synchronised back into supervisor view.

---

## 13. 🧬 Traceability

| AC ID | Story | Summary | Gherkin | Test class | Verdict |
| --- | --- | --- | --- | --- | --- |
| AC-1.2 | [CAM-XXXXX] | Inline drill-down (no modal) | `docs/{FEATURE-ID}/gherkin/sup-001-drill.feature` | --- | --- |
| AC-2.7 | [CAM-XXXXX] | Empty-eligible message | `docs/{FEATURE-ID}/gherkin/sup-002-empty.feature` | --- | --- |
| AC-3.5 | [CAM-XXXXX] | Submit disabled until Reason | `docs/{FEATURE-ID}/gherkin/sup-003-required.feature` | --- | --- |
| AC-3.8 | [CAM-XXXXX] | Mutual exclusivity | `docs/{FEATURE-ID}/gherkin/sup-003-exclusive.feature` | --- | --- |
| AC-6.5 | [CAM-XXXXX] | Grove hidden when flag off | `docs/{FEATURE-ID}/gherkin/mst-002-hidden.feature` | --- | --- |
| AC-7.3 | [CAM-XXXXX] | Confirm gated by ack | `docs/{FEATURE-ID}/gherkin/mst-003-ack.feature` | --- | --- |
| AC-8.6 | [CAM-XXXXX] | Post-transfer no actions | `docs/{FEATURE-ID}/gherkin/mst-004-readonly.feature` | --- | --- |

---

## 14. 🚀 Launch Plan

| Target | Milestone | Description | Exit |
| --- | --- | --- | --- |
| [YYYY-MM-DD] | ✅ UAT | Supervisor pilot — Reassign + Escalate first | No P0/P1 mutual-exclusivity defects |
| [YYYY-MM-DD] | 🛑 Early Access | Pilot tenant — Grove flag enabled | End-to-end transfer + post-transfer visibility verified |
| [YYYY-MM-DD] | 🛑 Launch | All tenants — Grove gated per contract | Adoption metric trending positive |

### Operation Checklist

- [ ] Capacity threshold per program defined
- [ ] Director recipient resolution per program verified (escalation routing)
- [ ] Grove notification email + contract reference seeded for contracted tenants
- [ ] Audit + telemetry wired
- [ ] Banners + Reassign/Escalate panels accessible (keyboard + screen reader)

---

## 15. 📣 Cross-Functional Impact Checklist

| Team | Y/N | Action |
| --- | --- | --- |
| Configuration | Y | Per-tenant Grove contract config. |
| Permissions | Y | Supervisor scope verified. |
| Customer Success | Y | Escalation Reason taxonomy training. |
| Legal | Y | Grove transfer acknowledgment language sign-off. |
| Analytics | Y | Telemetry. |

---

## 16. ⚠️ Risks

| Risk | L/I | Mitigation |
| --- | --- | --- |
| Supervisor accidentally Grove-transfers | L / H | Acknowledgment + distinct CTA + audit. |
| Reassignment to over-capacity worker | L / M | Eligibility filter + capacity badge. |
| Escalation reasons drift / inconsistent | M / M | Closed dropdown + required field. |
| Grove UI leaks on non-contracted tenant | L / H | Render gate based on `managedServices.enabled`; tests. |
| Director recipient resolution fails | M / M | Pre-resolution preview shown in form (AC-3.4). |

---

## 17. ❓ Open Questions

| Question | Impact | Date | Owner |
| --- | --- | --- | --- |
| Confirm capacity threshold per program (v1 reference 17). | Blocks AC-2.3 + AC-4.3. | 2026-04-25 | Product |
| Confirm Director recipient resolution model (single Director per program vs list). | Blocks AC-3.4. | 2026-04-25 | Product |
| Confirm Grove notification email + contract reference data model. | Blocks MST-001 / MST-003. | 2026-04-25 | Product + Legal |
| Confirm whether transferred cases retain link to original Supervisor for accountability reporting in MOD-05. | Cross-module reporting. | 2026-04-25 | Product |
| Confirm whether supervisors may revoke / reverse a Grove transfer. | Affects v1 deferred scope. | 2026-04-25 | Product |

---

## 18. 💬 FAQs

**Q:** Why mutual exclusivity between Reassign + Escalate?
**A:** Source SUP-003 #8 — single-action focus per case row reduces accidental dual mutations.

**Q:** Why is Grove transfer feature-flagged?
**A:** Not all tenants have managed-services contracts. Flag-off renders nothing — clean separation.

**Q:** Can a Supervisor reassign across programs?
**A:** No — same-program peers only (AC-2.3 + AC-4.3).

---

## 19. 🧭 Decision Log

| # | Decision | Rationale | Date | By |
| --- | --- | --- | --- | --- |
| 1 | Inline state-swap pattern, not modal. | Source §27 — keeps supervisor in flow. | 2026-04-25 | Product |
| 2 | Same-program reassignment only. | Avoids cross-program data exposure / capacity weirdness. | 2026-04-25 | Product |
| 3 | Grove gated by `managedServices.enabled`; invisible when off. | Source §28; protects non-contracted tenants. | 2026-04-25 | Product |
| 4 | Post-transfer visibility = read-only with badges (no removal). | Preserves accountability without action affordance. | 2026-04-25 | Product |

---

## 20. 📝 Change Log

| Date | By | Sections | Reason |
| --- | --- | --- | --- |
| 2026-04-25 | Kendrew Peacey | All | Initial draft — derived from §§27, 28. |

---

## 21. 🧾 Informed By

★

| Document | Path | SHA-256 |
| --- | --- | --- |
| Master PRD | [[PRD - Northwoods Traverse Workspace]] | sha256:[at approval] |
| Source PRD | `00 - Inbox/Northwoods_Traverse_PRD_v2.0.docx` (§§27, 28) | sha256:[at approval] |

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
