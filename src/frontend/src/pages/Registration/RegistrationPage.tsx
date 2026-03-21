/**
 * Registration Page Component (FR-001 - User Registration)
 * Main page wrapper for user registration
 */

import React from 'react';
import { RegistrationForm } from '../../components/forms/RegistrationForm';

/**
 * Registration page with form and contextual information
 */
export const RegistrationPage: React.FC = () => {
  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center p-4">
      <div className="w-full max-w-md">
        {/* Logo */}
        <div className="flex items-center justify-center gap-2 mb-8">
          <div className="w-8 h-8 bg-blue-500 rounded-lg flex items-center justify-center text-white text-xl font-bold">
            +
          </div>
          <span className="text-xl font-semibold text-gray-900">PatientAccess</span>
        </div>

        {/* Registration Card */}
        <div className="bg-white border border-gray-200 rounded-2xl shadow-sm p-8">
          <RegistrationForm />
        </div>
      </div>
    </div>
  );
};
