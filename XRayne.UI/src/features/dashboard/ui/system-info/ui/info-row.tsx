import { ReactNode } from "react";

interface Props {
  label: string;
  children?: ReactNode;
  defaultValue?: ReactNode;
}

function InfoRow({ children, label, defaultValue = "n/a" }: Props) {
  return (
    <div className="flex items-end gap-2 mt-2">
      <p className="text-sm text-foreground/70">{label}</p>
      <span className="min-w-0 flex-1 border-b border-dotted border-muted h-[calc(80%)]] mb-0.5" />
      <div className="text-sm font-medium">
        {children || <span className="text-muted">{defaultValue}</span>}
      </div>
    </div>
  );
}

export default InfoRow;
