import { Dispatch, SetStateAction } from "react";

export interface SidebarContextType {
  open: boolean;
  isMobile: boolean;
  setOpen: Dispatch<SetStateAction<boolean>>;
}
