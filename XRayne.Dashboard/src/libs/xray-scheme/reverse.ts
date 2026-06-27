import { objectSchema } from "./common";
import type { JsonSchema } from "./json-schema";

export const xrayReverseSchema: JsonSchema = {
  type: "object",
  description: "Reverse proxy configuration.",
  additionalProperties: true,
  properties: {
    bridges: { type: "array", items: objectSchema },
    portals: { type: "array", items: objectSchema },
  },
};
