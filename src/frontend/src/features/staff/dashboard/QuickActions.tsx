/**
 * Quick Actions Component for US_068 - Staff Dashboard
 * Provides three quick action buttons: Walk-in Booking, View Queue, Mark Arrivals
 */

import { useNavigate } from 'react-router-dom';
import { useState } from 'react';

export function QuickActions() {
  const navigate = useNavigate();
  const [showTestModal, setShowTestModal] = useState(false);
  const [testId, setTestId] = useState('');

  const handleTestNavigation = () => {
    if (testId.trim()) {
      navigate(`/staff/verification/${testId.trim()}`);
      setShowTestModal(false);
      setTestId('');
    }
  };

  const actions = [
    {
      id: 'walkin',
      label: 'Walk-in Booking',
      icon: '🚶',
      description: 'Book immediate appointments',
      path: '/staff/walkin',
      bgColor: 'bg-primary-600',
      hoverColor: 'hover:bg-primary-700',
    },
    {
      id: 'queue',
      label: 'View Queue',
      icon: '📋',
      description: 'Manage patient queue',
      path: '/staff/queue',
      bgColor: 'bg-info',
      hoverColor: 'hover:bg-info-dark',
    },
    {
      id: 'arrivals',
      label: 'Mark Arrivals',
      icon: '✅',
      description: 'Check in patients',
      path: '/staff/arrivals',
      bgColor: 'bg-success',
      hoverColor: 'hover:bg-success-dark',
    },
    {
      id: 'test-verification',
      label: '🧪 Test Verification',
      icon: '🔬',
      description: '(DEV) Test code verification UI',
      onClick: () => setShowTestModal(true),
      bgColor: 'bg-amber-500',
      hoverColor: 'hover:bg-amber-600',
      isDev: true,
    },
  ];

  return (
    <>
      <div className="mb-6">
        <h2 className="text-lg font-semibold text-neutral-900 mb-3">Quick Actions</h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          {actions.map((action) => (
            <button
              key={action.id}
              onClick={() => action.onClick ? action.onClick() : navigate(action.path!)}
              className={`${action.bgColor} ${action.hoverColor} text-white rounded-lg p-4 shadow-sm hover:shadow-md transition-all text-left ${action.isDev ? 'border-2 border-amber-300' : ''}`}
            >
              <div className="flex items-center gap-3">
                <div className="text-3xl">{action.icon}</div>
                <div>
                  <h3 className="font-semibold">{action.label}</h3>
                  <p className="text-sm opacity-90">{action.description}</p>
                </div>
              </div>
            </button>
          ))}
        </div>
      </div>

      {/* Test Verification Modal */}
      {showTestModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg shadow-xl w-full max-w-md p-6">
            <h3 className="text-xl font-semibold mb-4">🧪 Test Medical Code Verification</h3>
            
            <div className="mb-4 p-3 bg-blue-50 border border-blue-200 rounded text-sm">
              <p className="text-blue-800 mb-2">
                <strong>Note:</strong> This is a development shortcut. In production, navigation would be from the Verification Queue (US_055).
              </p>
            </div>

            <div className="mb-4">
              <label className="block text-sm font-medium text-gray-700 mb-2">
                ExtractedClinicalDataId (GUID)
              </label>
              <input
                type="text"
                value={testId}
                onChange={(e) => setTestId(e.target.value)}
                placeholder="e.g., a1b2c3d4-e5f6-7890-abcd-ef1234567890"
                className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:outline-none font-mono text-sm"
              />
              <p className="mt-2 text-xs text-gray-500">
                Get this ID from the database or create test data via API
              </p>
            </div>

            <div className="mb-4 p-3 bg-amber-50 border border-amber-200 rounded text-xs">
              <p className="font-semibold text-amber-900 mb-1">Quick SQL Query:</p>
              <code className="block bg-white p-2 rounded text-amber-800 overflow-x-auto">
                SELECT ExtractedDataId FROM ExtractedClinicalData LIMIT 1;
              </code>
            </div>

            <div className="flex justify-end gap-3">
              <button
                onClick={() => {
                  setShowTestModal(false);
                  setTestId('');
                }}
                className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50"
              >
                Cancel
              </button>
              <button
                onClick={handleTestNavigation}
                disabled={!testId.trim()}
                className="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Navigate
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
