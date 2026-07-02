import { useEffect, useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { useMutation } from "@tanstack/react-query";
import { Edit3Icon, ExternalLinkIcon, MegaphoneIcon, PlusIcon, SaveIcon } from "lucide-react";
import { toast } from "sonner";

import { Button } from "@core/ui/button";
import { Card, CardAction, CardContent, CardHeader, CardTitle } from "@core/ui/card";
import { Field, FieldError, FieldLabel } from "@core/ui/field";
import { Input } from "@core/ui/input";
import {
  Item,
  ItemActions,
  ItemContent,
  ItemDescription,
  ItemMedia,
  ItemTitle,
} from "@core/ui/item";
import { Spinner } from "@core/ui/spinner";

import {
  type AppSubscriptionSettingsDto,
  updateSubscriptionSettings,
  useAppSettings,
} from "@features/settings";

import { emptyToNull } from "../lib/app-settings";
import { type SubscriptionSettingsForm, subscriptionSettingsSchema } from "../lib/validation";
import AnnounceDialog from "./announce-dialog";

interface SubscriptionSettingsCardProps {
  subscription: AppSubscriptionSettingsDto;
}

function SubscriptionSettingsCard({ subscription }: SubscriptionSettingsCardProps) {
  const {
    formState: { errors },
    handleSubmit,
    register,
    reset,
    setValue,
    watch,
  } = useForm<SubscriptionSettingsForm>({
    defaultValues: toForm(subscription),
    resolver: zodResolver(subscriptionSettingsSchema),
  });
  const [announceDialogOpen, setAnnounceDialogOpen] = useState(false);
  const announce = watch("announce");
  const hasAnnounce = Boolean(announce?.message || announce?.url);

  useEffect(() => {
    reset(toForm(subscription));
  }, [reset, subscription]);

  const saveMutation = useMutation({
    mutationKey: ["app-settings", "subscription", "save"],
    mutationFn: updateSubscriptionSettings,
    onSuccess: (settings) => {
      useAppSettings.setData(settings);
      toast.success("Subscription settings saved");
    },
    onError: (error) => {
      toast.error(error?.message || "Unable to save subscription settings");
    },
  });

  const save = (form: SubscriptionSettingsForm) => {
    saveMutation.mutate({
      subscriptionProfileTitle: form.subscriptionProfileTitle.trim(),
      subscriptionSupportUrl: emptyToNull(form.subscriptionSupportUrl ?? ""),
      subscriptionWebsiteUrl: emptyToNull(form.subscriptionWebsiteUrl ?? ""),
      subscriptionUpdateIntervalHours: Math.max(1, form.subscriptionUpdateIntervalHours || 1),
      announce: form.announce,
    });
  };

  return (
    <Card className="w-full">
      <form className="grid gap-6" onSubmit={handleSubmit(save)}>
        <CardHeader>
          <CardTitle>Subscription</CardTitle>
          <CardAction>
            <Button type="submit" disabled={saveMutation.isPending}>
              {saveMutation.isPending ? <Spinner /> : <SaveIcon />}
              Save
            </Button>
          </CardAction>
        </CardHeader>
        <CardContent className="grid gap-6">
          <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
            <Field>
              <FieldLabel htmlFor="subscription-profile-title">Profile title</FieldLabel>
              <Input
                id="subscription-profile-title"
                placeholder="Enter the profile title"
                {...register("subscriptionProfileTitle")}
              />
              <FieldError>{errors.subscriptionProfileTitle?.message}</FieldError>
            </Field>
            <Field>
              <FieldLabel htmlFor="subscription-support-url">Support URL</FieldLabel>
              <Input
                id="subscription-support-url"
                placeholder="https://support.example.com"
                {...register("subscriptionSupportUrl")}
              />
              <FieldError>{errors.subscriptionSupportUrl?.message}</FieldError>
            </Field>
            <Field>
              <FieldLabel htmlFor="subscription-website-url">Website URL</FieldLabel>
              <Input
                id="subscription-website-url"
                placeholder="https://example.com"
                {...register("subscriptionWebsiteUrl")}
              />
              <FieldError>{errors.subscriptionWebsiteUrl?.message}</FieldError>
            </Field>
            <Field>
              <FieldLabel htmlFor="subscription-update-interval">Update interval, hours</FieldLabel>
              <Input
                id="subscription-update-interval"
                type="number"
                min={1}
                placeholder="24"
                {...register("subscriptionUpdateIntervalHours", { valueAsNumber: true })}
              />
              <FieldError>{errors.subscriptionUpdateIntervalHours?.message}</FieldError>
            </Field>
          </div>
          <Item variant="muted" className="items-start">
            <ItemMedia variant="icon">
              <MegaphoneIcon />
            </ItemMedia>
            <ItemContent>
              <ItemTitle>
                {hasAnnounce
                  ? (announce?.message ?? "Announce message is not set")
                  : "Announce is not configured"}
              </ItemTitle>
              <ItemDescription>
                {hasAnnounce
                  ? "Shown in Happ, v2RayTun, and INCY subscription clients."
                  : "Add a message for subscription clients when users need to know about changes."}
              </ItemDescription>
            </ItemContent>
            <ItemActions>
              {hasAnnounce && announce?.url ? (
                <Button type="button" variant="link" size="sm" asChild>
                  <a href={announce.url} target="_blank" rel="noreferrer">
                    <ExternalLinkIcon />
                    Open
                  </a>
                </Button>
              ) : null}
              {hasAnnounce ? (
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={() => setAnnounceDialogOpen(true)}
                >
                  <Edit3Icon />
                  Edit
                </Button>
              ) : (
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={() => setAnnounceDialogOpen(true)}
                >
                  Setup
                </Button>
              )}
            </ItemActions>
          </Item>
        </CardContent>
      </form>
      <AnnounceDialog
        open={announceDialogOpen}
        announce={announce}
        onOpenChange={setAnnounceDialogOpen}
        onSave={(nextAnnounce) =>
          setValue("announce", nextAnnounce, { shouldDirty: true, shouldTouch: true })
        }
      />
    </Card>
  );
}

function toForm(subscription: AppSubscriptionSettingsDto): SubscriptionSettingsForm {
  return {
    subscriptionProfileTitle: subscription.subscriptionProfileTitle,
    subscriptionSupportUrl: subscription.subscriptionSupportUrl ?? "",
    subscriptionWebsiteUrl: subscription.subscriptionWebsiteUrl ?? "",
    subscriptionUpdateIntervalHours: subscription.subscriptionUpdateIntervalHours,
    announce: subscription.announce,
  };
}

export default SubscriptionSettingsCard;
