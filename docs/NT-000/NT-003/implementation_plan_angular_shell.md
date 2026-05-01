# Implementation Plan: Angular Frontend Shell — NT-003

> **Feature:** NT-000 — Phase 0 Scaffolding
> **Story:** NT-003 — Angular Frontend Shell
> **Design Document:** `docs/NT-000/NT-003/design_angular_shell.md`
> **Design Checksum (validated):** `sha256-eba37109931a59c674b2d1cfe2d536a616beaeabef302b72974c1048eb3db65d`
> **Plan Created:** 2026-04-30
> **Branch:** `story/NT-003`
> **Skill:** plan-implementation v3.4.0

---

## Plan Header

### Scope

Create the Angular frontend shell for Northwoods Traverse by completing the `ng new` scaffold merge into the existing NT-001 stub workspace (`src/frontend/traverse-workspace/`), establishing the Material 3 theme, core providers, HTTP interceptors, route guards, shared presentational components, persona nav shell, environment files, and proxy configuration. This plan covers all 37 acceptance criteria (AC-001 through AC-037) across 8 user stories.

### Risk Assessment

| Risk | Category | Phases Affected | Mitigation |
|------|----------|-----------------|------------|
| `ng new` merge may overwrite NT-001 stub settings | Technical | Phase 1 | Phase 1 explicitly verifies OnPush schematic, path aliases, proxy reference are preserved; sequence is: merge generated files → verify NT-001 settings present → only then commit |
| `mat.define-theme()` API differences across Material versions | Technical | Phase 3 | Package.json pins `@angular/material ^18.0.0`; Phase 3 verifies `ng build` succeeds before proceeding; EC-002 fallback documented |
| TypeScript strict mode rejects authored code | Technical | All phases | Strict mode enabled in tsconfig stub already; each phase verifies `ng build` produces 0 TypeScript errors before moving on |
| Responsive breakpoint CSS does not match design AC-032/033 | Functional | Phase 6 | Phase 6 tests sidebar at 767px, 768px, 1024px, 1025px boundaries; CSS grid tokens are defined in Phase 3 so Phase 6 only applies them |
| `AuthService.logout()` called during in-flight request | Functional | Phase 5 | EC-006 addressed in Phase 5; `takeUntilDestroyed` pattern enforced in AppShellComponent |
| `.gitattributes` missing `*.scss`/`*.css` LF rules | Functional | Phase 2 (first action) | Phase 2 adds LF rules as its first step before any SCSS is created; no SCSS file is committed before this step |

### Effort Estimate (CU-Derived — Plan Gate, Checkpoint 3)

| Input | Value |
|-------|-------|
| Adjusted CU (from design document §9) | 119.0 |
| `hours_per_cu` (project-baseline.json) | 2.0 |
| Total estimate | **238.0 hours** |
| Confidence range (Checkpoint 3 ±30%) | **166.6 – 309.4 hours** |

> **Warm-up period active** — 0 of 6 required actuals logged for northwoods-traverse. Default calibration factor 1.4 in use. Actual bounds may be wider than ±30% until calibration stabilises.

### Phase Effort Distribution

| Phase | Description | Estimated Hours |
|-------|-------------|----------------|
| Phase 1 | Scaffold merge + build verification | 20 hrs |
| Phase 2 | `.gitattributes` fix + Material 3 theme + global styles | 22 hrs |
| Phase 3 | Core models, `KpiStatus` type, `AppRoutes` constants | 14 hrs |
| Phase 4 | Core providers — `app.config.ts`, interceptors, `GlobalErrorHandler` | 32 hrs |
| Phase 5 | `AuthService` stub + route guards | 24 hrs |
| Phase 6 | `AppShellComponent` — nav shell, responsive, persona chips | 40 hrs |
| Phase 7 | Shared presentational components (4 components) | 30 hrs |
| Phase 8 | Environment files, proxy config, auth feature placeholders, `APP_ROUTES` | 28 hrs |
| Phase 9 | Dashboard placeholder component + `ng build --configuration production` final verification | 28 hrs |
| **Total** | | **238 hours** |

### Dummy Code Handling

The following placeholder code is created in this story — all are intentional Phase 0 stubs documented with `// Phase 0 stub` comments:

| File | Dummy/Stub Nature | When Replaced |
|------|--------------------|---------------|
| `AuthService` | Hardcoded `isAuthenticated = signal(true)`, `roles = ['social-worker', 'admin']` | Phase 1 Auth story |
| `AuthService.getToken()` | Returns `null` always | Phase 1 Auth story |
| `LoginPlaceholderComponent` | "Authentication not yet configured" card; "Continue to Dashboard" button | Phase 1 Auth story |
| `UnauthorizedComponent` | Simple "Access Denied" card | Phase 1 Auth story (or earlier if needed) |
| `DashboardComponent` | Static 6 KPI slot tiles, all `status='slate'` | MOD-01/MOD-03 Phase 1 |
| All Phase 1+ nav items | `disabled: true` | Each Phase 1+ story enables its route |
| `environment.prod.ts` | Placeholder production URLs | CI/CD pipeline configuration |

No legacy dummy code from NT-001 or NT-002 needs to be removed in this story.

---

## Phase 1: `ng new` Scaffold Merge + Build Verification

### What
Run `ng new traverse` to generate the standard Angular scaffold, then merge it into the existing NT-001 stub workspace at `src/frontend/traverse-workspace/`. Preserve all NT-001 stub settings.

### Why
The NT-001 stub provides `angular.json` and `tsconfig.json` with approved architectural settings (OnPush schematic default, path aliases, proxy reference). Running `ng new` verbatim would overwrite these. The merge approach installs the boilerplate (package.json, src/, tsconfig.app.json, tsconfig.spec.json) while retaining all NT-001 decisions. (Design §3, decision DES-001.)

### Steps

**Step 1.1 — Install Angular CLI (if not present)**

- Check: `ng version` — if Angular CLI 18.x is not installed, run `npm install -g @angular/cli@18`
- Why: The workspace requires Angular 18+ for standalone components, signals, and Material 3.

**Step 1.2 — Run `ng new` in a temporary directory**

```bash
cd /tmp
ng new traverse --standalone --strict --routing --style=scss --skip-git --skip-install
```

- Why: `--skip-install` avoids downloading node_modules into the temp location (we will run `npm install` in the real workspace). `--skip-git` prevents git init in /tmp.

**Step 1.3 — Merge generated files into the worktree workspace**

Copy these files from `/tmp/traverse/` into `src/frontend/traverse-workspace/`:
- `package.json` (generated — establishes Angular 18+ dependencies)
- `tsconfig.app.json` (generated — extends root tsconfig, references src/main.ts)
- `tsconfig.spec.json` (generated — extends root tsconfig, for Jest/Karma config)
- `.editorconfig` (generated — standard Angular editor settings)
- `src/` directory tree (generated — index.html, main.ts, app.component.ts, app.routes.ts, styles.scss, assets/)

**Do NOT copy:**
- `angular.json` (preserve NT-001 version)
- `tsconfig.json` (preserve NT-001 version with path aliases)
- `.gitignore` (preserve repo root version)
- `proxy.conf.json` (will be created explicitly in Phase 8)

**Step 1.4 — Verify NT-001 settings are preserved**

After merge, verify the following in `angular.json`:
- `"projects": { "traverse": {...} }` — project name is "traverse"
- `"schematics": { "@schematics/angular:component": { "changeDetection": "OnPush", "standalone": true, "style": "scss" } }` — OnPush schematic default present
- `"serve": { "options": { "proxyConfig": "proxy.conf.json" } }` — proxy reference present

Verify in `tsconfig.json`:
- `"paths": { "@core/*": ["app/core/*"], "@shared/*": ["app/shared/*"], "@features/*": ["app/features/*"] }` — path aliases present
- `"strict": true` — strict mode enabled
- `_stub_notice` field is still present (confirms the right file was kept)

**Step 1.5 — Run `npm install`**

```bash
cd src/frontend/traverse-workspace
npm install
```

Verify: exits with 0 errors. Note: peer dependency warnings are acceptable; errors are not.

**Step 1.6 — Verify initial build**

```bash
ng build
```

Verify: exits with code 0. TypeScript errors are acceptable at this point (generated app.component.ts has not been updated yet). The goal is to confirm the workspace is structurally sound.

### Files Touched

| File | Change |
|------|--------|
| `src/frontend/traverse-workspace/package.json` | Created (from ng new) |
| `src/frontend/traverse-workspace/tsconfig.app.json` | Created (from ng new) |
| `src/frontend/traverse-workspace/tsconfig.spec.json` | Created (from ng new) |
| `src/frontend/traverse-workspace/.editorconfig` | Created (from ng new) |
| `src/frontend/traverse-workspace/src/` | Created directory tree (from ng new) |
| `src/frontend/traverse-workspace/angular.json` | Verified unchanged (NT-001 preserved) |
| `src/frontend/traverse-workspace/tsconfig.json` | Verified unchanged (NT-001 preserved) |

### Not Changed
- `.v-model/config.json` — not touched
- `CLAUDE.md` — not touched
- NT-002 shared library files — not touched (this is a pure frontend phase)

### Tests
- Step 1.6: `ng build` exits with code 0 (structural soundness)
- Step 1.4 manual: `angular.json` project name is "traverse"; schematics contain OnPush default

### Success Criteria
- [ ] `npm install` exits with 0 errors
- [ ] `angular.json` project name is "traverse" and preserves schematics block
- [ ] `tsconfig.json` path aliases (`@core/*`, `@shared/*`, `@features/*`) are present
- [ ] `tsconfig.json` `_stub_notice` field is present (confirms NT-001 stub was preserved)
- [ ] `ng build` (initial) exits with code 0

### Risks / Compatibility
- If `ng new` generates with a different project name than "traverse", rename the project key in `angular.json` before merging.
- If the Angular CLI version is <18, the Material 3 `mat.define-theme()` API will not be available — resolve by upgrading CLI.

---

## Phase 2: `.gitattributes` LF Fix + Material 3 Theme + Global Styles

### What

Step 2.1: Add `*.scss` and `*.css` LF rules to `.gitattributes` (SF-1 carryover).
Step 2.2: Create `src/styles/theme.scss` — Material 3 theme with Traverse brand tokens.
Step 2.3: Update `src/styles.scss` — typography reset, layout primitives, shell grid CSS.

### Why

SF-1 (NT-001 code review finding) requires `.scss`/`.css` LF rules before any SCSS is committed (AC-010). The Material 3 theme and global styles are the design language foundation — all component styles reference `--mat-sys-*` and `--traverse-status-*` tokens defined here (AC-006, AC-007, AC-008, AC-009).

### Steps

**Step 2.1 — Add LF rules to `.gitattributes`**

Add to `.gitattributes` in the repo root:
```gitattributes
# Stylesheet line endings — always LF to prevent CRLF diffs on Windows
*.scss text eol=lf
*.css  text eol=lf
```

**Step 2.2 — Create `src/styles/` directory and `theme.scss`**

Create `src/frontend/traverse-workspace/src/styles/theme.scss` with:
- `@use '@angular/material' as mat;`
- `$traverse-theme` defined using `mat.define-theme()` with `mat.$azure-palette` as primary seed and `mat.$cyan-palette` as tertiary
- Typography: `brand-family: 'Segoe UI, Arial, sans-serif'`
- `html { @include mat.all-component-themes($traverse-theme); }`
- `:root` block with 6 `--traverse-status-*` custom properties (exact hex values from design §4.3)
- Comment explaining `#1e4d8c` brand colour and `mat.$azure-palette` as closest approximation

**Step 2.3 — Replace generated `src/styles.scss`**

Replace the `ng new`-generated `styles.scss` with the content from design §4.3:
- `@use './styles/theme' as *;`
- `@use '@angular/material' as mat;`
- Typography reset: `html, body` at 13px, Segoe UI/Arial
- Form control font-size: 14px
- `:root` with layout primitive variables (sidebar width, topnav height, content padding)
- `.traverse-shell-grid` CSS Grid class with responsive breakpoints

**Step 2.4 — Verify build with theme**

```bash
ng build
```

Verify: exits with code 0; no SCSS compilation errors; Material theme compiles successfully.

### Files Touched

| File | Change |
|------|--------|
| `.gitattributes` (repo root) | Add `*.scss` and `*.css` LF rules |
| `src/frontend/traverse-workspace/src/styles/theme.scss` | Created |
| `src/frontend/traverse-workspace/src/styles.scss` | Replaced with design content |

### Scope Flag
`.gitattributes` is at the repository root and is not in the protected files list (protected files are: `.gitignore`, `.v-model/config.json`, `.v-model/auto-v-model-interactions.json`, `CLAUDE.md`, `skills/_shared/**`). This modification is within scope — it adds only the SCSS/CSS LF rules specified by AC-010 and does not touch any protected content. **No orchestrator approval required for this change.**

### Not Changed
- `angular.json` — not touched in this phase
- Any component file — components are created in later phases

### Tests
- Step 2.4: `ng build` exits with code 0 with theme in place
- Manual: `src/styles/theme.scss` contains 6 `--traverse-status-*` custom properties
- Manual: `src/styles.scss` contains `.traverse-shell-grid` class with grid breakpoints

### Success Criteria
- [ ] `.gitattributes` contains `*.scss text eol=lf` and `*.css text eol=lf`
- [ ] `src/styles/theme.scss` exists with `mat.define-theme()` call and 6 status tokens
- [ ] `src/styles.scss` contains `.traverse-shell-grid` CSS Grid class
- [ ] `ng build` exits with code 0 after theme addition
- [ ] No hardcoded hex/rgb colours appear in `styles.scss` (only `var(--mat-sys-*)` or `var(--traverse-status-*)`)

### Risks / Compatibility
- EC-002: If `mat.define-theme()` is not available (Material < 17), fallback is `mat.define-light-theme()` — but `package.json` pins `^18.0.0` so this should not occur.
- The `mat.all-component-themes()` call applies themes globally. This is correct for Phase 0.

---

## Phase 3: Core Models, `KpiStatus` Type, and `AppRoutes` Constants

### What

Step 3.1: Create `src/app/core/models/api-error.ts` — `ProblemDetails` interface and `ApiError` class.
Step 3.2: Create `src/app/core/models/kpi-status.type.ts` — `KpiStatus` string union type.
Step 3.3: Create `src/app/core/routes/app-routes.ts` — `AppRoutes` typed constants.

### Why

Models and route constants have no upstream dependencies — they are leaf nodes of the dependency graph. Creating them first means all subsequent phases can import from them without circular dependency risk. `AppRoutes` is needed by guards (Phase 5) and route config (Phase 8); `ApiError` is needed by interceptors (Phase 4) and shared components (Phase 7). (SOLID DIP: consumers depend on abstractions defined here.)

### Steps

**Step 3.1 — Create directory structure**

```bash
mkdir -p src/app/core/models
mkdir -p src/app/core/routes
mkdir -p src/app/core/guards
mkdir -p src/app/core/handlers
mkdir -p src/app/core/interceptors
mkdir -p src/app/core/services
mkdir -p src/app/core/shell
mkdir -p src/app/features/auth
mkdir -p src/app/features/dashboard
mkdir -p src/app/shared/components/page-header
mkdir -p src/app/shared/components/error-display
mkdir -p src/app/shared/components/loading-spinner
mkdir -p src/app/shared/components/kpi-status-badge
mkdir -p src/environments
```

**Step 3.2 — Create `api-error.ts`**

```typescript
// ProblemDetails interface (RFC 7807) and ApiError class.
// Used by errorInterceptor to normalise all HTTP errors into a consistent shape
// before they reach component-level error handlers.
export interface ProblemDetails {
  type: string;
  title: string;
  status: number;
  detail: string;
  instance?: string;
  errors?: Record<string, string[]>;
}

export class ApiError extends Error {
  constructor(public readonly problem: ProblemDetails) {
    super(problem.detail ?? problem.title);
    this.name = 'ApiError';
  }
}
```

**Step 3.3 — Create `kpi-status.type.ts`**

```typescript
// KpiStatus — string union type for KPI status values.
// Maps to --traverse-status-* CSS custom properties defined in theme.scss.
// Values are used by KpiStatusBadgeComponent (AC-027).
export type KpiStatus = 'green' | 'yellow' | 'red' | 'fuchsia' | 'emerald' | 'slate';
```

**Step 3.4 — Create `app-routes.ts`**

Implement `AppRoutes` constant exactly as specified in design §4.9, including all 16 path entries and 2 dynamic path helpers (`caseDetail`, `workItemDetail`). (AC-020, AC-021, AC-022.)

**Step 3.5 — Verify TypeScript compiles**

```bash
ng build
```

Verify: 0 TypeScript errors on the new files (they have no external dependencies beyond TypeScript built-ins).

### Files Touched

| File | Change |
|------|--------|
| `src/app/core/models/api-error.ts` | Created |
| `src/app/core/models/kpi-status.type.ts` | Created |
| `src/app/core/routes/app-routes.ts` | Created |
| All `mkdir -p` directories | Created (empty) |

### Not Changed
- Existing `src/` files from ng new (app.component.ts, main.ts, etc.) — not yet touched
- Any `.v-model` or shared backend files

### Tests
- Step 3.5: `ng build` exits with code 0
- Manual: `AppRoutes` contains exactly 16 string-typed path entries (AC-021)
- Manual: `AppRoutes.caseDetail('123')` returns `'/cases/123'` (AC-022)
- Manual: `KpiStatus` type accepts `'green'` and rejects `'purple'` at compile time

### Success Criteria
- [ ] `api-error.ts` exports `ProblemDetails` interface and `ApiError` class
- [ ] `kpi-status.type.ts` exports `KpiStatus` type with 6 string literals
- [ ] `app-routes.ts` exports `AppRoutes` `as const` with 16 path entries and 2 dynamic helpers
- [ ] `ng build` exits with code 0

### Risks / Compatibility
- None. Pure TypeScript type definitions with no external Angular or Material dependencies.

---

## Phase 4: Core Providers — Interceptors, `GlobalErrorHandler`, and `app.config.ts`

### What

Step 4.1: Create `authInterceptor` (`auth.interceptor.ts`).
Step 4.2: Create `correlationIdInterceptor` (`correlation-id.interceptor.ts`).
Step 4.3: Create `errorInterceptor` (`error.interceptor.ts`).
Step 4.4: Create `GlobalErrorHandler` (`global-error-handler.ts`).
Step 4.5: Create `app.config.ts` wiring all providers.
Step 4.6: Update `main.ts` to use `bootstrapApplication(AppComponent, appConfig)`.

### Why

The interceptors and error handler form the HTTP and error infrastructure that all features depend on. They must exist before guards (Phase 5) or the shell (Phase 6) because the shell will make HTTP calls. `app.config.ts` is the single registration point — SOLID SRP: each interceptor has exactly one responsibility. (AC-011 through AC-015.)

### Steps

**Step 4.1 — Create `auth.interceptor.ts`**

Implement as a functional interceptor per design §4.6. Key behaviour:
- Calls `inject(AuthService).getToken()`; passes request through if null
- Clones request with `Authorization: Bearer {token}` header if token present
- In Phase 0, always passes through (stub returns null)
- Import: `AuthService` from `@core/services/auth.service` (path alias)

**Step 4.2 — Create `correlation-id.interceptor.ts`**

Implement per design §4.6:
- Generates UUID v4 via `crypto.randomUUID()`
- Clones request with `X-Correlation-Id: {uuid}` header
- No external dependencies other than Angular HTTP types

**Step 4.3 — Create `error.interceptor.ts`**

Implement per design §4.6:
- `catchError` on `HttpErrorResponse`
- If `status === 401`, calls `inject(Router).navigate(['/login'])`
- Maps to `ProblemDetails` (uses response body if it has `type` property; synthesises otherwise)
- Returns `throwError(() => new ApiError(problem))`
- Imports: `Router` from `@angular/router`, `ProblemDetails`/`ApiError` from `@core/models/api-error`

**Step 4.4 — Create `global-error-handler.ts`**

Implement per design §4.5 (AC-015):
- Implements Angular `ErrorHandler`
- Injects `MatSnackBar` — displays message from `ApiError.problem.detail ?? title`
- If `ApiError` and `status === 401`, navigates to `/login`
- Falls through to `console.error` for unknown errors
- SnackBar config: `duration: 6000, horizontalPosition: 'end', verticalPosition: 'top', panelClass: ['error-snackbar']`

**Step 4.5 — Create `app.config.ts`**

Implement exactly as shown in design §4.5 (AC-011 through AC-014):
- `provideHttpClient(withInterceptors([authInterceptor, correlationIdInterceptor, errorInterceptor]))`
- `provideRouter(APP_ROUTES, withEnabledBlockingInitialNavigation())`
- `provideAnimations()`
- `{ provide: ErrorHandler, useClass: GlobalErrorHandler }`

Note: `APP_ROUTES` is imported from `@core/routes/app.routes` — but `app.routes.ts` is created in Phase 8. For now, import from a temporary stub `app.routes.ts` that exports an empty `APP_ROUTES: Routes = []`. This will be replaced in Phase 8.

**Step 4.6 — Update `main.ts`**

Replace `ng new`-generated `main.ts` with:
```typescript
import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { appConfig } from './app/app.config';

bootstrapApplication(AppComponent, appConfig)
  .catch(err => console.error(err));
```

**Step 4.7 — Create temporary `app.routes.ts` stub**

```typescript
// Temporary Phase 4 stub — full route table added in Phase 8.
import { Routes } from '@angular/router';
export const APP_ROUTES: Routes = [];
```

**Step 4.8 — Verify build**

```bash
ng build
```

Verify: 0 TypeScript errors. (AppComponent from ng new may need updating; that is addressed in Phase 6.)

### Files Touched

| File | Change |
|------|--------|
| `src/app/core/interceptors/auth.interceptor.ts` | Created |
| `src/app/core/interceptors/correlation-id.interceptor.ts` | Created |
| `src/app/core/interceptors/error.interceptor.ts` | Created |
| `src/app/core/handlers/global-error-handler.ts` | Created |
| `src/app/app.config.ts` | Created |
| `src/app/app.routes.ts` (stub) | Created (temporary, replaced Phase 8) |
| `src/main.ts` | Updated |

### Not Changed
- `angular.json` — not touched
- `src/app/app.component.ts` — not touched yet (Phase 6)

### Tests
- Step 4.8: `ng build` exits with code 0
- Manual: `app.config.ts` contains `provideHttpClient`, `provideRouter`, `provideAnimations`, `ErrorHandler` provider
- Manual: `error.interceptor.ts` does NOT show a snackbar directly (that is `GlobalErrorHandler`'s role)
- Edge case: `authInterceptor` called with null token → request passes through unmodified (unit-testable behaviour)

### Success Criteria
- [ ] All 3 interceptors created as functional (non-class) interceptors
- [ ] `GlobalErrorHandler` implements Angular `ErrorHandler` interface
- [ ] `app.config.ts` registers all 4 required providers (AC-011 through AC-014)
- [ ] `main.ts` uses `bootstrapApplication(AppComponent, appConfig)`
- [ ] `ng build` exits with code 0

### Risks / Compatibility
- `APP_ROUTES` stub (empty array) means no routes exist yet — this is intentional at this phase; the app renders a blank `<router-outlet>`.
- `inject(AuthService)` in `auth.interceptor.ts` creates a forward dependency — `AuthService` is created in Phase 5. Write the import now; it will resolve once Phase 5 creates the file.

---

## Phase 5: `AuthService` Stub and Route Guards

### What

Step 5.1: Create `AuthService` stub (`auth.service.ts`).
Step 5.2: Create `authGuard` (`auth.guard.ts`).
Step 5.3: Create `adminGuard` (`admin.guard.ts`).
Step 5.4: Create `NavItem` model (`nav-item.model.ts`).

### Why

Guards depend on `AuthService`; the nav shell (Phase 6) depends on both guards and `NavItem`. Creating `AuthService` and guards before the shell maintains a clean dependency hierarchy (SOLID DIP). The stub `AuthService` returns hardcoded dev values — no IdP connection — because the contract (API surface) is what matters in Phase 0, not the implementation. (AC-016 through AC-019.)

### Steps

**Step 5.1 — Create `auth.service.ts`**

Implement exactly as shown in design §4.7:
- `@Injectable({ providedIn: 'root' })`
- 3 private signals: `_isAuthenticated = signal(true)`, `_roles = signal(['social-worker', 'admin'])`, `_currentRole = signal('social-worker')`
- 4 public methods: `isAuthenticated()`, `hasRole(role: string)`, `getToken()`, `getCurrentRole()`
- `logout()` method: sets `_isAuthenticated.set(false)`, navigates to `/login` via `inject(Router)`
- All stub methods commented with `// Phase 0 stub — replace in Phase 1 Auth story`
- `getToken()` always returns `null` in Phase 0

**Step 5.2 — Create `auth.guard.ts`**

Implement per design §4.8 (AC-017, AC-019):
- Functional `CanActivateFn` — not a class
- Checks `auth.isAuthenticated()`; if false, returns `router.createUrlTree([AppRoutes.login], { queryParams: { returnUrl: state.url } })`
- If true, returns `true`

**Step 5.3 — Create `admin.guard.ts`**

Implement per design §4.8 (AC-018, AC-019):
- Functional `CanActivateFn`
- If authenticated AND `hasRole('admin')`: return `true`
- If not authenticated: redirect to `/login?returnUrl=...`
- If authenticated but not admin: redirect to `AppRoutes.unauthorized`

**Step 5.4 — Create `nav-item.model.ts`**

```typescript
// NavItem — defines a single navigation item in the sidebar.
// The `roles` array controls which personas see this item.
// The `disabled` flag is true for Phase 0 placeholder items (routes not yet implemented).
export interface NavItem {
  label: string;
  icon: string;       // Material icon name
  route: string;      // Route path from AppRoutes
  roles: string[];    // Persona roles that see this item
  disabled?: boolean; // True for Phase 0 placeholder items
}
```

**Step 5.5 — Verify build (resolve Phase 4 forward dependency)**

```bash
ng build
```

Verify: The `auth.interceptor.ts` import of `AuthService` now resolves. 0 TypeScript errors.

### Files Touched

| File | Change |
|------|--------|
| `src/app/core/services/auth.service.ts` | Created |
| `src/app/core/guards/auth.guard.ts` | Created |
| `src/app/core/guards/admin.guard.ts` | Created |
| `src/app/core/shell/nav-item.model.ts` | Created |

### Not Changed
- `auth.interceptor.ts` — only the import resolves; the file is not changed
- Any Phase 1+ feature files

### Tests
- Step 5.5: `ng build` exits with code 0
- Manual: `authGuard` is exported as a function (not a class) — `typeof authGuard === 'function'`
- Manual: `adminGuard` is exported as a function
- Behaviour: `AuthService.isAuthenticated()` returns `true` (Phase 0 stub)
- Behaviour: `AuthService.hasRole('admin')` returns `true` (Phase 0 stub)
- Behaviour: `AuthService.getToken()` returns `null`
- Edge case: `authGuard` called with unauthenticated user → UrlTree pointing to `/login` returned
- Edge case: `adminGuard` called with authenticated non-admin → UrlTree pointing to `/unauthorized` returned
- Edge case EC-006: `AuthService.logout()` sets `_isAuthenticated` to false before navigating

### Success Criteria
- [ ] `AuthService` has the exact 4 public methods specified (AC-016)
- [ ] `authGuard` is a functional `CanActivateFn` (AC-019)
- [ ] `adminGuard` is a functional `CanActivateFn` (AC-019)
- [ ] `authGuard` redirects to `/login?returnUrl=<url>` for unauthenticated access (AC-017)
- [ ] `adminGuard` redirects to `/unauthorized` for non-admin authenticated users (AC-018)
- [ ] `ng build` exits with code 0

### Risks / Compatibility
- `AuthService.logout()` uses `inject(Router)` at call time (not constructor). This is the correct Angular pattern for functional-style services. Ensure `inject()` is called only within an injection context (i.e., not in class field initializers).

---

## Phase 6: `AppShellComponent` — Nav Shell, Responsive Layout, Persona Chips

### What

Step 6.1: Create `AppShellComponent` with sidebar nav, top toolbar, persona chip, and router-outlet.
Step 6.2: Implement responsive sidebar behaviour using `BreakpointObserver` signals.
Step 6.3: Update `app.component.ts` to be the thin root with `<router-outlet>`.

### Why

The shell is the visual frame for the entire application — every authenticated page renders inside it. It is the smart container for nav visibility (SOLID SRP: it owns nav state, not the pages). Responsive behaviour must be implemented now (EC-003) so Phase 1 features inherit correct mobile UX without rework. (AC-029 through AC-033.)

### Steps

**Step 6.1 — Create `app-shell.component.ts`**

Implement the smart container per design §4.4:
- `selector: 'app-shell'`
- `standalone: true`
- `changeDetection: ChangeDetectionStrategy.OnPush`
- Inject: `AuthService`, `BreakpointObserver`, `DestroyRef`, `Router`
- Signals:
  - `currentRole = computed(() => this.authService.getCurrentRole())`
  - `sidebarExpanded = signal(true)`
  - `isMobile = signal(false)`
- `BreakpointObserver` subscription (via `toSignal` or `takeUntilDestroyed`) updates `isMobile` and `sidebarExpanded` based on breakpoints (< 768px = mobile; 768–1024px = tablet; > 1024px = desktop)
- `navItems`: define the 10 `NavItem` objects from design §4.11, with `disabled: true` on all except Dashboard
- `toggleSidebar()` method

**Step 6.2 — Create `app-shell.component.html`**

Implement the three-region layout:
- `<mat-toolbar>` (top nav): menu hamburger button, "Traverse Workspace" title, spacer, persona chip
- `<nav mat-nav-list>` (sidebar): rendered in a `<mat-sidenav>` for mobile, or inline for desktop/tablet
- `<main class="content-area">`: contains `<router-outlet />`
- Role chip: `<mat-chip>{{ currentRole() | titlecase }}</mat-chip>` — non-interactive in Phase 0
- Nav items: `*ngFor` over filtered nav items based on `currentRole()`; items with `disabled: true` rendered with `[disabled]="true"` on the anchor

**Step 6.3 — Create `app-shell.component.scss`**

- Host element applies `.traverse-shell-grid` class (uses CSS Grid variables from `styles.scss`)
- Sidebar styles: width controlled by `--traverse-sidebar-width` variable; icon-only mode at tablet breakpoint
- Top toolbar: pinned to top; `grid-column: 1 / -1`; height `var(--traverse-topnav-height)`
- Content area: `padding: var(--traverse-content-padding)`; `grid-row: 2`; `grid-column: 2`

**Step 6.4 — Update `app.component.ts`**

Replace the `ng new`-generated `AppComponent` with a thin root:
```typescript
@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  template: '<router-outlet />',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppComponent {}
```

**Step 6.5 — Verify build**

```bash
ng build
```

Verify: 0 TypeScript errors; no template errors.

### Files Touched

| File | Change |
|------|--------|
| `src/app/core/shell/app-shell.component.ts` | Created |
| `src/app/core/shell/app-shell.component.html` | Created |
| `src/app/core/shell/app-shell.component.scss` | Created |
| `src/app/app.component.ts` | Replaced with thin root |
| `src/app/app.component.spec.ts` | Updated (remove generated test referencing old AppComponent content) |

### Not Changed
- `app.config.ts` — providers unchanged
- `auth.service.ts` — unchanged
- All other Phase 1–5 files

### Tests
- Step 6.5: `ng build` exits with code 0
- Manual: `AppShellComponent` has `changeDetection: ChangeDetectionStrategy.OnPush` (AC-004)
- Manual: `AppComponent` template contains only `<router-outlet />` — no logic
- Behaviour AC-029: Top nav renders "Traverse Workspace" and a persona chip
- Behaviour AC-030: Left sidebar contains nav items for the current role
- Behaviour AC-032: At viewport < 768px, sidebar is not visible and hamburger button is present
- Behaviour AC-033: At 768–1024px, sidebar shows icon-only; at > 1024px, shows icon + label
- Edge case EC-003: On mobile, sidebar closes after navigation event (`NavigationEnd` subscription)
- Edge case EC-006: `takeUntilDestroyed(destroyRef)` used on all subscriptions

### Success Criteria
- [ ] `AppShellComponent` renders top nav with app name and persona chip (AC-029)
- [ ] Sidebar renders 10 nav items; all except Dashboard are `disabled: true` (AC-030)
- [ ] `AppComponent` is a thin root with only `<router-outlet />` (no logic)
- [ ] Sidebar collapses on mobile (`isMobile` signal) and uses `MatSidenav` for overlay (AC-032)
- [ ] Sidebar is icon-only at tablet breakpoint (AC-033)
- [ ] `ChangeDetectionStrategy.OnPush` on both `AppComponent` and `AppShellComponent` (AC-004)
- [ ] `ng build` exits with code 0

### Risks / Compatibility
- `BreakpointObserver` (from Angular CDK) must be imported in `AppShellComponent`. Confirm `@angular/cdk` is in `package.json` (it should be, as Angular Material depends on it).
- Mobile hamburger button must be accessible: `aria-label="Toggle sidebar"` (design §4.4 template specified this).

---

## Phase 7: Shared Presentational Components

### What

Step 7.1: Create `PageHeaderComponent`.
Step 7.2: Create `ErrorDisplayComponent`.
Step 7.3: Create `LoadingSpinnerComponent`.
Step 7.4: Create `KpiStatusBadgeComponent`.

### Why

All four components are stateless presentational components — no service injection, typed `input()` signals, `OnPush`. They are used by Phase 1+ feature pages. Creating them now means Phase 1 stories can use them immediately. Each is independently testable. (AC-023 through AC-028.)

### Steps

**Step 7.1 — Create `page-header.component.ts`**

Implement per design §4.10:
- `selector: 'app-page-header'`
- Inputs: `title = input.required<string>()`, `subtitle = input<string>('')`
- Template: `<h1>{{ title() }}</h1>` with conditional `@if (subtitle())` block
- Inline styles using `--mat-sys-on-surface` and `--mat-sys-on-surface-variant` tokens
- `changeDetection: ChangeDetectionStrategy.OnPush`, `standalone: true`

**Step 7.2 — Create `error-display.component.ts`**

Implement per design §4.10:
- `selector: 'app-error-display'`
- Input: `error = input.required<string | ApiError>()`
- Computed: `protected errorMessage = computed(() => { ... })` — extracts string from `ApiError.problem.detail` or uses string directly
- Template: `<mat-card role="alert">` with `<mat-icon color="warn">error_outline</mat-icon>`
- Imports: `MatCardModule`, `MatIconModule`, `CommonModule`
- `changeDetection: ChangeDetectionStrategy.OnPush`, `standalone: true`

**Step 7.3 — Create `loading-spinner.component.ts`**

Implement per design §4.10:
- `selector: 'app-loading-spinner'`
- Input: `isLoading = input<boolean>(false)`
- Template: `@if (isLoading())` overlay with `<mat-spinner diameter="48" />`
- ARIA: `role="status" aria-label="Loading" aria-live="polite"` on overlay div
- Inline styles: `position: absolute; inset: 0; display: flex; ...`
- Imports: `MatProgressSpinnerModule`, `CommonModule`
- `changeDetection: ChangeDetectionStrategy.OnPush`, `standalone: true`

**Step 7.4 — Create `kpi-status-badge.component.ts`**

Implement per design §4.10:
- `selector: 'app-kpi-status-badge'`
- Inputs: `status = input.required<KpiStatus>()`, `label = input<string>('')`
- Template: `<mat-chip [class]="'kpi-badge kpi-badge--' + status()">{{ label() || status() }}</mat-chip>`
- Inline styles: 6 CSS rules, one per status value, each using `var(--traverse-status-{status})`
- Imports: `MatChipsModule`, `CommonModule`
- `changeDetection: ChangeDetectionStrategy.OnPush`, `standalone: true`

**Step 7.5 — Verify build**

```bash
ng build
```

Verify: 0 TypeScript errors; no template errors on any of the 4 components.

### Files Touched

| File | Change |
|------|--------|
| `src/app/shared/components/page-header/page-header.component.ts` | Created |
| `src/app/shared/components/error-display/error-display.component.ts` | Created |
| `src/app/shared/components/loading-spinner/loading-spinner.component.ts` | Created |
| `src/app/shared/components/kpi-status-badge/kpi-status-badge.component.ts` | Created |

### Not Changed
- No existing files modified in this phase

### Tests
- Step 7.5: `ng build` exits with code 0
- Manual: All 4 components have `standalone: true` and `changeDetection: ChangeDetectionStrategy.OnPush` (AC-028)
- Manual: `PageHeaderComponent` has `title = input.required<string>()` and `subtitle = input<string>('')` (AC-023)
- Manual: `ErrorDisplayComponent.error` accepts both `string` and `ApiError` inputs (AC-024)
- Manual: `LoadingSpinnerComponent` hides spinner when `isLoading = false` (AC-025)
- Manual: `KpiStatusBadgeComponent` renders different CSS class per status value (AC-026)
- Manual: No `any` types in any of the 4 files (AC-005)

### Success Criteria
- [ ] All 4 components created as standalone with `OnPush`
- [ ] `PageHeaderComponent` inputs match AC-023 signature exactly
- [ ] `ErrorDisplayComponent` handles both `string` and `ApiError` inputs (AC-024)
- [ ] `KpiStatusBadgeComponent` uses `--traverse-status-{status}` CSS tokens (AC-009)
- [ ] No hardcoded hex colours in any component stylesheet (AC-009)
- [ ] `ng build` exits with code 0

### Risks / Compatibility
- `input()` signals require Angular 17.1+. With `package.json` pinned to Angular 18+, this is guaranteed.
- `KpiStatusBadgeComponent` inline styles use string interpolation for CSS class — this is safe because `KpiStatus` is a closed string union; no user input reaches the class name.

---

## Phase 8: Environment Files, Proxy Config, Auth Feature Placeholders, and `APP_ROUTES`

### What

Step 8.1: Create `src/environments/environment.ts` and `environment.prod.ts`.
Step 8.2: Create `proxy.conf.json`.
Step 8.3: Create `LoginPlaceholderComponent` and `UnauthorizedComponent`.
Step 8.4: Create `DashboardComponent` (6 KPI placeholder tiles).
Step 8.5: Replace the Phase 4 stub `app.routes.ts` with the full `APP_ROUTES` route table.

### Why

All lazy-loaded route components must exist before `APP_ROUTES` references them — creating feature placeholders first prevents compilation errors when the route table is wired. The proxy config establishes the Angular dev server routing to all 9 backends. (AC-034 through AC-037.)

### Steps

**Step 8.1 — Create environment files**

Create `src/environments/environment.ts` and `environment.prod.ts` exactly as shown in design §4.12, with 9 service URL entries each. Verify `angular.json` references environment replacement correctly (generated by `ng new`).

**Step 8.2 — Create `proxy.conf.json`**

Create at `src/frontend/traverse-workspace/proxy.conf.json` with exactly 9 entries (design §4.13). Each entry has `target`, `changeOrigin: true`, and `pathRewrite` stripping the service prefix. Verify `angular.json` already references `"proxyConfig": "proxy.conf.json"` (NT-001 stub — Step 1.4 verified this).

**Step 8.3 — Create auth feature placeholders**

`LoginPlaceholderComponent` (`src/app/features/auth/login-placeholder.component.ts`):
- `selector: 'app-login-placeholder'`, standalone, OnPush
- Renders a `MatCard` with "Authentication not yet configured (Phase 0)" subtitle
- `proceed()` button navigates to `AppRoutes.dashboard`
- Comment: `// Phase 0 stub — remove in Phase 1 Auth story`

`UnauthorizedComponent` (`src/app/features/auth/unauthorized.component.ts`):
- `selector: 'app-unauthorized'`, standalone, OnPush
- Renders "Access Denied" heading and "Go to Dashboard" button

**Step 8.4 — Create `DashboardComponent`**

`src/app/features/dashboard/dashboard.component.ts`:
- `selector: 'app-dashboard'`, standalone, OnPush
- Template: `<app-page-header title="Dashboard" />` + grid of 6 KPI tiles
- Static `kpiSlots` array: `[{ label: 'KPI Slot 1' }, ..., { label: 'KPI Slot 6' }]`
- Each tile: `<app-kpi-status-badge [status]="'slate'" [label]="slot.label" />` + subtitle "Available in Phase 1"
- 3-column CSS Grid layout for the tile area
- Imports: `PageHeaderComponent`, `KpiStatusBadgeComponent`, `CommonModule`

**Step 8.5 — Replace stub `app.routes.ts` with full route table**

Replace the Phase 4 stub with the full `APP_ROUTES` per design §4.9:
- Root path: `AppShellComponent` as layout, `canActivate: [authGuard]`
- Child: `dashboard` → lazy-loads `DashboardComponent`
- `login` → lazy-loads `LoginPlaceholderComponent`
- `unauthorized` → lazy-loads `UnauthorizedComponent`
- Wildcard `**` → redirect to `dashboard`

**Step 8.6 — Verify build**

```bash
ng build
```

Verify: 0 TypeScript errors, 0 template errors, all lazy imports resolve.

### Files Touched

| File | Change |
|------|--------|
| `src/environments/environment.ts` | Created |
| `src/environments/environment.prod.ts` | Created |
| `proxy.conf.json` | Created |
| `src/app/features/auth/login-placeholder.component.ts` | Created |
| `src/app/features/auth/unauthorized.component.ts` | Created |
| `src/app/features/dashboard/dashboard.component.ts` | Created |
| `src/app/app.routes.ts` | Replaced (full route table replaces Phase 4 stub) |

### Not Changed
- `angular.json` `serve.options.proxyConfig` — already set by NT-001 stub (verified Phase 1)
- `auth.service.ts`, guards — unchanged

### Tests
- Step 8.6: `ng build` exits with code 0
- Manual: `environment.ts` has exactly 9 service keys (AC-034)
- Manual: `environment.prod.ts` has same shape with `production: true` (AC-035)
- Manual: `proxy.conf.json` has exactly 9 entries, `/api/workflow` through `/api/calendar` (AC-036)
- Manual: `angular.json` serve options contain `proxyConfig: "proxy.conf.json"` (AC-037)
- Manual: `DashboardComponent` has 6 KPI tiles, all with `status='slate'` (AC-031)
- Behaviour: Navigating to `/` redirects to `/dashboard` (route table test)
- Behaviour: Navigating to `/login` loads `LoginPlaceholderComponent`
- Edge case EC-004: No proxy key is a prefix of another (e.g., no `/api/ai` when `/api/ai-copilot` exists)

### Success Criteria
- [ ] `environment.ts` has 9 service URL entries (AC-034)
- [ ] `environment.prod.ts` has same 9 keys with `production: true` (AC-035)
- [ ] `proxy.conf.json` has 9 entries with `pathRewrite` strips (AC-036)
- [ ] `angular.json` references `proxy.conf.json` (AC-037)
- [ ] `DashboardComponent` renders 6 KPI placeholder tiles (AC-031)
- [ ] `APP_ROUTES` includes shell, dashboard, login, unauthorized, and wildcard routes
- [ ] `ng build` exits with code 0

### Risks / Compatibility
- The `DashboardComponent` imports `PageHeaderComponent` and `KpiStatusBadgeComponent` by path — if the workspace path aliases are not resolved by `tsconfig.app.json`, the build will fail. Verify `tsconfig.app.json` extends root `tsconfig.json` (it should, from `ng new`).

---

## Phase 9: Final Build Verification and `ng serve` Smoke Test

### What

Step 9.1: Run `ng build --configuration production` (AC-002).
Step 9.2: Run `ng build` with lint check (AC-002 zero ESLint errors).
Step 9.3: Run `ng serve` and smoke-test the running app (AC-003).
Step 9.4: Verify no `any` types in authored source (AC-005).
Step 9.5: Verify all components have `OnPush` (AC-004).

### Why

All 8 user stories converge here. The production build is the definitive "compilation contract" — zero TypeScript errors, zero lint errors, correct output. Smoke-testing `ng serve` confirms the runtime works (routing, Material theme renders, shell is visible). These checks cannot be substituted by phase-level `ng build` runs because production mode enables additional AOT and tree-shaking checks. (AC-001, AC-002, AC-003, AC-004, AC-005.)

### Steps

**Step 9.1 — Production build**

```bash
ng build --configuration production
```

Verify: exits with code 0. No TypeScript errors. No AOT compilation errors.

**Step 9.2 — ESLint (if configured)**

```bash
ng lint
```

If Angular ESLint is not configured in the `ng new` output (it is optional), add it:
```bash
ng add @angular-eslint/schematics
ng lint
```

Verify: 0 ESLint errors (AC-002).

**Step 9.3 — `ng serve` smoke test**

```bash
ng serve &
```

Verify: opens `http://localhost:4200` in browser (or use `curl -s http://localhost:4200 | grep Traverse` to confirm HTML is served). The browser should render the app shell: left sidebar, top nav with "Traverse Workspace" and persona chip, 6 KPI placeholder tiles in the content area. (AC-003.)

**Step 9.4 — TypeScript `any` audit**

```bash
grep -r "any" src/app --include="*.ts" | grep -v ".spec.ts" | grep -v "// "
```

Verify: 0 results for `any` in non-comment positions (AC-005). The design uses `unknown` for interceptor request types.

**Step 9.5 — `OnPush` audit**

```bash
grep -r "OnPush" src/app --include="*.ts" -l
```

Verify: all component files appear in this list (AC-004).

**Step 9.6 — Token audit (no hardcoded colours)**

```bash
grep -rE "#[0-9a-fA-F]{3,6}|rgb\(" src/app --include="*.scss" --include="*.css"
```

Verify: 0 results in component files. The `theme.scss` file will have status hex values — those are the token definition source, which is correct.

### Files Touched

| File | Change |
|------|--------|
| (ESLint config if not present) | `.eslintrc.json` or `eslint.config.js` created by `ng add @angular-eslint/schematics` |

### Not Changed
- All source files from Phases 1–8 — no functional changes in this phase

### Tests
- Step 9.1: `ng build --configuration production` exits with code 0 (AC-002)
- Step 9.2: `ng lint` reports 0 errors (AC-002)
- Step 9.3: `ng serve` renders app shell at `http://localhost:4200` (AC-003)
- Step 9.4: 0 `any` types in authored source (AC-005)
- Step 9.5: All components use `OnPush` (AC-004)
- Step 9.6: No hardcoded hex/rgb in component SCSS (AC-009)

### Success Criteria
- [ ] `ng build --configuration production` exits with code 0 (AC-002)
- [ ] `ng lint` exits with 0 errors (AC-002)
- [ ] `ng serve` renders the app shell with sidebar, top nav, and 6 KPI tiles (AC-003)
- [ ] No `any` types in authored TypeScript (AC-005)
- [ ] All components have `changeDetection: ChangeDetectionStrategy.OnPush` (AC-004)
- [ ] No hardcoded hex colours in component SCSS (AC-009)
- [ ] TypeScript strict mode enabled, `ng build` produces 0 TypeScript errors (AC-001, AC-005)

### Risks / Compatibility
- Production build may catch AOT-specific errors not caught by development build. If any appear, they must be resolved before this story is considered complete.
- `ng add @angular-eslint/schematics` modifies `angular.json` — verify proxy reference is still present after the add.

---

## Acceptance Criteria Traceability

| AC | Phase | Step |
|----|-------|------|
| AC-001 | Phase 1 | Step 1.4, 1.5 |
| AC-002 | Phase 9 | Step 9.1, 9.2 |
| AC-003 | Phase 9 | Step 9.3 |
| AC-004 | Phases 4, 6, 7 | All component creation steps; verified Phase 9 step 9.5 |
| AC-005 | Phase 9 | Step 9.4 |
| AC-006 | Phase 2 | Step 2.2 |
| AC-007 | Phase 2 | Step 2.2 |
| AC-008 | Phase 2 | Step 2.3 |
| AC-009 | Phase 7 | Step 7.1–7.4; verified Phase 9 step 9.6 |
| AC-010 | Phase 2 | Step 2.1 |
| AC-011 | Phase 4 | Step 4.5 |
| AC-012 | Phase 4 | Step 4.5 |
| AC-013 | Phase 4 | Step 4.5 |
| AC-014 | Phase 4 | Step 4.5 |
| AC-015 | Phase 4 | Step 4.4 |
| AC-016 | Phase 5 | Step 5.1 |
| AC-017 | Phase 5 | Step 5.2 |
| AC-018 | Phase 5 | Step 5.3 |
| AC-019 | Phase 5 | Steps 5.2, 5.3 |
| AC-020 | Phase 3 | Step 3.4 |
| AC-021 | Phase 3 | Step 3.4 |
| AC-022 | Phase 3 | Step 3.4 |
| AC-023 | Phase 7 | Step 7.1 |
| AC-024 | Phase 7 | Step 7.2 |
| AC-025 | Phase 7 | Step 7.3 |
| AC-026 | Phase 7 | Step 7.4 |
| AC-027 | Phase 3 | Step 3.3 |
| AC-028 | Phase 7 | Steps 7.1–7.4 |
| AC-029 | Phase 6 | Step 6.1–6.2 |
| AC-030 | Phase 6 | Step 6.1–6.2 |
| AC-031 | Phase 8 | Step 8.4 |
| AC-032 | Phase 6 | Step 6.2 |
| AC-033 | Phase 6 | Step 6.2 |
| AC-034 | Phase 8 | Step 8.1 |
| AC-035 | Phase 8 | Step 8.1 |
| AC-036 | Phase 8 | Step 8.2 |
| AC-037 | Phase 8 | Step 8.2 |

---

## SOLID Principles Applied

| Principle | Application in This Plan |
|-----------|--------------------------|
| **SRP** | Each file has one responsibility: `authInterceptor` adds tokens only; `correlationIdInterceptor` generates correlation IDs only; `errorInterceptor` maps errors only; `GlobalErrorHandler` displays errors only |
| **OCP** | `AppRoutes` is extended (new entries added) per Phase 1+ story without modifying existing entries; interceptor chain is extended by adding to the array in `app.config.ts` without modifying existing interceptors |
| **LSP** | `ApiError extends Error` — all `Error` consumers work correctly with `ApiError`; `authGuard` and `adminGuard` both satisfy `CanActivateFn` interface without restriction |
| **ISP** | `AuthService` exposes 4 targeted methods instead of one monolithic interface; `NavItem` has only the fields the nav shell needs |
| **DIP** | Guards depend on `AuthService` (a service injectable) not on a concrete implementation; `appConfig` wires `ErrorHandler` to `GlobalErrorHandler` via the token, not a direct import; interceptors receive `inject(AuthService)` at call time, not at construction time |

---

## Pre-Implementation Verification Checklist

Before `execute-implementation` begins Phase 1, verify:

- [ ] Worktree is on branch `story/NT-003` (`git branch --show-current`)
- [ ] `src/frontend/traverse-workspace/angular.json` and `tsconfig.json` stubs are present (NT-001 output)
- [ ] All 9 backend service directories exist under `src/services/` (NT-002 output)
- [ ] Node.js >= 18 and npm >= 9 are installed (`node --version`, `npm --version`)
- [ ] Angular CLI >= 18 is installed or can be installed (`ng version`)

<!-- Generated by skill: plan-implementation v3.4.0 | 2026-04-30 00:00 -->
