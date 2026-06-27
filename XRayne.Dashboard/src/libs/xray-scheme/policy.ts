import { objectSchema } from "./common";
import type { JsonSchema } from "./json-schema";

export const xrayPolicySchema: JsonSchema = {
  type: "object",
  description: "Local policy configuration for user levels and system stats.",
  additionalProperties: true,
  properties: {
    levels: {
      type: "object",
      description: "Policies keyed by user level.",
      additionalProperties: objectSchema,
    },
    system: {
      type: "object",
      description: "System-wide policy options.",
      additionalProperties: true,
      properties: {
        statsInboundUplink: { type: "boolean" },
        statsInboundDownlink: { type: "boolean" },
        statsOutboundUplink: { type: "boolean" },
        statsOutboundDownlink: { type: "boolean" },
      },
    },
  },
};
