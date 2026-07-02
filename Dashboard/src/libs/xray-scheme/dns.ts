import { objectSchema } from "./common";
import type { JsonSchema } from "./json-schema";

export const xrayDnsSchema: JsonSchema = {
  type: "object",
  description: "Built-in DNS server configuration. If omitted, Xray uses system DNS settings.",
  additionalProperties: true,
  properties: {
    hosts: {
      type: "object",
      description: "Static domain to IP/domain mappings.",
      additionalProperties: true,
    },
    servers: {
      type: "array",
      description: "DNS server definitions as strings or server objects.",
      items: { anyOf: [{ type: "string", minLength: 1 }, objectSchema] },
    },
    clientIp: {
      type: "string",
      description:
        "Deprecated alias retained for legacy configs. Prefer ECS-related DNS options when available.",
      deprecated: true,
    },
    queryStrategy: {
      type: "string",
      enum: ["UseIP", "UseIPv4", "UseIPv6", "UseSystem"],
      description: "DNS query strategy.",
    },
    disableCache: { type: "boolean", description: "Disable DNS cache." },
    disableFallback: { type: "boolean", description: "Disable fallback DNS servers." },
    disableFallbackIfMatch: {
      type: "boolean",
      description: "Disable fallback DNS servers when a domain rule matches.",
    },
    tag: { type: "string", minLength: 1, description: "Inbound tag for DNS traffic." },
  },
};
