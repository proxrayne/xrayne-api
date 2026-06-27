import { ComponentPropsWithoutRef, ReactNode, forwardRef } from "react";

import { cn } from "@core/lib/utils";

import { SidebarTrigger, useSidebar } from "./sidebar";

type PageProps = ComponentPropsWithoutRef<"main">;

interface HeaderProps extends ComponentPropsWithoutRef<"div"> {
  children: ReactNode;
}

type TitleProps = ComponentPropsWithoutRef<"h1">;

type SubtitleProps = ComponentPropsWithoutRef<"p">;

type ToolbarProps = ComponentPropsWithoutRef<"div">;

const Page = forwardRef<HTMLElement, PageProps>(
  ({ className, children, ...props }, ref) => (
    <main
      ref={ref}
      className={cn("pb-8 flex flex-col min-h-full", className)}
      {...props}
    >
      {children}
    </main>
  ),
);

Page.displayName = "Page";

const PageHeader = forwardRef<HTMLDivElement, HeaderProps>(
  ({ className, children, ...props }, ref) => {
    const { isMobile } = useSidebar();

    return (
      <div
        ref={ref}
        data-mobile={isMobile ? "true" : undefined}
        className={cn(
          "group/page-header -mx-3 mb-4 grid grid-cols-[minmax(0,1fr)_auto] items-start gap-x-3 gap-y-1 px-3 pb-3 pt-5 backdrop-blur-2xl sticky top-0 z-10 data-[mobile=true]:grid-cols-[auto_minmax(0,1fr)_auto]",
          className,
        )}
        {...props}
      >
        {isMobile && <SidebarTrigger />}
        {children}
      </div>
    );
  },
);

PageHeader.displayName = "PageHeader";

const PageTitle = forwardRef<HTMLHeadingElement, TitleProps>(
  ({ className, children, ...props }, ref) => (
    <h1
      ref={ref}
      className={cn(
        "col-start-1 row-start-1 min-w-0 text-xl font-semibold leading-7 group-data-[mobile=true]/page-header:col-start-2",
        className,
      )}
      {...props}
    >
      {children}
    </h1>
  ),
);

PageTitle.displayName = "PageTitle";

const PageSubtitle = forwardRef<HTMLParagraphElement, SubtitleProps>(
  ({ className, children, ...props }, ref) => (
    <p
      ref={ref}
      className={cn(
        "col-start-1 col-end-3 row-start-2 min-w-0 text-sm text-foreground-500 group-data-[mobile=true]/page-header:col-start-2 group-data-[mobile=true]/page-header:col-end-4 text-secondary-foreground",
        className,
      )}
      {...props}
    >
      {children}
    </p>
  ),
);

PageSubtitle.displayName = "PageSubtitle";

const PageToolbar = forwardRef<HTMLDivElement, ToolbarProps>(
  ({ className, children, ...props }, ref) => (
    <div
      ref={ref}
      className={cn(
        "col-start-2 row-start-1 flex shrink-0 items-center justify-end gap-2 group-data-[mobile=true]/page-header:col-start-3",
        className,
      )}
      {...props}
    >
      {children}
    </div>
  ),
);

PageToolbar.displayName = "PageToolbar";

export default Object.assign(Page, {
  Header: PageHeader,
  Subtitle: PageSubtitle,
  Title: PageTitle,
  Toolbar: PageToolbar,
});
