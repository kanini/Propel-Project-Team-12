/**
 * Role-based redirect utility (FR-002 - User Authentication)
 * Determines appropriate dashboard based on user role
 */

/**
 * User roles supported by the system
 */
export type UserRole = 'Patient' | 'Staff' | 'Admin';

/**
 * Gets redirect path based on user role (FR-002 AC1, AC4)
 * @param role - User role from JWT token
 * @returns Dashboard path for the role
 */
export const getRedirectPath = (role: string): string => {
  switch (role) {
    case 'Patient':
      return '/patient/dashboard';
    case 'Staff':
      return '/staff/dashboard';
    case 'Admin':
      return '/admin/dashboard';
    default:
      // Unknown role - redirect to home or error page
      return '/';
  }
};

/**
 * Validates if role is valid
 * @param role - Role to validate
 * @returns True if role is valid
 */
export const isValidRole = (role: string): role is UserRole => {
  return ['Patient', 'Staff', 'Admin'].includes(role);
};
