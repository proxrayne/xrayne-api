import type { Control } from "react-hook-form";
import { Controller } from "react-hook-form";
import { InfoIcon } from "lucide-react";

import {
  AccordionContent,
  AccordionItem,
  AccordionTrigger,
} from "@core/ui/accordion";
import { Alert, AlertDescription, AlertTitle } from "@core/ui/alert";
import { Field, FieldDescription, FieldLabel } from "@core/ui/field";
import { Input } from "@core/ui/input";

import type { PanelSettingsDto, UpdatePanelSettingsRequest } from "../lib/api.types";
import { FieldImpactBadge } from "./field-impact-badge";

interface PathsSectionProps {
  control: Control<UpdatePanelSettingsRequest>;
  impacts: PanelSettingsDto["fieldImpacts"];
}

export function PathsSection({ control, impacts }: PathsSectionProps) {
  return (
    <AccordionItem value="paths">
      <AccordionTrigger>Пути</AccordionTrigger>
      <AccordionContent className="flex flex-col gap-4 pt-2">
        <Alert className="border-sky-500/30 bg-sky-500/10 text-sky-200">
          <InfoIcon />
          <AlertTitle>Интеграция в разработке</AlertTitle>
          <AlertDescription>
            Значения сохраняются и будут применены при следующих операциях с
            сертификатами и георесурсами Xray. Сейчас изменения в этих полях
            ни на что не влияют — интеграция с xray-core ещё не реализована.
          </AlertDescription>
        </Alert>

        <Controller
          control={control}
          name="certificatesDirectory"
          render={({ field }) => (
            <Field>
              <FieldLabel htmlFor="certs-dir">
                Директория хранения сертификатов <FieldImpactBadge impact={impacts.certificatesDirectory} />
              </FieldLabel>
              <Input
                id="certs-dir"
                placeholder="Оставьте пустым для значения по умолчанию"
                value={field.value ?? ""}
                onChange={(e) => field.onChange(e.target.value || null)}
              />
              <FieldDescription>
                Используется для генерации и хранения SSL-сертификатов для Xray.
              </FieldDescription>
            </Field>
          )}
        />

        <Controller
          control={control}
          name="geoResourcesDirectory"
          render={({ field }) => (
            <Field>
              <FieldLabel htmlFor="geo-dir">
                Директория для файлов георесурсов <FieldImpactBadge impact={impacts.geoResourcesDirectory} />
              </FieldLabel>
              <Input
                id="geo-dir"
                placeholder="Оставьте пустым для значения по умолчанию"
                value={field.value ?? ""}
                onChange={(e) => field.onChange(e.target.value || null)}
              />
              <FieldDescription>geoip.dat, geosite.dat и аналогичные файлы для Xray.</FieldDescription>
            </Field>
          )}
        />
      </AccordionContent>
    </AccordionItem>
  );
}
