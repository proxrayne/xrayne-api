import type { AxiosError } from "axios";

export class ResponseError extends Error {
  constructor(
    message: string,
    name: string = "UnknownError",
    public status: number = 400,
  ) {
    super(message);
    this.name = name;
  }

  static axios(error: AxiosError<{ detail: string; name: string; status: number }>) {
    let message: string;
    if (error.response?.data?.detail) {
      message = error.response.data.detail;
    } else if (error.message) {
      message = error.message;
    } else {
      message = "Unknown error";
    }

    return new ResponseError(message, error?.response?.data?.name, error.response?.status);
  }
}
