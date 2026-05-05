# Northwoods_Traverse
Northwoods Traverse Application

## Getting Started

### Prerequisites

| Tool | Minimum Version | Purpose |
|------|----------------|---------|
| [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10) | 10.0.x | Backend build and run |
| [Node.js LTS](https://nodejs.org/) | 20.x | Angular workspace (NT-003) |
| [Angular CLI](https://angular.dev/tools/cli) | 18.x | Frontend scaffolding and build (NT-003) |
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) | 4.x | Local dev environment (NT-004) |

### Build the Backend

```bash
dotnet build Traverse.sln
```

This builds the two existing AI shared libraries. Additional projects are added as
Phase 0 stories complete (NT-002 adds shared libs; NT-005 adds service API stubs).

---

## Quickstart (Local Dev)

### One-time setup

```bash
git clone <repo>
cd Northwoods_Traverse
cp .env.example .env       # create your local credential file (gitignored)
```

The `.env` file is gitignored — it holds developer-local overrides. The defaults in
`.env.example` are safe for local development. Edit `.env` only if you need to change
a port (see Troubleshooting below).

### Start the full stack

```bash
docker compose up          # foreground — Ctrl-C to stop
docker compose up -d       # detached (background)
```

**First run:** takes 3–5 minutes (image pulls + database init). Subsequent runs are under 30 seconds.

All three infrastructure services (PostgreSQL, RabbitMQ, n8n) start in the default profile.
The nine API stub services require the `--profile api` flag and the NT-005 service projects:

```bash
docker compose --profile api up -d   # full stack (requires NT-005 to be merged)
```

### Verify it's working

```bash
docker compose ps                        # all services should show "running (healthy)"
curl http://localhost:5001/health/live   # workflow API: {"status":"Healthy"}
```

Open in your browser:

- `http://localhost:15672` — RabbitMQ management UI (credentials: `traverse` / `traverse_dev`)
- `http://localhost:5678` — n8n editor (no login required in local dev)

### Run the Angular workspace

```bash
cd src/frontend/traverse-workspace
npm install                               # one-time dependency install
ng serve --proxy-config proxy.conf.json   # http://localhost:4200
```

The Angular dev server runs outside Docker. The `proxy.conf.json` forwards `/api/{service}`
requests to the appropriate backend port (5001–5009) on `localhost`.

### Stop and clean up

```bash
docker compose down        # stop containers; data volumes are preserved
docker compose down -v     # stop containers AND delete all data volumes
```

`docker compose down -v` resets all databases. Use this when you need to re-run the database
initialisation script (e.g., after adding a new service database to `docker/postgres/init.sql`).

---

## Port Map

| Service | Default Port | Env Override |
|---------|-------------|--------------|
| PostgreSQL | 5432 | `POSTGRES_PORT` |
| RabbitMQ AMQP | 5672 | `RABBITMQ_PORT` |
| RabbitMQ Management UI | 15672 | `RABBITMQ_MGMT_PORT` |
| n8n editor | 5678 | `N8N_PORT` |
| Workflow API | 5001 | `WORKFLOW_PORT` |
| KPI/SLA Policy API | 5002 | `KPI_PORT` |
| Admin Configuration API | 5003 | `ADMIN_PORT` |
| Notifications API | 5004 | `NOTIFICATIONS_PORT` |
| Reporting API | 5005 | `REPORTING_PORT` |
| AI Copilot API | 5006 | `AICOPILOT_PORT` |
| Compliance Framework API | 5007 | `COMPLIANCE_PORT` |
| Universal Search API | 5008 | `SEARCH_PORT` |
| Calendar API | 5009 | `CALENDAR_PORT` |

To override a port, add the variable to your `.env` file (not `.env.example`). Example:

```ini
POSTGRES_PORT=5433
```

---

## Troubleshooting

| Symptom | Cause | Fix |
|---------|-------|-----|
| Port 5432 already in use | Local PostgreSQL install running | Add `POSTGRES_PORT=5433` to your `.env` |
| `init.sql` didn't run / service database missing | postgres-data volume already existed from a previous run | `docker compose down -v && docker compose up` |
| API stubs in restart loop | postgres or rabbitmq still starting up | Wait 60 s; check `docker compose logs workflow-api` |
| n8n login prompt appears | `N8N_BASIC_AUTH_ACTIVE` was set to `true` accidentally | Remove the env var override from `.env`; restart: `docker compose restart n8n` |
| n8n cannot save workflows (Linux only) | n8n runs as uid 1000; bind-mounted directory has different owner | Run `chown 1000:1000 n8n/workflows` on the host, then restart n8n |
| Changes to a Dockerfile have no effect | Docker reused a cached image | Force rebuild: `docker compose up --build` |

### Security Note

The default credentials in `.env.example` (`traverse` / `traverse_dev`) are for **local
development only**. Never reuse them in any non-local environment.

n8n is configured in no-authentication mode (`N8N_BASIC_AUTH_ACTIVE=false`). This is safe
on a developer laptop where port 5678 is not exposed to the network. Production deployments
**must** enable n8n authentication and restrict network access.

---

## Project Layout

```
src/
├── shared/              # Shared libraries (AI abstractions + infrastructure libs)
├── services/            # One folder per microservice API (stub projects from NT-005)
└── frontend/
    └── traverse-workspace/  # Angular workspace (from NT-003)

docker/                  # Per-service Dockerfiles (multi-stage, .NET 10 SDK/runtime)
├── postgres/
│   └── init.sql         # Creates nine service databases on first postgres start
├── workflow/Dockerfile
├── kpi/Dockerfile
├── admin/Dockerfile
├── notifications/Dockerfile
├── reporting/Dockerfile
├── aicopilot/Dockerfile
├── compliance/Dockerfile
├── search/Dockerfile
└── calendar/Dockerfile

n8n/
└── workflows/           # Version-controlled n8n workflow JSON files (bind-mounted into container)

docker-compose.yml       # Single-command local environment (NT-004)
.env.example             # Environment variable template (copy to .env before first run)
```
