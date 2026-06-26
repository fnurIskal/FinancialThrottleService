import type {
  QueueResponse,
  StatusResponse,
  LogsResponse,
  SuspendedResponse,
  RetryResponse,
  ProcessResponse,
  ForceSendResponse,
} from "../types";

const delay = (ms = 400) => new Promise((r) => setTimeout(r, ms));

export const mockQueue: QueueResponse = {
  groups: [
    {
      databaseName: "RAS_STAJ107",
      securityId: 282,
      templateId: 21,
      securityCode: "THYAO",
      orderType: 30,
      itemCount: 2,
      items: [
        {
          quarter: 202409,
          isOriginal: false,
          username: "boss",
          disclosureId: 1001,
        },
        {
          quarter: 202406,
          isOriginal: true,
          username: "boss",
          disclosureId: 1002,
        },
      ],
    },
    {
      databaseName: "RAS_STAJ107",
      securityId: 292,
      templateId: 21,
      securityCode: "SASA",
      orderType: 30,
      itemCount: 1,
      items: [
        {
          quarter: 202409,
          isOriginal: false,
          username: "analyst1",
          disclosureId: 1003,
        },
      ],
    },
    {
      databaseName: "RAS_STAJ32501",
      securityId: 50,
      templateId: 241,
      securityCode: "MSSTOCK",
      orderType: 10,
      itemCount: 1,
      items: [
        {
          quarter: 202412,
          isOriginal: false,
          username: "analyst2",
          disclosureId: 2001,
        },
      ],
    },
    {
      databaseName: "RAS_STAJ107",
      securityId: 346,
      templateId: 21,
      securityCode: "AKBNK",
      orderType: 30,
      itemCount: 2,
      items: [
        {
          quarter: 202409,
          isOriginal: false,
          username: "boss",
          disclosureId: 1004,
        },
        {
          quarter: 202406,
          isOriginal: false,
          username: "boss",
          disclosureId: 1005,
        },
      ],
    },
    {
      databaseName: "RAS_STAJ107",
      securityId: 101,
      templateId: 31,
      securityCode: "EREGL",
      orderType: 20,
      itemCount: 3,
      items: [
        {
          quarter: 202503,
          isOriginal: false,
          username: "Sentris",
          disclosureId: 1605794,
        },
        {
          quarter: 202512,
          isOriginal: false,
          username: "Sentris",
          disclosureId: 1605795,
        },
        {
          quarter: 202603,
          isOriginal: true,
          username: "Sentris",
          disclosureId: 1605796,
        },
      ],
    },
  ],
};

export const mockStatus: StatusResponse = {
  isRunning: true,
  queuedCount: 5,
  suspendedCount: 2,
  processedThisCycle: 3,
  lastHeartbeatUtc: new Date(Date.now() - 15000).toISOString(),
  workerStartedUtc: new Date(Date.now() - 2 * 60 * 60 * 1000).toISOString(),
  avgProcessTimeMs: 84000,
};

export const mockSuspended: SuspendedResponse = {
  groups: [
    {
      databaseName: "RAS_STAJ107",
      securityId: 999,
      securityCode: "TESTSEC",
      templateId: 21,
      failureCount: 3,
      firstFailedUtc: new Date(Date.now() - 3 * 60 * 60 * 1000).toISOString(),
      nextRetryUtc: new Date(Date.now() + 60 * 60 * 1000).toISOString(),
      retryInProgress: false,
    },
    {
      databaseName: "RAS_STAJ32501",
      securityId: 456,
      securityCode: "SEC_456",
      templateId: 241,
      failureCount: 10,
      firstFailedUtc: new Date(Date.now() - 6 * 60 * 60 * 1000).toISOString(),
      nextRetryUtc: new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString(),
      retryInProgress: false,
    },
  ],
  totalCount: 2,
};

const mockLogMessages = [
  {
    level: "Information" as const,
    category: "financial" as const,
    message: "GÖNDER: RAS_STAJ107|282|21 Quarter=202409",
    groupKey: "RAS_STAJ107|282|21",
  },
  {
    level: "Information" as const,
    category: "system" as const,
    message: "Heartbeat written successfully",
    groupKey: "",
  },
  {
    level: "Warning" as const,
    category: "financial" as const,
    message: "BEKLE: RAS_STAJ107|292|21 — missing TableTypeId [3]",
    groupKey: "RAS_STAJ107|292|21",
  },
  {
    level: "Information" as const,
    category: "api" as const,
    message: "GET /api/queue responded 200 OK in 42ms",
    groupKey: "",
  },
  {
    level: "Error" as const,
    category: "financial" as const,
    message: "HATA: RAS_STAJ32501|50|241 — Sql timeout after 30s",
    groupKey: "RAS_STAJ32501|50|241",
  },
  {
    level: "Debug" as const,
    category: "consistency" as const,
    message: "ConsistencyCheck triggered for RAS_STAJ107|282|21",
    groupKey: "RAS_STAJ107|282|21",
  },
  {
    level: "Information" as const,
    category: "financial" as const,
    message: "GÖNDER: RAS_STAJ107|346|21 Quarter=202409",
    groupKey: "RAS_STAJ107|346|21",
  },
  {
    level: "Warning" as const,
    category: "system" as const,
    message: "Worker cycle took 28.4s (threshold: 25s)",
    groupKey: "",
  },
  {
    level: "Information" as const,
    category: "api" as const,
    message: "POST /api/queue/.../process responded 200 OK",
    groupKey: "",
  },
  {
    level: "Error" as const,
    category: "system" as const,
    message: "gRPC connection refused — Worker not reachable",
    groupKey: "",
  },
];

export const mockLogs = (date: string): LogsResponse => ({
  date,
  totalCount: mockLogMessages.length,
  logs: mockLogMessages.map((m, i) => ({
    id: `mock_${i}`,
    timestamp: new Date(
      Date.now() - (mockLogMessages.length - i) * 90000,
    ).toISOString(),
    level: m.level,
    category: m.category,
    message: m.message,
    groupKey: m.groupKey,
    securityCode: m.groupKey.split("|")[1] ?? "",
    databaseName: m.groupKey.split("|")[0] ?? "",
    securityId: parseInt(m.groupKey.split("|")[1] ?? "0") || 0,
    templateId: parseInt(m.groupKey.split("|")[2] ?? "0") || 0,
    exception: m.level === "Error" ? "SqlException: Timeout expired" : null,
    metadata: null,
  })),
});

// Mock API functions — same signature as real endpoints
export const mockFetchQueue = async (): Promise<QueueResponse> => {
  await delay();
  return mockQueue;
};

export const mockFetchStatus = async (): Promise<StatusResponse> => {
  await delay(200);
  return { ...mockStatus };
};

export const mockFetchLogs = async (date: string): Promise<LogsResponse> => {
  await delay();
  return mockLogs(date);
};

export const mockFetchSuspended = async (): Promise<SuspendedResponse> => {
  await delay();
  return mockSuspended;
};

export const mockProcessGroup = async (
  databaseName: string,
  securityId: number,
  templateId: number,
): Promise<ProcessResponse> => {
  await delay(800);
  return {
    success: true,
    message: `Group ${databaseName}|${securityId}|${templateId} queued for processing`,
    groupKey: `${databaseName}|${securityId}|${templateId}`,
    timestamp: new Date().toISOString(),
  };
};

export const mockRetrySuspended = async (
  databaseName: string,
  securityId: number,
  templateId: number,
): Promise<RetryResponse> => {
  await delay(600);
  return {
    success: true,
    message: `Retry scheduled for ${databaseName}|${securityId}|${templateId}`,
    retryScheduled: true,
    nextRetryTime: new Date(Date.now() + 5 * 60 * 1000).toISOString(),
  };
};

export const mockForceSendSuspended = async (
  databaseName: string,
  securityId: number,
  templateId: number,
): Promise<ForceSendResponse> => {
  await delay(600);
  return {
    message: `${databaseName}|${securityId}|${templateId} will be force-sent on next cycle`,
  };
};
