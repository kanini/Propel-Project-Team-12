/**
 * MedicalCodesTable component for SCR-023 - Clinical Verification
 * Displays medical codes (ICD-10/CPT) with accept/reject/modify actions
 */

import { useState } from 'react';
import type { VerificationMedicalCodeDto } from '../../types/clinicalData';
import { ConfidenceBadge } from '../health/ConfidenceBadge';

interface MedicalCodesTableProps {
  codes: VerificationMedicalCodeDto[];
  actionInProgress: string | null;
  onAccept: (codeId: string) => void;
  onReject: (codeId: string, reason?: string) => void;
  onModify: (codeId: string, codeValue: string, codeDescription: string) => void;
  searchTerm: string;
  statusFilter: string | null;
}

export function MedicalCodesTable({
  codes,
  actionInProgress,
  onAccept,
  onReject,
  onModify,
  searchTerm,
  statusFilter,
}: MedicalCodesTableProps) {
  const [editId, setEditId] = useState<string | null>(null);
  const [editCode, setEditCode] = useState('');
  const [editDesc, setEditDesc] = useState('');
  const [rejectId, setRejectId] = useState<string | null>(null);
  const [rejectReason, setRejectReason] = useState('');

  const filtered = codes.filter((code) => {
    const matchesSearch =
      !searchTerm ||
      code.codeValue.toLowerCase().includes(searchTerm.toLowerCase()) ||
      code.codeDescription.toLowerCase().includes(searchTerm.toLowerCase()) ||
      code.codeSystem.toLowerCase().includes(searchTerm.toLowerCase());
    
    // Handle status filtering with case-insensitive comparison and normalization
    const codeStatus = (code.verificationStatus || '').trim();
    const filterStatus = (statusFilter || '').trim();
    
    // Normalize backend enum values to frontend expected values
    const normalizedStatus = 
      (codeStatus === 'Accepted' || codeStatus === 'Modified' || codeStatus === 'StaffVerified') 
        ? 'Verified' 
        : codeStatus;
    
    const matchesStatus = !filterStatus || 
      normalizedStatus.toLowerCase() === filterStatus.toLowerCase();
    
    // Debug: Log filter status values to help diagnose the issue  
    if (filterStatus === 'Verified' && codes.indexOf(code) < 5) {
      console.log('Medical Code:', code.codeValue, 'Raw Status:', code.verificationStatus, 
        'Normalized:', normalizedStatus, 'Matches:', matchesStatus);
    }
    
    return matchesSearch && matchesStatus;
  });

  const startEdit = (code: VerificationMedicalCodeDto) => {
    setEditId(code.medicalCodeId);
    setEditCode(code.codeValue);
    setEditDesc(code.codeDescription);
  };

  const handleModifyConfirm = (codeId: string) => {
    onModify(codeId, editCode, editDesc);
    setEditId(null);
  };

  const handleRejectConfirm = (codeId: string) => {
    onReject(codeId, rejectReason || undefined);
    setRejectId(null);
    setRejectReason('');
  };

  if (filtered.length === 0) {
    return (
      <div className="text-center py-8">
        <p className="text-neutral-500 text-sm">No medical codes match the current filter.</p>
      </div>
    );
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b border-neutral-200 bg-neutral-50">
            <th className="text-left py-3 px-4 font-medium text-neutral-600">System</th>
            <th className="text-left py-3 px-4 font-medium text-neutral-600">Code</th>
            <th className="text-left py-3 px-4 font-medium text-neutral-600">Description</th>
            <th className="text-left py-3 px-4 font-medium text-neutral-600">Status</th>
            <th className="text-left py-3 px-4 font-medium text-neutral-600">Verified By</th>
            <th className="text-right py-3 px-4 font-medium text-neutral-600">Actions</th>
          </tr>
        </thead>
        <tbody>
          {filtered.map((code) => {
            const isActing = actionInProgress === code.medicalCodeId;
            const isPending = code.verificationStatus === 'AISuggested';
            const isEditing = editId === code.medicalCodeId;
            const isRejecting = rejectId === code.medicalCodeId;

            return (
              <tr key={code.medicalCodeId} className="border-b border-neutral-100 hover:bg-neutral-50">
                <td className="py-3 px-4">
                  <span
                    className={`inline-flex items-center rounded-md px-2 py-0.5 text-xs font-medium ring-1 ring-inset ${
                      code.codeSystem === 'ICD10'
                        ? 'bg-blue-50 text-blue-700 ring-blue-200'
                        : 'bg-purple-50 text-purple-700 ring-purple-200'
                    }`}
                  >
                    {code.codeSystem}
                  </span>
                </td>
                <td className="py-3 px-4 font-mono font-medium text-neutral-800">
                  {isEditing ? (
                    <input
                      type="text"
                      value={editCode}
                      onChange={(e) => setEditCode(e.target.value)}
                      className="border border-blue-300 rounded px-2 py-1 text-xs w-24 font-mono"
                    />
                  ) : (
                    code.codeValue
                  )}
                </td>
                <td className="py-3 px-4 text-neutral-700 max-w-xs">
                  {isEditing ? (
                    <input
                      type="text"
                      value={editDesc}
                      onChange={(e) => setEditDesc(e.target.value)}
                      className="border border-blue-300 rounded px-2 py-1 text-xs w-full"
                    />
                  ) : (
                    <span className="truncate block">{code.codeDescription}</span>
                  )}
                </td>
                <td className="py-3 px-4">
                  <ConfidenceBadge
                    confidenceScore={code.confidenceScore}
                    verificationStatus={code.verificationStatus}
                  />
                </td>
                <td className="py-3 px-4 text-xs text-neutral-500">
                  {code.verifiedByName ?? '—'}
                </td>
                <td className="py-3 px-4 text-right">
                  {isEditing ? (
                    <div className="flex items-center justify-end gap-2">
                      <button
                        onClick={() => handleModifyConfirm(code.medicalCodeId)}
                        disabled={isActing}
                        className="px-2 py-1 text-xs bg-blue-600 text-white rounded hover:bg-blue-700 disabled:opacity-50"
                      >
                        Save
                      </button>
                      <button
                        onClick={() => setEditId(null)}
                        className="px-2 py-1 text-xs bg-neutral-200 text-neutral-700 rounded hover:bg-neutral-300"
                      >
                        Cancel
                      </button>
                    </div>
                  ) : isRejecting ? (
                    <div className="flex items-center justify-end gap-2">
                      <input
                        type="text"
                        value={rejectReason}
                        onChange={(e) => setRejectReason(e.target.value)}
                        placeholder="Reason (optional)"
                        className="border border-neutral-300 rounded px-2 py-1 text-xs w-28"
                      />
                      <button
                        onClick={() => handleRejectConfirm(code.medicalCodeId)}
                        disabled={isActing}
                        className="px-2 py-1 text-xs bg-red-600 text-white rounded hover:bg-red-700 disabled:opacity-50"
                      >
                        Confirm
                      </button>
                      <button
                        onClick={() => { setRejectId(null); setRejectReason(''); }}
                        className="px-2 py-1 text-xs bg-neutral-200 text-neutral-700 rounded hover:bg-neutral-300"
                      >
                        Cancel
                      </button>
                    </div>
                  ) : isPending ? (
                    <div className="flex items-center justify-end gap-1.5">
                      <button
                        onClick={() => onAccept(code.medicalCodeId)}
                        disabled={isActing}
                        className="px-2.5 py-1.5 text-xs bg-green-600 text-white rounded-md hover:bg-green-700 disabled:opacity-50 transition-colors"
                      >
                        Accept
                      </button>
                      <button
                        onClick={() => setRejectId(code.medicalCodeId)}
                        disabled={isActing}
                        className="px-2.5 py-1.5 text-xs bg-red-100 text-red-700 rounded-md hover:bg-red-200 disabled:opacity-50 transition-colors"
                      >
                        Reject
                      </button>
                      <button
                        onClick={() => startEdit(code)}
                        disabled={isActing}
                        className="px-2.5 py-1.5 text-xs bg-blue-100 text-blue-700 rounded-md hover:bg-blue-200 disabled:opacity-50 transition-colors"
                      >
                        Modify
                      </button>
                    </div>
                  ) : (
                    <span className="text-xs text-neutral-400">—</span>
                  )}
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}
