import { api } from "@core/api/instance";

import { CoreStatusDto } from "./api.types";

export async function fetchCoreStatus(signal?: AbortSignal) {
  const { data } = await api.get<CoreStatusDto>("core/status", { signal });

  return data;
}
