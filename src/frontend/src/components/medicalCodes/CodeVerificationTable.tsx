/**
 * CodeVerificationTable Component (EP-008-US-052, AC1)
 * Table displaying AI-suggested medical codes with verification actions
 */

import React, { useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import {
  acceptCode,
  selectCode,
  toggleCodeSelection,
} from '../../store/slices/medicalCodeVerificationSlice';
import { VerificationBadge } from './VerificationBadge';
import { ConfidenceBar } from './ConfidenceBar';
import { ModifyCodeModal } from './ModifyCodeModal';
import { RejectCodeModal } from './RejectCodeModal';
import type { MedicalCodeSuggestion } from '../../types/medicalCode.types';
import type { RootState, AppDispatch } from '../../store';
import { toast } from '../../utils/toast';

export const CodeVerificationTable: React.FC = () => {
  const dispatch = useDispatch<AppDispatch>();
  const suggestions = useSelector(
    (state: RootState) => state.medicalCodeVerification.suggestions
  );
  const selectedCodes = useSelector(
    (state: RootState) => state.medicalCodeVerification.selectedCodes
  );
  const selectedCodeId = useSelector(
    (state: RootState) => state.medicalCodeVerification.selectedCodeId
  );

  const [modifyModalCode, setModifyModalCode] =
    useState<MedicalCodeSuggestion | null>(null);
  const [rejectModalCode, setRejectModalCode] =
    useState<MedicalCodeSuggestion | null>(null);

  const handleAccept = async (code: MedicalCodeSuggestion) => {
    try {
      await dispatch(acceptCode(code.id)).unwrap();
      toast.success('Code accepted', { autoClose: 200 }); // UXR-501: 200ms feedback
    } catch (error) {
          const errorMessage = error instanceof Error ? error.message : 'Failed to accept codes';
          toast.error(errorMessage);
        }
  };

  const handleModify = (code: MedicalCodeSuggestion) => {
    setModifyModalCode(code);
  };

  const handleReject = (code: MedicalCodeSuggestion) => {
    setRejectModalCode(code);
  };

  const handleRowClick = (code: MedicalCodeSuggestion) => {
    dispatch(selectCode(code.id));
  };

  const handleCheckboxChange = (codeId: string) => {
    dispatch(toggleCodeSelection(codeId));
  };

  if (suggestions.length === 0) {
    return (
      <div
        className="text-center py-12 bg-gray-50 rounded-lg border border-gray-200"
        role="status"
        aria-live="polite"
      >
        <div className="text-gray-400 text-5xl mb-4">
          <span aria-hidden="true">📋</span>
        </div>
        <p className="text-gray-600 font-medium mb-2">
          No pending verifications
        </p>
        <p className="text-gray-500 text-sm">
          All codes have been verified or there are no suggestions yet.
        </p>
      </div>
    );
  }

  return (
    <>
      <div className="bg-white border border-gray-200 rounded-lg shadow-sm overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full" aria-label="Medical code suggestions">
            <thead>
              <tr className="bg-gray-50 border-b border-gray-200">
                <th className="px-4 py-3 text-left">
                  <span className="sr-only">Select for bulk operations</span>
                </th>
                <th className="px-4 py-3 text-left text-sm font-semibold text-gray-700">
                  Code System
                </th>
                <th className="px-4 py-3 text-left text-sm font-semibold text-gray-700">
                  Code
                </th>
                <th className="px-4 py-3 text-left text-sm font-semibold text-gray-700">
                  Description
                </th>
                <th className="px-4 py-3 text-left text-sm font-semibold text-gray-700">
                  Confidence
                </th>
                <th className="px-4 py-3 text-left text-sm font-semibold text-gray-700">
                  Status
                </th>
                <th className="px-4 py-3 text-left text-sm font-semibold text-gray-700">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody>
              {suggestions.map((code) => (
                <tr
                  key={code.id}
                  onClick={() => handleRowClick(code)}
                  className={`border-b border-gray-100 hover:bg-gray-50 cursor-pointer transition-colors ${
                    selectedCodeId === code.id
                      ? 'bg-blue-50 border-l-4 border-l-blue-500'
                      : ''
                  }`}
                  role="row"
                  aria-selected={selectedCodeId === code.id}
                >
                  <td className="px-4 py-3">
                    {code.verificationStatus === 'Pending' &&
                      code.confidenceScore > 95 && (
                        <input
                          type="checkbox"
                          checked={selectedCodes.includes(code.id)}
                          onChange={(e) => {
                            e.stopPropagation();
                            handleCheckboxChange(code.id);
                          }}
                          onClick={(e) => e.stopPropagation()}
                          className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                          aria-label={`Select ${code.code}`}
                        />
                      )}
                  </td>
                  <td className="px-4 py-3">
                    <span className="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-gray-100 text-gray-800">
                      {code.codeSystem}
                    </span>
                  </td>
                  <td className="px-4 py-3">
                    <span className="font-mono font-semibold text-gray-900">
                      {code.code}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-sm text-gray-600">
                    {code.description}
                  </td>
                  <td className="px-4 py-3">
                    <ConfidenceBar score={code.confidenceScore} />
                  </td>
                  <td className="px-4 py-3">
                    <VerificationBadge status={code.verificationStatus} />
                  </td>
                  <td className="px-4 py-3">
                    {code.verificationStatus === 'Pending' ? (
                      <div
                        className="flex gap-2"
                        onClick={(e) => e.stopPropagation()}
                      >
                        <button
                          onClick={() => handleAccept(code)}
                          className="px-3 py-1 bg-green-600 text-white text-sm rounded hover:bg-green-700 transition-colors"
                          aria-label={`Accept ${code.code}`}
                        >
                          ✓ Accept
                        </button>
                        <button
                          onClick={() => handleModify(code)}
                          className="px-3 py-1 border border-gray-300 text-gray-700 text-sm rounded hover:bg-gray-50 transition-colors"
                          aria-label={`Modify ${code.code}`}
                        >
                          ✎ Modify
                        </button>
                        <button
                          onClick={() => handleReject(code)}
                          className="px-3 py-1 bg-red-600 text-white text-sm rounded hover:bg-red-700 transition-colors"
                          aria-label={`Reject ${code.code}`}
                        >
                          ✗ Reject
                        </button>
                      </div>
                    ) : (
                      <span className="text-gray-400 text-sm">—</span>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* Modals */}
      {modifyModalCode && (
        <ModifyCodeModal
          code={modifyModalCode}
          isOpen={!!modifyModalCode}
          onClose={() => setModifyModalCode(null)}
        />
      )}

      {rejectModalCode && (
        <RejectCodeModal
          code={rejectModalCode}
          isOpen={!!rejectModalCode}
          onClose={() => setRejectModalCode(null)}
        />
      )}
    </>
  );
};
