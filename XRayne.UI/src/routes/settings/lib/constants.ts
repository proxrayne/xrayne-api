import { z } from "zod";

import { isIpAddress } from "@core/lib/network";

const nullableString = z.string().nullable();
const max256NullableString = z.string().max(256).nullable();
const max1024NullableString = z.string().max(1024).nullable();

export const formSchema = z
  .object({
    bindIp: nullableString.refine(
      (value) => value === null || isEmpty(value) || isIpAddress(value),
      "Invalid IP address",
    ),
    domain: max256NullableString,
    port: z.preprocess(
      (v) => (v === null ? null : Number(v)),
      z.number().int().min(1).max(65535).nullable(),
    ),
    pathBase: z
      .string()
      .min(1)
      .max(256)
      .regex(/^\/$|^\/.+\/$/, "PathBase must start with '/' and end with '/'.")
      .optional()
      .nullable(),
    sessionLifetimeMinutes: z.preprocess(
      (v) => (v === null ? null : Number(v)),
      z.number().int().min(1).nullable(),
    ),
    certPublicKeyPath: max1024NullableString,
    certPrivateKeyPath: max1024NullableString,
  })
  .refine(
    (value) =>
      isEmpty(value.certPublicKeyPath) === isEmpty(value.certPrivateKeyPath),
    {
      message:
        "Panel certificate public and private key paths must be provided together (or both left empty).",
      path: ["panelCertPublicKeyPath"],
    },
  );

export type FormValues = z.infer<typeof formSchema>;

function isEmpty(value: string | null | undefined) {
  return value === null || value === undefined || value.trim().length === 0;
}

export const FORM_ID = "settings-form";
