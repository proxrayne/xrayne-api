import { EncryptionMethod, Protocol } from "@libs/xray-config";

export const PROTOCOL_OPTIONS = [
  {
    label: "Vless",
    value: Protocol.Vless,
  },
  {
    label: "VMess",
    value: Protocol.VMess,
  },
  {
    label: "Trojan",
    value: Protocol.Trojan,
  },
  {
    label: "ShadowSocks",
    value: Protocol.ShadowSocks,
  },
  {
    label: "Wireguard",
    value: Protocol.Wireguard,
  },
  {
    label: "Hysteria",
    value: Protocol.Hysteria,
  },
  {
    label: "Http",
    value: Protocol.Http,
  },
  {
    label: "Socks",
    value: Protocol.Socks,
  },
] as const;

export const ENCRYPTION_METHODS = Object.entries(EncryptionMethod)
  .filter(([, value]) => value !== EncryptionMethod.None)
  .map(([key, value]) => ({ label: key, value }));

export const SOCKS_AUTH = [
  {
    label: "NoAuth",
    value: "noauth",
  },
  {
    label: "Password",
    value: "password",
  },
] as const;

export const TRAFFIC = [
  {
    label: "HTTP",
    value: "http",
  },
  {
    label: "TLS",
    value: "tls",
  },
  {
    label: "Quic",
    value: "quic",
  },
  {
    label: "FakeDNS",
    value: "fakedns",
  },
] as const;
