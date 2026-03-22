import { Link, useLocation } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';

interface NavigationItem {
  name: string;
  path: string;
  icon: string;
  roles: string[];
}

/**
 * Navigation configuration mapping routes to allowed roles (US_020, AC1, AC4).
 * Patient → Dashboard, Providers, Appointments, Intake, Documents
 * Staff → Dashboard, Queue, Walk-in, Verification
 * Admin → Dashboard, Users, Audit, Settings
 */
const navigationConfig: NavigationItem[] = [
  // Patient navigation
  { name: 'Dashboard', path: '/dashboard', icon: '🏠', roles: ['Patient'] },
  { name: 'Find Provider', path: '/providers', icon: '🔍', roles: ['Patient'] },
  { name: 'My Appointments', path: '/appointments', icon: '📅', roles: ['Patient'] },
  { name: 'Intake Forms', path: '/intake', icon: '📋', roles: ['Patient'] },
  { name: 'Documents', path: '/documents', icon: '📄', roles: ['Patient'] },

  // Staff navigation
  { name: 'Staff Dashboard', path: '/staff/dashboard', icon: '🏥', roles: ['Staff', 'Admin'] },
  { name: 'Patient Queue', path: '/staff/queue', icon: '👥', roles: ['Staff', 'Admin'] },
  { name: 'Walk-in Registration', path: '/staff/walk-in', icon: '🚶', roles: ['Staff', 'Admin'] },
  { name: 'Verification', path: '/staff/verification', icon: '✅', roles: ['Staff', 'Admin'] },

  // Admin navigation
  { name: 'Admin Dashboard', path: '/admin/dashboard', icon: '⚙️', roles: ['Admin'] },
  { name: 'User Management', path: '/admin/users', icon: '👤', roles: ['Admin'] },
  { name: 'Audit Logs', path: '/admin/audit', icon: '📊', roles: ['Admin'] },
  { name: 'Settings', path: '/admin/settings', icon: '🔧', roles: ['Admin'] },
];

/**
 * Desktop sidebar navigation with role-based menu filtering (US_020, AC1, AC4).
 * Conditionally renders navigation items based on user role.
 * Implements UXR-003: Persistent role-based navigation.
 */
export const Sidebar = () => {
  const { role, user } = useAuth();
  const location = useLocation();

  // Filter navigation items based on user role (US_020, AC1)
  const visibleItems = navigationConfig.filter((item) =>
    role ? item.roles.includes(role) : false
  );

  const isActive = (path: string) => location.pathname === path;

  return (
    <aside className="hidden md:flex md:flex-col md:w-64 bg-white border-r border-gray-200 min-h-screen">
      {/* Logo and User Info */}
      <div className="p-6 border-b border-gray-200">
        <h1 className="text-xl font-bold text-blue-600">Patient Access</h1>
        {user && (
          <div className="mt-4">
            <p className="text-sm font-medium text-gray-900">{user.name}</p>
            <p className="text-xs text-gray-500">{user.email}</p>
            <span className="inline-block mt-2 px-2 py-1 text-xs font-medium rounded-full bg-blue-100 text-blue-800">
              {role}
            </span>
          </div>
        )}
      </div>

      {/* Navigation Menu */}
      <nav className="flex-1 p-4 space-y-1 overflow-y-auto">
        {visibleItems.map((item) => (
          <Link
            key={item.path}
            to={item.path}
            className={`
              flex items-center gap-3 px-4 py-3 rounded-lg text-sm font-medium transition-colors
              ${isActive(item.path)
                ? 'bg-blue-50 text-blue-600'
                : 'text-gray-700 hover:bg-gray-50 hover:text-gray-900'
              }
            `}
          >
            <span className="text-xl">{item.icon}</span>
            <span>{item.name}</span>
          </Link>
        ))}
      </nav>

      {/* Footer */}
      <div className="p-4 border-t border-gray-200">
        <Link
          to="/logout"
          className="flex items-center gap-3 px-4 py-3 rounded-lg text-sm font-medium text-gray-700 hover:bg-gray-50 hover:text-gray-900 transition-colors"
        >
          <span className="text-xl">🚪</span>
          <span>Logout</span>
        </Link>
      </div>
    </aside>
  );
};
