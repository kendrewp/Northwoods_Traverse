// src/app/core/shell/app-shell.component.ts
//
// AppShellComponent — the authenticated application layout shell.
//
// RESPONSIBILITY (SRP): This smart container component has one responsibility —
// managing the top nav and sidebar layout frame for all authenticated pages.
// It owns nav visibility state and responsive layout state. It does not own
// feature data (each feature page owns its own state via its store).
//
// SMART CONTAINER PATTERN: AppShellComponent injects services and manages state
// (Angular Coding Standards §3). The sidebar nav items are driven by AuthService
// to implement persona-based filtering. Feature pages rendered in <router-outlet>
// are presentational components that receive inputs from their own stores.
//
// RESPONSIVE BEHAVIOUR:
// - Desktop (> 1024px): sidebar expanded, icon + label, 220px wide
// - Tablet (768–1024px): sidebar icon-only, 64px wide; sidebarExpanded = false
// - Mobile (< 768px): sidebar hidden; MatSidenav overlay on hamburger tap
//
// ANGULAR CDK BreakpointObserver drives the isMobile/sidebarExpanded signals.
// toSignal() bridges the Observable<BreakpointState> to a signal, enabling
// OnPush change detection to react when the breakpoint changes.
//
// ROUTER NAVIGATION EVENT: On mobile, the sidebar closes after navigation to
// prevent the overlay from remaining open after the user taps a nav item.
// This is implemented via Router events subscription with takeUntilDestroyed.
// (EC-003 in the design document.)
//
// NAV ITEMS: Defined as a readonly array of NavItem objects. Each item has a
// `roles` field — the nav list filters to items where currentRole() is in the
// roles array. Items with `disabled: true` are rendered but not interactive.
//
// PHASE 0 STUB: All nav items except Dashboard are disabled: true.
// Each Phase 1+ story enables its nav item as part of its implementation.
//
// AC-029: Top nav with app name and persona chip
// AC-030: Sidebar with role-filtered nav items
// AC-032: Mobile hamburger + sidebar collapse
// AC-033: Icon-only at tablet, icon+label at desktop

import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { BreakpointObserver } from '@angular/cdk/layout';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';
import { MatChipsModule } from '@angular/material/chips';
import { MatSidenavModule } from '@angular/material/sidenav';
import { TitleCasePipe } from '@angular/common';
import { RouterLinkWithHref } from '@angular/router';
import { filter } from 'rxjs/operators';
import { AuthService } from '@core/services/auth.service';
import { AppRoutes } from '@core/routes/app-routes';
import { NavItem } from './nav-item.model';

/** Breakpoint boundary for mobile layout (< 768px). */
const BREAKPOINT_MOBILE = '(max-width: 767px)';
/** Breakpoint boundary for tablet layout (768px – 1024px). */
const BREAKPOINT_TABLET = '(max-width: 1024px)';

/**
 * App shell — the outer layout frame containing the top toolbar and sidebar nav.
 *
 * All authenticated routes are rendered inside this component's <router-outlet>.
 * The sidebar and toolbar adapt to the current viewport and persona role.
 */
@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [
    RouterOutlet,
    RouterLinkWithHref,
    MatToolbarModule,
    MatIconModule,
    MatButtonModule,
    MatListModule,
    MatChipsModule,
    MatSidenavModule,
    TitleCasePipe,
  ],
  templateUrl: './app-shell.component.html',
  styleUrls: ['./app-shell.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppShellComponent implements OnInit {
  // ─── Dependencies ─────────────────────────────────────────────────────────
  protected readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly breakpointObserver = inject(BreakpointObserver);
  private readonly destroyRef = inject(DestroyRef);

  // ─── State Signals ────────────────────────────────────────────────────────

  /** The current user's active persona/role — drives nav filtering and the chip label. */
  protected readonly currentRole = computed(() => this.authService.getCurrentRole());

  /** Whether the sidebar is fully expanded (icon + label). False = icon-only (tablet). */
  protected readonly sidebarExpanded = signal<boolean>(true);

  /** Whether the viewport is mobile (< 768px). Drives MatSidenav vs. inline sidebar. */
  protected readonly isMobile = signal<boolean>(false);

  // ─── Nav Item Definitions ─────────────────────────────────────────────────
  // Phase 0: all nav items except Dashboard are disabled: true.
  // Phase 1+ stories set disabled: false when their route is implemented.

  /**
   * All possible navigation items. Filtered by currentRole() before display.
   */
  protected readonly allNavItems: readonly NavItem[] = [
    {
      label: 'Dashboard',
      icon: 'dashboard',
      route: AppRoutes.dashboard,
      roles: ['social-worker', 'supervisor', 'director', 'deputy', 'admin'],
      disabled: false,  // Phase 0: Dashboard is the only enabled route
    },
    {
      label: 'Cases',
      icon: 'folder_open',
      route: AppRoutes.cases,
      roles: ['social-worker', 'supervisor', 'director', 'deputy'],
      disabled: true,  // Phase 0 stub — enabled in MOD-01 Phase 2
    },
    {
      label: 'Work Items',
      icon: 'assignment',
      route: AppRoutes.workItems,
      roles: ['social-worker', 'supervisor'],
      disabled: true,  // Phase 0 stub — enabled in MOD-01 Phase 2
    },
    {
      label: 'Team',
      icon: 'group',
      route: AppRoutes.workItems,
      roles: ['supervisor', 'director'],
      disabled: true,  // Phase 0 stub — enabled in MOD-01 Phase 2
    },
    {
      label: 'Reports',
      icon: 'bar_chart',
      route: AppRoutes.reporting,
      roles: ['supervisor', 'director', 'deputy'],
      disabled: true,  // Phase 0 stub — enabled in MOD-05 Phase 3
    },
    {
      label: 'Admin',
      icon: 'settings',
      route: AppRoutes.admin,
      roles: ['admin'],
      disabled: true,  // Phase 0 stub — enabled in MOD-07 Phase 1
    },
    {
      label: 'Compliance',
      icon: 'gavel',
      route: AppRoutes.compliance,
      roles: ['admin', 'director'],
      disabled: true,  // Phase 0 stub — enabled in MOD-10 Phase 5
    },
    {
      label: 'Search',
      icon: 'search',
      route: AppRoutes.search,
      roles: ['social-worker', 'supervisor', 'director', 'deputy', 'admin'],
      disabled: true,  // Phase 0 stub — enabled in MOD-08 Phase 5
    },
    {
      label: 'Calendar',
      icon: 'calendar_today',
      route: AppRoutes.calendar,
      roles: ['social-worker'],
      disabled: true,  // Phase 0 stub — enabled in MOD-09 Phase 5
    },
    {
      label: 'Statewide',
      icon: 'public',
      route: AppRoutes.deputyDashboard,
      roles: ['deputy'],
      disabled: true,  // Phase 0 stub — enabled in MOD-12 Phase 6
    },
  ];

  /**
   * Nav items visible to the current persona — derived from allNavItems filtered
   * by whether the current role appears in each item's roles array.
   */
  protected readonly visibleNavItems = computed<readonly NavItem[]>(() => {
    const role = this.currentRole();
    return this.allNavItems.filter(item => item.roles.includes(role));
  });

  // ─── Lifecycle ────────────────────────────────────────────────────────────

  ngOnInit(): void {
    this.initBreakpointObserver();
    this.initMobileSidebarClose();
  }

  // ─── Public Methods ────────────────────────────────────────────────────────

  /**
   * Toggles the sidebar expanded state.
   * On mobile: opens/closes the MatSidenav overlay.
   * On tablet/desktop: expands/collapses between icon+label and icon-only modes.
   */
  toggleSidebar(): void {
    this.sidebarExpanded.update(expanded => !expanded);
  }

  // ─── Private Methods ──────────────────────────────────────────────────────

  /**
   * Initialises the CDK BreakpointObserver to drive responsive layout signals.
   *
   * Tablet breakpoint (≤ 1024px): collapses sidebar to icon-only.
   * Mobile breakpoint (≤ 767px): hides sidebar and enables MatSidenav overlay.
   */
  private initBreakpointObserver(): void {
    this.breakpointObserver
      .observe([BREAKPOINT_MOBILE, BREAKPOINT_TABLET])
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(state => {
        const mobile = state.breakpoints[BREAKPOINT_MOBILE] ?? false;
        const tablet = state.breakpoints[BREAKPOINT_TABLET] ?? false;

        this.isMobile.set(mobile);
        // On mobile or tablet, default to collapsed/icon-only sidebar
        this.sidebarExpanded.set(!mobile && !tablet);
      });
  }

  /**
   * On mobile, closes the sidebar overlay after each navigation event.
   *
   * This prevents the sidebar from remaining open after the user taps a nav
   * item. On tablet/desktop, navigation does not affect sidebar state.
   * (EC-003 in the design document.)
   */
  private initMobileSidebarClose(): void {
    this.router.events
      .pipe(
        filter(event => event instanceof NavigationEnd),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => {
        if (this.isMobile()) {
          this.sidebarExpanded.set(false);
        }
      });
  }
}
