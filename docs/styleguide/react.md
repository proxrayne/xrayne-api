# React Styleguide

## Formatting

Use the dashboard Prettier settings:

- print width: 100;
- two spaces;
- semicolons;
- double quotes;
- trailing commas;
- `endOfLine: auto`.

Run:

```powershell
npm.cmd run check-format
```

## Imports

Order imports as follows:

1. external packages;
2. generated route types and relative imports;
3. `@core/*`;
4. `@features/*`;
5. `@libs/*`;
6. side-effect imports such as global CSS.

Use `import type` for type-only imports.

## Naming

- Component files use kebab case: `control-buttons.tsx`.
- React components use `PascalCase`.
- Hooks start with `use`.
- Route modules usually use `index.ts` plus a sibling screen file when needed.
- API files use `api.ts`; API types use `api.types.ts`.
- Feature public exports live in `index.ts`.

## JSDoc

- Codex-added shared methods in `src/core` must include JSDoc.
- This rule applies to utilities and hooks under `src/core/lib`,
  `src/core/hooks`, `src/core/utils`, and similar shared core folders.
- Document what the function/hook returns and any non-obvious browser, SSR,
  subscription, storage, or side-effect behavior.
- Keep JSDoc concise. Do not repeat obvious TypeScript types in prose.
- Feature-local helpers may use JSDoc when the behavior is non-obvious, but
  core utilities and hooks require it.

## Components

- Keep one reusable component per file.
- Route/page components compose feature hooks and UI sections.
- Put route-only sections under `src/routes/<route>/ui`.
- Put reusable domain UI under `src/features/<feature>/ui`.
- Put generic reusable primitives under `src/core/ui`.
- Prefer existing shadcn primitives before creating custom UI.
- Use `lucide-react` icons for common actions.

## Routes

- Add routes in `src/routes.ts`.
- Keep auth behavior in client middleware.
- Use generated route types from `./+types/...`.
- Keep route modules thin: layout, loading/error composition, and feature
  assembly belong there; business data logic belongs in feature hooks.

## Loaders And Actions

- Prefer client middleware and TanStack Query for current data loading patterns.
- Add React Router loaders/actions only when route lifecycle semantics are the
  right fit for the feature.
- Keep loaders/actions thin: call feature APIs/query helpers and return typed
  data or redirects.
- Do not duplicate API client logic inside loaders/actions.
- When adding a loader/action, type it with the generated route types and document
  how errors, redirects, and auth are handled.

## Data And State

- Use TanStack Query for server state.
- Use `@core/lib/query` as the shared query client.
- Feature query hooks should return ergonomic names such as `isLoaded`, `error`,
  `refetch`, and domain data.
- Add static helpers such as `.getOrFetch`, `.setData`, `.invalidate`, or `.fetch`
  only when route middleware or cross-feature flows need them.
- Keep local UI state inside the owning component.

## API Client

- Use `@core/api/instance`.
- Do not create ad hoc Axios instances for panel API calls.
- Put feature HTTP functions in `src/features/<feature>/lib/api.ts`.
- Put request/response types in `api.types.ts`.
- Let `ResponseError` normalize Axios errors.

## Forms

- Use React Hook Form.
- Use `zod` and `zodResolver` when form validation has more than simple required
  fields.
- Keep route-specific form providers under the owning route.
- Surface mutation errors through `sonner` toasts or inline form errors depending
  on whether the error is global or field-specific.

## Loading, Error, Empty States

- Route/page components must represent loading, error, and empty states.
- Use `Placeholder` for error and empty states.
- Provide retry actions when a query can be refetched.
- Avoid leaving raw `loading...` placeholders in final user-facing flows.

## SSR

`react-router.config.ts` currently sets `ssr: false`. Still guard browser-only
APIs in shared hooks/utilities when they may be evaluated outside the browser,
as done by `useIsDark`.
