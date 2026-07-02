import type { NodeAuthType, NodeDto, NodeStatus } from "@features/node";

export interface NodeRelationCounts {
  inbounds: number;
  outbounds: number;
  routingRules: number;
  certificates: number;
  geoResources: number;
}

export type { NodeAuthType, NodeStatus };

export type NodeModel = NodeDto;
