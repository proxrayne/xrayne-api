import { AlertCircleIcon, ServerIcon } from "lucide-react";
import { Controller, type SubmitHandler, type UseFormReturn } from "react-hook-form";

import type { CreateNodeFormInput, CreateNodeFormValues } from "../lib/create-node-form-schema";

import { Alert, AlertDescription, AlertTitle } from "@core/ui/alert";
import { Button } from "@core/ui/button";
import { DialogFooter } from "@core/ui/dialog";
import { Field, FieldError, FieldGroup, FieldLabel, FieldSet } from "@core/ui/field";
import { Input } from "@core/ui/input";
import { Spinner } from "@core/ui/spinner";
import { Tabs, TabsList, TabsTrigger } from "@core/ui/tabs";
import { Textarea } from "@core/ui/textarea";
import { ButtonGroup } from "@core/ui/button-group";

interface CreateNodeFormProps {
  errorMessage?: string | null;
  form: UseFormReturn<CreateNodeFormInput, unknown, CreateNodeFormValues>;
  isPending: boolean;
  onCancel: () => void;
  onSubmit: SubmitHandler<CreateNodeFormValues>;
}

function CreateNodeForm({
  errorMessage,
  form,
  isPending,
  onCancel,
  onSubmit,
}: CreateNodeFormProps) {
  const authType = form.watch("authType");

  return (
    <form className="space-y-6" onSubmit={form.handleSubmit(onSubmit)}>
      {errorMessage && (
        <Alert variant="destructive">
          <AlertCircleIcon />
          <AlertTitle>Node creation failed</AlertTitle>
          <AlertDescription>{errorMessage}</AlertDescription>
        </Alert>
      )}

      <FieldSet>
        <FieldGroup className="grid gap-4 md:grid-cols-2">
          <Field className="md:col-span-2">
            <FieldLabel htmlFor="node-name">Name</FieldLabel>
            <Input id="node-name" autoComplete="off" {...form.register("name")} />
            <FieldError errors={[form.formState.errors.name]} />
          </Field>

          <Field>
            <FieldLabel htmlFor="node-address">Address</FieldLabel>
            <Input
              id="node-address"
              autoComplete="off"
              placeholder="node.example.com"
              {...form.register("address")}
            />
            <FieldError errors={[form.formState.errors.address]} />
          </Field>

          <Field>
            <FieldLabel htmlFor="node-api-port">API port</FieldLabel>
            <Input
              id="node-api-port"
              type="number"
              {...form.register("apiPort", { valueAsNumber: true })}
            />
            <FieldError errors={[form.formState.errors.apiPort]} />
          </Field>

          <Field>
            <FieldLabel htmlFor="node-ssh-username">SSH username</FieldLabel>
            <Input
              id="node-ssh-username"
              autoComplete="username"
              {...form.register("sshUsername")}
            />
            <FieldError errors={[form.formState.errors.sshUsername]} />
          </Field>

          <Field>
            <FieldLabel htmlFor="node-port">SSH port</FieldLabel>
            <Input
              id="node-port"
              type="number"
              {...form.register("port", { valueAsNumber: true })}
            />
            <FieldError errors={[form.formState.errors.port]} />
          </Field>

          <Field className="md:col-span-2">
            <FieldLabel htmlFor="node-working-directory">Working directory</FieldLabel>
            <Input id="node-working-directory" {...form.register("workingDirectory")} />
            <FieldError errors={[form.formState.errors.workingDirectory]} />
          </Field>
        </FieldGroup>
      </FieldSet>

      <Controller
        control={form.control}
        name="authType"
        render={({ field }) => {
          const isPassword = field.value === "password";
          const inputId = isPassword ? "node-password" : "node-ssh-key";

          return (
            <Field>
              <div className="flex flex-wrap items-center justify-between gap-3">
                <FieldLabel htmlFor={inputId}>SSH authentication</FieldLabel>
                <ButtonGroup>
                  <Button
                    size="xs"
                    variant={isPassword ? "secondary" : "outline"}
                    onClick={() => field.onChange("password")}
                  >
                    Password
                  </Button>
                  <Button
                    size="xs"
                    variant={authType === "privateKey" ? "secondary" : "outline"}
                    onClick={() => field.onChange("privateKey")}
                  >
                    Private key
                  </Button>
                </ButtonGroup>
              </div>
              {isPassword ? (
                <>
                  <Input
                    id="node-password"
                    type="password"
                    autoComplete="current-password"
                    placeholder="Password"
                    {...form.register("password")}
                  />
                  <FieldError errors={[form.formState.errors.password]} />
                </>
              ) : (
                <>
                  <Textarea
                    id="node-ssh-key"
                    className="min-h-36 font-mono text-xs"
                    placeholder="SSH key"
                    spellCheck={false}
                    {...form.register("sshKey")}
                  />
                  <FieldError errors={[form.formState.errors.sshKey]} />
                </>
              )}
            </Field>
          );
        }}
      />

      <Field>
        <FieldLabel htmlFor="node-note">Note</FieldLabel>
        <Textarea id="node-note" className="min-h-20" {...form.register("note")} />
        <FieldError errors={[form.formState.errors.note]} />
      </Field>

      <DialogFooter>
        <Button type="button" variant="secondary" onClick={onCancel} disabled={isPending}>
          Cancel
        </Button>
        <Button type="submit" disabled={isPending}>
          {isPending && <Spinner />}
          Create
        </Button>
      </DialogFooter>
    </form>
  );
}

export default CreateNodeForm;
