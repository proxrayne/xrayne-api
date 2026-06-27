import type { HeaderMap, JsonValue } from "./common";

import type { AddressPortStrategy, CertificateUsageType, DomainStrategy, Fingerprint, HeadersType, HysteriaMasqueradeType, KcpHeaderType, OperationSystem, StreamNetwork, StreamSecurity, TProxy, TcpCongestion, XHttpMode } from "./enums";

import type { InboundHttpSettings } from "./inbounds";

import type { OutboundHttpSettings } from "./outbounds";

export type BaseSettingsHeaders = NoneSettingsHeaders | HttpSettingsHeaders;

export interface CustomSockopt {
  /**
   * An optional field. Specifies the operating system for which this option will be applied. If the
   * current operating system does not match the specified one, this option (sockopt) will be skipped.
   */
  system?: OperationSystem;
  /** Required parameter. Setting type. Acceptable values: int or str. */
  type: string;
  /** Optional parameter. The protocol level that determines the scope. Default: 6 (TCP). */
  level?: string;
  /**
   * The name of the option to set. Uses decimal notation (in the example, the TCP_CONGESTION value
   * defined as 0xd is converted to decimal 13).
   */
  opt?: string;
  /**
   * The value to set for the option. In the example, the value is set to bbr. If type specified as int,
   * the value must be a decimal number.
   */
  value?: string;
}

/** One layer in the FinalMask chain. Specific settings depend on the type and are preserved as raw JSON. */
export interface FinalMaskLayer {
  type?: string;
  settings?: JsonValue;
}

/** FinalMask configuration used for traffic obfuscation and QUIC tuning. */
export interface FinalMaskSettings {
  tcp?: FinalMaskLayer[];
  udp?: FinalMaskLayer[];
  quicParams?: QuicParamsSettings;
}

/** gRPC configuration for the current connection, valid only if this connection uses gRPC. */
export interface GrpcSettings {
  /** A string that can be used as Host for some other purpose. */
  authority?: string;
  /**
   * A string specifying the service name, similar to a path in HTTP/2. The client will use this name to
   * communicate, and the server will check whether the service name matches.
   */
  serviceName?: string;
  /** [BETA] true includes multiMode, default value: false. */
  multiMode?: boolean;
  /** Setting a gRPC user agent can prevent gRPC traffic from being blocked by some CDNs. */
  user_agent?: string;
  /**
   * A health check is performed if no data is transmitted for a specified period of time, measured in
   * seconds. If this value is less than 10, then will be used as the minimum value 10.
   */
  idle_timeout?: number;
  /**
   * The health check response timeout in seconds. If the health check is not completed within this time
   * and there is still no data transfer, the health check will be considered a failure. Default value:
   * 20.
   */
  health_check_timeout?: number;
  /** true enables health checking if there are no child connections. Default value: false. */
  permit_without_stream?: boolean;
  /**
   * The initial h2 Stream window size. If the value is less than or equal to 0, this feature has no
   * effect. If the value is greater than 65535, the dynamic window mechanism is disabled. The default
   * value is 0, meaning it has no effect.
   */
  initial_windows_size?: number;
}

/**
 * Happy Eyeballs implementation (RFC-8305), applicable only to TCP. When the target is a domain, a
 * connection race is performed between the resulting IP addresses, and the first successful result is
 * chosen.
 */
export interface HappyEyeballs {
  /**
   * The time interval between each "race" request, in milliseconds. The default is 0 (meaning the
   * feature is disabled); the recommended value is 250.
   */
  tryDelayMs?: number;
  /**
   * The type of the first IP address when sorting IP addresses. This is the default false(meaning IPv4
   * will be first).
   */
  prioritizeIPv6?: boolean;
  /**
   * "First Address Family count" from RFC-8305, default value is 1. This parameter specifies the
   * alternating order in which IP addresses of different versions are sorted. For example, the IP
   * address queue for dialing would be sorted as 46464646 (if the value is 1) or 44664466 (if the value
   * is 2) (where 6 is the IPv6 address and 4 is the IPv4 address).
   */
  interleave?: number;
  /**
   * Maximum number of simultaneous attempts. This is used to prevent the kernel from creating a large
   * number of connections if many IP addresses are allowed and none of the connections are successful.
   * The default is 4; setting this value to 0 disables happyEyeballs.
   */
  maxConcurrentTry?: number;
}

/** Raw HTTP request model. */
export interface HttpRequest {
  /** HTTP version, default value is "1.1". */
  version?: string;
  /** HTTP method, default value is "GET". */
  method?: string;
  /**
   * Path, array of strings. The default value is ["/"]. If there are multiple values, one is randomly
   * selected for each request.
   */
  path?: string[];
  /**
   * HTTP headers, key-value pairs, where each key represents the name of an HTTP header and the
   * corresponding value is an array.
   */
  headers?: HeaderMap;
}

/** Raw HTTP response model. */
export interface HttpResponse {
  /** HTTP version, default value is "1.1". */
  version?: string;
  /** HTTP status, default value is "200". */
  status?: string;
  /** HTTP status description, default value is "OK". */
  reason?: string;
  /**
   * HTTP headers, key-value pairs, where each key represents the name of an HTTP header and the
   * corresponding value is an array.
   */
  headers?: HeaderMap;
}

/**
 * The HTTP cloaking configuration must be the same on both the incoming and outgoing connection, and
 * its contents must match.
 */
export interface HttpSettingsHeaders {
  type: typeof HeadersType.Http;
  /** HTTP request. */
  request?: HttpRequest;
  /** HTTP response. */
  response?: HttpResponse;
}

export type HttpSettings = InboundHttpSettings | OutboundHttpSettings;

/** HTTPUpgrade configuration for the current connection, valid only if this connection uses HTTPUpgrade. */
export interface HttpUpgradeSettings {
  /** Used only for incoming connections and specifies whether to accept the PROXY protocol. */
  acceptProxyProtocol?: boolean;
  /** The HTTP path used by HTTPUpgrade, by default "/". */
  path?: string;
  /**
   * The host sent in the HTTPUpgrade HTTP request is empty by default. If the server-side value is
   * empty, the host value sent by the client is not validated.
   */
  host?: string;
  /**
   * Custom HTTP headers, a key-value pair where each key represents the name of an HTTP header and the
   * corresponding value is a string. Empty by default.
   */
  headers?: HeaderMap;
}

export interface HysteriaMasquerade {
  /** One of file, proxy or string. */
  type?: HysteriaMasqueradeType;
  /** Directory path for file masquerading. */
  dir?: string;
  /** Upstream URL for proxy masquerading. */
  url?: string;
  /** Whether to rewrite Host when proxy masquerading is used. */
  rewriteHost?: boolean;
  /** Whether to skip TLS verification for proxy masquerading. */
  insecure?: boolean;
  /** Inline content for string masquerading. */
  content?: string;
  /** Headers for string masquerading. */
  headers?: Record<string, string>;
  /** Status code for string masquerading. */
  statusCode?: number;
}

export interface HysteriaObfs {
  /** Obfuscation type. */
  type?: string;
  /** Obfuscation password. */
  password?: string;
}

export interface HysteriaSettingsLegacyQuic {
  /**
   * Upload speed limit. Default value is 0. The format is user-friendly and supports various common bits
   * per second notations, including 1000000, 100kb, 20 mb, 100 mbps, 1g, 1 tbps and so on. Case is not
   * significant, and spaces between units are optional. Without units, bps (bits per second) is used by
   * default; the value cannot be lower than 65535 bps.
   */
  up?: string;
  /**
   * Download speed limit. Default value is 0. The format is user-friendly and supports various common
   * bits per second notations, including 1000000, 100kb, 20 mb, 100 mbps, 1g, 1 tbps and so on. Case is
   * not significant, and spaces between units are optional. Without units, bps (bits per second) is used
   * by default; the value cannot be lower than 65535 bps.
   */
  down?: string;
  /** Configuration of UDP port hopping. */
  udphop?: HysteriaUDPhop;
  initStreamReceiveWindow?: number;
  maxStreamReceiveWindow?: number;
  initConnectionReceiveWindow?: number;
  maxConnectionReceiveWindow?: number;
  /**
   * Maximum idle timeout (in seconds). How long the server will wait before closing the connection if it
   * has not received any data from the client. Range: 4~120 seconds, default: 30 seconds.
   */
  maxIdleTimeout?: number;
  /** QUIC KeepAlive interval (in seconds). Range: 2~60 seconds. Disabled by default. */
  keepAlivePeriod?: number;
  /** Whether to disable Path MTU Discovery. */
  disablePathMTUDiscovery?: boolean;
}

/**
 * A low-level QUIC transport implementation for Hysteria2 in Xray. Typically used in conjunction with
 * the hysteria2 outbound protocol (https://xtls.github.io/config/outbounds/hysteria.html).
 */
export interface HysteriaSettings {
  /** Version of protocol */
  version?: number;
  /** Hysteria authentication password must match on both server and client. */
  auth?: string;
  /** Idle wait time for a single QUIC native UDP connection in seconds. */
  udpIdleTimeout?: number;
  /** HTTP/3 masquerading settings. */
  masquerade?: HysteriaMasquerade;
  /** Legacy upload speed limit. Newer configs use finalmask.quicParams.brutalUp. */
  up?: string;
  /** Legacy download speed limit. Newer configs use finalmask.quicParams.brutalDown. */
  down?: string;
  /** Legacy UDP port hopping settings. Newer configs use finalmask.quicParams.udpHop. */
  udphop?: HysteriaUDPhop;
  /** Legacy QUIC window setting. Newer configs use finalmask.quicParams. */
  initStreamReceiveWindow?: number;
  /** Legacy QUIC window setting. Newer configs use finalmask.quicParams. */
  maxStreamReceiveWindow?: number;
  /** Legacy QUIC window setting. Newer configs use finalmask.quicParams. */
  initConnectionReceiveWindow?: number;
  /** Legacy QUIC window setting. Newer configs use finalmask.quicParams. */
  maxConnectionReceiveWindow?: number;
  /** Legacy maximum idle timeout (seconds). Newer configs use finalmask.quicParams.maxIdleTimeout. */
  maxIdleTimeout?: number;
  /** Legacy QUIC keepalive interval (seconds). Newer configs use finalmask.quicParams.keepAlivePeriod. */
  keepAlivePeriod?: number;
  /** Legacy PMTU toggle. Newer configs use finalmask.quicParams.disablePathMTUDiscovery. */
  disablePathMTUDiscovery?: boolean;
}

export interface HysteriaUDPhop {
  /** Port range to hop to. */
  port: string;
  /** Port hopping interval in seconds. */
  interval: number;
}

export interface KCPHeaders {
  /** Camouflage type. */
  type?: KcpHeaderType;
  /** Used in conjunction with the masking type "dns", you can specify an arbitrary domain. */
  domain?: string;
}

/** mKCP configuration for the current connection, valid only if this connection uses mKCP. */
export interface KcpSettings {
  /** Maximum transmission unit. Select a value between 576 and 1460. By default 1350. */
  mtu?: number;
  /**
   * Transmission time interval, in milliseconds (ms), mKCP will send data at this rate. Select a value
   * between 10 and 100. By default 50.
   */
  tti?: number;
  /**
   * The sending channel throughput, i.e. the maximum bandwidth used by the host to send data, in MB/s
   * (note these are bytes, not bits). Can be set to 0, meaning very little throughput. By default 5
   */
  uplinkCapacity?: number;
  /**
   * The receive channel throughput, i.e. the maximum bandwidth used by the host to receive data, in MB/s
   * (note that these are bytes, not bits). Can be set to 0, meaning very little throughput. By default
   * 20.
   */
  downlinkCapacity?: number;
  /** Enable or disable overload control. By default false. */
  congestion?: boolean;
  /** The read buffer size for a single connection, in MB. By default 2. */
  readBufferSize?: number;
  /** The write buffer size for a single connection, in MB. By default 2. */
  writeBufferSize?: number;
  /** Configuring data header masking */
  header?: KCPHeaders;
  /**
   * Optional password encryption used to encrypt the data stream using the AES-128-GCM algorithm. The
   * client and server must use the same password.
   */
  seed?: string;
}

/** No masking is performed. */
export interface NoneSettingsHeaders {
  type: typeof HeadersType.None;
}

/** QUIC tuning options used by XHTTP H3 and Hysteria. */
export interface QuicParamsSettings {
  congestion?: string;
  debug?: boolean;
  brutalUp?: string;
  brutalDown?: string;
  udpHop?: QuicUdpHopSettings;
  initStreamReceiveWindow?: number;
  maxStreamReceiveWindow?: number;
  initConnectionReceiveWindow?: number;
  maxConnectionReceiveWindow?: number;
  maxIdleTimeout?: number;
  keepAlivePeriod?: number;
  disablePathMTUDiscovery?: boolean;
  maxIncomingStreams?: number;
}

export interface QuicUdpHopSettings {
  ports?: string;
  interval?: string;
}

/**
 * Renamed from the TCP transport layer (the original name was ambiguous), the outgoing RAW transport
 * layer sends TCP and UDP data generated by proxy protocol wrappers directly, and the kernel does not
 * use other transport layers (such as XHTTP) to transmit its traffic.
 */
export interface RawSettings {
  /** For incoming connections only, specifies whether to accept the PROXY protocol. */
  acceptProxyProtocol?: boolean;
  /** Data packet header masking settings, default value: NoneHeaderObject. */
  header?: BaseSettingsHeaders;
}

/**
 * Reality Configuration. Reality is Xray's original technology. Reality provides a higher level of
 * security than TLS and is configured in the same way as TLS.
 */
export interface RealitySettings {
  /** If the value is true, print debugging information. */
  show?: boolean;
  /** Required parameter, format is the same as dest in VLESS fallbacks. */
  target?: unknown;
  dest?: unknown;
  /**
   * An optional parameter, the format is the same as xver
   * (https://xtls.github.io/config/features/fallback.html#fallbackobject) in VLESS fallbacks.
   */
  xver?: number;
  /** Required parameter, list of those available serverName to the client, wildcards * are not supported yet. */
  serverNames?: string[];
  /** A required parameter, generated using the command ./xray x25519. */
  privateKey?: string;
  /** Optional parameter, minimum version of Xray client, format: x.y.z. */
  minClientVer?: string;
  /** Optional parameter, maximum version of Xray client, format: x.y.z. */
  maxClientVer?: string;
  /** An optional parameter, the maximum allowed time difference in milliseconds. */
  maxTimeDiff?: number;
  /**
   * A mandatory parameter, the list of available shortId for the client, can be used to distinguish
   * between different clients. For format requirements, see shortId. If it contains an empty value,
   * shortId the client may be empty.
   */
  shortIds?: string[];
  /**
   * For server use only. A private key used to add an additional post-quantum signature using the
   * ML-DSA-65 scheme to the certificate issued to the Reality client.
   */
  mldsa65Seed?: string;
  /**
   * Optional parameter. Speed ​​limit for backup REALITY connections. The limit takes effect after the
   * specified number of bytes have been transferred. Defaults to 0.
   */
  afterBytes?: number;
  /**
   * Optional parameter. Rate limit for REALITY backup connections. Specifies the base rate
   * (bytes/second). The default is 0, which disables rate limiting.
   */
  bytesPerSec?: number;
  /**
   * Optional parameter. Rate limit for backup REALITY connections. Specifies the burst rate
   * (bytes/second). Effective when the value is greater than bytesPerSec.
   */
  burstBytesPerSec?: number;
  /**
   * One of serverNames the servers. If serverNames the server contains an empty value, then, as with
   * TLS, the client can use it "serverName": "0.0.0.0" to establish a connection without SNI. Unlike
   * TLS, REALITY does not require or have an option to allow insecure connections for this feature. When
   * using this feature, ensure that dest you return the default certificate when accepting connections
   * without SNI.
   */
  serverName?: string;
  /** A required parameter, the same as in TLSObject . */
  fingerprint?: Fingerprint;
  /**
   * Required parameter: public key corresponding to the server's private key. Generated by the command
   * ./xray x25519 -i "server secret key".
   */
  password?: string;
  publicKey?: string;
  /**
   * Optional parameter. The public key for verifying the ML-DSA-65 signature. If this field is not
   * empty, the client will use the specified key to validate the certificate returned by the server. For
   * details, see the parameter description "mldsa65Seed".
   */
  mldsa65Verify?: string;
  /**
   * One of shortIds the servers. Length is 8 bytes, that is 16 hexadecimal digits (0-f), it can be less
   * than 16, the kernel will automatically add 0 to the end, but the number of digits must be even
   * (because one byte consists of 2 hexadecimal digits). 0 is also an even number, so if shordIds the
   * server contains an empty value "", the client may also be empty.
   */
  shortId?: string;
  /** The initial path and parameters for the crawler, it is recommended to use different ones for each client. */
  spiderX?: string;
}

export interface Sockopt {
  /** An integer. If the value is nonzero, the outgoing connection is marked with this value using SO_MARK. */
  mark?: number;
  /** Used to set the maximum TCP packet segment (Maximum Segment Size). */
  tcpMaxSeg?: number;
  /**
   * Enable TCP Fast Open (https://en.wikipedia.org/wiki/TCP_Fast_Open). If the value is equal to true or
   * a positive integer , TFO is enabled; if the value is equal to false or a negative number , TFO is
   * forcibly disabled; if the parameter is absent or equal to 0, the system default settings are used.
   * Can be used for both incoming and outgoing connections.
   */
  tcpFastOpen?: boolean;
  /** If the target address is a domain name, you can configure the corresponding value. Default value: "AsIs". */
  domainStrategy?: DomainStrategy;
  /** Whether to enable transparent proxying (Linux only). */
  tproxy?: TProxy;
  /**
   * Happy Eyeballs implementation (RFC-8305), applicable only to TCP. When the target is a domain, a
   * connection race is performed between the resulting IP addresses, and the first successful result is
   * chosen.
   */
  happyEyeballs?: HappyEyeballs;
  /**
   * The outbound proxy identifier. If the value is not empty, the specified outbound proxy will be used
   * to establish the connection. This option can be used to support chained forwarding at the transport
   * level.
   */
  dialerProxy?: string;
  /** For inbound only, specifies whether to accept the PROXY protocol. */
  acceptProxyProtocol?: boolean;
  /**
   * TCP idle timeout threshold in seconds. When a TCP connection's idle timeout reaches this threshold,
   * Keep-Alive packets are sent.
   */
  tcpKeepAliveIdle?: number;
  /**
   * The interval (in seconds) between keep-alive packets sent after the TCP connection enters the
   * Keep-Alive state. The remaining behavior is described above.
   */
  tcpKeepAliveInterval?: number;
  /** In milliseconds. More details (https://github.com/grpc/proposal/blob/master/A18-tcp-user-timeout.md) */
  tcpUserTimeout?: number;
  /**
   * TCP congestion control algorithm. Supported only on Linux. If this parameter is not configured, the
   * system default is used.
   */
  tcpcongestion?: TcpCongestion;
  /** Specifies the network interface name for outgoing traffic. Supported on Linux, iOS, macOS, and Windows. */
  interface?: string;
  /**
   * By default, this setting is set to false. Set it to true to enable Multipath TCP
   * (https://en.wikipedia.org/wiki/Multipath_TCP).
   */
  tcpMptcp?: boolean;
  /** If set to true, the address ::accepts only IPv6 connections. Supported only on Linux. */
  V6Only?: boolean;
  /**
   * This option has been removed because golang enables TCP no delay by default. If you want to disable
   * it, use custom Sockopt.
   */
  tcpNoDelay?: boolean;
  /**
   * The declared window size is limited to this value. The kernel will choose the maximum value between
   * this value and SOCK_MIN_RCVBUF/2.
   */
  tcpWindowClamp?: number;
  /**
   * Use SRV or TXT records to determine the destination address/port for outgoing traffic. Default none
   * (disabled).
   */
  addressPortStrategy?: AddressPortStrategy;
  /**
   * An array allowing advanced users to specify any necessary sockopt options. Theoretically, all of the
   * above connection-related settings can be configured here. Currently, Linux, Windows, and Darwin
   * operating systems are supported. The example below is equivalent "tcpcongestion": "bbr"in the
   * kernel.
   */
  customSockopt?: CustomSockopt[];
}

export interface StreamSettings {
  /** The type of transport method used by the connection's data stream, by default "raw". */
  network?: StreamNetwork;
  /** Whether transport layer encryption is enabled. */
  security?: StreamSecurity;
  /**
   * TLS configuration. TLS is provided by Golang, and TLS negotiation typically results in TLS 1.3; DTLS
   * is not supported.
   */
  tlsSettings?: TlsSettings;
  /**
   * Reality Configuration. Reality is Xray's original technology. Reality provides a higher level of
   * security than TLS and is configured in the same way as TLS.
   */
  realitySettings?: RealitySettings;
  /** RAW configuration for the current connection, valid only if that connection uses RAW. */
  rawSettings?: RawSettings;
  tcpSettings?: RawSettings;
  /** XHTTP configuration for the current connection, valid only if this connection uses XHTTP. */
  xhttpSettings?: XHttpSettings;
  /** mKCP configuration for the current connection, valid only if this connection uses mKCP. */
  kcpSettings?: KcpSettings;
  /** gRPC configuration for the current connection, valid only if this connection uses gRPC. */
  grpcSettings?: GrpcSettings;
  /** WebSocket configuration for the current connection, valid only if this connection uses WebSocket. */
  wsSettings?: WSSettings;
  /** HTTPUpgrade configuration for the current connection, valid only if this connection uses HTTPUpgrade. */
  httpupgradeSettings?: HttpUpgradeSettings;
  /** A low-level QUIC transport implementation for Hysteria2 in Xray. */
  hysteriaSettings?: HysteriaSettings;
  /** FinalMask configuration for traffic obfuscation and QUIC tuning. */
  finalmask?: FinalMaskSettings;
  /** Specific settings related to transparent proxying. */
  sockopt?: Sockopt;
}

/** The server certificate will be automatically reloaded every 3600 seconds (that is, every hour). */
export interface TlsCertificate {
  /**
   * OCSP Stapling refresh interval in seconds, defaults to 0. Any non-zero value will enable OCSP
   * Stapling and override the default certificate warm reload time of 3600 seconds (OCSP Stapling is
   * performed during reboot).
   */
  ocspStapling?: number;
  /**
   * Download only once (default false). If set to true, the certificate hot reload and OCSP stapling
   * features will be disabled.
   */
  oneTimeLoading?: boolean;
  /** Certificate usage, default value: "encipherment". */
  usage?: CertificateUsageType;
  /**
   * Only effective when the certificate is in use "issue", if the value is true, the CA certificate will
   * be embedded in the certificate chain when the certificate is issued.
   */
  buildChain?: boolean;
  /** Path to a certificate file, such as one generated using OpenSSL, with a .crt extension. */
  certificateFile?: string;
  /**
   * The path to a key file, such as one generated using OpenSSL, with the .key extension.
   * Password-protected key files are currently not supported.
   */
  keyFile?: string;
  /**
   * An array of strings representing the certificate contents, see the example for the format. Use
   * either certificate or certificateFile.
   */
  certificate?: string[];
  /**
   * An array of strings representing the key's contents; see the example for the format. Use either key
   * or keyFile.
   */
  key?: string[];
}

/**
 * TLS configuration. TLS is provided by Golang, and TLS negotiation typically results in TLS 1.3; DTLS
 * is not supported.
 */
export interface TlsSettings {
  /** Specifies the domain name of the server certificate, useful when connecting via an IP address. */
  serverName?: string;
  /**
   * If the value is true, the server will reject the TLS handshake if the received SNI does not match
   * the certificate's domain name. The default value is false.
   */
  rejectUnknownSni?: boolean;
  /**
   * Client-only. The SNI list used for certificate validation (at least one SAN from the certificate
   * must be in this list). This list overrides the list serverName used for validation and is intended
   * for special purposes, such as domain fronting. Compared to the previous method of changing
   * serverName and enabling allowInsecure, this method is more secure, as it still performs certificate
   * signature verification.
   */
  verifyPeerCertInNames?: string[];
  /**
   * An array of strings specifying the ALPN values ​​specified during the TLS handshake. Default value:
   * ["h2", "http/1.1"].
   */
  alpn?: string[];
  /** This is the minimum acceptable TLS version. */
  minVersion?: string;
  /** This is the maximum allowed TLS version. */
  maxVersion?: string;
  /**
   * Whether to allow insecure connections (client only). Default value: false. If the value is true,
   * Xray will not check the validity of the TLS certificate provided by the remote host.
   */
  allowInsecure?: boolean;
  /** Used to configure a colon-separated list of supported cipher suites. */
  cipherSuites?: string;
  /** A list of certificates, each element of which represents a certificate (fullchain recommended). */
  certificates?: TlsCertificate[];
  /**
   * Whether to disable operating system root certificates. Default value: false. If the value is true,
   * Xray will only use certificates specified in certificates for the TLS handshake. If the value is
   * false, Xray will only use operating system root certificates for the TLS handshake.
   */
  disableSystemRoot?: boolean;
  /**
   * Whether to enable session restore. Disabled by default, it will only work if both the server and
   * client support and have enabled this feature.
   */
  enableSessionResumption?: boolean;
  /** This parameter is used to customize the specified fingerprint TLS Client Hello. Default value chrome. */
  fingerprint?: Fingerprint;
  /**
   * Used to specify the SHA256 hash of a remote server's certificate; hex format, case-insensitive. For
   * example: e8e2d387fdbffeb38e9c9065cf30a97ee23c0e3d32ee6f78ffae40966befccc9. This encoding matches the
   * SHA-256 certificate fingerprint in the Chrome certificate viewer, as well as the Certificate
   * Fingerprints SHA-256 format in crt.sh.
   */
  pinnedPeerCertSha256?: string[];
  /**
   * An array of strings specifying the preferred curves for ECDHE execution during the TLS handshake.
   * The supported curves are listed below (case insensitive): CurveP256 CurveP384 CurveP521 X25519
   * x25519Kyber768Draft00 For example, setting a value
   * "curvePreferences":["x25519Kyber768Draft00"]enables an experimental algorithm. Since this algorithm
   * is still in draft form, this field may change at any time.
   */
  curvePreferences?: string[];
  /**
   * The (Pre)-Master-Secret log file, the path to which is specified here, can be used in Wireshark and
   * other programs to decrypt TLS connections established by Xray.
   */
  masterKeyLog?: string;
  /**
   * Client-only. Set by ECHConfig; if set, the client enables the Encrypted Client Hello. Two formats
   * are supported.
   */
  echConfigList?: string;
  /**
   * Server-only. Enables Encrypted Client Hello on the server side. Create keys with the command xray
   * tls ech --serverName example.com [where example.com] is the SNI that will be exposed externally (you
   * can specify any). The Server Key also contains the ECHConfig; if the client Config is lost, it can
   * be restored with [] xray tls ech -i "you server key".
   */
  echServerKeys?: string;
  /**
   * Controls the policy when using DNS queries for ECH Config, the options available are none(default),
   * half, full.
   */
  echForceQuery?: string;
  /** Configures the socket connection-based settings used when performing DNS queries for records ECH. */
  echSockopt?: Sockopt;
}

/**
 * Transport is the way the current Xray node interacts with other nodes. A transport defines how data
 * is transmitted. Typically, both ends of a network connection must use the same transport. For
 * example, if one end uses WebSocket, the other end must also use WebSocket, otherwise the connection
 * will fail. Docs (https://xtls.github.io/config/transport.html)
 */
export interface TransportConfig extends StreamSettings {}

/** WebSocket configuration for the current connection, valid only if this connection uses WebSocket. */
export interface WSSettings {
  /**
   * For incoming connections only, specifies whether to accept the PROXY protocol. If set true, then
   * after establishing a TCP connection at the lowest level, the requesting party must first send PROXY
   * protocol v1 or v2, otherwise the connection will be closed.
   */
  acceptProxyProtocol?: boolean;
  /** The path used by WebSocket in the HTTP protocol, the default value is "/". */
  path?: string;
  /**
   * The host sent in the WebSocket HTTP request; the default value is empty. If the server-side value is
   * empty, the host value sent by the client is not checked.
   */
  host?: string;
  /**
   * Custom HTTP headers are key-value pairs where each key represents the name of an HTTP header and the
   * corresponding value is a string. Default value: empty.
   */
  headers?: HeaderMap;
  /**
   * Specifies the time interval for sending Ping messages to maintain a connection. If not specified or
   * set to 0, Ping messages are not sent (the current behavior is the default).
   */
  heartbeatPeriod?: number;
}

export interface XHttpDownloadSettings extends StreamSettings {
  address?: string;
  port?: number;
}

export interface XHttpExtraSettings {
  header?: Record<string, string>;
  xPaddingBytes?: string;
  noGRPCHeader?: boolean;
  noSSEHeader?: boolean;
  scMaxEachPostBytes?: number;
  scMinPostsIntervalMs?: number;
  scMaxBufferedPosts?: number;
  scStreamUpServerSecs?: number;
  xmux?: XMux;
  downloadSettings?: XHttpDownloadSettings;
}

/** XHTTP configuration for the current connection, valid only if this connection uses XHTTP. */
export interface XHttpSettings {
  /** Host header: in the HTTP request. */
  host?: string;
  /** The HTTP path that the client uses to send requests. */
  path?: string;
  /** Mode for sending data from the client to the server. Use client-only. */
  mode?: XHttpMode;
  /**
   * The sharing scheme for the original JSON for all parameters except host, path, mode. When extra is
   * present, only these four parameters are valid.
   */
  extra?: XHttpExtraSettings;
  /** Additional HTTP headers. */
  headers?: HeaderMap;
}

export interface XMux {
  maxConcurrency?: number;
  maxConnections?: number;
  cMaxReuseTimes?: number;
  hMaxRequestTimes?: number;
  hMaxReusableSecs?: number;
  hKeepAlivePeriod?: number;
}

