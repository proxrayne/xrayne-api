import { CircleStopIcon, HardDriveDownloadIcon, PlayIcon, RefreshCwIcon } from "lucide-react";

import { Button } from "@core/ui/button";
import { ButtonGroup } from "@core/ui/button-group";
import { Skeleton } from "@core/ui/skeleton";
import { Spinner } from "@core/ui/spinner";

import { useCoreStatus, useRestartCore, useStartCore, useStopCore } from "@features/core";

import CoreUpdateModal from "../../core-update-modal";

function CoreControl() {
  const status = useCoreStatus();
  if (!status) {
    return <Skeleton className="h-8 w-25 rounded-3xl" />;
  }

  if (!status.status) {
    return (
      <CoreUpdateModal>
        <Button size="sm" variant="secondary" disabled={status.isInstalling}>
          <HardDriveDownloadIcon />
          Install
        </Button>
      </CoreUpdateModal>
    );
  }

  if (["starting", "stopped"].includes(status.status)) {
    return <StartAction {...status} />;
  }

  return <StopAndRestartAction {...status} />;
}

function StartAction({ status }: CoreStatus) {
  const [start, { isPending }] = useStartCore();

  const isStarting = isPending || status === "starting";

  return (
    <Button variant="secondary" size="sm" onClick={() => start()} disabled={isStarting}>
      {isStarting ? <Spinner /> : <PlayIcon />}
      Start
    </Button>
  );
}

function StopAndRestartAction({ status }: CoreStatus) {
  const [stop, { isPending: isPendingStop }] = useStopCore();
  const [restart, { isPending: isPendingRestart }] = useRestartCore();

  const isStopping = isPendingStop || status === "starting";
  const isRestarting = isPendingRestart || status === "restarting";

  return (
    <ButtonGroup>
      <Button variant="secondary" size="sm" disabled={isStopping} onClick={() => stop()}>
        {isStopping ? <Spinner /> : <CircleStopIcon />}
        Stop
      </Button>
      <Button variant="secondary" size="sm" disabled={isRestarting} onClick={() => restart()}>
        {isRestarting ? <Spinner /> : <RefreshCwIcon />}
        Restart
      </Button>
    </ButtonGroup>
  );
}

export default CoreControl;
