import { useMutation, useQuery } from "@tanstack/react-query";
import { toast } from "@heroui/react";

import {
  fetchCoreStatus,
  fetchInstallingStatus,
  fetchXrayReleases,
  installCore,
} from "./api";
import { FetchXrayReleasesQuery } from "./api.types";

export function useCoreStatus(poolingInterval?: number | false) {
  const { data, isFetched, error, refetch } = useQuery({
    queryKey: ["core", "status"],
    queryFn: ({ signal }) => fetchCoreStatus(signal),
    refetchOnWindowFocus: true,
    refetchInterval: poolingInterval,
  });

  return {
    status: data,
    isLoaded: isFetched,
    error,
    refetch,
  };
}

export function useCoreReleases(query: FetchXrayReleasesQuery) {
  const { data, isFetched, error, refetch } = useQuery({
    queryKey: ["core", "releases", query.page],
    queryFn: ({ signal }) => fetchXrayReleases(query, signal),
  });

  return {
    releases: data,
    isLoaded: isFetched,
    error,
    refetch,
  };
}

interface CoreInstallingStatusOptions {
  enabled?: boolean;
  pullingInterval?: number;
}

export function useCoreInstallingStatus({
  pullingInterval = 3_000,
  enabled = true,
}: CoreInstallingStatusOptions = {}) {
  const { data, isFetched, error, refetch } = useQuery({
    queryKey: ["core", "install", "status"],
    queryFn: ({ signal }) => fetchInstallingStatus(signal),
    refetchInterval: ({ state: { data } }) =>
      data?.step !== "Idle" && enabled ? pullingInterval : false,
    enabled,
  });

  return {
    status: data,
    isLoaded: isFetched,
    error,
    refetch,
  };
}

export function useCoreInstall(version: string) {
  const { mutateAsync, ...mutation } = useMutation({
    mutationKey: ["core", "install", version],
    mutationFn: () => installCore(version),
    onError: () => {
      toast.danger("Unhandled error", {
        description:
          "An error occurred while installing the kernel, please check the logs for details.",
      });
    },
  });

  return [mutateAsync, mutation] as const;
}
