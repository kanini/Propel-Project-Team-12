import { useAuth } from '../../hooks/useAuth';
import { useLocation } from 'react-router-dom';

/**
 * Header component matching wireframe SCR-003, SCR-004, SCR-005
 * Displays logo, breadcrumb, notifications, and user avatar
 */
export const Header = () => {
    const { user } = useAuth();
    const location = useLocation();

    // Generate breadcrumb from current path
    const getBreadcrumb = (): { label: string; path: string }[] => {
        const paths = location.pathname.split('/').filter(Boolean);
        const breadcrumbs = [{ label: 'Home', path: '/' }];

        if (paths.length > 0) {
            // Map paths to readable labels
            const pathLabels: Record<string, string> = {
                dashboard: 'Dashboard',
                appointments: 'My Appointments',
                providers: 'Find Provider',
                staff: 'Staff',
                admin: 'Admin',
                users: 'Users',
                audit: 'Audit Log',
                settings: 'Settings',
                queue: 'Queue',
                arrivals: 'Arrivals',
                'walk-in': 'Walk-in',
                verification: 'Verification',
            };

            paths.forEach((path, index) => {
                const label = pathLabels[path] || path.charAt(0).toUpperCase() + path.slice(1);
                breadcrumbs.push({
                    label,
                    path: '/' + paths.slice(0, index + 1).join('/'),
                });
            });
        }

        return breadcrumbs;
    };

    const breadcrumbs = getBreadcrumb();
    const currentPage = breadcrumbs[breadcrumbs.length - 1]?.label || 'Dashboard';

    // Get user initials for avatar
    const getUserInitials = () => {
        if (!user?.name) return 'U';
        const names = user.name.split(' ');
        if (names.length >= 2) {
            return (names[0]?.[0] || '') + (names[names.length - 1]?.[0] || '');
        }
        return names[0]?.[0] || 'U';
    };

    return (
        <header className="bg-white border-b border-gray-200 sticky top-0 z-40">
            <div className="flex items-center justify-between h-16 px-6">
                {/* Left Section - Logo and Breadcrumb */}
                <div className="flex items-center gap-3">
                    {/* Logo */}
                    <div className="flex items-center gap-2">
                        <div
                            className="w-7 h-7 bg-blue-600 rounded flex items-center justify-center text-white font-bold text-sm"
                            aria-hidden="true"
                        >
                            +
                        </div>
                        <span className="text-base font-semibold text-gray-900 hidden sm:inline">
                            PatientAccess
                        </span>
                    </div>

                    {/* Breadcrumb */}
                    <nav className="hidden md:flex items-center" aria-label="Breadcrumb">
                        <span className="text-gray-400 mx-2">/</span>
                        {breadcrumbs.length > 1 && breadcrumbs[0] && (
                            <>
                                <a
                                    href={breadcrumbs[0].path}
                                    className="text-sm text-gray-500 hover:text-gray-700"
                                >
                                    {breadcrumbs[0].label}
                                </a>
                                <span className="text-gray-400 mx-2">/</span>
                            </>
                        )}
                        <span className="text-sm text-gray-800 font-medium">{currentPage}</span>
                    </nav>
                </div>

                {/* Right Section - Notifications and User Avatar */}
                <div className="flex items-center gap-4">
                    {/* Notifications Button */}
                    <button
                        className="relative p-2 text-gray-600 hover:bg-gray-100 rounded-lg transition-colors"
                        aria-label="Notifications"
                    >
                        <svg
                            className="w-5 h-5"
                            fill="none"
                            stroke="currentColor"
                            viewBox="0 0 24 24"
                            aria-hidden="true"
                        >
                            <path
                                strokeLinecap="round"
                                strokeLinejoin="round"
                                strokeWidth={2}
                                d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9"
                            />
                        </svg>
                        {/* Notification Badge (optional) */}
                        <span
                            className="absolute top-1.5 right-1.5 w-2 h-2 bg-red-600 rounded-full"
                            aria-hidden="true"
                        />
                    </button>

                    {/* User Avatar */}
                    <button
                        className="flex items-center justify-center w-9 h-9 rounded-full bg-blue-100 text-blue-700 text-sm font-semibold hover:bg-blue-200 transition-colors"
                        aria-label={`User menu — ${user?.name || 'User'}`}
                    >
                        {getUserInitials()}
                    </button>
                </div>
            </div>
        </header>
    );
};
