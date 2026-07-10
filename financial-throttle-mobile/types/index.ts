export interface WaitingItem {
  quarter: number;
  isOriginal: boolean;
  username: string;
  disclosureId: number;
  tableTypeId: number;
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
  waitReason?: string;
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

export interface SuspendedGroup {
  databaseName: string;
  securityId: number;
  securityCode: string;
  templateId: number;
  failureCount: number;
  firstFailedUtc: string;
  nextRetryUtc: string;
  retryInProgress: boolean;
  lastError?: string;
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

export interface ForceSendResponse {
  message: string;
}

export interface CheckerItemResult {
  itemQuarterlyCode: number | null;
  originalDefinition: string;
  inQuarterly: boolean;
  inQuarterlyOriginal: boolean;
  status: 'processed' | 'not_found';
  quarterlyValue: number | null;
  originalValue: number | null;
  valuesMatch: boolean | null;
}

export interface CheckerResponse {
  databaseName: string;
  securityId: number;
  templateId: number;
  quarter: number;
  itemQuarterlyCode?: number;
  totalCount: number;
  processedCount: number;
  notFoundCount: number;
  items: CheckerItemResult[];
}
