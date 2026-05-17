import type { Control, FieldErrors } from "react-hook-form";
import { Controller, type UseFormRegister } from "react-hook-form";

import {
  AccordionContent,
  AccordionItem,
  AccordionTrigger,
} from "@core/ui/accordion";
import { Field, FieldDescription, FieldError, FieldLabel } from "@core/ui/field";
import { Input } from "@core/ui/input";

import type { PanelSettingsDto, UpdatePanelSettingsRequest } from "../lib/api.types";
import { FieldImpactBadge } from "./field-impact-badge";

interface PanelSectionProps {
  control: Control<UpdatePanelSettingsRequest>;
  register: UseFormRegister<UpdatePanelSettingsRequest>;
  errors: FieldErrors<UpdatePanelSettingsRequest>;
  impacts: PanelSettingsDto["fieldImpacts"];
}

export function PanelSection({ control, register, errors, impacts }: PanelSectionProps) {
  return (
    <AccordionItem value="panel">
      <AccordionTrigger>Панель</AccordionTrigger>
      <AccordionContent className="flex flex-col gap-4 pt-2">
        <Controller
          control={control}
          name="bindIp"
          render={({ field }) => (
            <Field>
              <FieldLabel htmlFor="bind-ip">
                IP-адрес для управления панелью <FieldImpactBadge impact={impacts.bindIp} />
              </FieldLabel>
              <Input
                id="bind-ip"
                placeholder="Оставьте пустым для подключения с любого IP"
                value={field.value ?? ""}
                onChange={(e) => field.onChange(e.target.value || null)}
              />
              <FieldDescription>Оставьте пустым для подключения с любого IP.</FieldDescription>
              {errors.bindIp?.message && <FieldError>{errors.bindIp.message}</FieldError>}
            </Field>
          )}
        />

        <Controller
          control={control}
          name="domain"
          render={({ field }) => (
            <Field>
              <FieldLabel htmlFor="domain">
                Домен панели <FieldImpactBadge impact={impacts.domain} />
              </FieldLabel>
              <Input
                id="domain"
                placeholder="https://panel.example.com"
                value={field.value ?? ""}
                onChange={(e) => field.onChange(e.target.value || null)}
              />
              <FieldDescription>Оставьте пустым для подключения с любых доменов и IP.</FieldDescription>
            </Field>
          )}
        />

        <Field>
          <FieldLabel htmlFor="port">
            Порт панели <FieldImpactBadge impact={impacts.port} />
          </FieldLabel>
          <Input
            id="port"
            type="number"
            min={1}
            max={65535}
            {...register("port", {
              required: true,
              valueAsNumber: true,
              min: { value: 1, message: "Порт от 1 до 65535" },
              max: { value: 65535, message: "Порт от 1 до 65535" },
            })}
          />
          <FieldDescription>Порт, на котором работает панель.</FieldDescription>
          {errors.port?.message && <FieldError>{errors.port.message}</FieldError>}
        </Field>

        <Field>
          <FieldLabel htmlFor="web-base-path">
            Корневой путь URL панели <FieldImpactBadge impact={impacts.webBasePath} />
          </FieldLabel>
          <Input
            id="web-base-path"
            placeholder="/"
            {...register("webBasePath", {
              required: true,
              pattern: {
                value: /^\/$|^\/.+\/$/,
                message: "Должен начинаться с '/' и заканчиваться '/'",
              },
            })}
          />
          <FieldDescription>Должен начинаться с '/' и заканчиваться '/'.</FieldDescription>
          {errors.webBasePath?.message && <FieldError>{errors.webBasePath.message}</FieldError>}
        </Field>

        <Field>
          <FieldLabel htmlFor="session">
            Продолжительность сессии (минуты) <FieldImpactBadge impact={impacts.sessionLifetimeMinutes} />
          </FieldLabel>
          <Input
            id="session"
            type="number"
            min={1}
            {...register("sessionLifetimeMinutes", {
              required: true,
              valueAsNumber: true,
              min: { value: 1, message: "Минимум 1 минута" },
            })}
          />
          {errors.sessionLifetimeMinutes?.message && (
            <FieldError>{errors.sessionLifetimeMinutes.message}</FieldError>
          )}
        </Field>

        <Controller
          control={control}
          name="trustedProxyCidrs"
          render={({ field }) => (
            <Field>
              <FieldLabel htmlFor="trusted-proxy">
                Trusted proxy CIDRs <FieldImpactBadge impact={impacts.trustedProxyCidrs} />
              </FieldLabel>
              <Input
                id="trusted-proxy"
                placeholder="127.0.0.1/32,::1/128"
                value={field.value ?? ""}
                onChange={(e) => field.onChange(e.target.value || null)}
              />
              <FieldDescription>
                IP/CIDR через запятую, которым разрешено устанавливать forwarded host, proto и client IP заголовки.
              </FieldDescription>
            </Field>
          )}
        />
      </AccordionContent>
    </AccordionItem>
  );
}
