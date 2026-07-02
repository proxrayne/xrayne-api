import { type FormEvent, type ReactNode, useEffect, useId, useState } from "react";
import { useNavigate } from "react-router";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { toast } from "sonner";
import { z } from "zod";

import { urls } from "@core/lib/urls";
import { Button } from "@core/ui/button";
import { Input } from "@core/ui/input";
import { Spinner } from "@core/ui/spinner";
import { Field, FieldError, FieldLabel } from "@core/ui/field";

import { type NodeDto, nodeQueryKeys, useDeleteNodeMutation } from "@features/node";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@core/ui/dialog";

interface DeleteNodeDialogProps {
  node: NodeDto;
  children: ReactNode;
}

function DeleteNodeDialog({ node, children }: DeleteNodeDialogProps) {
  const [open, setOpen] = useState(false);
  const [canClose, setCanClose] = useState(true);

  const handleOpenChange = (nextOpen: boolean) => {
    if (!nextOpen && !canClose) {
      return;
    }

    setOpen(nextOpen);
  };

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogTrigger asChild>{children}</DialogTrigger>
      <DialogContent>
        <DeleteNodeDialogContent
          node={node}
          onCanCloseChange={setCanClose}
          onClose={() => setOpen(false)}
        />
      </DialogContent>
    </Dialog>
  );
}

interface DeleteNodeDialogContentProps {
  node: NodeDto;
  onCanCloseChange: (canClose: boolean) => void;
  onClose: () => void;
}

function DeleteNodeDialogContent({
  node,
  onCanCloseChange,
  onClose,
}: DeleteNodeDialogContentProps) {
  const navigate = useNavigate();
  const deleteMutation = useDeleteNodeMutation(node.id);
  const {
    handleSubmit: handleValidSubmit,
    register,
    formState,
  } = useForm({
    resolver: zodResolver(
      z.object({
        nodeName: z.literal(node.name, {
          error: () => `Type ${node.name} to confirm deletion.`,
        }),
      }),
    ),
    defaultValues: {
      nodeName: "",
    },
    mode: "onChange",
  });
  const canDelete = formState.isValid;

  useEffect(() => {
    onCanCloseChange(!deleteMutation.isPending);

    return () => onCanCloseChange(true);
  }, [deleteMutation.isPending, onCanCloseChange]);

  const errorMessage =
    formState.errors.nodeName?.message ||
    (deleteMutation.isError ? deleteMutation.error?.message || "Unable to delete node" : null);
  const inputId = `delete-node-name-input`;

  const submitDelete = handleValidSubmit(() =>
    deleteMutation.mutateAsync(void 0, {
      onSuccess: async (_d, _v, _m, { client }) => {
        await client.invalidateQueries({ queryKey: nodeQueryKeys.all, exact: true });
        client.removeQueries({ queryKey: nodeQueryKeys.detail(node.id) });
        toast.success("Node deleted");
        onClose();
        navigate(urls.nodes(), { replace: true });
      },
      onError: (error) => {
        toast.error(error?.message || "Unable to delete node");
      },
    }),
  );

  const handleFormSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    event.stopPropagation();
    void submitDelete(event);
  };

  return (
    <form className="grid gap-6" onSubmit={handleFormSubmit}>
      <DialogHeader>
        <DialogTitle>Delete node?</DialogTitle>
        <DialogDescription>
          This permanently removes the saved connection profile for{" "}
          <span className="font-medium text-foreground">{node.name}</span>. Type the node name to
          confirm.
        </DialogDescription>
      </DialogHeader>

      <Field>
        <FieldLabel htmlFor={inputId} className="text-sm font-medium">
          Node name
        </FieldLabel>
        <Input
          id={inputId}
          autoFocus
          disabled={deleteMutation.isPending}
          autoComplete="off"
          aria-invalid={Boolean(errorMessage)}
          {...register("nodeName")}
        />
        {errorMessage && <FieldError>{errorMessage}</FieldError>}
      </Field>

      <DialogFooter showCloseButton>
        <Button
          type="submit"
          variant="destructive"
          disabled={!canDelete || deleteMutation.isPending}
        >
          {deleteMutation.isPending && <Spinner />}
          Delete node
        </Button>
      </DialogFooter>
    </form>
  );
}

export default DeleteNodeDialog;
