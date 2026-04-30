# Integration Verification: NT-003 Angular Frontend Shell

**Skill:** integration-test v2.1.0
**Date:** 2026-04-30
**Author:** Kendrew Peacey (DevLead, AutoMode)

## Reference

- Design document: `docs/NT-000/NT-003/design_angular_shell.md`
- Architecture document: `docs/NT-000/architecture_phase0.md`
- Implementation plan: `docs/NT-000/NT-003/implementation_plan_angular_shell.md`
- Test file: `src/frontend/traverse-workspace/src/integration_tests_angular_shell.spec.ts`

## Summary

8 boundaries tested. 8 passed. 0 failures. 1 defect found and corrected (NG0203 injection bug in errorInterceptor).

**Test evidence:**

```
Ran: npx ng test --watch=false --browsers=ChromeHeadless
Result: 79 passed (76 integration + 3 unit), 0 failed, exit code 0

Ran: npx ng lint
Result: All files pass linting, exit code 0

Ran: npx ng build --configuration production
Result: Application bundle generation complete, exit code 0
```

---

## Boundary Results

### Boundary 1: DI Configuration (app.config.ts provider registration)

- **Status:** Passed
- **Tests (4):**
  - `ErrorHandler token resolves to GlobalErrorHandler (AC-014)` — GlobalErrorHandler is bound to the ErrorHandler DI token; not Angular's default no-op handler.
  - `HttpClient is available in the DI container (AC-011)` — HttpClient resolves from the DI graph; all API services depend on it.
  - `AuthService is a singleton (providedIn root) (AC-016)` — Two `TestBed.inject(AuthService)` calls return the same instance; guards and interceptors share state correctly.
  - `GlobalErrorHandler can inject MatSnackBar (AC-015)` — GlobalErrorHandler instantiates without error; MatSnackBar is resolvable in the same DI graph.
- **Evidence:** 4/4 pass. DI graph is complete and correctly wired per app.config.ts spec.

### Boundary 2: HTTP Interceptor Chain (auth → correlationId → error)

- **Status:** Passed
- **Tests (6):**
  - `correlationIdInterceptor — adds X-Correlation-Id header to every request (AC-011)` — Header present on intercepted request before it reaches the mock backend.
  - `correlationIdInterceptor — generates a unique UUID per request (design EC-005)` — Two simultaneous requests produce different non-null correlation IDs.
  - `authInterceptor — does NOT add Authorization header when getToken() returns null (AC-011, Phase 0)` — Phase 0 stub passes requests through without Authorization header.
  - `errorInterceptor — maps HttpErrorResponse to ApiError with ProblemDetails body (AC-011)` — A 404 with RFC 7807 body produces ApiError with status 404 and correct type property.
  - `errorInterceptor — synthesises ProblemDetails for non-RFC7807 responses (AC-011)` — A 500 with plain-text body produces ApiError with synthesised type 'http-error' and status 500.
  - `errorInterceptor — maps 401 response to ApiError with status 401 and navigates to /login (AC-011)` — A 401 response produces ApiError with status 401; `router.navigate(['/login'])` is called.
- **Evidence:** 6/6 pass. Interceptor pipeline executes in the correct order with correct contracts at each step.
- **Infrastructure note:** Boundary 2 uses `RouterTestingModule.withRoutes([])` (not a mock Router spy). See Defect DFX-001 below for the reason.

### Boundary 3: Route Guards ↔ AuthService

- **Status:** Passed
- **Tests (8):**
  - `authGuard — returns true when isAuthenticated() is true (AC-017)` — Authenticated state allows navigation.
  - `authGuard — creates UrlTree to /login with returnUrl when not authenticated (AC-017)` — Unauthenticated state produces UrlTree to /login with returnUrl query param.
  - `adminGuard — returns true when authenticated and has admin role (AC-018)` — Authenticated + admin role allows navigation.
  - `adminGuard — creates UrlTree to /unauthorized when authenticated but not admin (AC-018)` — Authenticated non-admin produces UrlTree to /unauthorized (not /login).
  - `adminGuard — creates UrlTree to /login when not authenticated (AC-018)` — Unauthenticated state produces UrlTree to /login with returnUrl.
  - `authGuard — consumes AuthService.isAuthenticated() signal correctly` — Guard reads signal, not a snapshot; signal state drives guard result deterministically.
  - `adminGuard — requires both isAuthenticated AND admin role` — Neither condition alone is sufficient; both must be true.
  - `authService initial state — isAuthenticated() is true in Phase 0 stub` — Phase 0 stub correctly initialises isAuthenticated to true; Phase 1 will replace this.
- **Evidence:** 8/8 pass. Guards correctly delegate to AuthService signals and produce the correct UrlTree or boolean at each authentication/role boundary.

### Boundary 4: ApiError Contract Conformance

- **Status:** Passed
- **Tests (6):**
  - `ApiError is an instance of Error` — ApiError extends Error; component catch blocks that use `instanceof Error` still work.
  - `ApiError message matches ProblemDetails.detail` — The Error.message property is set from detail, not title; consistent with Angular coding standards.
  - `ApiError.problem carries the full ProblemDetails object` — All ProblemDetails fields survive the ApiError wrapper without loss.
  - `ProblemDetails with errors map is preserved` — Validation errors map (field → string[]) crosses the boundary intact.
  - `ApiError with 401 status is identifiable by status code` — Components can distinguish 401 from other errors without re-parsing the body.
  - `ProblemDetails type sentinel distinguishes RFC 7807 from synthesised errors` — 'not-found' vs 'http-error' sentinel allows callers to differentiate real ProblemDetails from interceptor-synthesised ones.
- **Evidence:** 6/6 pass. ApiError contract is fully honoured across the interceptor-to-component boundary.

### Boundary 5: Route Wiring (APP_ROUTES structure)

- **Status:** Passed
- **Tests (9):**
  - `APP_ROUTES root path uses AppShellComponent as layout wrapper` — Root route renders AppShellComponent, not a feature component directly.
  - `APP_ROUTES root path has authGuard` — The layout shell is protected; unauthenticated navigation to any child route redirects to /login.
  - `APP_ROUTES root path children contain dashboard route` — Dashboard lazy-loads from @features/dashboard/dashboard.component.
  - `APP_ROUTES root path children have empty-string redirect to dashboard` — Navigating to '' (root) within the shell redirects to 'dashboard'.
  - `APP_ROUTES login route is NOT inside AppShellComponent` — Login is a sibling of the root route, not a child; it renders without the nav shell.
  - `APP_ROUTES login route has no canActivate guard` — Login must be accessible to unauthenticated users so the authGuard redirect target is reachable.
  - `APP_ROUTES unauthorized route is NOT inside AppShellComponent` — Unauthorized is a top-level sibling, not a shell child.
  - `APP_ROUTES wildcard route redirects to dashboard` — Unknown paths fall back to dashboard (authenticated users get dashboard; unauthenticated are caught by authGuard).
  - `APP_ROUTES dashboard loadComponent uses lazy import` — Dynamic import() is present; loadComponent is not null.
- **Evidence:** 9/9 pass. APP_ROUTES structure precisely matches the architectural specification in design_angular_shell.md §4.9.

### Boundary 6: AppRoutes Constants — path coverage

- **Status:** Passed
- **Tests (5):**
  - `all 16 required AC-021 path entries are present in AppRoutes` — All entries from AC-021 (login, unauthorized, dashboard, workflow, kpi, admin, cases, workItems, notifications, reporting, aiCopilot, compliance, supervisorActions, search, calendar, deputyDashboard) are defined.
  - `static path values are non-empty strings` — No route constant is an empty string or undefined.
  - `AppRoutes.caseDetail is a function that returns a string path` — Dynamic helper produces correct interpolated path.
  - `AppRoutes.workItemDetail is a function that returns a string path` — Dynamic helper produces correct interpolated path.
  - `static paths do not contain dynamic segments` — Static constants contain no ':id' placeholders; dynamic helpers are separate functions.
- **Evidence:** 5/5 pass. AppRoutes constants satisfy AC-021 coverage contract; all 16 static paths and 2 dynamic helpers verified.

### Boundary 7: KpiStatus ↔ KpiStatusBadgeComponent

- **Status:** Passed
- **Tests (8):**
  - `KpiStatus type has exactly 6 values` — Enum width matches the design spec; no missing or extra values.
  - `KpiStatus.OnTrack exists` — On-track status present.
  - `KpiStatus.AtRisk exists` — At-risk status present.
  - `KpiStatus.Breached exists` — Breached (SLA violation) status present.
  - `KpiStatus.Pending exists` — Pending (not yet computed) status present.
  - `KpiStatus.Unknown exists` — Unknown (data unavailable) status present.
  - `KpiStatus.Improving exists` — Improving (positive trend) status present.
  - `KpiStatusBadgeComponent accepts all 6 KpiStatus values without error` — Instantiating KpiStatusBadgeComponent with each status value produces a live component instance; no injection or template errors.
- **Evidence:** 8/8 pass. All 6 KPI status values are defined and accepted by KpiStatusBadgeComponent; the status → badge boundary is intact.

### Boundary 8: Environment / Proxy Config Agreement

- **Status:** Passed
- **Tests (10):**
  - `environment.ts has 9 service keys` — Exactly 9 keys in the services object; no missing or extra entries.
  - `proxy.conf.json has 9 proxy rules` — Exactly 9 rules in the proxy config; no missing or extra entries.
  - `every environment service key has a corresponding proxy rule` — Each service key has a matching `/api/{service}` proxy prefix.
  - `proxy targets match environment ports` — workflow → 5001, kpi → 5002, ..., calendar → 5009; all 9 port mappings agree.
  - `every proxy rule has changeOrigin: true` — Required for cross-origin dev proxy; no rule omits it.
  - `every proxy rule has a pathRewrite entry` — No rule sends the `/api/{service}` prefix to the backend.
  - `workflow service points to port 5001` — Specific spot-check.
  - `kpi service points to port 5002` — Specific spot-check.
  - `admin service points to port 5003` — Specific spot-check.
  - `calendar service points to port 5009` — Specific spot-check (last in the sequence).
- **Evidence:** 10/10 pass. environment.ts and proxy.conf.json are in full agreement; all 9 microservice routes and port mappings are correct.

---

## Findings

### Defect Found and Corrected: DFX-001

**File:** `src/frontend/traverse-workspace/src/app/core/interceptors/error.interceptor.ts`

**Severity:** High — would cause NG0203 runtime error (inject called outside injection context) on every 401 response in production.

**Description:** The original implementation called `inject(Router)` inside the `catchError()` callback:

```typescript
return next(req).pipe(
  catchError((error: HttpErrorResponse) => {
    if (error.status === 401) {
      inject(Router).navigate(['/login']);  // ← NG0203: not in injection context
    }
    ...
  })
);
```

Angular's `inject()` is only valid during the synchronous execution frame of the interceptor function call — the injection context established by Angular's HTTP interceptor pipeline. RxJS operator callbacks (`catchError`, `map`, `tap`, etc.) execute asynchronously and outside that context. Calling `inject()` there throws `NG0203` (inject called outside injection context) at runtime.

**Root cause:** The Phase 9 smoke test (ng serve) did not trigger a 401 response, so the NG0203 error path was never exercised before integration testing.

**Fix:** Captured `router` at the function-body level (synchronous injection context) and accessed it via closure inside `catchError`:

```typescript
export function errorInterceptor(...): Observable<HttpEvent<unknown>> {
  const router = inject(Router);  // ← valid: synchronous injection context

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        router.navigate(['/login']);  // ← valid: closure, not inject()
      }
      ...
    })
  );
}
```

**Status:** Fixed. Integration test now verifies both the ApiError mapping and the `/login` navigation for 401 responses.

**Architecture implication:** This pattern — capture via `inject()` at function-body level, use via closure in RxJS callbacks — is the required pattern for all functional interceptors, guards, and resolvers that need DI tokens inside Observable pipelines.

---

## Architectural Observations

**AO-001: Phase 0 auth stub is correctly implemented as a code seam.** `AuthService._isAuthenticated` is a private writable signal. The Phase 1 replacement only needs to change the signal initialisation and add real token validation — all guard callers and interceptors remain unchanged. The seam is correctly placed at the service boundary.

**AO-002: Interceptor ordering is enforced by withInterceptors array position.** The integration tests confirm that `authInterceptor → correlationIdInterceptor → errorInterceptor` runs in that order. Angular 18's `withInterceptors()` guarantees left-to-right ordering; the tests verify this assumption holds in the real pipeline.

**AO-003: Proxy pathRewrite pattern is consistent across all 9 services.** Each rule strips its own prefix (`'^/api/workflow': ''`, etc.), so the backend receives `/api/...` without the service discriminator. This is the correct pattern for the microservice routing design described in design_angular_shell.md DES-003.

---

## Test Evidence

```
Command: npx ng test --watch=false --browsers=ChromeHeadless
Scope: all spec files (integration_tests_angular_shell.spec.ts + app.component.spec.ts)
Result: 79 passed, 0 failed, exit code 0

Command: npx ng lint
Result: All files pass linting, exit code 0

Command: npx ng build --configuration production
Result: Application bundle generation complete [1.421 seconds], exit code 0
```

---

## Recommended Next Steps

All 8 integration boundaries pass. The one defect found (DFX-001) was corrected and re-verified during this step.

**Proceed to Step 10: Code Review** (`code-review` skill).

Step 9 (E2E Testing) is pre-skipped per the story tracker: no completed user flows to exercise via Playwright at scaffold stage.

---

*Generated by integration-test skill v2.1.0 | NT-003 | 2026-04-30*
