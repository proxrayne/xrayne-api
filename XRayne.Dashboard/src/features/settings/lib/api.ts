import { api } from "@core/api/instance";

import type {
  PanelSettingsResponse,
  UpdatePanelSettingsRequest,
  UpdatePanelSettingsResponse,
} from "./api.types";

export async function fetchPanelSettings(signal?: AbortSignal) {
  const { data } = await api.get<PanelSettingsResponse>("settings/panel", {
    signal,
  });

  return data;
}

export async function updatePanelSettings(payload: UpdatePanelSettingsRequest) {
  const { data } = await api.put<UpdatePanelSettingsResponse>("settings/panel", payload);
  return data;
}

export async function restartPanel() {
  await api.post("settings/panel/restart");
}

export async function pingApi(signal?: AbortSignal) {
  await api.get("version", { signal });
}
