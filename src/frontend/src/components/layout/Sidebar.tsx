/**
 * Sidebar Layout Component (NFR-006 RBAC)
 * 
 * Persistent sidebar with role-based navigation, logo, and user profile.
 * Provides logout functionality and displays current user role.
 */

import React from 'react';
import { useNavigate } from 'react-router-dom';
import { NavigationMenu } from './NavigationMenu';
import { getUserRole, clearAuth } from '../../utils/tokenStorage';
import { useAppDispatch } from '../../store/hooks';
import { logout } from '../../store/slices/authSlice';

export const Sidebar: React.FC = () => {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const role = getUserRole();

  const handleLogout = () => {
    clearAuth();
    dispatch(logout());
    navigate('/login');
  };

  return (
    <div className="w-64 bg-white border-r border-gray-200 flex flex-col min-h-screen">
      {/* Logo */}
      <div className="p-6 border-b border-gray-200">
        <div className="flex items-center gap-2">
          <div className="w-8 h-8 bg-blue-500 rounded-lg flex items-center justify-center text-white text-xl font-bold">
            +
          </div>
          <span className="text-xl font-semibold text-gray-900">PatientAccess</span>
        </div>
      </div>

      {/* Navigation Menu */}
      <div className="flex-1 py-6 px-2 overflow-y-auto">
        <NavigationMenu />
      </div>

      {/* User Profile */}
      <div className="p-4 border-t border-gray-200">
        <div className="flex items-center gap-3 mb-3">
          <div className="w-10 h-10 bg-gray-200 rounded-full flex items-center justify-center text-gray-600 font-semibold">
            {role?.[0] || 'U'}
          </div>
          <div className="flex-1 min-w-0">
            <p className="text-sm font-medium text-gray-900 truncate">User</p>
            <p className="text-xs text-gray-500">{role}</p>
          </div>
        </div>
        <button
          onClick={handleLogout}
          className="w-full px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200 transition-colors"
        >
          Logout
        </button>
      </div>
    </div>
  );
};
