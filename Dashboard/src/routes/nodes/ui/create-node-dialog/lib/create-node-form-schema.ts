import { z } from "zod";

import type { CreateNodeRequest } from "@features/node";

export const createNodeSchema = z
  .object({
    name: z.string().trim().min(1, "Name is required.").max(64),
    address: z.string().trim().min(1, "Address is required.").max(64),
    port: z.number().int().min(1).max(65535).default(22),
    apiPort: z.number().int().min(1).max(65535).default(8443),
    sshUsername: z.string().trim().min(1, "SSH username is required.").max(64),
    authType: z.enum(["password", "privateKey"]),
    password: z.string().max(256).optional(),
    sshKey: z.string().max(512).optional(),
    workingDirectory: z.string().trim().min(1, "Working directory is required.").max(256),
    note: z.string().trim().max(512).default(""),
  })
  .superRefine((value, ctx) => {
    if (value.authType === "password" && !value.password?.trim()) {
      ctx.addIssue({
        code: "custom",
        path: ["password"],
        message: "Password is required for password authentication.",
      });
    }

    if (value.authType === "privateKey" && !value.sshKey?.trim()) {
      ctx.addIssue({
        code: "custom",
        path: ["sshKey"],
        message: "Private key is required for private-key authentication.",
      });
    }
  });

export type CreateNodeFormInput = z.input<typeof createNodeSchema>;

export type CreateNodeFormValues = z.output<typeof createNodeSchema>;

export const createNodeDefaultValues = {
  name: "",
  address: "",
  port: 22,
  apiPort: 8443,
  sshUsername: "root",
  authType: "password",
  password: "",
  sshKey: "",
  workingDirectory: "/opt/xrayne",
  note: "",
} satisfies CreateNodeFormInput;

export function buildCreateNodePayload(values: CreateNodeFormValues): CreateNodeRequest {
  return {
    name: values.name.trim(),
    address: values.address.trim(),
    port: values.port,
    apiPort: values.apiPort,
    sshUsername: values.sshUsername.trim(),
    authType: values.authType,
    password: values.authType === "password" ? values.password?.trim() : undefined,
    sshKey: values.authType === "privateKey" ? values.sshKey?.trim() : undefined,
    workingDirectory: values.workingDirectory.trim(),
    note: values.note.trim(),
  };
}
