/**
 * Patient Dashboard Page (NFR-006 RBAC)
 * 
 * Main dashboard for Patient role users.
 * Displays appointments, health information, and patient-specific actions.
 * Aligned with SCR-003 wireframe specifications.
 */

import React from 'react';

export const PatientDashboardPage: React.FC = () => {
  return (
    <div className="min-h-screen bg-gray-50 p-8">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-semibold text-gray-900">Patient Dashboard</h1>
          <p className="text-gray-600 mt-1">Welcome back! Here's your health overview.</p>
        </div>

        {/* Stats Grid */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
          <div className="bg-white border border-gray-200 rounded-xl p-6 shadow-sm">
            <p className="text-sm font-medium text-gray-600">Upcoming Appointments</p>
            <p className="text-3xl font-semibold text-gray-900 mt-2">2</p>
            <p className="text-sm text-gray-500 mt-1">Next: Tomorrow at 2:00 PM</p>
          </div>
          
          <div className="bg-white border border-gray-200 rounded-xl p-6 shadow-sm">
            <p className="text-sm font-medium text-gray-600">Pending Intake</p>
            <p className="text-3xl font-semibold text-gray-900 mt-2">1</p>
            <p className="text-sm text-gray-500 mt-1">Complete before next visit</p>
          </div>
          
          <div className="bg-white border border-gray-200 rounded-xl p-6 shadow-sm">
            <p className="text-sm font-medium text-gray-600">Documents</p>
            <p className="text-3xl font-semibold text-gray-900 mt-2">5</p>
            <p className="text-sm text-gray-500 mt-1">Lab results, prescriptions</p>
          </div>
        </div>

        {/* Appointments Section */}
        <div className="bg-white border border-gray-200 rounded-xl p-6 shadow-sm mb-8">
          <h2 className="text-xl font-semibold text-gray-900 mb-4">Upcoming Appointments</h2>
          <div className="space-y-4">
            <div className="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
              <div>
                <p className="font-medium text-gray-900">Dr. Sarah Johnson</p>
                <p className="text-sm text-gray-600">Annual Checkup</p>
                <p className="text-sm text-gray-500">Tomorrow, Feb 15 at 2:00 PM</p>
              </div>
              <span className="px-3 py-1 bg-green-100 text-green-800 text-sm font-medium rounded-full">
                Confirmed
              </span>
            </div>
            
            <div className="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
              <div>
                <p className="font-medium text-gray-900">Dr. Michael Chen</p>
                <p className="text-sm text-gray-600">Follow-up Visit</p>
                <p className="text-sm text-gray-500">Feb 22 at 10:30 AM</p>
              </div>
              <span className="px-3 py-1 bg-blue-100 text-blue-800 text-sm font-medium rounded-full">
                Scheduled
              </span>
            </div>
          </div>
        </div>

        {/* Quick Actions */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <button className="flex items-center justify-center gap-2 p-4 bg-blue-500 text-white font-medium rounded-lg hover:bg-blue-600 transition-colors">
            <span>Book Appointment</span>
          </button>
          
          <button className="flex items-center justify-center gap-2 p-4 bg-white border border-gray-300 text-gray-700 font-medium rounded-lg hover:border-gray-400 hover:bg-gray-50 transition-colors">
            <span>Upload Documents</span>
          </button>
          
          <button className="flex items-center justify-center gap-2 p-4 bg-white border border-gray-300 text-gray-700 font-medium rounded-lg hover:border-gray-400 hover:bg-gray-50 transition-colors">
            <span>Health Dashboard</span>
          </button>
        </div>
      </div>
    </div>
  );
};
