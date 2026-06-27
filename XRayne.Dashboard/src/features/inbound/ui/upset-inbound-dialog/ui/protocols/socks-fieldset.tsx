import { Field, FieldContent, FieldHeader, FieldLabel } from "@core/ui/field";
import { Label } from "@core/ui/label";
import { RadioGroup, RadioGroupItem } from "@core/ui/radio-group";
import { Switch } from "@core/ui/switch";
import { Input } from "@core/ui/input";

import { SOCKS_AUTH } from "../../lib/constants";

function SocksFieldset() {
  return (
    <>
      <Field>
        <FieldHeader>
          <FieldLabel>Auth</FieldLabel>
        </FieldHeader>
        <FieldContent>
          <RadioGroup className="flex flex-row">
            {SOCKS_AUTH.map(({ label, value }) => {
              const inputId = `socks-${value}-input`;

              return (
                <div className="flex items-center gap-3" key={value}>
                  <RadioGroupItem id={inputId} value={value} />
                  <Label htmlFor={inputId}>{label}</Label>
                </div>
              );
            })}
          </RadioGroup>
        </FieldContent>
      </Field>

      <Field>
        <FieldHeader>
          <FieldLabel>UDP</FieldLabel>
        </FieldHeader>
        <FieldContent>
          <Switch />
        </FieldContent>
      </Field>

      <Field>
        <FieldHeader>
          <FieldLabel>UDP IP</FieldLabel>
        </FieldHeader>
        <FieldContent>
          <Input />
        </FieldContent>
      </Field>
    </>
  );
}

export default SocksFieldset;
