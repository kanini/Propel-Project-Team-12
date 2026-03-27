/**
 * SeverityBadge Component (US_048)
 * Displays conflict severity with appropriate color coding.
 * Critical = Red, Warning = Amber, Info = Blue
 */

import { ConflictSeverity } from "../types/conflict.types";

interface SeverityBadgeProps {
  severity: ConflictSeverity;
  showIcon?: boolean;
  className?: string;
}

export const SeverityBadge = ({
  severity,
  showIcon = true,
  className = "",
}: SeverityBadgeProps) => {
  const getStyles = () => {
    switch (severity) {
      case ConflictSeverity.Critical:
        return {
          bg: "bg-red-100",
          text: "text-red-800",
          border: "border-red-300",
          icon: "⚠️",
        };
      case ConflictSeverity.Warning:
        return {
          bg: "bg-amber-100",
          text: "text-amber-800",
          border: "border-amber-300",
          icon: "⚠",
        };
      case ConflictSeverity.Info:
        return {
          bg: "bg-blue-100",
          text: "text-blue-800",
          border: "border-blue-300",
          icon: "ℹ",
        };
      default:
        return {
          bg: "bg-gray-100",
          text: "text-gray-800",
          border: "border-gray-300",
          icon: "•",
        };
    }
  };

  const styles = getStyles();

  return (
    <span
      className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border ${styles.bg} ${styles.text} ${styles.border} ${className}`}
      aria-label={`${severity} severity`}
    >
      {showIcon && (
        <span className="mr-1" aria-hidden="true">
          {styles.icon}
        </span>
      )}
      {severity}
    </span>
  );
};
