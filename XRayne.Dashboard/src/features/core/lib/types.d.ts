declare interface CoreStatus {
  isInstalled: boolean;
  status: "started" | "starting" | "stopped" | "stopping" | "restarting" | null;
  isInstalling: boolean;
  version: string | null;
}
