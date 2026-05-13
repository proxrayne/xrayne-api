import {
  CircleStopIcon,
  HardDriveDownloadIcon,
  PlayIcon,
  RefreshCwIcon,
} from "lucide-react";

import { Button } from "@core/ui/button";
import { ButtonGroup } from "@core/ui/button-group";

import { CoreStatusDto } from "@features/core";

import CoreUpdateModal from "../../core-update-modal";

function CoreControl({ isInstalled, isStarted }: CoreStatusDto) {
  if (!isInstalled) {
    return (
      <CoreUpdateModal>
        <Button size="sm" variant="secondary">
          <HardDriveDownloadIcon />
          Install
        </Button>
      </CoreUpdateModal>
    );
  }

  if (!isStarted) {
    return (
      <Button variant="secondary" size="sm">
        <PlayIcon />
        Start
      </Button>
    );
  }

  return (
    <ButtonGroup>
      <Button variant="secondary" size="sm">
        <CircleStopIcon />
        Stop
      </Button>
      <Button variant="secondary" size="sm">
        <RefreshCwIcon />
        Restart
      </Button>
    </ButtonGroup>
  );
}

export default CoreControl;
