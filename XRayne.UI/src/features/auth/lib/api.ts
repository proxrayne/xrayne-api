import { api, setAuthorizationToken } from "@core/api/instance";

import { LoginOptions, LoginResponseDto } from "./api.types";

export async function login(
  username: string,
  password: string,
  { saveMe = true }: LoginOptions = {},
) {
  const { data } = await api.post<LoginResponseDto>("auth/login", {
    username,
    password,
  });

  setAuthorizationToken(data.accessToken, {
    save: saveMe,
    expire: new Date(data.expireAt),
  });

  return data;
}
