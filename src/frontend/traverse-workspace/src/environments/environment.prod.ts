// src/environments/environment.prod.ts
//
// Production environment configuration.
//
// These placeholder URLs must be replaced with actual production service hostnames
// before deploying. In CI/CD pipelines, these values are injected at build time
// via environment variable substitution or configuration management.
//
// The file shape MUST remain identical to environment.ts — angular.json references
// this file for fileReplacements in the production build configuration, and any
// missing keys will cause runtime errors in production.
//
// Phase 0 stub — replace placeholder URLs with actual hostnames in CI/CD pipeline.
//
// AC-035: same shape as environment.ts with production: true

/**
 * Production environment — placeholder production service URLs.
 *
 * Replace `api.traverse.example.com` with the actual production API gateway
 * hostname before deploying. Each path segment corresponds to the backend
 * service's route prefix on the API gateway.
 */
// Phase 0 stub — replace placeholder URLs in CI/CD pipeline configuration
export const environment = {
  production: true,
  services: {
    /** MOD-02 Workflow Execution Engine */
    workflow:      'https://api.traverse.example.com/workflow',
    /** MOD-03 KPI / SLA Policy Engine */
    kpi:           'https://api.traverse.example.com/kpi',
    /** MOD-07 Admin Configuration Suite */
    admin:         'https://api.traverse.example.com/admin',
    /** MOD-04 Notification Centre */
    notifications: 'https://api.traverse.example.com/notifications',
    /** MOD-05 Reporting Dashboard */
    reporting:     'https://api.traverse.example.com/reporting',
    /** MOD-06 AI Copilot */
    aiCopilot:     'https://api.traverse.example.com/ai-copilot',
    /** MOD-10 Compliance Monitoring */
    compliance:    'https://api.traverse.example.com/compliance',
    /** MOD-08 Global Search */
    search:        'https://api.traverse.example.com/search',
    /** MOD-09 Calendar / Scheduling */
    calendar:      'https://api.traverse.example.com/calendar',
  },
} as const;
