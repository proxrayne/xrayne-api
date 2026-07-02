import { PropsWithChildren } from "react";

import { useStreamPulling } from "@core/hooks/use-stream";

import { CoreStatusContext } from "../lib/constants";

function CoreStatusProvider({ children }: PropsWithChildren) {
  const stream = useStreamPulling<CoreStatus>("core/status/stream");

  return <CoreStatusContext.Provider value={stream}>{children}</CoreStatusContext.Provider>;
}

export default CoreStatusProvider;
