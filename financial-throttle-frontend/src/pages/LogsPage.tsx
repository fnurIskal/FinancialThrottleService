import { useEffect, useState, useCallback, useMemo, useRef } from "react";
import {
  Download,
  Search,
  Calendar,
  ChevronDown,
  SlidersHorizontal,
  ChevronLeft,
  ChevronRight,
} from "lucide-react";
import { fetchLogs } from "../api/endpoints";
import type { LogEntry } from "../types";
import Header from "../components/layout/Header";
import Spinner from "../components/ui/Spinner";
import toast from "react-hot-toast";

// ── Types ─────────────────────────────────────────────────────────────────────

type CategoryFilter =
  | "all"
  | "system"
  | "financial"
  | "api"
  | "consistency"
  | "error";
type LevelFilter = "all" | "Information" | "Warning" | "Error" | "Debug";

// ── Level config ──────────────────────────────────────────────────────────────

const LEVEL_META: Record<
  string,
  { label: string; dot: string; text: string; row: string }
> = {
  information: {
    label: "INFO",
    dot: "bg-blue-400",
    text: "text-blue-300",
    row: "",
  },
  warning: {
    label: "WARN",
    dot: "bg-amber-400",
    text: "text-amber-300",
    row: "bg-amber-900/10",
  },
  error: {
    label: "ERROR",
    dot: "bg-red-400",
    text: "text-red-300",
    row: "bg-red-900/10",
  },
  debug: { label: "DEBUG", dot: "bg-gray-500", text: "text-gray-500", row: "" },
};

function getLevelMeta(level: string) {
  return LEVEL_META[level.toLowerCase()] ?? LEVEL_META.debug;
}

// ── Category badge config ─────────────────────────────────────────────────────

const CATEGORY_BADGE: Record<string, string> = {
  financial: "bg-blue-500/15   text-blue-300   border border-blue-500/30",
  system: "bg-purple-500/15 text-purple-300 border border-purple-500/30",
  api: "bg-green-500/15  text-green-300  border border-green-500/30",
  consistency: "bg-orange-500/15 text-orange-300 border border-orange-500/30",
};

function getCategoryBadge(cat: string) {
  return (
    CATEGORY_BADGE[cat.toLowerCase()] ??
    "bg-gray-700 text-gray-300 border border-gray-600"
  );
}

// ── Column layout ─────────────────────────────────────────────────────────────

const COL_W = 100;

const COLS: { label: string; width: number }[] = [
  { label: "Time", width: COL_W },
  { label: "Level", width: COL_W },
  { label: "Category", width: COL_W },
  { label: "Database", width: COL_W },
  { label: "Sec ID", width: COL_W },
  { label: "Code", width: COL_W },
  { label: "Template", width: COL_W },
  { label: "Quarter", width: COL_W },
  { label: "Message", width: 0 },
];

// ── Helpers ───────────────────────────────────────────────────────────────────

function formatTs(ts: string) {
  try {
    return new Date(ts).toLocaleTimeString("tr-TR", {
      hour12: false,
      hour: "2-digit",
      minute: "2-digit",
      second: "2-digit",
    });
  } catch {
    return ts;
  }
}

function formatDateDisplay(dateStr: string) {
  try {
    const [y, m, d] = dateStr.split("-").map(Number);
    return new Date(y, m - 1, d).toLocaleDateString("en-US", {
      year: "numeric",
      month: "short",
      day: "numeric",
    });
  } catch {
    return dateStr;
  }
}

function Dash() {
  return <span className="text-gray-600">—</span>;
}

// ── Log table row ─────────────────────────────────────────────────────────────

function LogTableRow({ log }: { log: LogEntry }) {
  const meta = getLevelMeta(log.level);
  const catBadge = getCategoryBadge(log.category);

  return (
    <>
      <tr
        className={`border-b border-gray-800/50 hover:bg-white/5 transition-colors ${meta.row}`}
      >
        <td className="px-3 py-2 text-gray-500 font-mono tabular-nums whitespace-nowrap text-[11px]">
          {formatTs(log.timestamp)}
        </td>

        <td className="px-3 py-2">
          <span
            className={`inline-flex items-center gap-1.5 font-bold text-[11px] tracking-wide ${meta.text}`}
          >
            <span
              className={`w-1.5 h-1.5 rounded-full flex-shrink-0 ${meta.dot}`}
            />
            {meta.label}
          </span>
        </td>

        <td className="px-3 py-2">
          <span
            className={`inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-semibold tracking-wide ${catBadge}`}
          >
            {log.category}
          </span>
        </td>

        <td className="px-3 py-2 text-amber-300/80 font-mono text-[11px] whitespace-nowrap overflow-hidden">
          {log.databaseName ?? <Dash />}
        </td>

        <td className="px-3 py-2 text-gray-400 font-mono tabular-nums text-[11px]">
          {log.securityId ?? <Dash />}
        </td>

        <td className="px-3 py-2 text-cyan-400 font-mono text-[11px] whitespace-nowrap overflow-hidden">
          {log.securityCode ?? <Dash />}
        </td>

        <td className="px-3 py-2 text-gray-400 font-mono tabular-nums text-[11px]">
          {log.templateId != null ? `#${log.templateId}` : <Dash />}
        </td>

        <td className="px-3 py-2 text-gray-400 font-mono tabular-nums text-[11px]">
          {log.quarter ?? <Dash />}
        </td>

        <td className="px-3 py-2 text-gray-100 leading-relaxed text-[11px]">
          {log.message}
        </td>
      </tr>

      {log.exception && (
        <tr className="border-b border-gray-800/30 bg-red-950/20">
          <td colSpan={9} className="px-3 pb-2 pt-0">
            <div className="text-red-400 font-mono text-[10px] border-l-2 border-red-600/50 pl-3 ml-2 py-0.5 break-all whitespace-pre-wrap">
              {log.exception}
            </div>
          </td>
        </tr>
      )}
    </>
  );
}

// ── Level dropdown ────────────────────────────────────────────────────────────

const LEVEL_OPTIONS: { value: LevelFilter; label: string; dot: string }[] = [
  { value: "all", label: "All Levels", dot: "bg-gray-400" },
  { value: "Information", label: "INFO", dot: "bg-blue-400" },
  { value: "Warning", label: "WARN", dot: "bg-amber-400" },
  { value: "Error", label: "ERROR", dot: "bg-red-400" },
  { value: "Debug", label: "DEBUG", dot: "bg-gray-500" },
];

function LevelDropdown({
  value,
  onChange,
}: {
  value: LevelFilter;
  onChange: (v: LevelFilter) => void;
}) {
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    function onOutside(e: MouseEvent) {
      if (ref.current && !ref.current.contains(e.target as Node))
        setOpen(false);
    }
    document.addEventListener("mousedown", onOutside);
    return () => document.removeEventListener("mousedown", onOutside);
  }, []);

  const selected =
    LEVEL_OPTIONS.find((o) => o.value === value) ?? LEVEL_OPTIONS[0];

  return (
    <div ref={ref} className="relative">
      <button
        onClick={() => setOpen((o) => !o)}
        className="flex items-center gap-2 h-[38px] px-3 bg-gray-50 border border-gray-200 rounded-lg text-sm text-gray-700 hover:bg-gray-100 transition-colors cursor-pointer whitespace-nowrap"
      >
        <SlidersHorizontal size={14} className="text-gray-400 flex-shrink-0" />
        <span className="flex items-center gap-1.5">
          {value !== "all" && (
            <span className={`w-1.5 h-1.5 rounded-full ${selected.dot}`} />
          )}
          {selected.label}
        </span>
        <ChevronDown
          size={14}
          className={`text-gray-400 transition-transform flex-shrink-0 ${open ? "rotate-180" : ""}`}
        />
      </button>

      {open && (
        <div className="absolute right-0 top-full mt-1 w-40 bg-white border border-gray-200 rounded-xl shadow-lg z-50 py-1 overflow-hidden">
          {LEVEL_OPTIONS.map((opt) => (
            <button
              key={opt.value}
              onClick={() => {
                onChange(opt.value);
                setOpen(false);
              }}
              className={`w-full flex items-center gap-2.5 px-3 py-2 text-sm transition-colors text-left ${
                value === opt.value
                  ? "bg-indigo-50 text-indigo-700 font-semibold"
                  : "text-gray-700 hover:bg-gray-50"
              }`}
            >
              <span
                className={`w-2 h-2 rounded-full flex-shrink-0 ${opt.dot}`}
              />
              {opt.label}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}

// ── Category pills ────────────────────────────────────────────────────────────

const ACTIVE_GRADIENT = "linear-gradient(135deg, #667eea 0%, #764ba2 100%)";

const CATEGORY_PILLS: { value: CategoryFilter; label: string }[] = [
  { value: "all", label: "All" },
  { value: "system", label: "System" },
  { value: "financial", label: "Financial" },
  { value: "api", label: "API" },
  { value: "consistency", label: "Consistency" },
  { value: "error", label: "Error" },
];

function CategoryPill({
  label,
  active,
  isError,
  onClick,
}: {
  label: string;
  active: boolean;
  isError?: boolean;
  onClick: () => void;
}) {
  return (
    <button
      onClick={onClick}
      style={
        active
          ? { background: isError ? "#ef4444" : ACTIVE_GRADIENT }
          : undefined
      }
      className={`px-4 py-[7px] rounded-full text-[13px] font-medium transition-all ${
        active
          ? "text-white font-semibold shadow-sm"
          : "bg-gray-100 text-gray-600 hover:bg-gray-200"
      }`}
    >
      {label}
    </button>
  );
}

const PAGE_SIZE = 50;

// ── Main page ─────────────────────────────────────────────────────────────────

export default function LogsPage() {
  const today = new Date().toISOString().split("T")[0];

  const [date, setDate] = useState(today);
  const [logs, setLogs] = useState<LogEntry[]>([]);
  const [loadedParams, setLoadedParams] = useState<string | null>(null);
  const [categoryFilter, setCategoryFilter] = useState<CategoryFilter>("all");
  const [levelFilter, setLevelFilter] = useState<LevelFilter>("all");
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [page, setPage] = useState(1);
  const [revision, setRevision] = useState(0);

  const dateInputRef = useRef<HTMLInputElement>(null);

  // 300 ms debounce on search
  useEffect(() => {
    const t = setTimeout(() => setDebouncedSearch(search), 300);
    return () => clearTimeout(t);
  }, [search]);

  // Derive API-level filter from category shortcut
  const effectiveCategory = categoryFilter === "error" ? "all" : categoryFilter;
  const effectiveLevel = categoryFilter === "error" ? "Error" : levelFilter;

  // Derive loading: true whenever the current fetch params differ from what was last resolved.
  // This avoids calling setLoading(true) synchronously inside an effect.
  const currentParams = useMemo(
    () => `${date}|${effectiveCategory}|${effectiveLevel}|${revision}`,
    [date, effectiveCategory, effectiveLevel, revision],
  );
  const loading = loadedParams !== currentParams;

  // Fetch logs — all setState calls happen inside async callbacks, never synchronously
  useEffect(() => {
    let cancelled = false;
    fetchLogs(date, effectiveCategory, effectiveLevel)
      .then((data) => {
        if (!cancelled) {
          setLogs(data.logs);
          setLoadedParams(currentParams);
        }
      })
      .catch(() => {
        if (!cancelled) {
          toast.error("Failed to load logs");
          setLogs([]);
          setLoadedParams(currentParams);
        }
      });
    return () => {
      cancelled = true;
    };
  }, [currentParams, date, effectiveCategory, effectiveLevel]);

  const refresh = useCallback(() => setRevision((r) => r + 1), []);

  // Client-side: cross-filter when both category and level are active simultaneously
  const filtered = useMemo(() => {
    let result = logs;

    if (effectiveLevel !== "all" && effectiveCategory !== "all") {
      result = result.filter(
        (l) => l.level.toLowerCase() === effectiveLevel.toLowerCase(),
      );
    }

    if (!debouncedSearch) return result;
    const q = debouncedSearch.toLowerCase();
    return result.filter(
      (l) =>
        l.message.toLowerCase().includes(q) ||
        l.securityCode?.toLowerCase().includes(q) ||
        l.databaseName?.toLowerCase().includes(q) ||
        l.groupKey?.toLowerCase().includes(q),
    );
  }, [logs, debouncedSearch, effectiveLevel, effectiveCategory]);

  const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE));
  const paginated = filtered.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);

  const handleCategoryClick = (cat: CategoryFilter) => {
    setCategoryFilter(cat);
    setPage(1);
    if (cat === "error") setLevelFilter("all");
  };

  const handleExport = () => {
    const blob = new Blob([JSON.stringify(filtered, null, 2)], {
      type: "application/json",
    });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = `logs-${date}-${effectiveCategory}-${effectiveLevel}.json`;
    a.click();
    URL.revokeObjectURL(url);
    toast.success("Logs exported");
  };

  const activeLabel =
    [
      categoryFilter !== "all" ? categoryFilter : null,
      levelFilter !== "all" && categoryFilter !== "error"
        ? levelFilter.toLowerCase()
        : null,
    ]
      .filter(Boolean)
      .join(" · ") || "all";

  return (
    <div className="flex-1 flex flex-col overflow-hidden">
      <Header title="Logs" subtitle={`${filtered.length} entries`} />

      <div className="p-6 flex-1 flex flex-col gap-4 overflow-hidden min-h-0">
        {/* ── Filter card ──────────────────────────────────────────────── */}
        <div className="card flex-shrink-0 space-y-4">
          {/* Row 1: Search · Date · Level dropdown · Export */}
          <div className="flex gap-3 items-center">
            {/* Search — fixed 300px */}
            <div className="relative w-[300px]">
              <Search
                size={14}
                className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 pointer-events-none"
              />
              <input
                value={search}
                onChange={(e) => {
                  setSearch(e.target.value);
                  setPage(1);
                }}
                placeholder="Search logs…"
                className="w-full h-[38px] pl-9 pr-3 bg-gray-50 border border-gray-200 rounded-lg text-sm text-gray-800 placeholder-gray-400 outline-none focus:border-indigo-400 focus:ring-1 focus:ring-indigo-200 transition-colors"
              />
            </div>

            {/* Date picker — styled button over hidden native input */}
            <div
              onClick={() => dateInputRef.current?.showPicker?.()}
              className=" hover:bg-gray-100 relative flex-shrink-0"
            >
              <button className="flex items-center gap-2 h-[38px] px-3 bg-gray-50 border border-gray-200 rounded-lg text-sm text-gray-700 transition-colors cursor-pointer whitespace-nowrap">
                <Calendar size={14} className="text-gray-400 flex-shrink-0" />
                {formatDateDisplay(date)}
                <ChevronDown
                  size={14}
                  className="text-gray-400 flex-shrink-0"
                />
              </button>
              <input
                ref={dateInputRef}
                type="date"
                value={date}
                onChange={(e) => {
                  if (e.target.value) {
                    setDate(e.target.value);
                    setPage(1);
                  }
                }}
                className="absolute inset-0 opacity-0 w-full h-full cursor-pointer"
                tabIndex={-1}
              />
            </div>

            {/* Level dropdown */}
            <LevelDropdown
              value={categoryFilter === "error" ? "Error" : levelFilter}
              onChange={(v) => {
                setLevelFilter(v);
                setPage(1);
                if (categoryFilter === "error") setCategoryFilter("all");
              }}
            />

            {/* Export */}
            <button
              onClick={handleExport}
              className="flex items-center gap-2 h-[38px] px-4 bg-gray-50 border border-gray-200 rounded-lg text-sm text-gray-700 hover:bg-gray-100 transition-colors cursor-pointer flex-shrink-0"
            >
              <Download size={14} className="text-gray-400" />
              Export
            </button>
          </div>

          {/* Row 2: Category pills */}
          <div className="flex flex-wrap gap-2">
            {CATEGORY_PILLS.map(({ value, label }) => (
              <CategoryPill
                key={value}
                label={label}
                active={categoryFilter === value}
                isError={value === "error"}
                onClick={() => handleCategoryClick(value)}
              />
            ))}
          </div>
        </div>

        {/* ── Log table ────────────────────────────────────────────────── */}
        <div className="bg-gray-900 rounded-xl flex-1 min-h-0 w-full flex flex-col overflow-hidden">
          {/* macOS traffic-light bar */}
          <div className="flex items-center gap-2 px-4 py-2.5 border-b border-gray-800 flex-shrink-0">
            <span className="w-3 h-3 rounded-full bg-red-500/70" />
            <span className="w-3 h-3 rounded-full bg-yellow-500/70" />
            <span className="w-3 h-3 rounded-full bg-green-500/70" />
            <span className="ml-3 text-xs text-gray-500 font-mono">
              logs / {date} / {activeLabel} — {filtered.length} entries · page{" "}
              {page}/{totalPages}
            </span>
          </div>

          {/* Scrollable table */}
          <div className="flex-1 overflow-auto min-h-0">
            {loading ? (
              <div className="flex justify-center py-16">
                <Spinner />
              </div>
            ) : filtered.length === 0 ? (
              <p className="text-gray-500 text-center py-16 text-sm">
                No logs found for the selected filters.
              </p>
            ) : (
              <table
                className="w-full"
                style={{
                  tableLayout: "fixed",
                  borderCollapse: "collapse",
                  minWidth: 900,
                }}
              >
                <colgroup>
                  {COLS.map((c, i) => (
                    <col
                      key={i}
                      style={c.width ? { width: c.width } : undefined}
                    />
                  ))}
                </colgroup>

                <thead className="sticky top-0 z-10">
                  <tr className="bg-gray-800 border-b border-gray-700">
                    {COLS.map((c) => (
                      <th
                        key={c.label}
                        className="text-left text-gray-400 font-semibold px-3 py-2.5 tracking-widest uppercase text-[10px]"
                      >
                        {c.label}
                      </th>
                    ))}
                  </tr>
                </thead>

                <tbody>
                  {paginated.map((log, i) => (
                    <LogTableRow key={log.id ?? i} log={log} />
                  ))}
                </tbody>
              </table>
            )}
          </div>

          {/* Pagination bar */}
          {!loading && filtered.length > 0 && (
            <div className="flex-shrink-0 flex items-center justify-between px-4 py-2.5 border-t border-gray-800 bg-gray-900/80">
              <span className="text-xs text-gray-500 font-mono tabular-nums">
                {(page - 1) * PAGE_SIZE + 1}–
                {Math.min(page * PAGE_SIZE, filtered.length)} of{" "}
                {filtered.length}
              </span>

              <div className="flex items-center gap-1">
                <button
                  onClick={() => setPage(1)}
                  disabled={page === 1}
                  className="px-2 py-1 rounded text-xs text-gray-400 hover:text-white hover:bg-gray-700 disabled:opacity-30 disabled:cursor-not-allowed transition-colors"
                >
                  «
                </button>
                <button
                  onClick={() => setPage((p) => Math.max(1, p - 1))}
                  disabled={page === 1}
                  className="p-1 rounded text-gray-400 hover:text-white hover:bg-gray-700 disabled:opacity-30 disabled:cursor-not-allowed transition-colors"
                >
                  <ChevronLeft size={14} />
                </button>

                {/* Page number pills */}
                {Array.from({ length: totalPages }, (_, i) => i + 1)
                  .filter(
                    (p) =>
                      p === 1 || p === totalPages || Math.abs(p - page) <= 1,
                  )
                  .reduce<(number | "…")[]>((acc, p, idx, arr) => {
                    if (idx > 0 && p - (arr[idx - 1] as number) > 1)
                      acc.push("…");
                    acc.push(p);
                    return acc;
                  }, [])
                  .map((p, i) =>
                    p === "…" ? (
                      <span
                        key={`ellipsis-${i}`}
                        className="px-1 text-xs text-gray-600"
                      >
                        …
                      </span>
                    ) : (
                      <button
                        key={p}
                        onClick={() => setPage(p as number)}
                        className={`min-w-[28px] px-2 py-1 rounded text-xs font-mono transition-colors ${
                          page === p
                            ? "bg-indigo-600 text-white font-semibold"
                            : "text-gray-400 hover:text-white hover:bg-gray-700"
                        }`}
                      >
                        {p}
                      </button>
                    ),
                  )}

                <button
                  onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                  disabled={page === totalPages}
                  className="p-1 rounded text-gray-400 hover:text-white hover:bg-gray-700 disabled:opacity-30 disabled:cursor-not-allowed transition-colors"
                >
                  <ChevronRight size={14} />
                </button>
                <button
                  onClick={() => setPage(totalPages)}
                  disabled={page === totalPages}
                  className="px-2 py-1 rounded text-xs text-gray-400 hover:text-white hover:bg-gray-700 disabled:opacity-30 disabled:cursor-not-allowed transition-colors"
                >
                  »
                </button>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
