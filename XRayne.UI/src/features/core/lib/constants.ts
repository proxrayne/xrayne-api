import { createContext } from "react";

import { StreamPullingResult } from "@core/hooks/use-stream";

export const CoreStatusContext = createContext(
  {} as StreamPullingResult<CoreStatus>,
);
