/**
 * VerificationTable component for SCR-023 - Clinical Verification
 * Displays clinical data items with verify/reject action buttons
 */

import { useState } from 'react';
import type { VerificationItemDto } from '../../types/clinicalData';
import { ConfidenceBadge } from '../health/ConfidenceBadge';

interface VerificationTableProps {
  items: VerificationItemDto[];
  actionInProgress: string | null;
  onVerify: (id: string) => void;
  onReject: (id: string, reason?: string) => void;
  searchTerm: string;
  statusFilter: string | null;
}

export function VerificationTable({
  items,
  actionInProgress,
  onVerify,
  onReject,
  searchTerm,
  statusFilter,
}: VerificationTableProps) {
  const [rejectId, setRejectId] = useState<string | null>(null);
  const [rejectReason, setRejectReason] = useState('');

  const filtered = items.filter((item) => {
    const matchesSearch =
      !searchTerm ||
      item.dataType.toLowerCase().includes(searchTerm.toLowerCase()) ||
      item.dataValue.toLowerCase().includes(searchTerm.toLowerCase());
    
    // Handle status filtering with case-insensitive comparison and normalization
    const itemStatus = (item.verificationStatus || '').trim();
    const filterStatus = (statusFilter || '').trim();
    
    // Normalize backend enum values to frontend expected values  
    const normalizedStatus = 
      itemStatus === 'StaffVerified' ? 'Verified' : itemStatus;
    
    const matchesStatus = !filterStatus || 
      normalizedStatus.toLowerCase() === filterStatus.toLowerCase();
    
    return matchesSearch && matchesStatus;
  });

  const handleRejectConfirm = (id: string) => {
    onReject(id, rejectReason || undefined);
    setRejectId(null);
    setRejectReason('');
  };

  if (filtered.length === 0) {
    return (
      <div className="text-center py-8">
        <p className="text-neutral-500 text-sm">No clinical data items match the current filter.</p>
      </div>
    );
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b border-neutral-200 bg-neutral-50">
            <th className="text-left py-3 px-4 font-medium text-neutral-600">Type</th>
            <th className="text-left py-3 px-4 font-medium text-neutral-600">Value</th>
            <th className="text-left py-3 px-4 font-medium text-neutral-600">Source</th>
            <th className="text-left py-3 px-4 font-medium text-neutral-600">Status</th>
            <th className="text-right py-3 px-4 font-medium text-neutral-600">Actions</th>
          </tr>
        </thead>
        <tbody>
          {filtered.map((item) => {
            const isActing = actionInProgress === item.extractedDataId;
            const isPending = item.verificationStatus === 'AISuggested';

            return (
              <tr key={item.extractedDataId} className="border-b border-neutral-100 hover:bg-neutral-50">
                <td className="py-3 px-4">
                  <span className="inline-flex items-center rounded-md bg-neutral-100 px-2 py-1 text-xs font-medium text-neutral-700">
                    {item.dataType}
                  </span>
                </td>
                <td className="py-3 px-4 text-neutral-800 max-w-xs truncate">{item.dataValue}</td>
                <td className="py-3 px-4 text-neutral-500 text-xs">
                  {item.sourceDocument && <span>{item.sourceDocument}</span>}
                  {item.sourcePageNumber && <span className="ml-1">(p.{item.sourcePageNumber})</span>}
                </td>
                <td className="py-3 px-4">
                  <ConfidenceBadge
                    confidenceScore={item.confidenceScore}
                    verificationStatus={item.verificationStatus}
                  />
                </td>
                <td className="py-3 px-4 text-right">
                  {isPending ? (
                    <div className="flex items-center justify-end gap-2">
                      {rejectId === item.extractedDataId ? (
                        <div className="flex items-center gap-2">
                          <input
                            type="text"
                            value={rejectReason}
                            onChange={(e) => setRejectReason(e.target.value)}
                            placeholder="Reason (optional)"
                            className="border border-neutral-300 rounded px-2 py-1 text-xs w-32"
                          />
                          <button
                            onClick={() => handleRejectConfirm(item.extractedDataId)}
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
                      ) : (
                        <>
                          <button
                            onClick={() => onVerify(item.extractedDataId)}
                            disabled={isActing}
                            className="px-3 py-1.5 text-xs bg-green-600 text-white rounded-md hover:bg-green-700 disabled:opacity-50 transition-colors"
                          >
                            {isActing ? '...' : 'Verify'}
                          </button>
                          <button
                            onClick={() => setRejectId(item.extractedDataId)}
                            disabled={isActing}
                            className="px-3 py-1.5 text-xs bg-red-100 text-red-700 rounded-md hover:bg-red-200 disabled:opacity-50 transition-colors"
                          >
                            Reject
                          </button>
                        </>
                      )}
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
