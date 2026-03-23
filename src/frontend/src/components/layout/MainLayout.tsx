import { Sidebar } from './Sidebar';
import { BottomNav } from './BottomNav';

interface MainLayoutProps {
  children: React.ReactNode;
}

/**
 * Main layout wrapper with sidebar and bottom navigation (US_020).
 * Displays Sidebar on desktop (md+) and BottomNav on mobile (<md).
 */
export const MainLayout = ({ children }: MainLayoutProps) => {
  return (
    <div className="flex min-h-screen bg-neutral-50">
      {/* Desktop Sidebar */}
      <Sidebar />

      {/* Main Content Area */}
      <main className="flex-1 overflow-x-hidden">
        <div className="container mx-auto px-4 py-6 md:py-8 pb-20 md:pb-8">
          {children}
        </div>
      </main>

      {/* Mobile Bottom Navigation */}
      <BottomNav />
    </div>
  );
};
