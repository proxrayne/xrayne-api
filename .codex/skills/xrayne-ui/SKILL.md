---
name: xrayne-ui
description: Frontend development guidance for XRayne.Node. Use when Codex works on XRayne.UI React Router routes, layouts, auth middleware, TanStack Query data hooks, Axios API clients, HeroUI components, Tailwind styles, Vite config, TypeScript types, forms, navigation, or frontend build/typecheck/lint issues.
---

# XRayne UI

## Quick Start

Read `references/ui-map.md` before UI edits. Keep changes consistent with the existing feature-sliced structure and React Router conventions.

## Implementation Rules

- Place shared primitives in `src/core`; place domain features under `src/features/<feature>`.
- Use aliases from `tsconfig.json`: `@core/*` and `@features/*`.
- Add routes through `src/routes.ts`; route modules live under feature route folders.
- Keep auth redirects in client middleware, not ad hoc component effects.
- Use the shared Axios instance from `@core/api/instance`.
- Use TanStack Query helpers near each feature's `lib/query.ts`.
- Use HeroUI components already present in the project for forms, cards, buttons, sidebar, and feedback.
- Keep UI text in English unless the existing screen is already localized.
- Do not add Docker image build or API container assumptions for frontend delivery.

## Validation

Run from `XRayne.UI`:

```powershell
npm run typecheck
npm run lint
npm run build
```

For local dev:

```powershell
npm run dev
```

The Vite dev server proxies `/api` to `http://localhost:5097`.

