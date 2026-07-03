import { useState } from "react";
import {
  LayoutDashboard,
  List,
  AlertTriangle,
  FileText,
  Settings,
  Search,
  ChevronLeft,
  ChevronRight,
} from "lucide-react";
import type { Page } from "../../types";

interface SidebarProps {
  activePage: Page;
  onNavigate: (page: Page) => void;
}

const menuItems: { id: Page; label: string; icon: React.ReactNode }[] = [
  { id: "dashboard", label: "Dashboard", icon: <LayoutDashboard size={18} /> },
  { id: "queue", label: "Queue", icon: <List size={18} /> },
  { id: "suspended", label: "Suspended", icon: <AlertTriangle size={18} /> },
  { id: "logs", label: "Logs", icon: <FileText size={18} /> },
  { id: "checker", label: "Checker", icon: <Search size={18} /> },
  // { id: "settings", label: "Settings", icon: <Settings size={18} /> },
];

export default function Sidebar({ activePage, onNavigate }: SidebarProps) {
  const [isCollapsed, setIsCollapsed] = useState(false);
  const activeIndex = menuItems.findIndex((item) => item.id === activePage);

  return (
    <aside
      className={`${
        isCollapsed ? "w-16" : "w-60"
      } min-h-screen flex flex-col flex-shrink-0 relative select-none z-20 transition-all duration-300 overflow-hidden`}
      style={{ background: "var(--sidebar-bg, #5f7bf4)" }}
    >
      {/* Logo */}
      <div className="py-5 z-10 flex items-center justify-center px-3">
        <div
          className={`flex items-center gap-2.5 ${isCollapsed ? "justify-center" : ""}`}
        >
          <div className="w-8 h-8 rounded-lg flex items-center justify-center text-sm font-bold bg-white/20 text-white flex-shrink-0">
            FT
          </div>
          <div
            className={`overflow-hidden transition-all duration-300 ${
              isCollapsed ? "w-0 opacity-0" : "w-auto opacity-100"
            }`}
          >
            <div className="text-sm font-bold text-white whitespace-nowrap">
              Financial
            </div>
            <div className="text-xs text-white/50 whitespace-nowrap">
              Throttle Service
            </div>
          </div>
        </div>
      </div>

      {/* Nav Container */}
      <nav
        className={`flex-1 py-2 relative ${
          isCollapsed ? "px-0" : "pl-3"
        } pr-0 overflow-visible`}
      >
        {/* Active Indicator */}
        <div
          className="nav-active-indicator absolute left-0 right-0 bg-white rounded-l-[24px]"
          style={{
            height: "44px",
            top: "8px",
            transform: `translateY(${activeIndex * 44}px)`,
          }}
        />

        {/* Menu Items */}
        <div className="relative z-10 flex flex-col">
          {menuItems.map((item) => {
            const isActive = activePage === item.id;
            return (
              <button
                key={item.id}
                onClick={() => onNavigate(item.id)}
                title={isCollapsed ? item.label : undefined}
                className={`w-full flex items-center h-[44px] transition-colors duration-300 outline-none text-sm font-semibold ${
                  isCollapsed ? "justify-center px-0" : "gap-3 pl-4 pr-2"
                } ${isActive ? "text-indigo-600" : "text-white/70 hover:text-white"}`}
              >
                <span
                  className={`transition-colors duration-300 flex-shrink-0 ${
                    isActive ? "text-indigo-500" : "text-white/50"
                  }`}
                >
                  {item.icon}
                </span>
                <span
                  className={`overflow-hidden whitespace-nowrap transition-all duration-300 ${
                    isCollapsed ? "w-0 opacity-0" : "w-auto opacity-100"
                  }`}
                >
                  {item.label}
                </span>
              </button>
            );
          })}
        </div>
      </nav>

      {/* Toggle Button */}
      <div className="p-3 z-10 flex justify-center">
        <button
          onClick={() => setIsCollapsed((prev) => !prev)}
          className="w-8 h-8 rounded-xl bg-white hover:bg-white/75 flex items-center justify-center text-white/60 hover:text-white transition-colors duration-200"
          title={isCollapsed ? "Expand sidebar" : "Collapse sidebar"}
        >
          {isCollapsed ? (
            <ChevronRight size={16} color="#1a1535" />
          ) : (
            <ChevronLeft size={16} color="#1a1535" />
          )}
        </button>
      </div>
    </aside>
  );
}
