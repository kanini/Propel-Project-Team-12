/**
 * ChatBubble component (US_033)
 * Renders directional chat bubbles for AI (left) and user (right) messages
 * Accessible with ARIA roles (UXR-207)
 */

import { memo } from 'react';
import type { ChatMessage, ExtractedDataItem } from '../../../types/intake';

interface ChatBubbleProps {
  message: ChatMessage;
  userName?: string;
}

/**
 * Get user initials for avatar
 */
function getInitials(name: string): string {
  return name
    .split(' ')
    .map((n) => n[0])
    .join('')
    .toUpperCase()
    .slice(0, 2);
}

/**
 * Format timestamp to readable time
 */
function formatTime(timestamp: string): string {
  const date = new Date(timestamp);
  return date.toLocaleTimeString('en-US', {
    hour: 'numeric',
    minute: '2-digit',
    hour12: true,
  });
}

/**
 * ChatBubble - Renders a single chat message
 * @param message - The chat message to display
 * @param userName - User's name for avatar initials
 */
function ChatBubble({ message, userName = 'Patient' }: ChatBubbleProps) {
  const isAI = message.sender === 'ai';

  return (
    <div
      className={`flex items-start gap-3 ${isAI ? '' : 'flex-row-reverse'}`}
      role="listitem"
      aria-label={`${isAI ? 'AI Assistant' : userName} said: ${message.content}`}
    >
      {/* Avatar */}
      <div
        className={`w-8 h-8 rounded-full flex items-center justify-center flex-shrink-0 text-sm font-medium ${
          isAI
            ? 'bg-primary-100 text-primary-700'
            : 'bg-neutral-200 text-neutral-700'
        }`}
        aria-hidden="true"
      >
        {isAI ? (
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
        ) : (
          getInitials(userName)
        )}
      </div>

      {/* Message container */}
      <div className={`flex flex-col max-w-[75%] ${isAI ? '' : 'items-end'}`}>
        {/* Sender label */}
        <span className="text-xs text-neutral-500 mb-1">
          {isAI ? 'AI Assistant' : userName}
        </span>

        {/* Message bubble */}
        <div
          className={`px-4 py-2.5 rounded-2xl ${
            isAI
              ? 'bg-neutral-100 text-neutral-800 rounded-tl-sm'
              : 'bg-primary-500 text-white rounded-tr-sm'
          }`}
        >
          <p className="text-sm leading-relaxed whitespace-pre-wrap">
            {message.content}
          </p>
        </div>

        {/* Extracted data indicator (for AI messages) */}
        {isAI && message.extractedData && message.extractedData.length > 0 && (
          <ExtractedDataIndicator extractedData={message.extractedData} />
        )}

        {/* Timestamp */}
        <span className="text-xs text-neutral-400 mt-1">
          {formatTime(message.timestamp)}
        </span>
      </div>
    </div>
  );
}

/**
 * ExtractedDataIndicator - Shows what data was extracted from the message
 */
function ExtractedDataIndicator({
  extractedData,
}: {
  extractedData: ExtractedDataItem[];
}) {
  return (
    <div className="mt-2 px-3 py-2 bg-green-50 border border-green-200 rounded-lg">
      <div className="flex items-center gap-1.5 mb-1">
        <svg
          className="w-4 h-4 text-green-600"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
          aria-hidden="true"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M5 13l4 4L19 7"
          />
        </svg>
        <span className="text-xs font-medium text-green-700">
          Data captured
        </span>
      </div>
      <ul className="space-y-0.5">
        {extractedData.map((item, index) => (
          <li key={index} className="text-xs text-green-600">
            <span className="font-medium">{item.field}:</span> {item.value}
            {item.confidence < 80 && (
              <span className="text-yellow-600 ml-1">
                (needs confirmation)
              </span>
            )}
          </li>
        ))}
      </ul>
    </div>
  );
}

export default memo(ChatBubble);
