/**
 * ModeToggle component (US_035)
 * AI/Manual intake mode switcher with data preservation warning
 */

import { memo, useState, useCallback } from 'react';

export type IntakeMode = 'ai' | 'manual';

interface ModeToggleProps {
  currentMode: IntakeMode;
  onModeChange: (mode: IntakeMode) => void;
  disabled?: boolean;
  hasPartialData?: boolean;
}

/**
 * ModeToggle - Toggle between AI and Manual intake modes
 */
function ModeToggle({
  currentMode,
  onModeChange,
  disabled = false,
  hasPartialData = false,
}: ModeToggleProps) {
  const [showConfirm, setShowConfirm] = useState(false);
  const [pendingMode, setPendingMode] = useState<IntakeMode | null>(null);

  const handleToggle = useCallback(
    (newMode: IntakeMode) => {
      if (newMode === currentMode || disabled) return;

      if (hasPartialData) {
        setPendingMode(newMode);
        setShowConfirm(true);
      } else {
        onModeChange(newMode);
      }
    },
    [currentMode, disabled, hasPartialData, onModeChange]
  );

  const handleConfirm = useCallback(() => {
    if (pendingMode) {
      onModeChange(pendingMode);
    }
    setShowConfirm(false);
    setPendingMode(null);
  }, [pendingMode, onModeChange]);

  const handleCancel = useCallback(() => {
    setShowConfirm(false);
    setPendingMode(null);
  }, []);

  return (
    <>
      <div
        className="inline-flex rounded-lg p-1 bg-neutral-100"
        role="group"
        aria-label="Intake mode selection"
      >
        <button
          type="button"
          onClick={() => handleToggle('ai')}
          disabled={disabled}
          className={`px-4 py-2 text-sm font-medium rounded-md transition-colors ${
            currentMode === 'ai'
              ? 'bg-white text-primary-700 shadow-sm'
              : 'text-neutral-600 hover:text-neutral-900'
          } ${disabled ? 'opacity-50 cursor-not-allowed' : ''}`}
          aria-pressed={currentMode === 'ai'}
        >
          <span className="flex items-center gap-2">
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M8 10h.01M12 10h.01M16 10h.01M9 16H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-5l-5 5v-5z"
              />
            </svg>
            AI Chat
          </span>
        </button>

        <button
          type="button"
          onClick={() => handleToggle('manual')}
          disabled={disabled}
          className={`px-4 py-2 text-sm font-medium rounded-md transition-colors ${
            currentMode === 'manual'
              ? 'bg-white text-primary-700 shadow-sm'
              : 'text-neutral-600 hover:text-neutral-900'
          } ${disabled ? 'opacity-50 cursor-not-allowed' : ''}`}
          aria-pressed={currentMode === 'manual'}
        >
          <span className="flex items-center gap-2">
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
              />
            </svg>
            Form
          </span>
        </button>
      </div>

      {/* Confirmation Dialog */}
      {showConfirm && (
        <div
          className="fixed inset-0 bg-black/50 flex items-center justify-center z-50"
          role="dialog"
          aria-modal="true"
          aria-labelledby="mode-switch-title"
        >
          <div className="bg-white rounded-lg shadow-xl max-w-sm mx-4 p-6">
            <h3
              id="mode-switch-title"
              className="text-lg font-semibold text-neutral-900 mb-2"
            >
              Switch to {pendingMode === 'ai' ? 'AI Chat' : 'Manual Form'}?
            </h3>
            <p className="text-sm text-neutral-600 mb-4">
              Your progress will be preserved. You can continue where you left off
              in the {pendingMode === 'ai' ? 'chat' : 'form'}.
            </p>
            <div className="flex justify-end gap-3">
              <button
                type="button"
                onClick={handleCancel}
                className="px-4 py-2 text-sm text-neutral-600 hover:text-neutral-800"
              >
                Cancel
              </button>
              <button
                type="button"
                onClick={handleConfirm}
                className="px-4 py-2 text-sm bg-primary-600 text-white rounded-md hover:bg-primary-700"
              >
                Switch Mode
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}

export default memo(ModeToggle);
