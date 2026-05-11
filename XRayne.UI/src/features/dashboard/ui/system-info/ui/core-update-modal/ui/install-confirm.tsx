import {
  Alert,
  Button,
  Link,
  Modal,
  Spinner,
  WarningIcon,
} from "@heroui/react";

import {
  CheckCircleIcon,
  ExclamationCircleIcon,
  QuestionMarkCircleIcon,
} from "@heroicons/react/16/solid";

import Placeholder from "@core/ui/placeholder";
import ColoredIcon from "@core/ui/colored-icon";
import { compareVersions } from "@core/lib/core";

import {
  GitHubReleaseDto,
  useCoreInstall,
  useCoreInstallingStatus,
} from "@features/core";

interface InstallConfirmProps {
  isUpdate: boolean;
  currentVersion: string | null;
  release: GitHubReleaseDto;
}

function InstallConfirm({ release, currentVersion }: InstallConfirmProps) {
  const [install, { isPending, data, error }] = useCoreInstall(release.tagName);
  const { status, error: statusError } = useCoreInstallingStatus(data ?? null);

  if (error || statusError || status?.step === "failure") {
    return (
      <Placeholder>
        <ColoredIcon variant="danger" asChild>
          <Placeholder.Media>
            <ExclamationCircleIcon />
          </Placeholder.Media>
        </ColoredIcon>
        <Placeholder.Header>Installation failure</Placeholder.Header>
        <Placeholder.Subheader>
          Please check logs for more details
        </Placeholder.Subheader>
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
        <Modal.Footer className="mt-2 max-sm:[&>button]:w-full">
          <Button variant="secondary" slot="close">
            Close
          </Button>
        </Modal.Footer>
      </>
    );
  }

  if (!data) {
    return (
      <>
        <Placeholder>
          <ColoredIcon
            variant={release.prerelease ? "warning" : "accent"}
            asChild
          >
            <Placeholder.Media>
              {release.prerelease ? (
                <WarningIcon />
              ) : (
                <QuestionMarkCircleIcon />
              )}
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
              <Link className="text-xs" target="_blank" href={release.htmlUrl}>
                release changelog
              </Link>{" "}
              before installing.
            </p>
          </Placeholder.Subheader>
        </Placeholder>
        {(!currentVersion ||
          compareVersions(release.tagName, currentVersion) === -1) && (
          <Alert
            status="warning"
            className="shadow-none bg-warning/10 -mt-4 mb-6 gap-x-2"
          >
            <Alert.Indicator />
            <Alert.Content>
              <Alert.Title>Please note</Alert.Title>
              <Alert.Description>
                Older versions may not support current settings.
              </Alert.Description>
            </Alert.Content>
          </Alert>
        )}
        <Modal.Footer className="mt-2 max-sm:[&>button]:w-full">
          <Button
            variant="primary"
            onClick={() => install()}
            isPending={isPending}
          >
            Yes, install {release.tagName}
          </Button>
        </Modal.Footer>
      </>
    );
  }

  return (
    <Placeholder>
      <ColoredIcon variant="accent" asChild>
        <Placeholder.Media>
          <Spinner size="xl" className="flex" />
        </Placeholder.Media>
      </ColoredIcon>
      <Placeholder.Header>Installation in progress</Placeholder.Header>
      <Placeholder.Subheader>
        {status?.message ??
          `Requesting installation xray-core ${release.tagName}`}
      </Placeholder.Subheader>
    </Placeholder>
  );
}

export default InstallConfirm;
