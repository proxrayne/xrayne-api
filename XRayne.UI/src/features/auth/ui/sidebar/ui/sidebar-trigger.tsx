import { Button } from "@heroui/react";

import { EqualsIcon } from "@heroicons/react/16/solid";

import { useSidebar } from "../lib/hooks";

function SidebarTrigger() {
  const { isMobile, setOpen } = useSidebar();

  return (
    isMobile && (
      <Button
        size="md"
        variant="ghost"
        isIconOnly
        onClick={() => setOpen(true)}
      >
        <EqualsIcon className="size-5" />
      </Button>
    )
  );
}

export default SidebarTrigger;
