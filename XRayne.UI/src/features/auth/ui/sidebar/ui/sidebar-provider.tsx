import { ReactNode, useMemo, useState } from "react";
import { useMediaQuery } from "@heroui/react";

import { SidebarContext } from "../lib/constants";

interface Props {
  children: ReactNode;
}

function SidebarProvider({ children }: Props) {
  const [open, setOpen] = useState(false);
  const isMobile = useMediaQuery("(max-width: 1279px)");

  const context = useMemo(
    () => ({ open, setOpen, isMobile }),
    [open, setOpen, isMobile],
  );

  return (
    <SidebarContext.Provider value={context}>
      {children}
    </SidebarContext.Provider>
  );
}

export default SidebarProvider;
