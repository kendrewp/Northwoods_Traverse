// src/app/core/interceptors/auth.interceptor.ts
//
// authInterceptor — attaches the Bearer token to every outbound HTTP request.
//
// RESPONSIBILITY (SRP): This interceptor has exactly one responsibility — reading
// the current auth token from AuthService and adding it to the Authorization header.
// It does not handle errors, does not generate correlation IDs, and does not validate
// the token's content.
//
// PHASE 0 BEHAVIOUR: AuthService.getToken() always returns null in Phase 0 (stub).
// When the token is null, the request passes through unmodified. This means all
// API calls in Phase 0 are unauthenticated — correct behaviour until Phase 1 Auth.
//
// TOKEN READING: The token is read via inject() at call time (not at module load time).
// This is the correct Angular pattern for functional interceptors — inject() is called
// inside the interceptor function which runs within an injection context.

import { HttpRequest, HttpHandlerFn, HttpEvent } from '@angular/common/http';
import { inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AuthService } from '@core/services/auth.service';

/**
 * Functional HTTP interceptor that attaches Bearer authentication tokens.
 *
 * Reads the token from AuthService.getToken(). If null (Phase 0 stub, or
 * unauthenticated), the request passes through unmodified. If a token is
 * present, a new request is created with the Authorization header — Angular's
 * HttpRequest is immutable, so cloning is required.
 */
export function authInterceptor(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn
): Observable<HttpEvent<unknown>> {
  const token = inject(AuthService).getToken();

  // Pass through unmodified when no token — Phase 0 stub always takes this path
  if (!token) {
    return next(req);
  }

  // Clone the request to add the Authorization header — HttpRequest is immutable
  return next(req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }));
}
