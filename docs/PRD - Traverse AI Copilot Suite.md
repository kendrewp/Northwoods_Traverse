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
# PRD — Traverse AI Copilot Suite (MOD-06)

> **V-Model–Aligned PRD.** Inherits from [[PRD - Northwoods Traverse Workspace]]. Source: §13 (10 AI features).

---

## 1. Product Overview

| Field | Value |
| --- | --- |
| 📅 Target date | [QX YYYY — Phase 4 of master launch plan] |
| 🟡 Document status | Draft |
| 🧭 Team | PM / Product / Dev Lead / Dev / QA: [Names] |
| 🗃️ Work tracker (Jira epic) | [CAM-XXXXX] |
| 📎 Parent PRD | [[PRD - Northwoods Traverse Workspace]] |
| 🔗 Resources | `00 - Inbox/Northwoods_Traverse_PRD_v2.0.docx` (§13) · master §3.4 (AI Principles) · master §15.4 (AI Security) |

---

## 2. 🎯 Objective and Problem Alignment

### Overview

Ten AI features embedded across the Workspace, all governed by master AI principles (permission-aware, citations, no silent writes, explainability, privacy-by-design, safety boundaries, degraded mode). Each feature operates under human-in-the-loop control — AI may draft and recommend, but users confirm before any state-changing action.

### Problem Statement

Workers, Supervisors, and Directors carry cognitive load that the rest of the system can lighten — drafting notes, identifying secondary triggers, predicting breach, synthesising case summaries, balancing workload, narrating program health, answering policy questions, drafting outreach, and surfacing anomalies. Without an AI layer that is permission-aware and trustworthy, that cognitive cost stays with humans.

### Importance

- Reduces drafting time across notes, communications, and summaries.
- Surfaces issues earlier than threshold-based KPI alerts alone (breach prediction, anomaly flagging).
- Lowers the cost of policy lookup with cited answers.
- Frees Supervisor / Director attention for judgement work.
- All under human control + Admin governance — no autonomous decisions.

### High-Level Approach

- **Ten features** (FR-AI-001 → FR-AI-010), all consuming permission-scoped data via a shared AI context builder that mirrors API RBAC + masking.
- **Output labelling** — every AI surface labels output as "AI Draft" or "AI-generated" until a user explicitly saves / sends.
- **Citations** — Q&A and policy answers cite admin-approved sources with links.
- **Explainability** — every recommendation shows the signals used.
- **Audit** — every interaction is logged: user, feature, referenced records, sources cited, output, applied/saved flag.
- **Degraded mode** — when AI is unavailable, every dependent surface continues to work without AI.
- **Admin governance** (in MOD-07) — enable / disable per feature × persona × program; redaction allowlist / denylist; log retention.

---

## 3. 👥 Stakeholders and Personas

- **P1 — Social Worker** — Briefing Card, Completion Assistant, Policy Q&A, Outreach Drafting.
- **P2 — Supervisor** — Briefing Card (team), Trigger Detection, Smart Case Summary, Workload Balancing, Anomaly Flagging.
- **P3 — Director** — Briefing Card (program), Program Health Narrative.
- **P4 — Admin** — AI Governance lives in MOD-07: enable/disable, redaction config, knowledge sources, log retention.
- **AI Service Provider** — external dependency.

---

## 4. 🎬 Target Use Cases

- **Worker briefing:** Worker logs in → Briefing Card summarises last-session changes + recommends top action with deep links.
- **Note drafting:** Worker opens an Active work item → Completion Assistant pre-populates draft note from case history (within permissions); Worker reviews and saves.
- **Trigger nudge:** Case has no payment in 30 days → Supervisor sees a suggestion to trigger Failure-to-Pay secondary workflow with evidence; Supervisor confirms or dismisses.
- **Smart Case Summary:** Supervisor opens reassignment flow → AI summarises case history; not stored as a case note unless saved.
- **Workload balance:** A Worker is over-capacity → AI recommends candidate assignees with rationale; Supervisor accepts / modifies / dismisses.
- **Director narrative:** Program Health Narrative renders alongside Program Performance metrics; Director expands to see supporting data.
- **Policy Q&A:** Worker asks "What docs are required for a Home Visit in Ohio Child Welfare?" → cited answer with links to approved sources.
- **Outreach draft:** Worker uses approved client letter template → AI fills based on context; Worker reviews + sends.
- **Breach forecast:** AI predicts a work item is likely to breach before it turns Red; clearly labelled as forecast, not official KPI.
- **Anomaly flag:** AI detects a case with multiple secondary workflows + Case Type with consistent breaches → flag to Supervisor with evidence.

---

## 5. 📊 Success Conditions and Metrics

### Goals

1. AI features ship behind Admin governance with safe defaults (off by default per program until Admin enables).
2. AI never violates RBAC / row / field masking — verified by audit + tests.
3. Every AI surface degrades gracefully when AI is unavailable.
4. Users trust the assistance — labelled, cited, explainable, controllable.

### Non-Goals

- Autonomous decisions (eligibility / clinical / state-changing actions).
- Free-form chat that bypasses citations.
- Cross-tenant model fine-tuning.

### Success Metrics

| Goal | Metric |
| --- | --- |
| Adoption | % of Workers using Completion Assistant on ≥ 50 % of work items within 30 days — target ≥ 50 % |
| Drafting time saved | Median seconds-to-save on note-entry sub-screens vs baseline — target reduction TBD |
| Q&A trust | % of Q&A answers rated helpful — target ≥ 70 % |
| Safety | 0 in-production incidents of AI referencing data outside user permissions |
| Citation coverage | 100 % of Policy Q&A answers include at least one cited source or "cannot answer" disclaimer |
| Forecast accuracy | Forecast precision / recall vs actual breach — measured per program; flag for off-switch if below threshold |

### Guardrails

- Permission-aware context construction — always.
- No silent writes — AI drafts are draft until user saves.
- Citations or "I don't know" — never hallucinate policy.
- Forecast vs official KPI status visually distinct.
- Degraded mode default-on for every feature.
- Audit trail — actor, feature, referenced records, sources, output, applied flag.

---

## 6. 🤔 Assumptions

- AI service provider supports redaction + no-training contractual clause. — ❓ unvalidated
- Knowledge sources for Policy Q&A are admin-curated and version-controlled. — ❓ unvalidated
- Permission-aware context builder mirrors the same access rules as the standard API surface. — ❓ unvalidated
- Forecast / anomaly models can be evaluated per-program with sufficient historical data. — ❓ unvalidated

---

## 7. 🔗 Dependencies and V-Model Gates

★

| Dependency | Type | Status | Notes |
| --- | --- | --- | --- |
| AI service provider | External | PENDING | Hard blocker. PHI/PII redaction + no-training agreement required. |
| MOD-07 Admin Suite | Internal | DRAFT | Hard blocker — AI Governance UI lives there. |
| MOD-03 KPI Engine | Internal | DRAFT | Consumes explainability payload (Plain-Language Explanation, Breach Prediction). |
| MOD-02 Workflow Engine | Internal | DRAFT | Source of lifecycle data for Trigger Detection, Anomaly Flagging. |
| MOD-01 Workspace UI | Internal | DRAFT | Renders Briefing Card slot + Completion Assistant + Smart Summary surfaces. |
| MOD-04 Notifications | Internal | DRAFT | Surface for Trigger Detection + Anomaly flags. |
| Audit log | Shared | PENDING | Records every AI interaction. |
| Dev Lead feasibility | V-Model gate | PENDING | ★ |
| Estimation | V-Model gate | PENDING | ★ |

---

## 8. 📋 Requirements

★

| Priority | Job Story | Acceptance Criteria (G/W/T) | Jira | Source |
| --- | --- | --- | --- | --- |
| P0 | As any persona, when I land on the Dashboard, I want a persona-scoped Daily AI Briefing summarising changes since my last session and recommending top actions so that I start the day informed. | AC-1.1: *Given* my persona, *Then* the briefing is scoped (Worker = caseload; Supervisor = team; Director = program). AC-1.2: *Given* the briefing, *Then* it references only permission-scoped work items and cases — no PHI/PII the user can't see. AC-1.3: *Given* the briefing, *Then* it includes deep links to the underlying work items. AC-1.4: *Given* the card, *Then* it shows generated-at timestamp and supports rate-limited manual refresh. AC-1.5: *Given* AI unavailable, *Then* a non-blocking fallback shows standard summary counts. AC-1.6: *Given* Admin, *Then* the card can be enabled/disabled per persona × program × environment. | [CAM-XXXXX] | FR-AI-001 |
| P0 | As a Social Worker, when I open an Active work-item sub-screen, I want a contextual Completion Assistant that drafts notes, suggests required fields, and recommends required docs so that I work faster within guardrails. | AC-2.1: *Given* a sub-screen, *Then* the assistant is embedded inline. AC-2.2: *Given* AI-drafted content, *Then* it is editable draft text labelled "AI Draft" until I save. AC-2.3: *Given* a Document Upload sub-screen, *Then* AI suggests required document types based on Case Type + work item definition. AC-2.4: *Given* a Data Collection sub-screen, *Then* AI pre-populates fields from existing case history where permitted. AC-2.5: *Given* AI unavailable, *Then* the sub-screen functions normally without assistance. | [CAM-XXXXX] | FR-AI-002 |
| P0 | As a Supervisor, when a case shows a qualifying event pattern, I want AI to surface a secondary-workflow trigger suggestion with evidence so that I can act earlier. | AC-3.1: *Given* a qualifying pattern, *Then* an alert / notification surfaces with the suggested secondary workflow + evidence (e.g., "No payment recorded in 30 days"). AC-3.2: *Given* the suggestion, *Then* I review and decide whether to trigger; AI never auto-triggers. AC-3.3: *Given* the suggestion, *Then* it is auditable and includes the signals used. | [CAM-XXXXX] | FR-AI-003 |
| P1 | As a Supervisor, when I want earlier signal than threshold-based KPI, I want AI breach forecasts that supplement (do not replace) official KPI status so that I can intervene before red. | AC-4.1: *Given* a forecast, *Then* it is clearly labelled as forecast, distinct from official KPI status. AC-4.2: *Given* the forecast, *Then* it includes primary drivers (e.g., high workload, historical cycle time). AC-4.3: *Given* insufficient historical data, *Then* a forecast is not shown. AC-4.4: *Given* the forecast, *Then* it does not change official KPI status. AC-4.5: *Given* Admin, *Then* the feature can be disabled per program if accuracy is insufficient. | [CAM-XXXXX] | FR-AI-004 |
| P0 | As a Supervisor, when I review or reassign a case, I want a Smart Case Summary so that I get the salient narrative quickly without reading the whole case file. | AC-5.1: *Given* Case Detail or the reassignment flow, *Then* the summary is available. AC-5.2: *Given* the summary, *Then* it references only data visible to me per RBAC + masking. AC-5.3: *Given* the summary, *Then* it is labelled AI-generated and not stored as a case note unless I explicitly save it. AC-5.4: *Given* AI unavailable, *Then* the standard Case Detail is shown without summary. | [CAM-XXXXX] | FR-AI-005 |
| P1 | As a Supervisor, when balancing workload, I want AI candidate-assignee recommendations with rationale so that I can rebalance fast. | AC-6.1: *Given* the Team Workload View, *Then* recommendations surface there. AC-6.2: *Given* a recommendation, *Then* it includes a suggested assignee + rationale (e.g., "lowest at-risk count", "matching program expertise"). AC-6.3: *Given* the recommendation, *Then* I can accept, modify, or dismiss. AC-6.4: *Given* I accept, *Then* the standard assignment workflow runs with full audit logging. | [CAM-XXXXX] | FR-AI-006 |
| P1 | As a Director, when I view a program, I want an AI-generated Program Health Narrative so that trends, outliers, and risk signals are summarised in plain language. | AC-7.1: *Given* the Director Dashboard, *Then* the narrative renders alongside program metrics. AC-7.2: *Given* aggregated data, *Then* the narrative contains no PHI/PII. AC-7.3: *Given* the narrative, *Then* it is labelled AI-generated. AC-7.4: *Given* a program narrative, *Then* I can expand to see the supporting data. | [CAM-XXXXX] | FR-AI-007 |
| P0 | As a Worker, when I have a policy question in context, I want a cited Policy & Compliance Q&A so that I get accurate answers without leaving the cockpit. | AC-8.1: *Given* a question, *Then* only Admin-approved sources are used. AC-8.2: *Given* an answer, *Then* it includes citations + links to source sections. AC-8.3: *Given* my context (work item type, program, state), *Then* the Q&A is context-aware. AC-8.4: *Given* a question that cannot be answered from available sources, *Then* AI states this clearly rather than hallucinating. AC-8.5: *Given* every Q&A interaction, *Then* it is auditable. AC-8.6: *Given* an answer, *Then* I can rate helpfulness (feedback). | [CAM-XXXXX] | FR-AI-008 |
| P0 | As a Worker, when communicating with clients, I want AI-drafted letters / outreach using approved templates so that I can send faster within compliance. | AC-9.1: *Given* AI-drafted content, *Then* it is editable draft text and I must review before sending. AC-9.2: *Given* drafts, *Then* they use Admin-approved templates and include placeholders for missing info. AC-9.3: *Given* draft generation, *Then* field-level masking is respected and restricted fields excluded. AC-9.4: *Given* AI assistance, *Then* the audit event records that content was AI-assisted; the user-saved text is the source of truth. AC-9.5: *Given* Admin tone/style options, *Then* I can regenerate drafts and choose. | [CAM-XXXXX] | FR-AI-009 |
| P1 | As a Supervisor, when patterns are not visible through KPIs alone, I want anomaly flags with evidence so that I can investigate early. | AC-10.1: *Given* a flag, *Then* it surfaces in the Team Workload View or as in-app notification. AC-10.2: *Given* a flag, *Then* it includes evidence + signals used. AC-10.3: *Given* aggregated pattern data, *Then* flag summaries do not expose PHI/PII. AC-10.4: *Given* a flag, *Then* I can dismiss / act / escalate. AC-10.5: *Given* every flag, *Then* it is auditable. | [CAM-XXXXX] | FR-AI-010 |
| P0 | As an Admin / Security officer, when AI runs, I want shared guardrails enforced across all 10 features so that one feature cannot silently bypass them. | AC-11.1: *Given* any AI request, *Then* the AI context builder enforces the same RBAC + row + field rules as the standard API. AC-11.2: *Given* outputs, *Then* they never include identifiers or details the user cannot access elsewhere. AC-11.3: *Given* PHI/PII, *Then* it is minimised per Admin allowlist / denylist redaction config. AC-11.4: *Given* any AI surface, *Then* outputs are labelled "AI Draft" or "AI-generated" until user-saved. AC-11.5: *Given* every interaction, *Then* an AIInteractionLog record is written (user, feature, referenced records, sources cited, output, applied/saved flag). AC-11.6: *Given* Admin opt-out for logging, *Then* only minimal metadata is retained. AC-11.7: *Given* AI service unavailable, *Then* every dependent surface continues to function. AC-11.8: *Given* customer data, *Then* it is not used for model training unless contractually enabled. | [CAM-XXXXX] | Master §15.4 SEC-030..033 — applied here |

---

## 9. ⚙️ NFRs

★ Inherits master §9. Module deltas:

**Performance:** AI feature response ≤ 5 s for common interactions (briefing, note draft) under typical load; show progress indicator if longer (master §16.1).

**Security:** Permission-aware context. PHI/PII minimisation. Output labelling. No silent writes. AIInteractionLog mandatory.

**Reliability:** Degraded mode for all 10 features. Provider outage MUST NOT block the cockpit.

**Privacy:** Customer data not used for training unless contractually enabled (SEC-033). Per-tenant log retention configurable.

**Observability:** Telemetry: ai_request, ai_response, ai_applied, ai_dismissed, ai_unavailable, citation_clicked, ai_feedback. Per-feature error rate + latency.

**Compatibility:** Screen-reader accessibility on every AI surface (master §16.4).

---

## 10. 🎨 User Interaction and Design

- **AI Briefing Card** — slot in MOD-01 Dashboard.
- **Completion Assistant** — embedded in work-item sub-screen (MOD-01).
- **Smart Case Summary** — surfaced in Case Detail + reassignment flow (MOD-01 + MOD-11).
- **Trigger Detection** — Supervisor notification (MOD-04) + secondary trigger UI (MOD-01).
- **Workload Balancing** — Team Workload View (MOD-01 supervisor surface).
- **Program Health Narrative** — Director Dashboard / Analytics Hub (MOD-01 / MOD-05).
- **Policy Q&A** — embedded chat-style panel reachable from work-item sub-screen + cockpit.
- **Outreach Drafting** — embedded in client-communication work items.
- **Breach Forecast** — supplement in KPI explainability panel (MOD-03 payload + MOD-01 panel).
- **Anomaly Flagging** — Team Workload View + in-app notifications (MOD-01 + MOD-04).

UX rules:
- Every AI surface shows a "Report unsafe / incorrect" action (master SEC-032 #3).
- "AI Draft" or "AI-generated" labels are never hidden, even after editing.
- Citations are always linked to source.
- Forecast UI is visually distinct from official KPI badges.

---

## 11. 🔄 Key Flows

### Flow 1: Note drafting with Completion Assistant

1. Worker opens Active work item.
2. Sub-screen loads; assistant requests permission-scoped context.
3. AI returns draft note labelled "AI Draft".
4. Worker edits, completes required fields, saves.
5. Audit entry written (AIInteractionLog + sub-screen completion).

### Flow 2: Policy Q&A with citation

1. Worker asks question in cockpit.
2. Engine resolves persona / program / state context + Admin-approved sources.
3. AI returns answer + citations or "cannot answer".
4. Worker may click citation to view source; rate helpfulness.
5. Audit entry written.

### Flow 3: Trigger Detection → Supervisor confirm

1. AI scans active cases against trigger patterns (per Admin config).
2. Suggestion delivered to Supervisor (notification + Team view).
3. Supervisor reviews evidence; opens secondary trigger UI (MOD-01) or dismisses.
4. If triggered, MOD-02 handles execution; AI suggestion linked in audit.

### Flow 4: Degraded mode

1. AI service unreachable.
2. All AI surfaces render fallback states (Briefing Card → standard counts; Completion Assistant → no draft; Q&A → "Unavailable"; etc.).
3. Cockpit functions normally.

---

## 12. 🚫 Out of Scope and Deferred

### Out of Scope

- Autonomous decisions / state changes.
- Free-form open-ended chat without context / citations.
- Cross-tenant model fine-tuning.

### Deferred

- Voice-driven AI (cockpit text-only at launch).
- Federated learning / personalised model adaptation.
- AI-summarised digest notifications (MOD-04 dependency — defer).
- AI-driven workflow / KPI authoring (Admin uses MOD-07 directly).

---

## 13. 🧬 Traceability

| AC ID | Story | Summary | Gherkin | Test class | Verdict |
| --- | --- | --- | --- | --- | --- |
| AC-1.5 | [CAM-XXXXX] | Briefing degraded mode | `docs/{FEATURE-ID}/gherkin/ai-001-degraded.feature` | --- | --- |
| AC-2.2 | [CAM-XXXXX] | "AI Draft" labelling | `docs/{FEATURE-ID}/gherkin/ai-002-label.feature` | --- | --- |
| AC-3.2 | [CAM-XXXXX] | No auto-trigger | `docs/{FEATURE-ID}/gherkin/ai-003-confirm.feature` | --- | --- |
| AC-4.1 | [CAM-XXXXX] | Forecast visual distinction | `docs/{FEATURE-ID}/gherkin/ai-004-distinct.feature` | --- | --- |
| AC-8.4 | [CAM-XXXXX] | "I don't know" not hallucinate | `docs/{FEATURE-ID}/gherkin/ai-008-no-hallucinate.feature` | --- | --- |
| AC-11.1 | [CAM-XXXXX] | Permission-aware context | `docs/{FEATURE-ID}/gherkin/ai-shared-rbac.feature` | --- | --- |
| AC-11.7 | [CAM-XXXXX] | Cockpit functions when AI down | `docs/{FEATURE-ID}/gherkin/ai-shared-degraded.feature` | --- | --- |

---

## 14. 🚀 Launch Plan

| Target | Milestone | Description | Exit |
| --- | --- | --- | --- |
| [YYYY-MM-DD] | ✅ UAT | All 10 features behind Admin governance, default off | Citations + degraded mode verified |
| [YYYY-MM-DD] | 🛑 Early Access | Pilot tenants opt-in per feature | Adoption metrics + feedback loops live |
| [YYYY-MM-DD] | 🛑 Launch | All tenants — Admin-controlled enable | Master AI principles satisfied across 10 features |

### Operation Checklist

- [ ] AI provider contract signed (no-training, redaction)
- [ ] AIInteractionLog wired
- [ ] Per-feature kill switch (MOD-07 Admin)
- [ ] Redaction allowlist / denylist configured per tenant
- [ ] "Report unsafe / incorrect" feedback path live
- [ ] Knowledge sources curated for Policy Q&A

---

## 15. 📣 Cross-Functional Impact Checklist

| Team | Y/N | Action |
| --- | --- | --- |
| Analytics | Y | AI telemetry. |
| Configuration | Y | AI Governance defaults; Q&A source library. |
| Legal | Y | AI provider contractual clauses. |
| Customer Success | Y | Training on labels, citations, feedback. |
| Permissions | Y | Permission-aware context builder. |
| Product Marketing | Y | AI positioning + governance story. |

---

## 16. ⚠️ Risks

| Risk | L/I | Mitigation |
| --- | --- | --- |
| AI hallucinates policy answer | M / H | Citations or "I don't know" — AC-8.4. Knowledge sources Admin-curated. |
| Permission leak through AI context | L / H | Shared context builder mirrors API rules; integration tests. |
| Forecast accuracy too low | M / M | Per-program enable/disable. Telemetry-driven kill switch. |
| Customer-data training drift | L / H | Contractual no-training; periodic audit. |
| Degraded mode missing on a surface | M / H | Master guardrail + per-surface tests. |
| AI Draft saved silently | L / H | Labelling persists until user-saved; audit + tests. |
| Vendor outage cascades | M / M | Cockpit + downstream modules independent of AI. |

---

## 17. ❓ Open Questions

| Question | Impact | Date | Owner |
| --- | --- | --- | --- |
| Confirm AI provider + redaction posture + no-training contract. | Hard blocker. | 2026-04-25 | Legal + Security |
| Confirm Q&A knowledge source format + curation workflow. | Blocks AC-8.x. | 2026-04-25 | Product + Compliance |
| Confirm forecast model approach (heuristic, statistical, ML). | Affects AC-4.x scoping. | 2026-04-25 | Dev Lead |
| Confirm anomaly detection signals + per-program calibration policy. | Affects AC-10.x. | 2026-04-25 | Product + Dev Lead |
| Confirm "Report unsafe / incorrect" downstream workflow (Admin queue, retraining, etc.). | Blocks AC-8.6 + master SEC-032 #3. | 2026-04-25 | Product |

---

## 18. 💬 FAQs

**Q:** Can AI auto-trigger a secondary workflow?
**A:** No. Master AI principles require human-in-the-loop for any state change (FR-AI-003 #3).

**Q:** What about voice / chat?
**A:** Out of scope for v1 — text only. Free-form chat without citations is not supported.

**Q:** Can a customer disable AI entirely?
**A:** Yes — via MOD-07 Admin Governance, all features can be disabled per persona / program / tenant.

**Q:** Where does the AI Briefing Card content come from?
**A:** This module — content. The slot lives in MOD-01 Dashboard.

---

## 19. 🧭 Decision Log

| # | Decision | Rationale | Date | By |
| --- | --- | --- | --- | --- |
| 1 | All AI features default off; Admin opts in per persona × program. | Trust + governance posture. | 2026-04-25 | Product |
| 2 | "AI Draft" / "AI-generated" labelling persists until user-saved. | Master SEC-032. | 2026-04-25 | Product |
| 3 | Citations or "I don't know" — never hallucinate policy. | Master AI principle. | 2026-04-25 | Product |
| 4 | Forecast visually distinct from official KPI status. | Avoid trust collision. | 2026-04-25 | Product |

---

## 20. 📝 Change Log

| Date | By | Sections | Reason |
| --- | --- | --- | --- |
| 2026-04-25 | Kendrew Peacey | All | Initial draft — derived from §13. |

---

## 21. 🧾 Informed By

★

| Document | Path | SHA-256 |
| --- | --- | --- |
| Master PRD | [[PRD - Northwoods Traverse Workspace]] | sha256:[at approval] |
| Source PRD | `00 - Inbox/Northwoods_Traverse_PRD_v2.0.docx` (§13) | sha256:[at approval] |

---

## 22. ✅ Approval and Checksum

★

### Approval checklist

- [ ] Problem statement clear
- [ ] INVEST + G/W/T satisfied for all 10 features + shared guardrails
- [ ] NFRs quantified
- [ ] Out of Scope / Deferred explicit
- [ ] Open Questions resolved
- [ ] Dev Lead feasibility IN PLACE
- [ ] Estimation IN PLACE
- [ ] Traceability matrix populated
- [ ] AI provider contract signed
- [ ] AI Governance UI in MOD-07 ready

### Approval record

| Field | Value |
| --- | --- |
| Approved by | [Name] |
| Approval date | [YYYY-MM-DD] |
| SHA-256 | sha256-[at approval] |
| Pipeline tracker | [Path] |

---

*Template v1.0 · Camis V-Model–Aligned PRD · Compatible with `camis-v-model:draft-prd` skill v2.1.0*
