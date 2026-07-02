import { queryOptions, useMutation, useQuery } from "@tanstack/react-query";

import { createNode, deleteNode, fetchNode, fetchNodes } from "./api";

export const nodeQueryKeys = {
  all: ["nodes"] as const,
  detail: (id: number) => [...nodeQueryKeys.all, id] as const,
};

export const nodesQuery = queryOptions({
  queryKey: nodeQueryKeys.all,
  queryFn: ({ signal }) => fetchNodes(signal),
});

export function useNodesQuery() {
  return useQuery(nodesQuery);
}

export function nodeQuery(id: number) {
  return queryOptions({
    queryKey: nodeQueryKeys.detail(id),
    queryFn: ({ signal }) => fetchNode(id, signal),
    enabled: Number.isFinite(id),
  });
}

export function useNodeQuery(id: number) {
  return useQuery(nodeQuery(id));
}

export function useCreateNodeMutation() {
  return useMutation({
    mutationFn: createNode,
  });
}

export function useDeleteNodeMutation(nodeId: number) {
  return useMutation({
    mutationKey: ["deleteNode"],
    mutationFn: () => deleteNode(nodeId),
  });
}
