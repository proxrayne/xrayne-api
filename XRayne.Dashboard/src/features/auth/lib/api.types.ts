export interface LoginResponseDto {
  accessToken: string;
  admin: AdminAccount;
  expireAt: Date;
}

export interface LoginOptions {
  saveMe?: boolean;
}
