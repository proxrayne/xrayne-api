import { Field, FieldContent, FieldHeader, FieldLabel } from "@core/ui/field";
import { Switch } from "@core/ui/switch";

function SocksFieldset() {
  return (
    <Field>
      <FieldHeader>
        <FieldLabel>Allow transparent</FieldLabel>
      </FieldHeader>
      <FieldContent>
        <Switch />
      </FieldContent>
    </Field>
  );
}

export default SocksFieldset;
