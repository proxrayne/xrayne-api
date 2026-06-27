export function compareVersions(a: string, b: string): -1 | 0 | 1 {
  const normalize = (version: string) =>
    version
      .replace(/^v/i, "") // убираем v1.8.10
      .split(/[\.-]/) // поддержка 1.8.10-beta
      .map((part) => {
        const num = Number(part);
        return Number.isNaN(num) ? part : num;
      });

  const pa = normalize(a);
  const pb = normalize(b);

  const max = Math.max(pa.length, pb.length);

  for (let i = 0; i < max; i++) {
    const va = pa[i] ?? 0;
    const vb = pb[i] ?? 0;

    // number vs string
    if (typeof va === "number" && typeof vb === "string") {
      return 1;
    }

    if (typeof va === "string" && typeof vb === "number") {
      return -1;
    }

    if (va > vb) {
      return 1;
    }

    if (va < vb) {
      return -1;
    }
  }

  return 0;
}
