// src/app/app.config.ts
//
// Bootstrap providers for the Angular application.
//
// This is the single registration point for all application-level providers.
// Phase 1+ stories may add feature-specific providers here or to their own
// route configs, but the core providers defined here are always available.
//
// PROVIDER ORDER matters for interceptors: they run in array order (left to right):
// 1. authInterceptor — adds Bearer token header (must run first so token is present)
// 2. correlationIdInterceptor — generates X-Correlation-Id per request
// 3. errorInterceptor — catches HttpErrorResponse, maps to ApiError, handles 401
//
// SOLID SRP: Each interceptor is in its own file with one responsibility.
// SOLID DIP: Components depend on `ErrorHandler` token, not GlobalErrorHandler directly.
// The token is resolved to GlobalErrorHandler by this provider registration.
//
// AC-011: provideHttpClient with interceptors
// AC-012: provideRouter with blocking initial navigation
// AC-013: provideAnimations
// AC-014: GlobalErrorHandler via ErrorHandler token

import { ApplicationConfig, ErrorHandler } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter, withEnabledBlockingInitialNavigation } from '@angular/router';
import { provideAnimations } from '@angular/platform-browser/animations';
import { APP_ROUTES } from './core/routes/app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { correlationIdInterceptor } from './core/interceptors/correlation-id.interceptor';
import { errorInterceptor } from './core/interceptors/error.interceptor';
import { GlobalErrorHandler } from './core/handlers/global-error-handler';

/**
 * Application configuration — providers registered for the entire application.
 *
 * Passed to bootstrapApplication() in main.ts. This replaces Angular module
 * (NgModule) -based provider registration.
 */
export const appConfig: ApplicationConfig = {
  providers: [
    // HTTP client with interceptor pipeline in execution order
    provideHttpClient(withInterceptors([
      authInterceptor,         // 1. Attach Bearer token (Phase 0 stub: passes through)
      correlationIdInterceptor, // 2. Add X-Correlation-Id header
      errorInterceptor,        // 3. Map errors to ApiError; handle 401 redirect
    ])),

    // Router with all feature routes defined in app.routes.ts
    // withEnabledBlockingInitialNavigation: waits for router to complete initial
    // navigation before rendering — prevents flash of unauthenticated content
    provideRouter(APP_ROUTES, withEnabledBlockingInitialNavigation()),

    // Angular animations — required by Angular Material components (mat-sidenav,
    // mat-snackbar, etc.)
    provideAnimations(),

    // Global error handler — catches all uncaught Angular errors and displays
    // user-friendly snackbar messages; redirects on 401
    { provide: ErrorHandler, useClass: GlobalErrorHandler },
  ],
};
