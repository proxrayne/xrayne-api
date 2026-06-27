/** Extracts the string values from an enum-like object. */
export type EnumValue<T extends Record<string, string>> = T[keyof T];
/** Represents HTTP-style headers used by Xray transports. */
export type HeaderMap = Record<string, StringOrArray>;
/** Represents a primitive JSON value. */
export type JsonPrimitive = string | number | boolean | null;
/** Represents any JSON-compatible value. */
export type JsonValue = JsonPrimitive | JsonValue[] | { [key: string]: JsonValue };

/** Represents an Xray port value or range. */
export type Port = number | `${number}-${number}` | `env:${string}`;

/** Alias for Xray port values. */
export type PortValue = Port;
/** Represents a single string or a list of strings. */
export type StringOrArray = string | string[];
/** Represents an object with unknown value types. */
export type UnknownObject = Record<string, unknown>;

export interface ClientServer {
  /** Server address; IPv4, IPv6, and domain names are supported. */
  address?: string;
  /** The server port, usually the same as the port the server is listening on. */
  port?: number;
  /** Email address, optional, used to identify the user. */
  email?: string;
  /** Password. Required, any string. */
  password?: string;
}

export interface WithLevel {
  /**
   * User level. The local policy (https://xtls.github.io/config/policy.html#levelpolicyobject)
   * corresponding to this user level will be used for the connection. The value level corresponds to the
   * value level in the policy section. If not specified, the default value of 0 is used.
   */
  level?: number;
}

export interface WithUserLevel {
  /**
   * User level, the local policy (https://xtls.github.io/config/policy.html#levelpolicyobject)
   * corresponding to this user level will be used for the connection. The userLevel value corresponds to
   * the value level in the policy section . If not specified, the default value of 0 is used.
   */
  userLevel?: number;
}

export interface VNextModel<T extends WithLevel = WithLevel> {
  /** Server address; IPv4, IPv6, and domain names are supported. */
  address?: string;
  /** The server port, usually the same as the port the server is listening on. */
  port?: number;
  users?: T[];
}
