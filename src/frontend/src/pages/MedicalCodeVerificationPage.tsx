/**
 * Medical Code Verification Page
 * Main page for staff to verify AI-suggested medical codes (SCR-023)
 * Implements AC1-AC4 with side panel for code details
 */

import React, { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useParams } from 'react-router-dom';
import { fetchSuggestions, selectCode } from '../store/slices/medicalCodeVerificationSlice';
import { BulkVerificationControls } from '../components/medicalCodes/BulkVerificationControls';
import { CodeVerificationTable } from '../components/medicalCodes/CodeVerificationTable';
import { VerificationBadge } from '../components/medicalCodes/VerificationBadge';
import { ConfidenceBar } from '../components/medicalCodes/ConfidenceBar';
import type { RootState, AppDispatch } from '../store/index';

export const MedicalCodeVerificationPage: React.FC = () => {
  const dispatch = useDispatch<AppDispatch>();
  const { extractedDataId } = useParams<{ extractedDataId: string }>();
  const { suggestions, selectedCodeId, isLoading, error } = useSelector(
    (state: RootState) => state.medicalCodeVerification
  );

  // Find the selected code for side panel display
  const selectedCode = suggestions.find((s) => s.id === selectedCodeId);

  // Calculate statistics
  const pendingCount = suggestions.filter(
    (s) => s.verificationStatus === 'AISuggested'
  ).length;
  const verifiedCount = suggestions.filter(
    (s) => s.verificationStatus === 'Accepted' || s.verificationStatus === 'Modified'
  ).length;
  const rejectedCount = suggestions.filter(
    (s) => s.verificationStatus === 'Rejected'
  ).length;

  // Fetch suggestions on mount
  useEffect(() => {
    if (extractedDataId) {
      dispatch(fetchSuggestions(extractedDataId));
    }
  }, [dispatch, extractedDataId]);

  // Auto-select first code if none selected
  useEffect(() => {
    if (suggestions.length > 0 && !selectedCodeId && suggestions[0]) {
      dispatch(selectCode(suggestions[0].id));
    }
  }, [suggestions, selectedCodeId, dispatch]);

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="flex">
        {/* Main Content Area */}
        <div className="flex-1 p-8">
          {/* Page Header */}
          <div className="mb-6">
            <h1 className="text-3xl font-semibold text-gray-900 mb-2">
              Medical Code Verification
            </h1>
            <p className="text-sm text-gray-500">
              Review and verify AI-suggested ICD-10 and CPT codes
            </p>
          </div>

          {/* Statistics Row */}
          <div className="grid grid-cols-3 gap-4 mb-6">
            <div className="bg-white rounded-lg border border-gray-200 p-4 text-center">
              <div className="text-3xl font-semibold text-amber-600">
                {pendingCount}
              </div>
              <div className="text-sm text-gray-500 mt-1">Pending</div>
            </div>
            <div className="bg-white rounded-lg border border-gray-200 p-4 text-center">
              <div className="text-3xl font-semibold text-green-600">
                {verifiedCount}
              </div>
              <div className="text-sm text-gray-500 mt-1">Verified</div>
            </div>
            <div className="bg-white rounded-lg border border-gray-200 p-4 text-center">
              <div className="text-3xl font-semibold text-red-600">
                {rejectedCount}
              </div>
              <div className="text-sm text-gray-500 mt-1">Rejected</div>
            </div>
          </div>

          {/* Loading State (UXR-301) */}
          {isLoading && (
            <div className="bg-white rounded-lg border border-gray-200 p-12 text-center">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500 mx-auto mb-4"></div>
              <p className="text-gray-500">Loading code suggestions...</p>
            </div>
          )}

          {/* Error State */}
          {error && (
            <div className="bg-red-50 border border-red-200 rounded-lg p-4 mb-6">
              <p className="text-red-800 font-semibold">Error</p>
              <p className="text-red-700 text-sm">{error}</p>
            </div>
          )}

          {/* Empty State (UXR-605) */}
          {!isLoading && !error && suggestions.length === 0 && (
            <div className="bg-white rounded-lg border border-gray-200 p-12 text-center">
              <div className="text-6xl mb-4">📋</div>
              <h3 className="text-xl font-semibold text-gray-700 mb-2">
                No Pending Verifications
              </h3>
              <p className="text-gray-500">
                All medical codes have been verified or there are no suggestions
                available.
              </p>
            </div>
          )}

          {/* Main Content */}
          {!isLoading && !error && suggestions.length > 0 && (
            <>
              {/* Bulk Verification Controls */}
              <BulkVerificationControls />

              {/* Information Notice */}
              <div className="mb-4 p-4 bg-blue-50 border border-blue-200 rounded-lg">
                <p className="text-sm text-blue-800">
                  <strong>ℹ️ Note:</strong> Medical codes are automatically
                  suggested by AI based on extracted clinical data. All codes
                  must be verified by staff before being used in billing or
                  clinical records.
                </p>
              </div>

              {/* Code Verification Table */}
              <CodeVerificationTable />
            </>
          )}
        </div>

        {/* Side Panel for Code Details */}
        {selectedCode && (
          <div className="w-96 bg-white border-l border-gray-200 p-6 overflow-y-auto">
            <h3 className="text-lg font-semibold mb-4">Code Details</h3>

            {/* Code Information */}
            <div className="space-y-3 mb-6">
              <div className="flex justify-between items-center">
                <span className="text-sm text-gray-500">Code System</span>
                <span className="inline-block bg-gray-50 border border-gray-200 px-2 py-1 rounded text-xs font-semibold text-gray-700">
                  {selectedCode.codeSystem}
                </span>
              </div>
              <div className="flex justify-between items-start">
                <span className="text-sm text-gray-500">Code</span>
                <span className="font-mono font-semibold text-gray-900">
                  {selectedCode.code}
                </span>
              </div>
              <div className="flex justify-between items-start">
                <span className="text-sm text-gray-500">Description</span>
                <span className="text-sm text-gray-900 text-right max-w-xs">
                  {selectedCode.description}
                </span>
              </div>
              <div className="flex justify-between items-center">
                <span className="text-sm text-gray-500">Confidence</span>
                <ConfidenceBar score={selectedCode.confidenceScore} />
              </div>
              <div className="flex justify-between items-center">
                <span className="text-sm text-gray-500">Status</span>
                <VerificationBadge
                  status={selectedCode.verificationStatus}
                  size="sm"
                />
              </div>
              {selectedCode.verifiedAt && (
                <div className="flex justify-between items-start">
                  <span className="text-sm text-gray-500">Verified At</span>
                  <span className="text-sm text-gray-900 text-right">
                    {new Date(selectedCode.verifiedAt).toLocaleString()}
                  </span>
                </div>
              )}
            </div>

            {/* Source Clinical Data */}
            <div className="mb-6">
              <h4 className="text-sm font-semibold mb-3">
                Source Clinical Data
              </h4>
              <div className="p-3 bg-gray-50 border border-gray-200 rounded text-sm text-gray-700">
                {selectedCode.sourceClinicalText}
              </div>
            </div>

            {/* AI Rationale */}
            {selectedCode.rationale && (
              <div className="mb-6 p-3 bg-amber-50 border border-amber-200 rounded">
                <h4 className="text-sm font-semibold text-amber-900 mb-2">
                  AI Rationale
                </h4>
                <p className="text-sm text-amber-800">
                  {selectedCode.rationale}
                </p>
              </div>
            )}

            {/* RAG Context (if available) */}
            {selectedCode.retrievedContext && (
              <div className="mb-6">
                <h4 className="text-sm font-semibold mb-3">
                  Retrieved Context
                </h4>
                <div className="p-3 bg-gray-50 border border-gray-200 rounded text-xs text-gray-600 max-h-40 overflow-y-auto">
                  {selectedCode.retrievedContext}
                </div>
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
};
