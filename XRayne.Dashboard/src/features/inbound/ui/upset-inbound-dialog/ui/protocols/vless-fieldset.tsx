import { Button } from "@core/ui/button";
import { ButtonGroup } from "@core/ui/button-group";
import { Field, FieldContent, FieldHeader, FieldLabel } from "@core/ui/field";
import { InputGroup, InputGroupInput } from "@core/ui/input-group";
import { Separator } from "@core/ui/separator";
import Fallbacks from "./fallbacks";

function VlessFieldSet() {
  return (
    <>
      <Field>
        <FieldHeader>
          <FieldLabel htmlFor="decryption-input">Decryption</FieldLabel>
        </FieldHeader>
        <FieldContent>
          <InputGroup className="w-full">
            <InputGroupInput id="decryption-input" placeholder="none" />
          </InputGroup>
        </FieldContent>
      </Field>

      <Field>
        <FieldHeader>
          <FieldLabel htmlFor="encryption-input">Encryption</FieldLabel>
        </FieldHeader>
        <FieldContent>
          <InputGroup>
            <InputGroupInput id="encryption-input" placeholder="none" />
          </InputGroup>
        </FieldContent>
      </Field>

      <div className="-mt-2 flex max-md:flex-col-reverse gap-y-2 md:items-center">
        <div className="font-medium text-muted-foreground max-md:hidden">
          Setted: <b>none</b>
        </div>
        <ButtonGroup className="md:ml-auto max-md:w-full max-md:[&_button]:flex-1">
          <Button size="sm" variant="secondary">
            x25519
          </Button>
          <Button size="sm" variant="secondary">
            ML-KEM-768
          </Button>
          <Button size="sm" variant="destructive">
            Clear
          </Button>
        </ButtonGroup>
      </div>

      <Separator />

      <Fallbacks />
    </>
  );
}

export default VlessFieldSet;
