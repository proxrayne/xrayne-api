export type NodeStatus = "connected" | "connecting" | "error" | "disabled";

export type NodeAuthType = "password" | "privateKey";

export type CertificateMode = "domain" | "ip";

export type NodeProvisionStep =
  | "queued"
  | "preparing"
  | "uploading"
  | "installing"
  | "installingDependencies"
  | "downloadingImage"
  | "configuringCertificate"
  | "startingContainer"
  | "verifying"
  | "completed"
  | "failed"
  | number;

export interface NodeDto {
  id: number;
  name: string;
  address: string;
  port: number;
  apiPort: number;
  sshUsername: string;
  workingDirectory: string;
  note: string;
  certificateMode: CertificateMode;
  apiKeyFingerprint: string;
  xrayVersion?: string | null;
  lastStatusChange: string;
  lastSeenAt?: string | null;
  connectedAt?: string | null;
  reconnectAttemptCount: number;
  status: NodeStatus;
  authType: NodeAuthType;
  message?: string | null;
  installationMessage: string;
  createdAt: string;
  updatedAt?: string | null;
}

export interface CreateNodeRequest {
  name: string;
  address: string;
  port: number;
  apiPort: number;
  sshUsername: string;
  authType: NodeAuthType;
  sshKey?: string;
  password?: string;
  workingDirectory: string;
  note: string;
}

export interface CreateNodeResponse {
  node: NodeDto;
  jobId: string;
}

export interface NodeProvisionState {
  nodeId: number;
  jobId: string;
  step: NodeProvisionStep;
  message: string;
  updatedAt: string;
}
