import { objectSchema, portSchema, schemaVersion, tagSchema } from "./common";
import type { JsonSchema, JsonSchemaRegistration } from "./json-schema";
import { xrayStreamSettingsSchema } from "./transport";

export const xrayInboundLatestSchema: JsonSchema = {
  $id: "https://xrayne.local/schemas/xray/inbound-latest.schema.json",
  $schema: schemaVersion,
  title: "Xray inbound configuration schema, latest",
  type: "object",
  description: "Inbound connection configuration.",
  additionalProperties: true,
  required: ["protocol", "settings"],
  properties: {
    tag: tagSchema,
    listen: { type: "string", description: "Listening address." },
    port: portSchema,
    protocol: {
      type: "string",
      enum: [
        "dokodemo-door",
        "http",
        "shadowsocks",
        "socks",
        "trojan",
        "vless",
        "vmess",
        "wireguard",
        "hysteria2",
        "tun",
      ],
      description: "Inbound protocol.",
    },
    settings: objectSchema,
    streamSettings: xrayStreamSettingsSchema,
    sniffing: objectSchema,
    allocate: objectSchema,
  },
};

export const xrayInboundSchemas: JsonSchemaRegistration[] = [
  {
    uri: xrayInboundLatestSchema.$id!,
    fileMatch: ["file:///xray-inbound.json"],
    schema: xrayInboundLatestSchema,
  },
];
