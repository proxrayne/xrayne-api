import { stringArraySchema, xrayDurationSchema } from "./common";
import type { JsonSchema } from "./json-schema";

export const xrayObservatorySchema: JsonSchema = {
  type: "object",
  description: "Connection observatory configuration for outbound proxy health checks.",
  additionalProperties: true,
  properties: {
    subjectSelector: stringArraySchema,
    probeUrl: { type: "string", minLength: 1, description: "URL used to check connectivity." },
    probeInterval: xrayDurationSchema,
    enableConcurrency: { type: "boolean" },
  },
};

export const xrayBurstObservatorySchema: JsonSchema = {
  type: "object",
  description: "Burst observatory configuration for outbound proxy health checks.",
  additionalProperties: true,
  properties: {
    subjectSelector: stringArraySchema,
    pingConfig: {
      type: "object",
      additionalProperties: true,
      properties: {
        destination: { type: "string", minLength: 1 },
        connectivity: { type: "string" },
        interval: xrayDurationSchema,
        sampling: { type: "number", minimum: 1 },
        timeout: xrayDurationSchema,
      },
    },
  },
};
