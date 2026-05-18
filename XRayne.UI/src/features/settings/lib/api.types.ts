export type RestartImpact = "None" | "HotReload" | "FullRestart";

export interface PanelSettingsDto {
  bindIp: string | null;
  domain: string | null;
  port: number;
  pathBase: string;
  sessionLifetimeMinutes: number;
  panelCertPublicKeyPath: string | null;
  panelCertPrivateKeyPath: string | null;
}

export interface PanelSettingsResponse {
  settings: PanelSettingsDto;
  pendingRestart: boolean;
}

export interface UpdatePanelSettingsRequest {
  bindIp: string | null;
  domain: string | null;
  port: number | null;
  pathBase?: string | null;
  sessionLifetimeMinutes: number | null;
  certPublicKeyPath: string | null;
  certPrivateKeyPath: string | null;
}

export interface UpdatePanelSettingsResponse {
  requiresRestart: boolean;
  changedFields: string[];
}
