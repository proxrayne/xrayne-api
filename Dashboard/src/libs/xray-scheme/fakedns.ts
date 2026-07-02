import type { JsonSchema } from "./json-schema";

export const xrayFakeDnsSchema: JsonSchema = {
  type: "object",
  description: "FakeDNS configuration.",
  additionalProperties: true,
  properties: {
    ipPool: { type: "string", minLength: 1, description: "FakeDNS IP pool." },
    poolSize: { type: "number", minimum: 1, description: "FakeDNS pool size." },
  },
};
