import { useContext } from "react";

import { SidebarContext } from "./constants";

export function useSidebar() {
  return useContext(SidebarContext);
}
