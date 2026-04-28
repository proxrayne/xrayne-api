import { ReactNode } from "react";
import { cn } from "@heroui/styles";

interface Props {
  className?: string;
  children: ReactNode;
}

function Page({ className, children }: Props) {
  return <main className={cn("", className)}>{children}</main>;
}

export default Page;
