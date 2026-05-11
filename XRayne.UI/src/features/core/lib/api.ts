import { api } from "@core/api/instance";

import {
  CoreInstallingStatus,
  CoreStatusDto,
  FetchXrayReleasesQuery,
  GitHubReleaseDto,
} from "./api.types";

export async function fetchCoreStatus(signal?: AbortSignal) {
  const { data } = await api.get<CoreStatusDto>("core/status", { signal });

  return data;
}

export async function fetchXrayReleases(
  query: FetchXrayReleasesQuery,
  signal?: AbortSignal,
) {
  const { data } = await api.get<GitHubReleaseDto[]>(
    `core/releases?${new URLSearchParams(Object.entries(query))}`,
    { signal },
  );

  return data;
}

export async function fetchInstallingStatus(
  jobId: string,
  signal?: AbortSignal,
) {
  const { data } = await api.get<CoreInstallingStatus | null>(
    `core/install/${jobId}/status`,
    {
      signal,
    },
  );

  return data;
}

export async function installCore(version: string) {
  const { data } = await api.post<string>("core/install", { version });

  return data;
}
