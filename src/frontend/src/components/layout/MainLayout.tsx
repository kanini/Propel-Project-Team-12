import { Sidebar } from './Sidebar';
import { BottomNav } from './BottomNav';
import { Header } from './Header';

interface MainLayoutProps {
  children: React.ReactNode;
}

/**
 * Main layout wrapper with header, sidebar and bottom navigation (US_020).
 * Displays Header on all screen sizes, Sidebar on desktop (md+), and BottomNav on mobile (<md).
 */
export const MainLayout = ({ children }: MainLayoutProps) => {
  return (
    <div className="flex min-h-screen bg-neutral-50">
      {/* Desktop Sidebar */}
      <Sidebar />

      {/* Main Content Area with Header */}
      <div className="flex-1 flex flex-col overflow-x-hidden">
        {/* Header - Fixed at top */}
        <Header />

        {/* Main Content */}
        <main className="flex-1 overflow-x-hidden">
          <div className="container mx-auto px-4 py-6 md:py-8 pb-20 md:pb-8">
            {children}
          </div>
        </main>
      </div>

      {/* Mobile Bottom Navigation */}
      <BottomNav />
    </div>
  );
};
