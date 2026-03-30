/**
 * VerificationQueuePage for SCR-023A - Verification Queue
 * Shows patients with pending verifications by default, with search/filter.
 * Staff clicks "Review" to navigate to the detail ClinicalVerificationPage.
 */

import { useEffect, useState, useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import type { AppDispatch } from '../store';
import {
  fetchVerificationQueue,
  selectVerificationQueue,
  selectQueueLoading,
  selectQueueError,
} from '../store/slices/clinicalVerificationSlice';

export function VerificationQueuePage() {
  const dispatch = useDispatch<AppDispatch>();
  const navigate = useNavigate();
  const queue = useSelector(selectVerificationQueue);
  const isLoading = useSelector(selectQueueLoading);
  const error = useSelector(selectQueueError);

  const [searchTerm, setSearchTerm] = useState('');
  const [priorityFilter, setPriorityFilter] = useState('');

  // Load queue on mount
  useEffect(() => {
    dispatch(fetchVerificationQueue({ limit: 10 }));
  }, [dispatch]);

  // Debounced search
  const handleSearch = useCallback(
    (value: string) => {
      setSearchTerm(value);
      dispatch(fetchVerificationQueue({ limit: 10, search: value || undefined }));
    },
    [dispatch],
  );

  const handleReview = (patientId: string) => {
    navigate(`/staff/verification/${patientId}`);
  };

  // Client-side priority filter
  const filteredQueue = priorityFilter
    ? queue.filter((item) => item.priority === priorityFilter)
    : queue;

  const pendingTotal = queue.reduce(
    (sum, item) => sum + item.pendingClinicalDataCount + item.pendingMedicalCodesCount,
    0,
  );

  const getPriorityClasses = (priority: string) => {
    switch (priority) {
      case 'High':
        return 'text-red-600';
      case 'Medium':
        return 'text-amber-600';
      default:
        return 'text-green-600';
    }
  };

  const getCountPillClasses = (count: number) =>
    count > 0
      ? 'bg-amber-50 text-amber-700'
      : 'bg-neutral-100 text-neutral-500';

  const getConflictPillClasses = (count: number) =>
    count > 0
      ? 'bg-red-50 text-red-700'
      : 'bg-neutral-100 text-neutral-500';

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex items-center justify-between flex-wrap gap-3">
        <h1 className="text-2xl font-bold text-neutral-900">Verification Queue</h1>
        <p className="text-sm text-neutral-500">
          {pendingTotal} items pending verification across {queue.length} patients
        </p>
      </div>

      {/* Filter Bar */}
      <div className="flex gap-3 p-4 bg-white border border-neutral-200 rounded-lg shadow-sm flex-wrap items-center">
        <input
          type="search"
          value={searchTerm}
          onChange={(e) => handleSearch(e.target.value)}
          placeholder="Search by patient name or ID..."
          aria-label="Search patients"
          className="flex-1 min-w-[280px] px-3 py-2 border border-neutral-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none"
        />
        <select
          value={priorityFilter}
          onChange={(e) => setPriorityFilter(e.target.value)}
          aria-label="Filter by priority"
          className="px-3 py-2 border border-neutral-300 rounded-lg text-sm bg-white focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none"
        >
          <option value="">All Priorities</option>
          <option value="High">High Priority</option>
          <option value="Medium">Medium Priority</option>
          <option value="Low">Low Priority</option>
        </select>
      </div>

      {/* Error state */}
      {error && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-4">
          <p className="text-red-700 text-sm">{error}</p>
        </div>
      )}

      {/* Loading state */}
      {isLoading && (
        <div className="flex items-center justify-center py-12">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600" />
        </div>
      )}

      {/* Queue Table */}
      {!isLoading && filteredQueue.length > 0 && (
        <div className="bg-white border border-neutral-200 rounded-lg shadow-sm overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full" aria-label="Patients with pending verifications">
              <thead>
                <tr className="bg-neutral-50 border-b border-neutral-200">
                  <th className="text-left text-sm font-semibold text-neutral-700 px-4 py-3">Priority</th>
                  <th className="text-left text-sm font-semibold text-neutral-700 px-4 py-3">Patient Name</th>
                  <th className="text-left text-sm font-semibold text-neutral-700 px-4 py-3">Patient ID</th>
                  <th className="text-center text-sm font-semibold text-neutral-700 px-4 py-3">Clinical Data</th>
                  <th className="text-center text-sm font-semibold text-neutral-700 px-4 py-3">Medical Codes</th>
                  <th className="text-center text-sm font-semibold text-neutral-700 px-4 py-3">Conflicts</th>
                  <th className="text-left text-sm font-semibold text-neutral-700 px-4 py-3">Last Upload</th>
                  <th className="text-left text-sm font-semibold text-neutral-700 px-4 py-3">Action</th>
                </tr>
              </thead>
              <tbody>
                {filteredQueue.map((item) => (
                  <tr
                    key={item.patientId}
                    className="border-b border-neutral-100 hover:bg-neutral-50 cursor-pointer"
                    onClick={() => handleReview(item.patientId)}
                  >
                    <td className="px-4 py-3">
                      <span className={`text-xs font-semibold flex items-center gap-1 ${getPriorityClasses(item.priority)}`}>
                        <span aria-hidden="true">&#9679;</span> {item.priority}
                      </span>
                    </td>
                    <td className="px-4 py-3 font-semibold text-sm text-neutral-900">
                      {item.patientName}
                    </td>
                    <td className="px-4 py-3 text-xs text-neutral-500 font-mono">
                      {item.patientId.substring(0, 8)}...
                    </td>
                    <td className="px-4 py-3 text-center">
                      <span className={`inline-flex items-center justify-center min-w-[24px] h-5 px-1.5 rounded-full text-xs font-semibold ${getCountPillClasses(item.pendingClinicalDataCount)}`}>
                        {item.pendingClinicalDataCount}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-center">
                      <span className={`inline-flex items-center justify-center min-w-[24px] h-5 px-1.5 rounded-full text-xs font-semibold ${getCountPillClasses(item.pendingMedicalCodesCount)}`}>
                        {item.pendingMedicalCodesCount}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-center">
                      <span className={`inline-flex items-center justify-center min-w-[24px] h-5 px-1.5 rounded-full text-xs font-semibold ${getConflictPillClasses(item.conflictCount)}`}>
                        {item.conflictCount}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-xs text-neutral-500">
                      {item.lastUploadDate
                        ? new Date(item.lastUploadDate).toLocaleDateString('en-US', {
                            month: 'short',
                            day: 'numeric',
                            year: 'numeric',
                          })
                        : '—'}
                    </td>
                    <td className="px-4 py-3">
                      <button
                        onClick={(e) => {
                          e.stopPropagation();
                          handleReview(item.patientId);
                        }}
                        className="px-3 py-1.5 bg-blue-600 text-white text-xs font-medium rounded-lg hover:bg-blue-700 transition-colors"
                      >
                        Review
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Empty state */}
      {!isLoading && filteredQueue.length === 0 && !error && (
        <div className="flex items-center justify-center min-h-[300px]">
          <div className="text-center max-w-md">
            <div className="text-neutral-300 text-5xl mb-3">&#9989;</div>
            <h2 className="text-lg font-semibold text-neutral-900 mb-2">All Caught Up</h2>
            <p className="text-neutral-500 text-sm">
              No patients have pending clinical data or medical code verifications.
            </p>
          </div>
        </div>
      )}

      {/* Info Banner */}
      <div className="p-3 bg-blue-50 rounded-lg text-xs text-blue-700">
        <strong>Priority Calculation:</strong> Priority is automatically determined based on pending
        data volume. High-priority patients should be reviewed first to ensure critical data is
        verified before clinical encounters.
      </div>
    </div>
  );
}
