import { ReactElement, useState } from "react";

import CreateNodeDialogContent from "./ui/create-node-dialog-content";

import { Dialog, DialogContent, DialogTrigger } from "@core/ui/dialog";

interface CreateNodeDialogProps {
  children: ReactElement;
}

function CreateNodeDialog({ children }: CreateNodeDialogProps) {
  const [open, setOpen] = useState(false);
  const [canClose, setCanClose] = useState(true);

  const handleOpenChange = (nextOpen: boolean) => {
    if (!nextOpen && !canClose) {
      return;
    }

    setOpen(nextOpen);
    if (!nextOpen) {
      setCanClose(true);
    }
  };

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogTrigger asChild>{children}</DialogTrigger>
      <DialogContent className="max-h-[92vh] overflow-y-auto sm:max-w-3xl">
        {open && (
          <CreateNodeDialogContent
            onCanCloseChange={setCanClose}
            onClose={() => {
              setCanClose(true);
              setOpen(false);
            }}
          />
        )}
      </DialogContent>
    </Dialog>
  );
}

export default CreateNodeDialog;
