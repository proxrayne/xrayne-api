import { PlusIcon, Trash2Icon } from "lucide-react";

import { Field, FieldContent, FieldHeader, FieldLabel } from "@core/ui/field";
import { Button } from "@core/ui/button";
import { Input } from "@core/ui/input";
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@core/ui/select";

function Fallbacks() {
  return (
    <>
      <Field>
        <FieldLabel asChild>
          <p>Fallbacks</p>
        </FieldLabel>
        <FieldContent>
          <Button size="sm" variant="secondary" className="w-fit ml-auto">
            <PlusIcon />
            Add
          </Button>
        </FieldContent>
      </Field>

      <div className="flex flex-col gap-y-1">
        <FallbackItem index={0} />
      </div>
    </>
  );
}

interface ItemProps {
  index: number;
}

function FallbackItem({ index }: ItemProps) {
  return (
    <div>
      <div className="mb-3 flex items-center gap-x-3 justify-between px-3">
        <p className="font-medium">Fallback {index + 1}</p>
        <Button size="icon-sm" variant="destructive">
          <Trash2Icon />
        </Button>
      </div>
      <div className="p-4 bg-muted/70 rounded-xl flex flex-col gap-y-2">
        <Field>
          <FieldHeader>
            <FieldLabel htmlFor="server-name-input">
              Server name (SNI)
            </FieldLabel>
          </FieldHeader>
          <FieldContent>
            <Input id="server-name-input" placeholder="Empty for ignore" />
          </FieldContent>
        </Field>

        <Field>
          <FieldHeader>
            <FieldLabel asChild>
              <span>ALPN</span>
            </FieldLabel>
          </FieldHeader>
          <FieldContent>
            <Select>
              <SelectTrigger className="w-full">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectGroup>
                  <SelectItem value="any">any</SelectItem>
                  <SelectItem value="h2">h2</SelectItem>
                  <SelectItem value="http/1.1">http/1.1</SelectItem>
                </SelectGroup>
              </SelectContent>
            </Select>
          </FieldContent>
        </Field>

        <Field>
          <FieldHeader>
            <FieldLabel htmlFor="path-input">Path</FieldLabel>
          </FieldHeader>
          <FieldContent>
            <Input
              id="path-input"
              placeholder="Empty for ignore or /you-path"
            />
          </FieldContent>
        </Field>

        <Field>
          <FieldHeader>
            <FieldLabel htmlFor="dest-input">Destination</FieldLabel>
          </FieldHeader>
          <FieldContent>
            <Input id="dest-input" placeholder="Port or host or socket path" />
          </FieldContent>
        </Field>

        <Field>
          <FieldHeader>
            <FieldLabel asChild>
              <span>Proxy</span>
            </FieldLabel>
          </FieldHeader>
          <FieldContent>
            <Select>
              <SelectTrigger className="w-full">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectGroup>
                  <SelectItem value="0">off</SelectItem>
                  <SelectItem value="1">v1</SelectItem>
                  <SelectItem value="2">v2</SelectItem>
                </SelectGroup>
              </SelectContent>
            </Select>
          </FieldContent>
        </Field>
      </div>
    </div>
  );
}

export default Fallbacks;
