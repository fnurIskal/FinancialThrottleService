import { api } from './client';
import type {
  QueueResponse,
  StatusResponse,
  LogsResponse,
  SuspendedResponse,
  RetryResponse,
} from '../types';

export const fetchQueue = (): Promise<QueueResponse> =>
  api.get<QueueResponse>('/queue').then(r => r.data);

export const fetchStatus = (): Promise<StatusResponse> =>
  api.get<StatusResponse>('/status').then(r => r.data);

export const fetchLogs = (
  date: string,
  category?: string,
  level?: string
): Promise<LogsResponse> => {
  const hasCategory = category && category !== 'all';
  const hasLevel = level && level !== 'all';

  if (hasCategory) {
    return api.get<LogsResponse>(`/logs/${date}/${category}`).then(r => r.data);
  }
  if (hasLevel) {
    return api.get<LogsResponse>(`/logs/${date}/level/${level}`).then(r => r.data);
  }
  return api.get<LogsResponse>(`/logs/${date}`).then(r => r.data);
};

export const fetchSuspended = (): Promise<SuspendedResponse> =>
  api.get<SuspendedResponse>('/suspended').then(r => r.data);

export const retrySuspended = (
  databaseName: string,
  securityId: number,
  templateId: number
): Promise<RetryResponse> =>
  api
    .post<RetryResponse>(`/suspended/${databaseName}/${securityId}/${templateId}/retry`)
    .then(r => r.data);
