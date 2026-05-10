import { useEffect } from "react";
import { Alert, Button, Link, Modal, Surface, WarningIcon } from "@heroui/react";
import { useQueryClient } from "@tanstack/react-query";

import { CheckIcon } from "@heroicons/react/16/solid";

import Placeholder from "@core/ui/placeholder";

import {
  GitHubReleaseDto,
  useCoreInstall,
  useCoreInstallingStatus,
} from "@features/core";

import { compareVersions } from "../../xray-options/lib/helpers";

interface InstallConfirmProps {
  currentVersion: string | null;
  release: GitHubReleaseDto;
  onCancel(): void;
}

function InstallConfirm({
  release,
  currentVersion,
  onCancel,
}: InstallConfirmProps) {
  const [install, { isPending, isSuccess }] = useCoreInstall(release.tagName);
  const { status, isLoaded } = useCoreInstallingStatus({
    enabled: isSuccess,
  });

  const isProcessing =
    isPending ||
    ["Preparing", "Downloading", "Extracting", "SettingUp"].includes(
      status?.step ?? "Idle",
    );

  const isSuccessful =
    status?.step === "Version" && status.message === release.tagName.slice(1);

  const query = useQueryClient();
  useEffect(() => {
    if (isSuccessful) {
      query.invalidateQueries({ queryKey: ["core"] });
    }
  }, [isSuccessful, query]);

  if (isSuccessful) {
    return (
      <Placeholder>
        <Placeholder.Media className="p-4 rounded-3xl bg-accent/10 text-accent">
          <CheckIcon className="size-8" />
        </Placeholder.Media>
        <Placeholder.Header>Install successful</Placeholder.Header>
        <Placeholder.Subheader>
          Version {release.tagName} of the xray-core kernel is installed
        </Placeholder.Subheader>
      </Placeholder>
    );
  }

  return (
    <>
      <Placeholder>
        <Placeholder.Media className="p-5 rounded-3xl bg-warning/10 text-warning">
          <WarningIcon className="size-7" />
        </Placeholder.Media>
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
      {isLoaded && isSuccess ? (
        <Surface
          className="rounded-xl font-mono p-3 text-xs mb-4"
          variant="secondary"
        >
          {status?.message}
        </Surface>
      ) : (
        (!currentVersion ||
          compareVersions(release.tagName, currentVersion) === -1) && (
          <Alert
            status="warning"
            className="shadow-none bg-warning/10 -mt-2 mb-4"
          >
            <Alert.Indicator />
            <Alert.Content>
              <Alert.Title>Please note</Alert.Title>
              <Alert.Description>
                Older versions may not support current settings.
              </Alert.Description>
            </Alert.Content>
          </Alert>
        )
      )}
      <Modal.Footer className="mt-2 max-sm:flex-col-reverse max-sm:[&>button]:w-full">
        <Button variant="tertiary" onClick={onCancel}>
          Back
        </Button>
        <Button
          variant="primary"
          onClick={() => install()}
          isPending={isProcessing}
        >
          Install {release.prerelease && "pre-release"}
        </Button>
      </Modal.Footer>
    </>
  );
}

export default InstallConfirm;
