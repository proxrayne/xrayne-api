import axios, { AxiosError } from "axios";
import { addWeeks } from "date-fns";

import { ResponseError } from "@core/lib/errors";
import { cookies } from "@core/lib/cookie";
import { IS_SERVER } from "@core/lib/env";

export const api = axios.create({
  baseURL: `${getApiDomain()}/api`,
  headers: {
    "Content-Type": "application/json; charset=utf-8",
    "X-Platform": "Web",
  },
});

function getApiDomain() {
  if (import.meta.env.VITE_API_DOMAIN) {
    return import.meta.env.VITE_API_DOMAIN;
  }

  return IS_SERVER ? "" : "/";
}

api.interceptors.request.use((config) => {
  const authToken = getAuthorizationToken();
  if (authToken) {
    config.headers.Authorization = `Bearer ${authToken}`;
  }

  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error instanceof AxiosError && error.response?.status === 401) {
      clearAuthorizationToken();
    }

    if (error instanceof AxiosError) {
      return Promise.reject(ResponseError.axios(error));
    }

    return Promise.reject(error);
  },
);

const AUTH_TOKEN_KEY = "auth_token";

export function getAuthorizationToken(): string | null {
  return cookies.get(AUTH_TOKEN_KEY);
}

interface SetAuthorizationTokenOptions {
  save?: boolean;
  expire?: Date;
}

export function setAuthorizationToken(
  token: string,
  { save = true, expire }: SetAuthorizationTokenOptions = {},
) {
  cookies.set(AUTH_TOKEN_KEY, token, {
    path: "/",
    ...(save && { expires: expire ?? addWeeks(new Date(), 1) }),
  });
}

export function clearAuthorizationToken() {
  cookies.delete(AUTH_TOKEN_KEY);
  document.dispatchEvent(new CustomEvent("unauthorize"));
}
