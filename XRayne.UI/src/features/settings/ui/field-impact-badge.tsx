import { cn } from "@core/lib/utils";

import type { RestartImpact } from "../lib/api.types";

interface FieldImpactBadgeProps {
  impact?: RestartImpact;
}

export function FieldImpactBadge({ impact }: FieldImpactBadgeProps) {
  if (!impact || impact === "None") {
    return null;
  }

  const label = impact === "FullRestart" ? "Требует перезапуск" : "Применяется на лету";
  const tone = impact === "FullRestart"
    ? "border-amber-500/40 bg-amber-500/10 text-amber-300"
    : "border-emerald-500/30 bg-emerald-500/10 text-emerald-300";

  return (
    <span
      className={cn(
        "ml-2 inline-flex items-center rounded-full border px-2 py-0.5 text-xs",
        tone,
      )}
    >
      {label}
    </span>
  );
}
