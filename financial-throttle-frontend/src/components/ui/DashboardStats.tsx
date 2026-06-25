import { Play, Clock, PauseCircle, Activity } from "lucide-react";
import type { StatusResponse } from "../../types";

function formatHeartbeat(iso: string): string {
  try {
    return new Date(iso).toLocaleTimeString();
  } catch {
    return iso;
  }
}


interface MetricCardProps {
  icon: React.ReactNode;
  iconBg: string;
  label: string;
  value: React.ReactNode;
  description: string;
}

function MetricCard({ icon, iconBg, label, value, description }: MetricCardProps) {
  return (
    <div
      className="bg-white rounded-2xl p-5 flex flex-col gap-3 hover:-translate-y-0.5 transition-transform"
      style={{
        boxShadow: "0 2px 12px rgba(0,0,0,0.07), 0 1px 3px rgba(0,0,0,0.04)",
        border: "1px solid #f0f0f0",
      }}
    >
      <div className="flex items-center gap-2.5">
        <div
          className={`w-9 h-9 rounded-xl flex items-center justify-center flex-shrink-0 ${iconBg}`}
        >
          {icon}
        </div>
        <span className="text-[11px] font-semibold text-gray-400 uppercase tracking-widest">
          {label}
        </span>
      </div>

      <div className="text-[2rem] font-bold text-gray-900 leading-none tracking-tight px-0.5">
        {value}
      </div>

      <div>
        <div className="border-t border-gray-100 mb-2.5" />
        <p className="text-xs text-gray-400 leading-snug">{description}</p>
      </div>
    </div>
  );
}

interface DashboardStatsProps {
  status: StatusResponse | null;
}

export default function DashboardStats({ status }: DashboardStatsProps) {
  const isRunning = status?.isRunning;

  return (
    <div className="grid grid-cols-2 xl:grid-cols-4 gap-4">
      <MetricCard
        icon={
          <Play
            size={17}
            className={isRunning ? "text-green-600" : "text-gray-400"}
            fill="currentColor"
          />
        }
        iconBg={isRunning ? "bg-green-50" : "bg-gray-50"}
        label="Running"
        value={
          status === null ? (
            <span className="text-gray-300">—</span>
          ) : (
            <span className={isRunning ? "text-green-600" : "text-gray-400"}>
              {isRunning ? "Running" : "Idle"}
            </span>
          )
        }
        description={
          status === null
            ? "Checking worker status…"
            : isRunning
            ? "Active right now"
            : "Worker is not responding"
        }
      />

      <MetricCard
        icon={<Clock size={17} className="text-amber-500" />}
        iconBg="bg-amber-50"
        label="Pending Groups"
        value={
          status === null ? (
            <span className="text-gray-300">—</span>
          ) : (
            status.queuedCount
          )
        }
        description="Groups awaiting processing"
      />

      <MetricCard
        icon={<PauseCircle size={17} className="text-red-500" />}
        iconBg="bg-red-50"
        label="Suspended"
        value={
          status === null ? (
            <span className="text-gray-300">—</span>
          ) : (
            <span
              className={
                (status.suspendedCount ?? 0) > 0
                  ? "text-red-600"
                  : "text-gray-900"
              }
            >
              {status.suspendedCount}
            </span>
          )
        }
        description="Require attention"
      />

      <MetricCard
        icon={<Activity size={17} className="text-purple-500" />}
        iconBg="bg-purple-50"
        label="Last Heartbeat"
        value={
          status === null ? (
            <span className="text-gray-300">—</span>
          ) : status.lastHeartbeatUtc ? (
            <span className="text-lg font-bold text-gray-900">
              {formatHeartbeat(status.lastHeartbeatUtc)}
            </span>
          ) : (
            <span className="text-gray-300">—</span>
          )
        }
        description="Last process execution time"
      />
    </div>
  );
}
