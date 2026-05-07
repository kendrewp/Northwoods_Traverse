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
# PRD — Traverse Compliance Framework (MOD-10)

> **V-Model–Aligned PRD.** Inherits from [[PRD - Northwoods Traverse Workspace]]. Source: §25.

---

## 1. Product Overview

| Field | Value |
| --- | --- |
| 📅 Target date | [QX YYYY — Phase 5 of master launch plan] |
| 🟡 Document status | Draft |
| 🧭 Team | PM / Product / Dev Lead / Dev / QA: [Names] |
| 🗃️ Work tracker (Jira epic) | [CAM-XXXXX] |
| 📎 Parent PRD | [[PRD - Northwoods Traverse Workspace]] |
| 🔗 Resources | `00 - Inbox/Northwoods_Traverse_PRD_v2.0.docx` (§25) |

---

## 2. 🎯 Objective and Problem Alignment

### Overview

Architecture + admin surface for managing **state-specific regulatory compliance** as data, not code. Each tenant binds to one U.S. state at onboarding; that binding drives the KPI Policy Engine. Federal mandates (CAPTA, FFPSA, Title IV-D, Title IV-A, OAA, ARP Act) are a separate layer — state rules must meet or exceed federal minimums. Includes a Compliance Library, Cross-State Comparison, Update Notification + Acknowledgment, and immutable Update History.

### Problem Statement

Northwoods serves agencies in multiple states (Ohio, North Carolina, New York, Louisiana, California, Minnesota, …) with distinct regulatory timeframes, threshold rules, and statutory citations. Today rules are coupled to code, so updates require a release. Without rule-as-data, releases bottleneck regulatory currency and audits cannot demonstrate clean date-of-change.

### Importance

- Rule-as-data ⇒ updates ship without code releases.
- Versioned + auditable ⇒ regulatory audits demonstrate "when did the system reflect rule X?".
- Federal vs state layering ⇒ the system enforces federal floors automatically.
- Cross-State Comparison ⇒ Northwoods staff and prospective clients can see state variation.
- Acknowledgment flow ⇒ Admin awareness before live impact.

### High-Level Approach

- **Tenant-state binding** at onboarding; cannot be changed without a formal re-onboarding review (operational binding lives in MOD-07 ADM-004).
- **Compliance Library** of all rules across states, organized by state × program × work item, with timeframes / yellow / red / citation / federal mandate / version / status.
- **Cross-State Comparison** — click a row → inline comparison panel.
- **Compliance Update Notification & Acknowledgment** — pending updates surface as banner; Admin reviews + acknowledges before effective date; existing cases keep prior version.
- **Federal layering** — rule records carry an optional Federal Mandate field; publishing a state rule below the federal minimum is blocked.
- **Update History** — immutable audit trail per tenant, exportable to PDF / Excel.

---

## 3. 👥 Stakeholders and Personas

- **P4 — Admin** — primary user of Compliance Library + acknowledgment flows.
- **Northwoods Compliance / Implementation team** — publishes new rule versions.
- **MOD-03 KPI Engine** — consumes the active ruleset to compute thresholds.
- **MOD-07 Admin Configuration Suite** — Compliance Library UX surface (read-only) + binding action live there.

---

## 4. 🎬 Target Use Cases

- **Onboarding:** Tenant onboards bound to Ohio → ruleset OH v3 active → Admin lands on Client Configuration → sees binding info, programs, version, effective date.
- **Browsing rules:** Admin opens Compliance Library → All states → filters to Child Welfare → sees per-state timeframes; clicks Initial Contact row → Cross-State Comparison expands.
- **Federal floor enforcement:** Northwoods staff publishes a NC rule with timeframe below the federal minimum (CAPTA) → publish blocked with clear error.
- **Update flow:** Northwoods publishes Ohio v4 → tenant Admin sees pending update banner with affected work item, before/after, effective date, affected case count, federal driver if any → Admin reviews modal → acknowledges → update logged.
- **Audit:** State auditor requests evidence of rule application timeline → Admin exports compliance update history to PDF.
- **Forgotten ack:** A pending update sits unacknowledged for 30 days past effective date → escalation alert to Admin.

---

## 5. 📊 Success Conditions and Metrics

### Goals

1. Every state regulation change ships as data — no code release.
2. Federal floors are technically enforced — no state rule below.
3. Admins explicitly acknowledge each tenant-impacting update before live.
4. Audit history is immutable and exportable.
5. Existing cases continue under the rule version active at case-open.

### Non-Goals

- Authoring of legal text by Admins — Northwoods authors; Admins consume.
- Per-tenant rule overrides of legal floors (downward) — forbidden by federal layering.
- Real-time legal-update ingestion from external sources.
- Multi-state binding for a single tenant — v1 is one state per tenant.

### Success Metrics

| Goal | Metric |
| --- | --- |
| Update lead time | Median days from rule change → tenant active — target ≤ 5 days |
| Federal-floor violations | 0 published state rules below federal minimum |
| Acknowledgment compliance | % of pending updates acknowledged ≤ 30 days from publish — target ≥ 95 % |
| Audit success | 100 % of audited tenants demonstrate clean update history on demand |

### Guardrails

- Existing cases use the ruleset version effective at case open.
- Acknowledgment is mandatory for tenant-impacting updates.
- Update history is immutable.
- Federal-floor check enforced on every publish.

---

## 6. 🤔 Assumptions

- One state binding per tenant is sufficient for v1. — ❓ unvalidated
- Federal mandate list (CAPTA, FFPSA, Title IV-D, Title IV-A, OAA, ARP Act) is the v1 set. — ❓ unvalidated
- Northwoods owns rule curation; agency Admins do not author rule content. — ❓ unvalidated
- MOD-03 can pin running cases to the ruleset version at case-open. — ❓ unvalidated

---

## 7. 🔗 Dependencies and V-Model Gates

★

| Dependency | Type | Status | Notes |
| --- | --- | --- | --- |
| MOD-03 KPI Engine | Internal | DRAFT | Consumes active ruleset; pins versions to running cases. |
| MOD-07 Admin Configuration Suite | Internal | DRAFT | Hosts Compliance Library UX (ADM-004 / ADM-005 / ADM-006); this PRD owns rule data + publishing semantics. |
| Audit log infrastructure | Shared | PENDING | Records every binding + publish + acknowledgment. |
| Dev Lead feasibility | V-Model gate | PENDING | ★ |
| Estimation | V-Model gate | PENDING | ★ |

---

## 8. 📋 Requirements

★

| Priority | Job Story | Acceptance Criteria (G/W/T) | Jira | Source |
| --- | --- | --- | --- | --- |
| P1 | As an Admin, when I open Client Configuration, I want a visible tenant state binding with version + effective info so that I always know what compliance posture is active. | AC-1.1: *Given* the Admin Client Configuration screen, *Then* it shows current state binding, active ruleset version, licensed programs, effective date, last reviewed date, admin contact. AC-1.2: *Given* the binding, *Then* it cannot be changed without a formal re-onboarding review (per MOD-07 ADM-004 binding-change flow). AC-1.3: *Given* the active ruleset version, *Then* it is prominently displayed at all times in the Admin interface. AC-1.4: *Given* persona switch to Admin, *Then* it lands on Client Configuration by default. | [CAM-XXXXX] | COMP-001 |
| P1 | As an Admin / Northwoods compliance staff, when I need to view all rules, I want a Compliance Library across states / programs / work items so that I can audit + reference. | AC-2.1: *Given* the Admin sidebar, *Then* the Compliance Library is accessible. AC-2.2: *Given* state tabs, *Then* I can filter by state (All, OH, NC, NY, LA, CA, MN, …). AC-2.3: *Given* a program filter, *Then* rules narrow further. AC-2.4: *Given* each rule row, *Then* it shows work item, program, timeframe, yellow threshold, red threshold, citation, federal mandate indicator, version. AC-2.5: *Given* federal-mandate rules, *Then* they are visually distinguished from state-specific. AC-2.6: *Given* "Publish Rule Update", *Then* Northwoods admins can push a new rule version. AC-2.7: *Given* the active tenant state, *Then* it is highlighted in the state tab bar. | [CAM-XXXXX] | COMP-002 |
| P2 | As an Admin / staff, when I want to understand state variation, I want a Cross-State Comparison so that I can quickly compare timeframes and citations. | AC-3.1: *Given* a clicked rule row, *Then* a comparison panel appears inline below the table. AC-3.2: *Given* each state, *Then* a card shows state flag, name, timeframe, yellow threshold, citation, federal mandate (if applicable). AC-3.3: *Given* the tenant's state, *Then* it is highlighted as "Your State". AC-3.4: *Given* the same row clicked again or close, *Then* the panel dismisses. AC-3.5: *Given* the panel, *Then* it explicitly notes federal mandates set floors all states must meet or exceed. | [CAM-XXXXX] | COMP-003 |
| P1 | As a Northwoods admin / agency Admin, when a new compliance rule version is published, I want a notification + acknowledgment flow so that effects are explicit and auditable. | AC-4.1: *Given* a pending update for the tenant's state, *Then* it appears as a warning banner on Client Configuration. AC-4.2: *Given* a pending update, *Then* it shows affected work item, program, current value, new value, effective date, affected active case count, federal driver if any, publish date. AC-4.3: *Given* "Review", *Then* a modal shows full change detail + before/after comparison + impact warning. AC-4.4: *Given* an acknowledgment checkbox, *Then* the "Acknowledge & Schedule Update" button enables only when checked. AC-4.5: *Given* acknowledgment, *Then* the update is logged to compliance update history with date, applied-by user, version, summary. AC-4.6: *Given* cases opened before the effective date, *Then* they continue under the prior ruleset version; cases opened on/after use the new rules. AC-4.7: *Given* an unacknowledged update past 30 days from effective date, *Then* an escalation alert is sent to Admin. | [CAM-XXXXX] | COMP-004 |
| P1 | As a system, when publishing a state rule, I want federal-floor enforcement so that no state rule falls below the federal minimum. | AC-5.1: *Given* a rule record, *Then* an optional "Federal Mandate" field identifies the governing statute / program. AC-5.2: *Given* a federal-mandate rule, *Then* it has a distinct visual indicator (⚖) in both Library table and Cross-State Comparison. AC-5.3: *Given* a state rule publish with timeframe below the federal minimum, *Then* publish is prevented until resolved. AC-5.4: *Given* the acknowledgment modal, *Then* it identifies whether a change is federally driven. | [CAM-XXXXX] | COMP-005 |
| P1 | As an Admin / compliance officer, when audited, I want a complete, immutable Compliance Update History so that I can demonstrate when each rule applied. | AC-6.1: *Given* Client Configuration, *Then* a history table is visible below the pending updates. AC-6.2: *Given* each row, *Then* it shows version, applied date, applied by, rules changed count, and plain-language summary. AC-6.3: *Given* the history, *Then* entries are immutable — cannot be edited or deleted. AC-6.4: *Given* an export action, *Then* the history is exportable to PDF or Excel. AC-6.5: *Given* an audit, *Then* the history demonstrates the system was updated on specific dates. | [CAM-XXXXX] | COMP-006 |

---

## 9. ⚙️ NFRs

★ Inherits master §9. Module deltas:

**Performance:** Library renders ≤ 2 s for the full multi-state ruleset. Cross-State Comparison opens ≤ 200 ms.

**Reliability:** Publish operations are atomic per state-version; partial publishes prohibited.

**Security:** Read-only rule content for Admins; write only via Northwoods compliance staff (RBAC at API layer).

**Scale:** Designed for ≤ 50 states × 5 programs × ~50 work items = ~12 500 active rules without performance loss.

**Observability:** Telemetry: rule_published, rule_publish_blocked_federal_floor, pending_update_shown, update_acknowledged, update_overdue_alert, library_filter_applied, comparison_opened, history_exported.

---

## 10. 🎨 User Interaction and Design

- **Compliance Library UX** mirrors Work Item Library UX (column sorting, filter-as-you-type) — handled in MOD-07 ADM-005.
- **Federal mandate** indicator (⚖) appears in both Library table and Cross-State Comparison.
- **Active Tenant** badge in Library and "Your State" highlight in Comparison.
- **Pending Update banner** uses warning treatment + Review CTA.
- **Acknowledgment modal** — explicit checkbox; cannot proceed without it.
- **History table** — read-only, exportable.

---

## 11. 🔄 Key Flows

### Flow 1: Tenant onboarding (binding)

1. Northwoods System Admin onboards tenant.
2. Selects state binding → reviews ruleset → confirms binding.
3. Tenant Admin sees Client Configuration with binding info on next login.
**Exit:** KPI engine consumes the bound state's ruleset.

### Flow 2: Rule update → tenant acknowledgment

1. Northwoods compliance staff publishes Ohio v4 (federal floor check passes).
2. Tenant pending-update banner appears on Client Configuration.
3. Admin clicks Review → modal shows before/after, affected count, federal driver if any.
4. Admin checks acknowledgment → button enables → click "Acknowledge & Schedule Update".
5. History entry recorded; effective on the publish date going forward.
**Exit:** Cases opened on/after effective date use new rules; existing pin to prior.

### Flow 3: Federal floor block

1. Northwoods staff drafts NC rule for Initial Contact at 60 days; CAPTA federal minimum is 30 days (example).
2. Publish attempt → system blocks with error citing the federal mandate.
3. Staff revises to ≥ federal minimum → publish succeeds.
**Exit:** No floor violation reaches production.

---

## 12. 🚫 Out of Scope and Deferred

### Out of Scope

- Authoring of legal rule text by agency Admins.
- Multi-state binding per tenant.
- Real-time external regulatory feed ingestion.
- Per-tenant relaxation below the federal floor.

### Deferred

- Per-program calendar overrides (relates to MOD-07 ADM-007 deferred item).
- AI-assisted rule diff summarisation in update notifications.
- Self-service tenant request to change state binding (beyond formal re-onboarding).

---

## 13. 🧬 Traceability

| AC ID | Story | Summary | Gherkin | Test class | Verdict |
| --- | --- | --- | --- | --- | --- |
| AC-1.4 | [CAM-XXXXX] | Admin lands on Client Configuration | `docs/{FEATURE-ID}/gherkin/comp-001-landing.feature` | --- | --- |
| AC-2.7 | [CAM-XXXXX] | Active tenant highlighted | `docs/{FEATURE-ID}/gherkin/comp-002-active-tenant.feature` | --- | --- |
| AC-4.6 | [CAM-XXXXX] | Existing cases pin to prior version | `docs/{FEATURE-ID}/gherkin/comp-004-pinning.feature` | --- | --- |
| AC-4.7 | [CAM-XXXXX] | 30-day overdue escalation | `docs/{FEATURE-ID}/gherkin/comp-004-escalation.feature` | --- | --- |
| AC-5.3 | [CAM-XXXXX] | Federal floor block | `docs/{FEATURE-ID}/gherkin/comp-005-floor.feature` | --- | --- |
| AC-6.3 | [CAM-XXXXX] | History immutable | `docs/{FEATURE-ID}/gherkin/comp-006-immutable.feature` | --- | --- |

---

## 14. 🚀 Launch Plan

| Target | Milestone | Description | Exit |
| --- | --- | --- | --- |
| [YYYY-MM-DD] | ✅ UAT | Ohio reference + 1 secondary state seeded | Federal-floor + pinning verified |
| [YYYY-MM-DD] | 🛑 Early Access | Pilot tenant with one regulatory update cycle | Update flow verified end-to-end |
| [YYYY-MM-DD] | 🛑 Launch | All target states seeded | Acknowledgment compliance metric trending positive |

### Operation Checklist

- [ ] Reference rulesets seeded for OH (ODJFS) + at least one other state
- [ ] Federal mandate library defined (CAPTA, FFPSA, Title IV-D, Title IV-A, OAA, ARP Act)
- [ ] Northwoods publish flow + roles defined
- [ ] Admin notification banner + acknowledgment modal wired
- [ ] History export tested (PDF + Excel)
- [ ] 30-day overdue escalation wired

---

## 15. 📣 Cross-Functional Impact Checklist

| Team | Y/N | Action |
| --- | --- | --- |
| Compliance | Y | Rule curation + publish workflow. |
| Configuration | Y | Onboarding seeds state binding. |
| Customer Success | Y | Acknowledgment training. |
| Permissions | Y | Read-only Admin vs Northwoods publish role. |

---

## 16. ⚠️ Risks

| Risk | L/I | Mitigation |
| --- | --- | --- |
| Rule misconfigured below federal floor leaks | L / H | Floor check enforced on publish + tests. |
| Admin acknowledgment skipped under pressure | M / H | 30-day overdue escalation; visible banner persistent. |
| Pinning of in-flight cases fails → retroactive change | L / H | MOD-03 contract pins version at case-open; integration tests. |
| Rule version drift between Library and KPI engine | M / H | Single source-of-truth for rules; engine reads via shared contract. |
| Tenant-binding change skipped formal review | L / H | Binding-change flow lives in MOD-07 ADM-004 with acknowledgment + audit. |

---

## 17. ❓ Open Questions

| Question | Impact | Date | Owner |
| --- | --- | --- | --- |
| Confirm v1 state list (OH, NC, NY, LA, CA, MN) and seed content owner. | Blocks COMP-002 launch. | 2026-04-25 | Compliance |
| Confirm federal mandate enumeration + per-mandate floor source. | Blocks AC-5.3. | 2026-04-25 | Compliance |
| Confirm whether tenants can self-acknowledge updates or require Northwoods sign-off. | Affects AC-4.x. | 2026-04-25 | Product |
| Confirm export schema for PDF + Excel. | Blocks AC-6.4. | 2026-04-25 | Dev Lead |
| Confirm whether multi-state binding is on the v2 roadmap. | Affects scope wording. | 2026-04-25 | Product |

---

## 18. 💬 FAQs

**Q:** Why is rule content read-only for Admins?
**A:** Legal text + federal layering must be authored by Northwoods compliance staff to prevent agency-side drift below regulatory minimums.

**Q:** What happens to cases mid-flight when a new rule is published?
**A:** They continue under the rule version active at case open. Only new cases use the new rules (AC-4.6).

**Q:** Can a tenant disable the acknowledgment flow?
**A:** No — acknowledgment is mandatory for tenant-impacting updates.

---

## 19. 🧭 Decision Log

| # | Decision | Rationale | Date | By |
| --- | --- | --- | --- | --- |
| 1 | Rules-as-data, not code. | Updates without releases. | 2026-04-25 | Product |
| 2 | Federal-floor enforcement on publish. | Compliance integrity. | 2026-04-25 | Product |
| 3 | Existing cases pinned to ruleset version at case open. | No retroactive surprises. | 2026-04-25 | Product |
| 4 | One state binding per tenant in v1. | Avoid multi-binding complexity until proven needed. | 2026-04-25 | Product |
| 5 | Compliance Library UX hosted in MOD-07; data + semantics owned here. | Clear ownership boundary. | 2026-04-25 | Product |

---

## 20. 📝 Change Log

| Date | By | Sections | Reason |
| --- | --- | --- | --- |
| 2026-04-25 | Kendrew Peacey | All | Initial draft — derived from §25. |

---

## 21. 🧾 Informed By

★

| Document | Path | SHA-256 |
| --- | --- | --- |
| Master PRD | [[PRD - Northwoods Traverse Workspace]] | sha256:[at approval] |
| Source PRD | `00 - Inbox/Northwoods_Traverse_PRD_v2.0.docx` (§25) | sha256:[at approval] |

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
