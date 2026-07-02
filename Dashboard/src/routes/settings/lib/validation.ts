import { z } from "zod";

import type { WebhookEvent } from "@features/settings";

const optionalUrl = z
  .string()
  .trim()
  .refine((value) => !value || z.url().safeParse(value).success, "Enter a valid URL");

const requiredUrl = z.string().trim().min(1, "URL is required").pipe(z.url("Enter a valid URL"));

const thresholds = (label: string, isValid: (value: number) => boolean, message: string) =>
  z
    .string()
    .trim()
    .refine((value) => {
      if (!value) {
        return true;
      }

      return value
        .split(",")
        .map((item) => Number(item.trim()))
        .every((item) => Number.isInteger(item) && isValid(item));
    }, `${label} must be comma-separated integers. ${message}`);

export const subscriptionSettingsSchema = z.object({
  subscriptionProfileTitle: z.string().trim().min(1, "Profile title is required"),
  subscriptionSupportUrl: optionalUrl,
  subscriptionWebsiteUrl: optionalUrl,
  subscriptionUpdateIntervalHours: z
    .number("Update interval is required")
    .int("Update interval must be a whole number")
    .min(1, "Update interval must be at least 1 hour"),
  announce: z
    .object({
      message: z.string().nullable().optional(),
      url: z.string().nullable().optional(),
    })
    .nullable()
    .optional(),
});

export const announceSchema = z.object({
  message: z.string(),
  url: optionalUrl,
});

const webhookEventValues = [
  "userCreated",
  "userUpdated",
  "userDeleted",
  "deviceConnected",
  "deviceRevoked",
  "userStatusChanged",
  "trafficReset",
  "trafficPercentThresholdReached",
  "subscriptionHoursThresholdReached",
] as const satisfies readonly WebhookEvent[];

export const webhookSchema = z.object({
  url: requiredUrl,
  secret: z.string(),
  events: z.array(z.enum(webhookEventValues)),
  retryAttempts: z
    .number("Retry attempts is required")
    .int("Retry attempts must be a whole number")
    .min(0, "Retry attempts cannot be negative"),
  retryIntervalSeconds: z
    .number("Retry interval is required")
    .int("Retry interval must be a whole number")
    .min(1, "Retry interval must be at least 1 second"),
  subscriptionExpirationThresholdHours: thresholds(
    "Subscription thresholds",
    (value) => value > 0,
    "Use positive hours.",
  ),
  trafficThresholdPercents: thresholds(
    "Traffic thresholds",
    (value) => value >= 1 && value <= 100,
    "Use values from 1 to 100.",
  ),
});

export type SubscriptionSettingsForm = z.infer<typeof subscriptionSettingsSchema>;
export type AnnounceForm = z.infer<typeof announceSchema>;
export type WebhookForm = z.infer<typeof webhookSchema>;
