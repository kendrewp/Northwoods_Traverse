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

### Run Locally

Docker Compose setup is added in NT-004. Once available:

```bash
docker compose up
```

### Port Map

| Service | Port | Status |
|---------|------|--------|
| Traverse.Workflow.Api | 5001 | NT-005 |
| Traverse.KPI.Api | 5002 | NT-005 |
| Traverse.Admin.Api | 5003 | NT-005 |
| Traverse.Notifications.Api | 5004 | NT-005 |
| Traverse.Reporting.Api | 5005 | NT-005 |
| Traverse.AICopilot.Api | 5006 | NT-005 |
| Traverse.Compliance.Api | 5007 | NT-005 |
| Traverse.Search.Api | 5008 | NT-005 |
| Traverse.Calendar.Api | 5009 | NT-005 |

### Project Layout

```
src/
├── shared/              # Shared libraries (AI abstractions + infrastructure libs)
├── services/            # One folder per microservice API (placeholder until NT-005)
└── frontend/
    └── traverse-workspace/  # Angular workspace (placeholder until NT-003)

docker/                  # Per-service Dockerfiles (placeholder until NT-005) + postgres init
```

**Note on `.gitkeep` files:** Placeholder directories under `src/services/`, `docker/`, and
`src/frontend/traverse-workspace/` contain `.gitkeep` files so git tracks the directory.
Do not delete `.gitkeep` files from placeholder directories — their removal causes git to
stop tracking the empty directory. They will be removed naturally when real project files
are added to each directory in NT-002 through NT-005.
