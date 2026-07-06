import { api } from '../constants/api';
import type {
  QueueResponse,
  StatusResponse,
  SuspendedResponse,
  RetryResponse,
  ForceSendResponse,
  CheckerResponse,
} from '../types';

export const fetchQueue = (): Promise<QueueResponse> =>
  api.get<QueueResponse>('/queue').then((r) => r.data);

export const fetchStatus = (): Promise<StatusResponse> =>
  api.get<StatusResponse>('/status').then((r) => r.data);

export const fetchSuspended = (): Promise<SuspendedResponse> =>
  api.get<SuspendedResponse>('/suspended').then((r) => r.data);

export const retrySuspended = (
  databaseName: string,
  securityId: number,
  templateId: number
): Promise<RetryResponse> =>
  api
    .post<RetryResponse>(`/suspended/${databaseName}/${securityId}/${templateId}/retry`)
    .then((r) => r.data);

export const forceSendSuspended = (
  databaseName: string,
  securityId: number,
  templateId: number
): Promise<ForceSendResponse> =>
  api
    .post<ForceSendResponse>(`/suspended/${databaseName}/${securityId}/${templateId}/force-send`)
    .then((r) => r.data);

export const fetchChecker = (
  databaseName: string,
  securityId: number,
  templateId: number,
  quarter: number,
  itemQuarterlyCode?: number
): Promise<CheckerResponse> =>
  api
    .get<CheckerResponse>('/checker', {
      params: {
        databaseName,
        securityId,
        templateId,
        quarter,
        ...(itemQuarterlyCode ? { itemQuarterlyCode } : {}),
      },
    })
    .then((r) => r.data);
