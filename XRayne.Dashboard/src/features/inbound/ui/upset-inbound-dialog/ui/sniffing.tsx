import {
  Combobox,
  ComboboxChip,
  ComboboxChips,
  ComboboxChipsInput,
  ComboboxContent,
  ComboboxEmpty,
  ComboboxItem,
  ComboboxList,
  ComboboxValue,
  useComboboxAnchor,
} from "@core/ui/combobox";
import { Field, FieldContent, FieldHeader, FieldLabel, FieldSet } from "@core/ui/field";
import { Switch } from "@core/ui/switch";
import { Input } from "@core/ui/input";

import { TRAFFIC } from "../lib/constants";

function Sniffing() {
  const anchor = useComboboxAnchor();

  return (
    <FieldSet>
      <Field>
        <FieldHeader>
          <FieldLabel>Enabled</FieldLabel>
        </FieldHeader>
        <FieldContent>
          <Switch />
        </FieldContent>
      </Field>

      <Field>
        <FieldHeader>
          <FieldLabel>Traffic</FieldLabel>
        </FieldHeader>
        <FieldContent>
          <Combobox multiple autoHighlight items={TRAFFIC}>
            <ComboboxChips ref={anchor}>
              <ComboboxValue>
                {(values) => (
                  <>
                    {values.map((value: string) => (
                      <ComboboxChip key={value}>{value}</ComboboxChip>
                    ))}
                    <ComboboxChipsInput />
                  </>
                )}
              </ComboboxValue>
            </ComboboxChips>
            <ComboboxContent anchor={anchor}>
              <ComboboxEmpty>No items found.</ComboboxEmpty>
              <ComboboxList>
                {({ label, value }) => (
                  <ComboboxItem key={value} value={value}>
                    {label}
                  </ComboboxItem>
                )}
              </ComboboxList>
            </ComboboxContent>
          </Combobox>
        </FieldContent>
      </Field>

      <Field>
        <FieldHeader>
          <FieldLabel>Metadata only</FieldLabel>
        </FieldHeader>
        <FieldContent>
          <Switch />
        </FieldContent>
      </Field>

      <Field>
        <FieldHeader>
          <FieldLabel>Route only</FieldLabel>
        </FieldHeader>
        <FieldContent>
          <Switch />
        </FieldContent>
      </Field>

      <Field>
        <FieldHeader>
          <FieldLabel>IPs excluded</FieldLabel>
        </FieldHeader>
        <FieldContent>
          <Input />
        </FieldContent>
      </Field>

      <Field>
        <FieldHeader>
          <FieldLabel>Domains excluded</FieldLabel>
        </FieldHeader>
        <FieldContent>
          <Input />
        </FieldContent>
      </Field>
    </FieldSet>
  );
}

export default Sniffing;
