import { Outlet } from "react-router";

import { Route } from "./sign-in/+types";

import Sidebar, { SidebarProvider } from "../ui/sidebar";

import { requireAuthMiddleware } from "../lib/middlewares";

function AuthLayout() {
  return (
    <main className="container flex-auto flex">
      <SidebarProvider>
        <Sidebar />
        <div className="flex-auto px-3 md:px-5">
          <Outlet />
        </div>
      </SidebarProvider>
    </main>
  );
}

export const clientMiddleware: Route.MiddlewareFunction[] = [
  requireAuthMiddleware,
];

export default AuthLayout;
