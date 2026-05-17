import { ReactNode } from "react";
import { FormProvider as HookFormProvider, useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";

import { PanelSettingsDto } from "@features/settings";

import { FORM_ID, formSchema } from "../lib/constants";

interface Props {
  settings: PanelSettingsDto;
  children: ReactNode;
}

function FormProvider({ children, settings }: Props) {
  const form = useForm({
    defaultValues: formSchema.safeParse(settings).data,
    resolver: zodResolver(formSchema),
  });

  return (
    <HookFormProvider {...form}>
      <form
        className="flex flex-col gap-3"
        onSubmit={form.handleSubmit((values) => {
          console.log(values);
        })}
        id={FORM_ID}
      >
        {children}
      </form>
    </HookFormProvider>
  );
}

export default FormProvider;
