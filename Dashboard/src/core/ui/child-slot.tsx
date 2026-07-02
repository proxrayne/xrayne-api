import { cloneElement, ComponentProps, ElementType, isValidElement } from "react";

import { cn } from "@core/lib/utils";

function ChildSlot<T extends ElementType = "div">({ children, ...props }: ComponentProps<T>) {
  if (!isValidElement(children)) {
    throw new Error("Slot expects a single React element child");
  }

  return cloneElement(children, {
    ...props,
    ...(children.props ?? {}),
    // @ts-ignore
    className: cn(children.props?.className, props.className),
  });
}

export default ChildSlot;
