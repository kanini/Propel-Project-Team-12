/**
 * FormTooltip component (US_034, UXR-103)
 * Contextual help tooltips for medical terminology
 */

import { memo, useState, useCallback } from 'react';

interface FormTooltipProps {
  content: string;
  children: React.ReactNode;
}

/**
 * FormTooltip - Accessible tooltip for field help text
 */
function FormTooltip({ content, children }: FormTooltipProps) {
  const [isVisible, setIsVisible] = useState(false);

  const handleMouseEnter = useCallback(() => setIsVisible(true), []);
  const handleMouseLeave = useCallback(() => setIsVisible(false), []);
  const handleFocus = useCallback(() => setIsVisible(true), []);
  const handleBlur = useCallback(() => setIsVisible(false), []);

  return (
    <span className="relative inline-flex items-center">
      {children}
      <button
        type="button"
        className="ml-1 text-neutral-400 hover:text-neutral-600 focus:outline-none focus:ring-2 focus:ring-primary-500 rounded-full"
        onMouseEnter={handleMouseEnter}
        onMouseLeave={handleMouseLeave}
        onFocus={handleFocus}
        onBlur={handleBlur}
        aria-describedby="tooltip"
        aria-label="More information"
      >
        <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
          <path
            fillRule="evenodd"
            d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-8-3a1 1 0 00-.867.5 1 1 0 11-1.731-1A3 3 0 0113 8a3.001 3.001 0 01-2 2.83V11a1 1 0 11-2 0v-1a1 1 0 011-1 1 1 0 100-2zm0 8a1 1 0 100-2 1 1 0 000 2z"
            clipRule="evenodd"
          />
        </svg>
      </button>

      {/* Tooltip bubble */}
      {isVisible && (
        <div
          id="tooltip"
          role="tooltip"
          className="absolute z-10 bottom-full left-1/2 transform -translate-x-1/2 mb-2 px-3 py-2 text-xs text-white bg-neutral-800 rounded-md shadow-lg max-w-xs whitespace-normal"
        >
          {content}
          <div className="absolute top-full left-1/2 transform -translate-x-1/2 border-4 border-transparent border-t-neutral-800" />
        </div>
      )}
    </span>
  );
}

export default memo(FormTooltip);
