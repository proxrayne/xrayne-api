import { ReactNode } from "react";
import {
  PolarAngleAxis,
  RadialBar,
  RadialBarChart,
  ResponsiveContainer,
} from "recharts";

import { cn } from "@core/lib/utils";

interface DataItem {
  label: string;
  value: number;
  fill: string;
}

interface Props<T extends DataItem> {
  data: T[];
  className?: string;
  strokeWidth?: number;
  children?: ReactNode;
  footer?: ReactNode;
  classNames?: Partial<{ content: string }>;
}

function RadialChart<T extends DataItem>({
  data,
  className,
  strokeWidth = 10,
  children,
  footer,
  classNames = {},
}: Props<T>) {
  return (
    <div className={cn("relative", className)}>
      <ResponsiveContainer width="100%" height="100%">
        <RadialBarChart
          data={data}
          cx="50%"
          cy="52%"
          innerRadius="100%"
          outerRadius="80%"
          startAngle={225}
          endAngle={-45}
          barSize={strokeWidth}
        >
          <PolarAngleAxis
            type="number"
            domain={[0, 100]}
            dataKey="value"
            tick={false}
          />
          <RadialBar
            dataKey="value"
            cornerRadius={999}
            background={{
              fill: "var(--secondary)",
            }}
          />
        </RadialBarChart>
      </ResponsiveContainer>
      {children && (
        <div
          className={cn(
            "absolute left-[50%] top-[50%] translate-[-50%]",
            classNames.content,
          )}
        >
          {children}
        </div>
      )}
      {footer && (
        <div className="absolute bottom-3 left-[50%] translate-[-50%]">
          {footer}
        </div>
      )}
    </div>
  );
}

export default RadialChart;
