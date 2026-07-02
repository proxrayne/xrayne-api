import type { EnumValue } from "./common";

export const AddressPortStrategy = {
  None: "none",
  SrvPortOnly: "SrvPortOnly",
  SrvAddressOnly: "SrvAddressOnly",
  SrvPortAndAddress: "SrvPortAndAddress",
  TxtPortOnly: "TxtPortOnly",
  TxtAddressOnly: "TxtAddressOnly",
  TxtPortAndAddress: "TxtPortAndAddress",
} as const;
export type AddressPortStrategy = EnumValue<typeof AddressPortStrategy>;

export const AllocateStrategy = {
  Always: "always",
  Random: "random",
} as const;
export type AllocateStrategy = EnumValue<typeof AllocateStrategy>;

export const ApiServices = {
  Handler: "HandlerService",
  Logger: "LoggerService",
  Stats: "StatsService",
  Routing: "RoutingService",
  Reflection: "ReflectionService",
} as const;
export type ApiServices = EnumValue<typeof ApiServices>;

export const BalancerStrategyType = {
  Random: "random",
  RoundRobin: "roundRobin",
  LeastPing: "leastPing",
  LeastLoad: "leastLoad",
} as const;
export type BalancerStrategyType = EnumValue<typeof BalancerStrategyType>;

export const CertificateUsageType = {
  Encipherment: "encipherment",
  Verify: "verify",
  Issue: "issue",
} as const;
export type CertificateUsageType = EnumValue<typeof CertificateUsageType>;

export const DnsQueryStrategy = {
  UseIP: "UseIP",
  UseIPv4: "UseIPv4",
  UseIPv6: "UseIPv6",
  UseSystem: "UseSystem",
} as const;
export type DnsQueryStrategy = EnumValue<typeof DnsQueryStrategy>;

export const DomainMatcher = {
  Hybrid: "hybrid",
  Linear: "linear",
} as const;
export type DomainMatcher = EnumValue<typeof DomainMatcher>;

export const DomainStrategy = {
  AsIs: "AsIs",
  UseIP: "UseIP",
  UseIPv6v4: "UseIPv6v4",
  UseIPv6: "UseIPv6",
  UseIPv4v6: "UseIPv4v6",
  UseIPv4: "UseIPv4",
  ForceIP: "ForceIP",
  ForceIPv6v4: "ForceIPv6v4",
  ForceIPv6: "ForceIPv6",
  ForceIPv4v6: "ForceIPv4v6",
  ForceIPv4: "ForceIPv4",
} as const;
export type DomainStrategy = EnumValue<typeof DomainStrategy>;

export const EncryptionMethod = {
  None: "none",
  Chacha20Poly1305: "chacha20-poly1305",
  XChacha20Poly1305: "xchacha20-poly1305",
  Blake3Chacha20Poly1305: "2022-blake3-chacha20-poly1305",
  Aes128Gcm: "aes-128-gcm",
  Aes256Gcm: "aes-256-gcm",
  Blake3Aes128Gcm: "2022-blake3-aes-128-gcm",
  Blake3Aes256Gcm: "2022-blake3-aes-256-gcm",
} as const;
export type EncryptionMethod = EnumValue<typeof EncryptionMethod>;

export const Fingerprint = {
  None: "",
  Chrome: "chrome",
  Firefox: "firefox",
  Safari: "safari",
  iOS: "ios",
  Android: "android",
  Edge: "edge",
  e360: "360",
  Qq: "qq",
  Unsafe: "unsafe",
  Random: "random",
  Randomized: "randomized",
} as const;
export type Fingerprint = EnumValue<typeof Fingerprint>;

export const HeadersType = {
  None: "none",
  Http: "http",
} as const;
export type HeadersType = EnumValue<typeof HeadersType>;

export const HysteriaMasqueradeType = {
  File: "file",
  Proxy: "proxy",
  String: "string",
} as const;
export type HysteriaMasqueradeType = EnumValue<typeof HysteriaMasqueradeType>;

export const KcpHeaderType = {
  None: "none",
  Srtp: "srtp",
  Utp: "utp",
  WechatVideo: "wechat-video",
  Dtls: "dtls",
  Wireguard: "wireguard",
  Dns: "dns",
} as const;
export type KcpHeaderType = EnumValue<typeof KcpHeaderType>;

export const LogLevel = {
  None: "none",
  Debug: "debug",
  Info: "info",
  Warning: "warning",
  Error: "error",
} as const;
export type LogLevel = EnumValue<typeof LogLevel>;

export const NetProtocol = {
  Http: "http",
  Tls: "tls",
  Bittorrent: "bittorrent",
} as const;
export type NetProtocol = EnumValue<typeof NetProtocol>;

export const Network = {
  Tcp: "tcp",
  Udp: "udp",
} as const;
export type Network = EnumValue<typeof Network>;

export const NonIPQueryType = {
  Drop: "drop",
  Skip: "k",
  Reject: "reject",
} as const;
export type NonIPQueryType = EnumValue<typeof NonIPQueryType>;

export const OperationSystem = {
  Linux: "linux",
  Windows: "windows",
  Darwin: "darwin",
} as const;
export type OperationSystem = EnumValue<typeof OperationSystem>;

export const Protocol = {
  Http: "http",
  ShadowSocks: "shadowsocks",
  Socks: "socks",
  Vless: "vless",
  VMess: "vmess",
  Trojan: "trojan",
  Wireguard: "wireguard",
  DokodemoDoor: "dokodemo-door",
  Tun: "tun",
  BlackHole: "blackhole",
  Dns: "dns",
  Freedom: "freedom",
  Loopback: "loopback",
  Hysteria: "hysteria",
} as const;
export type Protocol = EnumValue<typeof Protocol>;

export const RoutingDomainStrategy = {
  AsIs: "AsIs",
  IPIfNonMatch: "IPIfNonMatch",
  IPOnDemand: "IPOnDemand",
} as const;
export type RoutingDomainStrategy = EnumValue<typeof RoutingDomainStrategy>;

export const RoutingRuleType = {
  Field: "field",
} as const;
export type RoutingRuleType = EnumValue<typeof RoutingRuleType>;

export const SocksAuth = {
  NoAuth: "noauth",
  Password: "password",
} as const;
export type SocksAuth = EnumValue<typeof SocksAuth>;

export const StreamNetwork = {
  Raw: "raw",
  Tcp: "tcp",
  XHttp: "xhttp",
  Kcp: "kcp",
  Grpc: "grpc",
  Ws: "ws",
  HttpUpgrade: "httpupgrade",
  Hysteria: "hysteria",
} as const;
export type StreamNetwork = EnumValue<typeof StreamNetwork>;

export const StreamSecurity = {
  None: "none",
  Tls: "tls",
  Reality: "reality",
} as const;
export type StreamSecurity = EnumValue<typeof StreamSecurity>;

export const TProxy = {
  Redirect: "redirect",
  On: "tproxy",
  Off: "off",
} as const;
export type TProxy = EnumValue<typeof TProxy>;

export const TcpCongestion = {
  Bbr: "bbr",
  Cubic: "cubic",
  Reno: "reno",
} as const;
export type TcpCongestion = EnumValue<typeof TcpCongestion>;

export const TrafficType = {
  Http: "http",
  Tls: "tls",
  Quic: "quic",
  Fakedns: "fakedns",
} as const;
export type TrafficType = EnumValue<typeof TrafficType>;

export const TransportProtocol = {
  Tcp: "tcp",
  Udp: "udp",
} as const;
export type TransportProtocol = EnumValue<typeof TransportProtocol>;

export const VlessEncryption = {
  None: "none",
} as const;
export type VlessEncryption = EnumValue<typeof VlessEncryption>;

export const VMessSecurity = {
  None: "none",
  Aes128Gcm: "aes-128-gcm",
  Chacha20Poly1305: "chacha20-poly1305",
  Auto: "auto",
  Zero: "zero",
} as const;
export type VMessSecurity = EnumValue<typeof VMessSecurity>;

export const XHttpMode = {
  Auto: "auto",
  Packet: "packet-up",
  Stream: "stream-up",
} as const;
export type XHttpMode = EnumValue<typeof XHttpMode>;

export const XrayMergeCollectionMode = {
  Replace: "Replace",
  Append: "Append",
  MergeByKey: "MergeByKey",
} as const;
export type XrayMergeCollectionMode = EnumValue<typeof XrayMergeCollectionMode>;

export const XtlsFlow = {
  None: "",
  XtlsRprxVision: "xtls-rprx-vision",
  XtlsRprxVisionUdp443: "xtls-rprx-vision-udp443",
} as const;
export type XtlsFlow = EnumValue<typeof XtlsFlow>;
