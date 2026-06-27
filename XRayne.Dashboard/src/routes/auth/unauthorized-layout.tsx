import { Outlet } from "react-router";

import { Route } from "./+types/unauthorized-layout";

import CommonTemplate from "@core/ui/common-template";

import { requireNoAuthMiddleware } from "@features/auth";

function UnauthorizedLayout() {
  return (
    <CommonTemplate>
      <Outlet />
    </CommonTemplate>
  );
}

export const clientMiddleware: Route.MiddlewareFunction[] = [
  requireNoAuthMiddleware,
];

export default UnauthorizedLayout;
