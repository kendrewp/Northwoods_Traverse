// src/app/shared/components/page-header/page-header.component.ts
//
// PageHeaderComponent — displays a page title and optional subtitle.
//
// PRESENTATIONAL COMPONENT (SRP): No service injection, no side effects.
// Accepts inputs; emits no outputs. Always yields the same output given the
// same inputs. (Angular Coding Standards §3 — presentational component rules.)
//
// SOLID ISP: Two focused inputs (title, subtitle). Does not carry navigation
// or action buttons — those belong in the smart page component that uses it.
//
// STYLES: Uses --mat-sys-on-surface and --mat-sys-on-surface-variant tokens
// for text colours (no hardcoded hex — AC-009). Border uses --mat-sys-outline-variant.
//
// AC-023: title = input.required<string>(), subtitle = input<string>('')
// AC-028: standalone, OnPush

import {
  ChangeDetectionStrategy,
  Component,
  input,
} from '@angular/core';

/**
 * Shared page header — renders a page title and optional subtitle.
 *
 * Used at the top of every feature page to provide consistent heading styles.
 * The subtitle field is optional; when omitted (or empty string), no subtitle
 * element is rendered.
 */
@Component({
  selector: 'app-page-header',
  standalone: true,
  imports: [],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <header class="page-header">
      <h1 class="page-title">{{ title() }}</h1>
      @if (subtitle()) {
        <p class="page-subtitle">{{ subtitle() }}</p>
      }
    </header>
  `,
  styles: [`
    .page-header {
      padding-bottom: 16px;
      border-bottom: 1px solid var(--mat-sys-outline-variant);
      margin-bottom: 16px;
    }
    .page-title {
      font-size: 20px;
      font-weight: 500;
      margin: 0;
      color: var(--mat-sys-on-surface);
    }
    .page-subtitle {
      font-size: 13px;
      color: var(--mat-sys-on-surface-variant);
      margin: 4px 0 0;
    }
  `],
})
export class PageHeaderComponent {
  /**
   * The page title text. Required — every page must have a title.
   * Rendered as an H1 element (one H1 per page for accessibility).
   */
  title = input.required<string>();

  /**
   * Optional subtitle text displayed below the title in a smaller, muted style.
   * Defaults to empty string — when falsy, the subtitle element is not rendered.
   */
  subtitle = input<string>('');
}
