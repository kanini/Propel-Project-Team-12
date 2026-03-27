/**
 * Bulk Verification Controls Component
 * Handles bulk selection and acceptance of high-confidence codes (>95%)
 * Edge case: Select All / Accept All functionality
 */

import React from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { toast } from 'react-toastify';
import {
  selectAllHighConfidence,
  clearSelection,
  bulkAcceptCodes,
} from '../../store/slices/medicalCodeVerificationSlice';
import type { RootState, AppDispatch } from '../../store/index';

export const BulkVerificationControls: React.FC = () => {
  const dispatch = useDispatch<AppDispatch>();
  const { suggestions, selectedCodes } = useSelector(
    (state: RootState) => state.medicalCodeVerification
  );

  // Filter high-confidence codes (>95%) that are pending
  const highConfidenceCodes = suggestions.filter(
    (s) => s.confidenceScore > 95 && s.verificationStatus === 'AISuggested'
  );

  const handleSelectAll = () => {
    dispatch(selectAllHighConfidence());
  };

  const handleAcceptAll = async () => {
    if (selectedCodes.length === 0) {
      toast.warning('No codes selected');
      return;
    }

    try {
      await dispatch(bulkAcceptCodes(selectedCodes)).unwrap();
      toast.success(`${selectedCodes.length} codes accepted`, {
        autoClose: 200,
      }); // UXR-501: 200ms feedback
    } catch (error: any) {
      toast.error(error || 'Failed to bulk accept codes');
    }
  };

  const handleClear = () => {
    dispatch(clearSelection());
  };

  // Don't show controls if no high-confidence codes available
  if (highConfidenceCodes.length === 0) {
    return null;
  }

  return (
    <div className="flex items-center gap-4 p-4 bg-blue-50 border border-blue-200 rounded-lg mb-4">
      <div className="flex-1">
        <span className="text-sm text-blue-800">
          <strong>{highConfidenceCodes.length}</strong> high-confidence codes
          {' (>95%) '} available for bulk verification
        </span>
      </div>
      <button
        onClick={handleSelectAll}
        className="px-4 py-2 border border-blue-300 text-blue-700 rounded-lg hover:bg-blue-100 transition-colors"
      >
        Select All High-Confidence
      </button>
      <button
        onClick={handleAcceptAll}
        disabled={selectedCodes.length === 0}
        className="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
      >
        Accept Selected ({selectedCodes.length})
      </button>
      <button
        onClick={handleClear}
        className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
      >
        Clear
      </button>
    </div>
  );
};
