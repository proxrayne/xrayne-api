export function isIpAddress(value: string) {
  return isIpv4Address(value) || isIpv6Address(value);
}

export function isIpNetwork(value: string) {
  const [address, prefix, ...rest] = value.trim().split("/");

  if (!address || !prefix || rest.length > 0 || !/^\d+$/.test(prefix)) {
    return false;
  }

  const prefixLength = Number(prefix);

  if (isIpv4Address(address)) {
    return prefixLength >= 0 && prefixLength <= 32;
  }

  if (isIpv6Address(address)) {
    return prefixLength >= 0 && prefixLength <= 128;
  }

  return false;
}

export function isIpv4Address(value: string) {
  const parts = value.trim().split(".");

  return (
    parts.length === 4 &&
    parts.every((part) => {
      if (!/^\d+$/.test(part)) {
        return false;
      }

      const parsed = Number(part);

      return parsed >= 0 && parsed <= 255 && part === String(parsed);
    })
  );
}

export function isIpv6Address(value: string) {
  const normalized = value.trim();

  if (!normalized.includes(":")) {
    return false;
  }

  const zoneIndex = normalized.indexOf("%");
  const address = zoneIndex >= 0 ? normalized.slice(0, zoneIndex) : normalized;

  if (address.split("::").length > 2) {
    return false;
  }

  const sides = address.split("::");
  const left = splitIpv6Groups(sides[0]);
  const right = splitIpv6Groups(sides[1] ?? "");
  const groups = [...left, ...right];
  const hasCompression = sides.length === 2;
  const ipv4Tail = groups.at(-1)?.includes(".") === true;

  if (ipv4Tail && !isIpv4Address(groups.at(-1)!)) {
    return false;
  }

  const hexGroups = ipv4Tail ? groups.slice(0, -1) : groups;

  if (!hexGroups.every((group) => /^[0-9a-fA-F]{1,4}$/.test(group))) {
    return false;
  }

  const groupCount = hexGroups.length + (ipv4Tail ? 2 : 0);

  return hasCompression ? groupCount < 8 : groupCount === 8;
}

function splitIpv6Groups(value: string) {
  return value.length === 0 ? [] : value.split(":");
}
