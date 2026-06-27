import { RefreshCcwIcon } from "lucide-react";

import { Field, FieldContent, FieldHeader, FieldLabel } from "@core/ui/field";
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@core/ui/select";
import {
  InputGroup,
  InputGroupButton,
  InputGroupInput,
} from "@core/ui/input-group";
import { randomShadowsocksPassword } from "@core/lib/crypto";
import { Switch } from "@core/ui/switch";

import { ENCRYPTION_METHODS } from "../../lib/constants";

function ShadowSocksFieldset() {
  return (
    <>
      <Field>
        <FieldHeader>
          <FieldLabel>Encryption method</FieldLabel>
        </FieldHeader>
        <FieldContent>
          <Select>
            <SelectTrigger className="w-full">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectGroup>
                {ENCRYPTION_METHODS.map(({ label, value }) => (
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
          <FieldLabel>Password</FieldLabel>
        </FieldHeader>
        <FieldContent>
          <InputGroup>
            <InputGroupInput />
            <InputGroupButton
              onClick={() => {
                console.log(randomShadowsocksPassword());
              }}
            >
              <RefreshCcwIcon />
            </InputGroupButton>
          </InputGroup>
        </FieldContent>
      </Field>

      <Field>
        <FieldHeader>
          <FieldLabel>Network</FieldLabel>
        </FieldHeader>
        <FieldContent>
          <Select>
            <SelectTrigger className="w-full">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectGroup>
                <SelectItem value="tcp,udp">TCP, UDP</SelectItem>
                <SelectItem value="tcp">TCP</SelectItem>
                <SelectItem value="udp">UDP</SelectItem>
              </SelectGroup>
            </SelectContent>
          </Select>
        </FieldContent>
      </Field>

      <Field>
        <FieldHeader>
          <FieldLabel>ivCheck</FieldLabel>
        </FieldHeader>
        <FieldContent>
          <Switch />
        </FieldContent>
      </Field>
    </>
  );
}

export default ShadowSocksFieldset;
