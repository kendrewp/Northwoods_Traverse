// src/app/core/handlers/global-error-handler.ts
//
// GlobalErrorHandler — the single point of user-facing error display for Angular errors.
//
// RESPONSIBILITY (SRP): This class has one responsibility — receiving uncaught Angular
// errors and presenting them to the user via MatSnackBar. It does not perform HTTP calls,
// does not manipulate state, and does not rethrow errors.
//
// REGISTRATION: Registered in app.config.ts via `{ provide: ErrorHandler, useClass: GlobalErrorHandler }`.
// This replaces Angular's default console-only ErrorHandler. All zones-caught errors
// (including unhandled Promise rejections from async operations) flow through here.
//
// ERROR HIERARCHY:
// 1. ApiError (from errorInterceptor): structured, displayable message from ProblemDetails.
//    - If status === 401: redirect to /login (belt-and-suspenders — errorInterceptor also redirects).
//    - Otherwise: display problem.detail ?? problem.title in the snackbar.
// 2. Generic Error: log to console.error (not user-displayable — no safe message available).
// 3. Unknown type: log to console.error as-is.
//
// SNACKBAR CONFIG: Displayed at top-right, auto-dismiss after 6 seconds, with 'error-snackbar'
// panel class so the global theme can style it with warn colours. A 'Dismiss' action allows
// the user to close it early.
//
// INJECT PATTERN: Uses inject() in the constructor body rather than constructor parameters
// to avoid circular dependency with MatSnackBar (which itself uses Angular's DI). Both
// Router and MatSnackBar are injected at handler instantiation time.

import { ErrorHandler, inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ApiError } from '@core/models/api-error';

/**
 * Global error handler registered with Angular's ErrorHandler token.
 *
 * Catches all uncaught Angular errors. Displays a user-friendly snackbar for
 * ApiError instances. Logs unrecognised errors to the console. Redirects to
 * /login on 401 responses.
 */
@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);

  /**
   * Handle an uncaught Angular error.
   *
   * @param error - The error that was not caught by any component or RxJS catchError.
   *   May be any type — Angular's zone.js wraps and rethrows all unhandled errors here.
   */
  handleError(error: unknown): void {
    if (error instanceof ApiError) {
      // Structured error from our interceptor — display the detail message
      const message = error.problem.detail ?? error.problem.title ?? 'An error occurred.';
      this.snackBar.open(message, 'Dismiss', {
        duration: 6000,
        horizontalPosition: 'end',
        verticalPosition: 'top',
        panelClass: ['error-snackbar'],
      });

      // Belt-and-suspenders: errorInterceptor already redirects on 401,
      // but GlobalErrorHandler also handles it in case an ApiError is thrown
      // outside the HTTP interceptor chain (e.g., from a service method).
      if (error.problem.status === 401) {
        this.router.navigate(['/login']);
      }
    } else if (error instanceof Error) {
      // Generic JavaScript error — not safe to display to the user (may expose
      // implementation details). Log to the console for developer inspection.
      console.error('[GlobalErrorHandler] Unhandled error:', error);
    } else {
      // Non-Error throw (string, object, etc.) — log as-is
      console.error('[GlobalErrorHandler] Unknown error type:', error);
    }
  }
}
