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
  port: number;
  webBasePath: string;
  sessionLifetimeMinutes: number;
  trustedProxyCidrs: string | null;
  certificatesDirectory: string | null;
  geoResourcesDirectory: string | null;
  panelCertPublicKeyPath: string | null;
  panelCertPrivateKeyPath: string | null;
}

export interface UpdatePanelSettingsResponse {
  requiresRestart: boolean;
  changedFields: string[];
}
