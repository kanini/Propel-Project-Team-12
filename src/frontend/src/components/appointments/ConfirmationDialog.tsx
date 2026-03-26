/**
 * ConfirmationDialog Component for US_024 - Appointment Booking Calendar
 * Displays booking confirmation with appointment details (AC-3, FR-012)
 * Provides calendar export and PDF download options (US_028 - AC-4)
 * Implements Google Calendar OAuth2 integration (US_039 - AC-4)
 */

import { useState, useEffect } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { Link, useNavigate } from 'react-router-dom';
import type { RootState, AppDispatch } from '../../store';
import { downloadConfirmationPDF, resetBooking } from '../../store/slices/appointmentSlice';
import { getCalendarStatus, getCalendarConnectUrl } from '../../api/calendarApi';

/**
 * ConfirmationDialog shows booking success with appointment details (AC-3, FR-012)
 */
export function ConfirmationDialog() {
    const navigate = useNavigate();
    const dispatch = useDispatch<AppDispatch>();
    const { bookingConfirmation } = useSelector(
        (state: RootState) => state.appointments
    );

    const [isDownloadingPDF, setIsDownloadingPDF] = useState(false);
    const [pdfError, setPdfError] = useState<string | null>(null);

    // US_039/US_040: Multi-provider calendar connection state (AC-4)
    const [googleConnected, setGoogleConnected] = useState<boolean>(false);
    const [outlookConnected, setOutlookConnected] = useState<boolean>(false);
    const [calendarLoading, setCalendarLoading] = useState(true);
    const [calendarError, setCalendarError] = useState<string | null>(null);
    const [connectingProvider, setConnectingProvider] = useState<'google' | 'outlook' | null>(null);

    if (!bookingConfirmation) {
        return null;
    }

    const { appointment, confirmationCode } = bookingConfirmation;

    /**
     * Check calendar connection status on mount (US_039/US_040 - AC-4)
     * Also checks for OAuth return via URL parameter
     */
    useEffect(() => {
        // Check if returning from OAuth flow
        const urlParams = new URLSearchParams(window.location.search);
        const provider = urlParams.get('provider');
        const connected = urlParams.get('calendar_connected') === 'true';

        if (connected && provider) {
            if (provider === 'google') {
                setGoogleConnected(true);
            } else if (provider === 'outlook') {
                setOutlookConnected(true);
            }
            setCalendarLoading(false);
            // Clean up URL params
            window.history.replaceState({}, '', window.location.pathname);
            return;
        }

        // Check current connection status for all providers
        const checkCalendarStatus = async () => {
            try {
                const status = await getCalendarStatus();
                setGoogleConnected(status.google.isConnected);
                setOutlookConnected(status.outlook.isConnected);
            } catch (error) {
                console.error('Failed to check calendar status:', error);
                setGoogleConnected(false);
                setOutlookConnected(false);
            } finally {
                setCalendarLoading(false);
            }
        };

        checkCalendarStatus();
    }, []);

    /**
     * Format date for display
     */
    const formatDate = (isoString: string): string => {
        const date = new Date(isoString);
        return date.toLocaleDateString('en-US', {
            weekday: 'long',
            year: 'numeric',
            month: 'long',
            day: 'numeric',
        });
    };

    /**
     * Format time for display
     */
    const formatTime = (isoString: string): string => {
        const date = new Date(isoString);
        return date.toLocaleTimeString('en-US', {
            hour: 'numeric',
            minute: '2-digit',
            hour12: true,
        });
    };

    /**
     * Handle Google Calendar OAuth connection (US_039 - AC-4)
     */
    const handleConnectGoogleCalendar = async () => {
        try {
            setCalendarError(null);
            setConnectingProvider('google');
            const { authorizationUrl } = await getCalendarConnectUrl('google');
            // Redirect to Google OAuth consent - callback will return to this page
            window.location.href = authorizationUrl;
        } catch (error) {
            console.error('Failed to initiate Google Calendar connection:', error);
            setCalendarError('Failed to connect to Google Calendar. Please try again.');
            setConnectingProvider(null);
        }
    };

    /**
     * Handle Outlook Calendar OAuth connection (US_040 - AC-4)
     */
    const handleConnectOutlookCalendar = async () => {
        try {
            setCalendarError(null);
            setConnectingProvider('outlook');
            const { authorizationUrl } = await getCalendarConnectUrl('outlook');
            // Redirect to Microsoft OAuth consent - callback will return to this page
            window.location.href = authorizationUrl;
        } catch (error) {
            console.error('Failed to initiate Outlook Calendar connection:', error);
            setCalendarError('Failed to connect to Outlook Calendar. Please try again.');
            setConnectingProvider(null);
        }
    };

    /**
     * Handle PDF download (US_028 - AC-4)
     */
    const handleDownloadPDF = async () => {
        try {
            setIsDownloadingPDF(true);
            setPdfError(null);

            await dispatch(
                downloadConfirmationPDF({
                    appointmentId: appointment.id,
                    confirmationNumber: confirmationCode,
                })
            ).unwrap();

            setIsDownloadingPDF(false);
        } catch (error) {
            setIsDownloadingPDF(false);
            setPdfError(error as string);
        }
    };

    return (
        <div className="max-w-2xl mx-auto">
            {/* Success icon and message */}
            <div className="text-center mb-8">
                <div
                    className="w-16 h-16 bg-success-light rounded-full flex items-center justify-center 
                             mx-auto mb-4"
                    aria-hidden="true"
                >
                    <svg
                        className="w-10 h-10 text-success"
                        fill="none"
                        viewBox="0 0 24 24"
                        stroke="currentColor"
                    >
                        <path
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            strokeWidth={2}
                            d="M5 13l4 4L19 7"
                        />
                    </svg>
                </div>
                <h2 className="text-2xl font-bold text-neutral-900 mb-2">
                    Appointment Confirmed!
                </h2>
                <p className="text-neutral-600">
                    Your appointment has been successfully booked.
                </p>
            </div>

            {/* Appointment details card */}
            <div className="bg-neutral-0 border border-neutral-200 rounded-lg shadow-sm p-6 mb-6">
                <h3 className="text-lg font-semibold text-neutral-900 mb-4">
                    Appointment Details
                </h3>

                <div className="space-y-3">
                    <div className="flex justify-between py-2 border-b border-neutral-100">
                        <span className="text-sm text-neutral-500">Provider</span>
                        <span className="text-sm font-medium text-neutral-800">
                            {appointment.providerName}
                        </span>
                    </div>
                    <div className="flex justify-between py-2 border-b border-neutral-100">
                        <span className="text-sm text-neutral-500">Specialty</span>
                        <span className="text-sm font-medium text-neutral-800">
                            {appointment.providerSpecialty}
                        </span>
                    </div>
                    <div className="flex justify-between py-2 border-b border-neutral-100">
                        <span className="text-sm text-neutral-500">Date</span>
                        <span className="text-sm font-medium text-neutral-800">
                            {formatDate(appointment.scheduledDateTime)}
                        </span>
                    </div>
                    <div className="flex justify-between py-2 border-b border-neutral-100">
                        <span className="text-sm text-neutral-500">Time</span>
                        <span className="text-sm font-medium text-neutral-800">
                            {formatTime(appointment.scheduledDateTime)}
                        </span>
                    </div>
                    <div className="flex justify-between py-2 border-b border-neutral-100">
                        <span className="text-sm text-neutral-500">Visit Reason</span>
                        <span className="text-sm font-medium text-neutral-800">
                            {appointment.visitReason}
                        </span>
                    </div>
                    <div className="flex justify-between py-2">
                        <span className="text-sm text-neutral-500">
                            Confirmation Code
                        </span>
                        <span className="text-sm font-mono font-medium text-primary-500">
                            {confirmationCode}
                        </span>
                    </div>
                </div>
            </div>

            {/* Action buttons */}
            <div className="space-y-3 mb-8">
                {/* Calendar error message */}
                {calendarError && (
                    <div className="rounded-lg bg-error-light p-3 border border-error-200">
                        <p className="text-sm text-error">{calendarError}</p>
                    </div>
                )}

                {/* Add to Calendar buttons */}
                <div className="grid grid-cols-2 gap-3">
                    {/* Google Calendar - OAuth-aware */}
                    {calendarLoading ? (
                        <div className="h-11 bg-neutral-100 animate-pulse rounded-lg" />
                    ) : googleConnected ? (
                        <div
                            className="inline-flex items-center justify-center gap-2 h-11 px-4 
                                     text-sm font-medium text-success bg-success/10 rounded-lg"
                        >
                            <svg
                                className="w-5 h-5"
                                fill="none"
                                viewBox="0 0 24 24"
                                stroke="currentColor"
                                aria-hidden="true"
                            >
                                <path
                                    strokeLinecap="round"
                                    strokeLinejoin="round"
                                    strokeWidth={2}
                                    d="M5 13l4 4L19 7"
                                />
                            </svg>
                            Synced to Google
                        </div>
                    ) : connectingProvider === 'google' ? (
                        <button
                            disabled
                            className="inline-flex items-center justify-center gap-2 h-11 px-4 
                                     border border-neutral-300 rounded-lg text-sm font-medium 
                                     text-neutral-700 bg-neutral-0 opacity-50 cursor-not-allowed"
                        >
                            <svg
                                className="w-5 h-5 animate-spin"
                                fill="none"
                                viewBox="0 0 24 24"
                                aria-hidden="true"
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
                            Connecting...
                        </button>
                    ) : (
                        <button
                            onClick={handleConnectGoogleCalendar}
                            className="inline-flex items-center justify-center gap-2 h-11 px-4 
                                     border border-neutral-300 rounded-lg text-sm font-medium 
                                     text-neutral-700 bg-neutral-0 hover:bg-neutral-50 
                                     focus:outline-none focus:ring-2 focus:ring-primary-500 
                                     focus:ring-offset-2 transition-colors"
                        >
                            <svg
                                className="w-5 h-5"
                                fill="currentColor"
                                viewBox="0 0 24 24"
                                aria-hidden="true"
                            >
                                <path d="M12 0C5.4 0 0 5.4 0 12s5.4 12 12 12 12-5.4 12-12S18.6 0 12 0zm5.5 16.5h-11v-9h11v9z" />
                            </svg>
                            Connect Google
                        </button>
                    )}

                    {/* Outlook Calendar - OAuth-aware (US_040) */}
                    {calendarLoading ? (
                        <div className="h-11 bg-neutral-100 animate-pulse rounded-lg" />
                    ) : outlookConnected ? (
                        <div
                            className="inline-flex items-center justify-center gap-2 h-11 px-4 
                                     text-sm font-medium text-success bg-success/10 rounded-lg"
                        >
                            <svg
                                className="w-5 h-5"
                                fill="none"
                                viewBox="0 0 24 24"
                                stroke="currentColor"
                                aria-hidden="true"
                            >
                                <path
                                    strokeLinecap="round"
                                    strokeLinejoin="round"
                                    strokeWidth={2}
                                    d="M5 13l4 4L19 7"
                                />
                            </svg>
                            Synced to Outlook
                        </div>
                    ) : connectingProvider === 'outlook' ? (
                        <button
                            disabled
                            className="inline-flex items-center justify-center gap-2 h-11 px-4 
                                     border border-neutral-300 rounded-lg text-sm font-medium 
                                     text-neutral-700 bg-neutral-0 opacity-50 cursor-not-allowed"
                        >
                            <svg
                                className="w-5 h-5 animate-spin"
                                fill="none"
                                viewBox="0 0 24 24"
                                aria-hidden="true"
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
                            Connecting...
                        </button>
                    ) : (
                        <button
                            onClick={handleConnectOutlookCalendar}
                            className="inline-flex items-center justify-center gap-2 h-11 px-4 
                                     border border-neutral-300 rounded-lg text-sm font-medium 
                                     text-neutral-700 bg-neutral-0 hover:bg-neutral-50 
                                     focus:outline-none focus:ring-2 focus:ring-primary-500 
                                     focus:ring-offset-2 transition-colors"
                        >
                            <svg
                                className="w-5 h-5"
                                fill="currentColor"
                                viewBox="0 0 24 24"
                                aria-hidden="true"
                            >
                                <path d="M7 0h10l5 5v14a3 3 0 01-3 3H5a3 3 0 01-3-3V3a3 3 0 013-3h2zm0 2H5a1 1 0 00-1 1v18a1 1 0 001 1h14a1 1 0 001-1V7h-4a2 2 0 01-2-2V2H7zm9 0v3h3l-3-3zm-6 8v8h4v-8h-4zm6 0v8h2v-8h-2zm-8 0v8h2v-8H8z" />
                            </svg>
                            Connect Outlook
                        </button>
                    )}
                </div>

                {/* Download PDF button (US_028 - AC-4) */}
                <button
                    onClick={handleDownloadPDF}
                    disabled={isDownloadingPDF}
                    className="w-full inline-flex items-center justify-center gap-2 h-11 px-4 
                             border border-neutral-300 rounded-lg text-sm font-medium 
                             text-neutral-700 bg-neutral-0 hover:bg-neutral-50 
                             focus:outline-none focus:ring-2 focus:ring-primary-500 
                             focus:ring-offset-2 transition-colors disabled:opacity-50 
                             disabled:cursor-not-allowed"
                >
                    {isDownloadingPDF ? (
                        <>
                            <svg
                                className="w-5 h-5 animate-spin"
                                fill="none"
                                viewBox="0 0 24 24"
                                aria-hidden="true"
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
                            Downloading PDF...
                        </>
                    ) : (
                        <>
                            <svg
                                className="w-5 h-5"
                                fill="none"
                                viewBox="0 0 24 24"
                                stroke="currentColor"
                                aria-hidden="true"
                            >
                                <path
                                    strokeLinecap="round"
                                    strokeLinejoin="round"
                                    strokeWidth={2}
                                    d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
                                />
                            </svg>
                            Download PDF Confirmation
                        </>
                    )}
                </button>

                {/* PDF error message */}
                {pdfError && (
                    <div className="rounded-lg bg-error-light p-4 border border-error-200">
                        <div className="flex items-start gap-3">
                            <svg
                                className="w-5 h-5 text-error mt-0.5 flex-shrink-0"
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
                                <p className="text-sm font-medium text-error">
                                    {pdfError}
                                </p>
                            </div>
                        </div>
                    </div>
                )}
            </div>

            {/* Navigation buttons */}
            <div className="flex gap-3">
                <button
                    onClick={() => {
                        dispatch(resetBooking());
                        navigate('/providers');
                    }}
                    className="flex-1 inline-flex items-center justify-center h-11 px-6 
                             border border-neutral-300 rounded-lg text-sm font-medium 
                             text-neutral-700 bg-neutral-0 hover:bg-neutral-50 
                             focus:outline-none focus:ring-2 focus:ring-primary-500 
                             focus:ring-offset-2 transition-colors"
                >
                    Book Another Appointment
                </button>
                <Link
                    to="/appointments"
                    className="flex-1 inline-flex items-center justify-center h-11 px-6 
                             bg-primary-500 text-neutral-0 rounded-lg text-sm font-medium 
                             hover:bg-primary-600 focus:outline-none focus:ring-2 
                             focus:ring-primary-500 focus:ring-offset-2 transition-colors"
                >
                    My Appointments
                </Link>
            </div>
        </div>
    );
}
