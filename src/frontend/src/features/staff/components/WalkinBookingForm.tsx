/**
 * WalkinBookingForm Component for US_029 - Walk-in Booking UI
 * Allows staff to book walk-in appointments for selected patient (AC-3)
 * - Provider selection dropdown
 * - Time slot display for today only
 * - Visit reason input (max 500 chars)
 * - Real-time slot availability
 */

import { useState, useEffect } from "react";
import {
  fetchProviderSlots,
  createWalkinAppointment,
} from "../../../api/staffApi";
import { fetchProviders } from "../../../api/providerApi";
import type {
  PatientSearchResult,
  TimeSlot,
  WalkinAppointmentData,
} from "../../../types/staff";
import type { Provider } from "../../../types/provider";

/**
 * Type guard for non-empty string
 */
function isNonEmptyString(value: string | undefined): value is string {
  return typeof value === "string" && value.length > 0;
}

interface WalkinBookingFormProps {
  /**
   * Selected patient for booking
   */
  patient: PatientSearchResult;
  /**
   * Callback when appointment is successfully created
   */
  onSuccess: (appointmentData: unknown) => void;
  /**
   * Callback when there's an error
   */
  onError: (error: string) => void;
}

/**
 * WalkinBookingForm for immediate appointment booking (US_029, AC3)
 */
export function WalkinBookingForm({
  patient,
  onSuccess,
  onError,
}: WalkinBookingFormProps) {
  const [providers, setProviders] = useState<Provider[]>([]);
  const [selectedProviderId, setSelectedProviderId] = useState("");
  const [timeSlots, setTimeSlots] = useState<TimeSlot[]>([]);
  const [selectedSlotId, setSelectedSlotId] = useState("");
  const [visitReason, setVisitReason] = useState<string>("");

  const [isLoadingProviders, setIsLoadingProviders] = useState(false);
  const [isLoadingSlots, setIsLoadingSlots] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const [formErrors, setFormErrors] = useState<{
    provider?: string;
    slot?: string;
    visitReason?: string;
  }>({});

  /**
   * Fetch providers on mount
   */
  useEffect(() => {
    const loadProviders = async () => {
      setIsLoadingProviders(true);
      try {
        // Fetch first page of providers
        const result = await fetchProviders({}, { page: 1, pageSize: 50 });
        setProviders(result.providers);
      } catch (err) {
        console.error("Failed to fetch providers:", err);
        onError("Failed to load providers. Please refresh and try again.");
      } finally {
        setIsLoadingProviders(false);
      }
    };

    loadProviders();
  }, [onError]);

  /**
   * Fetch time slots when provider is selected
   */
  useEffect(() => {
    if (!selectedProviderId) {
      setTimeSlots([]);
      setSelectedSlotId("");
      return;
    }

    // Capture providerId in a const to maintain type narrowing
    const providerId = selectedProviderId;

    const loadSlots = async () => {
      //Additional guard inside async function
      if (!providerId) return;

      setIsLoadingSlots(true);
      setTimeSlots([]);
      setSelectedSlotId("");

      try {
        // Get today's date in YYYY-MM-DD format
        const today = new Date();
        const dateStr = today.toISOString().split("T")[0]!; // Non-null assertion: split always returns array with at least one element

        // Type guard ensures providerId is string
        if (!isNonEmptyString(providerId)) return;

        const slots = (await fetchProviderSlots(
          providerId,
          dateStr,
        )) as TimeSlot[];

        // Filter only available slots
        const availableSlots = slots.filter(
          (slot) => slot.status === "available",
        );
        setTimeSlots(availableSlots);

        if (availableSlots.length === 0) {
          setFormErrors((prev) => ({
            ...prev,
            slot: "No available slots for this provider today",
          }));
        } else {
          setFormErrors((prev) => ({ ...prev, slot: undefined }));
        }
      } catch (err) {
        console.error("Failed to fetch slots:", err);
        setFormErrors((prev) => ({
          ...prev,
          slot: "Failed to load time slots",
        }));
      } finally {
        setIsLoadingSlots(false);
      }
    };

    loadSlots();
  }, [selectedProviderId]);

  /**
   * Format time slot for display
   */
  const formatSlotTime = (startTime: string, endTime: string): string => {
    const start = new Date(startTime);
    const end = new Date(endTime);

    const formatTime = (date: Date) => {
      return date.toLocaleTimeString("en-US", {
        hour: "numeric",
        minute: "2-digit",
        hour12: true,
      });
    };

    return `${formatTime(start)} - ${formatTime(end)}`;
  };

  /**
   * Validate form before submission
   */
  const validateForm = (): boolean => {
    const errors: typeof formErrors = {};

    if (!selectedProviderId) {
      errors.provider = "Please select a provider";
    }

    if (!selectedSlotId) {
      errors.slot = "Please select a time slot";
    }

    if (!visitReason.trim()) {
      errors.visitReason = "Visit reason is required";
    } else if (visitReason.length > 500) {
      errors.visitReason = "Visit reason must be 500 characters or less";
    }

    setFormErrors(errors);
    return Object.keys(errors).length === 0;
  };

  /**
   * Handle form submission
   */
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    setIsSubmitting(true);

    try {
      const appointmentData: WalkinAppointmentData = {
        patientId: patient.id,
        providerId: selectedProviderId,
        timeSlotId: selectedSlotId,
        visitReason: visitReason.trim(),
      };

      const result = await createWalkinAppointment(appointmentData);
      onSuccess(result);

      // Reset form
      setSelectedProviderId("");
      setSelectedSlotId("");
      setVisitReason("");
      setTimeSlots([]);
      setFormErrors({});
    } catch (err) {
      const errorMessage =
        err instanceof Error ? err.message : "Failed to create appointment";
      onError(errorMessage);
    } finally {
      setIsSubmitting(false);
    }
  };

  /**
   * Handle provider selection
   */
  const handleProviderChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    setSelectedProviderId(e.target.value);
    setFormErrors((prev) => ({ ...prev, provider: undefined }));
  };

  /**
   * Handle slot selection
   */
  const handleSlotSelect = (slotId: string) => {
    setSelectedSlotId(slotId);
    setFormErrors((prev) => ({ ...prev, slot: undefined }));
  };

  /**
   * Handle visit reason change
   */
  const handleVisitReasonChange = (
    e: React.ChangeEvent<HTMLTextAreaElement>,
  ) => {
    const value = e.target.value;
    if (value.length <= 500) {
      setVisitReason(value);
      setFormErrors((prev) => ({ ...prev, visitReason: undefined }));
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      {/* Patient Info Display */}
      <div className="bg-primary-50 border border-primary-200 rounded-lg p-4">
        <h3 className="text-sm font-semibold text-neutral-900 mb-2">
          Booking for:
        </h3>
        <p className="text-sm text-neutral-700">
          <span className="font-medium">{patient.fullName}</span>
          {" • "}
          DOB: {new Date(patient.dateOfBirth).toLocaleDateString("en-US")}
        </p>
        {patient.phone && (
          <p className="text-sm text-neutral-600 mt-1">
            Phone: {patient.phone}
          </p>
        )}
      </div>

      {/* Provider Selection */}
      <div>
        <label
          htmlFor="provider"
          className="block text-sm font-medium text-neutral-900 mb-2"
        >
          Select Provider <span className="text-error">*</span>
        </label>
        {isLoadingProviders ? (
          <div className="h-11 bg-neutral-200 rounded-lg animate-pulse" />
        ) : (
          <select
            id="provider"
            value={selectedProviderId}
            onChange={handleProviderChange}
            className={`w-full h-11 px-3 border rounded-lg text-sm 
                            focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500
                            ${
                              formErrors.provider
                                ? "border-error focus:ring-error"
                                : "border-neutral-300"
                            }`}
            aria-invalid={!!formErrors.provider}
            aria-describedby={
              formErrors.provider ? "provider-error" : undefined
            }
          >
            <option value="">Choose a provider...</option>
            {providers.map((provider) => (
              <option key={provider.id} value={provider.id}>
                {provider.name} - {provider.specialty}
              </option>
            ))}
          </select>
        )}
        {formErrors.provider && (
          <p
            id="provider-error"
            className="mt-1 text-sm text-error"
            role="alert"
          >
            {formErrors.provider}
          </p>
        )}
      </div>

      {/* Time Slots */}
      {selectedProviderId && (
        <div>
          <label className="block text-sm font-medium text-neutral-900 mb-2">
            Available Time Slots (Today) <span className="text-error">*</span>
          </label>

          {isLoadingSlots ? (
            <div className="grid grid-cols-2 md:grid-cols-3 gap-2">
              {Array.from({ length: 6 }).map((_, i) => (
                <div
                  key={i}
                  className="h-11 bg-neutral-200 rounded-lg animate-pulse"
                />
              ))}
            </div>
          ) : timeSlots.length === 0 ? (
            <div className="bg-neutral-50 border border-neutral-200 rounded-lg p-4 text-center">
              <p className="text-sm text-neutral-600">
                No available slots for this provider today
              </p>
            </div>
          ) : (
            <div className="grid grid-cols-2 md:grid-cols-3 gap-2">
              {timeSlots.map((slot) => (
                <button
                  key={slot.id}
                  type="button"
                  onClick={() => handleSlotSelect(slot.id)}
                  className={`h-11 px-3 border rounded-lg text-sm font-medium transition-colors
                                        ${
                                          selectedSlotId === slot.id
                                            ? "bg-primary-500 text-neutral-0 border-primary-500"
                                            : "bg-neutral-0 text-neutral-900 border-neutral-300 hover:border-primary-500 hover:bg-primary-50"
                                        }
                                        focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2`}
                  aria-pressed={selectedSlotId === slot.id}
                >
                  {formatSlotTime(slot.startTime, slot.endTime)}
                </button>
              ))}
            </div>
          )}

          {formErrors.slot && (
            <p id="slot-error" className="mt-2 text-sm text-error" role="alert">
              {formErrors.slot}
            </p>
          )}
        </div>
      )}

      {/* Visit Reason */}
      <div>
        <label
          htmlFor="visitReason"
          className="block text-sm font-medium text-neutral-900 mb-2"
        >
          Visit Reason <span className="text-error">*</span>
        </label>
        <textarea
          id="visitReason"
          value={visitReason}
          onChange={handleVisitReasonChange}
          rows={4}
          placeholder="Enter the reason for this walk-in visit..."
          className={`w-full px-3 py-2 border rounded-lg text-sm resize-none
                        focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500
                        ${
                          formErrors.visitReason
                            ? "border-error focus:ring-error"
                            : "border-neutral-300"
                        }`}
          aria-invalid={!!formErrors.visitReason}
          aria-describedby="visitReason-help visitReason-error"
          maxLength={500}
        />
        <div className="mt-1 flex justify-between items-center">
          <span id="visitReason-help" className="text-xs text-neutral-500">
            {visitReason.length}/500 characters
          </span>
          {formErrors.visitReason && (
            <p
              id="visitReason-error"
              className="text-sm text-error"
              role="alert"
            >
              {formErrors.visitReason}
            </p>
          )}
        </div>
      </div>

      {/* Submit Button */}
      <div className="flex justify-end gap-3 pt-4 border-t border-neutral-200">
        <button
          type="submit"
          disabled={isSubmitting || isLoadingSlots}
          className="px-6 py-2.5 bg-primary-500 text-neutral-0 font-medium rounded-lg
                        hover:bg-primary-600 focus:outline-none focus:ring-2 focus:ring-primary-500 
                        focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed
                        transition-colors"
        >
          {isSubmitting
            ? "Creating Appointment..."
            : "Book Walk-in Appointment"}
        </button>
      </div>
    </form>
  );
}
