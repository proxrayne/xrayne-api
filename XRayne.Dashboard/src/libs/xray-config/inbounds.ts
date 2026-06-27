import type { Port, WithLevel, WithUserLevel } from "./common";

import type {
  AllocateStrategy,
  EncryptionMethod,
  Protocol,
  SocksAuth,
  TransportProtocol,
  VlessEncryption,
  XtlsFlow,
} from "./enums";

import type { VMessDefaultSettings, WireguardPeer } from "./outbounds";

import type { ShadowSocksCommon } from "./protocols";

import type { StreamSettings } from "./transports";

export interface DokodemoDoorSettings extends WithUserLevel {
  /**
   * Redirect traffic to this address. This can be an IP address, such as "1.2.3.4", or a domain name,
   * such as , "xray.com"by default "localhost". If followRedirect(see below) is equal to true, then it
   * addressmay be empty.
   */
  address?: string;
  /**
   * Redirects traffic to the specified port of the target address, range [0, 65535], numeric type. If
   * empty or equal to 0, the listening address port is used by default.
   */
  port?: number;
  /**
   * This is a mapping map between local ports and the required remote addresses/ports (if inbound
   * listening on multiple ports). If a local port is not specified in this map, processing will be
   * performed according to the address/ settings port.
   */
  portMap?: Record<string, string>;
  /**
   * Supported network protocol types. For example, if specified "tcp", only TCP traffic will be
   * accepted. Default value: "tcp".
   */
  network?: TransportProtocol[];
  /**
   * If the value is true, dokodemo-door will recognize data redirected by iptables and forward it to the
   * appropriate target address. See the setting tproxy in the Transport Configuration
   * (https://xtls.github.io/config/transport.html#sockoptobject) section.
   */
  followRedirect?: boolean;
}

/**
 * Fallback provides Xray with a high degree of protection against active probing and has a unique
 * first packet reservation mechanism.
 */
export interface Fallback {
  /** Attempt to match TLS SNI (Server Name Indication), any value or empty string, defaults to "". */
  name?: string;
  /** Attempt to match the result of the TLS ALPN negotiation, any value or the empty string, defaults to "". */
  alpn?: string;
  /**
   * Attempt to match HTTP path of first packet, any value or empty string, default is empty string, if
   * not empty must start with /, h2c is not supported.
   */
  path?: string;
  /**
   * Specifies where TCP traffic is redirected after TLS decryption, two types of addresses are currently
   * supported (this field is required, otherwise the launch will fail): TCP, format "addr:port", where
   * addr supports IPv4, domain name, IPv6, if domain name is specified, TCP connection will be
   * established directly (without using built-in DNS). Unix domain socket, format - absolute path, for
   * example, "/dev/shm/domain.socket", at the beginning you can add @to denote abstract , @@ - to denote
   * abstract with filling. Note: Starting with version v25.7.26.
   */
  dest?: unknown;
  /**
   * Sending the PROXY protocol (https://www.haproxy.org/download/2.2/doc/proxy-protocol.txt),
   * specifically for transmitting the real source IP address and port of the request, is filled with
   * version 1 or 2; the default is 0, meaning it is not sent. If necessary, it is recommended to specify
   * 1.
   */
  xver?: number;
}

export interface HttpAccount {
  /** Username, data type: string. */
  user?: string;
  /** Password, data type: string. */
  pass?: string;
}
/**
 * Incoming connections are used to receive data. Available protocols can be found in the Incoming
 * Protocols section (https://xtls.github.io/config/inbounds/) section. Docs
 * (https://xtls.github.io/config/inbound.html)
 */
export type Inbound = InboundConfig;
/** Represents any supported inbound configuration. */
export type InboundConfig =
  | DokodemoDoorInbound
  | HttpInbound
  | HysteriaInbound
  | ShadowSocksInbound
  | SocksInbound
  | TrojanInbound
  | TUNInbound
  | VlessInbound
  | VMessInbound
  | WireguardInbound
  | GenericInbound;

/**
 * Incoming connections are used to receive data. Available protocols can be found in the Incoming
 * Protocols section (https://xtls.github.io/config/inbounds/) section. Docs
 * (https://xtls.github.io/config/inbound.html)
 */
export interface InboundBase {
  /**
   * Listening address, IP address, or Unix domain socket. The default value is "0.0.0.0", which accepts
   * connections on all network interfaces.
   */
  listen?: string;
  /** Port. */
  port: Port;
  protocol: Protocol;
  /** The tag for this incoming connection, used to identify this connection in other settings. */
  tag: string;
  /** Traffic detection is primarily used for transparent proxying and other purposes. */
  sniffing?: InboundSniffing;
  allocate?: InboundAllocate;
  /** Transport type is the way the current Xray node interacts with other nodes. */
  streamSettings?: StreamSettings;
}

export interface InboundAllocate {
  strategy?: AllocateStrategy;
  refresh?: number;
  concurrency?: number;
}

export interface InboundClient extends WithLevel {
  /** User email address, used to separate traffic from different users (displayed in logs, statistics). */
  email?: string;
}
/** Traffic detection is primarily used for transparent proxying and other purposes. */
export interface InboundSniffing {
  /** Enable traffic detection. */
  enabled?: boolean;
  /** Replace the current connection's target address with the specified types if the traffic matches them. */
  destOverride?: string[];
  /** If this setting is enabled, only connection metadata will be used to detect the target address. */
  metadataOnly?: boolean;
  /**
   * Use discovered domain names for routing only. The proxy target remains the IP address. The default
   * value is false.
   */
  routeOnly?: boolean;
}

export interface InboundHttpSettings extends WithUserLevel {
  /**
   * An array where each element represents a user account. Default value: an empty array. If accounts
   * not empty, the HTTP proxy will perform Basic Authentication authentication for incoming connections.
   */
  accounts?: HttpAccount[];
  /** If set true, all HTTP requests will be redirected, not just proxy requests. */
  allowTransparent?: boolean;
}

export interface InboundShadowSocksSettings extends ShadowSocksCommon {
  /** ShadowSocks clients */
  clients?: ShadowSocksClient[];
  /**
   * Supported network protocol types. For example, if specified "tcp", only TCP traffic will be
   * accepted. Default value: "tcp".
   */
  network?: TransportProtocol[];
}

export interface InboundSocksSettings extends WithUserLevel {
  /**
   * Authentication method for the Socks protocol. Both anonymous "noauth" and password-based methods are
   * supported "password". The default value is "noauth".
   */
  auth?: SocksAuth;
  /**
   * An array where each element represents a user account. This parameter is only valid if the parameter
   * auth is set to password. The default value is an empty array.
   */
  accounts?: SocksAccount[];
  /** Whether to enable UDP protocol support. The default value is false. */
  udp?: boolean;
  /**
   * If UDP support is enabled, Xray must know the IP address of the local computer. Warning : If you
   * have multiple IP addresses configured on your machine, this may affect how inbound works when using
   * UDP 0.0.0.0.
   */
  ip?: string;
}

export interface InboundTrojanSettings {
  /** An array representing a group of users approved by the server. */
  clients?: TrojanClient[];
  /**
   * An array containing a series of fallback routing configurations (optional). For details on
   * configuring fallbacks, see FallbackObject (https://xtls.github.io/en/config/features/fallback.html).
   */
  fallbacks?: Fallback[];
}

export interface InboundVlessSettings {
  /** An array representing a group of users approved by the server. */
  clients?: VlessClient[];
  /**
   * VLESS encryption settings (https://github.com/XTLS/Xray-core/pull/5067). This cannot be empty; to
   * disable it, you must explicitly set the value "none".
   */
  decryption?: VlessEncryption;
  /**
   * An array containing a series of fallback routing configurations (optional). For details on
   * configuring fallbacks, see FallbackObject.
   */
  fallbacks?: Fallback[];
}

export interface InboundVMessSettings {
  /** An array representing a group of users approved by the server. */
  clients?: VMessClient[];
  /** Default configuration for clients. Only valid when used with detour. */
  default?: VMessDefaultSettings;
}

export interface InboundWireguardSettings {
  /** Private key. */
  secretKey: string;
  /** List of peer servers, each entry represents the configuration of one server. */
  peers?: WireguardPeer[];
  /** Wireguard tun layer fragmentation size. */
  mtu?: number;
}

export interface HysteriaInboundSettings {
  /** Hysteria version, must be 2. */
  version?: number;
  /** An array representing a group of users approved by the server. */
  clients?: HysteriaInboundClient[];
}
/**
 * Tunnel, or Dokodemo door, can listen on a local port and send all data received on this port through
 * outbound to the specified server port, thereby achieving the effect of port forwarding.
 */
export interface DokodemoDoorInbound extends InboundBase {
  protocol: typeof Protocol.DokodemoDoor;
  settings?: DokodemoDoorSettings;
}

export interface GenericInbound extends InboundBase {
  settings?: unknown;
}
/**
 * Using incoming connections http is more appropriate in a local area network or local environment
 * where it can be used to listen for incoming connections and provide local services to other
 * programs.
 */
export interface HttpInbound extends InboundBase {
  protocol: typeof Protocol.Http;
  settings?: InboundHttpSettings;
}
/** Server-side configuration for the Hysteria protocol. */
export interface HysteriaInbound extends InboundBase {
  protocol: typeof Protocol.Hysteria;
  settings?: HysteriaInboundSettings;
}
/**
 * Shadowsocks protocol, compatible with most other implementations. Docs
 * (https://wikipedia.org/wiki/Shadowsocks)
 */
export interface ShadowSocksInbound extends InboundBase {
  protocol: typeof Protocol.ShadowSocks;
  settings?: InboundShadowSocksSettings;
}
/**
 * An implementation of the standard Socks protocol, compatible with Socks 4
 * (http://ftp.icm.edu.pl/packages/socks/socks4/SOCKS4.protocol), Socks 4a
 * (https://ftp.icm.edu.pl/packages/socks/socks4/SOCKS4A.protocol), Socks 5, and HTTP.
 */
export interface SocksInbound extends InboundBase {
  protocol: typeof Protocol.Socks;
  settings?: InboundSocksSettings;
}
/** We will now show how a trojan server will react to a valid Trojan Protocol and other protocols. */
export interface TrojanInbound extends InboundBase {
  protocol: typeof Protocol.Trojan;
  settings?: InboundTrojanSettings;
}
/**
 * Creates a TUN interface; traffic sent to this interface will be processed by Xray. Currently, only
 * Windows and Linux are supported.
 */
export interface TUNInbound extends InboundBase {
  protocol: typeof Protocol.Tun;
  settings?: TUNSettings;
}
/**
 * VLESS is a lightweight, stateless transport protocol that is split into an inbound and outbound
 * portion and can serve as a bridge between an Xray client and server.
 */
export interface VlessInbound extends InboundBase {
  protocol: typeof Protocol.Vless;
  settings?: InboundVlessSettings;
}
/** VMess is an encrypted transport protocol that is typically used as a bridge between Xray clients and servers. */
export interface VMessInbound extends InboundBase {
  protocol: typeof Protocol.VMess;
  settings?: InboundVMessSettings;
}
/** User-space implementation of the Wireguard protocol. */
export interface WireguardInbound extends InboundBase {
  protocol: typeof Protocol.Wireguard;
  settings?: InboundWireguardSettings;
}

export interface HysteriaInboundClient extends InboundClient {
  /** Authentication string. */
  auth: string;
}

export interface ShadowSocksClient extends InboundClient {
  method?: EncryptionMethod;
  password?: string;
}

export interface SocksAccount {
  /** Username, type - string. */
  user?: string;
  /** Password, type - string. */
  pass?: string;
}

export interface TrojanClient extends InboundClient {
  /** Required parameter, any string. */
  password?: string;
}

export interface TUNSettings extends WithUserLevel {
  /** The name of the created TUN interface. Default is "xray0". */
  name?: string;
  /** The MTU of the interface. Default is 1500. */
  MTU?: number;
}

export interface VlessClient extends InboundClient {
  /** The VLESS user identifier can be any string less than 30 bytes long or a valid UUID. */
  id?: string;
  /** Flow control mode, used to select the XTLS algorithm. */
  flow?: XtlsFlow;
}

export interface VMessClient extends InboundClient {
  /** The VMess user ID. This can be any string less than 30 bytes long or a valid UUID. */
  id?: string;
}
