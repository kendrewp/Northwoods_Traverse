@angular_shell
Feature: Shared presentational components and persona nav shell render correctly
  As a developer building feature pages
  I want PageHeaderComponent, ErrorDisplayComponent, LoadingSpinnerComponent, and KpiStatusBadgeComponent
  to exist with typed inputs and render correctly, and the app shell to display the correct navigation structure
  So that I can compose them into feature screens without writing them from scratch

  # Traces to: design_angular_shell.md §2 US-006 (shared components ACs 023–028)
  #            US-007 (persona nav shell ACs 029–033)
  #            and US-002 AC-007 (KPI status token runtime verification)

  # --- Automatable Acceptance Criteria ---

  @AC-007
  Scenario Outline: KPI status CSS custom properties are declared globally and resolve to the correct colour
    Given the Angular application has bootstrapped and global styles have been applied
    When a styled element uses the CSS custom property "<token_name>"
    Then the computed style value for the property is "<expected_hex>"

    Examples:
      | token_name                  | expected_hex |
      | --traverse-status-green     | #1a7a4a      |
      | --traverse-status-yellow    | #b45309      |
      | --traverse-status-red       | #c0152b      |
      | --traverse-status-fuchsia   | #a21caf      |
      | --traverse-status-emerald   | #065f46      |
      | --traverse-status-slate     | #475569      |

  @AC-023
  Scenario: PageHeaderComponent renders with a required title and optional subtitle
    Given the PageHeaderComponent is rendered with title "Case Management" and subtitle "Active cases for today"
    Then a heading element displaying "Case Management" is visible
    And a subtitle element displaying "Active cases for today" is visible

  @AC-023
  Scenario: PageHeaderComponent renders without a subtitle when none is provided
    Given the PageHeaderComponent is rendered with only the title "Dashboard"
    Then a heading element displaying "Dashboard" is visible
    And no subtitle element is rendered in the page header

  @AC-024
  Scenario Outline: ErrorDisplayComponent renders the error message for different input types
    Given the ErrorDisplayComponent is rendered with an error input of type "<input_type>"
    And the error value is "<error_value>"
    Then a Material card with role "alert" is displayed
    And the card body contains the text "<expected_message>"

    Examples:
      | input_type    | error_value                               | expected_message              |
      | plain string  | Something went wrong                      | Something went wrong          |
      | ApiError      | ApiError with detail "Record not found"   | Record not found              |
      | ApiError      | ApiError with title "Not Found" no detail | Not Found                     |

  @AC-025
  Scenario Outline: LoadingSpinnerComponent shows or hides the spinner overlay based on isLoading input
    Given the LoadingSpinnerComponent is rendered with isLoading set to "<is_loading>"
    Then the spinner overlay is "<visibility>"

    Examples:
      | is_loading | visibility |
      | true       | visible    |
      | false      | hidden     |

  @AC-026
  Scenario Outline: KpiStatusBadgeComponent renders a coloured chip using the correct status token
    Given the KpiStatusBadgeComponent is rendered with status "<status>" and label "<label>"
    Then a chip element is displayed
    And the chip's background colour comes from the "--traverse-status-<status>" CSS custom property
    And the chip displays the text "<displayed_text>"

    Examples:
      | status  | label       | displayed_text |
      | green   | On Track    | On Track       |
      | yellow  | Warning     | Warning        |
      | red     | Breached    | Breached       |
      | fuchsia | Escalated   | Escalated      |
      | emerald | Completed   | Completed      |
      | slate   |             | slate          |

  @AC-029
  Scenario: Top navigation bar displays the application name and the current user's persona chip
    Given a user is authenticated with the role "social-worker"
    And the app shell is rendered
    When the user views the top navigation bar
    Then the text "Traverse Workspace" is visible in the top navigation bar
    And a persona chip displaying "Social Worker" is visible in the top navigation bar

  @AC-030
  Scenario: Left sidebar renders navigation items appropriate to the current persona
    Given a user is authenticated with the role "social-worker"
    And the app shell is rendered on a desktop viewport wider than 1024 pixels
    When the user views the left sidebar
    Then the sidebar is 220 pixels wide
    And the sidebar displays a navigation item for "Dashboard"
    And the sidebar displays a navigation item for "Cases"
    And the sidebar does not display the "Admin" navigation item

  @AC-031
  Scenario: Dashboard content area renders six KPI placeholder tiles
    Given a user is authenticated and the dashboard route is active
    When the dashboard content area is rendered
    Then exactly 6 KPI placeholder tiles are visible
    And each tile displays a KpiStatusBadgeComponent with status "slate"
    And the tiles are labelled "KPI Slot 1" through "KPI Slot 6"
    And each tile shows the subtitle "Available in Phase 1"

  @AC-032
  Scenario: Sidebar collapses and a hamburger menu toggle appears on mobile viewports
    Given the app shell is rendered
    When the viewport width is set to 375 pixels (mobile)
    Then the left sidebar is not visible by default
    And a hamburger menu button is visible in the top navigation bar
    And tapping the hamburger menu button toggles the sidebar overlay open and closed

  @AC-033
  Scenario Outline: Sidebar display mode adapts to the current viewport width
    Given the app shell is rendered
    When the viewport width is "<viewport_px>" pixels
    Then the sidebar display mode is "<sidebar_mode>"

    Examples:
      | viewport_px | sidebar_mode                              |
      | 1280        | icon and text label visible (220px wide)  |
      | 900         | icon only — text labels hidden (64px wide)|
      | 375         | sidebar hidden — hamburger toggle shown   |

  # --- Human-Validation-Only Criteria ---
  # The following acceptance criteria require inspection of source file contents
  # and cannot be expressed as automated runtime Gherkin scenarios.
  # They are documented here for traceability and will be handled by validate-acceptance Phase 4.

  # @AC-027 [Human validation]: KpiStatus type is exported as a TypeScript string union:
  #   type KpiStatus = 'green' | 'yellow' | 'red' | 'fuchsia' | 'emerald' | 'slate'
  # Reason: TypeScript type aliases exist only at compile time; they have no runtime
  #   representation that can be asserted in a browser-observable scenario.
  #   The runtime behaviour (chip colours) is covered by AC-026.
  # Reviewer: Dev Lead

  # @AC-028 [Human validation]: All four shared components declare
  #   changeDetection: ChangeDetectionStrategy.OnPush and are standalone components.
  # Reason: OnPush vs Default detection is an internal Angular mechanism; from the outside
  #   both produce identical UI. Standalone vs module-based is a compile-time structure.
  #   The OnPush rule across all components is covered as a workspace-wide check by AC-004.
  # Reviewer: Dev Lead (covered by AC-004 code-level check)

# Generated by skill: generate-gherkin v2.2.0 | 2026-04-30 10:00
