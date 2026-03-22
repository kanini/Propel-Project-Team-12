/**
 * CreatePatientModal Component (US_029, AC2)
 * Modal form for creating new patient with minimal information
 */

import { useState } from "react";
import { createPatient } from "../../../api/staffApi";
import type {
  CreatePatientData,
  PatientSearchResult,
} from "../../../types/staff";

interface CreatePatientModalProps {
  isOpen: boolean;
  onClose: () => void;
  onPatientCreated: (patient: PatientSearchResult) => void;
}

export function CreatePatientModal({
  isOpen,
  onClose,
  onPatientCreated,
}: CreatePatientModalProps) {
  const [formData, setFormData] = useState<CreatePatientData>({
    firstName: "",
    lastName: "",
    dateOfBirth: "",
    phone: "",
    email: "",
  });
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [validationErrors, setValidationErrors] = useState<
    Record<string, string>
  >({});

  if (!isOpen) return null;

  const validateForm = (): boolean => {
    const errors: Record<string, string> = {};

    // Name validation
    if (!formData.firstName.trim()) {
      errors.firstName = "First name is required";
    } else if (formData.firstName.trim().length < 2) {
      errors.firstName = "First name must be at least 2 characters";
    }

    if (!formData.lastName.trim()) {
      errors.lastName = "Last name is required";
    } else if (formData.lastName.trim().length < 2) {
      errors.lastName = "Last name must be at least 2 characters";
    }

    // Date of birth validation
    if (!formData.dateOfBirth) {
      errors.dateOfBirth = "Date of birth is required";
    } else {
      const dob = new Date(formData.dateOfBirth);
      const today = new Date();
      const age = today.getFullYear() - dob.getFullYear();
      if (age < 0 || age > 150) {
        errors.dateOfBirth = "Please enter a valid date of birth";
      }
    }

    // Phone validation (US format: 10 digits)
    if (!formData.phone.trim()) {
      errors.phone = "Phone number is required";
    } else {
      const phoneDigits = formData.phone.replace(/\D/g, "");
      if (phoneDigits.length !== 10) {
        errors.phone = "Phone number must be 10 digits";
      }
    }

    // Email validation (optional but must be valid if provided)
    if (formData.email && formData.email.trim()) {
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      if (!emailRegex.test(formData.email)) {
        errors.email = "Please enter a valid email address";
      }
    }

    setValidationErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    setIsSubmitting(true);
    setError(null);

    try {
      const patient = await createPatient(formData);
      onPatientCreated(patient);
      handleClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to create patient");
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleClose = () => {
    setFormData({
      firstName: "",
      lastName: "",
      dateOfBirth: "",
      phone: "",
      email: "",
    });
    setValidationErrors({});
    setError(null);
    onClose();
  };

  const handlePhoneChange = (value: string) => {
    // Auto-format phone number as user types
    const digits = value.replace(/\D/g, "");
    let formatted = digits;

    if (digits.length > 3 && digits.length <= 6) {
      formatted = `(${digits.slice(0, 3)}) ${digits.slice(3)}`;
    } else if (digits.length > 6) {
      formatted = `(${digits.slice(0, 3)}) ${digits.slice(3, 6)}-${digits.slice(6, 10)}`;
    }

    setFormData({ ...formData, phone: formatted });
  };

  return (
    <div
      className="fixed inset-0 z-50 overflow-y-auto"
      aria-labelledby="modal-title"
      role="dialog"
      aria-modal="true"
    >
      {/* Backdrop */}
      <div className="flex items-center justify-center min-h-screen px-4 pt-4 pb-20 text-center sm:block sm:p-0">
        <div
          className="fixed inset-0 transition-opacity bg-neutral-900 bg-opacity-75"
          aria-hidden="true"
          onClick={handleClose}
        ></div>

        {/* Modal panel */}
        <div className="inline-block w-full max-w-md p-6 my-8 overflow-hidden text-left align-middle transition-all transform bg-white shadow-xl rounded-2xl">
          {/* Header */}
          <div className="flex items-center justify-between mb-4">
            <h3
              className="text-xl font-semibold text-neutral-900"
              id="modal-title"
            >
              Create New Patient
            </h3>
            <button
              type="button"
              onClick={handleClose}
              className="text-neutral-400 hover:text-neutral-600 transition-colors"
              aria-label="Close modal"
            >
              <svg
                className="w-6 h-6"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
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

          {/* Error message */}
          {error && (
            <div
              className="mb-4 p-3 bg-error-50 border border-error-200 rounded-lg text-sm text-error-700"
              role="alert"
            >
              {error}
            </div>
          )}

          {/* Form */}
          <form onSubmit={handleSubmit} className="space-y-4">
            {/* First Name */}
            <div>
              <label
                htmlFor="firstName"
                className="block text-sm font-medium text-neutral-700 mb-1"
              >
                First Name <span className="text-error-500">*</span>
              </label>
              <input
                type="text"
                id="firstName"
                value={formData.firstName}
                onChange={(e) =>
                  setFormData({ ...formData, firstName: e.target.value })
                }
                className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500 transition-colors ${
                  validationErrors.firstName
                    ? "border-error-300"
                    : "border-neutral-300"
                }`}
                aria-invalid={!!validationErrors.firstName}
                aria-describedby={
                  validationErrors.firstName ? "firstName-error" : undefined
                }
              />
              {validationErrors.firstName && (
                <p className="mt-1 text-sm text-error-600" id="firstName-error">
                  {validationErrors.firstName}
                </p>
              )}
            </div>

            {/* Last Name */}
            <div>
              <label
                htmlFor="lastName"
                className="block text-sm font-medium text-neutral-700 mb-1"
              >
                Last Name <span className="text-error-500">*</span>
              </label>
              <input
                type="text"
                id="lastName"
                value={formData.lastName}
                onChange={(e) =>
                  setFormData({ ...formData, lastName: e.target.value })
                }
                className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500 transition-colors ${
                  validationErrors.lastName
                    ? "border-error-300"
                    : "border-neutral-300"
                }`}
                aria-invalid={!!validationErrors.lastName}
                aria-describedby={
                  validationErrors.lastName ? "lastName-error" : undefined
                }
              />
              {validationErrors.lastName && (
                <p className="mt-1 text-sm text-error-600" id="lastName-error">
                  {validationErrors.lastName}
                </p>
              )}
            </div>

            {/* Date of Birth */}
            <div>
              <label
                htmlFor="dateOfBirth"
                className="block text-sm font-medium text-neutral-700 mb-1"
              >
                Date of Birth <span className="text-error-500">*</span>
              </label>
              <input
                type="date"
                id="dateOfBirth"
                value={formData.dateOfBirth}
                onChange={(e) =>
                  setFormData({ ...formData, dateOfBirth: e.target.value })
                }
                max={new Date().toISOString().split("T")[0]}
                className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500 transition-colors ${
                  validationErrors.dateOfBirth
                    ? "border-error-300"
                    : "border-neutral-300"
                }`}
                aria-invalid={!!validationErrors.dateOfBirth}
                aria-describedby={
                  validationErrors.dateOfBirth ? "dateOfBirth-error" : undefined
                }
              />
              {validationErrors.dateOfBirth && (
                <p
                  className="mt-1 text-sm text-error-600"
                  id="dateOfBirth-error"
                >
                  {validationErrors.dateOfBirth}
                </p>
              )}
            </div>

            {/* Phone */}
            <div>
              <label
                htmlFor="phone"
                className="block text-sm font-medium text-neutral-700 mb-1"
              >
                Phone Number <span className="text-error-500">*</span>
              </label>
              <input
                type="tel"
                id="phone"
                value={formData.phone}
                onChange={(e) => handlePhoneChange(e.target.value)}
                placeholder="(555) 555-5555"
                className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500 transition-colors ${
                  validationErrors.phone
                    ? "border-error-300"
                    : "border-neutral-300"
                }`}
                aria-invalid={!!validationErrors.phone}
                aria-describedby={
                  validationErrors.phone ? "phone-error" : undefined
                }
              />
              {validationErrors.phone && (
                <p className="mt-1 text-sm text-error-600" id="phone-error">
                  {validationErrors.phone}
                </p>
              )}
            </div>

            {/* Email (Optional) */}
            <div>
              <label
                htmlFor="email"
                className="block text-sm font-medium text-neutral-700 mb-1"
              >
                Email{" "}
                <span className="text-neutral-400 text-xs">(Optional)</span>
              </label>
              <input
                type="email"
                id="email"
                value={formData.email}
                onChange={(e) =>
                  setFormData({ ...formData, email: e.target.value })
                }
                placeholder="patient@example.com"
                className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500 transition-colors ${
                  validationErrors.email
                    ? "border-error-300"
                    : "border-neutral-300"
                }`}
                aria-invalid={!!validationErrors.email}
                aria-describedby={
                  validationErrors.email ? "email-error" : undefined
                }
              />
              {validationErrors.email && (
                <p className="mt-1 text-sm text-error-600" id="email-error">
                  {validationErrors.email}
                </p>
              )}
            </div>

            {/* Action Buttons */}
            <div className="flex gap-3 pt-4">
              <button
                type="button"
                onClick={handleClose}
                className="flex-1 px-4 py-2 border border-neutral-300 text-neutral-700 rounded-lg hover:bg-neutral-50 transition-colors font-medium"
                disabled={isSubmitting}
              >
                Cancel
              </button>
              <button
                type="submit"
                disabled={isSubmitting}
                className="flex-1 px-4 py-2 bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50 disabled:cursor-not-allowed transition-colors font-medium"
              >
                {isSubmitting ? "Creating..." : "Create Patient"}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}
