/**
 * Patient Health Dashboard page (SCR-016).
 * Displays comprehensive 360° patient health profile with demographics,
 * conditions, medications, allergies, vital trends, and recent encounters.
 */

import React, { useEffect } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useAppDispatch, useAppSelector } from "../store/hooks";
import {
  fetchPatientProfile360,
  clearProfile,
} from "../store/slices/patientProfileSlice";
import { DemographicsSection } from "../components/patient-profile/DemographicsSection";
import { ConditionsSection } from "../components/patient-profile/ConditionsSection";
import { MedicationsSection } from "../components/patient-profile/MedicationsSection";
import { AllergiesSection } from "../components/patient-profile/AllergiesSection";
import { VitalTrendsChart } from "../components/patient-profile/VitalTrendsChart";
import { EncountersSection } from "../components/patient-profile/EncountersSection";

export const PatientHealthDashboard: React.FC = () => {
  const navigate = useNavigate();
  const { patientId } = useParams<{ patientId?: string }>();
  const dispatch = useAppDispatch();

  const { user } = useAppSelector((state) => state.auth);
  const { profile, loading, error } = useAppSelector(
    (state) => state.patientProfile,
  );

  // Determine if current user is staff/admin
  const isStaffView = user?.role === "Staff" || user?.role === "Admin";
  const isPatient = user?.role === "Patient";

  // Get effective patient ID (from URL param for staff, from user for patients)
  const effectivePatientId = patientId || user?.userId;

  useEffect(() => {
    // Role-based authorization
    if (!user) {
      navigate("/login");
      return;
    }

    // Patients can only view their own profile
    if (isPatient && patientId && patientId !== user.userId) {
      navigate("/unauthorized");
      return;
    }

    // Fetch profile data
    if (effectivePatientId) {
      dispatch(
        fetchPatientProfile360({
          patientId: effectivePatientId,
          // Default to last 12 months for vital trends
          vitalRangeStart: new Date(
            Date.now() - 365 * 24 * 60 * 60 * 1000,
          ).toISOString(),
          vitalRangeEnd: new Date().toISOString(),
        }),
      );
    }

    // Cleanup on unmount
    return () => {
      dispatch(clearProfile());
    };
  }, [dispatch, effectivePatientId, user, isPatient, patientId, navigate]);

  // Loading state (UXR-502 skeleton loading)
  if (loading && !profile) {
    return (
      <div className="min-h-screen bg-gray-50 p-4 md:p-8">
        <div className="max-w-7xl mx-auto">
          <div className="mb-8">
            <div className="h-8 bg-gray-200 rounded w-1/3 animate-pulse mb-2" />
            <div className="h-4 bg-gray-200 rounded w-1/4 animate-pulse" />
          </div>
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            {[...Array(6)].map((_, i) => (
              <div
                key={i}
                className="bg-white rounded-lg shadow-sm border border-gray-200 p-6"
              >
                <div className="h-6 bg-gray-200 rounded w-1/2 animate-pulse mb-4" />
                <div className="space-y-3">
                  <div className="h-4 bg-gray-200 rounded animate-pulse" />
                  <div className="h-4 bg-gray-200 rounded w-5/6 animate-pulse" />
                  <div className="h-4 bg-gray-200 rounded w-4/6 animate-pulse" />
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    );
  }

  // Error state
  if (error) {
    return (
      <div className="min-h-screen bg-gray-50 p-4 md:p-8">
        <div className="max-w-7xl mx-auto">
          <div className="bg-red-50 border border-red-200 rounded-lg p-6">
            <div className="flex items-center gap-3">
              <svg
                className="h-6 w-6 text-red-600"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
                />
              </svg>
              <div>
                <h3 className="text-lg font-semibold text-red-900">
                  Error Loading Profile
                </h3>
                <p className="text-sm text-red-700 mt-1">{error}</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  // Empty state (UXR-605) - No profile data
  if (!profile || profile.profileCompleteness === 0) {
    return (
      <div className="min-h-screen bg-gray-50 p-4 md:p-8">
        <div className="max-w-4xl mx-auto">
          <div className="text-center py-16">
            <svg
              className="mx-auto h-24 w-24 text-gray-400"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              aria-hidden="true"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={1.5}
                d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
              />
            </svg>
            <h2 className="mt-6 text-2xl font-semibold text-gray-900">
              Build Your Health Profile
            </h2>
            <p className="mt-2 text-base text-gray-600 max-w-lg mx-auto">
              Upload your clinical documents to see your consolidated health
              summary with demographics, conditions, medications, allergies, and
              vital trends.
            </p>
            <div className="mt-8">
              <button
                type="button"
                onClick={() => navigate("/documents/upload")}
                className="inline-flex items-center px-6 py-3 border border-transparent text-base font-medium rounded-md shadow-sm text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-colors"
              >
                <svg
                  className="-ml-1 mr-3 h-5 w-5"
                  xmlns="http://www.w3.org/2000/svg"
                  viewBox="0 0 20 20"
                  fill="currentColor"
                  aria-hidden="true"
                >
                  <path
                    fillRule="evenodd"
                    d="M10 3a1 1 0 011 1v5h5a1 1 0 110 2h-5v5a1 1 0 11-2 0v-5H4a1 1 0 110-2h5V4a1 1 0 011-1z"
                    clipRule="evenodd"
                  />
                </svg>
                Upload Documents
              </button>
            </div>
          </div>
        </div>
      </div>
    );
  }

  // Main dashboard view with profile data
  return (
    <div className="min-h-screen bg-gray-50 p-4 md:p-8">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <header className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900">
            {isStaffView ? "Patient Health Profile" : "My Health Dashboard"}
          </h1>
          <div className="mt-2 flex items-center gap-4 text-sm text-gray-600">
            <span>
              Profile Completeness: {profile.profileCompleteness.toFixed(0)}%
            </span>
            {profile.hasUnresolvedConflicts && (
              <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-800">
                ⚠ Unresolved Conflicts
              </span>
            )}
            <span>
              Last Updated:{" "}
              {new Date(profile.lastAggregatedAt).toLocaleDateString()}
            </span>
          </div>
        </header>

        {/* Grid layout - responsive */}
        <div className="space-y-6">
          {/* Demographics - full width */}
          <DemographicsSection demographics={profile.demographics} />

          {/* Two-column grid for sections */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <ConditionsSection
              conditions={profile.conditions}
              isStaffView={isStaffView}
            />
            <MedicationsSection
              medications={profile.medications}
              isStaffView={isStaffView}
            />
            <AllergiesSection
              allergies={profile.allergies}
              isStaffView={isStaffView}
            />
          </div>

          {/* Vital trends - full width */}
          <VitalTrendsChart vitalTrends={profile.vitalTrends} />

          {/* Recent encounters - full width */}
          <EncountersSection encounters={profile.encounters} />
        </div>
      </div>
    </div>
  );
};
