import { useEffect, useState, useCallback } from "react";
import { AlertTriangle } from "lucide-react";
import { fetchSuspended, retrySuspended, forceSendSuspended } from "../api/endpoints";
import type { SuspendedGroup } from "../types";
import Header from "../components/layout/Header";
import Badge from "../components/ui/Badge";
import { TableShimmer } from "../components/ui/TableShimmer";
import ForceSendConfirmModal from "../components/modals/ForceSendConfirmModal";
import toast from "react-hot-toast";

function formatDate(iso: string) {
  try {
    return new Date(iso).toLocaleString();
  } catch {
    return iso;
  }
}

export default function SuspendedPage() {
  const [groups, setGroups] = useState<SuspendedGroup[]>([]);
  const [revision, setRevision] = useState(0);
  const [loadedRevision, setLoadedRevision] = useState<number | null>(null);
  const [retrying, setRetrying] = useState<string | null>(null);
  const [forceSending, setForceSending] = useState<string | null>(null);
  const [confirmGroup, setConfirmGroup] = useState<SuspendedGroup | null>(null);

  const loading = loadedRevision !== revision;
  const refresh = useCallback(() => setRevision((r) => r + 1), []);

  useEffect(() => {
    let cancelled = false;
    fetchSuspended()
      .then((data) => {
        if (!cancelled) {
          setGroups(data.groups);
          setLoadedRevision(revision);
        }
      })
      .catch(() => {
        if (!cancelled) {
          toast.error("Failed to load suspended groups");
          setLoadedRevision(revision);
        }
      });
    return () => { cancelled = true; };
  }, [revision]);

  const handleRetry = async (g: SuspendedGroup) => {
    const key = `${g.databaseName}-${g.securityId}-${g.templateId}`;
    setRetrying(key);
    const toastId = toast.loading(`Retrying ${g.securityCode}...`);
    try {
      await retrySuspended(g.databaseName, g.securityId, g.templateId);
      toast.success("Retry scheduled", { id: toastId });
      refresh();
    } catch {
      toast.error("Retry failed", { id: toastId });
    } finally {
      setRetrying(null);
    }
  };

  const handleForceSendConfirm = async () => {
    if (!confirmGroup) return;
    const g = confirmGroup;
    const key = `${g.databaseName}-${g.securityId}-${g.templateId}`;
    setForceSending(key);
    const toastId = toast.loading(`Scheduling force send for ${g.securityCode}...`);
    try {
      await forceSendSuspended(g.databaseName, g.securityId, g.templateId);
      toast.success("Force send scheduled", { id: toastId });
      setConfirmGroup(null);
      refresh();
    } catch {
      toast.error("Force send failed", { id: toastId });
    } finally {
      setForceSending(null);
    }
  };

  return (
    <div className="flex-1 flex flex-col overflow-hidden">
      <Header
        title="Suspended Groups"
        subtitle={`${groups.length} groups suspended`}
      />

      <div className="p-6 space-y-4 flex-1 overflow-y-auto">
        {groups.length > 0 && (
          <div className="flex items-center gap-3 bg-orange-50 border border-orange-200 rounded-xl px-5 py-3">
            <AlertTriangle
              size={18}
              className="text-orange-500 flex-shrink-0"
            />
            <p className="text-sm text-orange-700">
              <strong>
                {groups.length} group{groups.length !== 1 ? "s" : ""}
              </strong>{" "}
              are suspended. Review failure reasons and retry if necessary.
            </p>
          </div>
        )}

        <div className="card overflow-x-auto">
          {loading ? (
            <table className="w-full text-xs">
              <thead>
                <tr className="border-b border-gray-100">
                  {["Database", "Security", "Template", "First Error", "Attempts", "Next Retry", "Actions"].map((h) => (
                    <th key={h} className="text-left text-gray-400 font-medium py-2 pr-4">{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                <TableShimmer rows={6} cols={[
                  { widthClass: "w-2/3" },
                  { widthClass: "w-1/2" },
                  { widthClass: "w-1/4" },
                  { widthClass: "w-3/4" },
                  { widthClass: "w-8" },
                  { widthClass: "w-3/4" },
                  { widthClass: "w-20" },
                ]} />
              </tbody>
            </table>
          ) : groups.length === 0 ? (
            <p className="text-center text-gray-400 text-sm py-16">
              No suspended groups. All clear!
            </p>
          ) : (
            <table className="w-full text-xs">
              <thead>
                <tr className="border-b border-gray-100">
                  {[
                    "Database",
                    "Security",
                    "Template",
                    "First Error",
                    "Attempts",
                    "Next Retry",
                    "Actions",
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
                {groups.map((g) => {
                  const key = `${g.databaseName}-${g.securityId}-${g.templateId}`;
                  return (
                    <tr
                      key={key}
                      className="border-b border-gray-50 hover:bg-gray-50 transition-colors"
                    >
                      <td className="py-3 pr-4 font-medium text-gray-900">
                        {g.databaseName}
                      </td>
                      <td className="py-3 pr-4 text-gray-700">
                        {g.securityCode}
                      </td>
                      <td className="py-3 pr-4 text-gray-600">
                        #{g.templateId}
                      </td>
                      <td className="py-3 pr-4 text-gray-500">
                        {formatDate(g.firstFailedUtc)}
                      </td>
                      <td className="py-3 pr-4">
                        <Badge variant="danger">{g.failureCount}</Badge>
                      </td>
                      <td className="py-3 pr-4 text-gray-500">
                        {formatDate(g.nextRetryUtc)}
                      </td>
                      <td className="py-3">
                        <div className="flex items-center gap-2">
                          <button
                            onClick={() => handleRetry(g)}
                            disabled={retrying === key || forceSending === key || g.retryInProgress}
                            className="btn-primary text-xs px-3 py-1.5"
                          >
                            {retrying === key ? "Retrying..." : "Manual Retry"}
                          </button>
                          <button
                            onClick={() => setConfirmGroup(g)}
                            disabled={retrying === key || forceSending === key || g.retryInProgress}
                            className="btn-danger text-xs px-3 py-1.5"
                          >
                            Force Send
                          </button>
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          )}
        </div>
      </div>

      <ForceSendConfirmModal
        group={confirmGroup}
        onClose={() => setConfirmGroup(null)}
        onConfirm={handleForceSendConfirm}
        loading={forceSending !== null}
      />
    </div>
  );
}
