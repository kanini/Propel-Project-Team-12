/**
 * Pending Verifications Component for US_068 - Staff Dashboard
 * Displays patients with pending clinical data verifications (EP-009)
 * Shows top 5 patients with priority, name, and pending counts
 */

import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../../../store/hooks';
import {
  fetchVerificationQueue,
  selectVerificationQueue,
  selectQueueLoading,
  selectQueueError,
} from '../../../store/slices/clinicalVerificationSlice';

export function PendingVerifications() {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const queue = useAppSelector(selectVerificationQueue);
  const isLoading = useAppSelector(selectQueueLoading);
  const error = useAppSelector(selectQueueError);

  // Fetch top 5 patients on mount
  useEffect(() => {
    dispatch(fetchVerificationQueue({ limit: 5 }));
  }, [dispatch]);

  // Priority badge styling
  const getPriorityBadge = (priority: string) => {
    switch (priority) {
      case 'High':
        return 'bg-red-100 text-red-700 border-red-200';
      case 'Medium':
        return 'bg-amber-100 text-amber-700 border-amber-200';
      default:
        return 'bg-green-100 text-green-700 border-green-200';
    }
  };

  // Loading state
  if (isLoading) {
    return (
      <div className="bg-neutral-0 border border-neutral-200 rounded-lg p-6">
        <h2 className="text-lg font-semibold text-neutral-900 mb-4">
          Pending Verifications
        </h2>
        <div className="flex items-center justify-center py-8">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600" />
        </div>
      </div>
    );
  }

  // Error state
  if (error) {
    return (
      <div className="bg-neutral-0 border border-neutral-200 rounded-lg p-6">
        <h2 className="text-lg font-semibold text-neutral-900 mb-4">
          Pending Verifications
        </h2>
        <div className="bg-red-50 border border-red-200 rounded-lg p-4">
          <p className="text-red-700 text-sm">{error}</p>
          <button
            onClick={() => dispatch(fetchVerificationQueue({ limit: 5 }))}
            className="mt-2 text-red-600 hover:text-red-700 text-sm font-medium"
          >
            Retry
          </button>
        </div>
      </div>
    );
  }

  // Empty state
  if (queue.length === 0) {
    return (
      <div className="bg-neutral-0 border border-neutral-200 rounded-lg p-6">
        <h2 className="text-lg font-semibold text-neutral-900 mb-4">
          Pending Verifications
        </h2>
        <div className="text-center py-8">
          <p className="text-neutral-500 mb-2">
            No pending verifications
          </p>
          <p className="text-xs text-neutral-400">
            All clinical data has been verified
          </p>
        </div>
      </div>
    );
  }

  // Calculate total pending items
  const totalPending = queue.reduce(
    (sum, item) => sum + item.pendingClinicalDataCount + item.pendingMedicalCodesCount,
    0
  );

  return (
    <div className="bg-neutral-0 border border-neutral-200 rounded-lg p-6">
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-lg font-semibold text-neutral-900">
          Pending Verifications
        </h2>
        <button
          onClick={() => navigate('/staff/verification')}
          className="text-primary-600 hover:text-primary-700 font-medium text-sm"
        >
          View All →
        </button>
      </div>

      <div className="mb-4 p-3 bg-amber-50 border border-amber-200 rounded-lg">
        <p className="text-sm text-amber-800">
          <span className="font-semibold">{totalPending}</span> items need verification across{' '}
          <span className="font-semibold">{queue.length}</span> patients
        </p>
      </div>

      <div className="space-y-3">
        {queue.map((item) => (
          <div
            key={item.patientId}
            onClick={() => navigate(`/staff/verification/${item.patientId}`)}
            className="p-3 border border-neutral-200 rounded-lg hover:bg-neutral-50 cursor-pointer transition-colors"
          >
            <div className="flex items-start justify-between mb-2">
              <div className="flex-1 min-w-0">
                <h3 className="text-sm font-semibold text-neutral-900 truncate">
                  {item.patientName}
                </h3>
                <p className="text-xs text-neutral-500 mt-0.5">
                  ID: {item.patientId}
                </p>
              </div>
              <span
                className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium border ${getPriorityBadge(item.priority)}`}
              >
                {item.priority}
              </span>
            </div>

            <div className="flex items-center gap-3 text-xs">
              <div className="flex items-center gap-1">
                <span className="text-neutral-600">Clinical:</span>
                <span
                  className={`font-semibold ${item.pendingClinicalDataCount > 0 ? 'text-amber-700' : 'text-neutral-500'}`}
                >
                  {item.pendingClinicalDataCount}
                </span>
              </div>
              <div className="flex items-center gap-1">
                <span className="text-neutral-600">Codes:</span>
                <span
                  className={`font-semibold ${item.pendingMedicalCodesCount > 0 ? 'text-amber-700' : 'text-neutral-500'}`}
                >
                  {item.pendingMedicalCodesCount}
                </span>
              </div>
              {item.conflictCount > 0 && (
                <div className="flex items-center gap-1">
                  <span className="text-red-600">⚠️ Conflicts:</span>
                  <span className="font-semibold text-red-700">
                    {item.conflictCount}
                  </span>
                </div>
              )}
            </div>

            {item.lastUploadDate && (
              <p className="text-xs text-neutral-400 mt-2">
                Last upload: {new Date(item.lastUploadDate).toLocaleDateString()}
              </p>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
