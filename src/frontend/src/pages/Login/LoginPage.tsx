/**
 * Login Page Component (FR-002 - User Authentication)
 * Main page wrapper for user login
 * Matches wireframe SCR-002 design specifications
 */

import React from 'react';
import { LoginForm } from '../../components/forms/LoginForm';

/**
 * Login page with logo, form and proper styling per wireframe
 */
export const LoginPage: React.FC = () => {
  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center p-4">
      <div className="w-full max-w-md">
        {/* Logo */}
        <div className="flex items-center justify-center gap-2 mb-8">
          <div className="w-8 h-8 bg-blue-500 rounded-lg flex items-center justify-center text-white font-bold text-lg" aria-hidden="true">
            +
          </div>
          <span className="text-xl font-semibold text-gray-900">PatientAccess</span>
        </div>

        {/* Login Card */}
        <div className="bg-white border border-gray-200 rounded-2xl shadow-sm p-8">
          <LoginForm />
        </div>
      </div>
    </div>
  );
};
