import type { JsonSchema } from "./json-schema";

export const xrayApiSchema: JsonSchema = {
  type: "object",
  description: "API interface configuration provides APIs based on gRPC for remote invocation.",
  additionalProperties: true,
  properties: {
    tag: {
      type: "string",
      minLength: 1,
      description: "Outbound proxy identifier for API traffic.",
    },
    listen: {
      type: "string",
      description: "IP and port that the API service listens on.",
    },
    services: {
      type: "array",
      minItems: 1,
      uniqueItems: true,
      items: {
        type: "string",
        enum: [
          "HandlerService",
          "LoggerService",
          "StatsService",
          "ReflectionService",
          "ObservatoryService",
        ],
      },
      description: "Enabled API service names.",
    },
  },
};
