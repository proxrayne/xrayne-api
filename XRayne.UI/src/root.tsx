import { type PropsWithChildren } from "react";
import {
  Links,
  Meta,
  Outlet,
  Scripts,
  ScrollRestoration,
  useRouteError,
} from "react-router";
import { ThemeProvider } from "next-themes";

import { Route } from "./+types/root";

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
          {children}
        </ThemeProvider>
        <ScrollRestoration />
        <Scripts />
      </body>
    </html>
  );
}

export default () => {
  return <Outlet />;
};

export const links: Route.LinksFunction = () => [];

export function HydrateFallback() {
  return (
    <main className="fixed inset-0 flex flex-col gap-y-3 justify-center items-center">
      <h1 className="text-3xl font-medium">XRayne</h1>
    </main>
  );
}

export function meta({ error }: Route.MetaArgs) {
  if (error) {
    return [{ title: "Error" }];
  }

  return [
    {
      title: "XRayne",
    },
  ];
}

export function ErrorBoundary() {
  const error = useRouteError();

  return <div>error</div>;
}
