import { useInfiniteQuery, useMutation } from "@tanstack/react-query";
import { toast } from "sonner";

import {
  fetchXrayReleases,
  installCore,
  restartCore,
  startCore,
  stopCore,
} from "./api";
import { FetchXrayReleasesQuery } from "./api.types";

export function useCoreReleases(
  query: Pick<FetchXrayReleasesQuery, "perPage">,
) {
  const {
    data,
    isFetched,
    error,
    hasNextPage,
    isFetchingNextPage,
    fetchNextPage,
    refetch,
  } = useInfiniteQuery({
    queryKey: ["core", "releases"],
    queryFn: ({ signal, pageParam }) =>
      fetchXrayReleases({ ...query, page: pageParam }, signal),
    getNextPageParam: (last, pages) =>
      last.length !== query.perPage ? null : pages.length + 1,
    initialPageParam: 1,
    initialData: { pageParams: [1], pages: [] },
  });

  return {
    releases: data.pages.flatMap((x) => x),
    isLoaded: isFetched,
    error,
    hasMore: hasNextPage,
    isMoreLoading: isFetchingNextPage,
    loadMore: fetchNextPage,
    refetch,
  };
}

export function useCoreInstall(version: string) {
  const { mutateAsync, ...mutation } = useMutation({
    mutationKey: ["core", "install", version],
    mutationFn: () => installCore(version),
    onError: () => {
      toast.error("Unhandled error", {
        description:
          "An error occurred while installing the xray-core, please check the logs for details.",
      });
    },
  });

  return [mutateAsync, mutation] as const;
}

export function useStartCore() {
  const { mutateAsync, ...mutation } = useMutation({
    mutationKey: ["core", "start"],
    mutationFn: () => startCore(),
    onError: () => {
      toast.error("Unhandled error", {
        description:
          "An error occurred while starting xray-core, please check the logs for details.",
      });
    },
  });

  return [mutateAsync, mutation] as const;
}

export function useStopCore() {
  const { mutateAsync, ...mutation } = useMutation({
    mutationKey: ["core", "stop"],
    mutationFn: () => stopCore(),
    onError: () => {
      toast.error("Unhandled error", {
        description:
          "An error occurred while stopping xray-core, please check the logs for details.",
      });
    },
  });

  return [mutateAsync, mutation] as const;
}

export function useRestartCore() {
  const { mutateAsync, ...mutation } = useMutation({
    mutationKey: ["core", "restart"],
    mutationFn: () => restartCore(),
    onError: () => {
      toast.error("Unhandled error", {
        description:
          "An error occurred while restarting xray-core, please check the logs for details.",
      });
    },
  });

  return [mutateAsync, mutation] as const;
}
