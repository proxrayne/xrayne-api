import { redirect, type MiddlewareFunction } from "react-router";

import { urls } from "@core/lib/urls";

import { useAdminAccount } from "@features/admin";

export const requireAuthMiddleware = (async ({ request }, next) => {
  const account = await useAdminAccount.getOrFetch().catch(() => null);
  if (account) {
    return next();
  }

  const url = new URL(request.url);

  throw redirect(urls.signIn(encodeURI(url.pathname + url.search)).toString(), {
    status: 301,
  });
}) as MiddlewareFunction<Response>;

export const requireNoAuthMiddleware = (async (_, next) => {
  const account = await useAdminAccount.getOrFetch().catch(() => null);
  if (!account) {
    return next();
  }

  throw redirect(urls.root().toString(), { status: 301 });
}) as MiddlewareFunction<Response>;
