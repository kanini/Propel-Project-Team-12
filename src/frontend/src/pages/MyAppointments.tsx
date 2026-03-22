/**
 * MyAppointments Page for US_027 - Cancel/Reschedule and US_028 - PDF Confirmation
 * Displays patient's appointments and waitlist entries in tabs (AC-4)
 * Tabs: Upcoming | Past | Waitlist
 */

import React, { useEffect, useState } from "react";
import { useAppDispatch, useAppSelector } from "../store/hooks";
import {
  fetchWaitlist,
  selectWaitlistEntries,
  selectIsLoading as selectWaitlistLoading,
  selectError,
} from "../store/slices/waitlistSlice";
import {
  fetchMyAppointments,
  downloadConfirmationPDF,
  selectMyAppointments,
  selectIsLoadingAppointments,
} from "../store/slices/appointmentSlice";
import type { Appointment } from "../types/appointment";
import { WaitlistEntry } from "../components/waitlist/WaitlistEntry";
import { WaitlistEnrollmentModal } from "../components/waitlist/WaitlistEnrollmentModal";
import { EmptyState } from "../components/common/EmptyState";
import { SkeletonLoader } from "../components/common/SkeletonLoader";

type TabType = "upcoming" | "past" | "waitlist";

/**
 * MyAppointments page component (AC-4)
 */
export const MyAppointments: React.FC = () => {
  const dispatch = useAppDispatch();

  // Redux state for appointments
  const appointments = useAppSelector(selectMyAppointments);
  const isLoadingAppointments = useAppSelector(selectIsLoadingAppointments);

  // Redux state for waitlist
  const waitlistEntries = useAppSelector(selectWaitlistEntries);
  const isLoadingWaitlist = useAppSelector(selectWaitlistLoading);
  const waitlistError = useAppSelector(selectError);

  // Tab state
  const [activeTab, setActiveTab] = useState<TabType>("upcoming");

  // PDF download state
  const [downloadingPdfId, setDownloadingPdfId] = useState<string | null>(null);
  const [pdfError, setPdfError] = useState<string | null>(null);

  /**
   * Fetch appointments on mount
   */
  useEffect(() => {
    dispatch(fetchMyAppointments());
  }, [dispatch]);

  /**
   * Fetch waitlist entries when Waitlist tab is activated
   */
  useEffect(() => {
    if (activeTab === "waitlist") {
      dispatch(fetchWaitlist());
    }
  }, [activeTab, dispatch]);

  /**
   * Filter appointments by type
   */
  const now = new Date();
  const upcomingAppointments = appointments.filter(
    (apt) =>
      new Date(apt.scheduledDateTime) > now && apt.status !== "cancelled",
  );
  const pastAppointments = appointments.filter(
    (apt) =>
      new Date(apt.scheduledDateTime) <= now ||
      apt.status === "cancelled" ||
      apt.status === "completed",
  );

  /**
   * Handle PDF download (US_028 - AC-4)
   */
  const handleDownloadPDF = async (appointment: Appointment) => {
    try {
      setDownloadingPdfId(appointment.id);
      setPdfError(null);

      await dispatch(
        downloadConfirmationPDF({
          appointmentId: appointment.id,
          confirmationNumber: appointment.confirmationNumber || "",
        }),
      ).unwrap();

      setDownloadingPdfId(null);
    } catch (error) {
      setDownloadingPdfId(null);
      setPdfError(error as string);
    }
  };

  /**
   * Tab button class names
   */
  const getTabClassName = (tab: TabType): string => {
    const baseClasses =
      "px-6 py-3 text-sm font-medium border-b-2 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 transition-colors";
    if (activeTab === tab) {
      return `${baseClasses} border-blue-600 text-blue-600`;
    }
    return `${baseClasses} border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300`;
  };

  /**
   * Render tab content based on active tab
   */
  const renderTabContent = () => {
    switch (activeTab) {
      case "upcoming":
        return renderAppointmentsTab(upcomingAppointments, "upcoming");
      case "past":
        return renderAppointmentsTab(pastAppointments, "past");
      case "waitlist":
        return renderWaitlistContent();
      default:
        return null;
    }
  };

  /**
   * Render appointments tab (Upcoming or Past)
   */
  const renderAppointmentsTab = (
    appointmentsList: Appointment[],
    type: "upcoming" | "past",
  ) => {
    // Loading state
    if (isLoadingAppointments) {
      return (
        <div className="space-y-4">
          <SkeletonLoader />
          <SkeletonLoader />
          <SkeletonLoader />
        </div>
      );
    }

    // Empty state
    if (appointmentsList.length === 0) {
      return (
        <EmptyState
          title={`No ${type === "upcoming" ? "Upcoming" : "Past"} Appointments`}
          message={
            type === "upcoming"
              ? "You don't have any upcoming appointments scheduled. Browse providers to book an appointment."
              : "You don't have any past appointments on record."
          }
          showClearButton={false}
          icon={
            <svg
              className="w-24 h-24 mx-auto"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              strokeWidth={1.5}
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
              />
            </svg>
          }
        />
      );
    }

    // Display appointments
    return (
      <div className="space-y-4">
        {appointmentsList.map((appointment) => (
          <div
            key={appointment.id}
            className="bg-white border border-gray-200 rounded-lg p-6 hover:shadow-md transition-shadow"
          >
            <div className="flex justify-between items-start mb-4">
              <div className="flex-1">
                <h3 className="text-lg font-semibold text-gray-900">
                  {appointment.providerName}
                </h3>
                <p className="text-sm text-gray-600 mt-1">
                  {appointment.providerSpecialty}
                </p>
              </div>
              <span
                className={`inline-flex items-center px-3 py-1 rounded-full text-sm font-medium ${
                  appointment.status === "scheduled"
                    ? "bg-green-100 text-green-800"
                    : appointment.status === "cancelled"
                      ? "bg-red-100 text-red-800"
                      : "bg-gray-100 text-gray-800"
                }`}
              >
                {appointment.status}
              </span>
            </div>

            <div className="grid grid-cols-2 gap-4 mb-4">
              <div>
                <p className="text-xs text-gray-500 uppercase tracking-wide">
                  Date
                </p>
                <p className="text-sm font-medium text-gray-900 mt-1">
                  {new Date(appointment.scheduledDateTime).toLocaleDateString(
                    "en-US",
                    {
                      weekday: "short",
                      month: "short",
                      day: "numeric",
                      year: "numeric",
                    },
                  )}
                </p>
              </div>
              <div>
                <p className="text-xs text-gray-500 uppercase tracking-wide">
                  Time
                </p>
                <p className="text-sm font-medium text-gray-900 mt-1">
                  {new Date(appointment.scheduledDateTime).toLocaleTimeString(
                    "en-US",
                    {
                      hour: "numeric",
                      minute: "2-digit",
                      hour12: true,
                    },
                  )}
                </p>
              </div>
              <div>
                <p className="text-xs text-gray-500 uppercase tracking-wide">
                  Visit Reason
                </p>
                <p className="text-sm font-medium text-gray-900 mt-1">
                  {appointment.visitReason}
                </p>
              </div>
              <div>
                <p className="text-xs text-gray-500 uppercase tracking-wide">
                  Confirmation #
                </p>
                <p className="text-sm font-mono font-medium text-blue-600 mt-1">
                  {appointment.confirmationNumber}
                </p>
              </div>
            </div>

            <div className="flex gap-2 pt-4 border-t border-gray-100">
              {/* Download PDF button (US_028 - AC-4) */}
              <button
                onClick={() => handleDownloadPDF(appointment)}
                disabled={downloadingPdfId === appointment.id}
                className="inline-flex items-center justify-center gap-2 px-4 py-2 
                                         border border-gray-300 rounded-lg text-sm font-medium 
                                         text-gray-700 bg-white hover:bg-gray-50 
                                         focus:outline-none focus:ring-2 focus:ring-blue-500 
                                         focus:ring-offset-2 transition-colors disabled:opacity-50 
                                         disabled:cursor-not-allowed"
              >
                {downloadingPdfId === appointment.id ? (
                  <>
                    <svg
                      className="w-4 h-4 animate-spin"
                      fill="none"
                      viewBox="0 0 24 24"
                    >
                      <circle
                        className="opacity-25"
                        cx="12"
                        cy="12"
                        r="10"
                        stroke="currentColor"
                        strokeWidth="4"
                      />
                      <path
                        className="opacity-75"
                        fill="currentColor"
                        d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                      />
                    </svg>
                    Downloading...
                  </>
                ) : (
                  <>
                    <svg
                      className="w-4 h-4"
                      fill="none"
                      viewBox="0 0 24 24"
                      stroke="currentColor"
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth={2}
                        d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
                      />
                    </svg>
                    View Confirmation
                  </>
                )}
              </button>
            </div>
          </div>
        ))}

        {/* PDF error toast */}
        {pdfError && (
          <div className="fixed bottom-4 right-4 max-w-md bg-red-50 border border-red-200 rounded-lg p-4 shadow-lg">
            <div className="flex items-start gap-3">
              <svg
                className="w-5 h-5 text-red-600 mt-0.5 flex-shrink-0"
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
              <div className="flex-1">
                <p className="text-sm font-medium text-red-800">{pdfError}</p>
              </div>
              <button
                onClick={() => setPdfError(null)}
                className="text-red-400 hover:text-red-600"
              >
                <svg
                  className="w-5 h-5"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M6 18L18 6M6 6l12 12"
                  />
                </svg>
              </button>
            </div>
          </div>
        )}
      </div>
    );
  };

  /**
   * Render waitlist tab content
   */
  const renderWaitlistContent = () => {
    // Loading state
    if (isLoadingWaitlist) {
      return (
        <div className="space-y-4">
          <SkeletonLoader />
          <SkeletonLoader />
          <SkeletonLoader />
        </div>
      );
    }

    // Error state
    if (waitlistError && waitlistError.code !== "conflict") {
      return (
        <div
          className="bg-red-50 border border-red-200 rounded-lg p-4"
          role="alert"
          aria-live="assertive"
        >
          <div className="flex items-start gap-3">
            <svg
              className="w-5 h-5 text-red-600 mt-0.5"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
              />
            </svg>
            <div>
              <h3 className="text-sm font-medium text-red-800">
                Error loading waitlist
              </h3>
              <p className="text-sm text-red-700 mt-1">
                {waitlistError.message}
              </p>
              <button
                onClick={() => dispatch(fetchWaitlist())}
                className="mt-2 text-sm text-red-800 underline hover:text-red-900 focus:outline-none"
              >
                Try again
              </button>
            </div>
          </div>
        </div>
      );
    }

    // Empty state
    if (waitlistEntries.length === 0) {
      return (
        <EmptyState
          title="No Waitlist Entries"
          message="You're not on any waitlists. Browse providers to join one when your preferred appointment slot is unavailable."
          showClearButton={false}
          icon={
            <svg
              className="w-24 h-24 mx-auto"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              strokeWidth={1.5}
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"
              />
            </svg>
          }
        />
      );
    }

    // Display waitlist entries
    return (
      <div className="space-y-4">
        {waitlistEntries.map((entry) => (
          <WaitlistEntry key={entry.id} entry={entry} />
        ))}
      </div>
    );
  };

  return (
    <div className="container mx-auto px-4 py-8 max-w-6xl">
      {/* Page Header */}
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-gray-900">My Appointments</h1>
        <p className="text-gray-600 mt-1">
          View and manage your appointments and waitlist entries
        </p>
      </div>

      {/* Tabs */}
      <div className="bg-white rounded-lg shadow">
        {/* Tab Navigation */}
        <div className="border-b border-gray-200">
          <nav className="flex -mb-px" aria-label="Appointment tabs">
            <button
              onClick={() => setActiveTab("upcoming")}
              className={getTabClassName("upcoming")}
              aria-current={activeTab === "upcoming" ? "page" : undefined}
            >
              Upcoming
            </button>
            <button
              onClick={() => setActiveTab("past")}
              className={getTabClassName("past")}
              aria-current={activeTab === "past" ? "page" : undefined}
            >
              Past
            </button>
            <button
              onClick={() => setActiveTab("waitlist")}
              className={getTabClassName("waitlist")}
              aria-current={activeTab === "waitlist" ? "page" : undefined}
            >
              Waitlist
              {waitlistEntries.length > 0 && (
                <span className="ml-2 inline-flex items-center justify-center px-2 py-0.5 text-xs font-bold leading-none text-white bg-blue-600 rounded-full">
                  {waitlistEntries.length}
                </span>
              )}
            </button>
          </nav>
        </div>

        {/* Tab Content */}
        <div className="p-6">{renderTabContent()}</div>
      </div>

      {/* Waitlist Enrollment Modal */}
      <WaitlistEnrollmentModal />
    </div>
  );
};

export default MyAppointments;
