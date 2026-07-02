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
    | "queued"
    | "validation"
    | "downloading"
    | "extracting"
    | "installing"
    | "installed"
    | "failure";
  message: string | null;
}
