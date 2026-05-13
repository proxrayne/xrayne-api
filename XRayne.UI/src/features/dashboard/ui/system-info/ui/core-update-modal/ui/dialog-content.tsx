import { useState } from "react";
import { ArrowLeftIcon } from "lucide-react";
import { Link } from "react-router";
import { AnimatePresence, motion } from "framer-motion";

import { CoreStatusDto, GitHubReleaseDto } from "@features/core";
import { DialogDescription, DialogHeader, DialogTitle } from "@core/ui/dialog";
import { Button } from "@core/ui/button";
import { Separator } from "@core/ui/separator";

import ChooseVersion from "./choose-version";
import InstallConfirm from "./install-confirm";

function DialogContent({ isInstalled, version }: CoreStatusDto) {
  const [selected, setSelected] = useState<GitHubReleaseDto | null>(null);

  return (
    <>
      <AnimatePresence initial={false} mode="popLayout">
        {selected ? (
          <motion.div
            key="install"
            initial={{ opacity: 0, x: 460 }}
            animate={{ opacity: 1, x: 0 }}
            exit={{ opacity: 0, x: 460 }}
            transition={{ duration: 0.28, ease: "easeOut" }}
          >
            <DialogHeader className="flex-row gap-2">
              <Button
                size="icon-sm"
                variant="ghost"
                className="-ml-2"
                onClick={() => setSelected(null)}
              >
                <ArrowLeftIcon />
              </Button>
              <DialogTitle className="flex items-center">
                Installing
              </DialogTitle>
            </DialogHeader>
            <InstallConfirm
              release={selected}
              currentVersion={version}
              isUpdate={isInstalled}
            />
          </motion.div>
        ) : (
          <motion.div
            key="choose"
            initial={{ opacity: 0, x: -460 }}
            animate={{ opacity: 1, x: 0 }}
            exit={{ opacity: 0, x: -460 }}
            transition={{ duration: 0.28, ease: "easeOut" }}
          >
            <DialogHeader>
              <DialogTitle>
                {isInstalled ? "Update" : "Install"} xray-core
              </DialogTitle>
              <DialogDescription className="text-foreground/90 mt-1">
                <p>
                  You can install the required version of xray-core on this
                  node.
                </p>
                <p>
                  All data received from{" "}
                  <Link
                    to="https://github.com/xtls/xray-core"
                    target="_blank"
                    className="text-xs font-medium hover:underline text-foreground"
                  >
                    official repository
                  </Link>
                  .
                </p>
              </DialogDescription>
            </DialogHeader>
            <Separator className="mt-3 mb-1" />
            <div className="-mx-2">
              <ChooseVersion version={version} onSelect={setSelected} />
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </>
  );
}

export default DialogContent;
