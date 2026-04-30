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
 */
export function errorInterceptor(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn
): Observable<HttpEvent<unknown>> {
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // 401 Unauthorized: session expired or not authenticated — redirect to login
      if (error.status === 401) {
        inject(Router).navigate(['/login']);
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
