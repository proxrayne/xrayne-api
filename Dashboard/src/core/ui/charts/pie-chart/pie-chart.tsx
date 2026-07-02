import { useMemo } from "react";
import { Label, PieChart as NativePieChart, Pie } from "recharts";

import { createLighterShades } from "../lib/helpers";
interface DataItem {
  label: string;
  value: number;
}

interface Props<T extends DataItem> {
  data: T[];
}

const ACCENT = {
  light: "#0485f7",
};

function PieChart<T extends DataItem>({ data }: Props<T>) {
  const chartData = useMemo(() => {
    const colors = createLighterShades(ACCENT.light, data.length);

    console.log(colors);

    return data.map((item, index) => ({ ...item, fill: colors[index] }));
  }, [data]);

  return (
    <NativePieChart responsive className="w-full h-65">
      <Pie
        data={chartData}
        cx="50%"
        cy="50%"
        dataKey="value"
        nameKey="label"
        innerRadius={76}
        outerRadius={90}
        startAngle={90}
        endAngle={-270}
        paddingAngle={0}
        cornerRadius={10}
        stroke="none"
        isAnimationActive
      />
      <Label position="center">Flex: 1 1 200px</Label>
    </NativePieChart>
  );
}

export default PieChart;
