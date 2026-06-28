# Backend Architecture

## Overview

The backend is a .NET 9 solution with a layered architecture:

- `XRayne.Api`: panel REST API and dashboard static host.
- `XRayne.Node`: standalone remote-node REST API skeleton.
- `XRayne.Cli`: administrator command-line tool.
- `XRayne.Contracts`: shared contracts and configuration values.
- `XRayne.Infrastructure`: runtime services, xray-core lifecycle, jobs, and state.
- `XRayne.Repositories`: EF Core persistence and external clients.
- `XRayne.Test`: test project.

## API Host

`XRayne.Api/Program.cs` is the composition root:

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

## Node Host

`XRayne.Node` is an independent ASP.NET Core REST API service:

- no `ProjectReference` to panel projects;
- API key middleware using `X-Node-Api-Key`;
- Serilog console and daily file logging;
- OpenAPI/Scalar docs when `Docs` is enabled;
- controller stubs for core, system, and version APIs.

Keep it standalone until a dedicated shared contract package is introduced.

## Contracts Layer

`XRayne.Contracts` owns shared types used across backend projects:

- configuration options such as `JwtOptions`, `PanelSettings`, and `XrayOptions`;
- enums such as admin permissions, user status, node status, and update target;
- query/filter models such as cursor pagination and entity filters;
- shared values such as `PathProvider` and `AdminPermissionNames`;
- contract-level DI registration.

Avoid dependencies from contracts back into API, repositories, infrastructure, CLI,
or dashboard.

## Infrastructure Layer

`XRayne.Infrastructure` owns runtime behavior:

- `CoreService`, `CoreStateMachine`, `BackgroundTaskScheduler`;
- Quartz jobs for installing and operating xray-core;
- JWT token creation, settings application, restart scheduling;
- certificate, geo resource, routing rule, node, and system info services;
- OS-specific system info implementations.

Service interfaces live near implementations under `Services/Contracts`. Register
new services in `XRayne.Infrastructure/DependencyInjection.cs`.

## Repository Layer

`XRayne.Repositories` owns persistence:

- `AppDbContext` and migrations;
- EF entities under `Entities`;
- repository interfaces under `Contracts`;
- repository implementations under `Implementations`;
- external clients under `External`;
- runtime config-file utilities under `Utilities`.

Repositories expose async APIs, accept `CancellationToken`, and use admin-scoped
overloads when data belongs to an administrator.

## CLI

`XRayne.Cli` uses `System.CommandLine` with feature commands under
`Commands/<feature>`. Commands create async scopes, resolve services, write through
`ICliConsole`, and return integer exit codes.

CLI install/update flows own Docker Compose generation, release asset download,
runtime migrations, certificate installation, and shell command orchestration.
