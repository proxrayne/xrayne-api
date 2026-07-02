# Frontend Architecture

## Overview

`Dashboard` is a React 19 + React Router 7 framework-mode application.
`react-router.config.ts` sets `ssr: false`, so route code may rely on browser-only
behavior when guarded appropriately.

The dashboard is built by `Api/Dockerfile` and copied into API `wwwroot`.
Production API calls should therefore work with same-origin `/api`.

## Top-Level Structure

- `src/root.tsx`: document shell, metadata, query provider, tooltip provider,
  toaster, global auth invalidation handling, route error boundary.
- `src/routes.ts`: route tree declared with React Router dev helpers.
- `src/routes`: route modules and route-owned UI.
- `src/features`: domain features with API, query, type, and UI modules.
- `src/core`: shared API client, hooks, utilities, styles, types, and UI primitives.
- `src/libs`: reusable xray config/schema libraries independent of app features.

## Routing

Routes are wired in `src/routes.ts`:

- authorized layout: `routes/auth/authorized-layout.tsx`;
- index dashboard route: `routes/dashboard/index.ts`;
- settings route: `routes/settings/index.ts`;
- inbounds route: `routes/inbounds/index.ts`;
- unauthorized sign-in layout: `routes/auth/unauthorized-layout.tsx`;
- catch-all not found route: `features/service/routes/not-found.tsx`.

Authorized routes use client middleware from `features/auth/lib/middlewares.ts`.
Route modules should remain thin and compose feature hooks/UI.

Current code favors client middleware plus TanStack Query over route loaders and
actions. Introduce loaders/actions only for route lifecycle behavior that cannot
be expressed cleanly with existing feature query hooks.

## Data Flow

- HTTP calls go through `@core/api/instance`.
- `getApiDomain()` uses `VITE_API_DOMAIN` or same-origin `/api`.
- Authorization is stored in the `auth_token` cookie.
- On `401`, the API client clears the token and dispatches `unauthorize`.
- `root.tsx` listens for `unauthorize`, invalidates admin query data, and
  revalidates routes.
- TanStack Query is configured in `@core/lib/query` with retry and stale-time
  defaults.

## Feature Pattern

A feature typically contains:

- `index.ts` public exports;
- `lib/api.ts` HTTP functions;
- `lib/api.types.ts` or `types.d.ts` API/domain types;
- `lib/query.ts` TanStack Query hooks and helpers;
- `ui/*` feature components.

Route-owned forms and sections may live under `src/routes/<route>/ui` when they
are not reusable outside that route.

## UI Layer

Shared UI primitives live under `src/core/ui` and are primarily shadcn/Radix/Base
UI based. Styling uses Tailwind CSS 4 and `src/core/styles/app.css`.

Use existing primitives such as `Button`, `Card`, `Field`, `Input`,
`Placeholder`, `Page`, `Sidebar`, `Dialog`, `Select`, `Tabs`, and `Toaster`
before creating new primitives.
