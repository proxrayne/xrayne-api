import { Controller, useForm } from "react-hook-form";
import { useNavigate, useSearchParams } from "react-router";

import { urls } from "@core/lib/urls";
import { ResponseError } from "@core/lib/errors";
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@core/ui/card";
import { Button } from "@core/ui/button";
import { Spinner } from "@core/ui/spinner";
import { Field, FieldLabel } from "@core/ui/field";
import { Input } from "@core/ui/input";
import { Checkbox } from "@core/ui/checkbox";

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
    setFocus,
    formState: { errors, isSubmitting, isValid },
  } = useForm({
    defaultValues: { username: "", password: "", saveMe: true },
  });

  return (
    <main className="w-full px-4 py-10 flex-auto flex flex-col items-center justify-center">
      <Card className="max-w-md w-full">
        <CardHeader>
          <CardTitle className="text-lg leading-8">Sign in</CardTitle>
          <CardDescription>Use your administrator credentials to continue.</CardDescription>
        </CardHeader>
        <form
          onSubmit={handleSubmit(async ({ password, username, saveMe }) => {
            try {
              const { admin } = await login(username, password, { saveMe });

              useAdminAccount.setData(admin);

              navigate(search.get("return_url") ?? urls.root(), {
                replace: true,
              });
            } catch (error) {
              setError("root", {
                message: error instanceof ResponseError ? error.message : "Unhandled error.",
              });
              setFocus("username");
            }
          })}
        >
          <CardContent className="flex flex-col gap-4">
            <Field>
              <FieldLabel htmlFor="username-input">Username</FieldLabel>
              <Input
                autoFocus
                id="username-input"
                placeholder="admin"
                {...register("username", { required: true })}
              />
            </Field>
            <Field>
              <FieldLabel htmlFor="password-input">Password</FieldLabel>
              <Input
                id="password-input"
                placeholder="••••••••"
                type="password"
                {...register("password", { required: true })}
              />
            </Field>

            <Controller
              control={control}
              name="saveMe"
              render={({ field: { value, ...field } }) => (
                <Field orientation="horizontal">
                  <Checkbox checked={value} id="remember-me-checkbox" {...field} />
                  <FieldLabel htmlFor="remember-me-checkbox">Remember me</FieldLabel>
                </Field>
              )}
            />

            {errors.root && <p className="text-sm text-destructive mt-2">{errors.root.message}</p>}
          </CardContent>
          <CardFooter className="mt-6 flex flex-col gap-2">
            <Button size="lg" className="w-full" type="submit" disabled={isSubmitting || !isValid}>
              {isSubmitting && <Spinner className="size-4 text-white" />}
              Sign in
            </Button>
          </CardFooter>
        </form>
      </Card>
    </main>
  );
}

export default SignIn;
