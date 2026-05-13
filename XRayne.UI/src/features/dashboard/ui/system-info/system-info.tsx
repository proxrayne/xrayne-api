import prettyBytes from "pretty-bytes";
import { CircleAlertIcon } from "lucide-react";

import Placeholder from "@core/ui/placeholder";
import { Skeleton } from "@core/ui/skeleton";
import { Button } from "@core/ui/button";
import ColoredIcon from "@core/ui/colored-icon";

import XrayOptions from "./ui/xray-options/xray-options";
import CommonOptions from "./ui/common-options";
import UsedCard from "./ui/used-card";

import { useSystemStats } from "./lib/query";

function SystemInfo() {
  const { stats, isLoaded, error, refetch } = useSystemStats();

  return (
    <div className="grid grid-cols-4 gap-x-2 gap-y-3 md:gap-4">
      {(() => {
        if (!isLoaded) {
          return (
            <>
              {Array.from({ length: 4 }).map((_, i) => (
                <Skeleton
                  className="h-56 rounded-3xl col-span-4 sm:col-span-2 lg:col-span-1 "
                  key={i}
                />
              ))}
              {Array.from({ length: 2 }).map((_, i) => (
                <Skeleton
                  className="h-40 rounded-3xl col-span-4 md:col-span-2"
                  key={i}
                />
              ))}
            </>
          );
        }

        if (error || !stats) {
          return (
            <Placeholder className="col-span-full">
              <ColoredIcon asChild variant="danger">
                <Placeholder.Media>
                  <CircleAlertIcon />
                </Placeholder.Media>
              </ColoredIcon>
              <Placeholder.Header>Failure loading stats</Placeholder.Header>
              <Placeholder.Subheader>
                Unhandled error. Please, try to reload latter.
              </Placeholder.Subheader>
              <Placeholder.Actions>
                <Button onClick={() => refetch()}>Reload</Button>
              </Placeholder.Actions>
            </Placeholder>
          );
        }

        const { cpu, memory, swap, storage } = stats;

        return (
          <>
            <UsedCard
              percent={cpu.averageUsagePercent}
              footer={`CPU: ${cpu.cores.length} cores`}
              subheader="Average value"
              label="CPU Usage"
            />
            <UsedCard
              percent={memory.usedBytes / (memory.totalBytes / 100)}
              footer={`RAM: ${prettyBytes(memory.totalBytes)}`}
              subheader={`Used ${prettyBytes(memory.usedBytes)}`}
              label="RAM Usage"
            />
            <UsedCard
              percent={swap.usedBytes / (swap.totalBytes / 100)}
              footer={`SWAP: ${prettyBytes(swap.totalBytes)}`}
              subheader={`Used ${prettyBytes(swap.usedBytes)}`}
              label="SWAP Usage"
            />
            <UsedCard
              percent={storage.applicationDirectoryUsedDiskPercent}
              footer="Disk usage"
              subheader={`Used ${prettyBytes(storage.applicationDirectory.sizeBytes)}`}
              label="Disk usage"
            />
            <CommonOptions stats={stats} />
            <XrayOptions />
          </>
        );
      })()}
    </div>
  );
}

export default SystemInfo;
