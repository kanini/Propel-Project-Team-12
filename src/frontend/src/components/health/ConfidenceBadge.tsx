/**
 * ConfidenceBadge component for displaying AI confidence and verification status
 * Used across Health Dashboard and Clinical Verification views
 */

import type { VerificationStatus } from '../../types/clinicalData';

interface ConfidenceBadgeProps {
  confidenceScore: number;
  verificationStatus: VerificationStatus | string;
}

const statusConfig: Record<string, { label: string; className: string }> = {
  AISuggested: { label: 'AI Suggested', className: 'bg-blue-100 text-blue-800' },
  Verified: { label: 'Verified', className: 'bg-green-100 text-green-800' },
  Rejected: { label: 'Rejected', className: 'bg-red-100 text-red-800' },
  Conflict: { label: 'Conflict', className: 'bg-yellow-100 text-yellow-800' },
};

export function ConfidenceBadge({ confidenceScore, verificationStatus }: ConfidenceBadgeProps) {
  const config = statusConfig[verificationStatus] ?? statusConfig.AISuggested;

  const confidenceColor =
    confidenceScore >= 80
      ? 'text-green-700'
      : confidenceScore >= 50
        ? 'text-yellow-700'
        : 'text-red-700';

  return (
    <div className="flex items-center gap-2">
      <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${config.className}`}>
        {config.label}
      </span>
      <span className={`text-xs font-medium ${confidenceColor}`}>
        {confidenceScore.toFixed(0)}%
      </span>
    </div>
  );
}
