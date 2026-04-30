// src/app/app.component.ts
//
// AppComponent — the thin root component of the Angular application.
//
// RESPONSIBILITY (SRP): This component has exactly one responsibility — providing
// the <router-outlet> that Angular Router renders feature pages into. All layout,
// auth state, and nav logic lives in AppShellComponent (loaded as a route component),
// not here.
//
// WHY SO THIN: The application shell (sidebar + toolbar + responsive layout) is loaded
// as a route component (AppShellComponent) for all authenticated routes. This means
// the shell can be bypassed for unauthenticated pages (login, unauthorized) which
// render without the nav frame. If AppShellComponent were embedded directly in
// AppComponent's template, it would always render — including on the login page.
//
// Bootstrap chain:
//   main.ts → bootstrapApplication(AppComponent, appConfig)
//     → AppComponent renders <router-outlet>
//       → APP_ROUTES matches '' → AppShellComponent (canActivate: authGuard)
//         → AppShellComponent renders sidebar + toolbar + child <router-outlet>
//           → 'dashboard' child route → DashboardComponent
//
// AC-004: ChangeDetectionStrategy.OnPush

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

/**
 * Root component — renders the router outlet. All application content is
 * rendered by child route components (AppShellComponent for authenticated routes).
 */
@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  template: '<router-outlet />',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppComponent {}
