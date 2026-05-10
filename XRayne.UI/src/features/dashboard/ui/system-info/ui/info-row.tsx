import { ReactNode } from "react";
import { cn } from "@heroui/styles";

interface Props {
  label: string;
  children?: ReactNode;
  defaultValue?: ReactNode;
  className?: string;
  classNames?: Partial<{ content: string }>;
}

function InfoRow({
  children,
  label,
  className,
  classNames = {},
  defaultValue = "n/a",
}: Props) {
  return (
    <div className={cn("flex items-end gap-2 not-first:mt-2", className)}>
      <p className="text-sm text-foreground/70">{label}</p>
      <span className="min-w-0 flex-1 border-b border-dotted border-muted h-[calc(80%)]] mb-0.5" />
      <div className={cn("text-sm font-medium", classNames.content)}>
        {children || <span className="text-muted">{defaultValue}</span>}
      </div>
    </div>
  );
}

export default InfoRow;
