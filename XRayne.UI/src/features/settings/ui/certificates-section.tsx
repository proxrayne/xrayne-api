import type { Control } from "react-hook-form";
import { Controller } from "react-hook-form";

import {
  AccordionContent,
  AccordionItem,
  AccordionTrigger,
} from "@core/ui/accordion";
import { Field, FieldDescription, FieldLabel } from "@core/ui/field";
import { Input } from "@core/ui/input";

import type { PanelSettingsDto, UpdatePanelSettingsRequest } from "../lib/api.types";
import { FieldImpactBadge } from "./field-impact-badge";

interface CertificatesSectionProps {
  control: Control<UpdatePanelSettingsRequest>;
  impacts: PanelSettingsDto["fieldImpacts"];
}

export function CertificatesSection({ control, impacts }: CertificatesSectionProps) {
  return (
    <AccordionItem value="certs">
      <AccordionTrigger>Сертификаты</AccordionTrigger>
      <AccordionContent className="flex flex-col gap-4 pt-2">
        <Controller
          control={control}
          name="panelCertPublicKeyPath"
          render={({ field }) => (
            <Field>
              <FieldLabel htmlFor="cert-pub">
                Путь к файлу публичного ключа сертификата панели{" "}
                <FieldImpactBadge impact={impacts.panelCertPublicKeyPath} />
              </FieldLabel>
              <Input
                id="cert-pub"
                placeholder="/etc/xrayne/cert.pem"
                value={field.value ?? ""}
                onChange={(e) => field.onChange(e.target.value || null)}
              />
              <FieldDescription>Введите полный путь, начинающийся с '/'.</FieldDescription>
            </Field>
          )}
        />

        <Controller
          control={control}
          name="panelCertPrivateKeyPath"
          render={({ field }) => (
            <Field>
              <FieldLabel htmlFor="cert-priv">
                Путь к файлу приватного ключа сертификата панели{" "}
                <FieldImpactBadge impact={impacts.panelCertPrivateKeyPath} />
              </FieldLabel>
              <Input
                id="cert-priv"
                placeholder="/etc/xrayne/cert.key"
                value={field.value ?? ""}
                onChange={(e) => field.onChange(e.target.value || null)}
              />
              <FieldDescription>Введите полный путь, начинающийся с '/'.</FieldDescription>
            </Field>
          )}
        />
      </AccordionContent>
    </AccordionItem>
  );
}
