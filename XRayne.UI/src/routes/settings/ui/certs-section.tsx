import { useFormContext, useFormState } from "react-hook-form";

import { Card, CardContent, CardHeader, CardTitle } from "@core/ui/card";
import {
  Field,
  FieldContent,
  FieldDescription,
  FieldError,
  FieldHeader,
  FieldLabel,
  FieldSet,
} from "@core/ui/field";
import { Input } from "@core/ui/input";

import { FormValues } from "../lib/constants";

function CertsSection() {
  const { register } = useFormContext<FormValues>();
  const { errors } = useFormState<FormValues>();

  return (
    <Card>
      <CardHeader>
        <CardTitle>Certificates</CardTitle>
      </CardHeader>
      <CardContent className="lg:**:data-[slot='field']:flex-row **:data-[slot='field-content']:min-w-80">
        <FieldSet>
          <Field>
            <FieldHeader>
              <FieldLabel>Public Key Path</FieldLabel>
              <FieldDescription>
                The public key file path for the web panel. (begins with "/")
              </FieldDescription>
            </FieldHeader>
            <FieldContent>
              <Input
                placeholder="/root/certs/my.domain.com/fullchain.pem"
                {...register("certPublicKeyPath")}
              />
              {errors.certPublicKeyPath && (
                <FieldError>{errors.certPublicKeyPath.message}</FieldError>
              )}
            </FieldContent>
          </Field>

          <Field>
            <FieldHeader>
              <FieldLabel>Private Key Path</FieldLabel>
              <FieldDescription>
                The private key file path for the web panel. (begins with "/")
              </FieldDescription>
            </FieldHeader>
            <FieldContent>
              <Input
                placeholder="/root/certs/my.domain.com/privkey.pem"
                {...register("certPrivateKeyPath")}
              />
              {errors.certPrivateKeyPath && (
                <FieldError>{errors.certPrivateKeyPath.message}</FieldError>
              )}
            </FieldContent>
          </Field>
        </FieldSet>
      </CardContent>
    </Card>
  );
}

export default CertsSection;
