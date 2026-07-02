import { useEffect } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { Controller, useForm } from "react-hook-form";
import { useMutation } from "@tanstack/react-query";
import { DicesIcon, SaveIcon } from "lucide-react";
import { toast } from "sonner";

import { Button } from "@core/ui/button";
import { Checkbox } from "@core/ui/checkbox";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@core/ui/dialog";
import { Field, FieldDescription, FieldError, FieldLabel } from "@core/ui/field";
import { Input } from "@core/ui/input";
import {
  InputGroup,
  InputGroupAddon,
  InputGroupButton,
  InputGroupInput,
} from "@core/ui/input-group";
import { Spinner } from "@core/ui/spinner";

import {
  type AppWebhookDto,
  type CreateAppWebhookRequest,
  type UpdateAppWebhookRequest,
  createAppWebhook,
  updateAppWebhook,
  useAppSettings,
} from "@features/settings";

import { formatNumbers, parseNumbers, webhookEventOptions } from "../lib/app-settings";
import { type WebhookForm, webhookSchema } from "../lib/validation";

interface WebhookDialogProps {
  webhook?: AppWebhookDto | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

const emptyForm: WebhookForm = {
  url: "",
  secret: "",
  events: [],
  retryAttempts: 3,
  retryIntervalSeconds: 60,
  subscriptionExpirationThresholdHours: "",
  trafficThresholdPercents: "",
};

function WebhookDialog({ webhook, open, onOpenChange }: WebhookDialogProps) {
  const isEditMode = Boolean(webhook);
  const {
    control,
    formState: { errors },
    handleSubmit,
    register,
    reset,
    setValue,
    watch,
  } = useForm<WebhookForm>({
    defaultValues: emptyForm,
    resolver: zodResolver(webhookSchema),
  });
  const events = watch("events");

  useEffect(() => {
    if (!open) {
      return;
    }

    reset(
      webhook
        ? {
            url: webhook.url,
            secret: "",
            events: webhook.events,
            retryAttempts: webhook.retryAttempts,
            retryIntervalSeconds: webhook.retryIntervalSeconds,
            subscriptionExpirationThresholdHours: formatNumbers(
              webhook.subscriptionExpirationThresholdHours,
            ),
            trafficThresholdPercents: formatNumbers(webhook.trafficThresholdPercents),
          }
        : emptyForm,
    );
  }, [open, reset, webhook]);

  const createMutation = useMutation({
    mutationKey: ["app-settings", "webhooks", "create"],
    mutationFn: createAppWebhook,
    onSuccess: async () => {
      await useAppSettings.invalidate();
      toast.success("Webhook created");
      onOpenChange(false);
    },
    onError: (error) => {
      toast.error(error?.message || "Unable to create webhook");
    },
  });

  const updateMutation = useMutation({
    mutationKey: ["app-settings", "webhooks", "update", webhook?.id],
    mutationFn: (payload: UpdateAppWebhookRequest) => updateAppWebhook(webhook!.id, payload),
    onSuccess: async () => {
      await useAppSettings.invalidate();
      toast.success("Webhook saved");
      onOpenChange(false);
    },
    onError: (error) => {
      toast.error(error?.message || "Unable to save webhook");
    },
  });

  const isPending = createMutation.isPending || updateMutation.isPending;
  const title = isEditMode ? "Edit webhook" : "Add webhook";

  const generateSecret = () => {
    setValue("secret", createRandomSecret(), { shouldDirty: true, shouldTouch: true });
  };

  const save = (form: WebhookForm) => {
    const payload: UpdateAppWebhookRequest = {
      url: form.url.trim(),
      events: form.events,
      retryAttempts: Math.max(0, form.retryAttempts || 0),
      retryIntervalSeconds: Math.max(1, form.retryIntervalSeconds || 1),
      subscriptionExpirationThresholdHours: parseNumbers(form.subscriptionExpirationThresholdHours),
      trafficThresholdPercents: parseNumbers(form.trafficThresholdPercents),
    };

    if (isEditMode) {
      updateMutation.mutate(payload);
      return;
    }

    const createPayload: CreateAppWebhookRequest = {
      ...payload,
      secret: form.secret.trim() || null,
    };

    createMutation.mutate(createPayload);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-2xl">
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
          <DialogDescription>
            Delivery policy and notification events for this endpoint.
          </DialogDescription>
        </DialogHeader>
        <form className="grid gap-6" onSubmit={handleSubmit(save)}>
          <div className="-mr-2 grid max-h-[70vh] gap-5 overflow-y-auto overscroll-contain pr-4">
            <div className="grid gap-4 md:grid-cols-2">
              <Field className="md:col-span-2">
                <FieldLabel htmlFor="webhook-url">URL</FieldLabel>
                <Input
                  id="webhook-url"
                  placeholder="https://example.com/webhook"
                  {...register("url")}
                />
                <FieldError>{errors.url?.message}</FieldError>
              </Field>
              {!isEditMode && (
                <Field className="md:col-span-2">
                  <FieldLabel htmlFor="webhook-secret">Secret</FieldLabel>
                  <InputGroup>
                    <InputGroupInput
                      id="webhook-secret"
                      type="text"
                      placeholder="Enter the webhook secret"
                      {...register("secret")}
                    />
                    <InputGroupAddon align="inline-end">
                      <InputGroupButton
                        size="icon-xs"
                        title="Generate secret"
                        onClick={generateSecret}
                      >
                        <DicesIcon />
                        <span className="sr-only">Generate secret</span>
                      </InputGroupButton>
                    </InputGroupAddon>
                  </InputGroup>
                  <FieldDescription>
                    The secret is visible only once during creation. Save the value now to avoid
                    losing it.
                  </FieldDescription>
                </Field>
              )}
              <Field>
                <FieldLabel htmlFor="webhook-retry-attempts">Retry attempts</FieldLabel>
                <Input
                  id="webhook-retry-attempts"
                  type="number"
                  min={0}
                  placeholder="3"
                  {...register("retryAttempts", { valueAsNumber: true })}
                />
                <FieldError>{errors.retryAttempts?.message}</FieldError>
              </Field>
              <Field>
                <FieldLabel htmlFor="webhook-retry-interval">Retry interval, seconds</FieldLabel>
                <Input
                  id="webhook-retry-interval"
                  type="number"
                  min={1}
                  placeholder="60"
                  {...register("retryIntervalSeconds", { valueAsNumber: true })}
                />
                <FieldError>{errors.retryIntervalSeconds?.message}</FieldError>
              </Field>
              <Field>
                <FieldLabel htmlFor="webhook-subscription-thresholds">
                  Subscription hours
                </FieldLabel>
                <Input
                  id="webhook-subscription-thresholds"
                  placeholder="24, 72, 168"
                  {...register("subscriptionExpirationThresholdHours")}
                />
                <FieldError>{errors.subscriptionExpirationThresholdHours?.message}</FieldError>
              </Field>
              <Field>
                <FieldLabel htmlFor="webhook-traffic-thresholds">Traffic percents</FieldLabel>
                <Input
                  id="webhook-traffic-thresholds"
                  placeholder="50, 80, 100"
                  {...register("trafficThresholdPercents")}
                />
                <FieldError>{errors.trafficThresholdPercents?.message}</FieldError>
              </Field>
            </div>
            <div className="grid gap-3 rounded-3xl bg-secondary/60 p-4 md:grid-cols-2">
              {webhookEventOptions.map((event) => (
                <Controller
                  key={event.value}
                  control={control}
                  name="events"
                  render={({ field }) => (
                    <label className="flex items-center gap-3 text-sm">
                      <Checkbox
                        checked={events.includes(event.value)}
                        onCheckedChange={(checked) => {
                          field.onChange(
                            checked
                              ? [...events, event.value]
                              : events.filter((currentEvent) => currentEvent !== event.value),
                          );
                        }}
                      />
                      <span>{event.label}</span>
                    </label>
                  )}
                />
              ))}
            </div>
          </div>
          <DialogFooter showCloseButton>
            <Button type="submit" disabled={isPending}>
              {isPending ? <Spinner /> : <SaveIcon />}
              Save
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

function createRandomSecret() {
  const alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
  const bytes = new Uint8Array(36);
  crypto.getRandomValues(bytes);

  return Array.from(bytes, (byte) => alphabet[byte % alphabet.length]).join("");
}

export default WebhookDialog;
