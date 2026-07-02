import { AlertCircleIcon } from "lucide-react";

import { Card, CardContent, CardHeader } from "@core/ui/card";
import Page from "@core/ui/page";
import Placeholder from "@core/ui/placeholder";
import { Skeleton } from "@core/ui/skeleton";

import { useAppSettings } from "@features/settings";
import type { AppSettingsResponse, AppSubscriptionSettingsDto } from "@features/settings";

import ControlButtons from "./ui/control-buttons";
import NotificationsSettingsCard from "./ui/notifications-settings-card";
import SubscriptionSettingsCard from "./ui/subscription-settings-card";

function Settings() {
  const { settings, isLoaded, error, refetch } = useAppSettings();

  const content = (() => {
    if (!isLoaded) {
      return (
        <div className="grid gap-4">
          <SubscriptionSettingsSkeleton />
          <NotificationsSettingsSkeleton />
        </div>
      );
    }

    if (error || !settings) {
      return (
        <Placeholder>
          <Placeholder.Media>
            <AlertCircleIcon />
          </Placeholder.Media>
          <Placeholder.Header>Settings unavailable</Placeholder.Header>
          <Placeholder.Subheader>
            {error?.message || "Unable to load app settings."}
          </Placeholder.Subheader>
          <Placeholder.Actions>
            <button type="button" className="text-sm text-primary" onClick={() => refetch()}>
              Retry
            </button>
          </Placeholder.Actions>
        </Placeholder>
      );
    }

    return (
      <div className="grid gap-4">
        <SubscriptionSettingsCard subscription={toSubscriptionSettings(settings)} />
        <NotificationsSettingsCard webhooks={settings.webhooks} />
      </div>
    );
  })();

  return (
    <Page>
      <Page.Header>
        <Page.Title>Settings</Page.Title>
        <Page.Toolbar id={ControlButtons.PortalId} />
      </Page.Header>
      {content}
      <ControlButtons />
    </Page>
  );
}

function toSubscriptionSettings(settings: AppSettingsResponse): AppSubscriptionSettingsDto {
  return {
    subscriptionProfileTitle: settings.subscriptionProfileTitle,
    subscriptionSupportUrl: settings.subscriptionSupportUrl,
    subscriptionWebsiteUrl: settings.subscriptionWebsiteUrl,
    subscriptionUpdateIntervalHours: settings.subscriptionUpdateIntervalHours,
    announce: settings.announce,
  };
}

function SubscriptionSettingsSkeleton() {
  return (
    <Card className="w-full">
      <CardHeader>
        <Skeleton className="h-5 w-32" />
        <Skeleton className="h-9 w-24" />
      </CardHeader>
      <CardContent className="grid gap-6">
        <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
          {Array.from({ length: 4 }).map((_, index) => (
            <div key={index} className="grid gap-2">
              <Skeleton className="h-4 w-28" />
              <Skeleton className="h-10 w-full rounded-4xl" />
            </div>
          ))}
        </div>
        <div className="flex items-center gap-4 rounded-2xl bg-muted/60 p-4">
          <Skeleton className="size-10 rounded-full" />
          <div className="grid flex-1 gap-2">
            <Skeleton className="h-4 w-44" />
            <Skeleton className="h-4 w-64 max-w-full" />
          </div>
          <Skeleton className="h-9 w-20" />
        </div>
      </CardContent>
    </Card>
  );
}

function NotificationsSettingsSkeleton() {
  return (
    <Card className="w-full">
      <CardHeader>
        <Skeleton className="h-5 w-28" />
        <Skeleton className="h-9 w-20" />
      </CardHeader>
      <CardContent>
        <div className="grid gap-3 rounded-2xl bg-muted/40 p-4">
          {Array.from({ length: 3 }).map((_, index) => (
            <div key={index} className="grid grid-cols-[1fr_300px_2rem_5rem] items-center gap-4">
              <Skeleton className="h-4 w-full max-w-72" />
              <div className="flex flex-wrap gap-1.5">
                <Skeleton className="h-5 w-20 rounded-3xl" />
                <Skeleton className="h-5 w-24 rounded-3xl" />
                <Skeleton className="h-5 w-16 rounded-3xl" />
              </div>
              <Skeleton className="size-4 rounded-md" />
              <div className="flex justify-end gap-2">
                <Skeleton className="size-8 rounded-full" />
                <Skeleton className="size-8 rounded-full" />
              </div>
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  );
}

export default Settings;
