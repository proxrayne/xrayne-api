# Project Rules

## Product Boundaries

XRayne is an admin panel for managing `xray-core`. The panel surface is:

- `XRayne.Api`: ASP.NET Core REST API and static web host.
- `XRayne.Dashboard`: React Router dashboard built into the API image.
- `XRayne.Cli`: administrator CLI for installation and runtime operations.
- `XRayne.Node`: standalone infrastructure REST API service for remote node hosts.

`XRayne.Node` is intentionally independent and must not reference panel projects
directly unless a future architecture decision changes that boundary.

## Repository Layout

- `XRayne.Contracts`: shared options, enums, DTO-like models, values, and utilities.
- `XRayne.Infrastructure`: xray-core runtime services, jobs, state machines, and
  cross-cutting infrastructure services.
- `XRayne.Repositories`: EF Core DbContext, entities, migrations, repository
  interfaces/implementations, config-file utilities, and external release clients.
- `XRayne.Api`: HTTP controllers, auth, API filters, AutoMapper profiles,
  request/response models, static dashboard hosting.
- `XRayne.Cli`: System.CommandLine commands, installation services, runtime
  migrations, Docker Compose generation, shell integration.
- `XRayne.Dashboard`: React Router application with `src/core`, `src/features`,
  `src/routes`, and `src/libs`.
- `.codex/skills`: Codex skill entrypoints that point to this documentation.
- `.github/workflows/build.yml`: release artifact and image publishing workflow.

## Dependency Direction

- Keep shared contracts free of API/UI/EF dependencies.
- Keep HTTP behavior in `XRayne.Api`; delegate work to repositories/services.
- Keep EF persistence in `XRayne.Repositories`.
- Keep xray-core lifecycle and runtime behavior in `XRayne.Infrastructure`.
- Keep CLI orchestration in `XRayne.Cli`; do not move CLI install flows into API.
- Keep `XRayne.Node` standalone until an explicit contract package is introduced.

## Packaging

- Release workflow publishes CLI archives for Windows, macOS, and Linux.
- Release workflow builds API + UI image as a `tar.gz` release asset.
- Release workflow pushes `XRayne.Node` to `ghcr.io/vanyakrotov/xrayne-node`.
- Docker Compose, when present, is installer/runtime orchestration and should run
  prebuilt artifacts instead of local `build:`.

## Configuration

- Runtime paths are centralized through `PathProvider` in `XRayne.Contracts.Values`.
- API and CLI read normal `IConfiguration`; use `JsonConfig`/`EnvConfig` only when
  mutating runtime config files.
- `.env` is reserved for bootstrap/container values. Complex app configuration
  belongs in runtime `config.json`.

## Documentation Maintenance

Update `/docs` whenever:

- an endpoint is added, removed, renamed, or changes response/request shape;
- a route, feature folder, or shared UI convention changes;
- a project boundary or dependency direction changes;
- release artifacts, Docker behavior, or configuration keys change;
- a new public service, DTO, controller, endpoint, handler, or repository pattern
  is introduced.
