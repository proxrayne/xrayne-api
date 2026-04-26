import { Outlet } from "react-router";

import { Route } from "./sign-in/+types";

import CommonTemplate from "@core/ui/common-template";

import { requireNoAuthMiddleware } from "../lib/middlewares";

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
