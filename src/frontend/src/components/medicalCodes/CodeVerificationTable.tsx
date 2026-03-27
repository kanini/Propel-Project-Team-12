/**
 * Code Verification Table Component
 * Displays medical code suggestions with verification actions (AC1)
 * Supports Accept, Modify, and Reject workflows (AC2, AC3, AC4)
 */

import React, { useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { toast } from 'react-toastify';
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
import type { RootState, AppDispatch } from '../../store/index';

export const CodeVerificationTable: React.FC = () => {
  const dispatch = useDispatch<AppDispatch>();
  const { suggestions, selectedCodes, selectedCodeId } = useSelector(
    (state: RootState) => state.medicalCodeVerification
  );

  const [modifyModalOpen, setModifyModalOpen] = useState(false);
  const [rejectModalOpen, setRejectModalOpen] = useState(false);
  const [activeCode, setActiveCode] = useState<MedicalCodeSuggestion | null>(
    null
  );

  // Handle Accept action (AC2)
  const handleAccept = async (code: MedicalCodeSuggestion) => {
    try {
      await dispatch(acceptCode({ codeId: code.id })).unwrap();
      toast.success('Code accepted', { autoClose: 200 }); // UXR-501: 200ms feedback
    } catch (error: any) {
      toast.error(error || 'Failed to accept code');
    }
  };

  // Handle Modify action (AC3)
  const handleModify = (code: MedicalCodeSuggestion) => {
    setActiveCode(code);
    setModifyModalOpen(true);
  };

  // Handle Reject action (AC4)
  const handleReject = (code: MedicalCodeSuggestion) => {
    setActiveCode(code);
    setRejectModalOpen(true);
  };

  // Handle row click for side panel
  const handleRowClick = (code: MedicalCodeSuggestion) => {
    dispatch(selectCode(code.id));
  };

  // Handle checkbox toggle for bulk operations
  const handleCheckboxToggle = (codeId: string) => {
    dispatch(toggleCodeSelection(codeId));
  };

  return (
    <>
      <div className="bg-white border border-gray-200 rounded-lg shadow-sm overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full border-collapse">
            <thead>
              <tr className="bg-gray-50 border-b border-gray-200">
                <th className="text-left font-semibold text-gray-700 px-3 py-3 text-sm w-12">
                  {/* Checkbox column */}
                </th>
                <th className="text-left font-semibold text-gray-700 px-3 py-3 text-sm w-24">
                  System
                </th>
                <th className="text-left font-semibold text-gray-700 px-3 py-3 text-sm w-28">
                  Code
                </th>
                <th className="text-left font-semibold text-gray-700 px-3 py-3 text-sm">
                  Description
                </th>
                <th className="text-left font-semibold text-gray-700 px-3 py-3 text-sm w-32">
                  Confidence
                </th>
                <th className="text-left font-semibold text-gray-700 px-3 py-3 text-sm w-40">
                  Status
                </th>
                <th className="text-left font-semibold text-gray-700 px-3 py-3 text-sm w-80">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody>
              {suggestions.length === 0 ? (
                <tr>
                  <td colSpan={7} className="text-center py-12 text-gray-500">
                    No medical code suggestions available
                  </td>
                </tr>
              ) : (
                suggestions.map((code) => (
                  <tr
                    key={code.id}
                    onClick={() => handleRowClick(code)}
                    className={`border-b border-gray-100 hover:bg-gray-50 cursor-pointer transition-colors ${
                      selectedCodeId === code.id
                        ? 'bg-blue-50 border-l-4 border-l-blue-500'
                        : ''
                    }`}
                  >
                    {/* Checkbox for bulk selection */}
                    <td
                      className="px-3 py-3 align-middle"
                      onClick={(e) => e.stopPropagation()}
                    >
                      {code.verificationStatus === 'AISuggested' &&
                        code.confidenceScore > 95 && (
                          <input
                            type="checkbox"
                            checked={selectedCodes.includes(code.id)}
                            onChange={() => handleCheckboxToggle(code.id)}
                            className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                          />
                        )}
                    </td>

                    {/* Code System */}
                    <td className="px-3 py-3 align-middle">
                      <span className="inline-block bg-gray-50 border border-gray-200 px-2 py-1 rounded text-xs font-semibold text-gray-700">
                        {code.codeSystem}
                      </span>
                    </td>

                    {/* Code */}
                    <td className="px-3 py-3 align-middle">
                      <span className="font-mono font-semibold text-gray-900">
                        {code.code}
                      </span>
                    </td>

                    {/* Description */}
                    <td className="px-3 py-3 align-middle text-sm text-gray-700">
                      {code.description}
                    </td>

                    {/* Confidence */}
                    <td className="px-3 py-3 align-middle">
                      <ConfidenceBar score={code.confidenceScore} />
                    </td>

                    {/* Status Badge */}
                    <td className="px-3 py-3 align-middle">
                      <VerificationBadge status={code.verificationStatus} />
                    </td>

                    {/* Action Buttons */}
                    <td
                      className="px-3 py-3 align-middle"
                      onClick={(e) => e.stopPropagation()}
                    >
                      {code.verificationStatus === 'AISuggested' ? (
                        <div className="flex gap-2">
                          <button
                            onClick={() => handleAccept(code)}
                            className="px-3 py-1 bg-green-600 text-white text-sm rounded hover:bg-green-700 transition-colors"
                          >
                            ✓ Accept
                          </button>
                          <button
                            onClick={() => handleModify(code)}
                            className="px-3 py-1 bg-blue-500 text-white text-sm rounded hover:bg-blue-600 transition-colors"
                          >
                            ✎ Modify
                          </button>
                          <button
                            onClick={() => handleReject(code)}
                            className="px-3 py-1 bg-red-600 text-white text-sm rounded hover:bg-red-700 transition-colors"
                          >
                            ✗ Reject
                          </button>
                        </div>
                      ) : (
                        <span className="text-sm text-gray-400">
                          {code.verificationStatus === 'Accepted' || code.verificationStatus === 'Modified'
                            ? 'Verified'
                            : 'Rejected'}
                        </span>
                      )}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Modals */}
      {activeCode && (
        <>
          <ModifyCodeModal
            code={activeCode}
            isOpen={modifyModalOpen}
            onClose={() => {
              setModifyModalOpen(false);
              setActiveCode(null);
            }}
          />
          <RejectCodeModal
            code={activeCode}
            isOpen={rejectModalOpen}
            onClose={() => {
              setRejectModalOpen(false);
              setActiveCode(null);
            }}
          />
        </>
      )}
    </>
  );
};
