import type { JsonSchema } from "./json-schema";

export const xrayLogSchema: JsonSchema = {
  type: "object",
  description: "Log configuration controls how Xray outputs logs.",
  additionalProperties: true,
  properties: {
    access: {
      type: "string",
      description: "Access log file path. Empty or omitted values write access logs to stdout.",
    },
    error: {
      type: "string",
      description: "Error log file path. Empty or omitted values write error logs to stdout.",
    },
    loglevel: {
      type: "string",
      enum: ["debug", "info", "warning", "error", "none"],
      description: "Error log level. The access log does not have log levels.",
    },
    dnsLog: {
      type: "boolean",
      description: "Log DNS queries made by built-in DNS clients to the access log.",
    },
    maskAddress: {
      type: "string",
      description:
        "IP address masking level for logs. Known values include quarter, half, and full.",
    },
  },
};
