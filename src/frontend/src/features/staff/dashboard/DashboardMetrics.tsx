/**
 * Dashboard Metrics Component for US_068 - Staff Dashboard
 * Displays three stat cards: Today's Appointments, Queue Size, Pending Verifications
 */

import type { DashboardMetricsDto } from '../../../store/slices/staffDashboardSlice';

interface DashboardMetricsProps {
  metrics: DashboardMetricsDto;
}

export function DashboardMetrics({ metrics }: DashboardMetricsProps) {
  const statCards = [
    {
      id: 'appointments',
      label: "Today's Appointments",
      value: metrics.todayAppointments,
      icon: '📅',
      bgColor: 'bg-blue-50',
      textColor: 'text-blue-700',
      borderColor: 'border-blue-200',
    },
    {
      id: 'queue',
      label: 'Current Queue',
      value: metrics.currentQueueSize,
      icon: '👥',
      bgColor: 'bg-green-50',
      textColor: 'text-green-700',
      borderColor: 'border-green-200',
    },
    {
      id: 'verifications',
      label: 'Pending Verifications',
      value: metrics.pendingVerifications,
      icon: '✓',
      bgColor: 'bg-amber-50',
      textColor: 'text-amber-700',
      borderColor: 'border-amber-200',
    },
  ];

  return (
    <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
      {statCards.map((card) => (
        <div
          key={card.id}
          className={`${card.bgColor} ${card.borderColor} border rounded-lg p-6 shadow-sm hover:shadow-md transition-shadow`}
        >
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-neutral-600">
                {card.label}
              </p>
              <p className={`text-3xl font-bold mt-2 ${card.textColor}`}>
                {card.value}
              </p>
            </div>
            <div className="text-4xl">{card.icon}</div>
          </div>
        </div>
      ))}
    </div>
  );
}
