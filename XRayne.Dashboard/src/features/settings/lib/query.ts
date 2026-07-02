import { queryOptions, useQuery } from "@tanstack/react-query";

import { query } from "@core/lib/query";

import { fetchAppSettings } from "./api";

export const appSettingsQuery = queryOptions({
  queryKey: ["app", "settings"],
  queryFn: ({ signal }) => fetchAppSettings(signal),
  refetchOnWindowFocus: false,
  retry: 1,
});

export function useAppSettings() {
  const { data, isFetched, error, refetch } = useQuery(appSettingsQuery);

  return {
    settings: data,
    isLoaded: isFetched,
    error,
    refetch,
  };
}

useAppSettings.invalidate = () => query.invalidateQueries({ queryKey: appSettingsQuery.queryKey });

useAppSettings.setData = (settings: Awaited<ReturnType<typeof fetchAppSettings>>) =>
  query.setQueryData(appSettingsQuery.queryKey, settings);
