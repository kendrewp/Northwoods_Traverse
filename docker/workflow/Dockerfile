# ============================================================================
# Traverse.Workflow.Api — Multi-stage Dockerfile
#
# Stage layout (per .NET Coding Standards §18):
#   restore  : copy only .csproj files and restore NuGet packages
#              (layer-cache friendly — changes to source code do not
#               invalidate the restore layer)
#   build    : copy full source; dotnet build in Release
#   publish  : dotnet publish in Release; output to /app/publish
#   runtime  : aspnet runtime image only; no SDK; non-root user; minimal image
#
# Build context: repository root (.)
# All COPY paths are relative to the repository root.
#
# Usage:
#   docker build -f docker/workflow/Dockerfile .
#   docker compose --profile api up --build
# ============================================================================

# ---------------------------------------------------------------------------
# Stage 1: restore
# Copy only project files first. NuGet restore is the slowest step — keeping
# it in a dedicated layer means Docker reuses it on every source-only change.
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /src

# Copy solution file and global.json — needed by dotnet restore to resolve
# all project references in the solution graph.
COPY global.json ./
COPY Traverse.sln ./

# Copy shared library .csproj files (dependencies of the API project)
COPY src/shared/Traverse.Domain.Primitives/Traverse.Domain.Primitives.csproj \
     src/shared/Traverse.Domain.Primitives/
COPY src/shared/Traverse.Infrastructure.Auth/Traverse.Infrastructure.Auth.csproj \
     src/shared/Traverse.Infrastructure.Auth/
COPY src/shared/Traverse.Infrastructure.Http/Traverse.Infrastructure.Http.csproj \
     src/shared/Traverse.Infrastructure.Http/
COPY src/shared/Traverse.Infrastructure.Messaging/Traverse.Infrastructure.Messaging.csproj \
     src/shared/Traverse.Infrastructure.Messaging/
COPY src/shared/Traverse.Infrastructure.Observability/Traverse.Infrastructure.Observability.csproj \
     src/shared/Traverse.Infrastructure.Observability/
COPY src/shared/Traverse.Infrastructure.Persistence/Traverse.Infrastructure.Persistence.csproj \
     src/shared/Traverse.Infrastructure.Persistence/

# Copy the API project file
COPY src/services/workflow/Traverse.Workflow.Api/Traverse.Workflow.Api.csproj \
     src/services/workflow/Traverse.Workflow.Api/

# Restore only this project and its transitive dependencies.
RUN dotnet restore src/services/workflow/Traverse.Workflow.Api/Traverse.Workflow.Api.csproj

# ---------------------------------------------------------------------------
# Stage 2: build
# Copy all source files and compile in Release configuration.
# --no-restore reuses the packages downloaded in stage 1.
# ---------------------------------------------------------------------------
FROM restore AS build
WORKDIR /src

# Copy shared library source
COPY src/shared/ src/shared/

# Copy service project source
COPY src/services/workflow/Traverse.Workflow.Api/ src/services/workflow/Traverse.Workflow.Api/

RUN dotnet build src/services/workflow/Traverse.Workflow.Api/Traverse.Workflow.Api.csproj \
    -c Release \
    --no-restore

# ---------------------------------------------------------------------------
# Stage 3: publish
# Produce self-contained publish output. /p:UseAppHost=false suppresses the
# native executable wrapper — not needed inside a Linux container.
# ---------------------------------------------------------------------------
FROM build AS publish

RUN dotnet publish src/services/workflow/Traverse.Workflow.Api/Traverse.Workflow.Api.csproj \
    -c Release \
    --no-build \
    /p:UseAppHost=false \
    -o /app/publish

# ---------------------------------------------------------------------------
# Stage 4: runtime
# Final image: aspnet runtime only (no SDK). Approximately 200MB vs 800MB+ SDK.
# Non-root user for security (CIS Benchmark for containers).
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create a non-root user and group; chown the working directory.
# adduser is used for Alpine-compatible images; the aspnet base image supports it.
RUN adduser --disabled-password --gecos "" appuser \
    && chown -R appuser /app
USER appuser

# Copy published output from the publish stage.
COPY --from=publish /app/publish .

# Internal port the service listens on (overridden by ASPNETCORE_URLS in compose).
EXPOSE 8080

# ASPNETCORE_URLS tells Kestrel which address to bind to inside the container.
# The docker-compose.yml environment section sets this to http://+:8080.
ENV ASPNETCORE_URLS=http://+:8080

# Use the exec form to allow signal forwarding (SIGTERM for graceful shutdown).
ENTRYPOINT ["dotnet", "Traverse.Workflow.Api.dll"]
