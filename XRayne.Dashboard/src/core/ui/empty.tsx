import * as React from "react";

import { cn } from "@core/lib/utils";

function Empty({ className, ...props }: React.ComponentProps<"div">) {
  return (
    <div
      data-slot="empty"
      className={cn(
        "flex min-h-40 flex-col items-center justify-center gap-3 rounded-2xl bg-muted/60 px-6 py-8 text-center",
        className,
      )}
      {...props}
    />
  );
}

function EmptyMedia({ className, ...props }: React.ComponentProps<"div">) {
  return (
    <div
      data-slot="empty-media"
      className={cn("flex size-9 items-center justify-center rounded-full bg-secondary text-muted-foreground", className)}
      {...props}
    />
  );
}

function EmptyTitle({ className, ...props }: React.ComponentProps<"h3">) {
  return (
    <h3
      data-slot="empty-title"
      className={cn("text-sm font-semibold text-foreground", className)}
      {...props}
    />
  );
}

function EmptyDescription({ className, ...props }: React.ComponentProps<"p">) {
  return (
    <p
      data-slot="empty-description"
      className={cn("max-w-md text-sm text-muted-foreground", className)}
      {...props}
    />
  );
}

function EmptyAction({ className, ...props }: React.ComponentProps<"div">) {
  return <div data-slot="empty-action" className={cn("mt-1", className)} {...props} />;
}

export { Empty, EmptyAction, EmptyDescription, EmptyMedia, EmptyTitle };
