# ============================================================================
# Traverse.Calendar.Api — Multi-stage Dockerfile
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
#   docker build -f docker/calendar/Dockerfile .
#   docker compose --profile api up --build
# ============================================================================

# ---------------------------------------------------------------------------
# Stage 1: restore
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /src

COPY global.json ./
COPY Traverse.sln ./

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

COPY src/services/calendar/Traverse.Calendar.Api/Traverse.Calendar.Api.csproj \
     src/services/calendar/Traverse.Calendar.Api/

RUN dotnet restore src/services/calendar/Traverse.Calendar.Api/Traverse.Calendar.Api.csproj

# ---------------------------------------------------------------------------
# Stage 2: build
# ---------------------------------------------------------------------------
FROM restore AS build
WORKDIR /src

COPY src/shared/ src/shared/
COPY src/services/calendar/Traverse.Calendar.Api/ src/services/calendar/Traverse.Calendar.Api/

RUN dotnet build src/services/calendar/Traverse.Calendar.Api/Traverse.Calendar.Api.csproj \
    -c Release \
    --no-restore

# ---------------------------------------------------------------------------
# Stage 3: publish
# ---------------------------------------------------------------------------
FROM build AS publish

RUN dotnet publish src/services/calendar/Traverse.Calendar.Api/Traverse.Calendar.Api.csproj \
    -c Release \
    --no-build \
    /p:UseAppHost=false \
    -o /app/publish

# ---------------------------------------------------------------------------
# Stage 4: runtime
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN adduser --disabled-password --gecos "" appuser \
    && chown -R appuser /app
USER appuser

COPY --from=publish /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Traverse.Calendar.Api.dll"]
