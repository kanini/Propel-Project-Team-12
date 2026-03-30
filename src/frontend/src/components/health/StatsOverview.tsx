/**
 * StatsOverview component for Health Dashboard 360° (SCR-016)
 * Displays summary statistics cards at the top of the dashboard
 */

import type { DashboardStatsOverviewDto } from '../../types/clinicalData';

interface StatsOverviewProps {
  stats: DashboardStatsOverviewDto;
}

const statCards = [
  { key: 'totalExtractedItems', label: 'Total Extracted', icon: '📋', color: 'border-blue-400 bg-blue-50' },
  { key: 'verifiedItems', label: 'Verified', icon: '✅', color: 'border-green-400 bg-green-50' },
  { key: 'pendingItems', label: 'Pending Review', icon: '⏳', color: 'border-yellow-400 bg-yellow-50' },
  { key: 'totalDocuments', label: 'Documents', icon: '📄', color: 'border-purple-400 bg-purple-50' },
  { key: 'totalMedicalCodes', label: 'Medical Codes', icon: '🏷️', color: 'border-indigo-400 bg-indigo-50' },
] as const;

export function StatsOverview({ stats }: StatsOverviewProps) {
  return (
    <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-5 gap-4">
      {statCards.map(({ key, label, icon, color }) => (
        <div
          key={key}
          className={`rounded-lg border-l-4 p-4 shadow-sm ${color}`}
        >
          <div className="flex items-center gap-2 mb-1">
            <span className="text-lg" role="img" aria-label={label}>{icon}</span>
            <span className="text-sm font-medium text-neutral-600">{label}</span>
          </div>
          <p className="text-2xl font-bold text-neutral-900">
            {stats[key as keyof DashboardStatsOverviewDto]}
          </p>
        </div>
      ))}
    </div>
  );
}
