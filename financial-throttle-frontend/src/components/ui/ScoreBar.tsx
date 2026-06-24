interface ScoreBarProps {
  score: number; // 0-1
}

export default function ScoreBar({ score }: ScoreBarProps) {
  const pct = Math.round(score * 100);
  const color =
    pct >= 70 ? 'bg-green-500' : pct >= 40 ? 'bg-orange-400' : 'bg-red-400';

  return (
    <div className="flex items-center gap-2">
      <div className="flex-1 bg-gray-100 rounded-full h-1.5 overflow-hidden">
        <div className={`h-full rounded-full ${color}`} style={{ width: `${pct}%` }} />
      </div>
      <span className="text-xs text-gray-500 w-8 text-right">{pct}%</span>
    </div>
  );
}
