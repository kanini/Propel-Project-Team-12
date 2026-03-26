/**
 * WaitlistResponsePage - Handle waitlist slot notification confirm/decline actions (US_041)
 * AllowAnonymous page accessed via email/SMS links with response token
 * Routes: /waitlist/response/:token?action=confirm|decline
 */

import { useEffect, useState } from 'react';
import { useParams, useSearchParams, useNavigate } from 'react-router-dom';
import { confirmWaitlistSlot, declineWaitlistSlot } from '../api/waitlistApi';
import type { ConfirmWaitlistResponse } from '../api/waitlistApi';

type ActionType = 'confirm' | 'decline';

export const WaitlistResponsePage = () => {
    const { token } = useParams<{ token: string }>();
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();

    const action = searchParams.get('action') as ActionType;

    const [isProcessing, setIsProcessing] = useState(true);
    const [result, setResult] = useState<{
        success: boolean;
        message: string;
        appointmentDetails?: ConfirmWaitlistResponse['appointmentDetails'];
    } | null>(null);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        if (!token || !action) {
            setError('Invalid or missing notification link parameters.');
            setIsProcessing(false);
            return;
        }

        const processResponse = async () => {
            try {
                setIsProcessing(true);
                setError(null);

                if (action === 'confirm') {
                    const response = await confirmWaitlistSlot(token);
                    setResult({
                        success: response.success,
                        message: response.message,
                        appointmentDetails: response.appointmentDetails,
                    });
                } else if (action === 'decline') {
                    const response = await declineWaitlistSlot(token);
                    setResult({
                        success: true,
                        message: response.message,
                    });
                } else {
                    setError('Invalid action specified. Please use the link from your notification.');
                }
            } catch (err) {
                console.error('Error processing waitlist response:', err);
                setError(err instanceof Error ? err.message : 'An unexpected error occurred.');
            } finally {
                setIsProcessing(false);
            }
        };

        processResponse();
    }, [token, action]);

    /**
     * Render loading state
     */
    if (isProcessing) {
        return (
            <div className="min-h-screen bg-neutral-100 flex items-center justify-center p-4">
                <div className="max-w-md w-full bg-white rounded-lg shadow-sm border border-neutral-200 p-8">
                    <div className="flex flex-col items-center">
                        <div className="w-16 h-16 border-4 border-primary-500 border-t-transparent rounded-full animate-spin" />
                        <h2 className="mt-6 text-xl font-semibold text-neutral-900">
                            Processing your response...
                        </h2>
                        <p className="mt-2 text-sm text-neutral-600 text-center">
                            Please wait while we {action === 'confirm' ? 'book your appointment' : 'update your waitlist status'}.
                        </p>
                    </div>
                </div>
            </div>
        );
    }

    /**
     * Render error state
     */
    if (error) {
        return (
            <div className="min-h-screen bg-neutral-100 flex items-center justify-center p-4">
                <div className="max-w-md w-full bg-white rounded-lg shadow-sm border border-neutral-200 p-8">
                    <div className="flex flex-col items-center">
                        <div className="w-16 h-16 bg-error-light rounded-full flex items-center justify-center">
                            <svg className="w-10 h-10 text-error" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                            </svg>
                        </div>
                        <h2 className="mt-6 text-xl font-semibold text-neutral-900">
                            Unable to Process Request
                        </h2>
                        <p className="mt-3 text-sm text-neutral-600 text-center">
                            {error}
                        </p>
                        <div className="mt-6 flex gap-3">
                            <button
                                onClick={() => window.location.reload()}
                                className="px-4 py-2 border border-neutral-300 rounded-md text-sm font-medium text-neutral-700 bg-white hover:bg-neutral-50 transition-colors"
                            >
                                Try Again
                            </button>
                            <button
                                onClick={() => navigate('/')}
                                className="px-4 py-2 bg-primary-500 text-white rounded-md text-sm font-medium hover:bg-primary-600 transition-colors"
                            >
                                Go to Home
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        );
    }

    /**
     * Render success state - Confirm
     */
    if (result && action === 'confirm' && result.success && result.appointmentDetails) {
        return (
            <div className="min-h-screen bg-neutral-100 flex items-center justify-center p-4">
                <div className="max-w-md w-full bg-white rounded-lg shadow-sm border border-neutral-200 p-8">
                    <div className="flex flex-col items-center">
                        <div className="w-16 h-16 bg-success-light rounded-full flex items-center justify-center">
                            <svg className="w-10 h-10 text-success" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                            </svg>
                        </div>
                        <h2 className="mt-6 text-2xl font-bold text-neutral-900">
                            Appointment Confirmed!
                        </h2>
                        <p className="mt-2 text-sm text-neutral-600 text-center">
                            Your appointment from the waitlist has been successfully booked.
                        </p>

                        {/* Appointment Details Card */}
                        <div className="mt-6 w-full bg-neutral-50 rounded-lg p-4 border border-neutral-200">
                            <h3 className="text-sm font-semibold text-neutral-700 mb-3">Appointment Details</h3>
                            <div className="space-y-2">
                                <div className="flex justify-between text-sm">
                                    <span className="text-neutral-500">Provider</span>
                                    <span className="font-medium text-neutral-900">{result.appointmentDetails.providerName}</span>
                                </div>
                                <div className="flex justify-between text-sm">
                                    <span className="text-neutral-500">Date & Time</span>
                                    <span className="font-medium text-neutral-900">
                                        {new Date(result.appointmentDetails.scheduledDateTime).toLocaleDateString('en-US', {
                                            month: 'short',
                                            day: 'numeric',
                                            year: 'numeric',
                                        })}{' '}
                                        at{' '}
                                        {new Date(result.appointmentDetails.scheduledDateTime).toLocaleTimeString('en-US', {
                                            hour: 'numeric',
                                            minute: '2-digit',
                                            hour12: true,
                                        })}
                                    </span>
                                </div>
                                <div className="flex justify-between text-sm">
                                    <span className="text-neutral-500">Confirmation #</span>
                                    <span className="font-mono font-medium text-primary-500">
                                        {result.appointmentDetails.confirmationCode}
                                    </span>
                                </div>
                            </div>
                        </div>

                        <p className="mt-4 text-xs text-neutral-500 text-center">
                            A confirmation email has been sent to you with appointment details.
                        </p>

                        <div className="mt-6 flex gap-3 w-full">
                            <button
                                onClick={() => navigate('/appointments')}
                                className="flex-1 px-4 py-2 border border-neutral-300 rounded-md text-sm font-medium text-neutral-700 bg-white hover:bg-neutral-50 transition-colors"
                            >
                                View Appointments
                            </button>
                            <button
                                onClick={() => navigate('/')}
                                className="flex-1 px-4 py-2 bg-primary-500 text-white rounded-md text-sm font-medium hover:bg-primary-600 transition-colors"
                            >
                                Go to Dashboard
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        );
    }

    /**
     * Render failure state - Slot unavailable (EC-2 from US_041)
     */
    if (result && action === 'confirm' && !result.success) {
        return (
            <div className="min-h-screen bg-neutral-100 flex items-center justify-center p-4">
                <div className="max-w-md w-full bg-white rounded-lg shadow-sm border border-neutral-200 p-8">
                    <div className="flex flex-col items-center">
                        <div className="w-16 h-16 bg-warning-light rounded-full flex items-center justify-center">
                            <svg className="w-10 h-10 text-warning" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                            </svg>
                        </div>
                        <h2 className="mt-6 text-xl font-semibold text-neutral-900">
                            Slot No Longer Available
                        </h2>
                        <p className="mt-3 text-sm text-neutral-600 text-center">
                            {result.message}
                        </p>
                        <p className="mt-3 text-sm text-neutral-600 text-center">
                            You remain on the waitlist and will be notified when the next matching slot becomes available.
                        </p>
                        <div className="mt-6 flex gap-3 w-full">
                            <button
                                onClick={() => navigate('/appointments')}
                                className="flex-1 px-4 py-2 border border-neutral-300 rounded-md text-sm font-medium text-neutral-700 bg-white hover:bg-neutral-50 transition-colors"
                            >
                                View Waitlist
                            </button>
                            <button
                                onClick={() => navigate('/providers')}
                                className="flex-1 px-4 py-2 bg-primary-500 text-white rounded-md text-sm font-medium hover:bg-primary-600 transition-colors"
                            >
                                Book Appointment
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        );
    }

    /**
     * Render success state - Decline
     */
    if (result && action === 'decline') {
        return (
            <div className="min-h-screen bg-neutral-100 flex items-center justify-center p-4">
                <div className="max-w-md w-full bg-white rounded-lg shadow-sm border border-neutral-200 p-8">
                    <div className="flex flex-col items-center">
                        <div className="w-16 h-16 bg-info-light rounded-full flex items-center justify-center">
                            <svg className="w-10 h-10 text-info" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                            </svg>
                        </div>
                        <h2 className="mt-6 text-xl font-semibold text-neutral-900">
                            Slot Declined
                        </h2>
                        <p className="mt-3 text-sm text-neutral-600 text-center">
                            {result.message}
                        </p>
                        <p className="mt-3 text-sm text-neutral-600 text-center">
                            We'll notify you when the next matching appointment slot becomes available.
                        </p>
                        <div className="mt-6 flex gap-3 w-full">
                            <button
                                onClick={() => navigate('/appointments')}
                                className="flex-1 px-4 py-2 border border-neutral-300 rounded-md text-sm font-medium text-neutral-700 bg-white hover:bg-neutral-50 transition-colors"
                            >
                                View Waitlist
                            </button>
                            <button
                                onClick={() => navigate('/')}
                                className="flex-1 px-4 py-2 bg-primary-500 text-white rounded-md text-sm font-medium hover:bg-primary-600 transition-colors"
                            >
                                Go to Dashboard
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        );
    }

    // Fallback - should never reach here
    return (
        <div className="min-h-screen bg-neutral-100 flex items-center justify-center p-4">
            <div className="max-w-md w-full bg-white rounded-lg shadow-sm border border-neutral-200 p-8">
                <p className="text-center text-neutral-600">Processing...</p>
            </div>
        </div>
    );
};
