# Feature Conventions

## Adding Backend Features

1. Define shared options/enums/filter models in `Contracts` only when they
   are reused across projects.
2. Add or update EF entities and repositories in `Data`.
3. Add migrations through `add-migration.ps1`.
4. Add runtime/business services in `Infrastructure`.
5. Add controller endpoints and request/response models in `Api`.
6. Add AutoMapper profiles only when mapping is non-trivial or reused.
7. Add tests only for deterministic utilities, validators, mappers,
   serializers, configuration parsers, and stable wire contracts.
8. Update `/docs` and coordinate `xrayne-ui` frontend API types when the API
   changes.

## When To Create What

- Create a controller when exposing an HTTP surface.
- Create a service when behavior coordinates multiple dependencies, runtime state,
  process lifecycle, file mutation, or business rules.
- Create a repository when accessing persistent EF data.
- Create a DTO/request/response model for public API shapes.
- Create middleware/filter for cross-cutting HTTP behavior only.
- Create a utility for pure, stable, cross-feature logic.

## Validation And Edge Cases

- Validate inputs at the boundary closest to the user/API.
- For destructive operations, confirm in UI and use clear status codes in API.
- Keep async operations observable with job/status endpoints or stream updates
  when they are long running.

## Documentation Updates

Feature PRs must update docs when they:

- add or change API endpoints;
- introduce a new project/layer/folder convention;
- add a new runtime configuration key;
- add or change release artifact behavior;
- change frontend API contracts that must be mirrored in `xrayne-ui`.
