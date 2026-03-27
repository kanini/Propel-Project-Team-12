/**
 * MyAppointments Page (SCR-010)
 * Displays patient's appointments and waitlist entries in tabs
 * Tabs: Upcoming | Past | Waitlist
 * Includes Reschedule/Cancel actions and confirmation modal
 * US_027 - Cancel and Reschedule Appointments
 */

import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAppDispatch, useAppSelector } from "../store/hooks";
import {
  fetchWaitlist,
  selectWaitlistEntries,
  selectIsLoading as selectWaitlistLoading,
  selectError,
} from "../store/slices/waitlistSlice";
import {
  fetchMyAppointments,
  selectMyAppointments,
  selectIsLoadingAppointments,
} from "../store/slices/appointmentSlice";
import type { Appointment, TimeSlot } from "../types/appointment";
import { WaitlistEntry } from "../components/waitlist/WaitlistEntry";
import { WaitlistEnrollmentModal } from "../components/waitlist/WaitlistEnrollmentModal";
import { RescheduleModal } from "../components/appointments/RescheduleModal";
import { EmptyState } from "../components/common/EmptyState";
import { SkeletonLoader } from "../components/common/SkeletonLoader";
import { cancelAppointment, rescheduleAppointment as rescheduleAppointmentApi, fetchProviderAvailability } from "../api/appointmentApi";

type TabType = "upcoming" | "past" | "waitlist";

/**
 * MyAppointments page component (SCR-010)
 */
export const MyAppointments: React.FC = () => {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();

  // Redux state for appointments
  const appointments = useAppSelector(selectMyAppointments);
  const isLoadingAppointments = useAppSelector(selectIsLoadingAppointments);

  // Redux state for waitlist
  const waitlistEntries = useAppSelector(selectWaitlistEntries);
  const isLoadingWaitlist = useAppSelector(selectWaitlistLoading);
  const waitlistError = useAppSelector(selectError);

  // Tab state
  const [activeTab, setActiveTab] = useState<TabType>("upcoming");

  // Cancel modal state
  const [cancelModalOpen, setCancelModalOpen] = useState(false);
  const [selectedAppointment, setSelectedAppointment] = useState<Appointment | null>(null);
  const [isCancelling, setIsCancelling] = useState(false);

  // Reschedule modal state (US_027)
  const [rescheduleModalOpen, setRescheduleModalOpen] = useState(false);
  const [rescheduleAppointment, setRescheduleAppointment] = useState<Appointment | null>(null);
  const [availableSlots, setAvailableSlots] = useState<TimeSlot[]>([]);
  const [isLoadingSlots, setIsLoadingSlots] = useState(false);
  const [isRescheduling, setIsRescheduling] = useState(false);

  // Toast notification state (US_027)
  const [toast, setToast] = useState<{
    type: 'success' | 'error';
    message: string;
  } | null>(null);

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
   * Handle cancel appointment button click
   */
  const handleCancelClick = (appointment: Appointment) => {
    setSelectedAppointment(appointment);
    setCancelModalOpen(true);
  };

  /**
   * Handle cancel confirmation
   */
  const handleCancelConfirm = async () => {
    if (!selectedAppointment) return;

    try {
      setIsCancelling(true);
      
      // Call DELETE /api/appointments/{id}
      await cancelAppointment(selectedAppointment.id);

      // Close modal and reset state
      setCancelModalOpen(false);
      setSelectedAppointment(null);

      // Refresh appointments list
      await dispatch(fetchMyAppointments());
      
      // Show success toast notification
      setToast({
        type: 'success',
        message: 'Appointment cancelled successfully. A confirmation has been sent to your email.',
      });

      // Auto-hide toast after 6 seconds
      setTimeout(() => setToast(null), 6000);
    } catch (error) {
      console.error('Error canceling appointment:', error);
      
      // Show error toast
      setToast({
        type: 'error',
        message: error instanceof Error ? error.message : 'Failed to cancel appointment. Please try again.',
      });

      // Auto-hide toast after 8 seconds
      setTimeout(() => setToast(null), 8000);
    } finally {
      setIsCancelling(false);
    }
  };

  /**
   * Handle reschedule button click (US_027 AC-2)
   * Opens modal and fetches available slots for the provider
   */
  const handleRescheduleClick = async (appointment: Appointment) => {
    setRescheduleAppointment(appointment);
    setRescheduleModalOpen(true);
    setIsLoadingSlots(true);

    try {
      // Calculate date range (next 30 days)
      const startDate = new Date();
      const endDate = new Date();
      endDate.setDate(endDate.getDate() + 30);

      // Fetch available slots for the provider
      const slots = await fetchProviderAvailability(
        appointment.providerId,
        startDate.toISOString().split('T')[0],
        endDate.toISOString().split('T')[0]
      );

      // Filter out the current slot and convert to TimeSlot format
      const filteredSlots: TimeSlot[] = slots
        .filter((slot) => slot.id !== appointment.timeSlotId && slot.status === 'available')
        .map((slot) => ({
          id: slot.id,
          providerId: slot.providerId,
          startTime: slot.startTime,
          endTime: slot.endTime,
          status: 'available' as const,
        }));

      setAvailableSlots(filteredSlots);
    } catch (error) {
      console.error('Error fetching available slots:', error);
      alert(error instanceof Error ? error.message : 'Failed to load available slots. Please try again.');
      setRescheduleModalOpen(false);
    } finally {
      setIsLoadingSlots(false);
    }
  };

  /**
   * Handle reschedule confirmation (US_027 AC-3)
   */
  const handleRescheduleConfirm = async (newTimeSlotId: string) => {
    if (!rescheduleAppointment) return;

    try {
      setIsRescheduling(true);
      
      // Call PATCH /api/appointments/{id}/reschedule
      await rescheduleAppointmentApi(rescheduleAppointment.id, newTimeSlotId);

      // Close modal and reset state
      setRescheduleModalOpen(false);
      setRescheduleAppointment(null);
      setAvailableSlots([]);

      // Refresh appointments list
      await dispatch(fetchMyAppointments());
      
      // Show success toast notification
      setToast({
        type: 'success',
        message: 'Appointment rescheduled successfully! A confirmation has been sent to your email.',
      });

      // Auto-hide toast after 6 seconds
      setTimeout(() => setToast(null), 6000);
    } catch (error) {
      console.error('Error rescheduling appointment:', error);
      
      // Show error toast
      setToast({
        type: 'error',
        message: error instanceof Error ? error.message : 'Failed to reschedule appointment. Please try again.',
      });

      // Auto-hide toast after 8 seconds
      setTimeout(() => setToast(null), 8000);
    } finally {
      setIsRescheduling(false);
    }
  };

  /**
   * Handle reschedule modal cancel
   */
  const handleRescheduleCancel = () => {
    setRescheduleModalOpen(false);
    setRescheduleAppointment(null);
    setAvailableSlots([]);
  };

  /**
   * Close toast notification
   */
  const handleCloseToast = () => {
    setToast(null);
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
   * Render appointments tab (Upcoming or Past) with table layout
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

    // Display appointments in table format (SCR-010)
    return (
      <div className="overflow-x-auto">
        <table className="w-full border-collapse">
          <thead>
            <tr className="border-b border-gray-200 bg-gray-50">
              <th className="text-left text-xs font-semibold text-gray-700 uppercase tracking-wide py-3 px-4">
                Date & Time
              </th>
              <th className="text-left text-xs font-semibold text-gray-700 uppercase tracking-wide py-3 px-4">
                Provider
              </th>
              <th className="text-left text-xs font-semibold text-gray-700 uppercase tracking-wide py-3 px-4">
                Service
              </th>
              <th className="text-left text-xs font-semibold text-gray-700 uppercase tracking-wide py-3 px-4">
                Status
              </th>
              {type === 'upcoming' && (
                <th className="text-left text-xs font-semibold text-gray-700 uppercase tracking-wide py-3 px-4">
                  Intake
                </th>
              )}
              {type === 'upcoming' && (
                <th className="text-left text-xs font-semibold text-gray-700 uppercase tracking-wide py-3 px-4">
                  Actions
                </th>
              )}
            </tr>
          </thead>
          <tbody>
            {appointmentsList.map((appointment) => (
              <tr
                key={appointment.id}
                className="border-b border-gray-100 hover:bg-gray-50 transition-colors"
              >
                {/* Date & Time */}
                <td className="py-3 px-4 text-sm text-gray-900">
                  {new Date(appointment.scheduledDateTime).toLocaleDateString('en-US', {
                    month: 'short',
                    day: 'numeric',
                    year: 'numeric',
                  })}{' '}
                  ·{' '}
                  {new Date(appointment.scheduledDateTime).toLocaleTimeString('en-US', {
                    hour: 'numeric',
                    minute: '2-digit',
                    hour12: true,
                  })}
                </td>

                {/* Provider */}
                <td className="py-3 px-4 text-sm font-medium text-gray-900">
                  {appointment.providerName}
                </td>

                {/* Service */}
                <td className="py-3 px-4 text-sm text-gray-600">
                  {appointment.visitReason || appointment.providerSpecialty}
                </td>

                {/* Status */}
                <td className="py-3 px-4">
                  <span
                    className={`inline-flex items-center px-2 py-1 rounded-full text-xs font-medium ${
                      appointment.status === 'confirmed' || appointment.status === 'scheduled'
                        ? 'bg-green-100 text-green-800'
                        : appointment.status === 'cancelled'
                        ? 'bg-red-100 text-red-800'
                        : appointment.status === 'completed'
                        ? 'bg-blue-100 text-blue-800'
                        : 'bg-gray-100 text-gray-800'
                    }`}
                  >
                    {appointment.status.charAt(0).toUpperCase() + appointment.status.slice(1)}
                  </span>
                </td>

                {/* Intake (only for upcoming) */}
                {type === 'upcoming' && (
                  <td className="py-3 px-4">
                    <span
                      className={`inline-flex items-center px-2 py-1 rounded-full text-xs font-medium ${
                        appointment.intakeStatus === 'completed'
                          ? 'bg-green-100 text-green-800'
                          : appointment.intakeStatus === 'inProgress'
                          ? 'bg-blue-100 text-blue-800'
                          : 'bg-yellow-100 text-yellow-800'
                      }`}
                    >
                      {appointment.intakeStatus === 'inProgress'
                        ? 'In Progress'
                        : appointment.intakeStatus === 'completed'
                        ? 'Completed'
                        : 'Pending'}
                    </span>
                  </td>
                )}

                {/* Actions (only for upcoming) */}
                {type === 'upcoming' && (
                  <td className="py-3 px-4">
                    <div className="flex gap-2 items-center">
                      <button
                        onClick={() => handleRescheduleClick(appointment)}
                        className="text-sm text-blue-600 hover:text-blue-700 font-medium hover:bg-blue-50 px-2 py-1 rounded transition-colors"
                      >
                        Reschedule
                      </button>
                      <button
                        onClick={() => handleCancelClick(appointment)}
                        className="text-sm text-red-600 hover:text-red-700 font-medium hover:bg-red-50 px-2 py-1 rounded transition-colors"
                      >
                        Cancel
                      </button>
                    </div>
                  </td>
                )}
              </tr>
            ))}
          </tbody>
        </table>
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
    <div className="min-h-screen bg-gray-100">
      {/* Page Header */}
      <div className="bg-white border-b border-gray-200 px-6 py-6">
        <div className="flex items-center justify-between">
          <h1 className="text-3xl font-semibold text-gray-900">My appointments</h1>
          <button
            onClick={() => navigate('/providers')}
            className="px-5 py-2.5 bg-blue-600 text-white rounded-md font-medium hover:bg-blue-700 transition-colors focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
          >
            Book new
          </button>
        </div>
      </div>

      {/* Main Content */}
      <main className="p-6">
        {/* Tabs */}
        <div className="flex border-b border-gray-200 mb-6 bg-white">
          <button
            onClick={() => setActiveTab("upcoming")}
            className={`px-6 py-3 text-sm font-medium border-b-2 transition-colors ${
              activeTab === "upcoming"
                ? "border-blue-600 text-blue-600"
                : "border-transparent text-gray-500 hover:text-gray-700 hover:bg-gray-50"
            }`}
            aria-current={activeTab === "upcoming" ? "page" : undefined}
          >
            Upcoming
            <span className={`ml-2 inline-flex items-center justify-center min-w-[20px] h-5 px-2 rounded-full text-xs font-semibold ${
              activeTab === "upcoming"
                ? "bg-blue-50 text-blue-600"
                : "bg-gray-100 text-gray-600"
            }`}>
              {upcomingAppointments.length}
            </span>
          </button>
          <button
            onClick={() => setActiveTab("past")}
            className={`px-6 py-3 text-sm font-medium border-b-2 transition-colors ${
              activeTab === "past"
                ? "border-blue-600 text-blue-600"
                : "border-transparent text-gray-500 hover:text-gray-700 hover:bg-gray-50"
            }`}
            aria-current={activeTab === "past" ? "page" : undefined}
          >
            Past
            <span className={`ml-2 inline-flex items-center justify-center min-w-[20px] h-5 px-2 rounded-full text-xs font-semibold ${
              activeTab === "past"
                ? "bg-blue-50 text-blue-600"
                : "bg-gray-100 text-gray-600"
            }`}>
              {pastAppointments.length}
            </span>
          </button>
          <button
            onClick={() => setActiveTab("waitlist")}
            className={`px-6 py-3 text-sm font-medium border-b-2 transition-colors ${
              activeTab === "waitlist"
                ? "border-blue-600 text-blue-600"
                : "border-transparent text-gray-500 hover:text-gray-700 hover:bg-gray-50"
            }`}
            aria-current={activeTab === "waitlist" ? "page" : undefined}
          >
            Waitlist
            <span className={`ml-2 inline-flex items-center justify-center min-w-[20px] h-5 px-2 rounded-full text-xs font-semibold ${
              activeTab === "waitlist"
                ? "bg-blue-50 text-blue-600"
                : "bg-gray-100 text-gray-600"
            }`}>
              {waitlistEntries.length}
            </span>
          </button>
        </div>

        {/* Tab Content */}
        <div className="bg-white border border-gray-200 rounded-md shadow-sm">
          {renderTabContent()}
        </div>
      </main>

      {/* Cancel Confirmation Modal */}
      {cancelModalOpen && selectedAppointment && (
        <div
          className="fixed inset-0 bg-gray-900 bg-opacity-50 flex items-center justify-center z-50"
          onClick={(e) => {
            if (e.target === e.currentTarget && !isCancelling) {
              setCancelModalOpen(false);
            }
          }}
          onKeyDown={(e) => {
            if (e.key === 'Escape' && !isCancelling) {
              setCancelModalOpen(false);
            }
          }}
          role="dialog"
          aria-modal="true"
          aria-labelledby="cancel-modal-title"
        >
          <div className="bg-white rounded-lg p-6 w-full max-w-md shadow-lg">
            <h3 id="cancel-modal-title" className="text-lg font-semibold text-gray-900 mb-2">
              Cancel appointment?
            </h3>
            <p className="text-sm text-gray-600 mb-6">
              Are you sure you want to cancel your appointment with{' '}
              {selectedAppointment.providerName} on{' '}
              {new Date(selectedAppointment.scheduledDateTime).toLocaleDateString('en-US', {
                month: 'short',
                day: 'numeric',
                year: 'numeric',
              })}
              ? Cancellations within 24 hours of the appointment may be subject to a fee.
            </p>
            <div className="flex gap-3 justify-end">
              <button
                onClick={() => setCancelModalOpen(false)}
                disabled={isCancelling}
                className="px-4 py-2 border border-gray-300 rounded-md text-gray-700 font-medium hover:bg-gray-50 transition-colors focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Keep appointment
              </button>
              <button
                onClick={handleCancelConfirm}
                disabled={isCancelling}
                className="px-4 py-2 bg-red-600 text-white rounded-md font-medium hover:bg-red-700 transition-colors focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isCancelling ? 'Cancelling...' : 'Cancel appointment'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Waitlist Enrollment Modal */}
      <WaitlistEnrollmentModal />

      {/* Reschedule Modal (US_027) */}
      <RescheduleModal
        isOpen={rescheduleModalOpen}
        appointment={rescheduleAppointment}
        availableSlots={availableSlots}
        onConfirm={handleRescheduleConfirm}
        onCancel={handleRescheduleCancel}
        isRescheduling={isRescheduling}
        isLoadingSlots={isLoadingSlots}
      />

      {/* Toast Notification (US_027) */}
      {toast && (
        <div
          className="fixed top-6 right-6 max-w-md w-full bg-neutral-0 border rounded-lg shadow-lg 
                        animate-slide-down z-50"
          role="alert"
          aria-live="polite"
        >
          <div
            className={`flex items-start gap-3 p-4 border-l-4 ${
              toast.type === "success"
                ? "border-green-500 bg-green-50"
                : "border-red-500 bg-red-50"
            }`}
          >
            {/* Icon */}
            <div className="flex-shrink-0">
              {toast.type === "success" ? (
                <svg
                  className="w-6 h-6 text-green-600"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                  aria-hidden="true"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"
                  />
                </svg>
              ) : (
                <svg
                  className="w-6 h-6 text-red-600"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                  aria-hidden="true"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                  />
                </svg>
              )}
            </div>

            {/* Message */}
            <div className="flex-1">
              <p
                className={`text-sm font-medium ${
                  toast.type === "success" ? "text-green-800" : "text-red-800"
                }`}
              >
                {toast.type === "success" ? "Success" : "Error"}
              </p>
              <p className="mt-1 text-sm text-gray-700">{toast.message}</p>
            </div>

            {/* Close button */}
            <button
              onClick={handleCloseToast}
              className="flex-shrink-0 text-gray-400 hover:text-gray-600 
                                focus:outline-none focus:ring-2 focus:ring-blue-500 rounded p-1 
                                transition-colors"
              aria-label="Close notification"
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

export default MyAppointments;
