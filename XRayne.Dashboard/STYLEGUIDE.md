# XRayne.Dashboard Styleguide

This document is the source of truth for frontend code in `XRayne.Dashboard`.
Keep new code consistent with the existing feature-sliced structure and React Router conventions.

## Code Structure

- Keep React components focused and small.
- Use one file per component. Do not define multiple reusable components in the same file.
- Name component files in kebab case, matching the existing project style, for example `core-update-modal.tsx`.
- Export public feature/component entry points through local `index.ts` barrels where that pattern already exists.
- Keep shared primitives and cross-feature utilities under `src/core`.
- Keep domain-specific code under `src/features/<feature>`.
- Do not move feature-specific logic into `src/core`.

## Feature Layout

Use strict feature nesting:

- `src/features/<feature>/index.ts` for the public feature API.
- `src/features/<feature>/lib` for API functions, query helpers, types, constants, and feature utilities.
- `src/features/<feature>/ui` for feature components.
- `src/features/<feature>/ui/<widget>` for composed widgets with their own `index.ts`, main component file, and nested `ui` or `lib` folders when needed.
- `src/routes` for route modules wired through `src/routes.ts`.

When a feature grows, add folders that describe the component or behavior instead of collecting unrelated files in a flat directory.

## Libraries

Prefer existing libraries and shared wrappers over handwritten implementations:

- Use `date-fns` for date parsing, formatting, durations, and date arithmetic.
- Use TanStack Query for server-state queries, mutations, cache reads, cache writes, and invalidation.
- Use the shared Axios instance from `@core/api/instance` for HTTP calls.
- Keep feature query helpers near the feature in `src/features/<feature>/lib/query.ts`.
- Use React Hook Form for every form, including small modal forms and settings cards.
- Use `zod` and `zodResolver` for every form validation schema. Required fields, optional URL fields, numeric bounds, and array/string parsing rules must live in the schema instead of ad hoc submit handlers.
- Add placeholders to input fields. When the expected format is known, use a concrete format/example placeholder; otherwise use `Enter the ...`.
- Use existing project utilities before creating new utility functions.

## Imports

Follow the import order in every TypeScript and TSX file:

1. External packages, including React, React Router, TanStack Query, Axios, and date-fns.
2. Generated route types and relative imports such as `./+types/...` or nearby local files.
3. `@core/*` imports.
4. `@features/*` imports.
5. `@libs/*` imports.
6. Side-effect imports such as global CSS.

Keep a blank line between import groups. Sort imports alphabetically inside each group when it does not make multi-line component imports harder to read. Use `import type` for type-only imports when possible.

## UI Components

- Use shadcn as the primary UI component source.
- Import shared UI components through the aliases configured in `components.json`, usually from `@core/ui/...`.
- Add missing shadcn components with:

```powershell
npx shadcn@latest add <component name>
```

- Do not hand-copy shadcn component source from docs.
- Create custom UI primitives only when shadcn does not cover the need.
- Create feature-specific UI components inside the owning feature when they encode domain behavior.
- Use `lucide-react` icons for actions and navigation when an icon exists.

## API And Data Flow

- Keep route modules thin; route components should compose feature hooks and feature UI.
- Put HTTP request functions in feature `lib/api.ts` files.
- Put API DTOs and request/response shapes in feature `lib/api.types.ts` files.
- Put TanStack Query keys, options, hooks, and mutation helpers in feature `lib/query.ts` files.
- Keep auth redirects in client middleware instead of component effects.

## Formatting

- Format code with Prettier.
- Keep code readable through clear naming, early returns, and small functions.
- Avoid clever inline logic when a named helper makes intent clearer.
- Prefer explicit types at module boundaries and let TypeScript infer obvious local values.
- Run `npm run check-format`, `npm run typecheck`, and `npm run build` before handing off frontend changes.
