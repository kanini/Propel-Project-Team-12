/**
 * Admin Dashboard Page (NFR-006 RBAC)
 * 
 * Main dashboard for Admin role users.
 * Displays system metrics, user management, audit logs, and admin-specific actions.
 */

import React from 'react';

export const AdminDashboardPage: React.FC = () => {
  return (
    <div className="min-h-screen bg-gray-50 p-8">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-semibold text-gray-900">Admin Dashboard</h1>
          <p className="text-gray-600 mt-1">System management and monitoring</p>
        </div>

        {/* Stats Grid */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
          <div className="bg-white border border-gray-200 rounded-xl p-6 shadow-sm">
            <p className="text-sm font-medium text-gray-600">Total Users</p>
            <p className="text-3xl font-semibold text-gray-900 mt-2">1,284</p>
          </div>
          
          <div className="bg-white border border-gray-200 rounded-xl p-6 shadow-sm">
            <p className="text-sm font-medium text-gray-600">Active Patients</p>
            <p className="text-3xl font-semibold text-gray-900 mt-2">956</p>
          </div>
          
          <div className="bg-white border border-gray-200 rounded-xl p-6 shadow-sm">
            <p className="text-sm font-medium text-gray-600">Staff Members</p>
            <p className="text-3xl font-semibold text-gray-900 mt-2">24</p>
          </div>
          
          <div className="bg-white border border-gray-200 rounded-xl p-6 shadow-sm">
            <p className="text-sm font-medium text-gray-600">System Health</p>
            <p className="text-3xl font-semibold text-green-600 mt-2">99.9%</p>
          </div>
        </div>

        {/* Users Table */}
        <div className="bg-white border border-gray-200 rounded-xl p-6 shadow-sm mb-8">
          <h2 className="text-xl font-semibold text-gray-900 mb-4">Recent Users</h2>
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-gray-200">
                  <th className="text-left py-3 px-4 text-sm font-medium text-gray-600">Name</th>
                  <th className="text-left py-3 px-4 text-sm font-medium text-gray-600">Role</th>
                  <th className="text-left py-3 px-4 text-sm font-medium text-gray-600">Status</th>
                  <th className="text-left py-3 px-4 text-sm font-medium text-gray-600">Last Active</th>
                </tr>
              </thead>
              <tbody>
                <tr className="border-b border-gray-100">
                  <td className="py-3 px-4 text-sm text-gray-900">John Smith</td>
                  <td className="py-3 px-4 text-sm text-gray-600">Patient</td>
                  <td className="py-3 px-4">
                    <span className="px-2 py-1 bg-green-100 text-green-800 text-xs font-medium rounded-full">Active</span>
                  </td>
                  <td className="py-3 px-4 text-sm text-gray-600">2 hours ago</td>
                </tr>
                
                <tr className="border-b border-gray-100">
                  <td className="py-3 px-4 text-sm text-gray-900">Sarah Johnson</td>
                  <td className="py-3 px-4 text-sm text-gray-600">Staff</td>
                  <td className="py-3 px-4">
                    <span className="px-2 py-1 bg-green-100 text-green-800 text-xs font-medium rounded-full">Active</span>
                  </td>
                  <td className="py-3 px-4 text-sm text-gray-600">10 minutes ago</td>
                </tr>
                
                <tr className="border-b border-gray-100">
                  <td className="py-3 px-4 text-sm text-gray-900">Michael Chen</td>
                  <td className="py-3 px-4 text-sm text-gray-600">Admin</td>
                  <td className="py-3 px-4">
                    <span className="px-2 py-1 bg-green-100 text-green-800 text-xs font-medium rounded-full">Active</span>
                  </td>
                  <td className="py-3 px-4 text-sm text-gray-600">1 day ago</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        {/* Quick Actions */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <button className="flex items-center justify-center gap-2 p-4 bg-blue-500 text-white font-medium rounded-lg hover:bg-blue-600 transition-colors">
            <span>Manage Users</span>
          </button>
          
          <button className="flex items-center justify-center gap-2 p-4 bg-white border border-gray-300 text-gray-700 font-medium rounded-lg hover:border-gray-400 hover:bg-gray-50 transition-colors">
            <span>Audit Logs</span>
          </button>
          
          <button className="flex items-center justify-center gap-2 p-4 bg-white border border-gray-300 text-gray-700 font-medium rounded-lg hover:border-gray-400 hover:bg-gray-50 transition-colors">
            <span>Settings</span>
          </button>
          
          <button className="flex items-center justify-center gap-2 p-4 bg-white border border-gray-300 text-gray-700 font-medium rounded-lg hover:border-gray-400 hover:bg-gray-50 transition-colors">
            <span>Reports</span>
          </button>
        </div>
      </div>
    </div>
  );
};
