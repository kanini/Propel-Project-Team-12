/**
 * IntakeStatusBadge component (US_037)
 * Displays intake status with appropriate styling (Pending/In Progress/Completed)
 * Accessible with ARIA labels (UXR-203)
 */

import { memo } from 'react';
import type { IntakeStatus } from '../../../types/intakeAppointment';

interface IntakeStatusBadgeProps {
  status: IntakeStatus;
}

/**
 * Badge configuration for each status
 */
const statusConfig: Record<IntakeStatus, { label: string; className: string; icon: string }> = {
  pending: {
    label: 'Pending',
    className: 'bg-yellow-100 text-yellow-800 border-yellow-200',
    icon: '⏳',
  },
  inProgress: {
    label: 'In Progress',
    className: 'bg-blue-100 text-blue-800 border-blue-200',
    icon: '🔄',
  },
  completed: {
    label: 'Completed',
    className: 'bg-green-100 text-green-800 border-green-200',
    icon: '✓',
  },
};

/**
 * IntakeStatusBadge - Displays intake completion status
 * @param status - Current intake status (pending/inProgress/completed)
 */
function IntakeStatusBadge({ status }: IntakeStatusBadgeProps) {
  const config = statusConfig[status];

  return (
    <span
      className={`inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-medium border ${config.className}`}
      aria-label={`Intake status: ${config.label}`}
      role="status"
    >
      <span aria-hidden="true">{config.icon}</span>
      {config.label}
    </span>
  );
}

export default memo(IntakeStatusBadge);
