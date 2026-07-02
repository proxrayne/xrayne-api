import { api } from "@core/api/instance";

import type {
  AppSettingsResponse,
  AppSubscriptionSettingsDto,
  AppWebhookDto,
  CreateAppWebhookRequest,
  UpdateAppWebhookRequest,
} from "./api.types";

export async function fetchAppSettings(signal?: AbortSignal) {
  const { data } = await api.get<AppSettingsResponse>("settings/app", { signal });

  return data;
}

export async function updateSubscriptionSettings(payload: AppSubscriptionSettingsDto) {
  const { data } = await api.put<AppSettingsResponse>("settings/app/subscription", payload);

  return data;
}

export async function createAppWebhook(payload: CreateAppWebhookRequest) {
  const { data } = await api.post<AppWebhookDto>("settings/app/webhooks", payload);

  return data;
}

export async function updateAppWebhook(id: string, payload: UpdateAppWebhookRequest) {
  const { data } = await api.put<AppWebhookDto>(`settings/app/webhooks/${id}`, payload);

  return data;
}

export async function deleteAppWebhook(id: string) {
  await api.delete(`settings/app/webhooks/${id}`);
}

export async function restartPanel() {
  await api.post("settings/panel/restart");
}

export async function pingApi(signal?: AbortSignal) {
  await api.get("version", { signal });
}
