import { useState } from 'react';
import { Database, Wifi, Settings } from 'lucide-react';
import Header from '../components/layout/Header';
import toast from 'react-hot-toast';

const STORAGE_KEY = 'fts_settings';

interface AppSettings {
  maxParallelGroups: number;
  workerIntervalSeconds: number;
  useDummyData: boolean;
  financialApiUrl: string;
  priorityApiUrl: string;
  timeoutSeconds: number;
  maxRetries: number;
}

const defaults: AppSettings = {
  maxParallelGroups: 5,
  workerIntervalSeconds: 30,
  useDummyData: true,
  financialApiUrl: 'https://api.example.com/service-financialtransactionhub-test/',
  priorityApiUrl: 'http://localhost:11080/',
  timeoutSeconds: 120,
  maxRetries: 3,
};

function loadSettings(): AppSettings {
  try {
    const saved = localStorage.getItem(STORAGE_KEY);
    if (saved) return { ...defaults, ...JSON.parse(saved) };
  } catch { /* ignore */ }
  return defaults;
}

function SliderField({ label, value, min, max, onChange }: {
  label: string; value: number; min: number; max: number;
  onChange: (v: number) => void;
}) {
  return (
    <div className="space-y-2">
      <div className="flex justify-between text-xs">
        <span className="text-gray-600 font-medium">{label}</span>
        <span className="text-indigo-600 font-bold">{value}</span>
      </div>
      <input
        type="range" min={min} max={max} value={value}
        onChange={e => onChange(Number(e.target.value))}
        className="w-full accent-indigo-500"
      />
      <div className="flex justify-between text-xs text-gray-400">
        <span>{min}</span><span>{max}</span>
      </div>
    </div>
  );
}

function Toggle({ checked, onChange }: { checked: boolean; onChange: (v: boolean) => void }) {
  return (
    <button
      onClick={() => onChange(!checked)}
      className={`relative inline-flex w-10 h-6 rounded-full transition-colors ${checked ? 'bg-indigo-500' : 'bg-gray-200'}`}
    >
      <span
        className={`absolute top-1 left-1 w-4 h-4 bg-white rounded-full shadow transition-transform ${checked ? 'translate-x-4' : ''}`}
      />
    </button>
  );
}

const dbConnections = [
  { label: 'RAS_STAJ', description: 'Management DB' },
  { label: 'RAS_STAJ107', description: 'Turkey Finance DB' },
  { label: 'RAS_STAJ32501', description: 'MS-Source Finance DB' },
  { label: 'MongoDB', description: 'Logs database' },
];

export default function SettingsPage() {
  const [settings, setSettings] = useState<AppSettings>(loadSettings);
  const [dbStatus, setDbStatus] = useState<Record<string, 'idle' | 'testing' | 'ok' | 'fail'>>({});

  const set = <K extends keyof AppSettings>(key: K, value: AppSettings[K]) =>
    setSettings(prev => ({ ...prev, [key]: value }));

  const save = () => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(settings));
    toast.success('Settings saved');
  };

  const reset = () => {
    setSettings(defaults);
    localStorage.removeItem(STORAGE_KEY);
    toast.success('Settings reset to defaults');
  };

  const testConnection = async (db: string) => {
    setDbStatus(prev => ({ ...prev, [db]: 'testing' }));
    await new Promise(r => setTimeout(r, 1200));
    // Simulate — in production call a real health endpoint
    setDbStatus(prev => ({ ...prev, [db]: Math.random() > 0.3 ? 'ok' : 'fail' }));
  };

  return (
    <div className="flex-1 flex flex-col overflow-hidden">
      <Header title="Settings" subtitle="Configure worker and API settings" />

      <div className="p-6 space-y-5 overflow-y-auto flex-1">
        {/* Worker Config */}
        <div className="card space-y-5">
          <div className="flex items-center gap-2 mb-1">
            <Settings size={16} className="text-indigo-500" />
            <h2 className="text-sm font-bold text-gray-900">Worker Configuration</h2>
          </div>
          <SliderField
            label="Max Parallel Groups"
            value={settings.maxParallelGroups}
            min={1} max={10}
            onChange={v => set('maxParallelGroups', v)}
          />
          <SliderField
            label="Worker Interval (seconds)"
            value={settings.workerIntervalSeconds}
            min={10} max={120}
            onChange={v => set('workerIntervalSeconds', v)}
          />
          <div className="flex items-center justify-between">
            <div>
              <div className="text-xs font-medium text-gray-700">Use Dummy Data</div>
              <div className="text-xs text-gray-400">Use mock data instead of real DB</div>
            </div>
            <Toggle checked={settings.useDummyData} onChange={v => set('useDummyData', v)} />
          </div>
        </div>

        {/* API Config */}
        <div className="card space-y-4">
          <div className="flex items-center gap-2 mb-1">
            <Wifi size={16} className="text-indigo-500" />
            <h2 className="text-sm font-bold text-gray-900">API Configuration</h2>
          </div>
          <div className="space-y-1">
            <label className="text-xs font-medium text-gray-600">FinancialTransaction API URL</label>
            <input
              value={settings.financialApiUrl}
              onChange={e => set('financialApiUrl', e.target.value)}
              className="input w-full"
            />
          </div>
          <div className="space-y-1">
            <label className="text-xs font-medium text-gray-600">Priority Client API URL</label>
            <input
              value={settings.priorityApiUrl}
              onChange={e => set('priorityApiUrl', e.target.value)}
              className="input w-full"
            />
          </div>
          <SliderField
            label="Timeout (seconds)"
            value={settings.timeoutSeconds}
            min={30} max={300}
            onChange={v => set('timeoutSeconds', v)}
          />
          <SliderField
            label="Max Retries"
            value={settings.maxRetries}
            min={1} max={10}
            onChange={v => set('maxRetries', v)}
          />
        </div>

        {/* DB Connections */}
        <div className="card space-y-3">
          <div className="flex items-center gap-2 mb-1">
            <Database size={16} className="text-indigo-500" />
            <h2 className="text-sm font-bold text-gray-900">Database Connections</h2>
          </div>
          {dbConnections.map(db => {
            const st = dbStatus[db.label] ?? 'idle';
            return (
              <div key={db.label} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                <div>
                  <div className="text-xs font-medium text-gray-800">{db.label}</div>
                  <div className="text-xs text-gray-400">{db.description}</div>
                </div>
                <div className="flex items-center gap-3">
                  {st === 'ok' && <span className="text-xs text-green-600 font-medium">✅ Connected</span>}
                  {st === 'fail' && <span className="text-xs text-red-600 font-medium">❌ Failed</span>}
                  {st === 'testing' && <span className="text-xs text-gray-400">Testing...</span>}
                  <button
                    onClick={() => testConnection(db.label)}
                    disabled={st === 'testing'}
                    className="btn-secondary text-xs px-3 py-1.5 disabled:opacity-50"
                  >
                    Test
                  </button>
                </div>
              </div>
            );
          })}
        </div>

        {/* Actions */}
        <div className="flex gap-3">
          <button onClick={save} className="btn-primary px-6">Save Settings</button>
          <button onClick={reset} className="btn-secondary px-6">Reset Defaults</button>
        </div>
      </div>
    </div>
  );
}
