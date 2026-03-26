/**
 * Quick Actions Component for US_068 - Staff Dashboard
 * Provides three quick action buttons: Walk-in Booking, View Queue, Mark Arrivals
 */

import { useNavigate } from 'react-router-dom';

export function QuickActions() {
  const navigate = useNavigate();

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
  ];

  return (
    <div className="mb-6">
      <h2 className="text-lg font-semibold text-neutral-900 mb-3">Quick Actions</h2>
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        {actions.map((action) => (
          <button
            key={action.id}
            onClick={() => navigate(action.path)}
            className={`${action.bgColor} ${action.hoverColor} text-white rounded-lg p-4 shadow-sm hover:shadow-md transition-all text-left`}
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
  );
}
