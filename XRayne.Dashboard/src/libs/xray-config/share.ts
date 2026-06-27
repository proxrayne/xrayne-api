import type {
  Fingerprint,
  Protocol,
  StreamNetwork,
  StreamSecurity,
  VMessSecurity,
  XtlsFlow,
} from "./enums";

export interface HysteriaOptions {
  sni?: string;
  insecure?: boolean;
  obfs?: string;
  "obfs-password"?: string;
}

export interface HysteriaShareEntity extends QueryShareEntity {}

export interface JsonShareFormatter extends ShareFormatter {}

export interface QueryShareEntity extends V2RayShareEntity {}

export interface RealityTransferOptions extends TransferOptions {
  flow?: XtlsFlow;
  pbk?: string;
  pqv?: string;
  sid?: string;
  spx?: string;
}

export interface ShadowSocksOptions extends TransferOptions {}

export interface ShadowSocksShareEntity extends QueryShareEntity {}

export interface ShareEntity {
  protocol: Protocol;
  address: string;
  port: number;
  remark: string;
  network: StreamNetwork;
  security: StreamSecurity;
}

export interface ShareFormatter {
  name: string;
}

export interface SocksShareEntity extends QueryShareEntity {}

export interface StreamOptions {
  type?: string;
}

export interface TransferOptions extends StreamOptions {
  path?: string;
  host?: string;
  headerType?: string;
  seed?: string;
  serviceName?: string;
  authority?: string;
  mode?: string;
  security?: string;
  alpn?: string;
  sni?: string;
  ech?: string;
  fp?: string;
  allowInsecure?: boolean;
}

export interface TrojanOptions extends RealityTransferOptions {}

export interface TrojanShareEntity extends QueryShareEntity {}

export interface V2RayShareEntity extends ShareEntity {}

export interface VlessOptions extends RealityTransferOptions {
  encryption?: string;
}

export interface VlessShareEntity extends QueryShareEntity {}

export interface VMessOptions {
  add: string;
  port: number;
  v?: string;
  allowInsecure?: boolean;
  fp?: Fingerprint;
  id: string;
  alpn?: string;
  sni?: string;
  scy?: VMessSecurity;
  ps?: string;
  path?: string;
  tls?: StreamSecurity;
  net: StreamNetwork;
  type?: string;
  host?: string;
  aid?: string;
  authority?: string;
  mode?: string;
}

export interface VMessShareEntity extends V2RayShareEntity {}
