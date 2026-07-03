---
name: xrayne-dashboard
description: Frontend development guidance for  Use when Codex works on Dashboard React Router routes, layouts, auth middleware, TanStack Query data hooks, Axios API clients, shadcn components, Tailwind styles, Vite config, TypeScript types, forms, navigation, or frontend build/typecheck/lint issues.
---

# XRayne UI

## Quick Start

Read `references/ui-map.md`, `docs/architecture/frontend.md`, and `docs/styleguide/react.md` before UI edits. `Dashboard/STYLEGUIDE.md` is legacy supporting context; `/docs` is canonical.

## Implementation Rules

- Place shared primitives in `src/core`; place domain features under `src/features/<feature>`.
- Use aliases from `tsconfig.json`: `@core/*` and `@features/*`.
- Add routes through `src/routes.ts`; route modules live under feature route folders.
- Keep auth redirects in client middleware, not ad hoc component effects.
- Use the shared Axios instance from `@core/api/instance`.
- Use TanStack Query helpers near each feature's `lib/query.ts`.
- Add concise JSDoc to Codex-added shared utilities and hooks in `src/core`.
- Use shadcn components from `@core/ui` as the primary UI source.
- Add missing shadcn components with `npx shadcn@latest add <component name>`.
- Create custom UI primitives only when shadcn does not cover the need.
- Follow the import order from `STYLEGUIDE.md`: external packages, generated/relative imports, `@core`, `@features`, `@libs`, then side-effect imports.
- Keep UI text in English unless the existing screen is already localized.
- The release Docker image builds this UI and embeds `build/client` into the API `wwwroot`; keep production builds compatible with same-origin `/api` delivery.
- Follow `STYLEGUIDE.md`: one file per component, feature-first nesting, library-first implementations, and Prettier formatting.
- Update `docs/` when route structure, feature conventions, shared UI patterns, or frontend architecture changes.

## Validation

Run from `Dashboard`:

```powershell
npm run typecheck
npm run lint
npm run check-format
npm run build
```

For local dev:

```powershell
npm run dev
```

The Vite dev server proxies `/api` to `http://localhost:5097`.
