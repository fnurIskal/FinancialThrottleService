import { LayoutDashboard, List, AlertTriangle, FileText, Settings } from 'lucide-react';
import type { Page } from '../../types';

interface SidebarProps {
  activePage: Page;
  onNavigate: (page: Page) => void;
}

const menuItems: { id: Page; label: string; icon: React.ReactNode }[] = [
  { id: 'dashboard', label: 'Dashboard', icon: <LayoutDashboard size={18} /> },
  { id: 'queue', label: 'Queue', icon: <List size={18} /> },
  { id: 'suspended', label: 'Suspended', icon: <AlertTriangle size={18} /> },
  { id: 'logs', label: 'Logs', icon: <FileText size={18} /> },
  { id: 'settings', label: 'Settings', icon: <Settings size={18} /> },
];

export default function Sidebar({ activePage, onNavigate }: SidebarProps) {
  return (
    <aside className="w-60 min-h-screen bg-white border-r border-gray-100 flex flex-col shadow-sm flex-shrink-0">
      <div className="px-6 py-5 border-b border-gray-100">
        <div className="flex items-center gap-2">
          <div
            className="w-8 h-8 rounded-lg flex items-center justify-center text-white text-sm font-bold"
            style={{ background: 'linear-gradient(135deg, #667eea, #764ba2)' }}
          >
            FT
          </div>
          <div>
            <div className="text-sm font-bold text-gray-900">Financial</div>
            <div className="text-xs text-gray-400">Throttle Service</div>
          </div>
        </div>
      </div>

      <nav className="flex-1 px-3 py-4 space-y-1">
        {menuItems.map(item => {
          const isActive = activePage === item.id;
          return (
            <button
              key={item.id}
              onClick={() => onNavigate(item.id)}
              className={`w-full flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-colors ${
                isActive
                  ? 'bg-indigo-50 text-indigo-600'
                  : 'text-gray-500 hover:bg-gray-50 hover:text-gray-700'
              }`}
            >
              <span className={isActive ? 'text-indigo-500' : 'text-gray-400'}>
                {item.icon}
              </span>
              {item.label}
            </button>
          );
        })}
      </nav>

      <div className="m-3 p-4 rounded-xl text-white" style={{ background: 'linear-gradient(135deg, #667eea, #764ba2)' }}>
        <div className="text-xs font-semibold mb-1">System Status</div>
        <div className="text-xs opacity-80">All services operational</div>
        <div className="mt-3 flex items-center gap-1.5">
          <span className="w-2 h-2 rounded-full bg-green-300 animate-pulse" />
          <span className="text-xs opacity-90">Live monitoring</span>
        </div>
      </div>
    </aside>
  );
}
