import { format } from "date-fns";
import { Check, CircleAlertIcon } from "lucide-react";

import { Item, ItemActions, ItemContent, ItemDescription, ItemTitle } from "@core/ui/item";
import { Button } from "@core/ui/button";
import { cn } from "@core/lib/utils";
import Placeholder from "@core/ui/placeholder";
import { Skeleton } from "@core/ui/skeleton";
import ColoredIcon from "@core/ui/colored-icon";

import { GitHubReleaseDto, useCoreReleases } from "@features/core";

const CONTAINER_CLASSES = "mt-3 px-2 -mx-2 overflow-y-auto min-h-100 max-h-[70vh] no-scrollbar";

interface ChooseVersionProps {
  version: string | null;
  onSelect(version: GitHubReleaseDto): void;
}

function ChooseVersion({ version, onSelect }: ChooseVersionProps) {
  const { releases, isLoaded, error, hasMore, isMoreLoading, loadMore, refetch } = useCoreReleases({
    perPage: 10,
  });

  if (!isLoaded) {
    return (
      <div className={CONTAINER_CLASSES}>
        {Array.from({ length: 10 }).map((_, index) => (
          <ChooseVersion.ItemSkeleton key={index} />
        ))}
      </div>
    );
  }

  if (error || !releases) {
    return (
      <Placeholder>
        <ColoredIcon asChild variant="danger">
          <Placeholder.Media>
            <CircleAlertIcon />
          </Placeholder.Media>
        </ColoredIcon>
        <Placeholder.Header>Failed data loading</Placeholder.Header>
        <Placeholder.Subheader>
          Unhandled error. Please, try to reload latter.
        </Placeholder.Subheader>
        <Placeholder.Actions>
          <Button onClick={() => refetch()}>Try reload</Button>
        </Placeholder.Actions>
      </Placeholder>
    );
  }

  return (
    <div className={CONTAINER_CLASSES}>
      {releases.map((release) => (
        <Item key={release.tagName} size="xs" asChild>
          <button className="hover:bg-accent/65 active:bg-accent" onClick={() => onSelect(release)}>
            <ItemContent className="flex flex-col">
              <ItemTitle>{release.tagName}</ItemTitle>
              <ItemDescription className="text-xs">
                <span
                  className={cn(
                    "font-medium",
                    release.prerelease ? "text-orange-300/90" : "text-blue-400/90",
                  )}
                >
                  {release.prerelease ? "Pre-release" : "Stable"}
                </span>
                <span className="mx-1">&bull;</span>Published:{" "}
                {format(release.publishedAt, "dd MMM yyyy")}
              </ItemDescription>
            </ItemContent>
            <ItemActions>
              {version && release.tagName.endsWith(version) && (
                <Check className="size-4 text-blue-500" />
              )}
            </ItemActions>
          </button>
        </Item>
      ))}
      {isMoreLoading ? (
        <ChooseVersion.ItemSkeleton className="mx-3" />
      ) : (
        hasMore && (
          <Button
            size="sm"
            onClick={() => loadMore()}
            variant="secondary"
            className="mx-auto mt-2 block"
          >
            Load more
          </Button>
        )
      )}
    </div>
  );
}

ChooseVersion.ItemSkeleton = ({ className }: { className?: string }) => (
  <div className={cn("flex items-center justify-between gap-3 py-2", className)}>
    <div className="min-w-0 flex-1">
      <Skeleton className="h-4 w-20 rounded-md" />
      <Skeleton className="mt-2 h-3 w-44 rounded-md" />
    </div>
    <Skeleton className="size-6 shrink-0 rounded-full" />
  </div>
);

export default ChooseVersion;
