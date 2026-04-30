// src/app/features/auth/unauthorized.component.ts
//
// UnauthorizedComponent — displayed when adminGuard blocks access due to
// insufficient role permissions.
//
// This component is a permanent fixture (not a Phase 0 stub that gets removed).
// It serves the '/unauthorized' route which adminGuard redirects to for
// authenticated users who lack the required role. Its content may be enhanced
// in Phase 1+ (better error messaging, contact admin link) but the component
// itself remains.
//
// AC-028: standalone, OnPush

import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { AppRoutes } from '@core/routes/app-routes';

/**
 * Unauthorized page — displayed when a user navigates to a route that
 * requires a role they do not have.
 *
 * Provides a clear "Access Denied" message and a link back to the dashboard.
 */
@Component({
  selector: 'app-unauthorized',
  standalone: true,
  imports: [MatCardModule, MatButtonModule, MatIconModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="unauthorized-container">
      <mat-card class="unauthorized-card">
        <mat-card-header>
          <mat-icon mat-card-avatar class="warn-icon">lock</mat-icon>
          <mat-card-title>Access Denied</mat-card-title>
          <mat-card-subtitle>You do not have permission to access this page.</mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <p>If you believe you should have access to this area, please contact
          your system administrator.</p>
        </mat-card-content>
        <mat-card-actions>
          <button
            mat-raised-button
            color="primary"
            (click)="goToDashboard()"
          >
            Go to Dashboard
          </button>
        </mat-card-actions>
      </mat-card>
    </div>
  `,
  styles: [`
    .unauthorized-container {
      display: flex;
      align-items: center;
      justify-content: center;
      min-height: 100vh;
      background-color: var(--mat-sys-surface-container-low);
    }
    .unauthorized-card {
      max-width: 480px;
      width: 100%;
      padding: 8px;
    }
    .warn-icon {
      color: var(--mat-sys-error);
    }
  `],
})
export class UnauthorizedComponent {
  private readonly router = inject(Router);

  /**
   * Navigates the user back to the dashboard.
   *
   * This is the safe fallback — the user cannot access their intended destination
   * but can still use the parts of the application their role permits.
   */
  goToDashboard(): void {
    this.router.navigate([AppRoutes.dashboard]);
  }
}
