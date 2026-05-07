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
# PRD — Traverse Universal Search (MOD-08)

> **V-Model–Aligned PRD.** Inherits from [[PRD - Northwoods Traverse Workspace]]. Source: §19.

---

## 1. Product Overview

| Field | Value |
| --- | --- |
| 📅 Target date | [QX YYYY — Phase 3 of master launch plan] |
| 🟡 Document status | Draft |
| 🧭 Team | PM / Product / Dev Lead / Dev / QA: [Names] |
| 🗃️ Work tracker (Jira epic) | [CAM-XXXXX] |
| 📎 Parent PRD | [[PRD - Northwoods Traverse Workspace]] |
| 🔗 Resources | `00 - Inbox/Northwoods_Traverse_PRD_v2.0.docx` (§19) |

---

## 2. 🎯 Objective and Problem Alignment

### Overview

A persistent AI-powered top search bar on every non-Admin primary screen that combines: (1) keyword + natural-language query, (2) persona-aware autocomplete suggestion chips, (3) cross-entity federated search across cases / work items / team members / programs, and (4) a conversational AI response banner that synthesises results into a brief insight. Permission-aware, persona-aware, fast.

### Problem Statement

Today, finding "what's breached", "who has capacity", or "due this week" requires multiple navigations and filters. Without a single search surface, users guess at filter combinations or fall back to manual lists. Without natural language and an AI synthesis layer, search returns volume — not insight.

### Importance

- Single intent-aware entry point reduces clicks across personas.
- Persona-aware chips meet users where they are.
- Federated search across entities removes the "which list do I open?" decision.
- Conversational synthesis turns N results into a one-sentence "what to do" — without hallucinating case details.

### High-Level Approach

- Search bar at the top of every non-Admin primary screen (Social Worker Dashboard, Cases, Case Detail, Work Items, Supervisor Dashboard, Director Dashboard). **Never** on Admin configuration screens.
- Empty + focused → display ≥ 4 persona-specific chips.
- Typing → real-time filtering (client-side prototype, server-side production).
- Results dropdown → grouped sections (Cases / Work Items / Team Members / Programs) with empty groups hidden.
- AI banner at top of dropdown → 1–3 sentence narrative grounded in actual results, distinct branding, AI label.
- Selection → in-context navigation (Case → Cases view + Case Detail; Work Item → sub-screen modal; etc.).
- All grounded in master AI principles + permission-aware context.

---

## 3. 👥 Stakeholders and Personas

- **P1 — Social Worker** — primary user; chips for urgent / breached / due-dates.
- **P2 — Supervisor** — chips for team workload + capacity.
- **P3 — Director** — chips for program-level performance.
- **P4 — Admin** — search bar is **not** rendered on Admin screens.
- **MOD-06 AI Copilot** — owns the conversational response model.
- **MOD-01 Workspace UI** — hosts the search bar slot.

---

## 4. 🎬 Target Use Cases

- **NL query:** Worker types "show me breached cases" → results filtered to KPI=Breached; AI banner: "Found 1 breached case. Williams Family Safety Assessment is 2 days overdue and requires immediate action."
- **NL temporal:** Worker types "what is due this week" → work items in this week's window appear.
- **Capacity:** Supervisor types "who has capacity" → team members sorted by available caseload.
- **Federated:** Director types a program name → cases / work items / team members / programs all return in grouped sections.
- **Empty no-results:** Worker types "burgers" → "No results" with search tips.
- **Chips:** Worker focuses empty bar → 4+ persona chips appear; clicking executes immediately.

---

## 5. 📊 Success Conditions and Metrics

### Goals

1. Users find what they need in fewer clicks.
2. AI banner is grounded — never invents case details.
3. Search respects RBAC + masking — no PII in URLs or history.
4. Available everywhere a non-Admin user works; never on Admin screens.

### Non-Goals

- Search authoring / saved-searches surface (deferred).
- Replacing list views — results dropdown navigates *to* lists / detail.
- Full-text search across case notes (v1 limited to entity name + key metadata; deeper notes search deferred).
- Admin-screen search.

### Success Metrics

| Goal | Metric |
| --- | --- |
| Latency | Prototype keystroke → results ≤ 300 ms. Production P95 search latency ≤ 1.5 s for ≤ 10 000 active cases. |
| Adoption | % of sessions using search — target ≥ 50 % within 30 days |
| Index freshness | Index refresh lag ≤ 60 s after data mutation |
| AI safety | 0 hallucinated case details across audited search-AI banners |

### Guardrails

- No PII in URL parameters or browser history.
- AI banner grounded in actual returned data — citations not required for entity names but no fabrication.
- Permission-aware results — never return entities the user cannot access.
- Search bar absent from Admin screens.

---

## 6. 🤔 Assumptions

- Search index is updatable in ≤ 60 s after data mutation. — ❓ unvalidated
- Cross-entity result types are sufficient for v1 (Cases, Work Items, Team Members, Programs). — ❓ unvalidated
- AI conversational layer reuses MOD-06 infrastructure. — ❓ unvalidated
- Persona switching is detectable client-side for chip refresh. — ❓ unvalidated

---

## 7. 🔗 Dependencies and V-Model Gates

★

| Dependency | Type | Status | Notes |
| --- | --- | --- | --- |
| MOD-06 AI Copilot | Internal | DRAFT | Hard blocker for AI banner. |
| MOD-01 Workspace UI | Internal | DRAFT | Hosts search bar slot. |
| Search index infrastructure | Shared | PENDING | Hard blocker for production NFRs. |
| Identity / RBAC | External | PENDING | Hard blocker — permission-aware results. |
| Dev Lead feasibility | V-Model gate | PENDING | ★ |
| Estimation | V-Model gate | PENDING | ★ |

---

## 8. 📋 Requirements

★

| Priority | Job Story | Acceptance Criteria (G/W/T) | Jira | Source |
| --- | --- | --- | --- | --- |
| P1 | As any non-Admin persona, when I type natural-language queries, I want intent-based search so that I do not need exact field syntax. | AC-1.1: *Given* a query containing "breach", *Then* results return cases / work items with KPI = Breached. AC-1.2: *Given* "due this week" or "due today", *Then* work items in the relevant time window are returned. AC-1.3: *Given* "capacity", *Then* team-member records appear sorted by available case load. AC-1.4: *Given* an unrecognised query, *Then* a "no results" state appears with suggested alternatives. AC-1.5: *Given* the parser, *Then* it is intent-based and does not require exact field-match syntax. | [CAM-XXXXX] | SEARCH-001 |
| P1 | As any non-Admin persona, when I focus the empty search bar, I want persona-specific suggestion chips so that I can run common tasks instantly. | AC-2.1: *Given* the bar is focused with no input, *Then* ≥ 4 chips display. AC-2.2: *Given* my active persona, *Then* chips are persona-specific (Worker = urgent / breached / due dates; Supervisor = team workload / capacity; Director = program performance). AC-2.3: *Given* a chip click, *Then* the search bar populates and the query executes immediately. AC-2.4: *Given* I begin typing, *Then* chips are dismissed; results update in real time (client-side prototype, server-side production). | [CAM-XXXXX] | SEARCH-002 |
| P1 | As any non-Admin persona, when I search, I want federated results across Cases / Work Items / Team Members / Programs so that I do not need to choose which list to open. | AC-3.1: *Given* results, *Then* they are grouped under labeled sections (Cases / Work Items / Team Members / Programs). AC-3.2: *Given* each result row, *Then* it shows the entity name, contextual metadata (case ID, program, KPI badge), and a navigation chevron. AC-3.3: *Given* a Case result clicked, *Then* I navigate to Case Detail. AC-3.4: *Given* a Work Item result clicked, *Then* the Work Item sub-screen opens. AC-3.5: *Given* Team Member / Program results, *Then* they navigate to the Supervisor / Director dashboards respectively. AC-3.6: *Given* an empty group, *Then* it is hidden. AC-3.7: *Given* zero results across all entities, *Then* a "no results" message with search tips is shown. | [CAM-XXXXX] | SEARCH-003 |
| P1 | As any non-Admin persona, when results return, I want a 1–3 sentence AI narrative grounded in actual results so that I get insight, not just a list. | AC-4.1: *Given* results present, *Then* the AI banner appears at the top of the dropdown. AC-4.2: *Given* the narrative, *Then* it is grounded in actual returned data — no hallucinated details. AC-4.3: *Given* length, *Then* the narrative is ≤ 3 sentences. AC-4.4: *Given* visual treatment, *Then* the banner is distinct (e.g., branded gradient) and labeled "AI". AC-4.5: *Given* zero results, *Then* no AI narrative shows (or one offering a redirect suggestion). | [CAM-XXXXX] | SEARCH-004 |
| P1 | As any non-Admin persona, when I navigate the app, I want the search bar consistently placed and absent from Admin screens so that the surface is predictable. | AC-5.1: *Given* the 6 non-Admin primary screens (Social Worker Dashboard, Cases, Case Detail, Work Items, Supervisor Dashboard, Director Dashboard), *Then* the search bar is visible + functional on all 6. AC-5.2: *Given* the 4 Admin configuration screens (Work Item Library, Workflow Configurator, User Management, Notification Rules), *Then* the search bar is absent. AC-5.3: *Given* placement, *Then* it is consistent across screens — top of main content, below the page header — with consistent design-system styling. AC-5.4: *Given* navigation, *Then* search state (query text, open dropdown) clears between screens. AC-5.5: *Given* personas, *Then* P1 / P2 / P3 all have access. | [CAM-XXXXX] | SEARCH-005 |
| P2 | As any non-Admin persona, when I select a search result, I want context-preserving navigation so that selection is instant action. | AC-6.1: *Given* a Case result clicked, *Then* the active view is set to Cases and Case Detail opens. AC-6.2: *Given* a Work Item result clicked, *Then* the Work Item modal opens overlaying the current screen. AC-6.3: *Given* selection, *Then* the input is cleared and the dropdown closes. AC-6.4: *Given* I back-navigate from Case Detail after a search, *Then* I land in Cases list — not the search query. | [CAM-XXXXX] | SEARCH-006 |
| P0 | As any non-Admin persona, when I search, I want fast, fresh, safe results so that the bar is trustworthy. | AC-7.1: *Given* prototype, *Then* keystroke → results ≤ 300 ms. AC-7.2: *Given* production, *Then* P95 latency < 1.5 s for ≤ 10 000 active cases. AC-7.3: *Given* the index, *Then* refresh lag < 60 s after mutation. AC-7.4: *Given* an empty data set, *Then* search returns 0 results without throwing. AC-7.5: *Given* navigation, *Then* no PII is transmitted via URL parameters or browser history. | [CAM-XXXXX] | SEARCH-NFR-001 |

---

## 9. ⚙️ NFRs

★ Inherits master §9. Module deltas above (SEARCH-NFR-001).

**Security:** Permission-aware indexing — search index respects RBAC + row-level + field masking. AI banner consumes permission-scoped context only (master §15.4 SEC-030).

**Observability:** Telemetry: search_focused, suggestion_chip_clicked, query_submitted, result_clicked, ai_banner_shown, no_results.

**Compatibility:** WCAG 2.1 AA — keyboard navigation through chips + dropdown groups + result rows; screen-reader landmarks.

---

## 10. 🎨 User Interaction and Design

- **Persistent top bar** — single search input, optional voice hand-off deferred.
- **Focused empty state** — chips row beneath input.
- **Typing state** — chips collapse; results dropdown expands.
- **Results dropdown** — AI banner top, grouped sections beneath, navigation chevrons on each row.
- **No-results state** — friendly empty state with search tips.

---

## 11. 🔄 Key Flows

### Flow 1: NL query → AI banner → action

1. User focuses search bar; chips display.
2. User types "show me breached cases".
3. Server interprets intent; returns Cases section + Work Items section.
4. AI banner generates 1–3 sentence narrative grounded in returned data.
5. User clicks top result → Case Detail opens; search clears.

### Flow 2: Persona chip → execute

1. User focuses search bar (no input).
2. Persona chips appear.
3. User clicks "What's overdue?" chip.
4. Query executes; results render.
5. Action.

---

## 12. 🚫 Out of Scope and Deferred

### Out of Scope

- Admin-screen search.
- Saved searches / search history (v1).
- Voice search.
- Full-text case-note search at launch.

### Deferred

- Cross-tenant federated search.
- Custom result types (Reports, Audit Logs).
- Personalised suggestion learning beyond persona defaults.

---

## 13. 🧬 Traceability

| AC ID | Story | Summary | Gherkin | Test class | Verdict |
| --- | --- | --- | --- | --- | --- |
| AC-1.1 | [CAM-XXXXX] | Intent: breach | `docs/{FEATURE-ID}/gherkin/search-001-breach.feature` | --- | --- |
| AC-2.2 | [CAM-XXXXX] | Persona-specific chips | `docs/{FEATURE-ID}/gherkin/search-002-chips.feature` | --- | --- |
| AC-3.6 | [CAM-XXXXX] | Empty groups hidden | `docs/{FEATURE-ID}/gherkin/search-003-groups.feature` | --- | --- |
| AC-4.2 | [CAM-XXXXX] | AI grounded — no hallucination | `docs/{FEATURE-ID}/gherkin/search-004-grounded.feature` | --- | --- |
| AC-5.2 | [CAM-XXXXX] | Absent on Admin screens | `docs/{FEATURE-ID}/gherkin/search-005-no-admin.feature` | --- | --- |
| AC-7.5 | [CAM-XXXXX] | No PII in URL / history | `docs/{FEATURE-ID}/gherkin/search-007-pii.feature` | --- | --- |

---

## 14. 🚀 Launch Plan

| Target | Milestone | Description | Exit |
| --- | --- | --- | --- |
| [YYYY-MM-DD] | ✅ UAT | Persona pilots | Latency + grounded-banner verified |
| [YYYY-MM-DD] | 🛑 Early Access | Pilot tenant | NFR thresholds met |
| [YYYY-MM-DD] | 🛑 Launch | All tenants | Adoption metric trending positive |

### Operation Checklist

- [ ] Search index infrastructure operational
- [ ] Persona chip libraries seeded
- [ ] AI banner pipeline wired to MOD-06
- [ ] Telemetry events live
- [ ] No-PII-in-URL test passing

---

## 15. 📣 Cross-Functional Impact Checklist

| Team | Y/N | Action |
| --- | --- | --- |
| Analytics | Y | Search telemetry. |
| Permissions | Y | Permission-aware indexing. |
| Customer Success | Y | NL examples + chip cheat sheet. |

---

## 16. ⚠️ Risks

| Risk | L/I | Mitigation |
| --- | --- | --- |
| AI banner hallucinates case details | L / H | Grounded-only narrative; integration tests + audit. |
| Index lag → stale results | M / M | ≤ 60 s freshness; observability dashboards. |
| Permission leak via index | L / H | Permission-aware indexing + tests. |
| Performance under large tenants | M / M | NFRs sized to ≤ 10 000 active cases; load tests. |
| PII in URL leaks via deep-link | L / H | AC-7.5 mandate + tests. |

---

## 17. ❓ Open Questions

| Question | Impact | Date | Owner |
| --- | --- | --- | --- |
| Confirm search index technology (in-house vs managed). | NFR baseline. | 2026-04-25 | Dev Lead |
| Confirm persona-chip catalogue per persona at launch. | Blocks AC-2.2. | 2026-04-25 | Product |
| Confirm whether Director sees individual case results or aggregate-only (privacy stance). | Affects AC-3.x. | 2026-04-25 | Product |
| Confirm AI banner generation approach (template + slots vs free LLM). | Affects AC-4.2. | 2026-04-25 | Dev Lead + MOD-06 |
| Confirm Spanish / multilingual NL support scope at launch. | Affects v1 scope. | 2026-04-25 | Product |

---

## 18. 💬 FAQs

**Q:** Why no Admin search?
**A:** Source SEARCH-005 mandates absence on configuration screens — Admin tasks are configuration not retrieval.

**Q:** Will AI banner cite sources?
**A:** Banner cites the actual returned results; for policy answers see MOD-06 Policy Q&A (separate surface with citations).

**Q:** What if AI is unavailable?
**A:** Search still runs; banner is suppressed; chips + federated results work normally (master degraded-mode principle).

---

## 19. 🧭 Decision Log

| # | Decision | Rationale | Date | By |
| --- | --- | --- | --- | --- |
| 1 | AI banner grounded in returned results — never fabricate. | Master AI principles. | 2026-04-25 | Product |
| 2 | Search bar absent from Admin screens. | Source SEARCH-005. | 2026-04-25 | Product |
| 3 | Permission-aware indexing — never return entities user cannot access. | Master security. | 2026-04-25 | Product |

---

## 20. 📝 Change Log

| Date | By | Sections | Reason |
| --- | --- | --- | --- |
| 2026-04-25 | Kendrew Peacey | All | Initial draft — derived from §19. |

---

## 21. 🧾 Informed By

★

| Document | Path | SHA-256 |
| --- | --- | --- |
| Master PRD | [[PRD - Northwoods Traverse Workspace]] | sha256:[at approval] |
| Source PRD | `00 - Inbox/Northwoods_Traverse_PRD_v2.0.docx` (§19) | sha256:[at approval] |

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
