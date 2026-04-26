import { isRouteErrorResponse } from "react-router";

interface ErrorData {
  message: string;
  details: string;
  stack?: string;
  status: number;
}

export function parseRouteError(error: unknown): ErrorData {
  if (isRouteErrorResponse(error)) {
    const isNotFound = error.status === 404;

    return {
      status: error.status,
      message: isNotFound ? "Page not found" : "Error",
      details: isNotFound
        ? "The requested page could not be found."
        : error.statusText || "An unexpected error occurred.",
    };
  }

  if (import.meta.env.DEV && error && error instanceof Error) {
    return {
      message: "Oops!",
      details: error.message,
      stack: error.stack,
      status: 400,
    };
  }

  return {
    status: 500,
    message: "Oops!",
    details: "An unexpected error occurred.",
  };
}
