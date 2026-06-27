import { ReactElement } from "react";
import { useForm } from "react-hook-form";

import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@core/ui/dialog";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@core/ui/tabs";
import { Button } from "@core/ui/button";
import { FieldSet } from "@core/ui/field";

import { Protocol } from "@libs/xray-config";

import Basic from "./ui/basic";

import VlessFieldSet from "./ui/protocols/vless-fieldset";
import trojanFieldSet from "./ui/protocols/trojan-fieldset";
import ShadowSocksFieldset from "./ui/protocols/shadowsocks-fieldset";
import SocksFieldset from "./ui/protocols/socks-fieldset";
import Sniffing from "./ui/sniffing";

const PROTOCOL_COMPONENTS = {
  [Protocol.Vless]: VlessFieldSet,
  [Protocol.Trojan]: trojanFieldSet,
  [Protocol.ShadowSocks]: ShadowSocksFieldset,
  [Protocol.Socks]: SocksFieldset,
};

interface Props {
  children: ReactElement;
}

function UpsetInboundDialog({ children }: Props) {
  const {} = useForm({});

  const ProtocolComponent = PROTOCOL_COMPONENTS["socks"];

  const hasProtocols = Boolean(ProtocolComponent);

  return (
    <Dialog>
      <DialogTrigger asChild>{children}</DialogTrigger>
      <DialogContent className="md:max-w-2xl! **:data-[slot='field-content']:justify-center **:data-[slot='field-content']:flex-auto md:**:data-[slot='field-content']:max-w-64 md:**:data-[slot='field']:flex-row **:data-[slot='field-set']:px-1">
        <Tabs defaultValue="basic">
          <DialogHeader className="gap-y-2 mb-4">
            <DialogTitle>Add inbound</DialogTitle>
            <TabsList className="w-full mt-3">
              <TabsTrigger value="basic">Basic</TabsTrigger>
              {hasProtocols && <TabsTrigger value="protocol">Protocol</TabsTrigger>}
              <TabsTrigger value="stream">Stream</TabsTrigger>
              <TabsTrigger value="sniffing">Sniffing</TabsTrigger>
            </TabsList>
          </DialogHeader>

          <TabsContent value="basic" asChild>
            <Basic />
          </TabsContent>
          {hasProtocols && (
            <TabsContent value="protocol" asChild>
              <FieldSet>
                <ProtocolComponent />
              </FieldSet>
            </TabsContent>
          )}
          <TabsContent value="stream">Stream</TabsContent>
          <TabsContent value="sniffing" asChild>
            <Sniffing />
          </TabsContent>
        </Tabs>
        <DialogFooter>
          <DialogClose asChild>
            <Button variant="secondary" size="lg">
              Close
            </Button>
          </DialogClose>
          <Button type="submit" size="lg">
            Save values
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

export default UpsetInboundDialog;
