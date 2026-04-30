// src/app/shared/components/loading-spinner/loading-spinner.component.ts
//
// LoadingSpinnerComponent — overlay spinner for async load states.
//
// PRESENTATIONAL COMPONENT (SRP): No service injection. One boolean input.
// Shows a centred spinner overlay when isLoading is true; renders nothing otherwise.
//
// OVERLAY PATTERN: The spinner uses `position: absolute; inset: 0` to cover the
// nearest `position: relative` ancestor. The parent component must add
// `position: relative` to the container that should be covered during loading.
// Using absolute positioning (not fixed) keeps the overlay scoped to the card/panel
// being loaded rather than blocking the entire viewport.
//
// ARIA: `role="status" aria-label="Loading" aria-live="polite"` on the overlay div
// ensures screen readers announce when loading begins and ends. `polite` is correct
// here — the announcement should not interrupt the user's current action.
//
// BACKGROUND: Semi-transparent white (rgba token via CSS) provides visual indication
// that content is loading without completely hiding the stale content beneath.
// `z-index: 10` ensures the overlay renders above card content but below dialogs.
//
// AC-025: isLoading = input<boolean>(false)
// AC-028: standalone, OnPush

import {
  ChangeDetectionStrategy,
  Component,
  input,
} from '@angular/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

/**
 * Shared loading spinner — absolute-positioned overlay with a Material spinner.
 *
 * The parent element should have `position: relative` for the overlay to
 * correctly cover the loading area.
 */
@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  imports: [MatProgressSpinnerModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (isLoading()) {
      <div
        class="spinner-overlay"
        role="status"
        aria-label="Loading"
        aria-live="polite"
      >
        <mat-spinner diameter="48" />
      </div>
    }
  `,
  styles: [`
    .spinner-overlay {
      position: absolute;
      inset: 0;
      display: flex;
      align-items: center;
      justify-content: center;
      background: rgba(255, 255, 255, 0.6);
      z-index: 10;
    }
  `],
})
export class LoadingSpinnerComponent {
  /**
   * Controls spinner visibility.
   * When true: displays the full-overlay spinner.
   * When false (default): renders nothing.
   */
  isLoading = input<boolean>(false);
}
