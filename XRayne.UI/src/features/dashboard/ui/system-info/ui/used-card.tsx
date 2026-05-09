import { ReactNode } from "react";
import { RadialChart } from "@core/ui/charts";
import { Surface } from "@heroui/react";

import { getUsagePercentColor } from "../lib/helpers";

interface Props {
  percent: number;
  label: string;
  footer: ReactNode;
  subheader: ReactNode;
}

function UsedCard({ footer, percent, subheader, label }: Props) {
  return (
    <Surface className="rounded-4xl pt-3 relative col-span-2">
      <RadialChart
        className="h-48"
        strokeWidth={12}
        data={[
          {
            label,
            value: percent,
            fill: getUsagePercentColor(percent),
          },
        ]}
        classNames={{ content: "flex flex-col pt-1 items-center" }}
        footer={
          <p className="text-xs font-medium text-foreground/60">{footer}</p>
        }
      >
        <p className="text-xl/tight font-semibold">{percent.toFixed(2)}%</p>
        <p className="text-xs text-muted font-medium mt-0.5">{subheader}</p>
      </RadialChart>
    </Surface>
  );
}

export default UsedCard;
