export interface LevelPolicy {
  /**
   * Connection establishment time limit (handshake). Measured in seconds. The default value is 4. When
   * processing a new connection by the incoming proxy, if the time spent on the handshake exceeds this
   * value, the connection is terminated.
   */
  handshake?: number;
  /**
   * Connection idle timeout. Measured in seconds. The default value is 300. When processing a connection
   * by an incoming or outgoing proxy, if no data is transferred for a period of time (including outgoing
   * and incoming data), the connection is terminated.connIdle.
   */
  connIdle?: number;
  /**
   * The timeout limit after closing the outgoing connection channel. Measured in seconds. The default
   * value is 2. When a server (e.g., a remote website) closes the outgoing connection, the outgoing
   * proxy terminates the connection after uplinkOnly seconds.
   */
  uplinkOnly?: number;
  /**
   * The timeout limit after closing an incoming connection. Measured in seconds. The default value is 5.
   * When a client (e.g., a browser) closes an incoming connection, the incoming proxy terminates the
   * connection after downlinkOnly seconds.
   */
  downlinkOnly?: number;
  /** If the value is true, enable outgoing traffic accounting for all users of the current level. */
  statsUserUplink?: boolean;
  /** If the value is true, enable incoming traffic accounting for all users of the current level. */
  statsUserDownlink?: boolean;
  /**
   * The internal buffer size for each request, in kilobytes. Note that multiple requests can be
   * multiplexed over a single connection (e.g., when using mux.coolGRPC). This means that even if they
   * use the same underlying connection, their buffer pools are independent. Default value: On ARM, MIPS,
   * MIPSLE platforms the default value is 0; On ARM64, MIPS64, MIPS64LE platforms the default value is
   * 4; On other platforms, the default value is 512. The default value can be overridden using an
   * environment variable XRAY_RAY_BUFFER_SIZE. Note that the unit of measurement for environment
   * variables is megabytes (MB) (e.g., setting a value 1in an environment variable is equivalent to
   * setting it 1024in the configuration).
   */
  bufferSize?: number;
}

/**
 * A local policy that allows you to configure different user levels and corresponding policies. Docs
 * (https://xtls.github.io/config/policy.html)
 */
export interface PolicyConfig {
  /** Xray will apply different local policies based on the actual user level. */
  levels?: Record<string, LevelPolicy>;
  /** Xray system level policies. */
  system?: SystemPolicy;
}

/** Xray system level policies. */
export interface SystemPolicy {
  /** If the value is true, enable outgoing traffic accounting for all incoming connections. */
  statsInboundUplink?: boolean;
  /** If the value is true, enable incoming traffic accounting for all incoming connections. */
  statsInboundDownlink?: boolean;
  /** If the value is true, enable outgoing traffic accounting for all outgoing connections. */
  statsOutboundUplink?: boolean;
  /** If the value is true, enable incoming traffic accounting for all outgoing connections. */
  statsOutboundDownlink?: boolean;
}
