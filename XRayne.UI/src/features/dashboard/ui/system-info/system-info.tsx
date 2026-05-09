import { Button, Card, Skeleton, Surface, ToggleButton } from "@heroui/react";
import prettyBytes from "pretty-bytes";

import { ExclamationCircleIcon } from "@heroicons/react/16/solid";

import Placeholder from "@core/ui/placeholder";

import XrayOptions from "./ui/xray-options";
import CommonOptions from "./ui/common-options";
import UsedCard from "./ui/used-card";

import { useSystemStats } from "./lib/query";

function SystemInfo() {
  const { stats, isLoaded, error, refetch } = useSystemStats();

  return (
    <div className="grid grid-cols-4 md:grid-cols-6 gap-4">
      {(() => {
        if (!isLoaded) {
          return Array.from({ length: 3 }).map((_, i) => (
            <Skeleton className="h-56 rounded-3xl col-span-2" key={i} />
          ));
        }

        if (error || !stats) {
          return (
            <Surface className="rounded-4xl">
              <Placeholder className="col-span-6">
                <Placeholder.Media>
                  <ExclamationCircleIcon className="size-10" />
                </Placeholder.Media>
                <Placeholder.Header>Failure loading stats</Placeholder.Header>
                <Placeholder.Subheader>
                  Unhandled error. Please, try to reload latter.
                </Placeholder.Subheader>
                <Placeholder.Actions>
                  <Button variant="primary" onClick={() => refetch()}>
                    Reload
                  </Button>
                </Placeholder.Actions>
              </Placeholder>
            </Surface>
          );
        }

        const { cpu, memory, swap } = stats;

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

            <CommonOptions stats={stats} />

            <XrayOptions />
          </>
        );
      })()}
    </div>
  );
}

export default SystemInfo;
