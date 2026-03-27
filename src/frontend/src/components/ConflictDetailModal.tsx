/**
 * ConflictDetailModal Component (US_048, AC2)
 * Modal for viewing conflict details and resolution workflow.
 * Includes source references, entity comparison, and resolution controls.
 */

import { useState, useRef, useEffect } from "react";
import { useAppDispatch } from "../store/hooks";
import { resolveConflictThunk } from "../store/slices/conflictsSlice";
import type { DataConflict } from "../types/conflict.types";
import { SeverityBadge } from "./SeverityBadge";

interface ConflictDetailModalProps {
  conflict: DataConflict | null;
  isOpen: boolean;
  onClose: () => void;
}

export const ConflictDetailModal = ({
  conflict,
  isOpen,
  onClose,
}: ConflictDetailModalProps) => {
  const dispatch = useAppDispatch();
  const [resolution, setResolution] = useState("");
  const [isResolving, setIsResolving] = useState(false);
  const modalRef = useRef<HTMLDivElement>(null);
  const resolutionTextareaRef = useRef<HTMLTextAreaElement>(null);

  // Focus management
  useEffect(() => {
    if (isOpen && resolutionTextareaRef.current) {
      resolutionTextareaRef.current.focus();
    }
  }, [isOpen]);

  // ESC key to close
  useEffect(() => {
    const handleEsc = (e: KeyboardEvent) => {
      if (e.key === "Escape" && isOpen) {
        handleClose();
      }
    };
    window.addEventListener("keydown", handleEsc);
    return () => window.removeEventListener("keydown", handleEsc);
  }, [isOpen]);

  // Click outside to close
  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (modalRef.current && !modalRef.current.contains(e.target as Node)) {
        handleClose();
      }
    };
    if (isOpen) {
      document.addEventListener("mousedown", handleClickOutside);
    }
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, [isOpen]);

  const handleClose = () => {
    setResolution("");
    setIsResolving(false);
    onClose();
  };

  const handleResolve = async () => {
    if (!conflict || !resolution.trim()) return;

    setIsResolving(true);
    try {
      await dispatch(
        resolveConflictThunk({
          conflictId: conflict.id,
          request: { resolution },
        }),
      ).unwrap();
      handleClose();
    } catch (error) {
      console.error("Failed to resolve conflict:", error);
      setIsResolving(false);
    }
  };

  const handleDismiss = async () => {
    if (!conflict) return;

    setIsResolving(true);
    try {
      await dispatch(
        resolveConflictThunk({
          conflictId: conflict.id,
          request: { resolution: "Dismissed - No action required" },
        }),
      ).unwrap();
      handleClose();
    } catch (error) {
      console.error("Failed to dismiss conflict:", error);
      setIsResolving(false);
    }
  };

  if (!isOpen || !conflict) return null;

  return (
    <div
      className="fixed inset-0 z-50 overflow-y-auto"
      aria-labelledby="conflict-modal-title"
      role="dialog"
      aria-modal="true"
    >
      {/* Backdrop */}
      <div className="fixed inset-0 bg-black bg-opacity-40 transition-opacity" />

      {/* Modal */}
      <div className="flex min-h-screen items-center justify-center p-4">
        <div
          ref={modalRef}
          className="relative bg-white rounded-lg shadow-xl max-w-2xl w-full max-h-[90vh] overflow-y-auto"
        >
          {/* Header */}
          <div className="sticky top-0 bg-white border-b border-gray-200 px-6 py-4 flex items-center justify-between">
            <div className="flex items-center gap-3">
              <h2
                id="conflict-modal-title"
                className="text-xl font-semibold text-gray-900"
              >
                Conflict Details
              </h2>
              <SeverityBadge severity={conflict.severity} />
            </div>
            <button
              onClick={handleClose}
              className="text-gray-400 hover:text-gray-600 focus:outline-none focus:ring-2 focus:ring-blue-500 rounded"
              aria-label="Close modal"
            >
              <svg
                className="h-6 w-6"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M6 18L18 6M6 6l12 12"
                />
              </svg>
            </button>
          </div>

          {/* Body */}
          <div className="px-6 py-4 space-y-6">
            {/* Conflict Type */}
            <div>
              <h3 className="text-sm font-medium text-gray-700">
                Conflict Type
              </h3>
              <p className="mt-1 text-sm text-gray-900">
                {conflict.conflictType}
              </p>
            </div>

            {/* Entity Type */}
            <div>
              <h3 className="text-sm font-medium text-gray-700">Entity Type</h3>
              <p className="mt-1 text-sm text-gray-900">
                {conflict.entityType}
              </p>
            </div>

            {/* Description */}
            <div>
              <h3 className="text-sm font-medium text-gray-700">Description</h3>
              <div className="mt-2 bg-gray-50 rounded-md p-3 border border-gray-200">
                <p className="text-sm text-gray-900">{conflict.description}</p>
              </div>
            </div>

            {/* Source Documents */}
            <div>
              <h3 className="text-sm font-medium text-gray-700 mb-2">
                Source Documents ({conflict.sourceDataIds.length})
              </h3>
              <div className="space-y-2">
                {conflict.sourceDataIds.map((sourceId, index) => (
                  <div
                    key={sourceId}
                    className="flex items-center gap-2 text-sm text-gray-600 bg-gray-50 rounded px-3 py-2"
                  >
                    <span className="font-medium">Document {index + 1}:</span>
                    <span className="font-mono text-xs">
                      {sourceId.substring(0, 8)}...
                    </span>
                  </div>
                ))}
              </div>
            </div>

            {/* Resolution Notes */}
            <div>
              <label
                htmlFor="resolution-notes"
                className="block text-sm font-medium text-gray-700 mb-2"
              >
                Resolution Notes <span className="text-red-500">*</span>
              </label>
              <textarea
                ref={resolutionTextareaRef}
                id="resolution-notes"
                rows={4}
                value={resolution}
                onChange={(e) => setResolution(e.target.value)}
                placeholder="Enter resolution notes explaining your decision..."
                className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                required
              />
              <p className="mt-1 text-xs text-gray-500">
                Explain which source is correct and why, or describe the
                resolution action taken.
              </p>
            </div>

            {/* Timestamp */}
            <div className="pt-4 border-t border-gray-200">
              <p className="text-xs text-gray-500">
                Detected: {new Date(conflict.createdAt).toLocaleString()}
              </p>
            </div>
          </div>

          {/* Footer */}
          <div className="sticky bottom-0 bg-gray-50 border-t border-gray-200 px-6 py-4 flex items-center justify-between gap-3">
            <button
              onClick={handleDismiss}
              disabled={isResolving}
              className="px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              Dismiss Conflict
            </button>
            <div className="flex gap-3">
              <button
                onClick={handleClose}
                disabled={isResolving}
                className="px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              >
                Cancel
              </button>
              <button
                onClick={handleResolve}
                disabled={isResolving || !resolution.trim()}
                className="px-4 py-2 bg-blue-600 border border-transparent rounded-md shadow-sm text-sm font-medium text-white hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              >
                {isResolving ? (
                  <span className="flex items-center gap-2">
                    <svg
                      className="animate-spin h-4 w-4"
                      fill="none"
                      viewBox="0 0 24 24"
                    >
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
                        d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                      />
                    </svg>
                    Resolving...
                  </span>
                ) : (
                  "Resolve Conflict"
                )}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
