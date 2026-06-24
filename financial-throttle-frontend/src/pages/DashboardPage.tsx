import { useEffect, useState, useCallback } from "react";
import { RefreshCw } from "lucide-react";
import { fetchStatus, fetchQueue } from "../api/endpoints";
import type { StatusResponse, WaitingGroup } from "../types";
import Header from "../components/layout/Header";
import Badge from "../components/ui/Badge";
import Spinner from "../components/ui/Spinner";
import DashboardStats from "../components/ui/DashboardStats";
import GroupDetailModal from "../components/modals/GroupDetailModal";
import toast from "react-hot-toast";

function formatTime(iso: string) {
  try {
    return new Date(iso).toLocaleTimeString();
  } catch {
    return iso;
  }
}

export default function DashboardPage() {
  const [status, setStatus] = useState<StatusResponse | null>(null);
  const [groups, setGroups] = useState<WaitingGroup[]>([]);
  const [queueError, setQueueError] = useState<string | null>(null);
  const [revision, setRevision] = useState(0);
  const [loadedRevision, setLoadedRevision] = useState<number | null>(null);
  const [selectedGroup, setSelectedGroup] = useState<WaitingGroup | null>(null);

  // Initial load: no data yet. Background refresh: keep stale data visible.
  const isInitialLoad = loadedRevision === null;
  const isRefreshing = !isInitialLoad && loadedRevision !== revision;
  const refresh = useCallback(() => setRevision((r) => r + 1), []);

  useEffect(() => {
    let cancelled = false;

    const loadStatus = fetchStatus()
      .then((s) => { if (!cancelled) setStatus(s); })
      .catch(() => { if (!cancelled) toast.error("Worker unreachable — status unavailable"); });

    const loadQueue = fetchQueue()
      .then((q) => {
        if (!cancelled) {
          setGroups(q.groups.slice(0, 5));
          setQueueError(null);
        }
      })
      .catch(() => {
        if (!cancelled) setQueueError("Could not load queue — Worker may be down");
      });

    Promise.allSettled([loadStatus, loadQueue]).then(() => {
      if (!cancelled) setLoadedRevision(revision);
    });

    return () => { cancelled = true; };
  }, [revision]);

  useEffect(() => {
    const timer = setInterval(() => setRevision((r) => r + 1), 30000);
    return () => clearInterval(timer);
  }, []);


  return (
    <div className="flex-1 flex flex-col overflow-hidden">
      <Header
        title="Dashboard"
        subtitle={`Last updated: ${status ? formatTime(status.lastHeartbeatUtc) : "—"}`}
      />

      <div className="flex-1 p-6 space-y-6 overflow-y-auto">
        {/* Status cards */}
        <DashboardStats status={status} />

        {/* Last 5 processed groups */}
        <div className="card">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-sm font-bold text-gray-900">
              Last 5 Queue Groups
            </h2>
            <button
              onClick={refresh}
              disabled={isRefreshing}
              className="text-xs text-indigo-500 flex items-center gap-1 hover:underline disabled:opacity-50"
            >
              <RefreshCw size={12} className={isRefreshing ? "animate-spin" : ""} /> Refresh
            </button>
          </div>

          {isInitialLoad ? (
            <div className="flex justify-center py-10">
              <Spinner />
            </div>
          ) : queueError ? (
            <div className="flex flex-col items-center py-10 gap-2">
              <p className="text-sm text-red-500 font-medium">{queueError}</p>
              <p className="text-xs text-gray-400">Check that the Worker container is running and healthy.</p>
            </div>
          ) : groups.length === 0 ? (
            <p className="text-center text-gray-400 text-sm py-10">
              Queue is empty — no groups waiting to be processed.
            </p>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-xs">
                <thead>
                  <tr className="border-b border-gray-100">
                    {[
                      "Database",
                      "Security",
                      "Template",
                      "Items",
                      "Status",
                    ].map((h) => (
                      <th
                        key={h}
                        className="text-left text-gray-400 font-medium py-2 pr-4"
                      >
                        {h}
                      </th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {groups.map((g) => (
                    <tr
                      key={`${g.databaseName}-${g.securityId}-${g.templateId}`}
                      className="table-row-hover border-b border-gray-50"
                      onClick={() => setSelectedGroup(g)}
                    >
                      <td className="py-3 pr-4 font-medium text-gray-900">
                        {g.databaseName}
                      </td>
                      <td className="py-3 pr-4 text-gray-600">
                        {g.securityCode}
                      </td>
                      <td className="py-3 pr-4 text-gray-600">
                        #{g.templateId}
                      </td>
                      <td className="py-3 pr-4">
                        <Badge variant="info">{g.itemCount}</Badge>
                      </td>
                      <td className="py-3">
                        <Badge variant="success">Queued</Badge>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>

      <GroupDetailModal
        group={selectedGroup}
        onClose={() => setSelectedGroup(null)}
      />
    </div>
  );
}
