import { useEffect, useState, useCallback, useMemo } from "react";
import { Search, RefreshCw, ChevronLeft, ChevronRight } from "lucide-react";
import { fetchQueue, forceSendSuspended } from "../api/endpoints";
import type { WaitingGroup } from "../types";
import Header from "../components/layout/Header";
import Badge from "../components/ui/Badge";
import { TableShimmer } from "../components/ui/TableShimmer";
import GroupDetailModal from "../components/modals/GroupDetailModal";
import ForceSendConfirmModal from "../components/modals/ForceSendConfirmModal";
import toast from "react-hot-toast";

const PAGE_SIZE = 15;

export default function QueuePage() {
  const [groups, setGroups] = useState<WaitingGroup[]>([]);
  const [fetchError, setFetchError] = useState<string | null>(null);
  const [revision, setRevision] = useState(0);
  const [loadedRevision, setLoadedRevision] = useState<number | null>(null);
  const [search, setSearch] = useState("");
  const [filterDb, setFilterDb] = useState("all");
  const [page, setPage] = useState(1);
  const [selectedGroup, setSelectedGroup] = useState<WaitingGroup | null>(null);
  const [forceSendGroup, setForceSendGroup] = useState<WaitingGroup | null>(null);
  const [forceSending, setForceSending] = useState<string | null>(null);

  const isInitialLoad = loadedRevision === null;
  const loading = loadedRevision !== revision;
  const refresh = useCallback(() => setRevision((r) => r + 1), []);

  useEffect(() => {
    let cancelled = false;
    fetchQueue()
      .then((data) => {
        if (!cancelled) {
          console.log("Backend'den Gelen Ham Veri:", data); // Veri yapısını kontrol edelim
          setGroups(data?.groups ?? []); // Güvenli okuma için ?. ekledik
          setFetchError(null);
          setLoadedRevision(revision);
        }
      })
      .catch((error) => {
        // Hatayı parametre olarak aldık
        if (!cancelled) {
          console.error("fetchQueue Patlama Nedeni:", error); // Gerçek hatayı konsola basıyoruz
          setFetchError(
            "Could not load queue — Worker may be down or DB unreachable",
          );
          setLoadedRevision(revision);
        }
      });
    return () => {
      cancelled = true;
    };
  }, [revision]);

  const databases = useMemo(
    () => ["all", ...Array.from(new Set(groups.map((g) => g.databaseName)))],
    [groups],
  );

  const filtered = useMemo(() => {
    return groups.filter((g) => {
      const matchSearch =
        !search ||
        g.databaseName.toLowerCase().includes(search.toLowerCase()) ||
        g.securityCode.toLowerCase().includes(search.toLowerCase());
      const matchDb = filterDb === "all" || g.databaseName === filterDb;
      return matchSearch && matchDb;
    });
  }, [groups, search, filterDb]);

  const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE));
  const paged = filtered.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);

  const handleForceSend = async (g: WaitingGroup) => {
    const key = `${g.databaseName}-${g.securityId}-${g.templateId}`;
    setForceSendGroup(null);
    setForceSending(key);
    const toastId = toast.loading(`Force sending ${g.securityCode}...`);
    try {
      await forceSendSuspended(g.databaseName, g.securityId, g.templateId);
      toast.success("Force send scheduled", { id: toastId });
      refresh();
    } catch {
      toast.error("Force send failed", { id: toastId });
    } finally {
      setForceSending(null);
    }
  };

  return (
    <div className="flex-1 flex flex-col overflow-hidden">
      <Header title="Queue" subtitle={`${filtered.length} groups`} />

      <div className="p-6 flex-1 flex flex-col space-y-4 overflow-y-auto">
        {/* Filters */}
        <div className="card flex flex-wrap gap-3 items-center">
          <div className="relative flex-1 min-w-40">
            <Search
              size={14}
              className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400"
            />
            <input
              value={search}
              onChange={(e) => {
                setSearch(e.target.value);
                setPage(1);
              }}
              placeholder="Search by database or security..."
              className="input pl-8 w-full"
            />
          </div>

          <select
            value={filterDb}
            onChange={(e) => {
              setFilterDb(e.target.value);
              setPage(1);
            }}
            className="input"
          >
            {databases.map((db) => (
              <option key={db} value={db}>
                {db === "all" ? "All Databases" : db}
              </option>
            ))}
          </select>

          <button
            onClick={refresh}
            className="btn-primary flex items-center gap-2"
          >
            <RefreshCw size={14} className={loading ? "animate-spin" : ""} />
            Refresh
          </button>
        </div>

        {/* Table */}
        <div className="card flex-1 overflow-x-auto">
          {isInitialLoad || loading ? (
            <table className="w-full text-xs">
              <thead>
                <tr className="border-b border-gray-100">
                  {["Database", "Security Code", "Template", "Items", "Status", "Wait Reason", "Actions"].map((h) => (
                    <th key={h} className="text-left text-gray-400 font-medium py-2 pr-4">{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                <TableShimmer rows={PAGE_SIZE} cols={[
                  { widthClass: "w-2/3" },
                  { widthClass: "w-1/2" },
                  { widthClass: "w-1/4" },
                  { widthClass: "w-8" },
                  { widthClass: "w-14" },
                  { widthClass: "w-3/4" },
                  { widthClass: "w-14" },
                ]} />
              </tbody>
            </table>
          ) : fetchError ? (
            <div className="flex flex-col items-center py-16 gap-2">
              <p className="text-sm text-red-500 font-medium">{fetchError}</p>
              <p className="text-xs text-gray-400">
                Check that the Worker container is running and the database is
                reachable.
              </p>
            </div>
          ) : paged.length === 0 ? (
            <p className="text-center text-gray-400 text-sm py-16">
              No groups found.
            </p>
          ) : (
            <table className="w-full text-xs">
              <thead>
                <tr className="border-b border-gray-100">
                  {[
                    "Database",
                    "Security Code",
                    "Template",
                    "Items",
                    "Status",
                    "Wait Reason",
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
                {paged.map((g) => (
                  <tr
                    key={`${g.databaseName}-${g.securityId}-${g.templateId}`}
                    className="border-b border-gray-50 hover:bg-gray-50 transition-colors"
                  >
                    <td className="py-3 pr-4 font-medium text-gray-900">
                      {g.databaseName}
                    </td>
                    <td className="py-3 pr-4 text-gray-700">
                      {g.securityCode}
                    </td>
                    <td className="py-3 pr-4 text-gray-600">#{g.templateId}</td>
                    <td className="py-3 pr-4">
                      <Badge variant="info">{g.itemCount}</Badge>
                    </td>
                    <td className="py-3 pr-4">
                      <Badge
                        variant={
                          g.status === "suspended" ? "danger"
                          : g.status === "waiting" ? "warning"
                          : "success"
                        }
                      >
                        {g.status === "suspended" ? "Suspended"
                         : g.status === "waiting" ? "Waiting"
                         : "Queued"}
                      </Badge>
                    </td>
                    <td className="py-3 pr-4 text-gray-500 text-xs max-w-xs truncate" title={g.waitReason}>
                      {g.status === "waiting" ? (g.waitReason || "—") : "—"}
                    </td>
                    <td className="py-3">
                      <div className="flex gap-2">
                        <button
                          onClick={() => setSelectedGroup(g)}
                          className="btn-secondary text-xs px-3 py-1.5"
                        >
                          View
                        </button>
                        {g.status === "waiting" && (
                          <button
                            onClick={() => setForceSendGroup(g)}
                            disabled={forceSending === `${g.databaseName}-${g.securityId}-${g.templateId}`}
                            className="btn-danger text-xs px-3 py-1.5"
                          >
                            Force Send
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>

        {/* Pagination */}
        {totalPages > 1 && (
          <div className="flex items-center justify-between text-xs text-gray-500">
            <span>
              Showing {(page - 1) * PAGE_SIZE + 1}–
              {Math.min(page * PAGE_SIZE, filtered.length)} of {filtered.length}{" "}
              groups
            </span>
            <div className="flex items-center gap-1">
              <button
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                disabled={page === 1}
                className="p-1.5 rounded hover:bg-gray-100 disabled:opacity-40"
              >
                <ChevronLeft size={14} />
              </button>
              {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
                const n = Math.max(1, Math.min(totalPages - 4, page - 2)) + i;
                return (
                  <button
                    key={n}
                    onClick={() => setPage(n)}
                    className={`w-7 h-7 rounded text-xs ${n === page ? "bg-indigo-500 text-white" : "hover:bg-gray-100"}`}
                  >
                    {n}
                  </button>
                );
              })}
              <button
                onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                disabled={page === totalPages}
                className="p-1.5 rounded hover:bg-gray-100 disabled:opacity-40"
              >
                <ChevronRight size={14} />
              </button>
            </div>
          </div>
        )}
      </div>

      <GroupDetailModal
        group={selectedGroup}
        onClose={() => setSelectedGroup(null)}
      />

      <ForceSendConfirmModal
        group={forceSendGroup}
        onClose={() => setForceSendGroup(null)}
        onConfirm={() => forceSendGroup && handleForceSend(forceSendGroup)}
      />
    </div>
  );
}
