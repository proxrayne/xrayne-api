import {
  Button,
  Card,
  Checkbox,
  ErrorMessage,
  Form,
  Input,
  Label,
  Spinner,
  TextField,
} from "@heroui/react";
import { Controller, useForm } from "react-hook-form";
import { useNavigate, useSearchParams } from "react-router";

import { urls } from "@core/lib/urls";
import { ResponseError } from "@core/lib/errors";

import { useAdminAccount } from "@features/admin";
import { login } from "@features/auth/lib/api";

function SignIn() {
  const navigate = useNavigate();
  const [search] = useSearchParams();
  const {
    control,
    handleSubmit,
    register,
    setError,
    formState: { errors, isSubmitting, isValid },
  } = useForm({
    defaultValues: { username: "", password: "", saveMe: true },
  });

  return (
    <main className="w-full px-4 py-10 flex-auto flex flex-col items-center justify-center">
      <Card className="max-w-md w-full">
        <Card.Header>
          <Card.Title className="text-lg leading-8">Sign in</Card.Title>
          <Card.Description>
            Use your administrator credentials to continue.
          </Card.Description>
        </Card.Header>
        <Form
          onSubmit={handleSubmit(async ({ password, username, saveMe }) => {
            try {
              const { admin } = await login(username, password, { saveMe });

              useAdminAccount.setData(admin);

              navigate(search.get("return_url") ?? urls.root(), {
                replace: true,
              });
            } catch (error) {
              setError("root", {
                message:
                  error instanceof ResponseError
                    ? error.message
                    : "Unhandled error.",
              });
            }
          })}
        >
          <Card.Content className="flex flex-col gap-4">
            <TextField name="username" type="text">
              <Label>Username</Label>
              <Input
                placeholder="admin"
                variant="secondary"
                {...register("username", { required: true })}
              />
            </TextField>
            <TextField name="password" type="password">
              <Label>Password</Label>
              <Input
                placeholder="••••••••"
                variant="secondary"
                {...register("password", { required: true })}
              />
            </TextField>

            <Controller
              control={control}
              name="saveMe"
              render={({ field: { value, ...field } }) => (
                <Checkbox
                  id="save-me"
                  variant="secondary"
                  isSelected={value}
                  {...field}
                >
                  <Checkbox.Control {...register("saveMe")}>
                    <Checkbox.Indicator />
                  </Checkbox.Control>
                  <Checkbox.Content>
                    <Label htmlFor="save-me">Remember me</Label>
                  </Checkbox.Content>
                </Checkbox>
              )}
            />

            {errors.root && <ErrorMessage>{errors.root.message}</ErrorMessage>}
          </Card.Content>
          <Card.Footer className="mt-6 flex flex-col gap-2">
            <Button
              className="w-full"
              type="submit"
              isPending={isSubmitting}
              isDisabled={!isValid}
            >
              {isSubmitting && <Spinner className="size-4 text-white" />}
              Sign in
            </Button>
          </Card.Footer>
        </Form>
      </Card>
    </main>
  );
}

export default SignIn;
