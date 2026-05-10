import { Button, ButtonGroup } from "@heroui/react";

import {
  ArrowDownTrayIcon,
  ArrowPathIcon,
  PlayIcon,
  StopIcon,
} from "@heroicons/react/16/solid";

import { CoreStatusDto } from "@features/core";

import CoreUpdateModal from "../../core-update-modal";

function CoreControl({ isInstalled, isStarted }: CoreStatusDto) {
  if (!isInstalled) {
    return (
      <CoreUpdateModal>
        <Button size="sm" variant="secondary" className="h-8">
          <ArrowDownTrayIcon />
          Install
        </Button>
      </CoreUpdateModal>
    );
  }

  if (!isStarted) {
    return (
      <Button variant="tertiary" size="sm" className="h-8">
        <PlayIcon />
        Start
      </Button>
    );
  }

  return (
    <ButtonGroup size="sm" variant="tertiary" className="h-8">
      <Button>
        <StopIcon />
        Stop
      </Button>
      <Button>
        <ButtonGroup.Separator />
        <ArrowPathIcon />
        Restart
      </Button>
    </ButtonGroup>
  );
}

export default CoreControl;
