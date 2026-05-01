// src/environments/environment.ts
//
// Development environment configuration.
//
// All backend services run locally via Docker Compose on ports 5001–5009.
// These URLs are referenced by the proxy.conf.json dev server proxy rules.
// Components and services should access these via `environment.services.{service}`
// rather than hardcoding localhost URLs.
//
// This file is replaced by environment.prod.ts in production builds via
// the angular.json fileReplacements configuration.
//
// AC-034: 9 service URL entries

/**
 * Development environment — local Docker Compose service URLs.
 *
 * `as const` makes all values string literal types, preventing accidental
 * mutation and enabling type inference at each usage site.
 */
export const environment = {
  production: false,
  services: {
    /** MOD-02 Workflow Execution Engine — Traverse.Workflow.Api */
    workflow:      'http://localhost:5001/api',
    /** MOD-03 KPI / SLA Policy Engine — Traverse.KPI.Api */
    kpi:           'http://localhost:5002/api',
    /** MOD-07 Admin Configuration Suite — Traverse.Admin.Api */
    admin:         'http://localhost:5003/api',
    /** MOD-04 Notification Centre — Traverse.Notifications.Api */
    notifications: 'http://localhost:5004/api',
    /** MOD-05 Reporting Dashboard — Traverse.Reporting.Api */
    reporting:     'http://localhost:5005/api',
    /** MOD-06 AI Copilot — Traverse.AICopilot.Api */
    aiCopilot:     'http://localhost:5006/api',
    /** MOD-10 Compliance Monitoring — Traverse.Compliance.Api */
    compliance:    'http://localhost:5007/api',
    /** MOD-08 Global Search — Traverse.Search.Api */
    search:        'http://localhost:5008/api',
    /** MOD-09 Calendar / Scheduling — Traverse.Calendar.Api */
    calendar:      'http://localhost:5009/api',
  },
} as const;
