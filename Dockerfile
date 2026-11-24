# ── Stage 1: Build & Publish ───────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy directory configuration and solution
COPY Directory.Build.props ./
COPY TicketFlow.slnx ./

# Copy csproj and lockfiles for cached restore (locked-mode — T14)
COPY src/TicketFlow.Domain/TicketFlow.Domain.csproj src/TicketFlow.Domain/
COPY src/TicketFlow.Domain/packages.lock.json src/TicketFlow.Domain/
COPY src/TicketFlow.Application/TicketFlow.Application.csproj src/TicketFlow.Application/
COPY src/TicketFlow.Application/packages.lock.json src/TicketFlow.Application/
COPY src/TicketFlow.Infrastructure/TicketFlow.Infrastructure.csproj src/TicketFlow.Infrastructure/
COPY src/TicketFlow.Infrastructure/packages.lock.json src/TicketFlow.Infrastructure/
COPY src/TicketFlow.Api/TicketFlow.Api.csproj src/TicketFlow.Api/
COPY src/TicketFlow.Api/packages.lock.json src/TicketFlow.Api/

# Restore dependencies in locked mode
RUN dotnet restore src/TicketFlow.Api/TicketFlow.Api.csproj --locked-mode

# Copy source tree
COPY src/ ./src/

# Build & Publish Release binary
RUN dotnet publish src/TicketFlow.Api/TicketFlow.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Stage 2: Hardened Runtime (Chiseled Distroless — T15) ──────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS final

# Hardened environment configurations
ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_EnableDiagnostics=0 \
    DOTNET_CLI_TELEMETRY_OPTOUT=1

WORKDIR /app

# Run as non-root user (UID 1654: app user in Ubuntu Chiseled)
USER 1654:1654

# Copy published application from build stage
COPY --from=build --chown=1654:1654 /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "TicketFlow.Api.dll"]
