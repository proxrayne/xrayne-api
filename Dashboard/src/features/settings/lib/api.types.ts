export type WebhookEvent =
  | "userCreated"
  | "userUpdated"
  | "userDeleted"
  | "deviceConnected"
  | "deviceRevoked"
  | "userStatusChanged"
  | "trafficReset"
  | "trafficPercentThresholdReached"
  | "subscriptionHoursThresholdReached";

export interface SubscriptionAnnounce {
  message?: string | null;
  url?: string | null;
}

export interface AppSubscriptionSettingsDto {
  subscriptionProfileTitle: string;
  subscriptionSupportUrl?: string | null;
  subscriptionWebsiteUrl?: string | null;
  subscriptionUpdateIntervalHours: number;
  announce?: SubscriptionAnnounce | null;
}

export interface AppWebhookDto {
  id: string;
  url: string;
  events: WebhookEvent[];
  hasSecret: boolean;
  retryAttempts: number;
  retryIntervalSeconds: number;
  subscriptionExpirationThresholdHours: number[];
  trafficThresholdPercents: number[];
}

export interface CreateAppWebhookRequest {
  url: string;
  events: WebhookEvent[];
  secret?: string | null;
  retryAttempts: number;
  retryIntervalSeconds: number;
  subscriptionExpirationThresholdHours: number[];
  trafficThresholdPercents: number[];
}

export type UpdateAppWebhookRequest = Omit<CreateAppWebhookRequest, "secret">;

export interface AppSettingsResponse extends AppSubscriptionSettingsDto {
  webhooks: AppWebhookDto[];
}
