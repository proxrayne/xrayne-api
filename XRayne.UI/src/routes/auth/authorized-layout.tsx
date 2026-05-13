import { CSSProperties } from "react";
import { Outlet } from "react-router";

import { Route } from "./+types/authorized-layout";

import { SidebarProvider } from "@core/ui/sidebar";

import { Sidebar, requireAuthMiddleware } from "@features/auth";

function AuthLayout() {
  return (
    <main className="max-w-390 w-full flex-auto flex">
      <SidebarProvider
        style={
          {
            "--sidebar-width": "18rem",
          } as CSSProperties
        }
      >
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
