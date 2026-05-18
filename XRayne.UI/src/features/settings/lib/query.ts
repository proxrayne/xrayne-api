import { queryOptions, useQuery } from "@tanstack/react-query";

import { query } from "@core/lib/query";

import { fetchPanelSettings } from "./api";
import type { PanelSettingsDto } from "./api.types";

export const panelSettingsQuery = queryOptions({
  queryKey: ["panel", "settings"],
  queryFn: ({ signal }) => fetchPanelSettings(signal),
  refetchOnWindowFocus: false,
  staleTime: 0,
  retry: 1,
});

export function usePanelSettings() {
  const { data, isFetched, error, refetch } = useQuery(panelSettingsQuery);

  const settings = data?.settings;
  const pendingRestart = data?.pendingRestart ?? false;

  return {
    settings,
    pendingRestart,
    isLoaded: isFetched,
    error,
    refetch,
  };
}

usePanelSettings.getOrFetch = () => query.fetchQuery(panelSettingsQuery);

usePanelSettings.setData = (settings: PanelSettingsDto) =>
  query.setQueryData(panelSettingsQuery.queryKey, (prev) => ({
    pendingRestart: prev?.pendingRestart ?? false,
    settings,
  }));

usePanelSettings.invalidate = () =>
  query.invalidateQueries({ queryKey: panelSettingsQuery.queryKey });

usePanelSettings.fetch = () => query.fetchQuery(panelSettingsQuery);
