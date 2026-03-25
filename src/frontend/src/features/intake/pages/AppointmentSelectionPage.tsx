/**
 * AppointmentSelectionPage (US_037)
 * Entry point for intake flow - displays appointments requiring intake
 * Implements AC-1 through AC-6 with automatic navigation for single appointment
 */

import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import type { AppDispatch } from '../../../store';
import {
  fetchIntakeAppointments,
  selectIntakeAppointments,
  selectIntakeAppointmentsStatus,
  selectIntakeAppointmentsError,
} from '../../../store/slices/intakeAppointmentSlice';
import AppointmentCard from '../components/AppointmentCard';
import AppointmentCardSkeleton from '../components/AppointmentCardSkeleton';
import EmptyStateIntake from '../components/EmptyStateIntake';

/**
 * AppointmentSelectionPage - Main intake entry point
 * AC-1: Display list with provider name, date, time, intake status
 * AC-5: Auto-navigate when single appointment
 * AC-6: Empty state when no appointments
 */
export default function AppointmentSelectionPage() {
  const dispatch = useDispatch<AppDispatch>();
  const navigate = useNavigate();
  const appointments = useSelector(selectIntakeAppointments);
  const status = useSelector(selectIntakeAppointmentsStatus);
  const error = useSelector(selectIntakeAppointmentsError);

  // Fetch appointments on mount
  useEffect(() => {
    dispatch(fetchIntakeAppointments());
  }, [dispatch]);

  // AC-5: Auto-navigate when single appointment exists
  useEffect(() => {
    const firstAppointment = appointments[0];
    if (status === 'succeeded' && appointments.length === 1 && firstAppointment) {
      navigate(`/intake/${firstAppointment.appointmentId}`, { replace: true });
    }
  }, [status, appointments, navigate]);

  // Loading state with skeleton loaders
  if (status === 'loading') {
    return (
      <main
        className="max-w-3xl mx-auto px-4 py-8"
        role="main"
        aria-label="Loading appointments"
        aria-busy="true"
      >
        <h1 className="text-2xl font-bold text-neutral-900 mb-6">
          Pre-Visit Intake
        </h1>
        <p className="text-neutral-600 mb-6">Loading your appointments...</p>
        <div className="space-y-4">
          <AppointmentCardSkeleton />
          <AppointmentCardSkeleton />
          <AppointmentCardSkeleton />
        </div>
      </main>
    );
  }

  // Error state
  if (status === 'failed') {
    return (
      <main
        className="max-w-3xl mx-auto px-4 py-8"
        role="main"
        aria-label="Error loading appointments"
      >
        <h1 className="text-2xl font-bold text-neutral-900 mb-6">
          Pre-Visit Intake
        </h1>
        <div
          className="bg-red-50 border border-red-200 rounded-lg p-4 text-red-800"
          role="alert"
        >
          <div className="flex items-center gap-2 mb-2">
            <svg
              className="w-5 h-5"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
              aria-hidden="true"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
              />
            </svg>
            <span className="font-medium">Error loading appointments</span>
          </div>
          <p className="text-sm">{error || 'An unexpected error occurred. Please try again.'}</p>
          <button
            type="button"
            onClick={() => dispatch(fetchIntakeAppointments())}
            className="mt-3 text-sm font-medium text-red-700 hover:text-red-800 underline focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2 rounded"
          >
            Try again
          </button>
        </div>
      </main>
    );
  }

  // AC-6: Empty state when no appointments
  if (status === 'succeeded' && appointments.length === 0) {
    return (
      <main
        className="max-w-3xl mx-auto px-4 py-8"
        role="main"
        aria-label="No appointments requiring intake"
      >
        <h1 className="text-2xl font-bold text-neutral-900 mb-6">
          Pre-Visit Intake
        </h1>
        <EmptyStateIntake />
      </main>
    );
  }

  // AC-1: Display appointment list (when more than 1 appointment)
  return (
    <main
      className="max-w-3xl mx-auto px-4 py-8"
      role="main"
      aria-label="Select appointment for intake"
    >
      <h1 className="text-2xl font-bold text-neutral-900 mb-2">
        Pre-Visit Intake
      </h1>
      <p className="text-neutral-600 mb-6">
        Select an appointment to complete your intake forms.
      </p>

      {/* Appointment list */}
      <div
        className="space-y-4"
        role="list"
        aria-label="Appointments requiring intake"
      >
        {appointments.map((appointment) => (
          <AppointmentCard
            key={appointment.appointmentId}
            appointment={appointment}
          />
        ))}
      </div>
    </main>
  );
}
