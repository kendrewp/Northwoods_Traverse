// src/app/features/auth/login-placeholder.component.ts
//
// LoginPlaceholderComponent — Phase 0 authentication placeholder page.
//
// Phase 0 stub — remove in Phase 1 Auth story when real IdP is wired.
//
// WHY THIS EXISTS: authGuard redirects unauthenticated users to '/login'.
// Without a component at that route, the redirect causes a route error.
// In Phase 0, AuthService.isAuthenticated() always returns true (stub), so
// this component is never shown in normal dev usage. It exists to:
// 1. Provide a valid route target for the authGuard redirect path.
// 2. Give developers a fallback UI if they manually navigate to /login.
// 3. Establish the LoginComponent's location for Phase 1 to replace.
//
// The "Continue to Dashboard" button bypasses auth (dev-mode only) by
// navigating directly to the dashboard. Phase 1 removes this button
// and replaces the card content with the real IdP login flow.
//
// AC-028: standalone, OnPush

import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { AppRoutes } from '@core/routes/app-routes';

/**
 * Placeholder login page — displayed when authGuard redirects to /login.
 *
 * Phase 0 stub: shows a "not yet configured" message and a dev-mode button
 * to navigate to dashboard without authentication.
 * Remove this component in Phase 1 Auth story.
 */
// Phase 0 stub — remove in Phase 1 Auth story
@Component({
  selector: 'app-login-placeholder',
  standalone: true,
  imports: [MatCardModule, MatButtonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="login-container">
      <mat-card class="login-card">
        <mat-card-header>
          <mat-card-title>Traverse Workspace</mat-card-title>
          <mat-card-subtitle>Authentication not yet configured (Phase 0)</mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <p>Real authentication (OpenID Connect / IdP integration) will be
          implemented in Phase 1. For now, use the button below to access
          the application in development mode.</p>
        </mat-card-content>
        <mat-card-actions>
          <button
            mat-raised-button
            color="primary"
            (click)="proceed()"
          >
            Continue to Dashboard (Dev Mode)
          </button>
        </mat-card-actions>
      </mat-card>
    </div>
  `,
  styles: [`
    .login-container {
      display: flex;
      align-items: center;
      justify-content: center;
      min-height: 100vh;
      background-color: var(--mat-sys-surface-container-low);
    }
    .login-card {
      max-width: 480px;
      width: 100%;
      padding: 8px;
    }
  `],
})
export class LoginPlaceholderComponent {
  private readonly router = inject(Router);

  /**
   * Phase 0 only — navigates to dashboard without authentication.
   *
   * In Phase 1 Auth story: replace this method with the IdP login redirect.
   * The button itself will be replaced by an IdP-provided login widget or
   * redirect to the OIDC authorization endpoint.
   */
  // Phase 0 stub — replace in Phase 1 Auth story
  proceed(): void {
    this.router.navigate([AppRoutes.dashboard]);
  }
}
