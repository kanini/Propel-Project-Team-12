/**
 * Navigation Configuration for Role-Based Access Control (NFR-006)
 * 
 * Defines role-specific menu items for Patient, Staff, and Admin users.
 * Used by NavigationMenu component to filter and display appropriate menu items.
 */

export interface MenuItem {
  label: string;
  path: string;
  icon: string;
  section?: 'main' | 'health' | 'clinical' | 'management';
}

/**
 * Patient role navigation items
 * Visible only to users with Patient role
 */
export const patientMenuItems: MenuItem[] = [
  { label: 'Dashboard', path: '/patient/dashboard', icon: 'home', section: 'main' },
  { label: 'My Appointments', path: '/patient/appointments', icon: 'calendar', section: 'main' },
  { label: 'Find Providers', path: '/patient/providers', icon: 'search', section: 'main' },
  { label: 'Health Dashboard', path: '/patient/health', icon: 'heart', section: 'health' },
  { label: 'Documents', path: '/patient/documents', icon: 'file', section: 'health' },
  { label: 'Intake', path: '/patient/intake', icon: 'clipboard', section: 'health' },
];

/**
 * Staff role navigation items
 * Visible only to users with Staff role
 */
export const staffMenuItems: MenuItem[] = [
  { label: 'Dashboard', path: '/staff/dashboard', icon: 'home', section: 'main' },
  { label: 'Patient Queue', path: '/staff/queue', icon: 'users', section: 'clinical' },
  { label: 'Walk-in Booking', path: '/staff/walk-in', icon: 'user-plus', section: 'clinical' },
  { label: 'Patients', path: '/staff/patients', icon: 'user', section: 'clinical' },
  { label: 'Verification', path: '/staff/verification', icon: 'check-circle', section: 'clinical' },
  { label: 'Appointments', path: '/staff/appointments', icon: 'calendar', section: 'main' },
];

/**
 * Admin role navigation items
 * Visible only to users with Admin role
 */
export const adminMenuItems: MenuItem[] = [
  { label: 'Dashboard', path: '/admin/dashboard', icon: 'home', section: 'main' },
  { label: 'Users', path: '/admin/users', icon: 'users', section: 'management' },
  { label: 'Audit Logs', path: '/admin/audit', icon: 'shield', section: 'management' },
  { label: 'Settings', path: '/admin/settings', icon: 'settings', section: 'management' },
];

/**
 * Get menu items for a specific role
 * @param role - User role (Patient, Staff, or Admin)
 * @returns Array of menu items for the role
 */
export function getMenuItemsForRole(role: string): MenuItem[] {
  switch (role) {
    case 'Patient':
      return patientMenuItems;
    case 'Staff':
      return staffMenuItems;
    case 'Admin':
      return adminMenuItems;
    default:
      return [];
  }
}

/**
 * Group menu items by section
 * @param items - Array of menu items
 * @returns Object with section names as keys and menu items as values
 */
export function groupItemsBySection(items: MenuItem[]): Record<string, MenuItem[]> {
  const grouped: Record<string, MenuItem[]> = {};

  items.forEach(item => {
    const section = item.section || 'main';
    if (!grouped[section]) {
      grouped[section] = [];
    }
    grouped[section].push(item);
  });

  return grouped;
}

/**
 * Get redirect path based on user role (for 403 errors and unauthorized access)
 * @param role - User role
 * @returns Dashboard path for the role
 */
export function getRedirectPath(role: string): string {
  switch (role) {
    case 'Patient':
      return '/patient/dashboard';
    case 'Staff':
      return '/staff/dashboard';
    case 'Admin':
      return '/admin/dashboard';
    default:
      return '/login';
  }
}
