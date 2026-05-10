import { Button, Card, Skeleton } from "@heroui/react";

import { Cog6ToothIcon, Cog8ToothIcon, CogIcon } from "@heroicons/react/16/solid";

import Placeholder from "@core/ui/placeholder";

import { useCoreStatus } from "@features/core";

import InfoRow from "../info-row";
import CoreUpdateModal from "../core-update-modal";
import CoreControl from "./ui/core-control";

const GRID_COLS = "col-span-4 md:col-span-2";

function XrayOptions() {
  const { status, error, isLoaded, refetch } = useCoreStatus();

  if (!isLoaded) {
    return <XrayOptions.Skeleton />;
  }

  if (error || !status) {
    return (
      <Card className={GRID_COLS}>
        <Placeholder>
          <Placeholder.Header>Failed data loading</Placeholder.Header>
          <Placeholder.Subheader>
            Unhandled error. Please, try to reload latter.
          </Placeholder.Subheader>
          <Placeholder.Actions>
            <Button onClick={() => refetch()}>Try reload</Button>
          </Placeholder.Actions>
        </Placeholder>
      </Card>
    );
  }

  return (
    <Card className={GRID_COLS}>
      <Card.Header className="min-h-8 flex justify-between items-center gap-x-3 flex-row">
        <Card.Title className="font-semibold">Xray info</Card.Title>
        <CoreControl {...status} />
      </Card.Header>
      <Card.Content>
        <InfoRow label="Status" defaultValue="Not installed">
          {status.isInstalled && `${status.isStarted ? "Working" : "Stoped"}`}
        </InfoRow>
        <InfoRow label="Version" classNames={{ content: "flex gap-x-1" }}>
          {status.isInstalled && `v${status.version}`}
          <CoreUpdateModal>
            <Button size="sm" isIconOnly variant="ghost" className="size-5">
              <Cog6ToothIcon className="size-4" />
            </Button>
          </CoreUpdateModal>
        </InfoRow>
        <InfoRow label="Uptime"></InfoRow>
      </Card.Content>
    </Card>
  );
}

XrayOptions.Skeleton = () => (
  <Card className={GRID_COLS}>
    <Card.Header>
      <Card.Title className="min-h-8 flex justify-between items-center gap-x-3">
        <Skeleton className="h-4 my-1 w-25" />
        <Skeleton className="h-5 rounded-full w-10" />
      </Card.Title>
    </Card.Header>
  </Card>
);

export default XrayOptions;
