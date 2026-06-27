import type { ClientServer, VNextModel, WithLevel, WithUserLevel } from "./common";

import type {
  DomainStrategy,
  EncryptionMethod,
  HeadersType,
  NonIPQueryType,
  Protocol,
  TransportProtocol,
  VMessSecurity,
  XtlsFlow,
} from "./enums";

import type { TrojanServer, VlessUser } from "./protocols";

import type { HysteriaObfs, StreamSettings } from "./transports";

export interface BlackHoleResponse {
  /**
   * If type equal "none"(the default value), Blackhole will simply close the connection. If type equal
   * to "http", Blackhole will return a simple HTTP 403 packet and then close the connection.
   */
  type?: HeadersType;
}

export type BlackHoleSettings = OutboundBlackHoleSettings;

export type FreedomSettings = OutboundFreedomSettings;

export interface HysteriaServer extends ClientServer {
  obfs?: HysteriaObfs;
}

export type LoopbackSettings = OutboundLoopbackSettings;

export interface Mux {
  enabled?: boolean;
  concurrency?: number;
  xudpConcurrency?: number;
  xudpProxyUDP443?: string;
}
/**
 * Outgoing connections are used to send data. Available protocols can be found in the Outgoing
 * Protocols (https://xtls.github.io/config/outbounds) section. Docs
 * (https://xtls.github.io/config/outbound.html)
 */
export type Outbound = OutboundConfig;
/** Represents any supported outbound configuration. */
export type OutboundConfig =
  | BlackHoleOutbound
  | DnsOutbound
  | FreedomOutbound
  | HttpOutbound
  | HysteriaOutbound
  | LoopbackOutbound
  | ShadowSocksOutbound
  | SocksOutbound
  | TrojanOutbound
  | VlessOutbound
  | VMessOutbound
  | WireguardOutbound
  | GenericOutbound;

/**
 * Outgoing connections are used to send data. Available protocols can be found in the Outgoing
 * Protocols (https://xtls.github.io/config/outbounds) section. Docs
 * (https://xtls.github.io/config/outbound.html)
 */
export interface OutboundBase {
  /**
   * The IP address used to send data. This parameter is used if the host is configured with multiple IP
   * addresses. The default value is "0.0.0.0".
   */
  sendThrough?: string;
  /** The tag of this outgoing connection, used to identify this connection in other settings. */
  tag?: string;
  /**
   * The name of the connection protocol. For a list of available protocols, see the Outgoing Protocols
   * (https://xtls.github.io/config/outbounds) section in the left-hand menu.
   */
  protocol: Protocol;
  /** Transport type is the way the current Xray node interacts with other nodes. */
  streamSettings?: StreamSettings;
  /** Outbound proxy configuration. */
  proxySettings?: ProxySettings;
  /**
   * Mux settings. Mux allows you to multiplex multiple TCP connections over a single TCP connection. Mux
   * has an additional feature: transmitting UDP connections as XUDP.
   */
  mux?: Mux;
  /**
   * When an outgoing connection sends a request to a domain name, this option controls whether and how
   * it will be resolved to the outgoing IP address. The default value is AsIs, meaning sending to the
   * remote server "as is."
   */
  targetStrategy?: DomainStrategy;
}

export interface OutboundBlackHoleSettings {
  /** Configuring Blackhole Response. */
  response?: BlackHoleResponse;
}

export interface OutboundDnsSettings {
  /**
   * Changes the transport protocol for DNS traffic. Valid values ​​are "tcp"and "udp". If not specified,
   * the original transport protocol is used.
   */
  network?: TransportProtocol;
  /** Changes the DNS server address. If not specified, the address specified in the source is used. */
  address?: string;
  /** Changes the DNS server port. If not specified, the port specified in the source is used. */
  port?: number;
  /** Controls requests that are not for IP addresses (not A or AAAA). The default value is "reject". */
  nonIPQuery?: NonIPQueryType;
  /**
   * An array of integers ( int) specifying the DNS request types to block. For example, [ ]
   * "blockTypes": [65,28] means blocking type 65 (HTTPS) and 28 (AAAA). A common use case is blocking
   * type 65 to prevent browsers from initializing ECH.
   */
  blockTypes?: string[];
}

export interface OutboundFreedomSettings extends WithUserLevel {
  /** All parameters are similar in meaning domainStrategy to sockopt. The default value is "AsIs". */
  domainStrategy?: DomainStrategy;
  /**
   * Freedom will force all data to be sent to the specified address (not the address specified in the
   * incoming connection). The value is a string, for example: "127.0.0.1:80", ":1234".
   */
  redirect?: string;
  /**
   * Several key-value pairs used to control outgoing TCP fragmentation. In some cases, this can
   * circumvent censorship systems, such as SNI blacklists. "length" These "interval"are of the
   * Int32Range type. packets: two fragmentation modes are supported: "1-3" - TCP stream fragmentation,
   * applied to the first three client-side data write operations; "tlshello" - TLS handshake packet
   * fragmentation. length: fragment length (in bytes). interval: interval between fragments (in ms). If
   * the value is equal to 0and is set "packets": "tlshello", a fragmented Client Hello packet will be
   * sent in a single TCP packet (unless its original size exceeds the MSS or MTU, which causes automatic
   * fragmentation by the system).
   */
  fragment?: Record<string, string>;
  /**
   * UDP noise, used to send random data as "noise" before establishing a UDP connection. The presence of
   * this structure is considered enabled. It can fool sniffers, but it can also disrupt a normal
   * connection. Use at your own risk. For this reason, it bypasses port 53, as this disrupts DNS.
   */
  noises?: Array<Record<string, string>>;
  /**
   * The PROXY protocol is typically used in conjunction with redirect to redirect to an Nginx server or
   * another server with the PROXY protocol enabled. If the server doesn't support the PROXY protocol,
   * the connection will be terminated. proxyProtocol takes the value of the PROXY protocol version
   * number - 1or 2. If not specified, the default value is 0(the protocol is not used).
   */
  proxyProtocol?: number;
}

export interface OutboundHttpSettings extends WithLevel {
  /** HTTP proxy server address, required. */
  address: string;
  /** HTTP proxy server port, required. */
  port: number;
  /**
   * Username, string type. Required if authentication is required to connect to the server; otherwise,
   * leave this option unchecked.
   */
  user?: string;
  /**
   * Password, string type. Required if authentication is required to connect to the server; otherwise,
   * leave this option unchecked.
   */
  pass?: string;
  /**
   * The email address used to identify the user. This is optional if authentication is required to
   * connect to the server. Otherwise, leave this option unchecked.
   */
  email?: string;
  /**
   * HTTP headers, which are key-value pairs. Each key represents the name of an HTTP header, and all
   * key-value pairs will be attached to every request.
   */
  headers?: Record<string, string>;
}

export interface OutboundHysteriaSettings {
  /** Version of protocol */
  version?: number;
  /** Hysteria2 proxy server address. */
  address?: string;
  /** Hysteria2 proxy server port. */
  port?: number;
  servers?: HysteriaServer[];
}

export interface OutboundLoopbackSettings {
  /** The incoming protocol identifier used for rerouting. */
  inboundTag: string;
}

export interface OutboundShadowSocksSettings extends WithLevel {
  /** Email address, optional, used to identify the user. */
  email?: string;
  /** Shadowsocks server address; IPv4, IPv6, and domain names are supported. Required. */
  address?: string;
  /** Shadowsocks server port. */
  port?: number;
  /** Required parameter. */
  method?: EncryptionMethod;
  /** Required parameter. */
  password?: string;
  /** Turn on udp over tcp. */
  uot?: boolean;
  /** Implementation version UDP over TCP. Valid values: 1, 2. */
  UoTVersion?: number;
  servers?: ShadowSocksServer[];
}

export interface OutboundSocksSettings extends WithLevel {
  /** Server address, required parameter. */
  address: string;
  /** Server port, required parameter. */
  port: number;
  /** Username, data type: string */
  user?: string;
  /** Password, data type: string. */
  pass?: string;
  /** The email address used to identify the user. */
  email?: string;
}

export interface OutboundTrojanSettings extends WithLevel {
  /** Server address; IPv4, IPv6, and domain names are supported. */
  address?: string;
  /** The server port, usually the same as the port the server is listening on. */
  port?: number;
  /** Password. */
  password?: string;
  /** Email address, optional, used to identify the user. */
  email?: string;
  servers?: TrojanServer[];
}

export interface OutboundVlessSettings extends WithLevel {
  /** Server address pointing to the server, domain names, IPv4 and IPv6 are supported. */
  address?: string;
  /** The server port, usually the same as the port the server is listening on. */
  port?: number;
  /** The VLESS user identifier can be any string less than 30 bytes long or a valid UUID. */
  id?: string;
  /** VLESS encryption settings . This cannot be empty; to disable it, you must explicitly set the value "none". */
  encryption?: string;
  /** Flow control mode, used to select the XTLS algorithm. */
  flow?: XtlsFlow;
  /**
   * A simplified configuration for the VLESS reverse proxy. It serves the same purpose as the built-in
   * universal reverse proxy, but with a simpler setup.
   */
  reverse?: VlessReverse;
  /** Client only. External server endpoints. */
  vnext?: VlessVNext[];
}

export interface OutboundVMessSettings extends WithLevel {
  /** Server address, IP address or domain name supported. */
  address?: string;
  /** The port number the server listens on is a required parameter. */
  port?: number;
  /** The VMess user ID, can be any string less than 30 bytes long or a valid UUID. */
  id?: string;
  /**
   * Encryption method. The client will send data using the configured encryption method; the server will
   * automatically detect it; no configuration is required on the server.
   */
  security?: VMessSecurity;
  alterId?: number;
  /**
   * Enabled experimental features of the VMess protocol. (These features are unstable and may be removed
   * at any time.) Multiple enabled experiments can be separated by the | symbol, for example,
   * "AuthenticatedLength|NoTerminationSignal". AuthenticatedLength - enables the authenticated packet
   * length experiment. This experiment must be enabled on both the client and server, and the same
   * version of the program must be running. NoTerminationSignal - enables an experiment with disabling
   * the connection termination signal. This experiment may impact the stability of the proxied
   * connection.
   */
  experiments?: string;
  vnext?: VMessVNext[];
}

export interface OutboundWireguardSettings {
  /** User's personal key. Required field. */
  secretKey: string;
  /**
   * Wireguard launches a local virtual network interface (tun). It supports one or more IP addresses,
   * including IPv6.
   */
  address?: string[];
  /** A list of Wireguard servers. Each entry represents the configuration of one server. */
  peers?: WireguardOutboundPeer[];
  /**
   * By default, the system checks whether it's running Linux and whether the user has CAP_NET_ADMIN
   * privileges to decide whether to use the system virtual interface. If it's not used, gvisor is used.
   * The system virtual interface provides better performance. Please note that this only applies to IP
   * packet processing and is not related to the Wireguard kernel.
   */
  noKernelTun?: boolean;
  /** MTU of the lower level tun in Wireguard. */
  mtu?: number;
  /** Wireguard reserved bytes, filled as needed. */
  reserved?: number[];
  /** The number of Wireguard threads. The default value is equal to the number of processor cores. */
  workers?: number;
  /**
   * Unlike most proxy protocols, Wireguard does not allow domain names to be passed as targets. If a
   * domain name is passed as a target, it is resolved to an IP address via Xray's built-in DNS. See the
   * domainStrategyoutbound field for Freedomdetails. The default is ForceIP.
   */
  domainStrategy?: DomainStrategy;
}
/**
 * Blackhole is an outbound protocol that blocks all outgoing data. When combined with routing
 * configuration, it can be used to deny access to specific websites.
 */
export interface BlackHoleOutbound extends OutboundBase {
  protocol: typeof Protocol.BlackHole;
  settings?: OutboundBlackHoleSettings;
}
/**
 * DNS is an outbound protocol that is primarily used to intercept and forward DNS queries. This
 * outgoing protocol can only accept DNS traffic (including UDP and TCP queries), other types of
 * traffic will cause an error.
 */
export interface DnsOutbound extends OutboundBase {
  protocol: typeof Protocol.Dns;
  settings?: OutboundDnsSettings;
}
/** Freedom is an outgoing protocol that can be used to send (plain) TCP or UDP data to any network. */
export interface FreedomOutbound extends OutboundBase {
  protocol: typeof Protocol.Freedom;
  settings?: OutboundFreedomSettings;
}

export interface GenericOutbound extends OutboundBase {
  settings?: unknown;
}
/** HTTP protocol. */
export interface HttpOutbound extends OutboundBase {
  protocol: typeof Protocol.Http;
  settings?: OutboundHttpSettings;
}

export interface HysteriaOutbound extends OutboundBase {
  protocol: typeof Protocol.Hysteria;
  settings?: OutboundHysteriaSettings;
}

export interface LoopbackOutbound extends OutboundBase {
  protocol: typeof Protocol.Loopback;
  settings?: OutboundLoopbackSettings;
}

export interface ShadowSocksOutbound extends OutboundBase {
  protocol: typeof Protocol.ShadowSocks;
  settings?: OutboundShadowSocksSettings;
}

export interface SocksOutbound extends OutboundBase {
  protocol: typeof Protocol.Socks;
  settings?: OutboundSocksSettings;
}

export interface TrojanOutbound extends OutboundBase {
  protocol: typeof Protocol.Trojan;
  settings?: OutboundTrojanSettings;
}

export interface VlessOutbound extends OutboundBase {
  protocol: typeof Protocol.Vless;
  settings?: OutboundVlessSettings;
}

export interface VMessOutbound extends OutboundBase {
  protocol: typeof Protocol.VMess;
  settings?: OutboundVMessSettings;
}

export interface WireguardOutbound extends OutboundBase {
  protocol: typeof Protocol.Wireguard;
  settings?: OutboundWireguardSettings;
}

export interface WireguardOutboundPeer {
  /** Server address. */
  endpoint?: string;
  /** The server's public key for verification. */
  publicKey?: string;
  /** Additional symmetric encryption key. */
  preSharedKey?: string;
  /** Heartbeat packet sending interval, in seconds. The default value is 0 (no heartbeat). */
  keepAlive?: number;
  /** Wireguard only allows traffic from certain IP addresses. */
  allowedIPs?: string[];
}

/** Outbound proxy configuration. */
export interface ProxySettings {
  /**
   * If another Outbound tag is specified, data originating from that Outbound will be redirected through
   * the specified Outbound.
   */
  tag?: string;
  /**
   * trueConverts this setting to SockOpt.dialerProxy support transport-level redirection. Default
   * false(no conversion).
   */
  transportLayer?: boolean;
}

export interface ShadowSocksServer extends ClientServer {
  /** Required parameter. */
  method?: EncryptionMethod;
}

export interface VlessReverse {
  /**
   * This is the incoming proxy tag for this reverse proxy. When the server sends a request to the
   * reverse proxy, it will enter the routing system through the incoming connection with this tag. Use
   * the routing system to route it to the desired outgoing connection.
   */
  tag?: string;
}

export interface VlessVNext extends VNextModel<VlessUser> {}

export interface VMessDefaultSettings extends WithLevel {}

export interface VMessUser extends WithLevel {
  id?: string;
  email?: string;
  security?: VMessSecurity;
  alterId?: number;
}

export interface VMessVNext extends VNextModel<VMessUser> {}

export interface WireguardPeer {
  /** Public key for verification. */
  publicKey?: string;
  /** Allowed source IP addresses. */
  allowedIPs?: string[];
}
