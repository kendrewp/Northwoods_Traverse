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
# PRD — Traverse Notifications and Escalations (MOD-04)

> **V-Model–Aligned PRD.** Inherits from [[PRD - Northwoods Traverse Workspace]]. Source: §11.

---

## 1. Product Overview

| Field | Value |
| --- | --- |
| 📅 Target date | [QX YYYY — Phase 2 of master launch plan] |
| 🟡 Document status | Draft |
| 🧭 Team | PM / Product / Dev Lead / Dev / QA: [Names] |
| 🗃️ Work tracker (Jira epic) | [CAM-XXXXX] |
| 📎 Parent PRD | [[PRD - Northwoods Traverse Workspace]] |
| 🔗 Resources | [[Northwoods-Traverse]] · `00 - Inbox/Northwoods_Traverse_PRD_v2.0.docx` (§11) · [[PRD - Traverse KPI SLA Policy Engine]] |

---

## 2. 🎯 Objective and Problem Alignment

### Overview

Build the notification + escalation layer that turns KPI threshold-crossing and operational events into calibrated, deduped, multi-channel notifications routed up the organisational hierarchy: Worker at Yellow → Supervisor at Red → Director at Breach.

### Problem Statement

Today notifications are noisy or absent — teams either miss issues or suffer alert fatigue. There is no formal escalation ladder, so issues sit with workers until KPIs miss, then erupt without warning. Without a notification layer that is permission-aware, deduped, calibrated to quiet hours, and tied to the org hierarchy, the rest of the system's signal is lost.

### Importance

- Calibrated alerts mean issues surface *before* breach.
- Three-tier escalation aligns notifications to accountability (master §3 hierarchy).
- Dedupe + quiet hours + digest = signal without fatigue.
- Auditable per work item ("who was notified and when") for compliance.

### High-Level Approach

- **Triggers** from MOD-03 (threshold crossing) and MOD-02 (assignment, reassignment, secondary trigger / completion / primary resumption).
- **Three-tier escalation ladder** configurable per Program × Case Type, respecting the org hierarchy (FR-NOTIF-010).
- **Dedupe + throttle** — one alert per state transition; configurable reminder cadence for Breach.
- **Channels** — in-app + email baseline; SMS / Teams / Slack optional. PHI/PII minimisation in outbound content.
- **Quiet hours + digests** per persona / program with critical-Breach bypass.
- **Audit visibility** on the work item — every notification recorded.

---

## 3. 👥 Stakeholders and Personas

- **P1 — Social Worker** — Yellow notifications + Unread Alerts tile (MOD-01 TILE-004).
- **P2 — Supervisor** — Red notifications + escalation to oversight.
- **P3 — Director** — Breach notifications.
- **P4 — Admin** — configures triggers, escalation ladder, channels, quiet hours, dedupe.
- **MOD-03** — emits threshold events.
- **MOD-02** — emits operational events.
- **MOD-01** — renders in-app notifications + Unread Alerts tile.

---

## 4. 🎬 Target Use Cases

- **Yellow alert:** Work item crosses Yellow → Worker gets one in-app + email alert.
- **Red escalation:** Work item enters Red → Supervisor added; Worker reminded per cadence.
- **Breach:** Work item misses due_at → Director added; daily reminders until resolved.
- **Operational events:** New case assigned → Worker notified; Supervisor secondary-trigger → Worker notified primary paused or continuing; Primary resumes → Worker + Supervisor notified.
- **Quiet hours:** A Yellow event fires at 02:00 → suppressed and digested; a Breach fires at 02:00 → bypasses quiet hours per Admin config.
- **Resolution notification (optional):** A Yellow item gets an override and returns to Green — Admin opts to send a resolution notification.

---

## 5. 📊 Success Conditions and Metrics

### Goals

1. KPI threshold crossings produce exactly one alert per transition (no duplicates).
2. Escalation routes correctly up the org hierarchy.
3. Off-hours quiet behaves as configured; Breach bypass works.
4. Workers + Supervisors + Directors *trust* the alerts (signal-to-noise improves measurably).

### Non-Goals

- KPI computation (MOD-03).
- Workflow execution (MOD-02).
- Long-form messaging or chat — this is event notification, not collaboration.
- Per-user notification template authoring — Admin-managed only.

### Success Metrics

| Goal | Metric |
| --- | --- |
| No duplicate alerts | < 0.1 % duplicate-alert rate per state transition |
| Calibration | Reduction in user-reported "noise" complaints by [target] within 90 days |
| Escalation correctness | 100 % of Breach events have Director recorded in audit log |
| Quiet-hour respect | 0 non-Breach alerts delivered outside quiet hours (audit sample) |
| Trust | Telemetry: % of in-app alerts opened within 1 hour during business hours [target TBD] |

### Guardrails

- No PHI in email subject lines (master §15.3 SEC-021).
- Outbound content masking respects Admin restriction.
- Critical Breach may bypass quiet hours only if Admin explicitly configures.
- Resolution notifications opt-in only.

---

## 6. 🤔 Assumptions

- MOD-03 emits a `threshold_crossed` event per transition with payload (work item, case, program, due_at, policy id, KPI status, recipient scope). — ❓ unvalidated
- MOD-02 emits operational events (assigned, reassigned, secondary triggered/completed, primary resumed). — ❓ unvalidated
- Org hierarchy (Worker → Supervisor → Director) is queryable via base Traverse user model. — ❓ unvalidated
- Email + in-app are the launch channels; SMS / Teams / Slack are deferred. — ❓ unvalidated

---

## 7. 🔗 Dependencies and V-Model Gates

★

| Dependency | Type | Status | Notes |
| --- | --- | --- | --- |
| MOD-03 KPI Engine | Internal | DRAFT | Hard blocker — emits threshold events. |
| MOD-02 Workflow Engine | Internal | DRAFT | Hard blocker — operational events. |
| MOD-07 Admin Suite | Internal | DRAFT | Authoring of triggers / channels / ladder / quiet hours. |
| MOD-01 Workspace UI | Internal | DRAFT | Renders in-app notifications + Unread Alerts tile. |
| Email service | External | PENDING | Provider TBD. |
| SMS / Teams / Slack | External | DEFERRED | Optional. |
| Audit log infrastructure | Shared | PENDING | Records every notification. |
| Dev Lead feasibility | V-Model gate | PENDING | ★ |
| Estimation Checkpoint 1 | V-Model gate | PENDING | ★ |

---

## 8. 📋 Requirements

★

| Priority | Job Story | Acceptance Criteria (G/W/T) | Jira | Source |
| --- | --- | --- | --- | --- |
| P0 | As a system, when a KPI crosses Yellow / Red / Breached, I want to emit a notification event so that downstream channels can deliver the alert. | AC-1.1: *Given* a state transition, *Then* exactly one event is generated per transition (no repeated alerts for the same state unless a new threshold is crossed). AC-1.2: *Given* the event, *Then* it includes work item ID, title, case name, program type, due_at, policy id, KPI status, and recipient scope. AC-1.3: *Given* a return to a safer state (e.g., after a KPI override), *Then* a resolution notification may be generated if Admin enables it. | [CAM-XXXXX] | FR-NOTIF-001 |
| P1 | As an Admin, when configuring operational triggers, I want to enable / disable assignment, reassignment, secondary-trigger, secondary-complete, and primary-resumed notifications per persona and program so that channels stay calibrated. | AC-2.1: *Given* an Admin toggles an operational trigger off for a persona / program, *Then* no notification is sent for that event in scope. AC-2.2: *Given* the trigger is on, *Then* the appropriate persona is notified. | [CAM-XXXXX] | FR-NOTIF-002 |
| P0 | As an Admin, when defining escalation, I want a three-tier ladder (Worker @ Yellow, Supervisor @ Red, Director @ Breach) configurable per Program × Case Type so that escalations align to the org hierarchy. | AC-3.1: *Given* a configured ladder, *Then* Yellow notifies the assigned Worker. AC-3.2: *Given* Red, *Then* the assigned Supervisor is added. AC-3.3: *Given* Breach, *Then* the Director is added. AC-3.4: *Given* an event, *Then* the work item shows who was notified and when (audit visibility). AC-3.5: *Given* Breach, *Then* Admin may configure additional reminder cadences (e.g., daily) until resolved. | [CAM-XXXXX] | FR-NOTIF-010 |
| P0 | As a system, when generating notifications, I want dedupe + throttling so that workers are not spammed for the same state. | AC-4.1: *Given* a transition, *Then* exactly one alert per (work item, state) is delivered to a given recipient. AC-4.2: *Given* Breach reminders, *Then* cadence is Admin-configurable; AC-4.3: *Given* a reminder window, *Then* additional alerts in the same state collapse. | [CAM-XXXXX] | FR-NOTIF-011 |
| P0 | As any persona, when alerts are delivered, I want in-app + email channels with masking so that I get the signal without leaking PHI. | AC-5.1: *Given* a notification, *Then* in-app delivery surfaces in the Unread Alerts tile (TILE-004) and persists as unread until dismissed. AC-5.2: *Given* email, *Then* subject lines do not contain PHI/PII by default. AC-5.3: *Given* outbound content, *Then* Admin masking rules are applied. AC-5.4: *Given* Admin enables additional channels (SMS / Teams / Slack), *Then* delivery follows the same masking rules. | [CAM-XXXXX] | FR-NOTIF-020 |
| P1 | As an Admin / user, when off-hours, I want quiet hours + digest notifications so that off-hours disruption is minimised; Critical Breach may bypass if Admin configures. | AC-6.1: *Given* quiet hours configured per persona / program, *Then* non-critical alerts are suppressed and digested. AC-6.2: *Given* the digest cadence, *Then* digests deliver at the configured time. AC-6.3: *Given* Admin opts in, *Then* Breach bypasses quiet hours. AC-6.4: *Given* a user, *Then* they can adjust preferences within Admin-defined bounds. | [CAM-XXXXX] | FR-NOTIF-021 |

---

## 9. ⚙️ NFRs

★ Inherits master §9. Module deltas:

**Performance:** Event → in-app delivery ≤ 5 s (P95). Event → email delivery ≤ 60 s (P95). Digest run completes within configured window.

**Reliability:** Idempotent event consumption — duplicate events from MOD-03 do not cause duplicate alerts. Backlog visible to Admin via monitoring dashboard.

**Security:** PHI/PII masking at the outbound boundary. Email subject sanitisation by default. Channel-restriction by program for sensitive content.

**Scalability:** Sustained event throughput sized to expected concurrent users + KPI churn.

**Observability:** Telemetry: notification_emitted, notification_delivered (by channel), notification_dismissed, digest_emitted, escalation_added.

---

## 10. 🎨 User Interaction and Design

- **In-app surface:** Unread Alerts tile (MOD-01 TILE-004) shows count + drill-down.
- **Email:** PHI-safe subjects, deep-link to in-app surface.
- **Audit panel:** Each work item exposes a "Notifications history" view (who, when, channel).
- **Admin Configurator surface:** Lives in MOD-07 (notification rule editor + escalation ladder editor).

---

## 11. 🔄 Key Flows

### Flow 1: KPI threshold → escalation

**Trigger:** MOD-03 emits `threshold_crossed`.
**Steps:**
1. Engine resolves recipient scope per ladder (Yellow → Worker; Red → +Supervisor; Breach → +Director).
2. Apply dedupe (skip if same recipient already notified for same (item,state)).
3. Apply quiet hours (digest non-critical; deliver Breach if bypass enabled).
4. Apply masking; deliver via configured channels.
5. Write audit entry on the work item.
**Exit state:** All required parties notified or override recorded.

### Flow 2: Operational event

**Trigger:** MOD-02 emits assignment / reassignment / secondary trigger / completion / primary resumed.
**Steps:**
1. Engine checks per-persona / per-program toggles.
2. Apply masking, dedupe, quiet hours.
3. Deliver via channels.
4. Audit.
**Exit state:** Recipients informed.

### Flow 3: Resolution (optional)

**Trigger:** State returns to a safer level (e.g., Yellow → Green via override).
**Preconditions:** Admin enabled resolution notifications.
**Steps:**
1. Engine emits resolution event.
2. Same dedupe / channel / masking pipeline.
**Exit state:** Recipients informed of resolution.

---

## 12. 🚫 Out of Scope and Deferred

### Out of Scope

- Long-form messaging / chat / conversation threads.
- Per-user template authoring.
- Cross-tenant message routing.

### Deferred

- SMS / Teams / Slack channels (post-launch).
- AI summarised digest (could subsume manual digests in a future cycle — see MOD-06 anomaly summary).
- Read-receipt analytics beyond simple delivery telemetry.

---

## 13. 🧬 Traceability

| AC ID | Story / Jira | Summary | Gherkin path | Test class | Verdict |
| --- | --- | --- | --- | --- | --- |
| AC-1.1 | [CAM-XXXXX] | One alert per transition | `docs/{FEATURE-ID}/gherkin/notif-001-once.feature` | --- | --- |
| AC-3.4 | [CAM-XXXXX] | Audit visibility per work item | `docs/{FEATURE-ID}/gherkin/notif-010-audit.feature` | --- | --- |
| AC-4.1 | [CAM-XXXXX] | Dedupe per recipient | `docs/{FEATURE-ID}/gherkin/notif-011-dedupe.feature` | --- | --- |
| AC-5.2 | [CAM-XXXXX] | Email subject sanitisation | `docs/{FEATURE-ID}/gherkin/notif-020-email-subject.feature` | --- | --- |
| AC-6.3 | [CAM-XXXXX] | Breach bypass | `docs/{FEATURE-ID}/gherkin/notif-021-bypass.feature` | --- | --- |

---

## 14. 🚀 Launch Plan

| Target | Milestone | Description | Exit |
| --- | --- | --- | --- |
| [YYYY-MM-DD] | ✅ UAT | In-app + email pilot | No P0/P1 noise / dedupe defects |
| [YYYY-MM-DD] | 🛑 Early Access | Pilot tenant | Quiet-hours + bypass verified |
| [YYYY-MM-DD] | 🛑 Launch | All tenants | Master metrics trending positive |

### Operation Checklist

- [ ] Email provider integrated + DKIM/SPF
- [ ] Default Ohio escalation ladder seeded
- [ ] Quiet-hours defaults set per persona
- [ ] Admin notification rule editor wired (MOD-07)
- [ ] Backlog monitoring dashboard live

---

## 15. 📣 Cross-Functional Impact Checklist

| Team | Y/N | Action |
| --- | --- | --- |
| Analytics | Y | Notification telemetry. |
| Configuration | Y | Default ladder + quiet hours per program. |
| Permissions | Y | Recipient scope / RBAC. |
| Reporting | Y | Notification history exposed to MOD-05. |

---

## 16. ⚠️ Risks

| Risk | L/I | Mitigation |
| --- | --- | --- |
| Alert fatigue undermines trust | M / H | Dedupe + quiet hours + digest + Breach-only bypass. |
| PHI leaked in email subject | L / H | Default-on subject sanitisation; Admin audit. |
| Escalation routes to wrong recipient | M / H | Org-hierarchy resolution with audit visibility; integration tests. |
| Backlog under high churn | M / M | Idempotent processing; backlog dashboard; horizontal scaling. |
| Notification storms during MOD-03 misconfiguration | M / H | "KPI Unavailable" produces no alerts; monitoring alerts Admin. |

---

## 17. ❓ Open Questions

| Question | Impact | Date | Owner |
| --- | --- | --- | --- |
| Confirm email provider + transactional template engine. | Blocks AC-5.x. | 2026-04-25 | SRE / Product |
| Confirm dedupe time window for Breach reminders (24h default?). | Blocks AC-4.x. | 2026-04-25 | Product |
| Confirm whether Director is a single user or a list per program. | Affects FR-NOTIF-010 routing. | 2026-04-25 | Product |
| Confirm quiet-hours model: per-user, per-persona, or both? | Affects AC-6.x storage. | 2026-04-25 | Product |
| Confirm whether digest contents are AI-summarised (MOD-06) or list-only at launch. | Cross-module scoping. | 2026-04-25 | Product |

---

## 18. 💬 FAQs

**Q:** Can a Worker silence Breach alerts?
**A:** No. Master guardrails forbid Workers from suppressing critical alerts; Admin controls bypass behavior at the program level.

**Q:** What happens if the email provider is down?
**A:** In-app delivery still occurs; email backlog drains when provider recovers. Backlog dashboard alerts Admin if it grows beyond threshold.

**Q:** Does this module own SMS?
**A:** Optional channel post-launch; not in initial scope.

---

## 19. 🧭 Decision Log

| # | Decision | Rationale | Date | By |
| --- | --- | --- | --- | --- |
| 1 | Three-tier escalation aligned to org hierarchy. | Source §11.2 + master §3 hierarchy. | 2026-04-25 | Product |
| 2 | In-app + email at launch; SMS / Teams / Slack deferred. | Reduces v1 vendor scope. | 2026-04-25 | Product |
| 3 | Resolution notifications are opt-in only. | Avoids new noise source. | 2026-04-25 | Product |

---

## 20. 📝 Change Log

| Date | By | Sections | Reason |
| --- | --- | --- | --- |
| 2026-04-25 | Kendrew Peacey | All | Initial draft — derived from §11. |

---

## 21. 🧾 Informed By

★

| Document | Path | SHA-256 |
| --- | --- | --- |
| Master PRD | [[PRD - Northwoods Traverse Workspace]] | sha256:[at approval] |
| Source PRD | `00 - Inbox/Northwoods_Traverse_PRD_v2.0.docx` (§11) | sha256:[at approval] |

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
