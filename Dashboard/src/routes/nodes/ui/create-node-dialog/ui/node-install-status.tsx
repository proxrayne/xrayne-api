import { CheckCircle2Icon, KeyRoundIcon } from "lucide-react";

import type { NormalizedNodeProvisionStep } from "../lib/node-provision-step";

import { Alert, AlertDescription, AlertTitle } from "@core/ui/alert";
import { Button } from "@core/ui/button";
import { DialogFooter } from "@core/ui/dialog";
import { Spinner } from "@core/ui/spinner";

import type { CreateNodeResponse, NodeProvisionState } from "@features/node";

interface NodeInstallStatusProps {
  response: CreateNodeResponse;
  state: NodeProvisionState | null;
  step: NormalizedNodeProvisionStep | null;
}

function NodeInstallStatus({ response, state, step }: NodeInstallStatusProps) {
  return (
    <div className="space-y-5">
      <Alert>
        {step === "completed" ? <CheckCircle2Icon /> : <Spinner />}
        <AlertTitle>{response.node.name}</AlertTitle>
        <AlertDescription>
          {state?.message ?? "Waiting for provisioning to start."}
        </AlertDescription>
      </Alert>

      <div className="rounded-2xl bg-muted/50 p-4">
        <div className="flex items-center gap-2 text-sm font-medium">
          <KeyRoundIcon className="size-4 text-muted-foreground" />
          Installation status
        </div>
        <p className="mt-2 text-sm text-muted-foreground">
          {step ? `Current step: ${step}` : "Opening install stream."}
        </p>
      </div>

      <DialogFooter>
        <Button type="button" variant="secondary" disabled>
          Close
        </Button>
      </DialogFooter>
    </div>
  );
}

export default NodeInstallStatus;
