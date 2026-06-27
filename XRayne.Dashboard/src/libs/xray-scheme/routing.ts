import { schemaVersion, stringArraySchema } from "./common";
import type { JsonSchema, JsonSchemaRegistration } from "./json-schema";

export const xrayRoutingRuleLatestSchema: JsonSchema = {
  $id: "https://xrayne.local/schemas/xray/routing-rule-latest.schema.json",
  $schema: schemaVersion,
  title: "Xray routing rule schema, latest",
  type: "object",
  description:
    "Routing rule. Matching traffic is sent to the configured outbound tag or balancer tag.",
  additionalProperties: true,
  required: ["type"],
  properties: {
    type: {
      type: "string",
      const: "field",
      description: "Rule type. Current Xray configs commonly use field.",
      default: "field",
    },
    domain: stringArraySchema,
    ip: stringArraySchema,
    port: { type: ["string", "number"], description: "Destination port or port range." },
    sourcePort: { type: ["string", "number"], description: "Source port or port range." },
    network: {
      type: "string",
      pattern: "^(tcp|udp)(,(tcp|udp))*$",
      description: "Network matcher, for example tcp, udp, or tcp,udp.",
    },
    source: stringArraySchema,
    user: stringArraySchema,
    inboundTag: stringArraySchema,
    protocol: stringArraySchema,
    attrs: { type: "string", description: "Expression matcher for connection attributes." },
    outboundTag: { type: "string", minLength: 1, description: "Outbound tag used on match." },
    balancerTag: { type: "string", minLength: 1, description: "Balancer tag used on match." },
  },
};

export const xrayRoutingLatestSchema: JsonSchema = {
  $id: "https://xrayne.local/schemas/xray/routing-latest.schema.json",
  $schema: schemaVersion,
  title: "Xray routing configuration schema, latest",
  type: "object",
  description: "Routing configuration. Rules route connections through different outbounds.",
  additionalProperties: true,
  properties: {
    domainStrategy: {
      type: "string",
      enum: ["AsIs", "IPIfNonMatch", "IPOnDemand"],
      description: "Domain resolution strategy used by routing.",
    },
    domainMatcher: {
      type: "string",
      description: "Domain matcher implementation.",
    },
    rules: {
      type: "array",
      description: "Routing rules evaluated by Xray.",
      items: xrayRoutingRuleLatestSchema,
    },
    balancers: {
      type: "array",
      description: "Outbound balancers used by routing rules.",
      items: { type: "object", additionalProperties: true },
    },
  },
};

export const xrayRoutingSchemas: JsonSchemaRegistration[] = [
  {
    uri: xrayRoutingLatestSchema.$id!,
    fileMatch: ["file:///xray-routing.json"],
    schema: xrayRoutingLatestSchema,
  },
];
