import { api } from "@core/api/instance";

import { SystemInfoDto } from "./api.types";

export async function fetchSystemStats(signal?: AbortSignal) {
  const { data } = await api.get<SystemInfoDto>("system/snapshot", { signal });

  return data;
}
