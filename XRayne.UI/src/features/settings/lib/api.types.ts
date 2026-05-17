export type RestartImpact = "None" | "HotReload" | "FullRestart";

export interface PanelSettings {
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
  pendingRestart: boolean;
  fieldImpacts: Record<string, RestartImpact>;
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
  hotReloaded: string[];
}
