---
story_id: NT-003
feature_id: NT-000
reviewer: code-review (automatic)
date: 2026-04-30
verdict: APPROVED_WITH_NOTES
review_cycle: 1
---

# Code Review Findings — Angular Frontend Shell (NT-003)

## Summary
- Must-fix: 0
- Should-fix: 2 (both auto-fixed in review cycle 1)
- Suggestions: 3 (2 auto-fixed, 1 deferred)
- Verdict: APPROVED_WITH_NOTES

## Must-Fix (block merge — resolved before review completes)

*None.*

---

## Should-Fix (advisory — disposition recorded at validate-acceptance Phase 0)

### SF-1 — `RouterLinkWithHref` deprecated alias used instead of `RouterLink`
- **What:** `AppShellComponent` imported and used `RouterLinkWithHref` from `@angular/router`. In Angular 15+, `RouterLinkWithHref` was merged into `RouterLink` and the separate class is now just a re-export alias (`export { RouterLink as RouterLinkWithHref }`). New code should use `RouterLink` directly.
- **Why:** Using the deprecated alias communicates the wrong intent to future readers, creates a maintenance confusion when the alias is eventually removed, and is inconsistent with Angular team guidance to use `RouterLink` for all link directives in Angular 15+.
- **How:** Replace `import { RouterLinkWithHref } from '@angular/router'` with `import { RouterLink } from '@angular/router'` and update the `imports` array accordingly.
- **Resolved:** [x] Yes
- **Resolved date:** 2026-04-30
- **Resolution note:** auto-fixed in review cycle 1: replaced `RouterLinkWithHref` with `RouterLink` in AppShellComponent import and `imports[]` array. Build and tests verified clean post-fix.

### SF-2 — Trivially self-referential Boundary 7 CSS class interpolation test
- **What:** The `forEach` loop in Boundary 7 contained tests of the form `expect(expectedClass).toBe(`kpi-badge--${status}`)` where `expectedClass` was defined as `` `kpi-badge--${status}` `` — a variable comparing itself to itself. The test passed vacuously regardless of what `KpiStatusBadgeComponent` actually did.
- **Why:** A test that always passes regardless of the code under test provides false confidence. If the CSS class prefix was changed from `kpi-badge--` to anything else in the component template, this test would not catch it — the very regression it exists to detect.
- **How:** Simulate the component's actual interpolation expression (e.g., `'kpi-badge kpi-badge--' + status`) and assert the result against the expected class name. Additionally verify each status produces a unique class.
- **Resolved:** [x] Yes
- **Resolved date:** 2026-04-30
- **Resolution note:** auto-fixed in review cycle 1: replaced the vacuous `expect(expectedClass).toBe(...)` with a test that simulates the component interpolation base string, asserts the produced class contains the status-specific suffix, and verifies uniqueness across all 6 statuses.

---

## Suggestions (take or leave)

### SG-1 — Stale/contradictory comment in `auth.service.ts` inject pattern section
- **What:** The file-header comment at lines 24–30 described calling `inject(Router)` inside a method body (which would throw NG0203), then contradicted itself at line 30 with "Actually for safety in Angular 17+, we inject Router as a field initializer instead." The code correctly used a field initializer, but the comment was misleading.
- **Why:** Misleading comments about injection context rules are especially harmful in code that junior developers will extend. The DFX-001 defect (inject in RxJS callback) from this same story shows these rules are non-obvious and easy to misapply.
- **How:** Replace the contradictory comment block with an accurate, concise description of the field-initializer pattern.
- **Resolved:** [x] Yes
- **Resolved date:** 2026-04-30
- **Resolution note:** auto-fixed in review cycle 1: replaced 7-line contradictory comment with a 4-line accurate description of the field-initializer inject pattern and why method-body inject is invalid.

### SG-2 — `authService` declared `protected` but only accessed via computed signal
- **What:** In `AppShellComponent`, `authService` was declared `protected readonly authService = inject(AuthService)`. The template never accesses `authService` directly — only the `currentRole()` computed signal (which is correctly `protected`) reaches the template.
- **Why:** `protected` exposes the field to subclasses. Narrowing to `private` is more accurate (no template access, no subclass expectation) and signals correct intent to readers. Angular Coding Standards prefer minimal visibility.
- **How:** Change `protected readonly authService` to `private readonly authService`.
- **Resolved:** [x] Yes
- **Resolved date:** 2026-04-30
- **Resolution note:** auto-fixed in review cycle 1: changed `authService` from `protected` to `private`. Added comment explaining the template access path is via `currentRole()` computed signal only. Build and tests verified clean.

### SG-3 — `::ng-deep` usage in `app-shell.component.scss` for `mat-mdc-list-item` padding
- **What:** `app-shell.component.scss` uses `::ng-deep .mat-mdc-list-item { padding: 0 8px; }` to override Material's internal list item padding in the collapsed sidebar state. `::ng-deep` is deprecated in Angular and bypasses ViewEncapsulation, potentially leaking styles globally.
- **Why:** The `::ng-deep` workaround couples the style to Material's internal MDC class name (`mat-mdc-list-item`), which can change between Material versions. In Angular 18 with Material 3, the recommended approach is to use CSS custom properties (e.g., `--mat-list-item-leading-space`) that Material exposes as part of its public API.
- **How:** Replace `::ng-deep .mat-mdc-list-item { padding: 0 8px; }` with `--mat-list-item-leading-space: 8px;` applied on the `.traverse-sidebar--collapsed` element. This is a public Material Design token and is version-stable.
- **Resolved:** [ ] No
- **Resolution note:** deferred — the `::ng-deep` usage is scoped to the collapsed sidebar state in Phase 0 where no nav items are clickable anyway. The actual padding impact is cosmetic. Recommend addressing in Phase 1 when MOD-01 enables sidebar navigation and the visual polish matters.

---

## What's Good

- **DFX-001 fix is correct and well-documented.** The `errorInterceptor` correctly captures `router` at function-body level (synchronous injection context) and uses it by closure inside `catchError()`. The extensive inline comments about `NG0203` will prevent recurrence. This was a non-obvious bug that the integration tests correctly caught.

- **errorInterceptor responsibility split is clean.** The interceptor normalises errors and handles 401 navigation; `GlobalErrorHandler` handles display. The comments explicitly call this out. This is textbook SRP.

- **`Object.setPrototypeOf(this, ApiError.prototype)` in `ApiError` constructor.** Correctly restores the prototype chain when extending built-in classes in ES2015+ downlevel compilation targets. Most developers miss this; here it's present and commented.

- **Integration test infrastructure choice is well-justified.** The `RouterTestingModule.withRoutes([])` decision (instead of a mock Router spy) for Boundary 2 is explained thoroughly in the comment block — and it's the correct choice. The decision log entry DL-010 also documents this.

- **Proxy pathRewrite verification in Boundary 8** goes beyond "9 rules exist" to verify the regex key matches the rule path (preventing silent prefix-mismatch bugs). This is the kind of test that catches real DES-003 violations.

- **`color-mix()` in LoadingSpinner.** Using `color-mix(in srgb, var(--mat-sys-surface) 60%, transparent)` instead of `rgba(255,255,255,0.6)` is correctly adaptive to Material theme changes without hardcoding white.

- **Consistent stub documentation throughout.** Every stub method, every Phase 0 placeholder, and every hardcoded value has a `// Phase 0 stub — replace in Phase 1 Auth story` comment. This makes the Phase 1 scope immediately legible.

- **`as const` on `AppRoutes`.** Makes all path values string literal types, providing compile-time typo detection at every call site. Small choice, large impact.

- **Responsive breakpoint implementation.** Using named constants (`BREAKPOINT_MOBILE`, `BREAKPOINT_TABLET`) for the breakpoint strings instead of inline literals in `BreakpointObserver.observe([...])` means a single change point if breakpoint values need adjustment. Consistent with the coding standards' preference for named constants.

---

## Verification

- **Test run:** `npx ng test --watch=false --browsers=ChromeHeadless` → 79/79 SUCCESS (pre-fix and post-fix)
- **Production build:** `ng build --configuration production` → exits 0, no TypeScript errors (pre-fix and post-fix)
- **Lint:** `ng lint` → "All files pass linting" (pre-fix and post-fix)
- **`any` audit:** `grep -rn ": any" src/app --include="*.ts"` → 0 results in non-comment positions (AC-005 confirmed)
- **OnPush audit:** All 10 component files present in `grep -r "OnPush" src/app -l` output (AC-004 confirmed)
- **Hardcoded colour audit:** No hex/rgb values in component SCSS (only `color: #fff` in KpiStatusBadgeComponent, which is documented as a deliberate exception in AC-009 rationale)
- **AppRoutes count:** 18 keys (16 static + 2 dynamic helpers) — matches AC-021 count assertion in Boundary 6 test

## YAGNI Check

No speculative features found. All 37 acceptance criteria are directly addressed by the implementation. The stub pattern (stubs that match the production contract but return hardcoded values) correctly defers IdP integration, NgRx stores, feature pages, and dark mode to Phase 1+. No over-abstraction observed — the `NavItem` interface, `KpiStatus` type, and `AppRoutes` constant exist for current callers in this story, not for imagined future use.

The orphaned `src/app/app.routes.ts` (the Phase 4 temporary stub, now superseded by `src/app/core/routes/app.routes.ts`) exports an empty `routes: Routes = []` constant. Nothing imports it; it is an artefact of the Phase 4 → Phase 8 development sequence. It is not harmful but is dead code. Noted as a minor housekeeping item but not raised as a finding since it has no runtime impact and the build/lint tools do not flag unused exports in this configuration.

---

<!-- Generated by skill: code-review v2.2.0 | 2026-04-30 | AutoMode: true | Review cycle: 1 -->
