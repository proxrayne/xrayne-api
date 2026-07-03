# Project Rules

## Product Boundaries

XRayne Panel is an admin panel for managing `xray-core` and remote nodes. The
`xrayne-panel` repository contains the panel API, dashboard, and backend
services. The standalone CLI source lives in `xrayne-cli`.

- `Api`: ASP.NET Core REST API and static web host.
- `Dashboard`: React Router dashboard built into the API image.

Managed nodes are a panel domain feature handled through API controllers,
infrastructure services, repositories, and dashboard routes. This repository no
longer contains a standalone node-service project or node-service source code.

## Repository Layout

- `Contracts`: shared options, enums, DTO-like models, values, and utilities.
- `Infrastructure`: xray-core runtime services, jobs, state machines, and
  cross-cutting infrastructure services.
- `Repositories`: EF Core DbContext, entities, migrations, repository
  interfaces/implementations, config-file utilities, and external release clients.
- `Api`: HTTP controllers, auth, API filters, AutoMapper profiles,
  request/response models, static dashboard hosting.
- `Dashboard`: React Router application with `src/core`, `src/features`,
  `src/routes`, and `src/libs`.
- `.codex/skills`: Codex skill entrypoints that point to this documentation.
- `.github/workflows/build.yml`: API + UI image publishing workflow.

## Dependency Direction

- Keep shared contracts free of API/UI/EF dependencies.
- Keep HTTP behavior in `Api`; delegate work to repositories/services.
- Keep EF persistence in `Repositories`.
- Keep xray-core lifecycle and runtime behavior in `Infrastructure`.
- Keep managed-node provisioning, reconnect, and status orchestration in
  panel-owned services; do not plan work in a local standalone node-service
  project.
- Keep CLI orchestration in the `xrayne-cli` repository; do not move CLI install
  flows into the panel API.

## Packaging

- Release workflow builds API + UI image as a `tar.gz` release asset.
- Docker Compose, when present, is installer/runtime orchestration and should run
  prebuilt artifacts instead of local `build:`.

Public release/install assets are intentionally resolved from
`VanyaKrotov/xrayne`; source-level project documentation should refer to this
repository as `xrayne-panel`.

## Configuration

- Runtime paths are centralized through `PathProvider` in `Contracts.Values`.
- API reads normal `IConfiguration`; use `JsonConfig`/`EnvConfig` only when
  mutating runtime config files shared with the installed runtime.
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
