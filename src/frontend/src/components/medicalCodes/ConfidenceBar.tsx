/**
 * ConfidenceBar Component (EP-008-US-052)
 * Displays confidence score with color-coded bar
 * - Green: >85%
 * - Amber: 70-85%
 * - Red: <70%
 */

import React from 'react';

interface ConfidenceBarProps {
  score: number; // 0-100
  showLabel?: boolean;
}

export const ConfidenceBar: React.FC<ConfidenceBarProps> = ({
  score,
  showLabel = true,
}) => {
  const getColorClass = (score: number): string => {
    if (score > 85) return 'bg-green-500';
    if (score >= 70) return 'bg-amber-500';
    return 'bg-red-500';
  };

  return (
    <div className="flex items-center gap-2">
      <div
        className="w-16 h-2 bg-gray-200 rounded-full overflow-hidden"
        role="progressbar"
        aria-valuenow={score}
        aria-valuemin={0}
        aria-valuemax={100}
        aria-label={`Confidence score: ${score}%`}
      >
        <div
          className={`h-full transition-all duration-300 ${getColorClass(score)}`}
          style={{ width: `${score}%` }}
        />
      </div>
      {showLabel && (
        <span className="text-sm text-gray-600 min-w-[3ch]">{score}%</span>
      )}
    </div>
  );
};
