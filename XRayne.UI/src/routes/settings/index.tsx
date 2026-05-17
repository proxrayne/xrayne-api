import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { toast } from "sonner";

import { Route } from "./+types";

import { constructMetadata } from "@core/lib/meta";
import { Accordion } from "@core/ui/accordion";
import { Button } from "@core/ui/button";
import Page from "@core/ui/page";
import { Spinner } from "@core/ui/spinner";

import {
  CertificatesSection,
  PanelSection,
  PathsSection,
  RestartBanner,
  RestartButton,
  updatePanelSettings,
  usePanelSettings,
  type UpdatePanelSettingsRequest,
} from "@features/settings";

function Settings() {
  const { data, isLoading, error } = usePanelSettings();

  const {
    control,
    register,
    handleSubmit,
    reset,
    formState: { errors, isDirty, isSubmitting },
  } = useForm<UpdatePanelSettingsRequest>({
    defaultValues: toFormValues(data),
  });

  useEffect(() => {
    if (data) {
      reset(toFormValues(data));
    }
  }, [data, reset]);

  if (isLoading || !data) {
    return (
      <Page>
        <Page.Header>Настройки</Page.Header>
        <div className="flex justify-center py-12">
          <Spinner className="size-6" />
        </div>
      </Page>
    );
  }

  if (error) {
    return (
      <Page>
        <Page.Header>Настройки</Page.Header>
        <p className="text-destructive">Не удалось загрузить настройки.</p>
      </Page>
    );
  }

  async function onSubmit(values: UpdatePanelSettingsRequest) {
    try {
      const result = await updatePanelSettings(values);
      await usePanelSettings.invalidate();
      if (result.requiresRestart) {
        toast.warning(
          "Сохранено. Изменения требуют перезапуска панели.",
        );
      } else {
        toast.success("Сохранено. Изменения применены.");
      }
    } catch (e) {
      const message = e instanceof Error ? e.message : "Не удалось сохранить настройки";
      toast.error(message);
    }
  }

  const showBanner = data.pendingRestart || isDirty;

  return (
    <Page>
      <Page.Header>
        <div className="flex items-center justify-between gap-4">
          <span>Настройки</span>
          <div className="flex items-center gap-2">
            <Button
              type="submit"
              form={SETTINGS_FORM_ID}
              disabled={!isDirty || isSubmitting}
            >
              {isSubmitting && <Spinner className="size-4" />}
              Сохранить
            </Button>
            <RestartButton />
          </div>
        </div>
      </Page.Header>

      <form
        id={SETTINGS_FORM_ID}
        onSubmit={handleSubmit(onSubmit)}
        className="flex flex-col gap-6"
      >
        <RestartBanner visible={showBanner} />

        <Accordion type="multiple" defaultValue={["panel", "certs", "paths"]}>
          <PanelSection
            control={control}
            register={register}
            errors={errors}
            impacts={data.fieldImpacts}
          />
          <CertificatesSection control={control} impacts={data.fieldImpacts} />
          <PathsSection control={control} impacts={data.fieldImpacts} />
        </Accordion>
      </form>
    </Page>
  );
}

const SETTINGS_FORM_ID = "panel-settings-form";

function toFormValues(data?: { [k: string]: unknown }): UpdatePanelSettingsRequest {
  return {
    bindIp: (data?.bindIp as string | null) ?? null,
    domain: (data?.domain as string | null) ?? null,
    port: (data?.port as number) ?? 5097,
    webBasePath: (data?.webBasePath as string) ?? "/",
    sessionLifetimeMinutes: (data?.sessionLifetimeMinutes as number) ?? 7200,
    trustedProxyCidrs: (data?.trustedProxyCidrs as string | null) ?? null,
    certificatesDirectory: (data?.certificatesDirectory as string | null) ?? null,
    geoResourcesDirectory: (data?.geoResourcesDirectory as string | null) ?? null,
    panelCertPublicKeyPath: (data?.panelCertPublicKeyPath as string | null) ?? null,
    panelCertPrivateKeyPath: (data?.panelCertPrivateKeyPath as string | null) ?? null,
  };
}

export function meta({ matches }: Route.MetaArgs) {
  return constructMetadata({ title: "Settings" }, matches);
}

export default Settings;
