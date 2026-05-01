# docker/compliance/Dockerfile -- Traverse Compliance Framework API
#
# Multi-stage build following .NET Coding Standards section 18.
# Stage 1 (build): SDK image restores and publishes the project.
# Stage 2 (runtime): ASP.NET runtime image runs as non-root appuser.
#
# NOTE: This Dockerfile requires the Traverse.Compliance.Api project produced by NT-005.
#       Building this image before NT-005 is merged will fail with "project not found".
#       docker compose up (without --profile api) succeeds without building this image.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Traverse.sln", "global.json", "Directory.Build.props", "./"]
COPY ["src/", "src/"]

RUN dotnet restore "src/services/compliance/Traverse.Compliance.Api/Traverse.Compliance.Api.csproj"

RUN dotnet publish "src/services/compliance/Traverse.Compliance.Api/Traverse.Compliance.Api.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN useradd -m appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

HEALTHCHECK --interval=30s --timeout=10s \
    CMD wget -qO- http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "Traverse.Compliance.Api.dll"]
