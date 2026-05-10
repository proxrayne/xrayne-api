import { formatDuration } from "date-fns";
import { Card } from "@heroui/react";

import { parseDotnetTimeSpan } from "@core/lib/date";

import { SystemInfoDto } from "../lib/api.types";
import InfoRow from "./info-row";

interface Props {
  stats: SystemInfoDto;
}

function CommonOptions({
  stats: {
    network: { iPv4Addresses, iPv6Addresses },
    uptime,
  },
}: Props) {
  return (
    <Card className="col-span-4 md:col-span-2">
      <Card.Header className="min-h-8 flex-row justify-between items-center gap-x-3">
        <Card.Title className="font-semibold">System info</Card.Title>
      </Card.Header>
      <Card.Content>
        <InfoRow label="System uptime">
          {formatDuration(parseDotnetTimeSpan(uptime), {
            format: ["hours", "minutes"],
          })}
        </InfoRow>
        <InfoRow label="IPv4 addresses" defaultValue="n/a">
          {iPv4Addresses.length > 0 && iPv4Addresses.join(", ")}
        </InfoRow>
        <InfoRow label="IPv6 addresses" defaultValue="n/a">
          {iPv6Addresses.length > 0 && iPv6Addresses.join(", ")}
        </InfoRow>
      </Card.Content>
    </Card>
  );
}

export default CommonOptions;
