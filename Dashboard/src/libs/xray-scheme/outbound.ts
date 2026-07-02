import { objectSchema, schemaVersion, tagSchema } from "./common";
import type { JsonSchema, JsonSchemaRegistration } from "./json-schema";
import { xrayStreamSettingsSchema } from "./transport";

export const xrayOutboundLatestSchema: JsonSchema = {
  $id: "https://xrayne.local/schemas/xray/outbound-latest.schema.json",
  $schema: schemaVersion,
  title: "Xray outbound configuration schema, latest",
  type: "object",
  description: "Outbound connection configuration.",
  additionalProperties: true,
  required: ["protocol", "settings"],
  properties: {
    tag: tagSchema,
    protocol: {
      type: "string",
      enum: [
        "blackhole",
        "dns",
        "freedom",
        "http",
        "loopback",
        "shadowsocks",
        "socks",
        "trojan",
        "vless",
        "vmess",
        "wireguard",
        "hysteria2",
      ],
      description: "Outbound protocol.",
    },
    settings: objectSchema,
    streamSettings: xrayStreamSettingsSchema,
    proxySettings: objectSchema,
    sendThrough: { type: "string", description: "Local address used to send traffic." },
    mux: objectSchema,
  },
};

export const xrayOutboundSchemas: JsonSchemaRegistration[] = [
  {
    uri: xrayOutboundLatestSchema.$id!,
    fileMatch: ["file:///xray-outbound.json"],
    schema: xrayOutboundLatestSchema,
  },
];
