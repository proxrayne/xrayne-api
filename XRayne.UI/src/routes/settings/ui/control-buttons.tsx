import { RefreshCcwIcon } from "lucide-react";
import { Portal } from "radix-ui";
import { useEffect, useState } from "react";
import { useFormContext, useFormState } from "react-hook-form";

import { Button } from "@core/ui/button";
import { ButtonGroup } from "@core/ui/button-group";
import { Spinner } from "@core/ui/spinner";

import { FORM_ID, FormValues } from "../lib/constants";

interface Props {
  isLoading: boolean;
  pendingRestart: boolean;
}

function ControlButtons({ isLoading, pendingRestart }: Props) {
  const [node, setNode] = useState<Element | null>(null);

  useEffect(() => {
    setNode(document.getElementById(ControlButtons.PortalId));
  }, []);

  const { reset } = useFormContext<FormValues>();
  const { isSubmitting } = useFormState<FormValues>();

  if (pendingRestart) {
    return <Button onClick={() => {}}>Restart panel</Button>;
  }

  const disabled = isSubmitting || isLoading;

  return (
    <Portal.Root container={node}>
      <ButtonGroup>
        <Button variant="outline" disabled={disabled} onClick={() => reset()}>
          <RefreshCcwIcon />
          Reset
        </Button>
        <Button type="submit" form={FORM_ID} disabled={disabled}>
          {isSubmitting && <Spinner />}
          Save changes
        </Button>
      </ButtonGroup>
    </Portal.Root>
  );
}

ControlButtons.PortalId = "control-buttons";

export default ControlButtons;
