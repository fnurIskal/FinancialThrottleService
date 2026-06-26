export interface WaitingItem {
  quarter: number;
  isOriginal: boolean;
  username: string;
  disclosureId: number;
}

export interface WaitingGroup {
  databaseName: string;
  securityId: number;
  templateId: number;
  securityCode: string;
  itemCount: number;
  items: WaitingItem[];
  orderType?: number;
  status?: string;
}

export interface QueueResponse {
  groups: WaitingGroup[];
}

export interface StatusResponse {
  isRunning: boolean;
  queuedCount: number;
  suspendedCount: number;
  processedThisCycle: number;
  lastHeartbeatUtc: string;
  workerStartedUtc: string;
  avgProcessTimeMs?: number;
}

export interface LogEntry {
  id?: string;
  timestamp: string;
  level: string;
  category: string;
  message: string;
  groupKey?: string;
  securityCode?: string;
  databaseName?: string;
  securityId?: number;
  templateId?: number;
  quarter?: number;
  exception?: string | null;
  metadata?: unknown;
}

export interface LogsResponse {
  logs: LogEntry[];
  totalCount: number;
  date: string;
  category?: string;
  level?: string;
}

export interface SuspendedGroup {
  databaseName: string;
  securityId: number;
  securityCode: string;
  templateId: number;
  failureCount: number;
  firstFailedUtc: string;
  nextRetryUtc: string;
  retryInProgress: boolean;
}

export interface SuspendedResponse {
  groups: SuspendedGroup[];
  totalCount: number;
}

export interface RetryResponse {
  success: boolean;
  message: string;
  retryScheduled: boolean;
  nextRetryTime: string;
}

export interface ProcessResponse {
  success: boolean;
  message: string;
  groupKey: string;
  timestamp: string;
}

export interface ForceSendResponse {
  message: string;
}

export type Page = 'dashboard' | 'queue' | 'suspended' | 'logs' | 'settings';
