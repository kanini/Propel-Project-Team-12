/**
 * HealthDashboardPage for SCR-016 - Patient Health Dashboard 360°
 * Displays aggregated clinical data extracted from uploaded documents
 * with AI confidence badges, medical codes, and verification status
 */

import { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import type { AppDispatch } from '../store';
import {
  fetchHealthDashboard,
  setActiveTab,
  selectHealthDashboard,
  selectHealthLoading,
  selectHealthError,
  selectActiveTab,
} from '../store/slices/healthDashboardSlice';
import { PatientHeader } from '../components/health/PatientHeader';
import { StatsOverview } from '../components/health/StatsOverview';
import { ClinicalDataSection } from '../components/health/ClinicalDataSection';
import { MedicalCodesPanel } from '../components/health/MedicalCodesPanel';

const tabs = [
  { key: 'conditions', label: 'Conditions' },
  { key: 'medications', label: 'Medications' },
  { key: 'allergies', label: 'Allergies' },
  { key: 'vitals', label: 'Vitals' },
  { key: 'labResults', label: 'Lab Results' },
  { key: 'codes', label: 'Medical Codes' },
] as const;

export function HealthDashboardPage() {
  const dispatch = useDispatch<AppDispatch>();
  const dashboard = useSelector(selectHealthDashboard);
  const isLoading = useSelector(selectHealthLoading);
  const error = useSelector(selectHealthError);
  const activeTab = useSelector(selectActiveTab);

  useEffect(() => {
    dispatch(fetchHealthDashboard());
  }, [dispatch]);

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <div className="text-center">
          <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-blue-600 mx-auto mb-3" />
          <p className="text-neutral-500 text-sm">Loading health dashboard...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <div className="text-center max-w-md">
          <div className="text-red-500 text-4xl mb-3">!</div>
          <h2 className="text-lg font-semibold text-neutral-900 mb-2">Unable to Load Dashboard</h2>
          <p className="text-neutral-500 text-sm mb-4">{error}</p>
          <button
            onClick={() => dispatch(fetchHealthDashboard())}
            className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors text-sm font-medium"
          >
            Try Again
          </button>
        </div>
      </div>
    );
  }

  if (!dashboard) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <div className="text-center max-w-md">
          <div className="text-neutral-300 text-5xl mb-3">📋</div>
          <h2 className="text-lg font-semibold text-neutral-900 mb-2">No Health Data Yet</h2>
          <p className="text-neutral-500 text-sm">
            Upload clinical documents to see your health data extracted and organized here.
          </p>
        </div>
      </div>
    );
  }

  const tabContent: Record<string, React.ReactNode> = {
    conditions: (
      <ClinicalDataSection
        title="Conditions & Diagnoses"
        items={dashboard.conditions}
        emptyMessage="No conditions or diagnoses extracted."
      />
    ),
    medications: (
      <ClinicalDataSection
        title="Medications"
        items={dashboard.medications}
        emptyMessage="No medications extracted."
      />
    ),
    allergies: (
      <ClinicalDataSection
        title="Allergies"
        items={dashboard.allergies}
        emptyMessage="No allergies extracted."
      />
    ),
    vitals: (
      <ClinicalDataSection
        title="Vitals"
        items={dashboard.vitals}
        emptyMessage="No vital signs extracted."
      />
    ),
    labResults: (
      <ClinicalDataSection
        title="Lab Results"
        items={dashboard.labResults}
        emptyMessage="No lab results extracted."
      />
    ),
    codes: <MedicalCodesPanel codes={dashboard.medicalCodes} />,
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-neutral-900">Health Dashboard 360°</h1>
      </div>

      <PatientHeader demographics={dashboard.demographics} />

      <StatsOverview stats={dashboard.stats} />

      {/* Tab navigation */}
      <div className="border-b border-neutral-200">
        <nav className="-mb-px flex gap-6 overflow-x-auto" aria-label="Health data tabs">
          {tabs.map(({ key, label }) => {
            const isActive = activeTab === key;
            return (
              <button
                key={key}
                onClick={() => dispatch(setActiveTab(key))}
                className={`whitespace-nowrap border-b-2 py-3 px-1 text-sm font-medium transition-colors ${
                  isActive
                    ? 'border-blue-600 text-blue-600'
                    : 'border-transparent text-neutral-500 hover:border-neutral-300 hover:text-neutral-700'
                }`}
                aria-current={isActive ? 'page' : undefined}
              >
                {label}
              </button>
            );
          })}
        </nav>
      </div>

      {/* Active tab content */}
      {tabContent[activeTab]}
    </div>
  );
}
