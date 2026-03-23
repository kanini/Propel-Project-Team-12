import { Link, useLocation } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';

interface NavigationItem {
  name: string;
  path: string;
  icon: string;
  roles: string[];
}

/**
 * Mobile bottom navigation configuration (US_020, AC1, AC4).
 * Displays at <768px breakpoint with role-based filtering.
 */
const mobileNavigationConfig: NavigationItem[] = [
  // Patient mobile navigation (4 items max for mobile)
  { name: 'Home', path: '/dashboard', icon: '🏠', roles: ['Patient'] },
  { name: 'Providers', path: '/providers', icon: '🔍', roles: ['Patient'] },
  { name: 'Appointments', path: '/appointments', icon: '📅', roles: ['Patient'] },
  { name: 'Profile', path: '/profile', icon: '👤', roles: ['Patient'] },

  // Staff mobile navigation
  { name: 'Dashboard', path: '/staff/dashboard', icon: '🏥', roles: ['Staff', 'Admin'] },
  { name: 'Queue', path: '/staff/queue', icon: '👥', roles: ['Staff', 'Admin'] },
  { name: 'Walk-in', path: '/staff/walk-in', icon: '🚶', roles: ['Staff', 'Admin'] },
  { name: 'More', path: '/staff/menu', icon: '☰', roles: ['Staff', 'Admin'] },

  // Admin mobile navigation
  { name: 'Dashboard', path: '/admin/dashboard', icon: '⚙️', roles: ['Admin'] },
  { name: 'Users', path: '/admin/users', icon: '👤', roles: ['Admin'] },
  { name: 'Audit', path: '/admin/audit', icon: '📊', roles: ['Admin'] },
  { name: 'Settings', path: '/admin/settings', icon: '🔧', roles: ['Admin'] },
];

/**
 * Mobile bottom navigation with role-based filtering (US_020, AC1, AC4).
 * Displays at <768px breakpoint (mobile devices).
 * Conditionally shows navigation items based on user role.
 */
export const BottomNav = () => {
  const { role } = useAuth();
  const location = useLocation();

  // Filter navigation items based on user role (US_020, AC1)
  const visibleItems = mobileNavigationConfig.filter((item) =>
    role ? item.roles.includes(role) : false
  );

  const isActive = (path: string) => location.pathname === path;

  // Don't render on auth pages
  if (location.pathname === '/login' || location.pathname === '/register') {
    return null;
  }

  return (
    <nav className="md:hidden fixed bottom-0 left-0 right-0 bg-white border-t border-neutral-200 z-50">
      <div className="flex justify-around items-center h-16">
        {visibleItems.map((item) => (
          <Link
            key={item.path}
            to={item.path}
            className={`
              flex flex-col items-center justify-center flex-1 h-full transition-colors
              ${isActive(item.path)
                ? 'text-primary-600'
                : 'text-neutral-500 hover:text-neutral-900'
              }
            `}
          >
            <span className="text-2xl">{item.icon}</span>
            <span className="text-xs font-medium mt-1">{item.name}</span>
          </Link>
        ))}
      </div>
    </nav>
  );
};
