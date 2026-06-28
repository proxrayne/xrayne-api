# Feature Conventions

## Adding Backend Features

1. Define shared options/enums/filter models in `XRayne.Contracts` only when they
   are reused across projects.
2. Add or update EF entities and repositories in `XRayne.Repositories`.
3. Add migrations through `add-migration.ps1`.
4. Add runtime/business services in `XRayne.Infrastructure`.
5. Add controller endpoints and request/response models in `XRayne.Api`.
6. Add AutoMapper profiles only when mapping is non-trivial or reused.
7. Add focused tests for repository/service/API behavior.
8. Update `/docs` and frontend API types when the API changes.

## Adding Frontend Features

1. Create `src/features/<feature>` with `index.ts`, `lib`, and optional `ui`.
2. Add `lib/api.ts` and `lib/api.types.ts` for HTTP work.
3. Add `lib/query.ts` for TanStack Query hooks.
4. Add route modules under `src/routes/<route>` and wire them in `src/routes.ts`.
5. Compose screens from shared `@core/ui` primitives and feature UI.
6. Handle loading, error, empty, and success states.
7. Run typecheck, format check, and build when feasible.

## When To Create What

- Create a controller when exposing an HTTP surface.
- Create a service when behavior coordinates multiple dependencies, runtime state,
  process lifecycle, file mutation, or business rules.
- Create a repository when accessing persistent EF data.
- Create a DTO/request/response model for public API shapes.
- Create middleware/filter for cross-cutting HTTP behavior only.
- Create a React hook for reusable stateful UI or data logic.
- Create a component for reusable UI structure or route sections.
- Create a utility for pure, stable, cross-feature logic.
- Create a domain library under `src/libs` only when it is app-independent and
  reused across features.

## Validation And Edge Cases

- Validate inputs at the boundary closest to the user/API.
- Make mutation failures visible to users with field errors, inline errors, or
  toasts depending on context.
- For destructive operations, confirm in UI and use clear status codes in API.
- Keep async operations observable with job/status endpoints or stream updates
  when they are long running.

## Documentation Updates

Feature PRs must update docs when they:

- add or change API endpoints;
- introduce a new project/layer/folder convention;
- add a new runtime configuration key;
- add or change release artifact behavior;
- create a new route pattern or shared frontend primitive.
