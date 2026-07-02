import type { Route } from "./+types";

import { constructMetadata } from "@core/lib/meta";

export { default } from "./nodes";

export function meta({ matches }: Route.MetaArgs) {
  return constructMetadata({ title: "Nodes" }, matches);
}
