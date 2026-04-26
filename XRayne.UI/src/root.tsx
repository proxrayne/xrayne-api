import { useEffect, type PropsWithChildren } from "react";
import {
  Links,
  Meta,
  Outlet,
  Scripts,
  ScrollRestoration,
  useRevalidator,
  useRouteError,
} from "react-router";
import { ThemeProvider } from "next-themes";
import { QueryClientProvider } from "@tanstack/react-query";

import { Route } from "./+types/root";

import { query } from "@core/lib/query";
import { constructMetadata } from "@core/lib/meta";
import CommonTemplate from "@core/ui/common-template";

import { ErrorScreen, parseRouteError } from "@features/service";

import "@core/styles/app.css";

export function Layout({ children }: PropsWithChildren) {
  return (
    <html lang="en" suppressHydrationWarning translate="no">
      <head>
        <meta charSet="utf-8" />
        <meta
          name="viewport"
          content="width=device-width, initial-scale=1, maximum-scale=1, viewport-fit=cover"
        />
        <meta name="google" content="notranslate" />
        <meta name="theme-color" content="#0a0a0a" suppressHydrationWarning />
        <meta name="color-scheme" content="light dark" />
        <meta
          name="apple-mobile-web-app-status-bar-style"
          content="black-translucent"
        />
        <meta name="apple-mobile-web-app-capable" content="yes" />
        <Meta />
        <Links />
      </head>
      <body className="min-h-screen bg-background antialiased flex flex-col items-center">
        <ThemeProvider
          attribute={["class", "data-theme"]}
          defaultTheme="system"
          enableSystem
          enableColorScheme
          disableTransitionOnChange
        >
          <QueryClientProvider client={query}>{children}</QueryClientProvider>
        </ThemeProvider>
        <ScrollRestoration />
        <Scripts />
      </body>
    </html>
  );
}

export default () => {
  const { revalidate } = useRevalidator();

  useEffect(() => {
    const handler = async () => {
      await query.invalidateQueries({
        queryKey: ["admin-account"],
        refetchType: "none",
      });
      await revalidate();

      // reset admin data this
      // await Promise.all([]);
    };

    document.addEventListener("unauthorize", handler);

    return () => {
      document.removeEventListener("unauthorize", handler);
    };
  }, []);

  return <Outlet />;
};

export const links: Route.LinksFunction = () => [];

export function HydrateFallback() {
  return (
    <main className="fixed inset-0 flex flex-col gap-y-3 justify-center items-center">
      <h1 className="text-3xl font-medium">XRayne.Node</h1>
    </main>
  );
}

export function meta({ error }: Route.MetaArgs) {
  if (error) {
    const { message, details } = parseRouteError(error);

    return constructMetadata({
      title: message,
      description: details,
      robots: ["noindex", "nofollow"],
    });
  }

  return constructMetadata({
    title: "XRayne",
    description: "Panel for xray-core control",
  });
}

export function ErrorBoundary() {
  const error = useRouteError();
  const { details, message, status, stack } = parseRouteError(error);

  return (
    <CommonTemplate>
      <ErrorScreen status={status} title={message} details={details}>
        {stack}
      </ErrorScreen>
    </CommonTemplate>
  );
}
