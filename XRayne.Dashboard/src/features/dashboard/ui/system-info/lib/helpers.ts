export function getUsagePercentColor(percent: number): string {
  if (percent > 90) {
    return "var(--color-red-400)";
  }

  if (percent > 60) {
    return "var(--color-orange-400)";
  }

  return "var(--chart-3)";
}
