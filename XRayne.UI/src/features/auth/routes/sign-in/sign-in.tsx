import { Button, Input } from "@heroui/react";
import { type FormEvent, useState } from "react";
import { useNavigate } from "react-router";

type LoginResponse = {
  accessToken: string;
  expireAt: string;
};

function SignIn() {
  const navigate = useNavigate();
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      const response = await fetch("/api/auth/login", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ username, password }),
      });

      if (!response.ok) {
        const message = await readErrorMessage(response);
        throw new Error(message || "Unable to sign in.");
      }

      const result = (await response.json()) as LoginResponse;

      localStorage.setItem("xrayne.accessToken", result.accessToken);
      localStorage.setItem("xrayne.accessTokenExpiresAt", result.expireAt);

      await navigate("/");
    } catch (exception) {
      setError(
        exception instanceof Error ? exception.message : "Unable to sign in.",
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <main className="px-4 py-10">
      <section className="mx-auto flex w-full max-w-sm flex-col justify-center">
        <div className="mb-8">
          <p className="mb-2 text-sm font-medium text-primary">XRayne</p>
          <h1 className="text-3xl font-semibold">Sign in</h1>
          <p className="mt-2 text-sm text-muted">
            Use your administrator credentials to continue.
          </p>
        </div>

        <form className="flex flex-col gap-4" onSubmit={handleSubmit}>
          <label className="flex flex-col gap-2">
            <span className="text-sm font-medium">Username</span>
            <Input
              autoComplete="username"
              autoFocus
              fullWidth
              name="username"
              placeholder="admin"
              required
              value={username}
              onChange={(event) => setUsername(event.target.value)}
            />
          </label>

          <label className="flex flex-col gap-2">
            <span className="text-sm font-medium">Password</span>
            <Input
              autoComplete="current-password"
              fullWidth
              name="password"
              placeholder="Enter password"
              required
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
            />
          </label>

          {error && (
            <div className="rounded-md border border-danger/40 bg-danger/10 px-3 py-2 text-sm text-danger">
              {error}
            </div>
          )}

          <Button fullWidth isDisabled={isSubmitting} type="submit">
            {isSubmitting ? "Signing in..." : "Sign in"}
          </Button>
        </form>
      </section>
    </main>
  );
}

async function readErrorMessage(response: Response) {
  const contentType = response.headers.get("content-type");

  if (contentType?.includes("application/json")) {
    const body = await response.json();

    if (typeof body === "string") {
      return body;
    }

    if (body && typeof body === "object" && "message" in body) {
      return String(body.message);
    }
  }

  return await response.text();
}

export default SignIn;
