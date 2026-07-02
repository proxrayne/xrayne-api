import type { NodeProvisionState } from "@features/node";

const NODE_PROVISION_STEPS = [
  "queued",
  "preparing",
  "uploading",
  "installing",
  "installingDependencies",
  "downloadingImage",
  "configuringCertificate",
  "startingContainer",
  "verifying",
  "completed",
  "failed",
] as const;

export type NormalizedNodeProvisionStep = (typeof NODE_PROVISION_STEPS)[number];

export function getNodeProvisionStep(
  step: NodeProvisionState["step"],
): NormalizedNodeProvisionStep {
  if (typeof step === "number") {
    return NODE_PROVISION_STEPS[step] ?? "queued";
  }

  return step;
}
