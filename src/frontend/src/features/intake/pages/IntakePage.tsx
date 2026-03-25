/**
 * IntakePage (US_033, US_034, US_035)
 * Main intake page with mode toggle between AI conversational and Manual form
 * Implements UXR-101 (progress indicator), UXR-102 (data preservation), UXR-207 (live regions)
 */

import { useEffect, useCallback, useState } from 'react';
import { useParams, useNavigate, useSearchParams } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import type { AppDispatch } from '../../../store';
import {
  startIntake,
  setMode,
  switchMode,
  resetIntake,
  selectIntakeMode,
  selectIntakeStatus,
  selectProgress,
  selectSessionId,
  selectIntakeError,
} from '../../../store/slices/intakeSlice';
import ConversationalIntake from '../components/ConversationalIntake';
import IntakeSummary from '../components/IntakeSummary';
import ManualIntakeForm from '../components/ManualIntakeForm';

/**
 * IntakePage - Intake entry point with AI/Manual mode toggle
 */
export default function IntakePage() {
  const { appointmentId } = useParams<{ appointmentId: string }>();
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const dispatch = useDispatch<AppDispatch>();

  const mode = useSelector(selectIntakeMode);
  const status = useSelector(selectIntakeStatus);
  const progress = useSelector(selectProgress);
  const sessionId = useSelector(selectSessionId);
  const error = useSelector(selectIntakeError);

  const [showSummary, setShowSummary] = useState(false);

  // Initialize intake session on mount
  useEffect(() => {
    if (!appointmentId) {
      navigate('/intake');
      return;
    }

    // Check for mode in URL params
    const urlMode = searchParams.get('mode');
    const initialMode = urlMode === 'manual' ? 'manual' : 'ai';

    // Start new session if not already started
    if (!sessionId) {
      dispatch(
        startIntake({
          appointmentId: appointmentId,
          mode: initialMode,
        })
      );
    }
  }, [appointmentId, sessionId, searchParams, dispatch, navigate]);

  // Check if ready for summary (progress = 100)
  useEffect(() => {
    if (progress >= 100 && mode === 'ai') {
      setShowSummary(true);
    }
  }, [progress, mode]);

  // Handle mode toggle
  const handleModeToggle = useCallback(() => {
    const newMode = mode === 'ai' ? 'manual' : 'ai';

    if (sessionId) {
      // If session exists, call API to switch mode (preserves data)
      dispatch(switchMode(newMode));
    } else {
      // Just update local state
      dispatch(setMode(newMode));
    }

    setShowSummary(false);
  }, [mode, sessionId, dispatch]);

  // Handle suggest manual mode (from fallback banner)
  const handleSuggestManualMode = useCallback(() => {
    if (sessionId) {
      dispatch(switchMode('manual'));
    } else {
      dispatch(setMode('manual'));
    }
    setShowSummary(false);
  }, [sessionId, dispatch]);

  // Handle edit from summary
  const handleEditFromSummary = useCallback(() => {
    setShowSummary(false);
  }, []);

  // Handle successful completion
  const handleIntakeComplete = useCallback(() => {
    // Navigate back to appointment selection with success message
    navigate('/intake?completed=true');
    dispatch(resetIntake());
  }, [navigate, dispatch]);

  // Loading state
  if (status === 'loading' && !sessionId) {
    return (
      <main className="max-w-4xl mx-auto px-4 py-8">
        <div className="flex items-center justify-center py-12">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600" />
          <span className="ml-3 text-neutral-600">Loading intake form...</span>
        </div>
      </main>
    );
  }

  // Error state
  if (status === 'error' && !sessionId) {
    return (
      <main className="max-w-4xl mx-auto px-4 py-8">
        <div
          className="bg-red-50 border border-red-200 rounded-lg p-4"
          role="alert"
        >
          <div className="flex items-center gap-2 mb-2">
            <svg
              className="w-5 h-5 text-red-600"
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
            <span className="font-medium text-red-800">
              Error loading intake
            </span>
          </div>
          <p className="text-sm text-red-700">
            {error || 'Failed to start intake session. Please try again.'}
          </p>
          <button
            type="button"
            onClick={() => navigate('/intake')}
            className="mt-3 text-sm font-medium text-red-700 hover:text-red-800 underline"
          >
            Return to appointments
          </button>
        </div>
      </main>
    );
  }

  // Completed state
  if (status === 'complete') {
    return (
      <main className="max-w-4xl mx-auto px-4 py-8">
        <div className="bg-green-50 border border-green-200 rounded-lg p-8 text-center">
          <div className="w-16 h-16 mx-auto mb-4 rounded-full bg-green-100 flex items-center justify-center">
            <svg
              className="w-8 h-8 text-green-600"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M5 13l4 4L19 7"
              />
            </svg>
          </div>
          <h2 className="text-xl font-semibold text-green-800 mb-2">
            Intake Completed!
          </h2>
          <p className="text-green-700 mb-6">
            Your pre-visit information has been submitted successfully.
          </p>
          <button
            type="button"
            onClick={() => navigate('/dashboard')}
            className="px-6 py-2 bg-green-600 text-white rounded-md font-medium hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-green-500 focus:ring-offset-2 transition-colors"
          >
            Return to Dashboard
          </button>
        </div>
      </main>
    );
  }

  return (
    <main
      className="max-w-4xl mx-auto px-4 py-8"
      role="main"
      aria-label="Pre-visit intake form"
    >
      {/* Header with mode toggle */}
      <div className="mb-6">
        <div className="flex items-center justify-between mb-4">
          <div>
            <h1 className="text-2xl font-bold text-neutral-900">
              Pre-Visit Intake
            </h1>
            <p className="text-neutral-600 mt-1">
              {mode === 'ai'
                ? 'Answer questions conversationally with our AI assistant'
                : 'Fill out the structured intake form'}
            </p>
          </div>

          {/* Mode Toggle */}
          <div className="flex items-center gap-3">
            <span
              className={`text-sm ${
                mode === 'ai' ? 'font-medium text-primary-600' : 'text-neutral-500'
              }`}
            >
              AI Chat
            </span>
            <button
              type="button"
              onClick={handleModeToggle}
              className={`relative inline-flex h-6 w-11 flex-shrink-0 cursor-pointer rounded-full border-2 border-transparent transition-colors duration-200 ease-in-out focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2 ${
                mode === 'manual' ? 'bg-primary-600' : 'bg-neutral-200'
              }`}
              role="switch"
              aria-checked={mode === 'manual'}
              aria-label="Toggle between AI chat and manual form"
            >
              <span
                className={`pointer-events-none inline-block h-5 w-5 transform rounded-full bg-white shadow ring-0 transition duration-200 ease-in-out ${
                  mode === 'manual' ? 'translate-x-5' : 'translate-x-0'
                }`}
              />
            </button>
            <span
              className={`text-sm ${
                mode === 'manual' ? 'font-medium text-primary-600' : 'text-neutral-500'
              }`}
            >
              Form
            </span>
          </div>
        </div>

        {/* Overall progress bar (UXR-101) */}
        <div className="flex items-center gap-4">
          <div className="flex-1 h-2 bg-neutral-100 rounded-full overflow-hidden">
            <div
              className="h-full bg-primary-500 rounded-full transition-all duration-500"
              style={{ width: `${progress}%` }}
              role="progressbar"
              aria-valuenow={progress}
              aria-valuemin={0}
              aria-valuemax={100}
            />
          </div>
          <span className="text-sm text-neutral-600 font-medium">
            {Math.round(progress)}%
          </span>
        </div>
      </div>

      {/* Main content area */}
      <div
        className="min-h-[500px]"
        aria-live="polite"
        aria-atomic="false"
      >
        {showSummary && mode === 'ai' ? (
          <IntakeSummary
            onEdit={handleEditFromSummary}
            onConfirm={handleIntakeComplete}
          />
        ) : mode === 'ai' ? (
          <ConversationalIntake
            onSuggestManualMode={handleSuggestManualMode}
          />
        ) : (
          <ManualIntakeForm onComplete={handleIntakeComplete} />
        )}
      </div>

      {/* Back button */}
      <div className="mt-6">
        <button
          type="button"
          onClick={() => navigate('/intake')}
          className="text-sm text-neutral-600 hover:text-neutral-800 flex items-center gap-1"
        >
          <svg
            className="w-4 h-4"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M15 19l-7-7 7-7"
            />
          </svg>
          Back to appointment selection
        </button>
      </div>
    </main>
  );
}
