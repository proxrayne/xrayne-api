import { useContext } from "react";

import { CoreStatusContext } from "./constants";

export function useCoreStatusContext() {
  return useContext(CoreStatusContext);
}

export function useCoreStatus() {
  const context = useCoreStatusContext();

  return context.data;
}
