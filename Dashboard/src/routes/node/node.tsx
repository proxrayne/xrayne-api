import { Trash2Icon } from "lucide-react";
import { useParams } from "react-router";

import { Alert, AlertDescription, AlertTitle } from "@core/ui/alert";
import { Badge } from "@core/ui/badge";
import Page from "@core/ui/page";
import { Skeleton } from "@core/ui/skeleton";
import { Button } from "@core/ui/button";

import { useNodeQuery } from "@features/node";

import DeleteNodeDialog from "./ui/delete-node-dialog";

function NodeProfile() {
  const params = useParams();
  const nodeId = Number(params.nodeId);
  const isValidNodeId = Number.isFinite(nodeId);
  const nodeQuery = useNodeQuery(nodeId);

  if (!isValidNodeId) {
    return (
      <Page>
        <Page.Header>
          <Page.Title>Node profile</Page.Title>
        </Page.Header>
        <Alert variant="destructive">
          <AlertTitle>Invalid node</AlertTitle>
          <AlertDescription>The requested node id is not valid.</AlertDescription>
        </Alert>
      </Page>
    );
  }

  return (
    <Page>
      <Page.Header>
        <Page.Title>{nodeQuery.data?.name ?? "Node profile"}</Page.Title>
        {nodeQuery.data ? (
          <Page.Toolbar>
            <DeleteNodeDialog node={nodeQuery.data}>
              <Button variant="destructive">
                <Trash2Icon />
                Delete
              </Button>
            </DeleteNodeDialog>
          </Page.Toolbar>
        ) : null}
      </Page.Header>

      {nodeQuery.isLoading ? (
        <div className="space-y-3">
          <Skeleton className="h-9 w-64" />
          <Skeleton className="h-24 w-full" />
        </div>
      ) : nodeQuery.isError ? (
        <Alert variant="destructive">
          <AlertTitle>Unable to load node</AlertTitle>
          <AlertDescription>
            {nodeQuery.error?.message || "Node profile is unavailable."}
          </AlertDescription>
        </Alert>
      ) : nodeQuery.data ? (
        <div className="text-sm text-muted-foreground">
          <div className="flex flex-wrap items-center gap-2">
            <Badge variant="outline">{nodeQuery.data.status}</Badge>
            <span>
              {nodeQuery.data.address}:{nodeQuery.data.apiPort}
            </span>
          </div>
        </div>
      ) : null}
    </Page>
  );
}

export default NodeProfile;
