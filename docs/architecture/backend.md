# Backend Architecture

## Overview

The backend is a .NET 9 solution with a layered architecture:

- `Api`: panel REST API.
- `Contracts`: shared contracts and configuration values.
- `RemoteNode`: typed HTTP/SSE protocol client for standalone remote nodes.
- `Infrastructure`: runtime services, xray-core lifecycle, jobs, and state.
- `Repositories`: EF Core persistence.
- `Test`: test project.

## API Host

`Api/Program.cs` is the composition root:

- builds Serilog bootstrap and runtime loggers;
- loads runtime `config.json` and `.env` through project utilities;
- configures Kestrel from panel settings outside Development;
- registers controllers with `ApiExceptionFilter`;
- configures OpenAPI/Scalar when `Docs` is enabled;
- configures JWT bearer auth and admin permission policies;
- registers infrastructure, repositories, contracts, Quartz, and restart services;
- migrates the database on startup.

Controllers use `[Route("api/...")]`, inherit from `ApiControllerBase`, and use
`EndpointSummary`, `EndpointDescription`, and `ProducesResponseType`.

## Managed Nodes

Node management is part of the panel backend, not a separate local project.
Panel-owned node behavior includes:

- HTTP endpoints in `Api`;
- remote node HTTP/SSE protocol clients, DTOs, and typed protocol errors in
  `RemoteNode`;
- provisioning, reconnect, status streaming, and connection verification services
  in `Infrastructure`;
- node entities, encrypted connection data, and repository access in
  `Repositories`;
- shared node DTOs, enums, and configuration values in `Contracts`.

`RemoteNode` must not depend on `Contracts`, `Infrastructure`, `Repositories`,
or `Api`.
It is the only panel project that should know the remote node API wire shape.
Do not add or reference a local standalone node-service project when
implementing node management features in this repository.

`Infrastructure` owns the live connection lifecycle through
`IRemoteNodeConnectionManager`. The manager starts saved active node streams at
panel startup, exposes immediate connect/reconnect/disconnect operations for
controllers and services, keeps one worker per node, and persists heartbeat data
on a throttled interval instead of writing every SSE heartbeat to the database.

The standalone remote node `/api/ping` response and `/api/connect` SSE payloads
must include `Service`, `NodeVersion`, `Environment`, `StartedAt`, `Timestamp`,
`Uptime`, `Core`, and `System`.

## Contracts Layer

`Contracts` owns shared types used across backend projects:

- configuration options such as `JwtOptions`, `PanelSettings`, and `XrayOptions`;
- enums such as admin permissions, user status, node status, and update target;
- query/filter models such as cursor pagination and entity filters;
- shared values such as `PathProvider` and `AdminPermissionNames`;
- contract-level DI registration.

Avoid dependencies from contracts back into API, repositories, or
infrastructure.

## Infrastructure Layer

`Infrastructure` owns runtime behavior:

- `CoreService`, `CoreStateMachine`, `BackgroundTaskScheduler`;
- Quartz jobs for installing and operating xray-core;
- Octokit-backed GitHub release lookup and release asset download helpers;
- JWT token creation, settings application, restart scheduling;
- certificate, geo resource, routing rule, and node services;
- `Hardware.Info`-backed host system information through `ISystemInfoService`.

Service interfaces live near implementations under `Services/Contracts`. Register
new services in `Infrastructure/DependencyInjection.cs`.

## Repository Layer

`Repositories` owns persistence:

- `AppDbContext` and migrations;
- EF entities under `Entities`;
- repository interfaces under `Contracts`;
- repository implementations under `Implementations`;
- runtime config-file utilities under `Utilities`.

Repositories expose async APIs, accept `CancellationToken`, and use admin-scoped
overloads when data belongs to an administrator.

## CLI Split

The administrator CLI is now owned by the standalone `xrayne-cli` repository.
Keep install/update flows, runtime migrations, Docker Compose generation,
certificate installation, and shell command orchestration there instead of adding
new CLI code to `xrayne-panel`.

## UI Split

The web UI is now owned by the standalone `xrayne-ui` repository. Keep React
routes, frontend API clients, UI Docker image builds, and frontend release
artifacts there. The panel API no longer builds or serves static frontend files.
