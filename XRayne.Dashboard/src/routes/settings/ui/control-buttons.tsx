import { RefreshCcwIcon } from "lucide-react";
import { Portal } from "radix-ui";
import { useEffect, useState } from "react";
import { useFormContext, useFormState } from "react-hook-form";
import isEqual from "lodash/isEqual";
import { useMutation } from "@tanstack/react-query";
import { toast } from "sonner";

import { Button } from "@core/ui/button";
import { ButtonGroup } from "@core/ui/button-group";
import { Spinner } from "@core/ui/spinner";

import { restartPanel, usePanelSettings } from "@features/settings";

import { FORM_ID, FormValues } from "../lib/constants";
import { buildPanelUrl, waitRestart } from "../lib/helpers";

interface Props {
  isLoading: boolean;
  pendingRestart: boolean;
}

function ControlButtons({ isLoading, pendingRestart }: Props) {
  const [node, setNode] = useState<Element | null>(null);
  const [isChanged, setIsChanged] = useState(false);

  useEffect(() => {
    setNode(document.getElementById(ControlButtons.PortalId));
  }, []);

  const { reset, watch, formState } = useFormContext<FormValues>();
  const { isSubmitting } = useFormState<FormValues>();

  useEffect(() => {
    const { unsubscribe } = watch((values) => {
      setIsChanged(!isEqual(values, formState.defaultValues));
    });

    return unsubscribe;
  }, [watch]);

  if (pendingRestart) {
    return (
      <Portal.Root container={node}>
        <RestartButton />
      </Portal.Root>
    );
  }

  const disabled = isSubmitting || isLoading || !isChanged;

  return (
    <Portal.Root container={node}>
      <ButtonGroup>
        <Button variant="outline" disabled={disabled} onClick={() => reset()}>
          <RefreshCcwIcon />
          Reset
        </Button>
        <Button
          type="submit"
          form={FORM_ID}
          variant={isChanged ? "default" : "secondary"}
          disabled={disabled}
        >
          {isSubmitting && <Spinner />}
          Save changes
        </Button>
      </ButtonGroup>
    </Portal.Root>
  );
}

function RestartButton() {
  const { mutateAsync, isPending } = useMutation({
    mutationKey: ["panel", "restart"],
    mutationFn: async () => {
      await restartPanel();
      await waitRestart(async () => {
        const { settings } = await usePanelSettings.fetch();

        const nextUrl = buildPanelUrl(settings).toString();
        if (nextUrl !== location.href) {
          location.href = nextUrl;
        }
      }, 30_000);
    },
    onError: (error) => {
      toast.error(error?.message || "");
    },
  });

  return (
    <Button
      variant="destructive"
      onClick={() => mutateAsync()}
      disabled={isPending}
    >
      {isPending && <Spinner />}
      Restart panel
    </Button>
  );
}

ControlButtons.PortalId = "control-buttons";

export default ControlButtons;
