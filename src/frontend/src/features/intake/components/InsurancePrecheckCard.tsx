/**
 * InsurancePrecheckCard component (US_036)
 * Displays insurance verification status result
 */

import { memo } from 'react';

export interface PrecheckResult {
  status: 'verified' | 'pending' | 'failed' | 'not_found';
  providerName?: string;
  memberId?: string;
  effectiveDate?: string;
  expirationDate?: string;
  copayAmount?: number;
  deductibleRemaining?: number;
  message?: string;
}

interface InsurancePrecheckCardProps {
  result: PrecheckResult;
  onRetry?: () => void;
  onManualEntry?: () => void;
}

const statusConfig = {
  verified: {
    bgColor: 'bg-green-50',
    borderColor: 'border-green-200',
    iconColor: 'text-green-500',
    titleColor: 'text-green-800',
    icon: (
      <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
        <path
          fillRule="evenodd"
          d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z"
          clipRule="evenodd"
        />
      </svg>
    ),
    title: 'Insurance Verified',
  },
  pending: {
    bgColor: 'bg-yellow-50',
    borderColor: 'border-yellow-200',
    iconColor: 'text-yellow-500',
    titleColor: 'text-yellow-800',
    icon: (
      <svg className="w-5 h-5 animate-spin" fill="none" viewBox="0 0 24 24">
        <circle
          className="opacity-25"
          cx="12"
          cy="12"
          r="10"
          stroke="currentColor"
          strokeWidth="4"
        />
        <path
          className="opacity-75"
          fill="currentColor"
          d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"
        />
      </svg>
    ),
    title: 'Verifying Insurance',
  },
  failed: {
    bgColor: 'bg-red-50',
    borderColor: 'border-red-200',
    iconColor: 'text-red-500',
    titleColor: 'text-red-800',
    icon: (
      <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
        <path
          fillRule="evenodd"
          d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
          clipRule="evenodd"
        />
      </svg>
    ),
    title: 'Verification Failed',
  },
  not_found: {
    bgColor: 'bg-neutral-50',
    borderColor: 'border-neutral-200',
    iconColor: 'text-neutral-500',
    titleColor: 'text-neutral-800',
    icon: (
      <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
        <path
          fillRule="evenodd"
          d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z"
          clipRule="evenodd"
        />
      </svg>
    ),
    title: 'Insurance Not Found',
  },
};

/**
 * InsurancePrecheckCard - Shows insurance verification result
 */
function InsurancePrecheckCard({
  result,
  onRetry,
  onManualEntry,
}: InsurancePrecheckCardProps) {
  const config = statusConfig[result.status];

  return (
    <div
      className={`rounded-lg border p-4 ${config.bgColor} ${config.borderColor}`}
      role="status"
      aria-live="polite"
    >
      {/* Header */}
      <div className="flex items-center gap-3 mb-3">
        <span className={config.iconColor}>{config.icon}</span>
        <h3 className={`font-medium ${config.titleColor}`}>{config.title}</h3>
      </div>

      {/* Details for verified status */}
      {result.status === 'verified' && (
        <div className="space-y-2 text-sm">
          {result.providerName && (
            <div className="flex justify-between">
              <span className="text-neutral-600">Provider:</span>
              <span className="font-medium text-neutral-900">{result.providerName}</span>
            </div>
          )}
          {result.memberId && (
            <div className="flex justify-between">
              <span className="text-neutral-600">Member ID:</span>
              <span className="font-medium text-neutral-900">
                ****{result.memberId.slice(-4)}
              </span>
            </div>
          )}
          {result.copayAmount !== undefined && (
            <div className="flex justify-between">
              <span className="text-neutral-600">Estimated Copay:</span>
              <span className="font-medium text-neutral-900">
                ${result.copayAmount.toFixed(2)}
              </span>
            </div>
          )}
          {result.deductibleRemaining !== undefined && (
            <div className="flex justify-between">
              <span className="text-neutral-600">Deductible Remaining:</span>
              <span className="font-medium text-neutral-900">
                ${result.deductibleRemaining.toFixed(2)}
              </span>
            </div>
          )}
        </div>
      )}

      {/* Message for non-verified statuses */}
      {result.status !== 'verified' && result.message && (
        <p className="text-sm text-neutral-600">{result.message}</p>
      )}

      {/* Action buttons */}
      {(result.status === 'failed' || result.status === 'not_found') && (
        <div className="mt-4 flex gap-3">
          {onRetry && (
            <button
              type="button"
              onClick={onRetry}
              className="text-sm text-primary-600 hover:text-primary-700 font-medium"
            >
              Try Again
            </button>
          )}
          {onManualEntry && (
            <button
              type="button"
              onClick={onManualEntry}
              className="text-sm text-neutral-600 hover:text-neutral-700"
            >
              Enter Manually
            </button>
          )}
        </div>
      )}
    </div>
  );
}

export default memo(InsurancePrecheckCard);
