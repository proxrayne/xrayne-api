import type { BalancerStrategyType, DomainMatcher, NetProtocol, Network, RoutingDomainStrategy, RoutingRuleType } from "./enums";

/** This is an optional configuration parameter whose format varies for different load balancing strategies. */
export interface BalancerStrategySettings {
  observerTag?: string;
  aliveOnly?: boolean;
  timeout?: string;
  interval?: string;
  sampling?: number;
  /** Maximum allowed RTT (latency) time when measuring speed. */
  maxRTT?: string;
  /**
   * The number of optimal nodes selected by the load balancer. Traffic will be randomly distributed
   * among these nodes.
   */
  expected?: number;
  /** Maximum acceptable standard deviation of RTT time when measuring speed. */
  baselines?: string[];
  /**
   * An optional configuration parameter, an array, that allows you to specify weights for all outgoing
   * connections.
   */
  costs?: Coast[];
}

/** Balance strategy object */
export interface BalancerStrategy {
  /** Default is "random" */
  type?: BalancerStrategyType;
  /**
   * This is an optional configuration parameter whose format varies for different load balancing
   * strategies. Currently, this configuration parameter can only be added for the load balancing
   * strategy leastLoad.
   */
  settings?: BalancerStrategySettings;
}

/** The object allows you to specify weights for all outgoing connections. */
export interface Coast {
  /** Tag Outgoing connection matching. */
  match?: string;
  /** Weight value. The higher the value, the less likely the corresponding node is to be selected. */
  value?: number;
}

export interface RoutingBalancer {
  /** This load balancer's tag, used to match balancerTag against RuleObject. */
  tag?: string;
  /**
   * An array of strings, each of which will be used to match an outgoing channel tag prefix. For
   * example, for the following outgoing channel tags: [ "a", "ab", "c", "ba" ], "selector": ["a"] will
   * match [ "a", "ab" ].
   */
  selector?: string[];
  /** Balance strategy object */
  strategy?: BalancerStrategy;
}

/**
 * Configures routing. Specify rules to route connections through different outbounds. Docs
 * (https://xtls.github.io/config/routing.html)
 */
export interface RoutingConfig {
  /**
   * Domain name resolution strategy. Different strategies are used depending on the configuration.
   * Default value is "AsIs".
   */
  domainStrategy?: RoutingDomainStrategy;
  domainMatcher?: DomainMatcher;
  /** Matches an array, each element of which is a rule. */
  rules?: RoutingRule[];
  /** An array where each element is a load balancer configuration. */
  balancers?: RoutingBalancer[];
}

/** Routing rule object */
export interface RoutingRule {
  /** Routing rule type */
  type?: RoutingRuleType;
  /**
   * An array where each element represents a domain name mapping. The following formats are possible:
   * Simple string: The rule takes effect if this string matches any part of the target domain name. For
   * example, "sina.com" can match "sina.com," "sina.com.cn," and "www.sina.com," but it cannot match
   * "sina.cn." Regular expression: Starts with "regexp:", the rest is a regular expression. The rule
   * takes effect if this regular expression matches the target domain name. For example,
   * "regexp:\\\\.goo.\*\\\\.com\$"it matches "www.google.com" or "fonts.googleapis.com," but does not
   * match "google.com." (Note that in JSON, the backslash, often used in regular expressions, is used as
   * an escape character, so the backslash \in the regular expression should be replaced with \\.)
   * Subdomain (recommended): Starts with "domain:", the rest is the domain name. The rule takes effect
   * if this domain name is the target domain name or a subdomain of it. For example, "domain:xray.com"
   * matches "www.xray.com" and "xray.com," but does not match "wxray.com." Exact match: starts with
   * "full:", the rest is the domain name. The rule takes effect if this domain name exactly matches the
   * target domain name. For example, "full:xray.com" matches "xray.com" but does not match
   * "www.xray.com". Predefined domain list: Starts with "geosite:", the rest is a name, such as
   * geosite:googleor geosite:cn. For domain names and lists, see Predefined Domain Lists
   * (https://xtls.github.io/config/routing.html#%E8%B7%AF%E7%94%B1) Loading domain names from file: has
   * the form "ext:file:tag", must start with ext:(in lowercase) followed by the file name and tag, the
   * file is stored in the resources directory , the file format is the same as geosite.dat, the tag must
   * exist in the file.
   */
  domain?: string[];
  /**
   * An array where each element represents a range of IP addresses. The rule is triggered if any element
   * matches the target IP address. The following formats are possible: IP address: eg "127.0.0.1". CIDR
   * (https://en.wikipedia.org/wiki/Classless_Inter-Domain_Routing): For example, "10.0.0.0/8", can also
   * be used "0.0.0.0/0" "::/0"to specify all IPv4 or IPv6 addresses. Predefined IP address list: This
   * list is built into every Xray installation package, the filename is geoip.dat. The format is
   * "geoip:код_страны", must begin with geoip:(in lowercase), followed by a two-letter country code.
   * Almost all countries with internet access are supported. Special meaning: "geoip:private", includes
   * all private addresses, such as 127.0.0.1. The inversion function !: "geoip:!cn"denotes results not
   * included in geoip:cn. Multiple negated conditions are combined with the logical AND , while positive
   * conditions and the set of all negative conditions are combined with the logical OR . For example,
   * ip: ["geoip:!cn", "geoip:!us", "geoip:telegram"]matches IP addresses that are not in the US AND not
   * in China, OR are Telegram IP addresses. Loading IP addresses from a file: has the form
   * "ext:file:tag", must start with ext:(in lowercase) followed by the file name and tag, the file is
   * stored in the resource directory
   * s://xtls.github.io/config/features/env.html#%E8%B5%84%E6%BA%90%E6%96%87%E4%BB%B6%E8%B7%AF%E5%BE%84),
   * the file format is the same as geoip.dat, the tag must exist in the file.
   */
  ip?: string[];
  /**
   * Destination port range, three formats possible: "a-b": a and b are positive integers less than
   * 65536. This range is a closed interval, the rule takes effect if the destination port falls within
   * this range. a: a is a positive integer less than 65536. The rule takes effect if the destination
   * port is equal to a. A mixture of the two formats above, separated by a comma ",". For example:
   * "53,443,1000-2000".
   */
  port?: string;
  /**
   * Source port, three formats are possible: "a-b": a and b are positive integers less than 65536. This
   * range is a closed interval, the rule takes effect if the destination port falls within this range.
   * a: a is a positive integer less than 65536. The rule takes effect if the destination port is equal
   * to a. A mixture of the two formats above, separated by a comma ",". For example: "53,443,1000-2000".
   */
  sourcePort?: string;
  /**
   * Valid values: "tcp", "udp", or "tcp,udp". The rule takes effect if the connection type matches the
   * specified one.
   */
  network?: Network[];
  /**
   * An array where each element represents a range of IP addresses. Possible formats include IP address,
   * CIDR, GeoIP, and loading IP addresses from a file. The rule is applied if any element matches the
   * source IP address.
   */
  sourceIP?: string[];
  source?: string[];
  /**
   * The format is the same as for other IPs. It is used to specify the IP address used by the local host
   * inbound(when listening on all IP addresses, 0.0.0.0 different actual incoming IPs will result in
   * different values localIP). Doesn't work for UDP (tracking is impossible due to its datagram nature),
   * the IP address that is being listened on will always be visible (listen).
   */
  localIP?: string[];
  /**
   * An array where each element is an email address. The rule is triggered if any element matches the
   * source user. Similar to the domain name, matching using regular expressions starting with is also
   * supported regexp: (This also needs to be replaced \with \\, see the explanation in the section
   * domain.)
   */
  user?: string[];
  /**
   * The incoming VLESS connection allows the client to change the seventh and eighth bytes of the UUID
   * to any values ​​and use them as data vlessRoute. This allows the user to customize some of the
   * server's routing without changing any external fields.
   */
  vlessRoute?: string;
  /** An array where each element is a tag. The rule is applied if any element matches the incoming protocol tag. */
  inboundTag?: string[];
  /**
   * An array where each element represents a protocol. The rule takes effect if any protocol matches the
   * current connection's protocol type.
   */
  protocol?: NetProtocol[];
  /**
   * A JSON object where keys and values ​​are strings. Used to validate HTTP traffic attribute values
   * ​​(for obvious reasons, only 1.0 and 1.1 are supported). The rule is triggered if the HTTP headers
   * contain all the specified keys and the values ​​contain the specified substring. Keys are
   * case-insensitive. Values ​​support regular expressions.
   */
  attrs?: Record<string, string>;
  /** Matches the outgoing channel tag. */
  outboundTag?: string;
  /** Matches the load balancer tag. */
  balancerTag?: string;
  /**
   * If the connection comes from the local machine, a process-based match is made. If the connection is
   * not from the local machine, the match is immediately considered unsuccessful. Only Windows and Linux
   * are supported.
   */
  process?: string[];
  /** Optional, has no actual effect, used only to identify the name of this rule. */
  ruleTag?: string;
}

