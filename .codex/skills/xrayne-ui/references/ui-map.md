# XRayne UI Map

## Stack

- React 19.
- React Router 7 framework mode via `@react-router/dev`.
- Vite 7 with plugins: `mkcert`, `reactRouter`, Tailwind CSS 4, `vite-tsconfig-paths`, `svgr`.
- TypeScript strict mode.
- TanStack Query 5.
- Axios.
- HeroUI React and styles.
- React Hook Form.
- next-themes.

## Scripts

From `XRayne.UI`:

```powershell
npm run dev
npm run build
npm run start
npm run typecheck
npm run lint
```

`typecheck` runs `react-router typegen && tsc`.

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
- Wraps children in `ThemeProvider` and `QueryClientProvider`.
- Shows `PageProgress`.
- Listens for `unauthorize` events, invalidates `admin-account`, and revalidates routes.
- Uses `CommonTemplate` and `ErrorScreen` in `ErrorBoundary`.

## API Client

`src/core/api/instance.ts`:

- Base URL is `${import.meta.env.VITE_API_DOMAIN}/api`.
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
- Use HeroUI components where existing screens use them: `Button`, `Card`, `Form`, `Input`, `Checkbox`, `Spinner`, `ErrorMessage`, etc.

## Environment

Important env key:

```text
VITE_API_DOMAIN
```

When empty or undefined, Axios base URL becomes `undefined/api`; verify env behavior if changing startup scripts.

