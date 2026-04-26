import { Outlet } from "react-router";

import { Route } from "./sign-in/+types";

import { requireAuthMiddleware } from "../lib/middlewares";

function AuthLayout() {
  return (
    <div className="container">
      <Outlet />
    </div>
  );
}

export const clientMiddleware: Route.MiddlewareFunction[] = [
  requireAuthMiddleware,
];

export default AuthLayout;
