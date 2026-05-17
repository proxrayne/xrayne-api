import { queryOptions, useQuery } from "@tanstack/react-query";

import { query } from "@core/lib/query";

import { fetchPanelSettings } from "./api";
import type { PanelSettings } from "./api.types";

export const panelSettingsQuery = queryOptions({
  queryKey: ["panel-settings"],
  queryFn: ({ signal }) => fetchPanelSettings(signal),
  refetchOnWindowFocus: false,
  staleTime: 0,
});

export function usePanelSettings() {
  return useQuery(panelSettingsQuery);
}

usePanelSettings.getOrFetch = () => query.fetchQuery(panelSettingsQuery);

usePanelSettings.setData = (settings: PanelSettings) =>
  query.setQueryData(panelSettingsQuery.queryKey, settings);

usePanelSettings.invalidate = () =>
  query.invalidateQueries({ queryKey: panelSettingsQuery.queryKey });
