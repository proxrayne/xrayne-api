import { sleep } from "@core/lib/async";

import { FormValues } from "./constants";

export async function waitRestart(func: () => Promise<void>, timeout: number) {
  const deadline = Date.now() + timeout;
  while (Date.now() < deadline) {
    try {
      await func();

      return;
    } catch {
      await sleep(3_000);
    }
  }

  throw new Error("Waiting for restart timeout");
}

export function buildPanelUrl({
  domain,
  port,
}: Pick<FormValues, "domain" | "port" | "pathBase">) {
  const url = new URL(location.href);

  if (import.meta.env.DEV) {
    return url;
  }

  if (domain) {
    if (domain.includes("://")) {
      const domainUrl = new URL(domain);

      url.protocol = domainUrl.protocol;
      url.hostname = domainUrl.hostname;
    } else {
      url.hostname = domain;
    }
  }

  if (port) {
    url.port = String(port);
  }

  //   if (pathBase) {
  //     const nextPathBase =
  //       pathBase === "/" ? "/" : `/${pathBase.trim().replace(/^\/|\/$/g, "")}/`;

  //     url.pathname = nextPathBase;
  //   }

  return url;
}
