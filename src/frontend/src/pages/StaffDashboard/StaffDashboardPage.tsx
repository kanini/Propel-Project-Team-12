/**
 * Staff Dashboard Page (NFR-006 RBAC)
 * 
 * Main dashboard for Staff role users.
 * Displays patient queue, walk-in bookings, and staff-specific actions.
 */

import React from 'react';

export const StaffDashboardPage: React.FC = () => {
  return (
    <div className="min-h-screen bg-gray-50 p-8">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-semibold text-gray-900">Staff Dashboard</h1>
          <p className="text-gray-600 mt-1">Manage patient queue and appointments</p>
        </div>

        {/* Stats Grid */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
          <div className="bg-white border border-gray-200 rounded-xl p-6 shadow-sm">
            <p className="text-sm font-medium text-gray-600">Patients in Queue</p>
            <p className="text-3xl font-semibold text-gray-900 mt-2">12</p>
          </div>
          
          <div className="bg-white border border-gray-200 rounded-xl p-6 shadow-sm">
            <p className="text-sm font-medium text-gray-600">Walk-ins Today</p>
            <p className="text-3xl font-semibold text-gray-900 mt-2">5</p>
          </div>
          
          <div className="bg-white border border-gray-200 rounded-xl p-6 shadow-sm">
            <p className="text-sm font-medium text-gray-600">Pending Verification</p>
            <p className="text-3xl font-semibold text-gray-900 mt-2">8</p>
          </div>
          
          <div className="bg-white border border-gray-200 rounded-xl p-6 shadow-sm">
            <p className="text-sm font-medium text-gray-600">Appointments Today</p>
            <p className="text-3xl font-semibold text-gray-900 mt-2">28</p>
          </div>
        </div>

        {/* Patient Queue */}
        <div className="bg-white border border-gray-200 rounded-xl p-6 shadow-sm mb-8">
          <h2 className="text-xl font-semibold text-gray-900 mb-4">Today's Queue</h2>
          <div className="space-y-4">
            <div className="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
              <div>
                <p className="font-medium text-gray-900">John Smith</p>
                <p className="text-sm text-gray-600">Dr. Johnson • 2:00 PM</p>
              </div>
              <span className="px-3 py-1 bg-green-100 text-green-800 text-sm font-medium rounded-full">
                Arrived
              </span>
            </div>
            
            <div className="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
              <div>
                <p className="font-medium text-gray-900">Emily Davis</p>
                <p className="text-sm text-gray-600">Dr. Chen • 2:30 PM</p>
              </div>
              <span className="px-3 py-1 bg-blue-100 text-blue-800 text-sm font-medium rounded-full">
                Scheduled
              </span>
            </div>
            
            <div className="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
              <div>
                <p className="font-medium text-gray-900">Michael Brown</p>
                <p className="text-sm text-gray-600">Walk-in</p>
              </div>
              <span className="px-3 py-1 bg-yellow-100 text-yellow-800 text-sm font-medium rounded-full">
                Waiting
              </span>
            </div>
          </div>
        </div>

        {/* Quick Actions */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <button className="flex items-center justify-center gap-2 p-4 bg-blue-500 text-white font-medium rounded-lg hover:bg-blue-600 transition-colors">
            <span>Walk-in Booking</span>
          </button>
          
          <button className="flex items-center justify-center gap-2 p-4 bg-white border border-gray-300 text-gray-700 font-medium rounded-lg hover:border-gray-400 hover:bg-gray-50 transition-colors">
            <span>Search Patients</span>
          </button>
          
          <button className="flex items-center justify-center gap-2 p-4 bg-white border border-gray-300 text-gray-700 font-medium rounded-lg hover:border-gray-400 hover:bg-gray-50 transition-colors">
            <span>Verify Data</span>
          </button>
          
          <button className="flex items-center justify-center gap-2 p-4 bg-white border border-gray-300 text-gray-700 font-medium rounded-lg hover:border-gray-400 hover:bg-gray-50 transition-colors">
            <span>Appointments</span>
          </button>
        </div>
      </div>
    </div>
  );
};
