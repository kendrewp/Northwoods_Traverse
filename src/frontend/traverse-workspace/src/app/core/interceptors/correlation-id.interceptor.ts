// src/app/core/interceptors/correlation-id.interceptor.ts
//
// correlationIdInterceptor — generates and propagates a unique correlation ID
// for every outbound HTTP request.
//
// RESPONSIBILITY (SRP): This interceptor has exactly one responsibility — generating
// a unique identifier per request and adding it as the X-Correlation-Id header.
// The backend CorrelationIdMiddleware (from NT-002) reads this header and propagates
// it to all downstream microservice calls, enabling end-to-end distributed tracing.
//
// UUID GENERATION: Uses crypto.randomUUID() which is available in all browsers
// supported by Angular 18 (Chrome 90+, Safari 15.4+, Firefox 95+) and Node.js 14.17+.
// No polyfill is required (EC-005 in the design document).
//
// UNIQUENESS: A new UUID is generated per request, not per session. This matches
// the backend CorrelationIdMiddleware contract which assigns a new ID per HTTP request.

import { HttpRequest, HttpHandlerFn, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';

/**
 * Functional HTTP interceptor that adds a unique correlation ID to every outbound request.
 *
 * The X-Correlation-Id header enables end-to-end request tracing across the
 * Angular frontend and all nine backend microservices. The backend CorrelationId
 * middleware reads this header and propagates it via Serilog log context.
 */
export function correlationIdInterceptor(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn
): Observable<HttpEvent<unknown>> {
  // Generate a new UUID v4 for this specific request
  const correlationId = crypto.randomUUID();

  return next(req.clone({ setHeaders: { 'X-Correlation-Id': correlationId } }));
}
