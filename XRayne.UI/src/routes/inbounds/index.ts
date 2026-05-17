import { constructMetadata } from "@core/lib/meta";

import type { Route } from "./+types";

export { default } from "./inbounds";

export function meta({ matches }: Route.MetaArgs) {
  return constructMetadata({ title: "Inbounds" }, matches);
}
