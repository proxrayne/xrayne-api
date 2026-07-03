# XRayne UI Map

## Stack

Canonical frontend documentation lives in `docs/architecture/frontend.md` and `docs/styleguide/react.md`.

- React 19.
- React Router 7 framework mode via `@react-router/dev`.
- Vite 7 with plugins: `mkcert`, `reactRouter`, Tailwind CSS 4, `vite-tsconfig-paths`, `svgr`.
- TypeScript strict mode.
- TanStack Query 5.
- Axios.
- shadcn configured through `components.json`.
- Radix/Base UI primitives through generated shadcn components where needed.
- React Hook Form.
- next-themes.
- Prettier.

## Scripts

From `Dashboard`:

```powershell
npm run dev
npm run build
npm run start
npm run typecheck
npm run lint
npm run check-format
npm run format
```

`typecheck` runs `react-router typegen && tsc`.
`check-format` verifies Prettier formatting without writing files.
`format` applies Prettier formatting.

## Routing

Routes are declared in `src/routes.ts`:

- Authorized layout: `features/auth/routes/authorized-layout.tsx`
  - index route: `features/home/routes/main/index.ts`
- Unauthorized layout: `features/auth/routes/unauthorized-layout.tsx`
  - `sign-in`: `features/auth/routes/sign-in/index.ts`
- Catch-all: `features/service/routes/not-found.tsx`

Route type imports come from generated `+types` files.

## Root App

`src/root.tsx`:

- Defines document shell and metadata.
- Wraps children in `QueryClientProvider` and shared providers.
- Shows `PageProgress`.
- Listens for `unauthorize` events, invalidates `admin-account`, and revalidates routes.
- Uses `CommonTemplate` and `ErrorScreen` in `ErrorBoundary`.

## API Client

`src/core/api/instance.ts`:

- Base URL is `${getApiDomain()}/api`.
- `getApiDomain()` uses `VITE_API_DOMAIN` when present, otherwise returns an empty same-origin prefix in the browser/server. If the first browser path segment is not a known app route, it is treated as a deployed `PathBase` prefix.
- Adds `Content-Type` and `X-Platform: Web`.
- Adds `Authorization: Bearer <token>` from cookie key `auth_token`.
- On 401, clears auth token and dispatches `unauthorize`.
- Converts Axios failures to `ResponseError`.

When adding feature API functions, import `api` from `@core/api/instance`.

## Query Pattern

`src/core/lib/query.tsx` creates the shared query client.

Feature query example: `src/features/admin/lib/query.ts`.

- Export `queryOptions`.
- Wrap with a feature hook such as `useAdminAccount`.
- Add static helpers only when useful, currently `getOrFetch` and `setData`.

## Auth Flow

`features/auth/lib/middlewares.ts`:

- `requireAuthMiddleware`: calls `useAdminAccount.getOrFetch()`, redirects to sign-in with `return_url` when missing.
- `requireNoAuthMiddleware`: redirects authenticated users to root.

`sign-in`:

- Uses React Hook Form.
- Calls `login`.
- Sets `useAdminAccount` query data.
- Navigates to `return_url` or root.

## Layout And Components

- Authorized layout uses `SidebarProvider`, `Sidebar`, and a content area.
- Core UI primitives live under `src/core/ui`.
- Auth sidebar components live under `src/features/auth/ui/sidebar`.
- Use shadcn components from `@core/ui` as the primary UI source.
- Add missing shadcn components with `npx shadcn@latest add <component name>`.
- Create custom UI primitives only when shadcn does not cover the need.
- Follow `Dashboard/STYLEGUIDE.md` for one-file-one-component, feature nesting, library usage, and Prettier rules.

## Environment

Important env key:

```text
VITE_API_DOMAIN
```

When empty or undefined, the UI calls same-origin `/api`. For deployments under an API `PathBase`, the API client infers the first URL segment unless it is one of the known root routes.
