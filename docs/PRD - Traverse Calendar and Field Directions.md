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
# PRD — Traverse Calendar and Field Directions (MOD-09)

> **V-Model–Aligned PRD.** Inherits from [[PRD - Northwoods Traverse Workspace]]. Source: §§21, 22.

---

## 1. Product Overview

| Field | Value |
| --- | --- |
| 📅 Target date | [QX YYYY — Phase 5 of master launch plan] |
| 🟡 Document status | Draft |
| 🧭 Team | PM / Product / Dev Lead / Dev / QA: [Names] |
| 🗃️ Work tracker (Jira epic) | [CAM-XXXXX] |
| 📎 Parent PRD | [[PRD - Northwoods Traverse Workspace]] |
| 🔗 Resources | `00 - Inbox/Northwoods_Traverse_PRD_v2.0.docx` (§§21, 22) |

---

## 2. 🎯 Objective and Problem Alignment

### Overview

A Social-Worker-only Calendar surface that unifies **Work Item Events** (case-related, scheduled from the work item sub-screen) and **Manual Appointments** (free-form personal events). Surfaced as a dedicated screen, plus a Today's Schedule strip on the Dashboard and AI Briefing calendar awareness. For visit-type work items (Home Visit, Initial Contact, Placement Home Study, Initial Assessment, Safety Assessment, Eligibility Interview), the popup and the work item sub-screen expose a Visit Address panel + Get Directions hand-off to Google Maps / Waze / Apple Maps with traffic-aware travel estimates.

### Problem Statement

Field social workers today juggle their phone calendar, the Workspace, and a navigation app. They lose context switching between visit logistics and case work. Without a unified calendar, an in-context Visit Address panel, and one-tap directions, fieldwork is slower and error-prone.

### Importance

- Single calendar = one place to plan the day.
- Today's Schedule strip + AI Briefing calendar awareness = the cockpit shows visits in flow.
- Visit Address + Get Directions = mobile-first ergonomics for field staff.
- Hand-off to native apps respects the user's preferred navigation tool — no in-app GPS.

### High-Level Approach

- **Calendar screen** with Month + Week views (Week default), 7-column grids, "Today" navigation, color-coded legend.
- **Work Item Events** scheduled from the work item sub-screen "Schedule this work item on my calendar" control; clickable from calendar → Event Popup.
- **Manual Appointments** created from "Add Appointment" button or by clicking an empty cell / slot; full CRUD on calendar.
- **Today's Schedule strip** on Dashboard between AI Briefing and work-item list; shown only when ≥ 1 event today.
- **AI Briefing calendar awareness** — when events are present, briefing includes calendar summary + contextual recommendation.
- **Calendar Event Popup** is the unified click target — replaces direct sub-screen open. From the popup: open work item or edit appointment; visit-type popups show Visit Address + Get Directions.
- **Directions View** with traffic-aware estimates (light / moderate / heavy tiers) + launch buttons for Google Maps / Waze / Apple Maps using documented URL formats.
- **Visit-type work item list** maintained as a single configurable constant (currently: Home Visit, Initial Contact, Placement Home Study, Initial Assessment, Safety Assessment, Eligibility Interview).

---

## 3. 👥 Stakeholders and Personas

- **P1 — Social Worker** — sole persona using Calendar; sole persona seeing Visit Address + Get Directions.
- **MOD-01 Workspace UI** — hosts Today's Schedule strip slot.
- **MOD-06 AI Copilot** — provides AI Briefing calendar-aware content.

---

## 4. 🎬 Target Use Cases

- **Plan today:** Worker opens Dashboard → sees Today's Schedule strip with three appointments → clicks first → Calendar Event Popup → Open Work Item → completes pre-visit notes.
- **Schedule a visit:** Worker opens Home Visit work item → "Schedule this work item on my calendar" → picks date/time → confirmation; Work Item Event appears on calendar.
- **In the field:** Worker on mobile taps a Home Visit event → popup shows Visit Address (orange highlight) → "Get Directions" → Directions View with light/moderate/heavy estimates → taps "Open in Apple Maps" → device hands off to Apple Maps with destination pre-populated.
- **Personal appointment:** Worker clicks empty Tuesday 14:00 slot → Add Appointment modal → saves → appears purple on calendar; shows in Today's strip if today.
- **AI awareness:** Briefing notes "You have a Home Visit at 2 pm — consider completing the Safety Assessment documentation before you leave."

---

## 5. 📊 Success Conditions and Metrics

### Goals

1. Workers manage day in one calendar, not two.
2. Visits are findable, addressable, and navigable in two taps.
3. Directions hand-off works correctly to all three target apps on the device.
4. Today's strip + AI briefing calendar context surface useful signal without clutter.

### Non-Goals

- Calendar for Supervisor / Director / Admin personas.
- Two-way sync with external calendars (Google / Outlook / iCloud) — deferred.
- In-app turn-by-turn GPS — hand off to native apps only.
- Editing Work Item Events on the calendar — read-only on calendar; changes happen on the work item.
- Address authoring — addresses come from the case record.

### Success Metrics

| Goal | Metric |
| --- | --- |
| Field adoption | % of visit-type work items with a Work Item Event scheduled — target ≥ 60 % |
| Directions usage | Clicks on Get Directions per visit-type Work Item Event — target [TBD] |
| Today strip relevance | % of in-app sessions with at least one strip click when strip is shown |
| AI calendar context | % of briefings containing calendar summary when events exist — target = 100 % |

### Guardrails

- Calendar visible only to Social Workers.
- Visit Address shown only for visit-type work items with a known address.
- No PHI in URL parameters (master) — directions URLs use the address only.
- No silent calendar mutation (no auto-create / auto-delete).

---

## 6. 🤔 Assumptions

- Case record contains a usable street address for visit-type work items where applicable. — ❓ unvalidated
- Agency office address is configured per tenant for the directions origin. — ❓ unvalidated
- Mobile browser hand-off to Google Maps / Waze / Apple Maps via documented URL schemes works on iOS + Android. — ❓ unvalidated
- Traffic-tier mapping (light / moderate / heavy) by time-of-day is acceptable for v1 vs live traffic API. — ❓ unvalidated

---

## 7. 🔗 Dependencies and V-Model Gates

★

| Dependency | Type | Status | Notes |
| --- | --- | --- | --- |
| MOD-01 Workspace UI | Internal | DRAFT | Hosts Today's Schedule strip slot + Calendar Event Popup pattern. |
| MOD-06 AI Copilot | Internal | DRAFT | AI Briefing calendar awareness. |
| Base Traverse Case → Address mapping | Internal | PENDING | Hard blocker for Visit Address. |
| Tenant Office Address config | Internal | PENDING | Origin for directions. |
| Dev Lead feasibility | V-Model gate | PENDING | ★ |
| Estimation | V-Model gate | PENDING | ★ |

---

## 8. 📋 Requirements

★

| Priority | Job Story | Acceptance Criteria (G/W/T) | Jira | Source |
| --- | --- | --- | --- | --- |
| P1 | As a Social Worker, when I want to see all my time-bound commitments in one place, I want a Calendar screen unifying Work Item Events + Manual Appointments. | AC-1.1: *Given* the Worker sidebar, *Then* "Calendar" appears below "Work Items". AC-1.2: *Given* other personas, *Then* the Calendar is not visible. AC-1.3: *Given* the calendar opens, *Then* it defaults to Week view. AC-1.4: *Given* the calendar, *Then* both Work Item Events and Manual Appointments render. AC-1.5: *Given* the legend, *Then* it color-codes Work Item Events vs Manual Appointments. AC-1.6: *Given* an event chip, *Then* it shows time + title; Work Item Events also show client/case name. | [CAM-XXXXX] | CAL-001 |
| P1 | As a Social Worker, when I plan, I want Month + Week views with navigation + Today control so that I can pivot scope easily. | AC-2.1: *Given* the header, *Then* a Month/Week toggle is shown. AC-2.2: *Given* Month View, *Then* a 7-col multi-row grid renders with event chips truncated when needed and a "+N more" overflow indicator per cell. AC-2.3: *Given* Week View, *Then* a 7-col Sun–Sat grid with 8 am – 6 pm time slots renders; events occupy correct slot+day. AC-2.4: *Given* navigation arrows, *Then* I can move ± 1 month / ± 1 week. AC-2.5: *Given* a "Today" button, *Then* the calendar returns to today. AC-2.6: *Given* today, *Then* it is visually highlighted in both views. AC-2.7: *Given* my session, *Then* the chosen view persists. | [CAM-XXXXX] | CAL-002 |
| P1 | As a Social Worker, when I see Work Item Events on the calendar, I want them visually distinct, click-to-popup, and read-only on the calendar so that the work-item record stays the source of truth. | AC-3.1: *Given* a Work Item Event, *Then* it has a distinct visual style (e.g., blue background + case icon). AC-3.2: *Given* the chip, *Then* it shows time, work item name, client/case name. AC-3.3: *Given* a click (post DIR-001 Calendar Event Popup), *Then* the popup opens (NOT the sub-screen directly). AC-3.4: *Given* the calendar, *Then* Work Item Events cannot be created from the calendar — only from the Work Item sub-screen. AC-3.5: *Given* the calendar, *Then* Work Item Events are read-only — edit/delete from the work item only. | [CAM-XXXXX] | CAL-003 + DIR-001 alignment |
| P1 | As a Social Worker, when I am completing a work item, I want to schedule it on my calendar without losing task progress so that planning happens in flow. | AC-4.1: *Given* the Complete Task tab, *Then* a "Schedule this work item on my calendar" control is visible for all work item types. AC-4.2: *Given* I click it, *Then* an inline date + time picker expands. AC-4.3: *Given* a confirmation, *Then* a Work Item Event is created with item name, client name, and selected date/time, and an inline success message ("Added to calendar: …") is shown. AC-4.4: *Given* I change date/time, *Then* my task progress is preserved. AC-4.5: *Given* scheduling, *Then* it does not complete or advance the workflow. | [CAM-XXXXX] | CAL-004 |
| P1 | As a Social Worker, when I have personal time-bound events, I want to create / edit / delete Manual Appointments directly on the calendar so that I can manage my full day. | AC-5.1: *Given* the header, *Then* an "Add Appointment" button is available. AC-5.2: *Given* I click an empty day cell (Month) or empty time slot (Week), *Then* the Add Appointment modal opens pre-filled. AC-5.3: *Given* the form, *Then* it requires Title + Date + Time and accepts optional Notes. AC-5.4: *Given* save, *Then* the appointment renders with a distinct visual style (e.g., purple). AC-5.5: *Given* an existing appointment, *Then* clicking opens an Edit modal with all fields editable. AC-5.6: *Given* the Edit modal, *Then* a Delete button removes the appointment. AC-5.7: *Given* deletion, *Then* it is removed immediately within the session. AC-5.8: *Given* Manual Appointments, *Then* they are not linked to cases / work items / KPI rules. | [CAM-XXXXX] | CAL-005 |
| P1 | As a Social Worker, when I land on the Dashboard with events today, I want a Today's Schedule strip so that I see my plan in flow. | AC-6.1: *Given* ≥ 1 event today, *Then* the strip appears between the AI Briefing and the work-item list. AC-6.2: *Given* 0 events today, *Then* the strip is hidden. AC-6.3: *Given* events, *Then* they render as compact cards sorted ascending by time. AC-6.4: *Given* a card, *Then* it shows time, title, client/case (for Work Item Events). AC-6.5: *Given* visual coding, *Then* it matches the calendar legend. AC-6.6: *Given* a Work Item Event card click, *Then* the Calendar Event Popup opens (post DIR-001). AC-6.7: *Given* a Manual Appointment card, *Then* it opens the Calendar Event Popup (per DIR-001). | [CAM-XXXXX] | CAL-006 |
| P1 | As a Social Worker, when AI briefs me, I want calendar awareness so that the briefing helps me plan around appointments. | AC-7.1: *Given* events today, *Then* the briefing includes a calendar summary section. AC-7.2: *Given* the summary, *Then* it lists today's events with times and titles. AC-7.3: *Given* applicable context, *Then* the AI generates a contextual recommendation (e.g., "Home Visit at 2 pm — complete Safety Assessment first"). AC-7.4: *Given* visual treatment, *Then* the calendar section is clearly labelled and distinct from the KPI section. AC-7.5: *Given* 0 events today, *Then* the section is omitted. | [CAM-XXXXX] | CAL-007 |
| P1 | As a Social Worker, when I click any calendar event (anywhere), I want the Calendar Event Popup so that interactions are consistent. | AC-8.1: *Given* an event chip in Calendar (Month or Week), *When* I click, *Then* the popup opens — the legacy direct sub-screen / edit modal behavior is replaced. AC-8.2: *Given* a card in Today's Schedule, *When* I click, *Then* the popup opens. AC-8.3: *Given* the popup, *Then* it is centered modal overlay. AC-8.4: *Given* the header, *Then* its color band matches event type (blue Work Item, purple Manual). AC-8.5: *Given* the body, *Then* it shows event title, date/time, and event-type indicator. AC-8.6: *Given* a Work Item Event, *Then* it additionally shows case/client name, program, case ID. AC-8.7: *Given* a Manual Appointment, *Then* it shows notes if populated. AC-8.8: *Given* the X button, *Then* it closes the popup. AC-8.9: *Given* a Work Item Event, *Then* an "Open Work Item" button closes the popup and opens the sub-screen. AC-8.10: *Given* a Manual Appointment, *Then* an "Edit" button closes the popup and opens the Edit Appointment modal. | [CAM-XXXXX] | DIR-001 |
| P1 | As a Social Worker, when a Work Item Event is a visit-type item, I want a prominent Visit Address in the popup so that I see where I am going at a glance. | AC-9.1: *Given* a visit-type work item (Home Visit, Initial Contact, Placement Home Study, Initial Assessment, Safety Assessment, Eligibility Interview), *Then* a "Visit Address" section is shown in the popup. AC-9.2: *Given* the section, *Then* it shows client full name + case street address. AC-9.3: *Given* visual style, *Then* it is distinct (e.g., orange-tinted background). AC-9.4: *Given* a non-visit type, *Then* no address section is shown. AC-9.5: *Given* no address on file, *Then* no address section is shown. | [CAM-XXXXX] | DIR-002 |
| P1 | As a Social Worker, when a visit Work Item Event has an address, I want a Get Directions inline view inside the popup so that I do not stack modals. | AC-10.1: *Given* a visit-type Work Item Event with an address, *Then* a "Get Directions" button is visible in the popup. AC-10.2: *Given* the click, *Then* the popup transitions to a Directions View inside the same modal — no second overlay. AC-10.3: *Given* the Directions View, *Then* a "Back" button returns to Event Detail. AC-10.4: *Given* the X close, *Then* it remains accessible and closes the popup entirely. AC-10.5: *Given* the transition, *Then* underlying screen state does not reload or reset. | [CAM-XXXXX] | DIR-003 |
| P1 | As a Social Worker, when I am inside a visit-type work item sub-screen, I want a Visit Address panel + Get Directions so that I can navigate without leaving the task. | AC-11.1: *Given* the Complete Task tab on a visit-type work item, *Then* a Visit Address panel is visible. AC-11.2: *Given* the panel, *Then* it shows label + client name + street address. AC-11.3: *Given* "Get Directions", *Then* the Directions View opens as an overlay inside the work-item modal. AC-11.4: *Given* the overlay, *Then* it has a back/close control and dismissing it returns to the task. AC-11.5: *Given* opening / closing the directions, *Then* sub-screen + task progress are unaffected. | [CAM-XXXXX] | DIR-004 |
| P1 | As a Social Worker, when I open Directions, I want traffic-tier-aware estimates and one-tap launch in Google Maps / Waze / Apple Maps so that I plan and go. | AC-12.1: *Given* the Directions View header, *Then* it shows origin (agency office name + address) → destination (client name + address) in a clear "From → To" layout. AC-12.2: *Given* current time-of-day, *Then* a single estimated travel time is displayed for the active tier — light (off-peak), moderate (shoulder), heavy (AM 7–9 / PM 4–7 peaks). AC-12.3: *Given* the estimate, *Then* distance in miles is also displayed. AC-12.4: *Given* the comparison grid, *Then* it shows estimated travel times for all three tiers. AC-12.5: *Given* the active tier, *Then* it is visually highlighted in the comparison. AC-12.6: *Given* three navigation buttons, *Then* Google Maps / Waze / Apple Maps each open the respective app or mobile-web equivalent with destination pre-populated. AC-12.7: *Given* URL formats, *Then* Google = `maps.google.com/maps?saddr=[origin]&daddr=[destination]`, Waze = `waze.com/ul?q=[destination]`, Apple = `maps.apple.com/?daddr=[destination]`. AC-12.8: *Given* a launch click, *Then* it opens in a new tab or hands off to the native app on mobile. | [CAM-XXXXX] | DIR-005 |
| P1 | As a system, when determining which work items qualify for Visit Address + Directions UI, I want a single configurable list so that future additions are low-risk. | AC-13.1: *Given* the initial set, *Then* it is Home Visit, Initial Contact, Placement Home Study, Initial Assessment, Safety Assessment, Eligibility Interview. AC-13.2: *Given* a work item whose name matches the list, *Then* it qualifies for address + directions UI. AC-13.3: *Given* any other work item, *Then* the address/directions UI is hidden. AC-13.4: *Given* the list location, *Then* it lives in a single constant or configuration in code, supporting future additions without widespread changes. | [CAM-XXXXX] | DIR-006 |

---

## 9. ⚙️ NFRs

★ Inherits master §9. Module deltas:

**Performance:** Calendar render ≤ 1 s for typical month/week. Popup open ≤ 200 ms.

**Mobile-first:** Visit Address + Directions UI tested on iOS + Android mobile browsers. Hand-off URLs verified.

**Security:** Address values never sent in URL query strings beyond destination address (master no-PII-in-URL constraint applies; client name is not in the URL).

**Observability:** Telemetry: calendar_view_loaded, view_toggled, event_clicked, popup_opened, get_directions_clicked, maps_app_launched (per app), schedule_from_subscreen, manual_appointment_created.

---

## 10. 🎨 User Interaction and Design

- **Calendar header:** Month/Week toggle, navigation arrows, "Today", "Add Appointment", color-coded legend.
- **Event chip styles:** Blue (Work Item Event) vs purple (Manual Appointment).
- **Popup pattern:** Single-active modal; transitions to Directions View *inside* the modal — never opens a second overlay.
- **Visit Address panel:** Orange-tinted highlight in the popup and in the work-item sub-screen.
- **Directions View:** Origin → Destination header, large active-tier estimate, three-tier comparison grid, three launch buttons.

---

## 11. 🔄 Key Flows

### Flow 1: Schedule a visit work item

1. Worker opens Home Visit work item.
2. In Complete Task tab, clicks "Schedule this work item on my calendar".
3. Picks date + time → confirms.
4. Work Item Event created; success message inline; calendar updated.
**Exit:** Event visible on Calendar + Today's strip if today.

### Flow 2: Navigate to a visit (mobile)

1. Worker on mobile taps event in Today's strip.
2. Calendar Event Popup opens; Visit Address visible.
3. Worker taps "Get Directions" → in-modal Directions View.
4. Worker taps "Open in Apple Maps" → device hands off to Apple Maps with destination.
**Exit:** Native nav started; cockpit state intact on return.

### Flow 3: Personal appointment

1. Worker clicks an empty Tuesday 14:00 slot.
2. Add Appointment modal opens pre-filled.
3. Title + optional notes added → saves.
4. Appointment appears purple on calendar.
**Exit:** Manual Appointment created; not linked to any case or KPI.

---

## 12. 🚫 Out of Scope and Deferred

### Out of Scope

- Calendar for non-Worker personas.
- Work Item Event editing on the calendar.
- In-app GPS / live navigation.
- Address authoring.

### Deferred

- Two-way sync with Google / Outlook / iCloud calendars.
- Live traffic API (v1 uses time-of-day tiers).
- Per-program calendar overlays (e.g., on-call days).
- Reminders / push notifications for upcoming events (could leverage MOD-04).

---

## 13. 🧬 Traceability

| AC ID | Story | Summary | Gherkin | Test class | Verdict |
| --- | --- | --- | --- | --- | --- |
| AC-1.2 | [CAM-XXXXX] | Calendar visible only to Worker | `docs/{FEATURE-ID}/gherkin/cal-001-persona.feature` | --- | --- |
| AC-3.5 | [CAM-XXXXX] | Read-only Work Item Events on calendar | `docs/{FEATURE-ID}/gherkin/cal-003-readonly.feature` | --- | --- |
| AC-6.2 | [CAM-XXXXX] | Today strip hidden when 0 events | `docs/{FEATURE-ID}/gherkin/cal-006-empty.feature` | --- | --- |
| AC-8.1 | [CAM-XXXXX] | Calendar Event Popup unified click | `docs/{FEATURE-ID}/gherkin/dir-001-popup.feature` | --- | --- |
| AC-9.4 | [CAM-XXXXX] | Visit address only for visit types | `docs/{FEATURE-ID}/gherkin/dir-002-visit-only.feature` | --- | --- |
| AC-12.7 | [CAM-XXXXX] | Maps URL formats | `docs/{FEATURE-ID}/gherkin/dir-005-urls.feature` | --- | --- |
| AC-13.4 | [CAM-XXXXX] | Visit-type list single source | `docs/{FEATURE-ID}/gherkin/dir-006-constant.feature` | --- | --- |

---

## 14. 🚀 Launch Plan

| Target | Milestone | Description | Exit |
| --- | --- | --- | --- |
| [YYYY-MM-DD] | ✅ UAT | Worker pilot | Mobile hand-off verified across iOS + Android |
| [YYYY-MM-DD] | 🛑 Early Access | Pilot tenant | Adoption metric trending positive |
| [YYYY-MM-DD] | 🛑 Launch | All tenants | All NFRs met |

### Operation Checklist

- [ ] Tenant office address configured per tenant
- [ ] Visit-type work-item list defined as single constant
- [ ] Mobile hand-off tested on real devices
- [ ] Telemetry live
- [ ] AI Briefing calendar contract verified with MOD-06

---

## 15. 📣 Cross-Functional Impact Checklist

| Team | Y/N | Action |
| --- | --- | --- |
| Configuration | Y | Tenant office address. |
| Permissions | Y | Calendar persona gating. |
| Customer Success | Y | Field-worker mobile playbook. |
| Analytics | Y | Calendar telemetry. |

---

## 16. ⚠️ Risks

| Risk | L/I | Mitigation |
| --- | --- | --- |
| Mobile hand-off URL formats break on a vendor update | M / M | Test matrix on iOS + Android before each release; fallback to mobile web. |
| Address missing → empty Visit Address surface | M / M | Hide section gracefully when no address (AC-9.5); telemetry on missing-address frequency. |
| Traffic-tier estimates surprise users vs live conditions | M / M | Label the tier explicitly; document that this is a heuristic; live traffic API is deferred. |
| Calendar conflicts with personal calendar (no sync) | M / M | Communicated as v1 limitation; sync deferred. |
| Manual Appointment data unprotected (no PHI flag) | L / M | Manual appointments are personal; no special masking; documented. |

---

## 17. ❓ Open Questions

| Question | Impact | Date | Owner |
| --- | --- | --- | --- |
| Confirm tenant office address source (Admin config or base Traverse). | Blocks AC-12.1. | 2026-04-25 | Product |
| Confirm whether visit-type list lives in code constant or MOD-07 admin config (DIR-006 #4 hints "future" config). | Affects v1 scoping. | 2026-04-25 | Product |
| Confirm address field on the Case record (single line vs structured). | Blocks AC-9.2. | 2026-04-25 | Dev Lead |
| Confirm time-of-day tier boundaries for "shoulder" and "off-peak" (DIR-005 #2 specifies peaks only). | Blocks AC-12.2. | 2026-04-25 | Product |
| Confirm whether Today's strip should also surface non-event KPI urgency (out of scope here?). | Cross-module clarity. | 2026-04-25 | Product |

---

## 18. 💬 FAQs

**Q:** Why hand off to native apps instead of in-app GPS?
**A:** Workers prefer their own navigation tool; in-app GPS would require its own ongoing UX investment and not match user habit.

**Q:** Why are Work Item Events read-only on the calendar?
**A:** The work item is the source of truth; calendar mutations would create reconciliation problems.

**Q:** Why are Manual Appointments user-only?
**A:** They are personal; no case or KPI implications, no team sharing in v1.

---

## 19. 🧭 Decision Log

| # | Decision | Rationale | Date | By |
| --- | --- | --- | --- | --- |
| 1 | Calendar visible only to Social Worker. | Persona definition + scope discipline. | 2026-04-25 | Product |
| 2 | Hand off to Google / Waze / Apple via documented URLs; no in-app GPS. | User preference; reduces v1 scope. | 2026-04-25 | Product |
| 3 | Calendar Event Popup is the single click target — replaces direct sub-screen open / edit modal. | Source DIR-001; consistent UX. | 2026-04-25 | Product |
| 4 | Visit-type list as a single constant for v1; future MOD-07 admin config deferred. | Source DIR-006 #4. | 2026-04-25 | Product |

---

## 20. 📝 Change Log

| Date | By | Sections | Reason |
| --- | --- | --- | --- |
| 2026-04-25 | Kendrew Peacey | All | Initial draft — derived from §§21, 22. |

---

## 21. 🧾 Informed By

★

| Document | Path | SHA-256 |
| --- | --- | --- |
| Master PRD | [[PRD - Northwoods Traverse Workspace]] | sha256:[at approval] |
| Source PRD | `00 - Inbox/Northwoods_Traverse_PRD_v2.0.docx` (§§21, 22) | sha256:[at approval] |

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
