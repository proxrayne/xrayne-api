import type { DnsConfig, FakeDnsConfig } from "./dns";

import type { ApiServices, LogLevel, XrayMergeCollectionMode } from "./enums";

import type { InboundConfig } from "./inbounds";

import type { OutboundConfig } from "./outbounds";

import type { PolicyConfig } from "./policy";

import type { RoutingConfig } from "./routing";

import type { TransportConfig } from "./transports";

/**
 * API interface configuration provides a set of APIs based on gRPC for remote invocation. Docs
 * (https://xtls.github.io/config/api.html)
 */
export interface ApiConfig {
  /** Outbound proxy identifier. */
  tag?: string;
  /**
   * The IP and port that the API service listens on. This is an optional configuration item. When you
   * omit this item, you need to add inbounds and routing configurations according to the examples in the
   * relevant configurations below (https://xtls.github.io/config/api.html#related-configuration).
   */
  listen?: string;
  /**
   * List of enabled APIs, optional values can be found in Supported API List
   * (https://xtls.github.io/config/api.html#supported-api-list).
   */
  services?: ApiServices[];
}

export interface BridgeConfig {
  tag?: string;
  domain?: string;
}

/**
 * The connection monitoring component uses HTTP pings to check the connection status of outbound
 * proxies. Docs (https://xtls.github.io/config/observatory.html#burstobservatoryobject)
 */
export interface BurstObservatoryConfig {
  /**
   * An array of strings, each element of which will be used to match an outbound connection tag prefix.
   * For example, for the following outbound connection tags: [ "a", "ab", "c", "ba" ],
   * "subjectSelector": ["a"] will match [ "a", "ab" ].
   */
  subjectSelector?: string[];
  /** Ping schema */
  pingConfig?: PingConfig;
}

/** Log configuration controls how Xray outputs logs. Docs (https://xtls.github.io/config/log.html) */
export interface LogConfig {
  /**
   * The file path for the access log. The value is a valid file path, such as "/var/log/Xray/access.log"
   * (Linux) or "C:\\Temp\\Xray\\_access.log" (Windows). When this item is not specified or is an empty
   * value, the log is output to stdout.
   */
  access?: string;
  /**
   * The file path for the error log. The value is a valid file path, such as "/var/log/Xray/error.log"
   * (Linux) or "C:\\Temp\\Xray\\_error.log" (Windows). When this item is not specified or is an empty
   * value, the log is output to stdout.
   */
  error?: string;
  /**
   * The log level for error logs, indicating the information that needs to be recorded. The default
   * value is "warning". Note that this setting applies to the error log only. It doesn't affect the
   * access log (except for "none" value). The access log doesn't have log levels.
   */
  loglevel?: LogLevel;
  /**
   * Log DNS queries made by built-in DNS clients to the access log. Example log record: DOH//doh.server
   * got answer: domain.com -> [ip1, ip2] 2.333ms.
   */
  dnsLog?: boolean;
  /**
   * IP address masking, when enabled, will automatically replace the IP address appearing in the log. It
   * is used to protect privacy when sharing logs. The default is empty and is not enabled. Currently
   * available levels are quarter, half, full. The mask form corresponds to the following: ipv4 1.2.*.*
   * 1.*.*.* [Masked IPv4]; ipv6 1234:5678::/32 1234::/16 [Masked IPv6];
   */
  maskAddress?: string;
}

/** MetricsConfig Docs (https://xtls.github.io/config/metrics.html) */
export interface MetricsConfig {
  /**
   * Outbound proxy tag for metrics. By configuring AnyDoor's incoming connection and AnyDoor routing to
   * this outbound proxy, you can access metrics through AnyDoor.
   */
  tag?: string;
  /**
   * A simpler way is to simply listen on the specified address and port to provide the service. If this
   * field is empty when set tag, it is automatically set to Metrics. If both fields are unset, the
   * kernel will not start.
   */
  listen?: string;
}

/**
 * The connection monitoring component uses HTTP pings to check the connection status of outbound
 * proxies. Docs (https://xtls.github.io/config/observatory.html)
 */
export interface ObservatoryConfig {
  /**
   * An array of strings, each element of which will be used to match an outbound connection tag prefix.
   * For example, for the following outbound connection tags: [ "a", "ab", "c", "ba" ],
   * "subjectSelector": ["a"] will match [ "a", "ab" ].
   */
  subjectSelector?: string[];
  /** The URL used to check the outbound proxy connection status. */
  probeUrl?: string;
  /**
   * The interval between checks. Time format: number + unit, for example "10s", "2h45m". Supported
   * units: ns, us, ms, s, m, h(nanoseconds, microseconds, milliseconds, seconds, minutes, hours).
   */
  probeInterval?: string;
  /**
   * true - check all relevant outgoing proxies simultaneously, then pause for the time specified in
   * probeInterval. false- check the corresponding outgoing proxies in turn, pausing for the time
   * specified in probeIntervalafter checking each proxy.
   */
  enableConcurrency?: boolean;
}

/** Ping schema */
export interface PingConfig {
  /** The URL used to check the outbound proxy connection status. This URL must return HTTP status code 204. */
  destination?: string;
  /**
   * The URL used to test local network connectivity. An empty string means no local network connectivity
   * testing is performed.
   */
  connectivity?: string;
  /**
   * Check all matching outgoing proxies for the specified time, sending sampling + 1 requests to each
   * proxy. Time format: number + unit, e.g. "10s", "2h45m". Supported units: ns, us, ms, s, m, h
   * (nanoseconds, microseconds, milliseconds, seconds, minutes, hours).
   */
  interval?: string;
  /** The number of recent test results to keep. */
  sampling?: number;
  /** Response timeout during verification. The format is the same as for interval. */
  timeout?: string;
}

export interface PortalConfig {
  tag?: string;
  domain?: string;
}

/**
 * A reverse proxy can redirect traffic from a server to a client, that is, perform reverse traffic
 * forwarding. Docs (https://xtls.github.io/config/reverse.html)
 */
export interface ReverseConfig {
  /** An array where each element is a bridge. */
  bridges?: BridgeConfig[];
  /** An array where each element is a portal. */
  portals?: PortalConfig[];
}

/** Used to configure the collection of traffic statistics. Docs (https://xtls.github.io/config/stats.html) */
export interface StatsConfig {}

/**
 * It controls the version on which this config can run. When sharing config files, this prevents
 * accidental execution on unwanted client versions. During execution, the client will check if its
 * current version meets this requirement. Support of v25.8.3+. Docs
 * (https://xtls.github.io/config/#%E5%9F%BA%E7%A1%80%E9%85%8D%E7%BD%AE%E6%A8%A1%E5%9D%97)
 */
export interface VersionConfig {
  /** Min config version. Format: x.y.z */
  min?: string;
  /** Max config version. Format: x.y.z */
  max?: string;
}

/**
 * The configuration file of Xray is in JSON format, and the configuration format for the client and
 * server is the same, except for the actual configuration content. Docs
 * (https://xtls.github.io/config)
 */
export interface XrayConfig {
  /** Log configuration controls how Xray outputs logs. */
  log?: LogConfig;
  /**
   * It controls the version on which this config can run. When sharing config files, this prevents
   * accidental execution on unwanted client versions. During execution, the client will check if its
   * current version meets this requirement. Support of v25.8.3+.
   */
  version?: VersionConfig;
  /** API interface configuration provides a set of APIs based on gRPC for remote invocation. */
  api?: ApiConfig;
  /** Config for Built-in DNS server. */
  dns?: DnsConfig;
  /** Configures routing. Specify rules to route connections through different outbounds. */
  routing?: RoutingConfig;
  /** A local policy that allows you to configure different user levels and corresponding policies. */
  policy?: PolicyConfig;
  /** An array where each element represents an incoming connection configuration. */
  inbounds?: InboundConfig[];
  /** An array where each element represents an outgoing connection configuration. */
  outbounds?: OutboundConfig[];
  /** Transport is the way the current Xray node interacts with other nodes. */
  transport?: TransportConfig;
  /** Used to configure the collection of traffic statistics. */
  stats?: StatsConfig;
  /** A reverse proxy can redirect traffic from a server to a client, that is, perform reverse traffic forwarding. */
  reverse?: ReverseConfig;
  /** Setting up FakeDNS. Can be used in conjunction with transparent proxying to obtain real domain names. */
  fakedns?: FakeDnsConfig | FakeDnsConfig[];
  /** An easier (and hopefully better) way to export statistics. */
  metrics?: MetricsConfig;
  /** The connection monitoring component uses HTTP pings to check the connection status of outbound proxies. */
  observatory?: ObservatoryConfig;
  /** The connection monitoring component uses HTTP pings to check the connection status of outbound proxies. */
  burstObservatory?: BurstObservatoryConfig;
}

export interface XrayMergeOptions {
  collectionMode?: XrayMergeCollectionMode;
  overwriteWithDefaultValues?: boolean;
  keyProperties?: string[];
}

