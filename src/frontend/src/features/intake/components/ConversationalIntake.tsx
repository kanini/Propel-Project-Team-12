/**
 * ConversationalIntake component (US_033)
 * Main chat interface with message list, input, and auto-scroll
 * Implements AC-1, AC-2, and AC-4 (fallback on low confidence)
 */

import { memo, useCallback, useRef, useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import type { AppDispatch } from '../../../store';
import {
  sendMessage,
  addUserMessage,
  selectMessages,
  selectIsTyping,
  selectConsecutiveFailures,
  selectProgress,
} from '../../../store/slices/intakeSlice';
import ChatBubble from './ChatBubble';
import TypingIndicator from './TypingIndicator';

interface ConversationalIntakeProps {
  userName?: string;
  onSuggestManualMode?: () => void;
}

/**
 * ConversationalIntake - Chat-based intake interface
 * @param userName - Patient's name for display
 * @param onSuggestManualMode - Callback when suggesting manual mode switch
 */
function ConversationalIntake({
  userName = 'Patient',
  onSuggestManualMode,
}: ConversationalIntakeProps) {
  const dispatch = useDispatch<AppDispatch>();
  const messages = useSelector(selectMessages);
  const isTyping = useSelector(selectIsTyping);
  const consecutiveFailures = useSelector(selectConsecutiveFailures);
  const progress = useSelector(selectProgress);

  const [inputValue, setInputValue] = useState('');
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLTextAreaElement>(null);

  // Auto-scroll to bottom on new messages
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages, isTyping]);

  // Focus input on mount
  useEffect(() => {
    inputRef.current?.focus();
  }, []);

  // Handle sending message
  const handleSend = useCallback(() => {
    const trimmedMessage = inputValue.trim();
    if (!trimmedMessage || isTyping) return;

    // Add user message to state
    dispatch(addUserMessage(trimmedMessage));

    // Clear input
    setInputValue('');

    // Send to API
    dispatch(sendMessage({ message: trimmedMessage }));
  }, [inputValue, isTyping, dispatch]);

  // Handle keyboard input (Enter to send)
  const handleKeyDown = useCallback(
    (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
      if (e.key === 'Enter' && !e.shiftKey) {
        e.preventDefault();
        handleSend();
      }
    },
    [handleSend]
  );

  // Handle input change with auto-resize
  const handleInputChange = useCallback(
    (e: React.ChangeEvent<HTMLTextAreaElement>) => {
      setInputValue(e.target.value);
      // Auto-resize textarea
      e.target.style.height = 'auto';
      e.target.style.height = `${Math.min(e.target.scrollHeight, 120)}px`;
    },
    []
  );

  // Show fallback banner after 3 consecutive failures (AC-4 / AIR-S03)
  const showFallbackBanner = consecutiveFailures >= 3;

  return (
    <div className="flex flex-col h-full bg-white rounded-lg shadow-sm border border-neutral-200">
      {/* Progress indicator */}
      <div className="px-4 py-2 border-b border-neutral-100">
        <div className="flex items-center justify-between text-sm">
          <span className="text-neutral-600">Intake progress</span>
          <span className="font-medium text-primary-600">{Math.round(progress)}%</span>
        </div>
        <div className="mt-1 h-2 bg-neutral-100 rounded-full overflow-hidden">
          <div
            className="h-full bg-primary-500 rounded-full transition-all duration-300"
            style={{ width: `${progress}%` }}
            role="progressbar"
            aria-valuenow={progress}
            aria-valuemin={0}
            aria-valuemax={100}
            aria-label={`Intake ${Math.round(progress)}% complete`}
          />
        </div>
      </div>

      {/* Fallback banner (AC-4) */}
      {showFallbackBanner && (
        <div
          className="mx-4 mt-3 p-3 bg-yellow-50 border border-yellow-200 rounded-lg"
          role="alert"
        >
          <div className="flex items-start gap-2">
            <svg
              className="w-5 h-5 text-yellow-600 flex-shrink-0 mt-0.5"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
              aria-hidden="true"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
              />
            </svg>
            <div className="flex-1">
              <p className="text-sm font-medium text-yellow-800">
                Having trouble understanding?
              </p>
              <p className="text-sm text-yellow-700 mt-0.5">
                Would you prefer to fill out a structured form instead? Your data will be preserved.
              </p>
              <button
                type="button"
                onClick={onSuggestManualMode}
                className="mt-2 text-sm font-medium text-yellow-800 hover:text-yellow-900 underline focus:outline-none focus:ring-2 focus:ring-yellow-500 focus:ring-offset-2 rounded"
              >
                Switch to Manual Form
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Messages list */}
      <div
        className="flex-1 overflow-y-auto p-4 space-y-4"
        role="list"
        aria-label="Chat messages"
        aria-live="polite"
        aria-relevant="additions"
      >
        {messages.map((message) => (
          <ChatBubble key={message.id} message={message} userName={userName} />
        ))}

        {/* Typing indicator */}
        {isTyping && <TypingIndicator />}

        {/* Scroll anchor */}
        <div ref={messagesEndRef} />
      </div>

      {/* Input area */}
      <div className="border-t border-neutral-200 p-4">
        <div className="flex items-end gap-3">
          <textarea
            ref={inputRef}
            value={inputValue}
            onChange={handleInputChange}
            onKeyDown={handleKeyDown}
            placeholder="Type your response..."
            className="flex-1 resize-none rounded-lg border border-neutral-300 px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500 min-h-[44px] max-h-[120px]"
            rows={1}
            disabled={isTyping}
            aria-label="Type your message"
          />
          <button
            type="button"
            onClick={handleSend}
            disabled={!inputValue.trim() || isTyping}
            className="px-4 py-2.5 bg-primary-600 text-white rounded-lg font-medium text-sm hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            aria-label="Send message"
          >
            <svg
              className="w-5 h-5"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
              aria-hidden="true"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M12 19l9 2-9-18-9 18 9-2zm0 0v-8"
              />
            </svg>
          </button>
        </div>
        <p className="mt-2 text-xs text-neutral-500">
          Press Enter to send, Shift+Enter for new line
        </p>
      </div>
    </div>
  );
}

export default memo(ConversationalIntake);
