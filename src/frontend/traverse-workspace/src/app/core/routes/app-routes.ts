// src/app/core/routes/app-routes.ts
//
// AppRoutes — typed route map for all 12 PRD modules + auth routes.
//
// PURPOSE: Centralises all route path strings as typed constants so that
// programmatic navigation and routerLink bindings never hardcode string paths.
// Using `as const` makes each value a string literal type — the compiler will
// catch typos at every call site.
//
// SOLID DIP: Guards, interceptors, and components depend on this constant
// (an abstraction) rather than embedding route strings directly. Adding new
// routes in Phase 1+ stories only requires adding an entry here.
//
// DYNAMIC HELPERS: Routes that include parameters (caseDetail, workItemDetail)
// are expressed as arrow functions rather than template literals at the top level.
// This avoids the need for formatters at each call site while keeping the
// parameter contract explicit.
//
// AC-020, AC-021, AC-022

/**
 * Typed route constants for all Traverse application pages.
 * Use for programmatic navigation: `router.navigate([AppRoutes.dashboard])`
 * and template bindings: `[routerLink]="AppRoutes.cases"`.
 */
export const AppRoutes = {
  // ─── Auth ──────────────────────────────────────────────────────────────────
  /** Login page — authGuard redirects here when not authenticated. */
  login:              '/login',
  /** Unauthorized page — adminGuard redirects here when role is insufficient. */
  unauthorized:       '/unauthorized',

  // ─── Phase 0 Shell ─────────────────────────────────────────────────────────
  /** Main dashboard with 6 KPI placeholder tiles (Phase 0). */
  dashboard:          '/dashboard',

  // ─── Phase 1 — Core Engines (placeholder paths) ────────────────────────────
  /** Workflow Execution Engine (MOD-02). */
  workflow:           '/workflow',
  /** KPI / SLA Policy Engine (MOD-03). */
  kpi:                '/kpi',
  /** Admin Configuration Suite (MOD-07). */
  admin:              '/admin',

  // ─── Phase 2 — UI Shell ─────────────────────────────────────────────────────
  /** Case Management — case list (MOD-01). */
  cases:              '/cases',
  /** Work Items list (MOD-01). */
  workItems:          '/work-items',

  // ─── Phase 3 — Operational Layers ──────────────────────────────────────────
  /** Notifications centre (MOD-04). */
  notifications:      '/notifications',
  /** Reporting dashboard (MOD-05). */
  reporting:          '/reporting',

  // ─── Phase 4 — AI Copilot ───────────────────────────────────────────────────
  /** AI Copilot interface (MOD-06). */
  aiCopilot:          '/ai-copilot',

  // ─── Phase 5 — Advanced Features ────────────────────────────────────────────
  /** Compliance monitoring (MOD-10). */
  compliance:         '/compliance',
  /** Supervisor exception actions (MOD-11). */
  supervisorActions:  '/supervisor',
  /** Global search (MOD-08). */
  search:             '/search',
  /** Calendar / scheduling (MOD-09). */
  calendar:           '/calendar',

  // ─── Phase 6 — State Extension ──────────────────────────────────────────────
  /** Deputy Director dashboard (MOD-12). */
  deputyDashboard:    '/deputy',

  // ─── Dynamic Path Helpers ───────────────────────────────────────────────────
  // These functions produce route paths with embedded IDs.
  // Usage: `router.navigate([AppRoutes.caseDetail(case.id)])`

  /** Route to a specific case detail page.  Example: '/cases/abc-123' */
  caseDetail:         (id: string): string => `/cases/${id}`,
  /** Route to a specific work item detail page.  Example: '/work-items/xyz-456' */
  workItemDetail:     (id: string): string => `/work-items/${id}`,
} as const;
