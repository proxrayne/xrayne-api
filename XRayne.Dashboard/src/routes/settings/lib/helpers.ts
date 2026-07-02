import { sleep } from "@core/lib/async";

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
