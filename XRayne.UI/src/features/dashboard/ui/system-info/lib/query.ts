import { useQuery } from "@tanstack/react-query";

import { fetchSystemStats } from "./api";

export function useSystemStats(interval: number = 15_000) {
  const { data, isFetched, error, refetch } = useQuery({
    queryKey: ["system", "stats"],
    queryFn: ({ signal }) => fetchSystemStats(signal),
    refetchInterval: interval,
    refetchOnWindowFocus: true,
  });

  return {
    stats: data,
    isLoaded: isFetched,
    error,
    refetch,
  };
}
