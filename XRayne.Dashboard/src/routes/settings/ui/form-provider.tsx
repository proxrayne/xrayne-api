import { ReactNode, useEffect, useMemo } from "react";
import { FormProvider as HookFormProvider, useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import { toast } from "sonner";

import {
  PanelSettingsDto,
  restartPanel,
  updatePanelSettings,
  usePanelSettings,
} from "@features/settings";

import { FORM_ID, formSchema, FormValues } from "../lib/constants";
import { buildPanelUrl } from "../lib/helpers";

interface Props {
  settings: PanelSettingsDto;
  children: ReactNode;
}

function FormProvider({ children, settings }: Props) {
  const defaultValues = useMemo(
    () => formSchema.safeParse(settings).data,
    [settings],
  );

  const form = useForm({
    defaultValues,
    resolver: zodResolver(formSchema),
  });

  useEffect(() => {
    form.reset(defaultValues, {
      keepDirtyValues: true,
    });
  }, [defaultValues]);

  const { mutateAsync } = useMutation({
    mutationKey: ["panel", "change-settings"],
    mutationFn: async (values: FormValues) => {
      const { requiresRestart } = await updatePanelSettings(values);
      if (requiresRestart) {
        await restartPanel();
      }

      const nextUrl = buildPanelUrl(values).toString();
      if (location.href !== nextUrl) {
        location.href = nextUrl;
      }
    },
    onSuccess: () => usePanelSettings.invalidate(),
    onError: (error) => {
      toast.error(error?.message || "Failure saving data.", {
        duration: 5_000,
      });
    },
  });

  return (
    <HookFormProvider {...form}>
      <form
        id={FORM_ID}
        className="flex flex-col gap-3"
        onSubmit={form.handleSubmit((values) => mutateAsync(values))}
      >
        {children}
      </form>
    </HookFormProvider>
  );
}

export default FormProvider;
