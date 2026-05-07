// src/app/core/services/auth.service.ts
//
// AuthService — provides the authentication contract for the entire application.
//
// PHASE 0 STUB: The implementation below uses hardcoded dev values.
// The public API (isAuthenticated, hasRole, getToken, getCurrentRole, logout) is the
// stable contract. Phase 1 Auth story replaces the internals with real IdP integration
// without changing the method signatures. All callers (guards, interceptors, shell)
// depend on this contract, not the implementation.
//
// SOLID ISP: AuthService exposes only the 4 methods that callers actually need.
// The nav shell reads getCurrentRole(). Guards read isAuthenticated() and hasRole().
// The interceptor reads getToken(). No monolithic interface that forces callers to
// depend on methods they don't use.
//
// SOLID DIP: Guards import AuthService (this class, a concrete service). In Phase 1+,
// if an IAuthService interface is introduced for testing, guards can depend on the
// token instead. For Phase 0, the concrete class is acceptable (no test doubles needed).
//
// SIGNALS: Internal state is held as signals so components and computed() derivations
// react automatically when state changes (e.g., logout sets _isAuthenticated to false,
// which AppShellComponent's currentRole() computed signal re-evaluates).
//
// INJECT PATTERN: Router is injected as a class field initializer (not in a method body).
// Angular's inject() is only valid in synchronous injection contexts: constructors,
// field initializers, and factory functions. Since logout() is a user-action method,
// the Router must be captured at construction time via a field initializer and then
// accessed by reference in the method body.
//
// AC-016: isAuthenticated(), hasRole(), getToken(), getCurrentRole()

import { Injectable, inject, signal } from '@angular/core';
import { Router } from '@angular/router';

/**
 * Authentication service — provides auth state, role checking, and logout.
 *
 * Phase 0 stub: returns hardcoded dev values.
 * Replace internals in Phase 1 Auth story — do not change the public API.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  // Router injected as a field for use in logout() — avoids inject() call
  // inside a non-injection-context method body
  private readonly router = inject(Router);

  // ─── Private State Signals ─────────────────────────────────────────────────
  // Phase 0 stub — returns hardcoded dev values.
  // Replace these with real IdP session state in Phase 1 Auth story.

  /**
   * Whether the current user has an active authenticated session.
   * Phase 0 stub: always true for developer convenience.
   */
  // Phase 0 stub — replace in Phase 1 Auth story
  private readonly _isAuthenticated = signal<boolean>(true);

  /**
   * All roles assigned to the current user.
   * Phase 0 stub: dev persona has both social-worker and admin roles.
   */
  // Phase 0 stub — replace in Phase 1 Auth story
  private readonly _roles = signal<string[]>(['social-worker', 'admin']);

  /**
   * The currently active persona/role for nav display.
   * Phase 0 stub: social-worker is the default dev persona.
   */
  // Phase 0 stub — replace in Phase 1 Auth story
  private readonly _currentRole = signal<string>('social-worker');

  // ─── Public API ───────────────────────────────────────────────────────────��

  /**
   * Returns whether the user has an active authenticated session.
   *
   * Phase 0 stub: always returns true.
   * Phase 1+: validates session by calling /api/auth/me and checking the
   * server-set HttpOnly cookie (Angular cannot read HttpOnly cookie values directly).
   */
  isAuthenticated(): boolean {
    return this._isAuthenticated();
  }

  /**
   * Returns whether the current user has the specified role.
   *
   * Phase 0 stub: 'admin' returns true; all other roles return true (dev has both).
   * Phase 1+: reads roles from the JWT claims validated by the IdP.
   *
   * @param role - The role name to check (e.g., 'admin', 'social-worker').
   */
  hasRole(role: string): boolean {
    return this._roles().includes(role);
  }

  /**
   * Returns the bearer token for attaching to API calls.
   *
   * Phase 0 stub: always returns null. authInterceptor passes requests through
   * unauthenticated (correct behaviour in Phase 0 — no backend auth is wired).
   * Phase 1+: reads the token from the server-validated session.
   *
   * @returns The bearer token string, or null if no active session.
   */
  // Phase 0 stub — replace in Phase 1 Auth story
  getToken(): string | null {
    return null;
  }

  /**
   * Returns the user's active persona/role for display in the nav shell.
   *
   * Phase 0 stub: always returns 'social-worker'.
   * Phase 1+: reads the active persona from the session / user preferences.
   */
  getCurrentRole(): string {
    return this._currentRole();
  }

  /**
   * Logs the user out — clears authenticated state and navigates to /login.
   *
   * Phase 0 stub: sets _isAuthenticated to false and navigates.
   * Phase 1+: calls the IdP logout endpoint to invalidate the server session
   * and clear the HttpOnly session cookie before navigating.
   */
  // Phase 0 stub — replace in Phase 1 Auth story
  logout(): void {
    this._isAuthenticated.set(false);
    this.router.navigate(['/login']);
  }
}
