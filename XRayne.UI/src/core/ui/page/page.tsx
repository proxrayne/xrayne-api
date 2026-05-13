import { ReactNode } from "react";

import { cn } from "@core/lib/utils";

interface Props {
  className?: string;
  children: ReactNode;
}

function Page({ className, children }: Props) {
  return <main className={cn("pb-8", className)}>{children}</main>;
}

export default Page;
