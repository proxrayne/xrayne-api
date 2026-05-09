export function getUsagePercentColor(percent: number): string {
  if (percent > 90) {
    return "var(--danger)";
  }

  if (percent > 60) {
    return "var(--warning)";
  }

  return "var(--accent)";
}
