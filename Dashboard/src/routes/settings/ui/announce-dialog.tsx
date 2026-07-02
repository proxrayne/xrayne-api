import { useEffect } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";

import { Button } from "@core/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@core/ui/dialog";
import { Field, FieldError, FieldLabel } from "@core/ui/field";
import { Input } from "@core/ui/input";
import { Textarea } from "@core/ui/textarea";

import type { SubscriptionAnnounce } from "@features/settings";

import { emptyToNull } from "../lib/app-settings";
import { type AnnounceForm, announceSchema } from "../lib/validation";

interface AnnounceDialogProps {
  announce?: SubscriptionAnnounce | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSave: (announce: SubscriptionAnnounce | null) => void;
}

function AnnounceDialog({ announce, open, onOpenChange, onSave }: AnnounceDialogProps) {
  const {
    formState: { errors },
    handleSubmit,
    register,
    reset,
  } = useForm<AnnounceForm>({
    defaultValues: { message: "", url: "" },
    resolver: zodResolver(announceSchema),
  });

  useEffect(() => {
    if (open) {
      reset({
        message: announce?.message ?? "",
        url: announce?.url ?? "",
      });
    }
  }, [announce, open, reset]);

  const handleSave = ({ message, url }: AnnounceForm) => {
    const nextMessage = emptyToNull(message);
    const nextUrl = emptyToNull(url);

    onSave(nextMessage || nextUrl ? { message: nextMessage, url: nextUrl } : null);
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Edit announce</DialogTitle>
          <DialogDescription>
            Preview text and target URL for subscription clients.
          </DialogDescription>
        </DialogHeader>
        <form className="grid gap-4" onSubmit={handleSubmit(handleSave)}>
          <Field>
            <FieldLabel htmlFor="announce-message">Message</FieldLabel>
            <Textarea
              id="announce-message"
              placeholder="Enter the announce message"
              {...register("message")}
            />
          </Field>
          <Field>
            <FieldLabel htmlFor="announce-url">URL</FieldLabel>
            <Input id="announce-url" placeholder="https://example.com/news" {...register("url")} />
            <FieldError>{errors.url?.message}</FieldError>
          </Field>
          <DialogFooter showCloseButton>
            <Button type="submit">Apply</Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

export default AnnounceDialog;
