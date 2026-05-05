-- docker/postgres/init.sql
--
-- Runs ONCE on first postgres container initialisation (empty data volume).
-- The postgres Docker image executes all *.sql files in /docker-entrypoint-initdb.d/
-- alphabetically on first start; subsequent starts with an existing data volume skip
-- this directory entirely.
--
-- Creates one database per service following the database-per-service pattern
-- (.NET Coding Standards section 13). Each service owns its own schema, extensions,
-- and EF Core migrations -- nothing is created here beyond the empty database.
--
-- Why no IF NOT EXISTS: these databases must not already exist on first initialisation.
-- If a re-run is needed (e.g., after adding a new service database), run:
--   docker compose down -v
--   docker compose up
-- This destroys all volumes and re-runs init.sql cleanly (see README Troubleshooting).
--
-- Why plain SQL (no psql meta-commands): the postgres Docker image executes init scripts
-- via psql internally, but using plain CREATE DATABASE keeps the script portable across
-- any postgres image variant and avoids psql-specific syntax errors in non-interactive mode.
--
-- Ownership: each CREATE DATABASE runs as the POSTGRES_USER defined in docker-compose.yml
-- (default: traverse). That user becomes the database owner with full access. Production
-- deployments will use per-service users with least-privilege grants (out of scope Phase 0).

CREATE DATABASE traverse_workflow;
CREATE DATABASE traverse_kpi;
CREATE DATABASE traverse_admin;
CREATE DATABASE traverse_notifications;
CREATE DATABASE traverse_reporting;
CREATE DATABASE traverse_aicopilot;
CREATE DATABASE traverse_compliance;
CREATE DATABASE traverse_search;
CREATE DATABASE traverse_calendar;
