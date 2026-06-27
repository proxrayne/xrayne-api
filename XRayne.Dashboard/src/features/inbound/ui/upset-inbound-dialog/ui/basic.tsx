import {
  Field,
  FieldContent,
  FieldDescription,
  FieldHeader,
  FieldLabel,
  FieldSet,
} from "@core/ui/field";
import { Input } from "@core/ui/input";
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@core/ui/select";
import { Switch } from "@core/ui/switch";

import { PROTOCOL_OPTIONS } from "../lib/constants";

function Basic() {
  return (
    <FieldSet>
      <Field>
        <FieldHeader>
          <FieldLabel htmlFor="enable-switch">Enable</FieldLabel>
          <FieldDescription>Indicates whether the connection is active</FieldDescription>
        </FieldHeader>
        <FieldContent>
          <Switch id="enable-switch" />
        </FieldContent>
      </Field>

      <Field>
        <FieldHeader>
          <FieldLabel htmlFor="display-name-input">DisplayName</FieldLabel>
          <FieldDescription>The name displayed in the interface</FieldDescription>
        </FieldHeader>
        <FieldContent>
          <Input id="display-name-input" placeholder="Base inbound" />
        </FieldContent>
      </Field>

      <Field>
        <FieldHeader>
          <FieldLabel asChild>
            <span>Protocol</span>
          </FieldLabel>
          <FieldDescription>Incoming protocol</FieldDescription>
        </FieldHeader>
        <FieldContent>
          <Select defaultValue="vless">
            <SelectTrigger className="w-full">
              <SelectValue />
            </SelectTrigger>
            <SelectContent position="popper">
              <SelectGroup>
                {PROTOCOL_OPTIONS.map(({ label, value }) => (
                  <SelectItem value={value} key={value}>
                    {label}
                  </SelectItem>
                ))}
              </SelectGroup>
            </SelectContent>
          </Select>
        </FieldContent>
      </Field>

      <Field>
        <FieldHeader>
          <FieldLabel htmlFor="listen-input">Listen address</FieldLabel>
          <FieldDescription>Do not specify to listen any IPs</FieldDescription>
        </FieldHeader>
        <FieldContent>
          <Input id="listen-input" placeholder="0.0.0.0" />
        </FieldContent>
      </Field>

      <Field>
        <FieldHeader>
          <FieldLabel htmlFor="port-input">Listen port</FieldLabel>
          <FieldDescription>Listen port (single or range)</FieldDescription>
        </FieldHeader>
        <FieldContent>
          <Input id="port-input" placeholder="443" />
        </FieldContent>
      </Field>
    </FieldSet>
  );
}

export default Basic;
