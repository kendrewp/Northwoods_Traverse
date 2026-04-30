// src/app/core/guards/admin.guard.ts
//
// adminGuard — protects routes from non-admin access.
//
// RESPONSIBILITY (SRP): This guard has one responsibility — checking whether the user
// is authenticated AND has the 'admin' role.
//
// FUNCTIONAL GUARD (OCP): Same pattern as authGuard — CanActivateFn arrow function.
//
// THREE-WAY BRANCHING:
// 1. Authenticated + admin role: allow navigation (return true)
// 2. Not authenticated: redirect to /login?returnUrl=... (same as authGuard)
// 3. Authenticated but not admin: redirect to /unauthorized
//    This distinction matters because the user has a valid session but lacks permission.
//    Sending them to /login would create a confusing loop (they would log in and hit the
//    same redirect). Instead, /unauthorized informs them of the access restriction.
//
// AC-018, AC-019

import { CanActivateFn } from '@angular/router';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '@core/services/auth.service';
import { AppRoutes } from '@core/routes/app-routes';

/**
 * Route guard that restricts access to admin users only.
 *
 * Returns `true` for authenticated admin users. Redirects unauthenticated
 * users to /login. Redirects authenticated non-admin users to /unauthorized.
 */
export const adminGuard: CanActivateFn = (_, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isAuthenticated() && auth.hasRole('admin')) {
    return true;
  }

  if (!auth.isAuthenticated()) {
    // Not logged in — redirect to login with return URL
    return router.createUrlTree([AppRoutes.login], {
      queryParams: { returnUrl: state.url },
    });
  }

  // Authenticated but insufficient role — redirect to unauthorized page
  return router.createUrlTree([AppRoutes.unauthorized]);
};
