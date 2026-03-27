/**
 * ConflictList Component (US_048, AC2)
 * Displays paginated list of conflicts with filtering and sorting.
 * Always sorted by severity (Critical first), then by date (newest first).
 */

import { useEffect } from "react";
import { useAppDispatch, useAppSelector } from "../store/hooks";
import {
  fetchPatientConflicts,
  setFilterSeverity,
  setCurrentPage,
  openConflictModal,
} from "../store/slices/conflictsSlice";
import { ConflictSeverity } from "../types/conflict.types";
import { SeverityBadge } from "./SeverityBadge";

interface ConflictListProps {
  patientId: number;
  className?: string;
}

export const ConflictList = ({
  patientId,
  className = "",
}: ConflictListProps) => {
  const dispatch = useAppDispatch();
  const {
    conflicts,
    conflictsLoading,
    conflictsError,
    filterSeverity,
    currentPage,
    pageSize,
  } = useAppSelector((state) => state.conflicts);

  // Fetch conflicts on mount and when filters change
  useEffect(() => {
    dispatch(
      fetchPatientConflicts({
        patientId,
        severity: filterSeverity || undefined,
        unresolvedOnly: true,
        page: currentPage,
        pageSize,
      }),
    );
  }, [dispatch, patientId, filterSeverity, currentPage, pageSize]);

  const handleFilterChange = (severity: ConflictSeverity | null) => {
    dispatch(setFilterSeverity(severity));
  };

  const handlePageChange = (page: number) => {
    dispatch(setCurrentPage(page));
  };

  const handleConflictClick = (conflict: any) => {
    dispatch(openConflictModal(conflict));
  };

  const formatTimestamp = (timestamp: string) => {
    const date = new Date(timestamp);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 60) return `${diffMins} minutes ago`;
    if (diffHours < 24) return `${diffHours} hours ago`;
    if (diffDays < 7) return `${diffDays} days ago`;
    return date.toLocaleDateString();
  };

  const getEntityBadgeColor = (entityType: string) => {
    switch (entityType.toLowerCase()) {
      case "medication":
        return "bg-purple-100 text-purple-800";
      case "allergy":
        return "bg-orange-100 text-orange-800";
      case "condition":
      case "diagnosis":
        return "bg-green-100 text-green-800";
      default:
        return "bg-gray-100 text-gray-800";
    }
  };

  if (conflictsLoading && conflicts.length === 0) {
    return (
      <div className={`bg-white rounded-lg shadow p-6 ${className}`}>
        <div className="flex items-center justify-center">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
          <span className="ml-3 text-gray-600">Loading conflicts...</span>
        </div>
      </div>
    );
  }

  if (conflictsError) {
    return (
      <div
        className={`bg-red-50 border border-red-200 rounded-lg p-4 ${className}`}
      >
        <p className="text-red-800">
          Error loading conflicts: {conflictsError}
        </p>
      </div>
    );
  }

  return (
    <div className={`bg-white rounded-lg shadow ${className}`}>
      {/* Header with filter tabs */}
      <div className="border-b border-gray-200 px-6 py-4">
        <h2 className="text-lg font-semibold text-gray-900 mb-4">
          Data Conflicts
        </h2>
        <div className="flex gap-2 flex-wrap">
          <button
            onClick={() => handleFilterChange(null)}
            className={`px-4 py-2 rounded-md text-sm font-medium transition-colors ${
              filterSeverity === null
                ? "bg-blue-600 text-white"
                : "bg-gray-100 text-gray-700 hover:bg-gray-200"
            }`}
          >
            All ({conflicts.length})
          </button>
          <button
            onClick={() => handleFilterChange(ConflictSeverity.Critical)}
            className={`px-4 py-2 rounded-md text-sm font-medium transition-colors ${
              filterSeverity === ConflictSeverity.Critical
                ? "bg-red-600 text-white"
                : "bg-gray-100 text-gray-700 hover:bg-gray-200"
            }`}
          >
            Critical
          </button>
          <button
            onClick={() => handleFilterChange(ConflictSeverity.Warning)}
            className={`px-4 py-2 rounded-md text-sm font-medium transition-colors ${
              filterSeverity === ConflictSeverity.Warning
                ? "bg-amber-600 text-white"
                : "bg-gray-100 text-gray-700 hover:bg-gray-200"
            }`}
          >
            Warning
          </button>
          <button
            onClick={() => handleFilterChange(ConflictSeverity.Info)}
            className={`px-4 py-2 rounded-md text-sm font-medium transition-colors ${
              filterSeverity === ConflictSeverity.Info
                ? "bg-blue-600 text-white"
                : "bg-gray-100 text-gray-700 hover:bg-gray-200"
            }`}
          >
            Info
          </button>
        </div>
      </div>

      {/* Conflict list */}
      <div className="divide-y divide-gray-200">
        {conflicts.length === 0 ? (
          <div className="px-6 py-12 text-center">
            <svg
              className="mx-auto h-12 w-12 text-green-500"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"
              />
            </svg>
            <p className="mt-4 text-lg font-medium text-gray-900">
              No conflicts detected
            </p>
            <p className="mt-2 text-sm text-gray-500">
              All patient data is consistent and verified.
            </p>
          </div>
        ) : (
          conflicts.map((conflict) => (
            <div
              key={conflict.id}
              className="px-6 py-4 hover:bg-gray-50 cursor-pointer transition-colors"
              onClick={() => handleConflictClick(conflict)}
            >
              <div className="flex items-start justify-between gap-4">
                <div className="flex items-start gap-3 flex-1">
                  <SeverityBadge severity={conflict.severity} />
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2 mb-1">
                      <span
                        className={`px-2 py-0.5 rounded text-xs font-medium ${getEntityBadgeColor(
                          conflict.entityType,
                        )}`}
                      >
                        {conflict.entityType}
                      </span>
                      <span className="text-xs text-gray-500">
                        {formatTimestamp(conflict.createdAt)}
                      </span>
                    </div>
                    <p className="text-sm text-gray-900 line-clamp-2">
                      {conflict.description}
                    </p>
                  </div>
                </div>
                <button
                  onClick={(e) => {
                    e.stopPropagation();
                    handleConflictClick(conflict);
                  }}
                  className="px-3 py-1 bg-blue-600 text-white rounded-md text-sm font-medium hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-colors flex-shrink-0"
                >
                  Resolve
                </button>
              </div>
            </div>
          ))
        )}
      </div>

      {/* Pagination */}
      {conflicts.length > 0 && (
        <div className="border-t border-gray-200 px-6 py-4 flex items-center justify-between">
          <button
            onClick={() => handlePageChange(currentPage - 1)}
            disabled={currentPage === 1}
            className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            Previous
          </button>
          <span className="text-sm text-gray-700">Page {currentPage}</span>
          <button
            onClick={() => handlePageChange(currentPage + 1)}
            disabled={conflicts.length < pageSize}
            className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            Next
          </button>
        </div>
      )}
    </div>
  );
};
