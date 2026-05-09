import { Button, ButtonGroup, Card, Chip, Skeleton } from "@heroui/react";

import {
  ArrowDownTrayIcon,
  ArrowPathIcon,
  PlayIcon,
  StopIcon,
} from "@heroicons/react/16/solid";

import Placeholder from "@core/ui/placeholder";

import { useCoreStatus } from "@features/core";
import InfoRow from "./info-row";

function XrayOptions() {
  const { status, error, isLoaded, refetch } = useCoreStatus();

  if (!isLoaded) {
    return <XrayOptions.Skeleton />;
  }

  if (error || !status) {
    return (
      <Card className="col-span-3">
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
    <Card className="col-span-3">
      <Card.Header className="min-h-8 flex justify-between items-center gap-x-3 flex-row">
        <Card.Title>Xray info</Card.Title>
        {status.isInstalled ? (
          <ButtonGroup size="sm" variant="tertiary">
            {status.isStarted ? (
              <Button>
                <StopIcon />
                Stop
              </Button>
            ) : (
              <Button>
                <PlayIcon />
                Start
              </Button>
            )}
            <Button>
              <ButtonGroup.Separator />
              <ArrowPathIcon />
              Restart
            </Button>
          </ButtonGroup>
        ) : (
          <Button size="sm" variant="secondary">
            <ArrowDownTrayIcon />
            Install
          </Button>
        )}
      </Card.Header>
      <Card.Content>
        <InfoRow label="Core status" defaultValue="Not started">
          {status.isInstalled && `${status.isStarted ? "Working" : "Stoped"}`}
        </InfoRow>
        <InfoRow label="Core version">
          {status.isInstalled && `v${status.version}`}
        </InfoRow>
      </Card.Content>
    </Card>
  );
}

XrayOptions.Skeleton = () => (
  <Card className="col-span-3">
    <Card.Header>
      <Card.Title className="min-h-8 flex justify-between items-center gap-x-3">
        <Skeleton className="h-4 my-1 w-25" />
        <Skeleton className="h-5 rounded-full w-10" />
      </Card.Title>
    </Card.Header>
  </Card>
);

export default XrayOptions;
