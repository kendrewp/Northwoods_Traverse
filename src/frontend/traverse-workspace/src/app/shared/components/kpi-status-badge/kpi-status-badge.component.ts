// src/app/shared/components/kpi-status-badge/kpi-status-badge.component.ts
//
// KpiStatusBadgeComponent — renders a coloured chip indicating KPI status.
//
// PRESENTATIONAL COMPONENT (SRP): No service injection. Two inputs: status
// (required) and label (optional). Applies a CSS class per status value to
// pick up the corresponding --traverse-status-{status} token.
//
// CSS CLASS STRATEGY: Rather than ngStyle binding to the token directly, a
// CSS class suffix (`kpi-badge--{status}`) is applied. This is safer for
// Angular strict template checking — `status()` is typed as KpiStatus, which
// is a closed union. The class string interpolation is XSS-safe because
// KpiStatus only contains 6 fixed string literals and no user input reaches it.
//
// COLOURS: Component uses --traverse-status-* custom properties (defined in
// theme.scss) for backgrounds. White (#fff) text is hardcoded because the
// --traverse-status-* values are chosen for AA contrast against white.
// Using --mat-sys-on-primary-container here would not work — the status
// colours are not Material palette colours.
//
// EXCEPTION: The `color: #fff` in styles is a deliberate exception to the
// no-hardcoded-colours rule (AC-009) because the status tokens are defined
// as accessible foreground colours on white backgrounds, and the inverse
// (white text on the status colour as background) is also AA-contrast-safe.
// This is documented in the design §4.3.
//
// DISABLERIPPLE: Chips are non-interactive — disableRipple prevents the hover
// state from implying interactivity.
//
// AC-026: status = input.required<KpiStatus>(), label = input<string>('')
// AC-027: KpiStatus type used from core/models
// AC-028: standalone, OnPush

import {
  ChangeDetectionStrategy,
  Component,
  input,
} from '@angular/core';
import { MatChipsModule } from '@angular/material/chips';
import { KpiStatus } from '@core/models/kpi-status.type';

/**
 * Shared KPI status badge — a coloured chip indicating a KPI indicator's status.
 *
 * The chip colour is driven by the --traverse-status-{status} CSS custom property
 * defined in theme.scss. No hardcoded colours except white text (see file header).
 */
@Component({
  selector: 'app-kpi-status-badge',
  standalone: true,
  imports: [MatChipsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <mat-chip
      [class]="'kpi-badge kpi-badge--' + status()"
      disableRipple
    >
      {{ label() || status() }}
    </mat-chip>
  `,
  styles: [`
    /* Base badge styles — size and border-radius */
    .kpi-badge {
      font-size: 12px;
      font-weight: 500;
      border-radius: 4px;
      cursor: default;
    }

    /* Status-specific colours using --traverse-status-* tokens from theme.scss.
       White text (#fff) is explicitly acceptable here — see file header for rationale. */
    .kpi-badge--green   { background: var(--traverse-status-green);   color: #fff; }
    .kpi-badge--yellow  { background: var(--traverse-status-yellow);  color: #fff; }
    .kpi-badge--red     { background: var(--traverse-status-red);     color: #fff; }
    .kpi-badge--fuchsia { background: var(--traverse-status-fuchsia); color: #fff; }
    .kpi-badge--emerald { background: var(--traverse-status-emerald); color: #fff; }
    .kpi-badge--slate   { background: var(--traverse-status-slate);   color: #fff; }
  `],
})
export class KpiStatusBadgeComponent {
  /**
   * The KPI status value. Determines badge colour.
   * Must be one of the 6 KpiStatus values defined in kpi-status.type.ts.
   */
  status = input.required<KpiStatus>();

  /**
   * Optional display label. When omitted, the status value itself is displayed
   * (e.g., 'slate', 'green'). Phase 1+ feature stories provide meaningful labels.
   */
  label = input<string>('');
}
