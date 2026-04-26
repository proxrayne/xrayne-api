import { useEffect, useState } from "react";
import { QueryClient } from "@tanstack/react-query";

export const makeQueryClient = () =>
  new QueryClient({
    defaultOptions: {
      queries: {
        retry: 1,
        refetchOnWindowFocus: false,
        staleTime({ state }) {
          if (!state.dataUpdateCount || state.isInvalidated) {
            return 0;
          }

          // default staleTime for all queries
          return 5_000 * 60;
        },
      },
    },
  });

export const useDelayedQuery = (delay: number) => {
  const [enabled, setEnabled] = useState(!delay);

  useEffect(() => {
    if (enabled) {
      return;
    }

    const timeout = setTimeout(() => setEnabled(true), delay);

    return () => {
      clearTimeout(timeout);
    };
  }, []);

  return enabled;
};

export const query = makeQueryClient();

if (typeof window !== "undefined") {
  window.__TANSTACK_QUERY_CLIENT__ = query;
}
