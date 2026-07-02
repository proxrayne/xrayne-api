import { FilterIcon, FolderOpenIcon, /* MoreVerticalIcon, */ PlusIcon } from "lucide-react";

import { Button } from "@core/ui/button";
import Page from "@core/ui/page";
import { ButtonGroup } from "@core/ui/button-group";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@core/ui/table";
import { Switch } from "@core/ui/switch";
import Placeholder from "@core/ui/placeholder";
import ColoredIcon from "@core/ui/colored-icon";

import { InboundDialog } from "@features/inbound";

function Inbounds() {
  return (
    <Page>
      <Page.Header>
        <Page.Title>Inbounds</Page.Title>
        <Page.Toolbar className="flex items-center gap-x-1">
          <ButtonGroup>
            <Button variant="secondary">
              <FilterIcon />
              <span className="max-sm:hidden">Filter</span>
            </Button>
            <InboundDialog>
              <Button variant="secondary">
                <PlusIcon />
                New
              </Button>
            </InboundDialog>
          </ButtonGroup>
        </Page.Toolbar>
      </Page.Header>

      <Placeholder className="flex-auto">
        <ColoredIcon asChild variant="secondary">
          <Placeholder.Media>
            <FolderOpenIcon />
          </Placeholder.Media>
        </ColoredIcon>
        <Placeholder.Header>Inbounds not found</Placeholder.Header>
        <Placeholder.Subheader>Create inbound to connect clients</Placeholder.Subheader>
        <Placeholder.Actions>
          <InboundDialog>
            <Button>
              <PlusIcon />
              Create inbound
            </Button>
          </InboundDialog>
        </Placeholder.Actions>
      </Placeholder>

      {/* <div className="bg-secondary/60 rounded-2xl px-1.5 pb-1.5">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>#ID</TableHead>
              <TableHead>Enable</TableHead>
              <TableHead>Tag</TableHead>
              <TableHead className="text-right">Port</TableHead>
              <TableHead className="text-right">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            <TableRow>
              <TableCell className="font-medium">#1</TableCell>
              <TableCell>
                <Switch />
              </TableCell>
              <TableCell>Tag 1</TableCell>
              <TableCell align="right">443</TableCell>
              <TableCell align="right">
                <Button size="icon" variant="ghost">
                  <MoreVerticalIcon />
                </Button>
              </TableCell>
            </TableRow>
          </TableBody>
        </Table>
      </div> */}
    </Page>
  );
}

export default Inbounds;
