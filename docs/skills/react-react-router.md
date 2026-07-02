# Skill: React + React Router

Use this skill when working on `Dashboard`, including routes, layouts,
auth middleware, TanStack Query hooks, Axios API clients, shadcn components,
Tailwind styles, forms, or frontend build/typecheck/lint issues.

## Start Here

1. Read [Frontend architecture](../architecture/frontend.md).
2. Read [React styleguide](../styleguide/react.md).
3. Read [Feature conventions](../conventions/features.md).

## Placement Rules

- Put shared cross-feature primitives in `src/core`.
- Put domain features in `src/features/<feature>`.
- Put route modules in `src/routes`; wire them through `src/routes.ts`.
- Put reusable xray config/schema domain libraries in `src/libs`.
- Use aliases from `tsconfig.json`: `@core/*`, `@features/*`, and `@libs/*`.

## Required Frontend Rules

- Use the shared Axios instance from `@core/api/instance`.
- Put API functions in feature `lib/api.ts` and API types in `lib/api.types.ts`.
- Put TanStack Query keys/options/hooks in feature `lib/query.ts`.
- Add JSDoc to Codex-added shared utilities and hooks in `src/core`.
- Keep auth redirects in React Router client middleware.
- Prefer shadcn components from `@core/ui`; add missing shadcn primitives through
  the project shadcn workflow.
- Use React Hook Form and `zod` for forms that need validation.
- Handle loading, error, and empty states in route/page components.
- Keep route modules thin; compose feature hooks and feature UI.
- Respect `react-router.config.ts`: SSR is currently disabled.

## Validation

Run from `Dashboard`:

```powershell
npm.cmd run typecheck
npm.cmd run lint
npm.cmd run check-format
npm.cmd run build
```

Use `npm.cmd` on Windows when PowerShell blocks `npm.ps1`.
