import { Route } from "./+types";

import { constructMetadata } from "@core/lib/meta";

export { default } from "./sign-in";

export function meta({ matches }: Route.MetaArgs) {
  return constructMetadata({ title: "Sign in" }, matches);
}
