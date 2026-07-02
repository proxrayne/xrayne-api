import { useEffect, useMemo, useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useQueryClient } from "@tanstack/react-query";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router";

import { useStreamPulling } from "@core/hooks/use-stream";
import { urls } from "@core/lib/urls";
import { DialogHeader, DialogTitle } from "@core/ui/dialog";

import {
  nodeQueryKeys,
  type CreateNodeResponse,
  type NodeProvisionState,
  useCreateNodeMutation,
} from "@features/node";

import { getCreateNodeErrorMessage } from "../lib/create-node-error";
import {
  buildCreateNodePayload,
  createNodeDefaultValues,
  type CreateNodeFormInput,
  createNodeSchema,
  type CreateNodeFormValues,
} from "../lib/create-node-form-schema";
import { getNodeProvisionStep } from "../lib/node-provision-step";
import CreateNodeForm from "./create-node-form";
import NodeInstallStatus from "./node-install-status";

interface CreateNodeDialogContentProps {
  onCanCloseChange: (canClose: boolean) => void;
  onClose: () => void;
}

function CreateNodeDialogContent({ onCanCloseChange, onClose }: CreateNodeDialogContentProps) {
  const [response, setResponse] = useState<CreateNodeResponse | null>(null);
  const [streamPath, setStreamPath] = useState<string | null>(null);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const createMutation = useCreateNodeMutation();
  const navigate = useNavigate();
  const query = useQueryClient();
  const form = useForm<CreateNodeFormInput, unknown, CreateNodeFormValues>({
    defaultValues: createNodeDefaultValues,
    resolver: zodResolver(createNodeSchema),
  });
  const stream = useStreamPulling<NodeProvisionState>(streamPath);
  const step = useMemo(() => {
    if (!stream.data) {
      return null;
    }

    return getNodeProvisionStep(stream.data.step);
  }, [stream.data]);
  const isFailed = step === "failed" || Boolean(stream.error);
  const isInstalling = Boolean(response && streamPath && !isFailed);
  const isBusy = createMutation.isPending || isInstalling;
  const showForm = !response || isFailed;
  const errorMessage = submitError ?? stream.data?.message ?? stream.error?.message;

  useEffect(() => {
    onCanCloseChange(!isBusy);

    return () => onCanCloseChange(true);
  }, [isBusy, onCanCloseChange]);

  useEffect(() => {
    if (!response || step !== "completed") {
      return;
    }

    void query.invalidateQueries({ queryKey: nodeQueryKeys.all });
    onClose();
    navigate(urls.node(response.node.id).toString());
  }, [navigate, onClose, query, response, step]);

  useEffect(() => {
    if (step === "failed") {
      stream.disconnect();
    }
  }, [step, stream]);

  const handleSubmit = async (values: CreateNodeFormValues) => {
    setSubmitError(null);

    try {
      const result = await createMutation.mutateAsync(buildCreateNodePayload(values));
      setResponse(result);
      setStreamPath(`nodes/${result.node.id}/install/${result.jobId}/stream`);
    } catch (error) {
      setSubmitError(getCreateNodeErrorMessage(error));
    }
  };

  return (
    <>
      <DialogHeader>
        <DialogTitle>Create node</DialogTitle>
      </DialogHeader>

      {showForm ? (
        <CreateNodeForm
          errorMessage={errorMessage}
          form={form}
          isPending={createMutation.isPending}
          onCancel={onClose}
          onSubmit={handleSubmit}
        />
      ) : (
        <NodeInstallStatus response={response} state={stream.data} step={step} />
      )}
    </>
  );
}

export default CreateNodeDialogContent;
