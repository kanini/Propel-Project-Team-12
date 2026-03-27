/**
 * RejectCodeModal Component (EP-008-US-052, AC4)
 * Modal for rejecting AI-suggested codes with required reason
 */

import React, { useState } from 'react';
import { useDispatch } from 'react-redux';
import { rejectCode } from '../../store/slices/medicalCodeVerificationSlice';
import type { MedicalCodeSuggestion } from '../../types/medicalCode.types';
import type { AppDispatch } from '../../store';
import { toast } from '../../utils/toast';

interface RejectCodeModalProps {
  code: MedicalCodeSuggestion;
  isOpen: boolean;
  onClose: () => void;
}

const REJECTION_REASONS = [
  'Incorrect diagnosis',
  'Incorrect procedure code',
  'Insufficient documentation',
  'Code not supported by clinical evidence',
  'Wrong code system (ICD-10 vs CPT)',
  'Other (specify below)',
];

export const RejectCodeModal: React.FC<RejectCodeModalProps> = ({
  code,
  isOpen,
  onClose,
}) => {
  const dispatch = useDispatch<AppDispatch>();
  const [reason, setReason] = useState('');
  const [customReason, setCustomReason] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleReject = async () => {
    if (!reason) {
      toast.error('Please select a rejection reason');
      return;
    }

    const finalReason =
      reason === 'Other (specify below)' ? customReason : reason;

    if (reason === 'Other (specify below)' && !customReason) {
      toast.error('Please specify rejection reason');
      return;
    }

    setIsSubmitting(true);
    try {
      await dispatch(
        rejectCode({
          codeId: code.id,
          reason: finalReason,
        })
      ).unwrap();

      toast.error('Code rejected', { autoClose: 200 }); // UXR-501: 200ms feedback
      onClose();
      setReason('');
      setCustomReason('');
    } catch (error) {
      toast.error('Failed to reject code');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div
      className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50"
      role="dialog"
      aria-modal="true"
      aria-labelledby="reject-modal-title"
    >
      <div className="bg-white rounded-lg shadow-xl w-full max-w-md p-6">
        <h3
          id="reject-modal-title"
          className="text-xl font-semibold mb-4 text-red-600"
        >
          Reject Code
        </h3>

        <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded">
          <span className="font-mono font-semibold">{code.code}</span>
          <span className="text-gray-600 ml-2">— {code.description}</span>
        </div>

        <div className="mb-4">
          <label
            htmlFor="rejection-reason"
            className="block text-sm font-medium text-gray-700 mb-2"
          >
            Rejection Reason <span className="text-red-600">*</span>
          </label>
          <select
            id="rejection-reason"
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-red-500 focus:border-transparent"
            required
            aria-required="true"
          >
            <option value="">Select a reason...</option>
            {REJECTION_REASONS.map((r) => (
              <option key={r} value={r}>
                {r}
              </option>
            ))}
          </select>
        </div>

        {reason === 'Other (specify below)' && (
          <div className="mb-4">
            <label
              htmlFor="custom-reason"
              className="block text-sm font-medium text-gray-700 mb-2"
            >
              Specify Reason
            </label>
            <textarea
              id="custom-reason"
              value={customReason}
              onChange={(e) => setCustomReason(e.target.value)}
              rows={3}
              placeholder="Please provide specific details..."
              className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-red-500 focus:border-transparent"
              required
              aria-required="true"
            />
          </div>
        )}

        <div className="bg-amber-50 border border-amber-200 rounded p-3 mb-6">
          <p className="text-sm text-amber-800">
            ⚠️ This code will be flagged for manual coding. The data point will
            require manual review before use.
          </p>
        </div>

        <div className="flex justify-end gap-3">
          <button
            onClick={onClose}
            className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
            disabled={isSubmitting}
          >
            Cancel
          </button>
          <button
            onClick={handleReject}
            disabled={
              !reason ||
              (reason === 'Other (specify below)' && !customReason) ||
              isSubmitting
            }
            className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            {isSubmitting ? 'Rejecting...' : 'Reject Code'}
          </button>
        </div>
      </div>
    </div>
  );
};
