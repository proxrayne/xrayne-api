import { ReactNode } from "react";
import isString from "lodash/isString";

import { cn } from "@core/lib/utils";

import { SidebarTrigger, useSidebar } from "../sidebar";

interface Props {
  className?: string;
  children: ReactNode;
  hint?: ReactNode;
  classNames?: Partial<{ hint: string }>;
}

function PageHeader({ className, children, hint, classNames = {} }: Props) {
  const { isMobile } = useSidebar();

  return (
    <div
      className={cn(
        "-mx-3 px-3 pb-3 pt-5 mb-4 backdrop-blur-2xl flex items-center gap-x-3 sticky top-0 z-10",
        className,
      )}
    >
      {isMobile && <SidebarTrigger />}
      {isString(children) ? (
        <h3 className="text-xl font-semibold">{children}</h3>
      ) : (
        children
      )}
      {hint && <div className={cn("ml-auto", classNames.hint)}>{hint}</div>}
    </div>
  );
}

export default PageHeader;
