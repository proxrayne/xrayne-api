import { ReactElement, useState } from "react";

import { Button } from "@core/ui/button";
import { CodeEditor } from "@core/ui/code-editor";
import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@core/ui/dialog";

import { xrayInboundSchemas } from "@libs/xray-scheme";

interface Props {
  children: ReactElement;
}

const defaultInboundValue = JSON.stringify(
  {
    tag: "inbound-1",
    listen: "0.0.0.0",
    port: 443,
    protocol: "vless",
    settings: {
      clients: [],
      decryption: "none",
    },
    streamSettings: {
      network: "tcp",
      security: "none",
    },
    sniffing: {
      enabled: true,
      destOverride: ["http", "tls"],
    },
  },
  null,
  2,
);

function InboundDialog({ children }: Props) {
  const [value, setValue] = useState(defaultInboundValue);
  const [hasValidationErrors, setHasValidationErrors] = useState(false);

  return (
    <Dialog>
      <DialogTrigger asChild>{children}</DialogTrigger>
      <DialogContent className="md:max-w-4xl!">
        <DialogHeader className="gap-y-2">
          <DialogTitle>Add inbound</DialogTitle>
        </DialogHeader>

        <CodeEditor
          value={value}
          onChange={(nextValue) => setValue(nextValue ?? "")}
          onValidate={(markers) => setHasValidationErrors(markers.length > 0)}
          schemas={xrayInboundSchemas}
          path="file:///xray-inbound.json"
          language="json"
          height={520}
        />

        <DialogFooter>
          <DialogClose asChild>
            <Button variant="secondary" size="lg">
              Close
            </Button>
          </DialogClose>
          <Button type="submit" size="lg" disabled={hasValidationErrors}>
            Save values
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

export default InboundDialog;
