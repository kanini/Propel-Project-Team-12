/**
 * Medical Code Verification Page (EP-008-US-052, SCR-023)
 * Main page for staff to verify AI-suggested medical codes
 */

import React, { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useSearchParams } from 'react-router-dom';
import {
  fetchSuggestions,
  selectSelectedCode,
} from '../store/slices/medicalCodeVerificationSlice';
import { BulkVerificationControls } from '../components/medicalCodes/BulkVerificationControls';
import { CodeVerificationTable } from '../components/medicalCodes/CodeVerificationTable';
import type { RootState, AppDispatch } from '../store';

export const MedicalCodeVerificationPage: React.FC = () => {
  const dispatch = useDispatch<AppDispatch>();
  const [searchParams] = useSearchParams();
  const extractedDataId = searchParams.get('extractedDataId');

  const isLoading = useSelector(
    (state: RootState) => state.medicalCodeVerification.isLoading
  );
  const error = useSelector(
    (state: RootState) => state.medicalCodeVerification.error
  );
  const suggestions = useSelector(
    (state: RootState) => state.medicalCodeVerification.suggestions
  );
  const selectedCode = useSelector(selectSelectedCode);

  useEffect(() => {
    if (extractedDataId) {
      dispatch(fetchSuggestions(extractedDataId));
    }
  }, [dispatch, extractedDataId]);

  // Calculate statistics
  const pendingCount = suggestions.filter(
    (s) => s.verificationStatus === 'Pending'
  ).length;
  const verifiedCount = suggestions.filter(
    (s) => s.verificationStatus === 'StaffVerified'
  ).length;
  const rejectedCount = suggestions.filter(
    (s) => s.verificationStatus === 'StaffRejected'
  ).length;

  // Loading state (UXR-301: 4-8s loading with skeleton)
  if (isLoading) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="animate-pulse">
          <div className="h-8 bg-gray-200 rounded w-1/4 mb-6"></div>
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
            {[1, 2, 3, 4].map((i) => (
              <div
                key={i}
                className="bg-white border border-gray-200 rounded-lg p-6"
              >
                <div className="h-8 bg-gray-200 rounded mb-2"></div>
                <div className="h-4 bg-gray-200 rounded w-1/2"></div>
              </div>
            ))}
          </div>
          <div className="h-64 bg-gray-200 rounded"></div>
        </div>
      </div>
    );
  }

  // Error state (UXR-603: Global error banner)
  if (error) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div
          className="bg-red-50 border border-red-200 rounded-lg p-6"
          role="alert"
          aria-live="assertive"
        >
          <div className="flex items-center gap-3">
            <span className="text-red-600 text-2xl" aria-hidden="true">
              ⚠️
            </span>
            <div className="flex-1">
              <h3 className="text-lg font-semibold text-red-900 mb-1">
                Error Loading Suggestions
              </h3>
              <p className="text-red-700">{error}</p>
            </div>
            <button
              onClick={() => extractedDataId && dispatch(fetchSuggestions(extractedDataId))}
              className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors"
            >
              Retry
            </button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8">
      {/* Page Header */}
      <div className="flex justify-between items-center mb-6">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 mb-2">
            Clinical Code Verification
          </h1>
          <p className="text-gray-600">
            Review and verify AI-suggested medical codes
          </p>
        </div>
        {extractedDataId && (
          <span className="text-sm text-gray-500">
            Patient Data ID: <strong>{extractedDataId}</strong>
          </span>
        )}
      </div>

      {/* Statistics Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
        <div className="bg-white border border-gray-200 rounded-lg p-6">
          <div className="text-3xl font-bold text-amber-600 mb-1">
            {pendingCount}
          </div>
          <div className="text-sm text-gray-600">Pending Verification</div>
        </div>
        <div className="bg-white border border-gray-200 rounded-lg p-6">
          <div className="text-3xl font-bold text-green-600 mb-1">
            {verifiedCount}
          </div>
          <div className="text-sm text-gray-600">Staff-Verified</div>
        </div>
        <div className="bg-white border border-gray-200 rounded-lg p-6">
          <div className="text-3xl font-bold text-red-600 mb-1">
            {rejectedCount}
          </div>
          <div className="text-sm text-gray-600">Rejected</div>
        </div>
        <div className="bg-white border border-gray-200 rounded-lg p-6">
          <div className="text-3xl font-bold text-blue-600 mb-1">
            {suggestions.length}
          </div>
          <div className="text-sm text-gray-600">Total Suggestions</div>
        </div>
      </div>

      {/* Bulk Verification Controls */}
      <BulkVerificationControls />

      {/* Main Content Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Verification Table */}
        <div className="lg:col-span-2">
          <CodeVerificationTable />
        </div>

        {/* Side Panel - Source Reference */}
        <div className="lg:col-span-1">
          <div className="bg-white border border-gray-200 rounded-lg shadow-sm p-6 sticky top-4">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">
              Source Reference
            </h3>
            {selectedCode ? (
              <>
                <div className="bg-gray-50 border border-gray-200 rounded-lg p-4 mb-4">
                  <div className="text-center text-gray-400 text-5xl mb-3">
                    <span aria-hidden="true">📄</span>
                  </div>
                  <p className="text-sm text-gray-600 text-center">
                    Clinical Document
                  </p>
                </div>

                <div className="space-y-3 mb-4">
                  <div className="flex justify-between items-start border-b border-gray-100 pb-2">
                    <span className="text-sm text-gray-500">Code</span>
                    <span className="text-sm font-semibold text-gray-900 font-mono">
                      {selectedCode.code}
                    </span>
                  </div>
                  <div className="flex justify-between items-start border-b border-gray-100 pb-2">
                    <span className="text-sm text-gray-500">Code System</span>
                    <span className="text-sm font-semibold text-gray-900">
                      {selectedCode.codeSystem}
                    </span>
                  </div>
                  <div className="flex justify-between items-start border-b border-gray-100 pb-2">
                    <span className="text-sm text-gray-500">Confidence</span>
                    <span className="text-sm font-semibold text-gray-900">
                      {selectedCode.confidenceScore}%
                    </span>
                  </div>
                  <div className="flex justify-between items-start border-b border-gray-100 pb-2">
                    <span className="text-sm text-gray-500">Rank</span>
                    <span className="text-sm font-semibold text-gray-900">
                      #{selectedCode.rank}
                    </span>
                  </div>
                </div>

                <div className="bg-amber-50 border border-amber-200 rounded-lg p-4 mb-4">
                  <h4 className="text-sm font-semibold text-amber-900 mb-2">
                    AI Rationale
                  </h4>
                  <p className="text-sm text-amber-800">
                    {selectedCode.rationale}
                  </p>
                </div>

                <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                  <h4 className="text-sm font-semibold text-blue-900 mb-2">
                    Source Clinical Text
                  </h4>
                  <p className="text-sm text-blue-800 whitespace-pre-wrap">
                    {selectedCode.sourceClinicalText}
                  </p>
                </div>
              </>
            ) : (
              <div className="text-center py-12">
                <div className="text-gray-300 text-5xl mb-3">
                  <span aria-hidden="true">👆</span>
                </div>
                <p className="text-gray-500 text-sm">
                  Select a code to view source reference
                </p>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};
