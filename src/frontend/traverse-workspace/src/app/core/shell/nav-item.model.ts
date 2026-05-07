// src/app/core/shell/nav-item.model.ts
//
// NavItem — defines a single navigation item in the sidebar.
//
// DESIGN: The `roles` array drives persona-based nav filtering in AppShellComponent.
// Items where the current role is not in `roles` are not rendered. This keeps
// the filtering logic simple and data-driven rather than requiring switch statements
// per persona.
//
// The `disabled` flag marks Phase 0 placeholder items (routes not yet implemented).
// Disabled items are rendered in the nav list to communicate future feature presence
// but do not navigate. In Phase 1+ stories, each feature removes `disabled: true`
// from its nav item entry in AppShellComponent as part of its implementation.
//
// SOLID ISP: This interface contains only the fields that AppShellComponent needs
// to render a nav item. It does not carry routing state, component references, or
// Angular-specific types — keeping the model portable and independently testable.

/**
 * Defines a single entry in the sidebar navigation list.
 *
 * Used by AppShellComponent to render the persona-filtered nav items.
 * All Phase 0 nav items except Dashboard are marked disabled: true.
 */
export interface NavItem {
  /** Display label shown when sidebar is expanded (desktop mode). */
  label: string;
  /** Material icon name (from the 'material-icons' or 'material-symbols' font). */
  icon: string;
  /** Route path — must match a value from AppRoutes constants. */
  route: string;
  /** Persona roles that see this nav item. Empty array means hidden from all. */
  roles: string[];
  /**
   * When true, the nav item is rendered but not interactive.
   * Phase 0 placeholder: all routes except Dashboard are disabled.
   * Remove in Phase 1+ when the route's feature is implemented.
   */
  disabled?: boolean;
}
