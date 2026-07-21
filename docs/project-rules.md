# Project Rules

## Product Boundaries

XRayne Panel is an admin panel for managing remote nodes and their remote
`xray-core` runtimes. The `xrayne-panel` repository contains the panel API and
backend services. The standalone CLI source lives in `xrayne-cli`, and the
standalone frontend source lives in `xrayne-ui`.

- `Api`: ASP.NET Core REST API.

Managed nodes are a panel domain feature handled through API controllers,
infrastructure services, and repositories. This repository no
longer contains a standalone node-service project or node-service source code.

## Repository Layout

- `Contracts`: shared options, enums, DTO-like models, values, and utilities.
- `Infrastructure`: managed-node orchestration, background jobs, runtime state
  stores, and cross-cutting infrastructure services.
- `Data`: EF Core DbContext, entities, migrations, repository
  interfaces/implementations, config-file utilities, and external release clients.
- `Api`: HTTP controllers, auth, API filters, AutoMapper profiles, and
  request/response models.
- `.codex/skills`: Codex skill entrypoints that point to this documentation.
- `.github/workflows/build.yml`: API image publishing workflow.

## Dependency Direction

- Keep shared contracts free of API/EF dependencies.
- Keep HTTP behavior in `Api`; delegate work to repositories/services.
- Keep EF persistence in `Data`.
- Keep remote-node xray-core lifecycle and runtime behavior in `Infrastructure`.
- Keep managed-node provisioning, reconnect, and status orchestration in
  panel-owned services; do not plan work in a local standalone node-service
  project.
- Keep CLI orchestration in the `xrayne-cli` repository; do not move CLI install
  flows into the panel API.
- Keep frontend source, UI Docker image builds, and React conventions in the
  `xrayne-ui` repository.

## Packaging

- Release workflow builds the API-only image as a `tar.gz` release asset.
- Docker Compose, when present, is installer/runtime orchestration and should run
  prebuilt artifacts instead of local `build:`.

Public release/install assets are resolved from their owning public repositories:
CLI from `proxrayne/xrayne-cli`, API from `proxrayne/xrayne-api`, UI from
`proxrayne/xrayne-ui`, and remote-node images from `proxrayne/xrayne-node`.
This local repository remains named `xrayne-panel`.

## Configuration

- Runtime paths are centralized through `PathProvider` in `Contracts.Values`.
- API reads normal `IConfiguration`; use `JsonConfig`/`EnvConfig` only when
  mutating runtime config files shared with the installed runtime.
- `.env` is reserved for bootstrap/container values. Complex app configuration
  belongs in runtime `config.json`.

## Documentation Maintenance

Update `/docs` whenever:

- an endpoint is added, removed, renamed, or changes response/request shape;
- a project boundary or dependency direction changes;
- release artifacts, Docker behavior, or configuration keys change;
- a new public service, DTO, controller, endpoint, handler, or repository pattern
  is introduced.

## Test Policy

- Keep tests focused on deterministic helpers, validators, mappers,
  serializers, configuration parsers, and stable wire contracts.
- Do not add controller delegation, mock service call, repository CRUD, DI
  registration, service activation, or pass-through wrapper tests.
