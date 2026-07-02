import { formatDistanceToNow } from "date-fns";
import { FileKeyIcon, KeyRoundIcon, MoreVerticalIcon, ServerIcon } from "lucide-react";

import { Badge } from "@core/ui/badge";
import { Button } from "@core/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@core/ui/card";
import { cn } from "@core/lib/utils";
import { Skeleton } from "@core/ui/skeleton";

import type { NodeModel } from "../lib/types";
import NodeStatusBadge from "./node-status-badge";

interface Props {
  node: NodeModel;
  onOpen?: (node: NodeModel) => void;
}

interface DetailRowProps {
  label: string;
  value: string;
}

function DetailRow({ label, value }: DetailRowProps) {
  return (
    <div className="flex min-w-0 items-end gap-2">
      <p className="shrink-0 text-xs text-muted-foreground">{label}</p>
      <span className="mb-1 h-px min-w-4 flex-1 border-b border-dotted border-muted" />
      <p className="min-w-0 truncate text-xs font-medium" title={value}>
        {value}
      </p>
    </div>
  );
}

function NodeCard({ node, onOpen }: Props) {
  const lastStatusChange = formatDistanceToNow(new Date(node.lastStatusChange), {
    addSuffix: true,
  });

  return (
    <Card
      className={cn(
        "col-span-12 md:col-span-6 xl:col-span-4",
        onOpen && "cursor-pointer transition-colors hover:bg-card/80",
      )}
      onClick={() => onOpen?.(node)}
      onKeyDown={(event) => {
        if ((event.key === "Enter" || event.key === " ") && onOpen) {
          event.preventDefault();
          onOpen(node);
        }
      }}
      role={onOpen ? "link" : undefined}
      size="sm"
      tabIndex={onOpen ? 0 : undefined}
    >
      <CardHeader className="grid-cols-[1fr_auto] gap-3">
        <div className="flex min-w-0 items-start gap-3">
          <div className="flex size-10 shrink-0 items-center justify-center rounded-2xl bg-blue-400/15 text-blue-500 dark:text-blue-400">
            <ServerIcon className="size-5" />
          </div>
          <div className="min-w-0 flex-1">
            <CardTitle className="truncate font-semibold" title={node.name}>
              {node.name}
            </CardTitle>
            <CardDescription className="truncate" title={`${node.address}:${node.port}`}>
              {node.address}:{node.port}
            </CardDescription>
          </div>
        </div>
        <div className="col-start-2 row-start-1 self-start justify-self-end">
          <Button
            type="button"
            size="icon"
            variant="ghost"
            aria-label={`Open ${node.name} actions`}
            onClick={(event) => event.stopPropagation()}
          >
            <MoreVerticalIcon />
          </Button>
        </div>
      </CardHeader>

      <CardContent className="space-y-4">
        <div className="grid grid-cols-2 gap-2">
          <DetailRow label="API" value={String(node.apiPort)} />
          <DetailRow label="Xray" value={node.xrayVersion ?? "n/a"} />
          <DetailRow
            label="Auth"
            value={node.authType === "privateKey" ? "Private key" : "Password"}
          />
          <DetailRow label="Status" value={lastStatusChange} />
        </div>

        {(node.note || node.message) && (
          <div className="space-y-2">
            {node.message && (
              <p
                className={cn(
                  "line-clamp-2 rounded-2xl px-3 py-2 text-xs",
                  node.status === "error"
                    ? "bg-red-400/10 text-red-500 dark:text-red-400"
                    : "bg-blue-400/10 text-blue-500 dark:text-blue-400",
                )}
              >
                {node.message}
              </p>
            )}
            {node.note && (
              <p className="line-clamp-2 rounded-2xl bg-muted/60 px-3 py-2 text-xs text-muted-foreground">
                {node.note}
              </p>
            )}
          </div>
        )}

        <div className="flex items-end justify-between gap-3">
          <div className="flex min-w-0 flex-wrap gap-2">
            <Badge variant="outline">#{node.id}</Badge>
            <Badge variant="outline">
              {node.authType === "privateKey" ? <FileKeyIcon /> : <KeyRoundIcon />}
              SSH
            </Badge>
          </div>
          <NodeStatusBadge status={node.status} />
        </div>
      </CardContent>
    </Card>
  );
}

function NodeCardSkeleton() {
  return (
    <Card className="col-span-12 md:col-span-6 xl:col-span-4" size="sm">
      <CardHeader className="grid-cols-[1fr_auto] gap-3">
        <div className="flex min-w-0 items-start gap-3">
          <Skeleton className="size-10 shrink-0 rounded-2xl" />
          <div className="min-w-0 flex-1 space-y-2">
            <Skeleton className="h-4 w-40 max-w-full" />
            <Skeleton className="h-3 w-52 max-w-full" />
          </div>
        </div>
        <Skeleton className="size-8 rounded-2xl" />
      </CardHeader>

      <CardContent className="space-y-4">
        <div className="grid grid-cols-2 gap-2">
          {Array.from({ length: 4 }).map((_, index) => (
            <div className="flex min-w-0 items-end gap-2" key={index}>
              <Skeleton className="h-3 w-10 shrink-0" />
              <Skeleton className="mb-1 h-px min-w-4 flex-1" />
              <Skeleton className="h-3 w-16" />
            </div>
          ))}
        </div>

        <div className="space-y-2">
          <Skeleton className="h-8 w-full rounded-2xl" />
          <Skeleton className="h-8 w-4/5 rounded-2xl" />
        </div>

        <div className="flex items-end justify-between gap-3">
          <div className="flex min-w-0 flex-wrap gap-2">
            <Skeleton className="h-5 w-10 rounded-3xl" />
            <Skeleton className="h-5 w-14 rounded-3xl" />
          </div>
          <Skeleton className="h-5 w-24 rounded-3xl" />
        </div>
      </CardContent>
    </Card>
  );
}

export default Object.assign(NodeCard, {
  Skeleton: NodeCardSkeleton,
});
