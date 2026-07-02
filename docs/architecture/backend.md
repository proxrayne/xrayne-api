# Backend Architecture

## Overview

The backend is a .NET 9 solution with a layered architecture:

- `Api`: panel REST API and dashboard static host.
- `Cli`: administrator command-line tool.
- `Contracts`: shared contracts and configuration values.
- `Github`: reusable GitHub.com releases/assets client.
- `System`: reusable host system information client.
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
- migrates the database on startup;
- serves static dashboard files from `wwwroot` and falls back to `index.html`
  outside `/api`.

Controllers use `[Route("api/...")]`, inherit from `ApiControllerBase`, and use
`EndpointSummary`, `EndpointDescription`, and `ProducesResponseType`.

## Managed Nodes

Node management is part of the panel backend, not a separate local project.
Panel-owned node behavior includes:

- HTTP endpoints in `Api`;
- provisioning, reconnect, status streaming, and connection verification services
  in `Infrastructure`;
- node entities, encrypted connection data, and repository access in
  `Repositories`;
- shared node DTOs, enums, and configuration values in `Contracts`.

Do not add or reference a local standalone node-service project when
implementing node management features in this repository.

## Contracts Layer

`Contracts` owns shared types used across backend projects:

- configuration options such as `JwtOptions`, `PanelSettings`, and `XrayOptions`;
- enums such as admin permissions, user status, node status, and update target;
- query/filter models such as cursor pagination and entity filters;
- shared values such as `PathProvider` and `AdminPermissionNames`;
- contract-level DI registration.

Avoid dependencies from contracts back into API, repositories, infrastructure, CLI,
or dashboard.

## Infrastructure Layer

`Infrastructure` owns runtime behavior:

- `CoreService`, `CoreStateMachine`, `BackgroundTaskScheduler`;
- Quartz jobs for installing and operating xray-core;
- JWT token creation, settings application, restart scheduling;
- certificate, geo resource, routing rule, and node services;
- registration of `SystemInfo.ISystemInfoService` with panel runtime paths.

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

## CLI

`Cli` uses `System.CommandLine` with feature commands under
`Commands/<feature>`. Commands create async scopes, resolve services, write through
`ICliConsole`, and return integer exit codes.

CLI install/update flows own Docker Compose generation, release asset download,
runtime migrations, certificate installation, and shell command orchestration.
