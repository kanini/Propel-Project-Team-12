/**
 * VerificationBadge Component (EP-008-US-052, UXR-402)
 * Displays verification status with color-coded badges
 * - Amber: AI-Suggested (Pending)
 * - Green: Staff-Verified
 * - Red: Rejected
 */

import React from 'react';

interface VerificationBadgeProps {
  status: 'Pending' | 'StaffVerified' | 'StaffRejected';
  size?: 'sm' | 'md';
}

export const VerificationBadge: React.FC<VerificationBadgeProps> = ({
  status,
  size = 'md',
}) => {
  const baseClasses = 'inline-flex items-center rounded-full font-medium';
  const sizeClasses = size === 'sm' ? 'px-2 py-0.5 text-xs' : 'px-3 py-1 text-sm';

  const statusConfig = {
    Pending: {
      bgColor: 'bg-amber-100',
      textColor: 'text-amber-800',
      label: 'AI-Suggested',
    },
    StaffVerified: {
      bgColor: 'bg-green-100',
      textColor: 'text-green-800',
      label: 'Staff-Verified',
    },
    StaffRejected: {
      bgColor: 'bg-red-100',
      textColor: 'text-red-800',
      label: 'Rejected',
    },
  };

  const config = statusConfig[status];

  return (
    <span
      className={`${baseClasses} ${sizeClasses} ${config.bgColor} ${config.textColor}`}
      role="status"
      aria-label={`Verification status: ${config.label}`}
    >
      {config.label}
    </span>
  );
};
