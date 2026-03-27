/**
 * StaffPatientView Page (US_048, SCR-017)
 * 360-degree patient view with conflict alerts for staff verification.
 * This page demonstrates conflict detection and resolution workflow.
 */

import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useAppDispatch, useAppSelector } from "../../store/hooks";
import { useAuth } from "../../hooks/useAuth";
import { usePusherConflicts } from "../../hooks/usePusherConflicts";
import {
  fetchConflictSummary,
  clearConflicts,
} from "../../store/slices/conflictsSlice";
import { ConflictAlertBanner } from "../../components/ConflictAlertBanner";
import { ConflictList } from "../../components/ConflictList";
import { ConflictDetailModal } from "../../components/ConflictDetailModal";

/**
 * StaffPatientView Component
 * Main staff interface for viewing patient data and resolving conflicts.
 */
export const StaffPatientView = () => {
  const { patientId } = useParams<{ patientId: string }>();
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const { user, role } = useAuth();

  const { summary, summaryLoading, selectedConflict, isModalOpen } =
    useAppSelector((state) => state.conflicts);

  const [showConflictList, setShowConflictList] = useState(false);

  // Parse patientId from URL param
  const patientIdNum = patientId ? parseInt(patientId, 10) : null;

  // Enable Pusher real-time notifications (only for staff)
  usePusherConflicts({
    patientId: patientIdNum,
    enabled: role === "Staff" || role === "Admin",
  });

  // Verify staff authorization
  useEffect(() => {
    if (role !== "Staff" && role !== "Admin") {
      navigate("/unauthorized");
    }
  }, [role, navigate]);

  // Fetch conflict summary on mount
  useEffect(() => {
    if (patientIdNum) {
      dispatch(fetchConflictSummary(patientIdNum));
    }

    // Cleanup on unmount
    return () => {
      dispatch(clearConflicts());
    };
  }, [dispatch, patientIdNum]);

  const handleViewConflicts = () => {
    setShowConflictList(true);
    // Scroll to conflicts section
    const conflictSection = document.getElementById("conflicts-section");
    if (conflictSection) {
      conflictSection.scrollIntoView({ behavior: "smooth" });
    }
  };

  const handleCloseModal = () => {
    dispatch({ type: "conflicts/closeConflictModal" });
  };

  if (!patientIdNum) {
    return (
      <div className="min-h-screen bg-gray-50 p-6">
        <div className="max-w-7xl mx-auto">
          <div className="bg-red-50 border border-red-200 rounded-lg p-4">
            <p className="text-red-800">Invalid patient ID</p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white border-b border-gray-200">
        <div className="max-w-7xl mx-auto px-6 py-4">
          <div className="flex items-center justify-between">
            <div>
              <button
                onClick={() => navigate("/staff/queue")}
                className="text-sm text-gray-600 hover:text-gray-900 mb-2 flex items-center gap-1"
              >
                <svg
                  className="h-4 w-4"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M15 19l-7-7 7-7"
                  />
                </svg>
                Back to Queue
              </button>
              <h1 className="text-2xl font-bold text-gray-900">Patient View</h1>
              <p className="text-sm text-gray-600 mt-1">
                Patient ID: {patientIdNum}
              </p>
            </div>
            <div className="flex items-center gap-4">
              <span className="text-sm text-gray-600">
                Staff: {user?.email || "Unknown"}
              </span>
            </div>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="max-w-7xl mx-auto px-6 py-6 space-y-6">
        {/* Conflict Alert Banner (AC2) */}
        {summary && summary.totalUnresolved > 0 && (
          <ConflictAlertBanner
            summary={summary}
            onViewConflicts={handleViewConflicts}
          />
        )}

        {/* Loading state for summary */}
        {summaryLoading && !summary && (
          <div className="bg-white rounded-lg shadow p-6">
            <div className="flex items-center gap-3">
              <div className="animate-spin rounded-full h-5 w-5 border-b-2 border-blue-600"></div>
              <span className="text-gray-600">Loading conflict summary...</span>
            </div>
          </div>
        )}

        {/* Patient Profile Section (Placeholder) */}
        <div className="bg-white rounded-lg shadow p-6">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">
            Patient Profile
          </h2>
          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700">
                  Patient ID
                </label>
                <p className="mt-1 text-sm text-gray-900">{patientIdNum}</p>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">
                  Status
                </label>
                <p className="mt-1 text-sm text-gray-900">Active</p>
              </div>
            </div>
            <p className="text-sm text-gray-500 italic">
              Complete 360° patient profile with aggregated clinical data will
              be displayed here.
            </p>
          </div>
        </div>

        {/* Conflicts Section */}
        {showConflictList && (
          <div id="conflicts-section">
            <ConflictList patientId={patientIdNum} />
          </div>
        )}

        {/* Placeholder Sections (Future Enhancement) */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div className="bg-white rounded-lg shadow p-6">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">
              Clinical Data Summary
            </h3>
            <p className="text-sm text-gray-500 italic">
              Consolidated medications, allergies, conditions, and vital trends
              will be displayed here.
            </p>
          </div>
          <div className="bg-white rounded-lg shadow p-6">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">
              Recent Documents
            </h3>
            <p className="text-sm text-gray-500 italic">
              List of uploaded clinical documents with processing status.
            </p>
          </div>
        </div>
      </main>

      {/* Conflict Resolution Modal */}
      <ConflictDetailModal
        conflict={selectedConflict}
        isOpen={isModalOpen}
        onClose={handleCloseModal}
      />
    </div>
  );
};
