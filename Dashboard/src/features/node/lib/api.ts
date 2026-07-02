import { api } from "@core/api/instance";

import type { CreateNodeRequest, CreateNodeResponse, NodeDto } from "./api.types";

export async function fetchNodes(signal?: AbortSignal): Promise<NodeDto[]> {
  const { data } = await api.get<NodeDto[]>("nodes", { signal });

  return data;
}

export async function fetchNode(id: number, signal?: AbortSignal): Promise<NodeDto> {
  const { data } = await api.get<NodeDto>(`nodes/${id}`, { signal });

  return data;
}

export async function createNode(payload: CreateNodeRequest): Promise<CreateNodeResponse> {
  const { data } = await api.post<CreateNodeResponse>("nodes", payload);

  return data;
}

export async function deleteNode(id: number): Promise<void> {
  await api.delete(`nodes/${id}`);
}
