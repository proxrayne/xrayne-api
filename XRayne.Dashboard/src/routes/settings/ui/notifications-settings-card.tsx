import { useState } from "react";
import { useMutation } from "@tanstack/react-query";
import { BellIcon, Edit3Icon, LockIcon, PlusIcon, Trash2Icon, UnlockIcon } from "lucide-react";
import { toast } from "sonner";

import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@core/ui/alert-dialog";
import { Badge } from "@core/ui/badge";
import { Button } from "@core/ui/button";
import { Card, CardAction, CardContent, CardHeader, CardTitle } from "@core/ui/card";
import { Empty, EmptyAction, EmptyDescription, EmptyMedia, EmptyTitle } from "@core/ui/empty";
import { Spinner } from "@core/ui/spinner";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@core/ui/table";

import { type AppWebhookDto, deleteAppWebhook, useAppSettings } from "@features/settings";

import { formatEventLabel } from "../lib/app-settings";
import WebhookDialog from "./webhook-dialog";

interface NotificationsSettingsCardProps {
  webhooks: AppWebhookDto[];
}

function NotificationsSettingsCard({ webhooks }: NotificationsSettingsCardProps) {
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingWebhook, setEditingWebhook] = useState<AppWebhookDto | null>(null);
  const [deletingWebhook, setDeletingWebhook] = useState<AppWebhookDto | null>(null);

  const deleteMutation = useMutation({
    mutationKey: ["app-settings", "webhooks", "delete", deletingWebhook?.id],
    mutationFn: (id: string) => deleteAppWebhook(id),
    onSuccess: async () => {
      await useAppSettings.invalidate();
      toast.success("Webhook deleted");
      setDeletingWebhook(null);
    },
    onError: (error) => {
      toast.error(error?.message || "Unable to delete webhook");
    },
  });

  const openCreateDialog = () => {
    setEditingWebhook(null);
    setDialogOpen(true);
  };

  const openEditDialog = (webhook: AppWebhookDto) => {
    setEditingWebhook(webhook);
    setDialogOpen(true);
  };

  return (
    <Card className="w-full">
      <CardHeader>
        <CardTitle>Notifications</CardTitle>
        {webhooks.length ? (
          <CardAction>
            <Button onClick={openCreateDialog}>
              <PlusIcon />
              Add
            </Button>
          </CardAction>
        ) : null}
      </CardHeader>
      <CardContent>
        {webhooks.length ? (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>URL</TableHead>
                <TableHead>Events</TableHead>
                <TableHead>Secret</TableHead>
                <TableHead className="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {webhooks.map((webhook) => (
                <TableRow key={webhook.id}>
                  <TableCell className="max-w-[18rem] truncate font-medium">
                    <a
                      href={webhook.url}
                      target="_blank"
                      className="hover:underline underline-offset-2"
                    >
                      {webhook.url}
                    </a>
                  </TableCell>
                  <TableCell className="max-w-75">
                    {webhook.events.length ? (
                      <div className="flex flex-wrap gap-1.5">
                        {webhook.events.map((event) => (
                          <Badge key={event} variant="outline">
                            {formatEventLabel(event)}
                          </Badge>
                        ))}
                      </div>
                    ) : (
                      <Badge variant="outline" className="text-muted-foreground">
                        Disabled
                      </Badge>
                    )}
                  </TableCell>
                  <TableCell
                    className="w-10 text-muted-foreground"
                    align="center"
                    title={webhook.hasSecret ? "Secret" : "No secret"}
                  >
                    {webhook.hasSecret ? (
                      <LockIcon className="size-4" aria-hidden="true" />
                    ) : (
                      <UnlockIcon className="size-4" aria-hidden="true" />
                    )}
                    <span className="sr-only">
                      {webhook.hasSecret ? "Secret configured" : "No secret configured"}
                    </span>
                  </TableCell>
                  <TableCell>
                    <div className="flex justify-end gap-2">
                      <Button
                        variant="secondary"
                        size="icon-sm"
                        onClick={() => openEditDialog(webhook)}
                      >
                        <Edit3Icon />
                        <span className="sr-only">Edit webhook</span>
                      </Button>
                      <Button
                        variant="destructive"
                        size="icon-sm"
                        onClick={() => setDeletingWebhook(webhook)}
                      >
                        <Trash2Icon />
                        <span className="sr-only">Delete webhook</span>
                      </Button>
                    </div>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        ) : (
          <Empty>
            <EmptyMedia>
              <BellIcon />
            </EmptyMedia>
            <EmptyTitle>No webhooks configured</EmptyTitle>
            <EmptyDescription>Notification events will stay inside the panel.</EmptyDescription>
            <EmptyAction>
              <Button onClick={openCreateDialog}>
                <PlusIcon />
                Create
              </Button>
            </EmptyAction>
          </Empty>
        )}
      </CardContent>
      <WebhookDialog webhook={editingWebhook} open={dialogOpen} onOpenChange={setDialogOpen} />
      <AlertDialog
        open={Boolean(deletingWebhook)}
        onOpenChange={(open) => !open && setDeletingWebhook(null)}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete webhook?</AlertDialogTitle>
            <AlertDialogDescription>
              This webhook will stop receiving configured notification events.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel asChild>
              <Button variant="outline">Cancel</Button>
            </AlertDialogCancel>
            <AlertDialogAction asChild>
              <Button
                variant="destructive"
                disabled={deleteMutation.isPending}
                onClick={() => deletingWebhook && deleteMutation.mutate(deletingWebhook.id)}
              >
                {deleteMutation.isPending && <Spinner />}
                Delete
              </Button>
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </Card>
  );
}

export default NotificationsSettingsCard;
