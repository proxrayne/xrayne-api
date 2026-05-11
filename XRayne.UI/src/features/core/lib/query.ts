import { useInfiniteQuery, useMutation, useQuery } from "@tanstack/react-query";
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

interface CoreInstallingStatusOptions {
  pullingInterval?: number;
}

export function useCoreInstallingStatus(
  jobId: string | null,
  { pullingInterval = 5_000 }: CoreInstallingStatusOptions = {},
) {
  const { data, isFetched, error, refetch } = useQuery({
    queryKey: ["core", "install", jobId, "status"],
    queryFn: ({ signal }) => fetchInstallingStatus(jobId!, signal),
    refetchInterval: ({ state }) =>
      state.data?.step === "installed" ? false : pullingInterval,
    enabled: Boolean(jobId),
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
