import {
  Alert,
  Button,
  cn,
  Description,
  Label,
  ListBox,
  Skeleton,
} from "@heroui/react";
import { format } from "date-fns";
import find from "lodash/find";

import {
  CheckCircleIcon,
  ExclamationCircleIcon,
} from "@heroicons/react/16/solid";

import Placeholder from "@core/ui/placeholder";

import { GitHubReleaseDto, useCoreReleases } from "@features/core";

interface ChooseVersionProps {
  version: string | null;
  onSelect(version: GitHubReleaseDto): void;
}

function ChooseVersion({ version, onSelect }: ChooseVersionProps) {
  const { releases, isLoaded, error, refetch } = useCoreReleases({
    perPage: 10,
  });

  if (!isLoaded) {
    return <ChooseVersion.Skeleton />;
  }

  if (error || !releases) {
    return (
      <Placeholder>
        <Placeholder.Media>
          <ExclamationCircleIcon className="size-10" />
        </Placeholder.Media>
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
    <>
      <ListBox
        selectedKeys={new Set(version ? [`v${version}`] : [])}
        selectionMode="single"
        onSelectionChange={(keys) => {
          const key = String(Array.from(keys)[0]);

          onSelect(find(releases, { tagName: key })!);
        }}
      >
        {releases.map((release) => (
          <ListBox.Item
            id={release.tagName}
            key={release.tagName}
            textValue={release.tagName}
          >
            <div className="flex flex-col">
              <Label className="font-medium">{release.tagName}</Label>
              <Description className="mt-0.5">
                <span
                  className={cn(
                    "font-medium",
                    release.prerelease ? "text-warning/90" : "text-accent/90",
                  )}
                >
                  {release.prerelease ? "Pre-release" : "Stable"}
                </span>
                <span className="mx-1">&bull;</span>Published:{" "}
                {format(release.publishedAt, "dd MMM yyyy")}
              </Description>
            </div>
            <ListBox.ItemIndicator>
              {({ isSelected }) =>
                isSelected && (
                  <CheckCircleIcon className="size-6 text-accent/90" />
                )
              }
            </ListBox.ItemIndicator>
          </ListBox.Item>
        ))}
      </ListBox>
      <Alert status="warning" className="shadow-none bg-warning/10 mt-2">
        <Alert.Indicator />
        <Alert.Content>
          <Alert.Title>Please note</Alert.Title>
          <Alert.Description>
            Older versions may not support current settings.
          </Alert.Description>
        </Alert.Content>
      </Alert>
    </>
  );
}

ChooseVersion.Skeleton = () => (
  <div className="flex flex-col gap-1 px-2">
    {Array.from({ length: 10 }).map((_, index) => (
      <div key={index} className="flex items-center justify-between gap-3 py-2">
        <div className="min-w-0 flex-1">
          <Skeleton className="h-4 w-20 rounded-md" />
          <Skeleton className="mt-2 h-3 w-44 rounded-md" />
        </div>
        <Skeleton className="size-6 shrink-0 rounded-full" />
      </div>
    ))}
  </div>
);

export default ChooseVersion;
