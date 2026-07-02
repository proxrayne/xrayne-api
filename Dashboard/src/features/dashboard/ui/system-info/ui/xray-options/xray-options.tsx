import { SettingsIcon } from "lucide-react";
import capitalize from "lodash/capitalize";

import { Card, CardContent, CardHeader, CardTitle } from "@core/ui/card";
import { Button } from "@core/ui/button";
import { Skeleton } from "@core/ui/skeleton";
import Placeholder from "@core/ui/placeholder";

import { useCoreStatusContext } from "@features/core";

import InfoRow from "../info-row";
import CoreUpdateModal from "../core-update-modal";
import CoreControl from "./ui/core-control";

const GRID_COLS = "col-span-4 md:col-span-2";

function XrayOptions() {
  const { data: status, error, connect } = useCoreStatusContext();

  if (!status) {
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
            <Button onClick={() => connect()}>Try reload</Button>
          </Placeholder.Actions>
        </Placeholder>
      </Card>
    );
  }

  return (
    <Card className={GRID_COLS}>
      <CardHeader className="min-h-8 flex justify-between items-center gap-x-3 flex-row">
        <CardTitle className="font-semibold">Xray info</CardTitle>
        <CoreControl />
      </CardHeader>
      <CardContent>
        <InfoRow label="Status" defaultValue="Not installed">
          {status.isInstalled && capitalize(status.status ?? "unknown")}
        </InfoRow>
        <InfoRow label="Version" classNames={{ content: "flex gap-x-1" }}>
          <CoreUpdateModal>
            <Button size="icon-sm" variant="ghost" className="size-5">
              <SettingsIcon className="size-4" />
            </Button>
          </CoreUpdateModal>
          {status.isInstalled && `v${status.version}`}
        </InfoRow>
        <InfoRow label="Uptime"></InfoRow>
      </CardContent>
    </Card>
  );
}

XrayOptions.Skeleton = () => (
  <Card className={GRID_COLS}>
    <CardHeader>
      <CardTitle className="min-h-8 flex justify-between items-center gap-x-3">
        <Skeleton className="h-4 my-1 w-25" />
        <Skeleton className="h-5 rounded-full w-10" />
      </CardTitle>
    </CardHeader>
  </Card>
);

export default XrayOptions;
