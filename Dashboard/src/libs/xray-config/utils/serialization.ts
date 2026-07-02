import type { Port, UnknownObject } from "../common";
import type { DnsConfig } from "../dns";
import type { FakeDnsConfig } from "../dns";
import type { InboundConfig } from "../inbounds";
import type { OutboundConfig } from "../outbounds";
import type { StreamSettings } from "../transports";
import type { XrayConfig } from "../config";

function isPlainObject(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null && !Array.isArray(value);
}

function normalizeValue(value: unknown): unknown {
  if (value === null || value === undefined) {
    return undefined;
  }

  if (Array.isArray(value)) {
    return value.map(normalizeValue).filter((item) => item !== undefined);
  }

  if (!isPlainObject(value)) {
    return value;
  }

  const output: Record<string, unknown> = {};
  for (const [key, item] of Object.entries(value)) {
    const normalized = normalizeValue(item);
    if (normalized !== undefined) {
      output[key] = normalized;
    }
  }

  if ("rawSettings" in output && !("tcpSettings" in output)) {
    output.tcpSettings = output.rawSettings;
  }

  return output;
}

function normalizeAfterParse(value: unknown): unknown {
  if (Array.isArray(value)) {
    return value.map(normalizeAfterParse);
  }

  if (!isPlainObject(value)) {
    return value;
  }

  const output: Record<string, unknown> = {};
  for (const [key, item] of Object.entries(value)) {
    output[key] = normalizeAfterParse(item);
  }

  if ("tcpSettings" in output && !("rawSettings" in output)) {
    output.rawSettings = output.tcpSettings;
  }

  return output;
}

export function parsePort(value: number | string): Port {
  if (typeof value === "number") {
    return value;
  }

  if (/^env:/i.test(value) || /^\d+-\d+$/.test(value) || /^\d+$/.test(value)) {
    return value as Port;
  }

  throw new Error(`Invalid port value: ${value}`);
}

export function normalizeDnsHosts(hosts: DnsConfig["hosts"]): Record<string, string[]> | undefined {
  if (!hosts) {
    return undefined;
  }

  const result: Record<string, string[]> = {};
  for (const [key, value] of Object.entries(hosts)) {
    result[key] = value == null ? [] : Array.isArray(value) ? value : [value];
  }

  return result;
}

export function normalizeFakeDns(
  value: XrayConfig["fakedns"],
): FakeDnsConfig | FakeDnsConfig[] | undefined {
  if (value == null) {
    return undefined;
  }

  return Array.isArray(value) && value.length === 1 ? value[0] : value;
}

export function toXrayObject<
  T extends XrayConfig | InboundConfig | OutboundConfig | StreamSettings,
>(value: T): T {
  const normalized = normalizeValue(value) as T;

  if (isPlainObject(normalized) && "dns" in normalized && isPlainObject(normalized.dns)) {
    const dns = normalized.dns as DnsConfig;
    if (dns.hosts) {
      dns.hosts = normalizeDnsHosts(dns.hosts);
    }
  }

  if (isPlainObject(normalized) && "fakedns" in normalized) {
    normalized.fakedns = normalizeFakeDns(normalized.fakedns as XrayConfig["fakedns"]);
  }

  return normalized;
}

export function toXrayJson(value: XrayConfig, indented = true): string {
  return JSON.stringify(toXrayObject(value), null, indented ? 2 : undefined);
}

export function fromXrayJson(json: string): XrayConfig {
  return normalizeAfterParse(JSON.parse(json)) as XrayConfig;
}

export function mergeXrayConfig<T extends UnknownObject>(source: T, overlay: Partial<T>): T {
  return mergeObjects(source, overlay) as T;
}

function mergeObjects(source: unknown, overlay: unknown): unknown {
  if (overlay === undefined || overlay === null) {
    return cloneJson(source);
  }

  if (Array.isArray(source) || Array.isArray(overlay)) {
    return cloneJson(overlay);
  }

  if (!isPlainObject(source) || !isPlainObject(overlay)) {
    return cloneJson(overlay);
  }

  const result: Record<string, unknown> = { ...source };
  for (const [key, value] of Object.entries(overlay)) {
    result[key] = key in result ? mergeObjects(result[key], value) : cloneJson(value);
  }

  return result;
}

function cloneJson<T>(value: T): T {
  if (value === undefined || value === null) {
    return value;
  }

  return JSON.parse(JSON.stringify(value)) as T;
}
