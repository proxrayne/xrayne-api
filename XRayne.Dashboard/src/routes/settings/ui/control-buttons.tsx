import { Portal } from "radix-ui";
import { useEffect, useState } from "react";
import { useMutation } from "@tanstack/react-query";
import { toast } from "sonner";

import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@core/ui/alert-dialog";
import { Button } from "@core/ui/button";
import { Spinner } from "@core/ui/spinner";

import { pingApi, restartPanel } from "@features/settings";

import { waitRestart } from "../lib/helpers";

function ControlButtons() {
  const [node, setNode] = useState<Element | null>(null);

  useEffect(() => {
    setNode(document.getElementById(ControlButtons.PortalId));
  }, []);

  return (
    <Portal.Root container={node}>
      <RestartButton />
    </Portal.Root>
  );
}

function RestartButton() {
  const { mutateAsync, isPending } = useMutation({
    mutationKey: ["panel", "restart"],
    mutationFn: async () => {
      await restartPanel();
      await waitRestart(async () => {
        await pingApi();
      }, 30_000);
    },
    onError: (error) => {
      toast.error(error?.message || "");
    },
  });

  return (
    <AlertDialog>
      <AlertDialogTrigger asChild>
        <Button variant="destructive" disabled={isPending}>
          {isPending && <Spinner />}
          Restart panel
        </Button>
      </AlertDialogTrigger>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Restart panel?</AlertDialogTitle>
          <AlertDialogDescription>
            The panel API will be temporarily unavailable while the process restarts.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel asChild>
            <Button variant="outline">Cancel</Button>
          </AlertDialogCancel>
          <AlertDialogAction asChild>
            <Button variant="destructive" disabled={isPending} onClick={() => mutateAsync()}>
              {isPending && <Spinner />}
              Restart
            </Button>
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}

ControlButtons.PortalId = "control-buttons";

export default ControlButtons;
