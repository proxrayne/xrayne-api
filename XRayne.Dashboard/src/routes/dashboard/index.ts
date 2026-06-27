import { Route } from "./+types";

import { constructMetadata } from "@core/lib/meta";

export { default } from "./dashboard";

export function meta({ matches }: Route.MetaArgs) {
  return constructMetadata({ title: "Dashboard" }, matches);
}
