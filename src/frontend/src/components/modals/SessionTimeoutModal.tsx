import React from 'react';

interface SessionTimeoutModalProps {
  isOpen: boolean;
  secondsRemaining: number;
  onExtendSession: () => void;
  onLogout: () => void;
}

/**
 * Session timeout warning modal (US_022, UXR-604).
 * Displays at 13-minute mark with countdown and session extension option.
 */
export const SessionTimeoutModal: React.FC<SessionTimeoutModalProps> = ({
  isOpen,
  secondsRemaining,
  onExtendSession,
  onLogout,
}) => {
  if (!isOpen) return null;

  // Format seconds as MM:SS
  const minutes = Math.floor(secondsRemaining / 60);
  const seconds = secondsRemaining % 60;
  const timeDisplay = `${minutes}:${seconds.toString().padStart(2, '0')}`;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50">
      <div className="bg-white rounded-lg shadow-xl max-w-md w-full mx-4 p-6">
        {/* Icon */}
        <div className="flex items-center justify-center mb-4">
          <div className="w-12 h-12 bg-amber-100 rounded-full flex items-center justify-center">
            <svg
              className="w-6 h-6 text-amber-600"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
              xmlns="http://www.w3.org/2000/svg"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
              />
            </svg>
          </div>
        </div>

        {/* Title */}
        <h2 className="text-xl font-semibold text-gray-900 text-center mb-2">
          Session Expiring Soon
        </h2>

        {/* Message */}
        <p className="text-gray-600 text-center mb-4">
          Your session will expire due to inactivity. You will be automatically logged out in:
        </p>

        {/* Countdown Timer */}
        <div className="bg-gray-50 rounded-lg p-4 mb-6">
          <div className="text-4xl font-bold text-center text-gray-900 font-mono">
            {timeDisplay}
          </div>
        </div>

        {/* Warning Message */}
        <p className="text-sm text-gray-500 text-center mb-6">
          Click "Extend Session" to continue working or "Logout Now" to end your session.
        </p>

        {/* Action Buttons */}
        <div className="flex flex-col sm:flex-row gap-3">
          <button
            onClick={onLogout}
            className="flex-1 px-4 py-2 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500 transition-colors"
          >
            Logout Now
          </button>
          <button
            onClick={onExtendSession}
            className="flex-1 px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-colors font-medium"
          >
            Extend Session
          </button>
        </div>
      </div>
    </div>
  );
};
