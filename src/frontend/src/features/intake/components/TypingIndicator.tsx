/**
 * TypingIndicator component (US_033)
 * Animated three-dot typing indicator for AI responses
 * Accessible with ARIA label (UXR-207)
 */

import { memo } from 'react';

/**
 * TypingIndicator - Shows when AI is processing/typing
 */
function TypingIndicator() {
  return (
    <div
      className="flex items-start gap-3"
      role="status"
      aria-label="AI is typing"
      aria-live="polite"
    >
      {/* AI Avatar */}
      <div
        className="w-8 h-8 rounded-full bg-primary-100 text-primary-700 flex items-center justify-center flex-shrink-0"
        aria-hidden="true"
      >
        <svg
          className="w-5 h-5"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M9.75 17L9 20l-1 1h8l-1-1-.75-3M3 13h18M5 17h14a2 2 0 002-2V5a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z"
          />
        </svg>
      </div>

      {/* Typing bubble */}
      <div className="flex flex-col">
        <span className="text-xs text-neutral-500 mb-1">AI Assistant</span>
        <div className="px-4 py-3 bg-neutral-100 rounded-2xl rounded-tl-sm">
          <div className="flex items-center gap-1">
            <span
              className="w-2 h-2 bg-neutral-400 rounded-full animate-typing-dot"
              style={{ animationDelay: '0ms' }}
            />
            <span
              className="w-2 h-2 bg-neutral-400 rounded-full animate-typing-dot"
              style={{ animationDelay: '200ms' }}
            />
            <span
              className="w-2 h-2 bg-neutral-400 rounded-full animate-typing-dot"
              style={{ animationDelay: '400ms' }}
            />
          </div>
        </div>
      </div>

      {/* CSS animation styles injected via style tag */}
      <style>{`
        @keyframes typing-dot {
          0%, 60%, 100% {
            transform: translateY(0);
            opacity: 0.4;
          }
          30% {
            transform: translateY(-4px);
            opacity: 1;
          }
        }
        .animate-typing-dot {
          animation: typing-dot 1.4s infinite ease-in-out;
        }
      `}</style>
    </div>
  );
}

export default memo(TypingIndicator);
