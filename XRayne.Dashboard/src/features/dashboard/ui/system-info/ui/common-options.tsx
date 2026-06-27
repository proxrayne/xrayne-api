import { formatDuration } from "date-fns";

import { Card, CardContent, CardHeader, CardTitle } from "@core/ui/card";
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
      <CardHeader className="flex-row justify-between items-center gap-x-3">
        <CardTitle className="font-semibold">System info</CardTitle>
      </CardHeader>
      <CardContent>
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
      </CardContent>
    </Card>
  );
}

export default CommonOptions;
