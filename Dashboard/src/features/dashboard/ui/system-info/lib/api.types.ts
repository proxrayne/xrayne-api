export interface SystemInfoDto {
  cpu: CpuInfo;
  memory: MemoryInfo;
  swap: MemoryInfo;
  storage: StorageInfo;
  uptime: string;
  currentProcessThreadCount: number;
  systemThreadCount: number | null;
  network: NetworkInfo;
}

export interface CpuInfo {
  logicalCoreCount: number;
  averageUsagePercent: number;
  cores: CpuCoreInfo[];
}

export interface CpuCoreInfo {
  index: number;
  usagePercent: number | null;
}

export interface MemoryInfo {
  totalBytes: number;
  usedBytes: number;
  availableBytes: number;
}

export interface StorageInfo {
  applicationDirectory: DirectorySizeInfo;
  downloadsDirectory: DirectorySizeInfo;
  applicationDirectoryUsedDiskPercent: number;
}

export interface DirectorySizeInfo {
  path: string;
  sizeBytes: number;
}

export interface NetworkInfo {
  iPv4Addresses: string[];
  iPv6Addresses: string[];
}
