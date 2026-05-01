# docker/workflow/Dockerfile -- Traverse Workflow API
#
# Multi-stage build following .NET Coding Standards section 18.
# Stage 1 (build): SDK image restores and publishes the project.
# Stage 2 (runtime): ASP.NET runtime image runs as non-root appuser.
#
# NOTE: This Dockerfile requires the Traverse.Workflow.Api project produced by NT-005.
#       Building this image before NT-005 is merged will fail with "project not found".
#       docker compose up (without --profile api) succeeds without building this image.
#       Run 'docker compose --profile api up' only after NT-005 merges.
#
# The build context is the repo root (.) so the full src/ tree is available for
# dotnet restore, which may need shared library projects from src/shared/.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution manifest and global SDK pin first (layer-cache friendly).
# These files change rarely; restoring from cache unless they change saves time.
COPY ["Traverse.sln", "global.json", "Directory.Build.props", "./"]

# Copy the full source tree (needed because Traverse.Workflow.Api may reference
# shared libraries under src/shared/, which must be present for dotnet restore).
COPY ["src/", "src/"]

# Restore only the specific project to minimise restore scope.
RUN dotnet restore "src/services/workflow/Traverse.Workflow.Api/Traverse.Workflow.Api.csproj"

# Publish the release build to /app/publish.
# UseAppHost=false: do not produce a native executable wrapper (not needed in containers).
RUN dotnet publish "src/services/workflow/Traverse.Workflow.Api/Traverse.Workflow.Api.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ---------------------------------------------------------------------------
# Stage 2: runtime image (smaller than SDK; no build tools)
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create a non-root user and transfer directory ownership.
# Running as non-root is a container security best practice (.NET Coding Standards section 18).
RUN useradd -m appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .

# ASP.NET Core listens on HTTP port 8080 inside the container.
# HTTPS termination is handled at the deployment layer (out of scope Phase 0).
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Health probe used by docker compose depends_on (condition: service_healthy).
# wget is present in the aspnet base image; curl is not guaranteed.
HEALTHCHECK --interval=30s --timeout=10s \
    CMD wget -qO- http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "Traverse.Workflow.Api.dll"]
