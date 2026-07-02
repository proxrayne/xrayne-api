import { AxiosError } from "axios";

export function getCreateNodeErrorMessage(error: unknown) {
  if (error instanceof AxiosError) {
    const detail = error.response?.data?.detail;

    return typeof detail === "string" ? detail : error.message;
  }

  return error instanceof Error ? error.message : "Unexpected error.";
}
