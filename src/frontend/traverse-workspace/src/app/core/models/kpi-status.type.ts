// src/app/core/models/kpi-status.type.ts
//
// KpiStatus — string union type for KPI indicator status values.
//
// Maps directly to the --traverse-status-* CSS custom properties defined in theme.scss.
// Using a closed string union (rather than an enum) allows direct template string
// interpolation for CSS class names without a mapping function, and the compiler
// statically rejects any invalid status value at the call site.
//
// Values used by: KpiStatusBadgeComponent (AC-026), DashboardComponent (AC-031).
// Token source: src/styles/theme.scss §Traverse KPI Status Tokens (AC-007).

/**
 * The six KPI status values supported by the Traverse design system.
 * Each value maps to a --traverse-status-{value} CSS custom property.
 *
 * - green:   Active / On Track
 * - yellow:  Warning / Approaching threshold
 * - red:     Breached / Critical
 * - fuchsia: Secondary alert / Escalation
 * - emerald: Positive / Completed
 * - slate:   Placeholder / Unknown (used by Phase 0 dashboard tiles)
 */
export type KpiStatus = 'green' | 'yellow' | 'red' | 'fuchsia' | 'emerald' | 'slate';
