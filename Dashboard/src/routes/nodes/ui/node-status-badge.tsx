import { CircleAlertIcon, CircleCheckIcon, CircleDashedIcon, CircleOffIcon } from "lucide-react";

import { Badge } from "@core/ui/badge";
import { cn } from "@core/lib/utils";

import type { NodeStatus } from "../lib/types";

interface Props {
  status: NodeStatus;
}

const STATUS_STYLES: Record<NodeStatus, string> = {
  connected: "bg-green-400/15 text-green-500 dark:text-green-400",
  connecting: "bg-blue-400/15 text-blue-500 dark:text-blue-400",
  disabled: "bg-muted text-muted-foreground",
  error: "bg-red-400/15 text-red-500 dark:text-red-400",
};

const STATUS_ICONS = {
  connected: CircleCheckIcon,
  connecting: CircleDashedIcon,
  disabled: CircleOffIcon,
  error: CircleAlertIcon,
} satisfies Record<NodeStatus, typeof CircleCheckIcon>;

const STATUS_LABELS: Record<NodeStatus, string> = {
  connected: "Connected",
  connecting: "Connecting",
  disabled: "Disabled",
  error: "Error",
};

function NodeStatusBadge({ status }: Props) {
  const Icon = STATUS_ICONS[status];

  return (
    <Badge variant="secondary" className={cn("capitalize", STATUS_STYLES[status])}>
      <Icon />
      {STATUS_LABELS[status]}
    </Badge>
  );
}

export default NodeStatusBadge;
