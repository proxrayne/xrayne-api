import type { JsonSchema } from "./json-schema";

export const schemaVersion = "https://json-schema.org/draft/2020-12/schema";

export const objectSchema: JsonSchema = {
  type: "object",
  additionalProperties: true,
};

export const stringArraySchema: JsonSchema = {
  type: "array",
  items: { type: "string", minLength: 1 },
  uniqueItems: true,
};

export const xrayDurationSchema: JsonSchema = {
  type: "string",
  pattern: "^([0-9]+(ns|us|µs|ms|s|m|h))+$",
  description:
    "Xray duration string, for example 10s, 2h45m, or another duration accepted by Xray.",
};

export const tagSchema: JsonSchema = {
  type: "string",
  minLength: 1,
  description: "Non-empty Xray tag used to reference this configuration section.",
};

export const portSchema: JsonSchema = {
  type: ["number", "string"],
  minimum: 1,
  maximum: 65535,
  pattern: "^([1-9][0-9]{0,4})(-([1-9][0-9]{0,4}))?$",
  description: "Single port, numeric port string, or port range accepted by Xray.",
};
