import { useState } from "react";
import { Link, Modal, Separator } from "@heroui/react";
import { AnimatePresence, motion } from "framer-motion";

import { CoreStatusDto, GitHubReleaseDto } from "@features/core";

import ChooseVersion from "./choose-version";
import InstallConfirm from "./install-confirm";

function DialogContent({ isInstalled, version }: CoreStatusDto) {
  const [selected, setSelected] = useState<GitHubReleaseDto | null>(null);

  return (
    <Modal.Dialog className="overflow-x-hidden">
      <Modal.CloseTrigger />

      <AnimatePresence initial={false} mode="popLayout">
        {!selected && (
          <motion.div
            key="choose"
            initial={{ opacity: 0, x: -460 }}
            animate={{ opacity: 1, x: 0 }}
            exit={{ opacity: 0, x: -460 }}
            transition={{ duration: 0.3, ease: "easeOut" }}
          >
            <Modal.Header>
              <Modal.Heading>
                {isInstalled ? "Update" : "Install"} xray-core
              </Modal.Heading>
              <div className="text-sm/normal text-foreground/90 -mt-1">
                <p>
                  You can install the required version of xray-core on this
                  node.
                </p>
                <p>
                  All data received from{" "}
                  <Link
                    href="https://github.com/xtls/xray-core"
                    target="_blank"
                    className="text-xs"
                  >
                    official repository
                  </Link>
                  .
                </p>
              </div>
            </Modal.Header>
            <Separator className="mt-3 mb-1" />
            <Modal.Body className="-mx-2">
              <ChooseVersion version={version} onSelect={setSelected} />
            </Modal.Body>
          </motion.div>
        )}
        {selected && (
          <motion.div
            key="install"
            initial={{ opacity: 0, x: 460 }}
            animate={{ opacity: 1, x: 0 }}
            exit={{ opacity: 0, x: 460 }}
            transition={{ duration: 0.3, ease: "easeOut" }}
          >
            <Modal.Header>
              <Modal.Heading>Installing</Modal.Heading>
            </Modal.Header>
            <InstallConfirm
              release={selected}
              currentVersion={version}
              onCancel={() => setSelected(null)}
            />
          </motion.div>
        )}
      </AnimatePresence>
    </Modal.Dialog>
  );
}

export default DialogContent;
