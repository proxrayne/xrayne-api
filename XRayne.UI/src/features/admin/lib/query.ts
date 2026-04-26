import { queryOptions, useQuery } from "@tanstack/react-query";

import { query } from "@core/lib/query";

import { fetchAdminAccount } from "./api";

export const adminAccountQuery = queryOptions({
  queryKey: ["account"],
  queryFn: ({ signal }) => fetchAdminAccount(signal),
  refetchOnWindowFocus: true,
  retry: 1,
});

export function useAdminAccount() {
  const { data, isFetched, error, refetch } = useQuery(adminAccountQuery);

  return {
    account: data,
    isLoaded: isFetched,
    error,
    refetch,
  };
}

useAdminAccount.getOrFetch = () => query.fetchQuery(adminAccountQuery);
