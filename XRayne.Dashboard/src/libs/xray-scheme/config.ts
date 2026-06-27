import { objectSchema, schemaVersion } from "./common";
import { xrayApiSchema } from "./api";
import { xrayDnsSchema } from "./dns";
import { xrayFakeDnsSchema } from "./fakedns";
import { xrayInboundLatestSchema } from "./inbound";
import type { JsonSchema, JsonSchemaRegistration } from "./json-schema";
import { xrayLogSchema } from "./log";
import { xrayBurstObservatorySchema, xrayObservatorySchema } from "./observatory";
import { xrayOutboundLatestSchema } from "./outbound";
import { xrayPolicySchema } from "./policy";
import { xrayReverseSchema } from "./reverse";
import { xrayRoutingLatestSchema } from "./routing";
import { xrayStreamSettingsSchema } from "./transport";

const geodataSchema: JsonSchema = {
  type: "object",
  description: "Geodata file auto-update and hot-reload configuration.",
  additionalProperties: true,
};

const versionSchema: JsonSchema = {
  type: "object",
  description:
    "Controls the Xray version range this config may run on. Supported by Xray v25.8.3+.",
  additionalProperties: false,
  properties: {
    min: { type: "string", minLength: 1, description: "Minimum Xray version, format x.y.z." },
    max: { type: "string", minLength: 1, description: "Maximum Xray version, format x.y.z." },
  },
};

const baseProperties: Record<string, JsonSchema> = {
  log: xrayLogSchema,
  api: xrayApiSchema,
  dns: xrayDnsSchema,
  routing: xrayRoutingLatestSchema,
  policy: xrayPolicySchema,
  inbounds: {
    type: "array",
    description: "Inbound connection configurations.",
    items: xrayInboundLatestSchema,
  },
  outbounds: {
    type: "array",
    description: "Outbound connection configurations.",
    items: xrayOutboundLatestSchema,
  },
  transport: xrayStreamSettingsSchema,
  stats: {
    type: "object",
    description: "Traffic statistics configuration.",
    additionalProperties: true,
  },
  reverse: xrayReverseSchema,
  fakedns: {
    anyOf: [xrayFakeDnsSchema, { type: "array", items: xrayFakeDnsSchema }],
    description: "FakeDNS configuration.",
  },
  metrics: {
    type: "object",
    description: "Metrics export configuration.",
    additionalProperties: true,
    properties: {
      tag: { type: "string", minLength: 1 },
      listen: { type: "string" },
    },
  },
  observatory: xrayObservatorySchema,
  burstObservatory: xrayBurstObservatorySchema,
};

export const xrayConfig2024Schema: JsonSchema = {
  $id: "https://xrayne.local/schemas/xray/config-2024.schema.json",
  $schema: schemaVersion,
  title: "Xray full configuration schema, 2024-compatible",
  description:
    "Permissive JSON schema for full Xray configurations used by 2024-era Xray releases. Deprecated aliases are marked with the JSON Schema deprecated annotation.",
  type: "object",
  additionalProperties: true,
  properties: baseProperties,
};

export const xrayConfigLatestSchema: JsonSchema = {
  $id: "https://xrayne.local/schemas/xray/config-latest.schema.json",
  $schema: schemaVersion,
  title: "Xray full configuration schema, latest",
  description:
    "Permissive JSON schema for the latest documented Xray full configuration. The official Project X documentation is synchronized with the latest release.",
  type: "object",
  additionalProperties: true,
  properties: {
    ...baseProperties,
    geodata: geodataSchema,
    version: versionSchema,
  },
};

export const xrayConfigSchemas: JsonSchemaRegistration[] = [
  {
    uri: xrayConfig2024Schema.$id!,
    fileMatch: ["file:///xray-2024.json"],
    schema: xrayConfig2024Schema,
  },
  {
    uri: xrayConfigLatestSchema.$id!,
    fileMatch: ["file:///xray-latest.json", "file:///xrayne-editor.json"],
    schema: xrayConfigLatestSchema,
  },
];

export const xrayCoreConfigSchemas = xrayConfigSchemas;

export const xrayConfigSectionSchemas = {
  api: xrayApiSchema,
  burstObservatory: xrayBurstObservatorySchema,
  dns: xrayDnsSchema,
  fakedns: xrayFakeDnsSchema,
  inbound: xrayInboundLatestSchema,
  log: xrayLogSchema,
  observatory: xrayObservatorySchema,
  outbound: xrayOutboundLatestSchema,
  policy: xrayPolicySchema,
  reverse: xrayReverseSchema,
  routing: xrayRoutingLatestSchema,
  stats: objectSchema,
  transport: xrayStreamSettingsSchema,
};
