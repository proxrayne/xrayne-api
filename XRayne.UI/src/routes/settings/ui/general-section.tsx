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

function GeneralSection() {
  const { register } = useFormContext<FormValues>();
  const { errors } = useFormState<FormValues>();

  return (
    <Card>
      <CardHeader>
        <CardTitle>General</CardTitle>
      </CardHeader>
      <CardContent className="lg:**:data-[slot='field']:flex-row **:data-[slot='field-content']:min-w-80">
        <FieldSet>
          <Field>
            <FieldHeader>
              <FieldLabel htmlFor="bind-ip-input">
                Control panel IP address
              </FieldLabel>
              <FieldDescription>
                Leave blank to connect from any IP
              </FieldDescription>
            </FieldHeader>
            <FieldContent>
              <Input
                placeholder="0.0.0.0/24"
                id="bind-ip-input"
                {...register("bindIp")}
              />
              {errors.bindIp && (
                <FieldError>{errors.bindIp.message}</FieldError>
              )}
            </FieldContent>
          </Field>

          <Field>
            <FieldHeader>
              <FieldLabel>Panel domain</FieldLabel>
              <FieldDescription>
                Leave blank to connect with any domains and IPs
              </FieldDescription>
              {errors.domain && (
                <FieldError>{errors.domain.message}</FieldError>
              )}
            </FieldHeader>
            <FieldContent>
              <Input placeholder="my.domain.com" {...register("domain")} />
            </FieldContent>
          </Field>

          <Field>
            <FieldHeader>
              <FieldLabel>Panel port</FieldLabel>
              <FieldDescription>
                The port on which the panel operates
              </FieldDescription>
            </FieldHeader>
            <FieldContent>
              <Input
                type="number"
                inputMode="decimal"
                placeholder="5097"
                {...register("port")}
              />
              {errors.port && <FieldError>{errors.port.message}</FieldError>}
            </FieldContent>
          </Field>

          <Field>
            <FieldHeader>
              <FieldLabel>Root path of the panel URL</FieldLabel>
              <FieldDescription>
                Must start with '/' and end with '/'
              </FieldDescription>
            </FieldHeader>
            <FieldContent>
              <Input placeholder="/" {...register("pathBase")} />
              {errors.pathBase && (
                <FieldError>{errors.pathBase.message}</FieldError>
              )}
            </FieldContent>
          </Field>

          <Field>
            <FieldHeader>
              <FieldLabel>Lifetime session</FieldLabel>
              <FieldDescription>
                Session duration in the system (value: minute)
              </FieldDescription>
            </FieldHeader>
            <FieldContent>
              <Input
                placeholder="7200"
                type="number"
                inputMode="decimal"
                {...register("sessionLifetimeMinutes")}
              />
              {errors.sessionLifetimeMinutes && (
                <FieldError>{errors.sessionLifetimeMinutes.message}</FieldError>
              )}
            </FieldContent>
          </Field>
        </FieldSet>
      </CardContent>
    </Card>
  );
}

export default GeneralSection;
