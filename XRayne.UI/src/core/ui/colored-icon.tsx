import { forwardRef, HtmlHTMLAttributes } from "react";
import { cn } from "@heroui/styles";

import ChildSlot from "./child-slot";

type Variants =
  | "default"
  | "accent"
  | "danger"
  | "warning"
  | "success"
  | "secondary";
interface Props extends HtmlHTMLAttributes<HTMLDivElement> {
  asChild?: boolean;
  variant?: Variants;
}

const STYLE_PRESETS: Record<Variants, string> = {
  accent: "bg-accent/15 text-accent",
  danger: "bg-danger/15 text-danger",
  default: "bg-default/15 text-default",
  secondary: "bg-muted/15 text-muted",
  success: "bg-success/15 text-success",
  warning: "bg-warning/15 text-warning",
};

const ColoredIcon = forwardRef<HTMLDivElement, Props>(
  ({ className, children, variant = "default", asChild, ...props }, ref) => {
    const classes = cn(
      "rounded-3xl p-4 [&>svg]:size-8 [&_svg]:shrink-0 [&_img]:size-8",
      STYLE_PRESETS[variant],
      className,
    );
    if (asChild) {
      return (
        <ChildSlot {...props} className={classes}>
          {children}
        </ChildSlot>
      );
    }

    return (
      <div {...props} className={classes} ref={ref}>
        {children}
      </div>
    );
  },
);

export default ColoredIcon;
