// src/app/core/routes/app.routes.ts
//
// APP_ROUTES — Angular router route definitions (lazy-loaded).
//
// This is the full Phase 0 route table. The Phase 4 empty-array stub has been
// replaced with the complete route configuration.
//
// ROUTE STRUCTURE:
//
//   '' (root)  → AppShellComponent (layout shell, canActivate: authGuard)
//     ''       → redirect to 'dashboard'
//     'dashboard' → lazy-load DashboardComponent
//
//   'login'        → lazy-load LoginPlaceholderComponent (no guard — must be accessible
//                    to unauthenticated users so authGuard can redirect here)
//   'unauthorized' → lazy-load UnauthorizedComponent (no guard — accessible to
//                    authenticated users who lack the required role)
//   '**'           → redirect to 'dashboard' (authenticated wildcard fallback)
//
// LAZY LOADING: All components use loadComponent() with dynamic import() so they
// are only downloaded when the route is first accessed. This keeps the initial bundle
// small (the component code is in separate lazy chunks).
//
// BLOCKING INITIAL NAVIGATION: provideRouter is configured with
// withEnabledBlockingInitialNavigation() in app.config.ts. This ensures the router
// completes the initial navigation before Angular renders, preventing a flash of
// unauthenticated content on page load.
//
// PHASE 1+ ROUTES: As each Phase 1+ story implements its feature, it adds its route
// here. The authGuard and/or adminGuard are applied per the feature's access level.
//
// AC-012: APP_ROUTES used in provideRouter in app.config.ts

import { Routes } from '@angular/router';
import { AppShellComponent } from '@core/shell/app-shell.component';
import { authGuard } from '@core/guards/auth.guard';

/**
 * Application route table — Phase 0 routes only.
 *
 * Phase 1+ stories add routes to this array as their features are implemented.
 * All authenticated routes are children of the AppShellComponent layout route.
 */
export const APP_ROUTES: Routes = [
  // ─── Authenticated Shell ────────────────────────────────────────────────────
  // AppShellComponent acts as the layout wrapper for all authenticated routes.
  // authGuard protects the entire shell — unauthenticated users are redirected to /login.
  {
    path: '',
    component: AppShellComponent,
    canActivate: [authGuard],
    children: [
      // Default redirect: '/' → '/dashboard'
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },

      // Phase 0 Dashboard placeholder — 6 KPI slot tiles
      // Phase 1+: DashboardComponent will be replaced with real KPI data
      {
        path: 'dashboard',
        loadComponent: () =>
          import('@features/dashboard/dashboard.component').then(
            m => m.DashboardComponent
          ),
      },

      // Phase 1+ feature routes are added here as each story is implemented.
      // Example:
      //   { path: 'workflow', loadChildren: () => import('@features/workflow/routes').then(m => m.WORKFLOW_ROUTES) }
    ],
  },

  // ─── Unauthenticated Routes ──────────────────────────────────────────────────
  // These routes are intentionally NOT wrapped in AppShellComponent —
  // the login and unauthorized pages do not show the sidebar/toolbar.

  // Login — Phase 0 placeholder; Phase 1 replaces with real IdP login
  {
    path: 'login',
    loadComponent: () =>
      import('@features/auth/login-placeholder.component').then(
        m => m.LoginPlaceholderComponent
      ),
  },

  // Unauthorized — shown when adminGuard blocks access for insufficient role
  {
    path: 'unauthorized',
    loadComponent: () =>
      import('@features/auth/unauthorized.component').then(
        m => m.UnauthorizedComponent
      ),
  },

  // ─── Wildcard ────────────────────────────────────────────────────────────────
  // Unknown routes redirect to dashboard (authenticated users)
  // Note: unauthenticated users hitting this redirect will be caught by authGuard
  // on the shell route and redirected to /login instead.
  { path: '**', redirectTo: 'dashboard' },
];
