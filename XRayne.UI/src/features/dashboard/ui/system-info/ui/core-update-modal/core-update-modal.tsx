import { ReactElement } from "react";
import { Modal } from "@heroui/react";

import { useCoreStatus } from "@features/core";

import DialogContent from "./ui/dialog-content";

interface Props {
  children: ReactElement;
}

function CoreUpdateModal({ children }: Props) {
  const { status } = useCoreStatus();

  return (
    <Modal>
      {children}
      {status && (
        <Modal.Backdrop>
          <Modal.Container scroll="outside">
            <DialogContent {...status} />
          </Modal.Container>
        </Modal.Backdrop>
      )}
    </Modal>
  );
}

export default CoreUpdateModal;
