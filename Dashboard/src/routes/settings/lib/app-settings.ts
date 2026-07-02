import type { WebhookEvent } from "@features/settings";

export const webhookEventOptions: Array<{ value: WebhookEvent; label: string }> = [
  { value: "userCreated", label: "User created" },
  { value: "userUpdated", label: "User updated" },
  { value: "userDeleted", label: "User deleted" },
  { value: "deviceConnected", label: "Device connected" },
  { value: "deviceRevoked", label: "Device revoked" },
  { value: "userStatusChanged", label: "User status changed" },
  { value: "trafficReset", label: "Traffic reset" },
  { value: "trafficPercentThresholdReached", label: "Traffic threshold" },
  { value: "subscriptionHoursThresholdReached", label: "Subscription threshold" },
];

export function formatNumbers(values: number[]) {
  return values.join(", ");
}

export function parseNumbers(value: string) {
  return value
    .split(",")
    .map((item) => Number(item.trim()))
    .filter((item) => Number.isFinite(item));
}

export function emptyToNull(value: string) {
  const next = value.trim();

  return next.length ? next : null;
}

export function formatEventList(events: WebhookEvent[]) {
  if (!events.length) {
    return "Disabled";
  }

  return events.map(formatEventLabel).join(", ");
}

export function formatEventLabel(event: WebhookEvent) {
  const labels = new Map(webhookEventOptions.map((option) => [option.value, option.label]));

  return labels.get(event) ?? event;
}
