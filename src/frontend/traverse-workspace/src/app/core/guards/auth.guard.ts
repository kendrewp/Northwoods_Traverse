// src/app/core/guards/auth.guard.ts
//
// authGuard — protects routes from unauthenticated access.
//
// RESPONSIBILITY (SRP): This guard has one responsibility — checking whether the user
// is authenticated and redirecting to /login with returnUrl if not.
//
// FUNCTIONAL GUARD (OCP): Implemented as a CanActivateFn arrow function, not a class.
// Angular 15+ functional guards are preferred over class guards because they are simpler,
// tree-shakable, and avoid the need for `providedIn: 'root'` or module registration.
// New guards can be composed by combining existing guards without modifying them (OCP).
//
// RETURN URL: When redirecting to /login, the attempted URL is added as a `returnUrl`
// query parameter. The Phase 1 Login component will read this and navigate to it
// after successful authentication.
//
// AC-017, AC-019

import { CanActivateFn } from '@angular/router';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '@core/services/auth.service';
import { AppRoutes } from '@core/routes/app-routes';

/**
 * Route guard that blocks unauthenticated navigation.
 *
 * Returns `true` for authenticated users. Returns a UrlTree redirecting to
 * /login?returnUrl=<attempted-url> for unauthenticated users.
 */
export const authGuard: CanActivateFn = (_, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isAuthenticated()) {
    return true;
  }

  // Redirect to login with the attempted URL so the user can return after auth
  return router.createUrlTree([AppRoutes.login], {
    queryParams: { returnUrl: state.url },
  });
};
