import { Outlet } from "react-router";

import { Route } from "./+types/authorized-layout";

import {
  Sidebar,
  SidebarProvider,
  requireAuthMiddleware,
} from "@features/auth";

function AuthLayout() {
  return (
    <main className="max-w-390 w-full flex-auto flex">
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
