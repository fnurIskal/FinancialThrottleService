import { useEffect, useMemo, useState } from "react";
import { CheckCircle, XCircle } from "lucide-react";
import toast from "react-hot-toast";
import { fetchChecker } from "../api/endpoints";
import type { CheckerResponse, CheckerItemResult, CheckerState } from "../types";
import Header from "../components/layout/Header";
import Badge from "../components/ui/Badge";
import Spinner from "../components/ui/Spinner";

function BoolIcon({ value }: { value: boolean }) {
  return value ? (
    <CheckCircle size={15} className="text-green-500" />
  ) : (
    <XCircle size={15} className="text-red-400" />
  );
}

function ItemsTable({ items }: { items: CheckerItemResult[] }) {
  if (items.length === 0) {
    return (
      <p className="text-center text-gray-400 text-sm py-12">
        No items match the selected filters.
      </p>
    );
  }

  return (
    <table className="w-full text-xs">
      <thead>
        <tr className="border-b border-gray-100">
          {["Item Quarterly Code", "In Quarterly", "In Production", "Status"].map((h) => (
            <th key={h} className="text-left text-gray-400 font-medium py-2 pr-4">
              {h}
            </th>
          ))}
        </tr>
      </thead>
      <tbody>
        {items.map((item, i) => (
          <tr
            key={i}
            className="border-b border-gray-50 hover:bg-gray-50 transition-colors"
          >
            <td className="py-3 pr-4 font-medium text-gray-900">
              {item.itemQuarterlyCode ?? "—"}
            </td>
            <td className="py-3 pr-4">
              <BoolIcon value={item.inQuarterly} />
            </td>
            <td className="py-3 pr-4">
              <BoolIcon value={item.inQuarterlyOriginal} />
            </td>
            <td className="py-3 pr-4">
              <Badge variant={item.status === "processed" ? "success" : "danger"}>
                {item.status === "not_found" ? "not found" : item.status}
              </Badge>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}

interface CheckerPageProps {
  persistedState: CheckerState;
  onStateChange: (state: CheckerState) => void;
}

export default function CheckerPage({ persistedState, onStateChange }: CheckerPageProps) {
  const [databaseName, setDatabaseName] = useState(persistedState.databaseName);
  const [securityId, setSecurityId] = useState(persistedState.securityId);
  const [templateId, setTemplateId] = useState(persistedState.templateId);
  const [quarter, setQuarter] = useState(persistedState.quarter);
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState<CheckerResponse | null>(persistedState.response);
  const [showProcessed, setShowProcessed] = useState(true);
  const [showNotFound, setShowNotFound] = useState(true);

  useEffect(() => {
    onStateChange({ databaseName, securityId, templateId, quarter, response: result });
  }, [databaseName, securityId, templateId, quarter, result, onStateChange]);

  const canRun =
    !loading &&
    databaseName.trim() !== "" &&
    securityId !== "" &&
    templateId !== "" &&
    quarter !== "";

  const handleRun = async () => {
    if (!canRun) return;
    setLoading(true);
    setResult(null);
    try {
      const data = await fetchChecker(
        databaseName.trim(),
        parseInt(securityId, 10),
        parseInt(templateId, 10),
        parseInt(quarter, 10)
      );
      setResult(data);
      setShowProcessed(true);
      setShowNotFound(true);
    } catch {
      toast.error("Checker request failed — check your inputs or backend connection.");
    } finally {
      setLoading(false);
    }
  };

  const items = result?.items ?? [];
  const processedCount = items.filter((i) => i.status === "processed").length;
  const notFoundCount = items.filter((i) => i.status !== "processed").length;

  const filteredItems = useMemo(() => {
    return items.filter(
      (i) =>
        (i.status === "processed" && showProcessed) ||
        (i.status !== "processed" && showNotFound)
    );
  }, [items, showProcessed, showNotFound]);

  const subtitle = result
    ? `${result.totalCount} item${result.totalCount !== 1 ? "s" : ""} found`
    : undefined;

  return (
    <div className="flex-1 flex flex-col overflow-hidden">
      <Header title="Checker" subtitle={subtitle} />

      <div className="p-6 flex-1 flex flex-col space-y-4 overflow-y-auto">
        {/* Input Form */}
        <div className="card flex flex-wrap gap-3 items-end">
          <div className="flex flex-col gap-1 flex-1 min-w-36">
            <label className="text-xs text-gray-500 font-medium">Database Name</label>
            <input
              value={databaseName}
              onChange={(e) => setDatabaseName(e.target.value)}
              placeholder="e.g. RAS_101"
              className="input"
            />
          </div>

          <div className="flex flex-col gap-1 min-w-28">
            <label className="text-xs text-gray-500 font-medium">Security ID</label>
            <input
              type="number"
              value={securityId}
              onChange={(e) => setSecurityId(e.target.value)}
              placeholder="e.g. 282"
              className="input"
            />
          </div>

          <div className="flex flex-col gap-1 min-w-28">
            <label className="text-xs text-gray-500 font-medium">Template ID</label>
            <input
              type="number"
              value={templateId}
              onChange={(e) => setTemplateId(e.target.value)}
              placeholder="e.g. 21"
              className="input"
            />
          </div>

          <div className="flex flex-col gap-1 min-w-32">
            <label className="text-xs text-gray-500 font-medium">Quarter</label>
            <input
              type="number"
              value={quarter}
              onChange={(e) => setQuarter(e.target.value)}
              placeholder="e.g. 202503"
              className="input"
            />
          </div>

          <button
            onClick={handleRun}
            disabled={!canRun}
            className="btn-primary flex items-center gap-2 self-end disabled:opacity-50"
          >
            {loading && <Spinner size="sm" />}
            Run
          </button>
        </div>

        {/* Results */}
        {result && (
          <>
            {/* Summary */}
            <div className="card flex flex-wrap gap-6 items-center text-sm">
              <div className="flex items-center gap-2 text-gray-600">
                <span className="font-medium text-gray-900">{result.totalCount}</span>
                <span className="text-gray-400">total</span>
              </div>
              <div className="flex items-center gap-2">
                <CheckCircle size={16} className="text-green-500" />
                <span className="font-medium text-green-700">{result.processedCount}</span>
                <span className="text-gray-400">processed</span>
              </div>
              <div className="flex items-center gap-2">
                <XCircle size={16} className="text-red-400" />
                <span className="font-medium text-red-700">{result.notFoundCount}</span>
                <span className="text-gray-400">not found</span>
              </div>
            </div>

            {/* Filters */}
            <div className="card flex flex-wrap gap-6 items-center">
              <label className="flex items-center gap-2 text-sm text-gray-700 cursor-pointer select-none">
                <input
                  type="checkbox"
                  checked={showProcessed}
                  onChange={(e) => setShowProcessed(e.target.checked)}
                  className="h-4 w-4 rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
                />
                Processed
                <span className="px-1.5 py-0.5 rounded-full text-[10px] font-bold bg-gray-100 text-gray-400">
                  {processedCount}
                </span>
              </label>

              <label className="flex items-center gap-2 text-sm text-gray-700 cursor-pointer select-none">
                <input
                  type="checkbox"
                  checked={showNotFound}
                  onChange={(e) => setShowNotFound(e.target.checked)}
                  className="h-4 w-4 rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
                />
                Not Found
                <span className="px-1.5 py-0.5 rounded-full text-[10px] font-bold bg-gray-100 text-gray-400">
                  {notFoundCount}
                </span>
              </label>
            </div>

            {/* Table */}
            <div className="card flex-1 flex flex-col overflow-hidden">
              <p className="text-xs text-gray-400 font-medium mb-3">
                Showing {filteredItems.length} of {items.length}
              </p>

              <div className="overflow-x-auto">
                <ItemsTable items={filteredItems} />
              </div>
            </div>
          </>
        )}
      </div>
    </div>
  );
}
