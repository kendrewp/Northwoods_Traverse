// src/app/core/interceptors/error.interceptor.ts
//
// errorInterceptor — maps HTTP errors to ApiError and handles 401 redirects.
//
// RESPONSIBILITY (SRP): This interceptor has exactly one responsibility — catching
// HttpErrorResponse objects and normalising them into ApiError. It does NOT display
// UI feedback (that is GlobalErrorHandler's responsibility — SRP enforced).
//
// 401 HANDLING: On 401, the router navigates to /login. This is done here (interceptor
// level) rather than in GlobalErrorHandler because the redirect must happen before the
// error propagates to component-level catchError handlers, which might swallow it.
//
// INJECTION CONTEXT: Angular's inject() is only valid within the synchronous execution
// frame of the interceptor function, NOT inside RxJS operator callbacks such as
// catchError(). The Router must therefore be captured at the function body level —
// before returning the Observable — and referenced via closure inside catchError().
// Calling inject() inside the catchError callback throws NG0203 at runtime.
//
// ERROR MAPPING: If the server returns a valid RFC 7807 ProblemDetails body (identified
// by the presence of the 'type' property), it is used directly. Otherwise, a synthetic
// ProblemDetails is constructed from the HTTP status and message. This ensures all
// components receive a consistent ApiError regardless of server response format.
//
// RETHROW: The error is always rethrown as ApiError so component-level catchError can
// inspect the ProblemDetails structure. The GlobalErrorHandler also receives it for
// display purposes.

import { HttpRequest, HttpHandlerFn, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';
import { ApiError, ProblemDetails } from '@core/models/api-error';

/**
 * Functional HTTP interceptor that normalises all HTTP errors to ApiError.
 *
 * For 401 responses, navigates to /login before rethrowing. For all other errors,
 * maps the response body (or synthesises a ProblemDetails) and rethrows as ApiError.
 * Does not display snackbars or other UI — that is GlobalErrorHandler's role.
 *
 * IMPORTANT: Router is captured via inject() at the function body level (synchronous
 * injection context) and accessed by closure inside catchError(). Never call inject()
 * inside RxJS callbacks — Angular throws NG0203 (inject called outside injection context).
 */
export function errorInterceptor(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn
): Observable<HttpEvent<unknown>> {
  // Capture Router here — inject() is valid at function-body level (synchronous
  // injection context established by Angular's HTTP interceptor invocation).
  // The closure keeps the reference alive for the async catchError callback.
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // 401 Unauthorized: session expired or not authenticated — redirect to login.
      // router was captured in the injection context above; accessing it here via
      // closure is safe and avoids the NG0203 injection-outside-context error.
      if (error.status === 401) {
        router.navigate(['/login']);
      }

      // Map the error to a ProblemDetails structure.
      // If the response body has a 'type' property, treat it as a valid RFC 7807 object.
      // Otherwise, synthesise a ProblemDetails from the HTTP status and message.
      const problem: ProblemDetails = (error.error as ProblemDetails)?.type
        ? (error.error as ProblemDetails)
        : {
            type: 'http-error',
            title: error.statusText || 'Request Failed',
            status: error.status,
            detail: error.message,
          };

      return throwError(() => new ApiError(problem));
    })
  );
}
