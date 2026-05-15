import { ReactElement } from "react";

import { Dialog, DialogContent, DialogTrigger } from "@core/ui/dialog";

import { useCoreStatus } from "@features/core";

import Content from "./ui/dialog-content";

interface Props {
  children: ReactElement;
}

function CoreUpdateModal({ children }: Props) {
  const status = useCoreStatus();

  return (
    <Dialog>
      <DialogTrigger asChild>{children}</DialogTrigger>
      <DialogContent className="overflow-hidden">
        {status && <Content {...status} />}
      </DialogContent>
    </Dialog>
  );
}

export default CoreUpdateModal;
