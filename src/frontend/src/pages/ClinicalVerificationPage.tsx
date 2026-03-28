/**
 * ClinicalVerificationPage for SCR-023 - Clinical Data Verification
 * Staff/Admin page for reviewing, verifying, and rejecting AI-extracted clinical data and medical codes
 */

import { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useParams, useNavigate } from 'react-router-dom';
import type { AppDispatch } from '../store';
import {
  fetchVerificationDashboard,
  verifyDataPoint,
  rejectDataPoint,
  acceptMedicalCode,
  rejectMedicalCode,
  modifyMedicalCode,
  setVerificationTab,
  setVerificationSearchTerm,
  setVerificationStatusFilter,
  selectVerificationDashboard,
  selectVerificationLoading,
  selectVerificationError,
  selectVerificationTab,
  selectVerificationAction,
} from '../store/slices/clinicalVerificationSlice';
import { VerificationTable } from '../components/verification/VerificationTable';
import { MedicalCodesTable } from '../components/verification/MedicalCodesTable';

export function ClinicalVerificationPage() {
  const dispatch = useDispatch<AppDispatch>();
  const navigate = useNavigate();
  const { patientId: urlPatientId } = useParams<{ patientId: string }>();
  const dashboard = useSelector(selectVerificationDashboard);
  const isLoading = useSelector(selectVerificationLoading);
  const error = useSelector(selectVerificationError);
  const activeTab = useSelector(selectVerificationTab);
  const actionInProgress = useSelector(selectVerificationAction);

  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<string | null>(null);

  // Auto-load when navigating from queue with a patientId in the URL
  useEffect(() => {
    if (urlPatientId) {
      dispatch(fetchVerificationDashboard(urlPatientId));
    }
  }, [urlPatientId, dispatch]);

  useEffect(() => {
    dispatch(setVerificationSearchTerm(searchTerm));
  }, [searchTerm, dispatch]);

  useEffect(() => {
    dispatch(setVerificationStatusFilter(statusFilter));
  }, [statusFilter, dispatch]);

  const handleVerifyData = (id: string) => {
    dispatch(verifyDataPoint(id));
  };

  const handleRejectData = (id: string, reason?: string) => {
    dispatch(rejectDataPoint({ id, reason }));
  };

  const handleAcceptCode = (codeId: string) => {
    dispatch(acceptMedicalCode(codeId));
  };

  const handleRejectCode = (codeId: string, reason?: string) => {
    dispatch(rejectMedicalCode({ codeId, reason }));
  };

  const handleModifyCode = (codeId: string, codeValue: string, codeDescription: string) => {
    dispatch(modifyMedicalCode({ medicalCodeId: codeId, codeValue, codeDescription }));
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <button
          onClick={() => navigate('/staff/verification')}
          className="text-sm text-blue-600 hover:text-blue-700 flex items-center gap-1"
        >
          &larr; Back to Queue
        </button>
        <h1 className="text-2xl font-bold text-neutral-900">Clinical Data Verification</h1>
      </div>


      {error && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-4">
          <p className="text-red-700 text-sm">{error}</p>
        </div>
      )}

      {dashboard && (
        <>
          {/* Summary Cards */}
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
            <div className="bg-yellow-50 border-l-4 border-yellow-400 rounded-lg p-4">
              <p className="text-sm text-neutral-600">Pending</p>
              <p className="text-2xl font-bold text-neutral-900">{dashboard.pendingCount}</p>
            </div>
            <div className="bg-green-50 border-l-4 border-green-400 rounded-lg p-4">
              <p className="text-sm text-neutral-600">Verified</p>
              <p className="text-2xl font-bold text-neutral-900">{dashboard.verifiedCount}</p>
            </div>
            <div className="bg-red-50 border-l-4 border-red-400 rounded-lg p-4">
              <p className="text-sm text-neutral-600">Rejected</p>
              <p className="text-2xl font-bold text-neutral-900">{dashboard.rejectedCount}</p>
            </div>
            <div className="bg-orange-50 border-l-4 border-orange-400 rounded-lg p-4">
              <p className="text-sm text-neutral-600">Conflicts</p>
              <p className="text-2xl font-bold text-neutral-900">{dashboard.conflictCount}</p>
            </div>
          </div>

          {/* Filters + Tabs */}
          <div className="bg-white rounded-lg shadow-sm border border-neutral-200">
            <div className="p-4 border-b border-neutral-200">
              <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-3">
                {/* Tabs */}
                <div className="flex gap-1 bg-neutral-100 rounded-lg p-1">
                  <button
                    onClick={() => dispatch(setVerificationTab('clinicalData'))}
                    className={`px-4 py-2 text-sm font-medium rounded-md transition-colors ${
                      activeTab === 'clinicalData'
                        ? 'bg-white text-blue-600 shadow-sm'
                        : 'text-neutral-600 hover:text-neutral-900'
                    }`}
                  >
                    Clinical Data ({dashboard.clinicalData.length})
                  </button>
                  <button
                    onClick={() => dispatch(setVerificationTab('medicalCodes'))}
                    className={`px-4 py-2 text-sm font-medium rounded-md transition-colors ${
                      activeTab === 'medicalCodes'
                        ? 'bg-white text-blue-600 shadow-sm'
                        : 'text-neutral-600 hover:text-neutral-900'
                    }`}
                  >
                    Medical Codes ({dashboard.medicalCodes.length})
                  </button>
                </div>

                {/* Filters */}
                <div className="flex items-center gap-2">
                  <input
                    type="text"
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                    placeholder="Search..."
                    className="border border-neutral-300 rounded-lg px-3 py-1.5 text-sm w-40 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none"
                  />
                  <select
                    value={statusFilter ?? ''}
                    onChange={(e) => setStatusFilter(e.target.value || null)}
                    className="border border-neutral-300 rounded-lg px-3 py-1.5 text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none"
                    aria-label="Filter by status"
                  >
                    <option value="">All Statuses</option>
                    <option value="AISuggested">Pending</option>
                    <option value="Verified">Verified</option>
                    <option value="Rejected">Rejected</option>
                    <option value="Conflict">Conflict</option>
                  </select>
                </div>
              </div>
            </div>

            {/* Table Content */}
            <div className="p-0">
              {activeTab === 'clinicalData' ? (
                <VerificationTable
                  items={dashboard.clinicalData}
                  actionInProgress={actionInProgress}
                  onVerify={handleVerifyData}
                  onReject={handleRejectData}
                  searchTerm={searchTerm}
                  statusFilter={statusFilter}
                />
              ) : (
                <MedicalCodesTable
                  codes={dashboard.medicalCodes}
                  actionInProgress={actionInProgress}
                  onAccept={handleAcceptCode}
                  onReject={handleRejectCode}
                  onModify={handleModifyCode}
                  searchTerm={searchTerm}
                  statusFilter={statusFilter}
                />
              )}
            </div>
          </div>
        </>
      )}

      {!dashboard && !isLoading && !error && (
        <div className="flex items-center justify-center min-h-[300px]">
          <div className="text-center max-w-md">
            <div className="text-neutral-300 text-5xl mb-3">🔍</div>
            <h2 className="text-lg font-semibold text-neutral-900 mb-2">Select a Patient</h2>
            <p className="text-neutral-500 text-sm">
              Enter a patient ID above to review their AI-extracted clinical data and medical codes.
            </p>
          </div>
        </div>
      )}
    </div>
  );
}
