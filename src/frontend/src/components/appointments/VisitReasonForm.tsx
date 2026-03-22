/**
 * VisitReasonForm Component for US_024 - Appointment Booking Calendar
 * Collects visit reason and optional preferred slot swap (FR-008, FR-010)
 * Implements inline validation (UXR-601)
 */

import { useState, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import type { AppDispatch, RootState } from '../../store';
import {
    setVisitReason,
    setEnablePreferredSwap,
} from '../../store/slices/appointmentSlice';
import { PreferredSlotSelector } from './PreferredSlotSelector';

/**
 * VisitReasonForm collects visit reason and optional preferred swap (FR-008, FR-010)
 */
export function VisitReasonForm() {
    const dispatch = useDispatch<AppDispatch>();
    const { visitReason, enablePreferredSwap } = useSelector(
        (state: RootState) => state.appointments
    );

    const [reasonValue, setReasonValue] = useState(visitReason);
    const [error, setError] = useState<string | null>(null);
    const [touched, setTouched] = useState(false);

    const MAX_REASON_LENGTH = 200;

    /**
     * Sync local state with Redux on mount
     */
    useEffect(() => {
        setReasonValue(visitReason);
    }, [visitReason]);

    /**
     * Handle visit reason change with validation (UXR-601)
     */
    const handleReasonChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const value = e.target.value;
        setReasonValue(value);

        // Inline validation (UXR-601)
        if (touched) {
            if (!value.trim()) {
                setError('Visit reason is required');
            } else if (value.length > MAX_REASON_LENGTH) {
                setError(`Visit reason must be ${MAX_REASON_LENGTH} characters or less`);
            } else {
                setError(null);
            }
        }

        // Update Redux
        dispatch(setVisitReason(value));
    };

    /**
     * Handle blur for validation trigger
     */
    const handleBlur = () => {
        setTouched(true);

        if (!reasonValue.trim()) {
            setError('Visit reason is required');
        } else if (reasonValue.length > MAX_REASON_LENGTH) {
            setError(`Visit reason must be ${MAX_REASON_LENGTH} characters or less`);
        }
    };

    /**
     * Handle preferred swap toggle
     */
    const handleSwapToggle = (e: React.ChangeEvent<HTMLInputElement>) => {
        const checked = e.target.checked;
        dispatch(setEnablePreferredSwap(checked));
    };

    /**
     * Validate form is ready
     */
    const isValid = (): boolean => {
        return (
            reasonValue.trim().length > 0 &&
            reasonValue.length <= MAX_REASON_LENGTH
        );
    };

    return (
        <div className="bg-neutral-0 border border-neutral-200 rounded-lg shadow-sm p-6">
            <h3 className="text-lg font-semibold text-neutral-900 mb-4">
                Appointment details
            </h3>

            {/* Visit reason input */}
            <div className="mb-4">
                <label
                    htmlFor="visit-reason"
                    className="block text-sm font-medium text-neutral-700 mb-1"
                >
                    Reason for visit <span className="text-error">*</span>
                </label>
                <input
                    type="text"
                    id="visit-reason"
                    value={reasonValue}
                    onChange={handleReasonChange}
                    onBlur={handleBlur}
                    placeholder="e.g., Annual checkup, follow-up, new concern"
                    maxLength={MAX_REASON_LENGTH}
                    className={`
                        w-full h-10 px-3 border rounded-lg text-sm text-neutral-800
                        focus:outline-none focus:ring-2 transition-colors
                        ${error
                            ? 'border-error focus:ring-error focus:border-error'
                            : 'border-neutral-300 focus:ring-primary-500 focus:border-primary-500'
                        }
                    `}
                    aria-invalid={error ? 'true' : 'false'}
                    aria-describedby={error ? 'reason-error' : 'reason-hint'}
                    required
                />

                {/* Character count */}
                <div className="flex justify-between items-center mt-1">
                    {error ? (
                        <p
                            id="reason-error"
                            className="text-xs text-error"
                            role="alert"
                        >
                            {error}
                        </p>
                    ) : (
                        <p id="reason-hint" className="text-xs text-neutral-500">
                            Required field
                        </p>
                    )}
                    <p className="text-xs text-neutral-400">
                        {reasonValue.length}/{MAX_REASON_LENGTH}
                    </p>
                </div>
            </div>

            {/* Visit type select */}
            <div className="mb-5">
                <label
                    htmlFor="visit-type"
                    className="block text-sm font-medium text-neutral-700 mb-1"
                >
                    Visit type
                </label>
                <select
                    id="visit-type"
                    className="w-full h-10 px-3 border border-neutral-300 rounded-lg text-sm 
                             text-neutral-800 bg-neutral-0 focus:outline-none focus:ring-2 
                             focus:ring-primary-500 focus:border-primary-500 transition-colors"
                >
                    <option value="in-person">In-person</option>
                    <option value="telehealth">Telehealth</option>
                </select>
            </div>

            {/* Preferred slot swap checkbox */}
            <div className="flex items-start gap-2 pt-4 border-t border-neutral-100">
                <input
                    type="checkbox"
                    id="swap-toggle"
                    checked={enablePreferredSwap}
                    onChange={handleSwapToggle}
                    className="w-5 h-5 mt-0.5 rounded border-neutral-300 text-primary-500 
                             focus:ring-2 focus:ring-primary-500 focus:ring-offset-2 
                             cursor-pointer"
                />
                <div className="flex-1">
                    <label
                        htmlFor="swap-toggle"
                        className="text-sm text-neutral-700 cursor-pointer select-none"
                    >
                        Enable preferred slot swap
                    </label>
                    <p className="text-xs text-neutral-500 mt-1">
                        Select an earlier booked time slot. If it becomes available,
                        we'll automatically move your appointment and notify you.
                    </p>
                </div>
            </div>

            {/* Show PreferredSlotSelector when swap enabled */}
            {enablePreferredSwap && <PreferredSlotSelector />}

            {/* Validation summary for accessibility */}
            {!isValid() && touched && (
                <div className="sr-only" role="status" aria-live="polite">
                    Please correct the errors in the form
                </div>
            )}
        </div>
    );
}
