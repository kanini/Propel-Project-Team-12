import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';

interface ProtectedRouteProps {
  children: React.ReactNode;
  allowedRoles?: string[];
  requireAuth?: boolean;
}

/**
 * Protected route component that enforces role-based access control (US_020).
 * Redirects unauthorized users to appropriate dashboards based on their role.
 * 
 * @param children - Child components to render if authorized
 * @param allowedRoles - Array of roles allowed to access this route
 * @param requireAuth - Whether route requires authentication (default: true)
 */
export const ProtectedRoute = ({ 
  children, 
  allowedRoles, 
  requireAuth = true 
}: ProtectedRouteProps) => {
  const { isAuthenticated, role, isLoading, isInitializing } = useAuth();
  const location = useLocation();

  // Show loading state while initializing or checking authentication
  if (isInitializing || isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
          <p className="mt-4 text-gray-600">Loading...</p>
        </div>
      </div>
    );
  }

  // Redirect to login if authentication required but user not authenticated
  if (requireAuth && !isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  // If no specific roles required, allow access to authenticated users
  if (!allowedRoles || allowedRoles.length === 0) {
    return <>{children}</>;
  }

  // Check if user has required role (US_020, AC2)
  if (role && allowedRoles.includes(role)) {
    return <>{children}</>;
  }

  // Redirect unauthorized users to role-appropriate dashboard (US_020, AC2)
  const redirectPath = getRoleBasedRedirect(role);
  return <Navigate to={redirectPath} replace />;
};

/**
 * Gets the appropriate dashboard redirect path based on user role.
 */
const getRoleBasedRedirect = (role: string | null): string => {
  switch (role) {
    case 'Admin':
      return '/admin/dashboard';
    case 'Staff':
      return '/staff/dashboard';
    case 'Patient':
      return '/dashboard';
    default:
      return '/login';
  }
};
