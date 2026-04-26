import { api, getAuthorizationToken } from "@core/api/instance";

export async function fetchAdminAccount(signal?: AbortSignal) {
  if (!getAuthorizationToken()) {
    throw new Error("Unauthorized");
  }

  const { data } = await api.get<AdminAccount>("auth/me", { signal });

  return data;
}
