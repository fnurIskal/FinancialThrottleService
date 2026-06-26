import { useState, lazy, Suspense } from 'react';
import { Toaster } from 'react-hot-toast';
import Sidebar from './components/layout/Sidebar';
import Spinner from './components/ui/Spinner';
import type { Page } from './types';

const DashboardPage = lazy(() => import('./pages/DashboardPage'));
const QueuePage = lazy(() => import('./pages/QueuePage'));
const SuspendedPage = lazy(() => import('./pages/SuspendedPage'));
const LogsPage = lazy(() => import('./pages/LogsPage'));
const SettingsPage = lazy(() => import('./pages/SettingsPage'));

function PageLoader() {
  return (
    <div className="flex-1 flex items-center justify-center">
      <Spinner size="lg" />
    </div>
  );
}

export default function App() {
  const [activePage, setActivePage] = useState<Page>('dashboard');

  return (
    <div className="flex h-screen w-full overflow-hidden bg-white">
      <Sidebar activePage={activePage} onNavigate={setActivePage} />

      <main className="flex-1 flex flex-col min-w-0 overflow-hidden">
        <Suspense fallback={<PageLoader />}>
          {activePage === 'dashboard' && <DashboardPage onNavigate={setActivePage} />}
          {activePage === 'queue' && <QueuePage />}
          {activePage === 'suspended' && <SuspendedPage />}
          {activePage === 'logs' && <LogsPage />}
          {activePage === 'settings' && <SettingsPage />}
        </Suspense>
      </main>

      <Toaster
        position="top-right"
        toastOptions={{
          style: { fontSize: '13px', borderRadius: '10px' },
        }}
      />
    </div>
  );
}
