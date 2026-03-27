/**
 * BulkVerificationControls Component (EP-008-US-052, Edge Case 2)
 * Provides "Select All / Accept All" functionality for high-confidence codes (>95%)
 */

import React from 'react';
import { useDispatch, useSelector } from 'react-redux';
import {
  selectAllHighConfidence,
  clearSelection,
  bulkAcceptCodes,
  selectHighConfidenceCodes,
} from '../../store/slices/medicalCodeVerificationSlice';
import type { RootState, AppDispatch } from '../../store';
import { toast } from '../../utils/toast';

export const BulkVerificationControls: React.FC = () => {
  const dispatch = useDispatch<AppDispatch>();
  const highConfidenceCodes = useSelector(selectHighConfidenceCodes);
  const selectedCodes = useSelector(
    (state: RootState) => state.medicalCodeVerification.selectedCodes
  );

  const handleSelectAll = () => {
    dispatch(selectAllHighConfidence());
    toast.info(`Selected ${highConfidenceCodes.length} high-confidence codes`);
  };

  const handleAcceptAll = async () => {
    if (selectedCodes.length === 0) {
      toast.warning('No codes selected');
      return;
    }

    try {
      await dispatch(bulkAcceptCodes(selectedCodes)).unwrap();
      toast.success(`${selectedCodes.length} codes accepted`, {
        autoClose: 200, // UXR-501: 200ms feedback
      });
    } catch (error) {
      toast.error('Failed to accept codes');
    }
  };

  const handleClear = () => {
    dispatch(clearSelection());
  };

  if (highConfidenceCodes.length === 0) {
    return null;
  }

  return (
    <div
      className="flex items-center gap-4 p-4 bg-blue-50 border border-blue-200 rounded-lg mb-4"
      role="region"
      aria-label="Bulk verification controls"
    >
      <div className="flex-1">
        <span className="text-sm text-blue-800">
          <strong>{highConfidenceCodes.length}</strong> high-confidence codes
          (&gt;95%) available for bulk verification
        </span>
      </div>
      <button
        onClick={handleSelectAll}
        className="px-4 py-2 border border-blue-300 text-blue-700 rounded-lg hover:bg-blue-100 transition-colors"
        aria-label="Select all high-confidence codes"
      >
        Select All High-Confidence
      </button>
      <button
        onClick={handleAcceptAll}
        disabled={selectedCodes.length === 0}
        className="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
        aria-label={`Accept selected codes (${selectedCodes.length} selected)`}
      >
        Accept Selected ({selectedCodes.length})
      </button>
      <button
        onClick={handleClear}
        disabled={selectedCodes.length === 0}
        className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
        aria-label="Clear selection"
      >
        Clear
      </button>
    </div>
  );
};
