import { ReactNode } from "react";

import { RadialChart } from "@core/ui/charts";
import { Card } from "@core/ui/card";

import { getUsagePercentColor } from "../lib/helpers";

interface Props {
  percent: number;
  label: string;
  footer: ReactNode;
  subheader: ReactNode;
}

function UsedCard({ footer, percent, subheader, label }: Props) {
  return (
    <Card size="sm" className="py-0 relative col-span-4 sm:col-span-2 lg:col-span-1">
      <RadialChart
        className="h-48"
        strokeWidth={10}
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
        <p className="text-xl/tight font-semibold">
          {percent < 0.01 ? "< 0.01" : percent.toFixed(2)}%
        </p>
        <p className="text-xs text-muted-foreground font-medium mt-0.5">{subheader}</p>
      </RadialChart>
    </Card>
  );
}

export default UsedCard;
