import { useQuery } from "@tanstack/react-query";

import { fetchCoreStatus } from "./api";

export function useCoreStatus() {
  const { data, isFetched, error, refetch } = useQuery({
    queryKey: ["core", "status"],
    queryFn: ({ signal }) => fetchCoreStatus(signal),
    refetchOnWindowFocus: true,
  });

  return {
    status: data,
    isLoaded: isFetched,
    error,
    refetch,
  };
}
