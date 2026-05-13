import { forwardRef, HtmlHTMLAttributes } from "react";

import { cn } from "@core/lib/utils";

import ChildSlot from "./child-slot";

type Variants = "accent" | "danger" | "warning" | "success" | "secondary";
interface Props extends HtmlHTMLAttributes<HTMLDivElement> {
  asChild?: boolean;
  variant?: Variants;
}

const STYLE_PRESETS: Record<Variants, string> = {
  accent: "bg-blue-400/15 text-blue-400",
  danger: "bg-red-400/15 text-red-400",
  secondary: "bg-muted text-muted-foreground",
  success: "bg-green-400/15 text-green-400",
  warning: "bg-orange-400/15 text-orange-400",
};

const ColoredIcon = forwardRef<HTMLDivElement, Props>(
  ({ className, children, variant = "accent", asChild, ...props }, ref) => {
    const classes = cn(
      "rounded-2xl p-4 [&>svg]:size-8 [&_svg]:shrink-0 [&_img]:size-8",
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
