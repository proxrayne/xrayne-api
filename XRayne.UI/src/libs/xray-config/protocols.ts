import type { ClientServer, WithLevel } from "./common";

import type { EncryptionMethod, XtlsFlow } from "./enums";

import type { InboundShadowSocksSettings, InboundSocksSettings, InboundTrojanSettings, InboundVMessSettings, InboundVlessSettings, InboundWireguardSettings } from "./inbounds";

import type { OutboundShadowSocksSettings, OutboundSocksSettings, OutboundTrojanSettings, OutboundVMessSettings, OutboundVlessSettings, OutboundWireguardSettings } from "./outbounds";

export interface ShadowSocksCommon {
  email?: string;
  /** Encryption method, see available options above. */
  method?: EncryptionMethod;
  /** Required for Shadowsocks 2022. A pre-shared key similar to WireGuard is used as the password. */
  password?: string;
}

export type ShadowSocksSettings = InboundShadowSocksSettings | OutboundShadowSocksSettings;

export type SocksSettings = InboundSocksSettings | OutboundSocksSettings;

export interface TrojanServer extends ClientServer {}

export type TrojanSettings = InboundTrojanSettings | OutboundTrojanSettings;

export type VlessSettings = InboundVlessSettings | OutboundVlessSettings;

export interface VlessUser extends WithLevel {
  /** The VLESS user identifier can be any string less than 30 bytes long or a valid UUID. */
  id?: string;
  encryption?: string;
  email?: string;
  flow?: XtlsFlow;
}

export type VMessSettings = InboundVMessSettings | OutboundVMessSettings;

export type WireguardSettings = InboundWireguardSettings | OutboundWireguardSettings;

