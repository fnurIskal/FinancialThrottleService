function ShimmerCell({ widthClass = "w-3/4" }: { widthClass?: string }) {
  return (
    <td className="py-3 pr-4">
      <div className={`h-3 rounded bg-gray-200 animate-pulse ${widthClass}`} />
    </td>
  );
}

function DarkShimmerCell({ widthClass = "w-3/4" }: { widthClass?: string }) {
  return (
    <td className="px-3 py-2">
      <div className={`h-3 rounded bg-gray-700 animate-pulse ${widthClass}`} />
    </td>
  );
}

/** Shimmer skeleton for light-themed tables (Dashboard, Queue) */
export function TableShimmer({
  rows = 5,
  cols,
}: {
  rows?: number;
  cols: { widthClass?: string }[];
}) {
  return (
    <>
      {Array.from({ length: rows }).map((_, r) => (
        <tr key={r} className="border-b border-gray-50">
          {cols.map((c, i) => (
            <ShimmerCell key={i} widthClass={c.widthClass} />
          ))}
        </tr>
      ))}
    </>
  );
}

/** Shimmer skeleton for the dark log table */
export function LogTableShimmer({ rows = 12 }: { rows?: number }) {
  const colWidths = [
    "w-16",
    "w-12",
    "w-20",
    "w-24",
    "w-10",
    "w-14",
    "w-10",
    "w-16",
    "w-full",
  ];
  return (
    <>
      {Array.from({ length: rows }).map((_, r) => (
        <tr key={r} className="border-b border-gray-800/50">
          {colWidths.map((w, i) => (
            <DarkShimmerCell key={i} widthClass={w} />
          ))}
        </tr>
      ))}
    </>
  );
}
