import { objectSchema } from "./common";
import type { JsonSchema } from "./json-schema";

export const xrayStreamSettingsSchema: JsonSchema = {
  type: "object",
  description: "Transport settings used by inbound and outbound connections.",
  additionalProperties: true,
  properties: {
    network: {
      type: "string",
      enum: [
        "tcp",
        "raw",
        "kcp",
        "ws",
        "http",
        "h2",
        "domainsocket",
        "quic",
        "grpc",
        "httpupgrade",
        "xhttp",
      ],
      description: "Transport network.",
    },
    security: {
      type: "string",
      enum: ["none", "tls", "reality"],
      description: "Transport security layer.",
    },
    tlsSettings: objectSchema,
    realitySettings: objectSchema,
    tcpSettings: objectSchema,
    rawSettings: objectSchema,
    kcpSettings: objectSchema,
    wsSettings: objectSchema,
    httpSettings: objectSchema,
    httpupgradeSettings: objectSchema,
    grpcSettings: objectSchema,
    xhttpSettings: objectSchema,
    sockopt: objectSchema,
  },
};
