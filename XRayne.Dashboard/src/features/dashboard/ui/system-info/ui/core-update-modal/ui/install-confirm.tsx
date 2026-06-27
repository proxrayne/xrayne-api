import {
  CheckCircleIcon,
  CircleAlertIcon,
  CircleQuestionMarkIcon,
  TriangleAlertIcon,
} from "lucide-react";
import { useEffect } from "react";
import { useQueryClient } from "@tanstack/react-query";

import Placeholder from "@core/ui/placeholder";
import ColoredIcon from "@core/ui/colored-icon";
import { compareVersions } from "@core/lib/core";
import { Button } from "@core/ui/button";
import { DialogClose, DialogFooter } from "@core/ui/dialog";
import { Spinner } from "@core/ui/spinner";
import { Alert, AlertDescription, AlertTitle } from "@core/ui/alert";
import { useStreamPulling } from "@core/hooks/use-stream";

import { CoreInstallingStatus, GitHubReleaseDto, useCoreInstall } from "@features/core";

interface InstallConfirmProps {
  isUpdate: boolean;
  currentVersion: string | null;
  release: GitHubReleaseDto;
}

function InstallConfirm({ release, currentVersion }: InstallConfirmProps) {
  const [install, { isPending, data, error }] = useCoreInstall(release.tagName);
  const { data: status } = useStreamPulling<CoreInstallingStatus>(
    data ? `core/install/${data}/stream` : null,
  );

  const query = useQueryClient();
  useEffect(() => {
    if (status?.step !== "installed") {
      return;
    }

    query.invalidateQueries({ queryKey: ["core"] });
  }, [status?.step]);

  if (error || status?.step === "failure") {
    return (
      <Placeholder>
        <ColoredIcon variant="danger" asChild>
          <Placeholder.Media>
            <CircleAlertIcon />
          </Placeholder.Media>
        </ColoredIcon>
        <Placeholder.Header>Installation failure</Placeholder.Header>
        <Placeholder.Subheader>Please check logs for more details</Placeholder.Subheader>
        <Placeholder.Actions>
          <Button onClick={() => install()}>Try again</Button>
        </Placeholder.Actions>
      </Placeholder>
    );
  }

  if (status?.step === "installed") {
    return (
      <>
        <Placeholder>
          <ColoredIcon variant="success" asChild>
            <Placeholder.Media>
              <CheckCircleIcon />
            </Placeholder.Media>
          </ColoredIcon>
          <Placeholder.Header>Installation successful</Placeholder.Header>
          <Placeholder.Subheader>
            Version {release.tagName} of the xray-core is installed
          </Placeholder.Subheader>
        </Placeholder>
        <DialogFooter className="mt-2 max-sm:[&>button]:w-full">
          <DialogClose asChild>
            <Button variant="secondary">Close</Button>
          </DialogClose>
        </DialogFooter>
      </>
    );
  }

  if (!data) {
    return (
      <>
        <Placeholder>
          <ColoredIcon variant={release.prerelease ? "warning" : "accent"} asChild>
            <Placeholder.Media>
              {release.prerelease ? <TriangleAlertIcon /> : <CircleQuestionMarkIcon />}
            </Placeholder.Media>
          </ColoredIcon>
          <Placeholder.Header>
            Install {release.prerelease ? "pre-release" : "stable"} core
          </Placeholder.Header>
          <Placeholder.Subheader>
            <p>
              Do you really want to install <b>{release.tagName}</b> version?
            </p>
            <p>
              Please review the{" "}
              <a
                className="text-xs font-medium hover:underline"
                target="_blank"
                href={release.htmlUrl}
              >
                release changelog
              </a>{" "}
              before installing.
            </p>
          </Placeholder.Subheader>
        </Placeholder>
        {(!currentVersion || compareVersions(release.tagName, currentVersion) === -1) && (
          <Alert className="shadow-none bg-warning/10 -mt-4 mb-6 gap-x-2">
            <TriangleAlertIcon />
            <AlertTitle>Attention</AlertTitle>
            <AlertDescription className="font-medium">
              Older versions may not support current settings
            </AlertDescription>
          </Alert>
        )}
        <DialogFooter className="mt-2 max-sm:[&>button]:w-full">
          <Button onClick={() => install()} disabled={isPending}>
            Yes, install {release.tagName}
          </Button>
        </DialogFooter>
      </>
    );
  }

  return (
    <Placeholder>
      <ColoredIcon variant="accent" asChild>
        <Placeholder.Media>
          <Spinner className="flex size-10" />
        </Placeholder.Media>
      </ColoredIcon>
      <Placeholder.Header>Installation in progress</Placeholder.Header>
      <Placeholder.Subheader>
        {status?.message ?? `Requesting installation xray-core ${release.tagName}`}
      </Placeholder.Subheader>
    </Placeholder>
  );
}

export default InstallConfirm;
