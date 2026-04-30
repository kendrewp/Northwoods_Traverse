// src/app/shared/components/error-display/error-display.component.ts
//
// ErrorDisplayComponent — renders an error message in a Material card.
//
// PRESENTATIONAL COMPONENT (SRP): No service injection. Accepts an error input
// (either a string message or an ApiError). Derives the display message via a
// computed signal — no imperative logic required.
//
// ERROR UNION TYPE: The input accepts `string | ApiError` because both types appear
// in the codebase — string for simple one-off messages, ApiError for structured HTTP
// errors from errorInterceptor. The computed errorMessage signal handles both without
// requiring the caller to pre-process the error. (SOLID ISP: the component accepts the
// minimum viable union rather than forcing callers to always wrap strings.)
//
// COMPUTED SIGNAL: `errorMessage` is computed from the `error` input signal. Angular's
// OnPush change detection will re-run the computed when the input changes, without
// requiring any manual subscription or template logic.
//
// ROLE="ALERT": The mat-card has `role="alert"` so screen readers announce the error
// when it appears (WAI-ARIA live region semantics — Angular Coding Standards §14).
//
// AC-024: error = input.required<string | ApiError>()
// AC-028: standalone, OnPush

import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
} from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { ApiError } from '@core/models/api-error';

/**
 * Shared error display — renders a Material card with an error message.
 *
 * Accepts either a plain string or an ApiError. The component extracts
 * the most useful display message from either type.
 */
@Component({
  selector: 'app-error-display',
  standalone: true,
  imports: [MatCardModule, MatIconModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <mat-card class="error-card" role="alert">
      <mat-card-content class="error-content">
        <mat-icon class="error-icon" color="warn">error_outline</mat-icon>
        <span class="error-message">{{ errorMessage() }}</span>
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    .error-card {
      border-left: 4px solid var(--mat-sys-error);
      margin: 8px 0;
    }
    .error-content {
      display: flex;
      align-items: flex-start;
      gap: 12px;
    }
    .error-icon {
      flex-shrink: 0;
      margin-top: 2px;
    }
    .error-message {
      font-size: 14px;
      color: var(--mat-sys-on-surface);
      line-height: 1.5;
    }
  `],
})
export class ErrorDisplayComponent {
  /**
   * The error to display. Can be:
   * - A plain string message.
   * - An ApiError wrapping a ProblemDetails structure from the API.
   */
  error = input.required<string | ApiError>();

  /**
   * Derived display message — extracts the most useful text from the error.
   *
   * For string: used directly.
   * For ApiError: uses problem.detail (most specific), falls back to problem.title,
   * falls back to a generic message.
   *
   * This is a computed signal rather than a method call so Angular's OnPush change
   * detection can efficiently memoize the result.
   */
  protected readonly errorMessage = computed<string>(() => {
    const e = this.error();
    if (typeof e === 'string') {
      return e;
    }
    return e.problem.detail ?? e.problem.title ?? 'An error occurred.';
  });
}
