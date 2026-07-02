import { forwardRef, HtmlHTMLAttributes } from "react";

import { cn } from "@core/lib/utils";

type HtmlProps = HtmlHTMLAttributes<HTMLDivElement>;
type HtmlPProps = HtmlHTMLAttributes<HTMLParagraphElement>;

const Media = forwardRef<HTMLDivElement, HtmlProps>(({ className, children, ...props }, ref) => (
  <div {...props} ref={ref} className={cn("mb-2", className)}>
    {children}
  </div>
));

const Header = forwardRef<HTMLParagraphElement, HtmlPProps>(
  ({ className, children, ...props }, ref) => (
    <h4
      {...props}
      ref={ref}
      className={cn("text-base/relaxed text-center font-semibold", className)}
    >
      {children}
    </h4>
  ),
);

const Subheader = forwardRef<HTMLParagraphElement, HtmlProps>(
  ({ className, children, ...props }, ref) => (
    <div
      {...props}
      ref={ref}
      className={cn("text-sm text-center text-muted-foreground", className)}
    >
      {children}
    </div>
  ),
);

const Actions = forwardRef<HTMLParagraphElement, HtmlPProps>(
  ({ className, children, ...props }, ref) => (
    <h4 {...props} ref={ref} className={cn("mt-2", className)}>
      {children}
    </h4>
  ),
);

interface PlaceholderProps extends HtmlProps {}

const Placeholder = forwardRef<HTMLDivElement, PlaceholderProps>(
  ({ className, children, ...props }, ref) => (
    <div
      {...props}
      ref={ref}
      className={cn(
        "px-8 py-10 md:p-10 min-h-64 flex flex-col gap-2 items-center justify-center",
        className,
      )}
    >
      {children}
    </div>
  ),
);

export default Object.assign(Placeholder, {
  Media,
  Header,
  Subheader,
  Actions,
});
