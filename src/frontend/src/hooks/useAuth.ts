import { useSelector } from 'react-redux';
import type { RootState } from '../store';

/**
 * Custom hook for accessing authentication state and role information (US_020).
 * Provides convenient access to user, role, and authentication status.
 */
export const useAuth = () => {
  const { user, token, isAuthenticated, isLoading, isInitializing } = useSelector(
    (state: RootState) => state.auth
  );

  return {
    user,
    token,
    isAuthenticated,
    isLoading,
    isInitializing,
    role: user?.role || null,
    userId: user?.userId || null,
    /**
     * Check if user has one of the specified roles (US_020, AC1, AC2).
     * @param allowedRoles - Array of allowed role names
     * @returns True if user has any of the allowed roles
     */
    hasRole: (allowedRoles: string[]): boolean => {
      if (!user?.role) return false;
      return allowedRoles.includes(user.role);
    },
    /**
     * Check if user is an admin.
     */
    isAdmin: (): boolean => user?.role === 'Admin',
    /**
     * Check if user is staff.
     */
    isStaff: (): boolean => user?.role === 'Staff',
    /**
     * Check if user is a patient.
     */
    isPatient: (): boolean => user?.role === 'Patient',
  };
};
