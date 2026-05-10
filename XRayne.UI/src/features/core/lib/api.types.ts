export interface CoreStatusDto {
  isInstalled: boolean;
  isStarted: boolean;
  version: string | null;
}

export interface FetchXrayReleasesQuery {
  perPage?: number;
  page?: number;
}

export interface GitHubReleaseDto {
  id: number;
  tagName: string;
  htmlUrl: string;
  prerelease: boolean;
  createdAt: string;
  publishedAt: string;
}

export interface CoreInstallingStatus {
  step:
    | "Idle"
    | "Version"
    | "Preparing"
    | "Failure"
    | "Downloading"
    | "Extracting"
    | "SettingUp";
  message: string;
}
