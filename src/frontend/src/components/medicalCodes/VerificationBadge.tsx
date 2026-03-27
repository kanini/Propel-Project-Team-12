/**
 * Verification Badge Component
 * Displays verification status with color-coded badges (UXR-402)
 * - Amber: AI-suggested (Pending)
 * - Green: Staff-verified
 * - Red: Rejected
 */

import React from 'react';
import type { VerificationStatus } from '../../types/medicalCode.types';

interface VerificationBadgeProps {
  status: VerificationStatus;
  size?: 'sm' | 'md';
}

export const VerificationBadge: React.FC<VerificationBadgeProps> = ({
  status,
  size = 'md',
}) => {
  const baseClasses = 'inline-flex items-center rounded-full font-medium';
  const sizeClasses =
    size === 'sm' ? 'px-2 py-0.5 text-xs' : 'px-3 py-1 text-sm';

  const statusConfig: Record<
    VerificationStatus,
    { bgColor: string; textColor: string; label: string }
  > = {
    AISuggested: {
      bgColor: 'bg-amber-100',
      textColor: 'text-amber-800',
      label: 'AI-Suggested',
    },
    Accepted: {
      bgColor: 'bg-green-100',
      textColor: 'text-green-800',
      label: 'Accepted',
    },
    Modified: {
      bgColor: 'bg-blue-100',
      textColor: 'text-blue-800',
      label: 'Modified',
    },
    Rejected: {
      bgColor: 'bg-red-100',
      textColor: 'text-red-800',
      label: 'Rejected',
    },
  };

  const config = statusConfig[status];

  return (
    <span
      className={`${baseClasses} ${sizeClasses} ${config.bgColor} ${config.textColor}`}
    >
      {config.label}
    </span>
  );
};
