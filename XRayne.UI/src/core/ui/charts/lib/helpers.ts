import tinycolor from "tinycolor2";
export function createDarkerShades(color: string, count: number) {
  const base = tinycolor(color);

  return Array.from({ length: count }, (_, index) => {
    const amount = (index / Math.max(1, count - 1)) * 45;

    return base.clone().darken(amount).toHexString();
  });
}

export function createLighterShades(color: string, count: number) {
  const base = tinycolor(color);

  return Array.from({ length: count }, (_, index) => {
    const amount = (index / Math.max(1, count - 1)) * 30;

    return base.clone().lighten(amount).toHexString();
  });
}
