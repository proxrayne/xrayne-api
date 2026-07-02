import { AlertCircleIcon, FolderOpenIcon, PlusIcon, ServerIcon } from "lucide-react";
import { useNavigate } from "react-router";

import { useNodesQuery } from "@features/node";

import { urls } from "@core/lib/urls";
import { Alert, AlertDescription, AlertTitle } from "@core/ui/alert";
import { Button } from "@core/ui/button";
import ColoredIcon from "@core/ui/colored-icon";
import Page from "@core/ui/page";
import Placeholder from "@core/ui/placeholder";

import CreateNodeDialog from "./ui/create-node-dialog";
import NodeCard from "./ui/node-card";

function Nodes() {
  const navigate = useNavigate();
  const { data: nodes = [], error, isLoading, refetch } = useNodesQuery();

  return (
    <Page>
      <Page.Header>
        <Page.Title>Nodes</Page.Title>
        <Page.Toolbar>
          {isLoading ? (
            <div className="h-9 w-24 animate-pulse rounded-2xl bg-muted" />
          ) : (
            nodes.length > 0 && (
              <CreateNodeDialog>
                <Button type="button">
                  <PlusIcon />
                  Create
                </Button>
              </CreateNodeDialog>
            )
          )}
        </Page.Toolbar>
      </Page.Header>

      {error ? (
        <Alert variant="destructive">
          <AlertCircleIcon />
          <AlertTitle>Unable to load nodes</AlertTitle>
          <AlertDescription>
            <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
              <span>{error instanceof Error ? error.message : "Unexpected loading error."}</span>
              <Button type="button" onClick={() => void refetch()} variant="secondary">
                Retry
              </Button>
            </div>
          </AlertDescription>
        </Alert>
      ) : isLoading ? (
        <div className="grid grid-cols-12 gap-3 md:gap-4">
          {Array.from({ length: 6 }).map((_, index) => (
            <NodeCard.Skeleton key={index} />
          ))}
        </div>
      ) : nodes.length > 0 ? (
        <div className="grid grid-cols-12 gap-3 md:gap-4">
          {nodes.map((node) => (
            <NodeCard
              key={node.id}
              node={node}
              onOpen={() => navigate(urls.node(node.id).toString())}
            />
          ))}
        </div>
      ) : (
        <Placeholder className="flex-auto">
          <ColoredIcon asChild variant="secondary">
            <Placeholder.Media>
              <FolderOpenIcon />
            </Placeholder.Media>
          </ColoredIcon>
          <Placeholder.Header>No nodes found</Placeholder.Header>
          <Placeholder.Subheader>Add a node to connect remote Xray hosts</Placeholder.Subheader>
          <Placeholder.Actions>
            <CreateNodeDialog>
              <Button type="button">
                <ServerIcon />
                Add node
              </Button>
            </CreateNodeDialog>
          </Placeholder.Actions>
        </Placeholder>
      )}
    </Page>
  );
}

export default Nodes;
