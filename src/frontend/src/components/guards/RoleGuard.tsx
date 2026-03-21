/**
 * Role Guard Component (NFR-006 RBAC)
 * 
 * Protects routes based on user role, redirects unauthorized users.
 * Used to wrap protected routes and ensure role-based access control.
 */

import React from 'react';
import { Navigate } from 'react-router-dom';
import { getUserRole, isTokenExpired } from '../../utils/tokenStorage';
import { getRedirectPath } from '../../utils/navigationConfig';

interface RoleGuardProps {
  children: React.ReactNode;
  allowedRoles: string[];
}

/**
 * RoleGuard wraps protected routes and enforces role-based access control.
 * 
 * - If not authenticated or token expired: redirects to /login
 * - If role not in allowedRoles: redirects to user's role-appropriate dashboard
 * - If authorized: renders children
 */
export const RoleGuard: React.FC<RoleGuardProps> = ({ children, allowedRoles }) => {
  const role = getUserRole();

  // Check if user is authenticated and token is not expired
  if (!role || isTokenExpired()) {
    return <Navigate to="/login" replace />;
  }

  // Check if user's role is in the allowed roles list
  if (!allowedRoles.includes(role)) {
    // User does not have permission - redirect to their role-specific dashboard
    const redirectPath = getRedirectPath(role);
    return <Navigate to={redirectPath} replace />;
  }

  // User is authorized - render children
  return <>{children}</>;
};
