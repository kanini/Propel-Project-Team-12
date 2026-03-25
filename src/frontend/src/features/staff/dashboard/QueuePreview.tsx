/**
 * Queue Preview Component for US_068 - Staff Dashboard
 * Displays next 5 patients in chronological order with appointment details
 */

import { useNavigate } from 'react-router-dom';
import type { QueuePreviewDto } from '../../../store/slices/staffDashboardSlice';

interface QueuePreviewProps {
  queue: QueuePreviewDto[];
}

export function QueuePreview({ queue }: QueuePreviewProps) {
  const navigate = useNavigate();

  if (queue.length === 0) {
    return (
      <div className="bg-neutral-0 border border-neutral-200 rounded-lg p-6">
        <h2 className="text-lg font-semibold text-neutral-900 mb-3">
          Today's Queue
        </h2>
        <div className="text-center py-8">
          <p className="text-neutral-500">No patients in queue</p>
          <button
            onClick={() => navigate('/staff/walkin')}
            className="mt-4 text-primary-600 hover:text-primary-700 font-medium"
          >
            Book a Walk-in →
          </button>
        </div>
      </div>
    );
  }

  const getRiskBadgeColor = (level: string) => {
    switch (level) {
      case 'high':
        return 'bg-red-100 text-red-700 border-red-200';
      case 'medium':
        return 'bg-amber-100 text-amber-700 border-amber-200';
      default:
        return 'bg-green-100 text-green-700 border-green-200';
    }
  };

  return (
    <div className="bg-neutral-0 border border-neutral-200 rounded-lg p-6">
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-lg font-semibold text-neutral-900">
          Today's Queue Preview
        </h2>
        <button
          onClick={() => navigate('/staff/queue')}
          className="text-primary-600 hover:text-primary-700 font-medium text-sm"
        >
          View Full Queue →
        </button>
      </div>

      <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-neutral-200">
          <thead>
            <tr className="bg-neutral-50">
              <th className="px-4 py-3 text-left text-xs font-medium text-neutral-600 uppercase tracking-wider">
                Time
              </th>
              <th className="px-4 py-3 text-left text-xs font-medium text-neutral-600 uppercase tracking-wider">
                Patient
              </th>
              <th className="px-4 py-3 text-left text-xs font-medium text-neutral-600 uppercase tracking-wider">
                Provider
              </th>
              <th className="px-4 py-3 text-left text-xs font-medium text-neutral-600 uppercase tracking-wider">
                Wait
              </th>
              <th className="px-4 py-3 text-left text-xs font-medium text-neutral-600 uppercase tracking-wider">
                Risk
              </th>
            </tr>
          </thead>
          <tbody className="bg-neutral-0 divide-y divide-neutral-200">
            {queue.map((item) => (
              <tr key={item.appointmentId} className="hover:bg-neutral-50">
                <td className="px-4 py-3 text-sm text-neutral-900">
                  {new Date(item.appointmentTime).toLocaleTimeString('en-US', {
                    hour: 'numeric',
                    minute: '2-digit',
                  })}
                </td>
                <td className="px-4 py-3 text-sm font-medium text-neutral-900">
                  {item.patientName}
                </td>
                <td className="px-4 py-3 text-sm text-neutral-600">
                  {item.providerName}
                </td>
                <td className="px-4 py-3 text-sm text-neutral-600">
                  {item.estimatedWait}
                </td>
                <td className="px-4 py-3 text-sm">
                  <span
                    className={`inline-flex items-center px-2 py-1 rounded-full text-xs font-medium border ${getRiskBadgeColor(item.riskLevel)}`}
                  >
                    {item.riskLevel}
                  </span>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
