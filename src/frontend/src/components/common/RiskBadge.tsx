/**
 * US_038 AC-3: Risk indicator badge for no-show risk scoring (TR-020, FR-023)
 * Displays color-coded badge (green/amber/red) alongside appointment details
 * WCAG 2.2 AA compliant: Text label + color (not color alone)
 */

interface RiskBadgeProps {
    /** No-show risk score (0-100) */
    score: number;
    /** Risk level classification */
    riskLevel: 'Low' | 'Medium' | 'High';
    /** Show numeric score alongside label (default: false) */
    showScore?: boolean;
}

/**
 * Reusable risk badge component with accessible color-coded visual indicator
 * @param score - No-show risk score (0-100)
 * @param riskLevel - Risk level: "Low" (<40), "Medium" (40-70), or "High" (>70)
 * @param showScore - Optional flag to display numeric score
 */
export const RiskBadge = ({ score, riskLevel, showScore = false }: RiskBadgeProps) => {
    // Color mapping using Tailwind design tokens (colors match statusConfig badge pattern)
    const colorClasses = {
        Low: 'bg-green-50 text-green-700 border-green-200',
        Medium: 'bg-amber-50 text-amber-700 border-amber-200',
        High: 'bg-red-50 text-red-700 border-red-200',
    };

    // Accessible text descriptions
    const ariaLabel = `No-show risk: ${riskLevel}, score ${score} out of 100`;

    return (
        <span
            className={`inline-flex items-center px-2.5 py-1 rounded-full text-xs font-medium border ${colorClasses[riskLevel]}`}
            aria-label={ariaLabel}
        >
            {riskLevel} Risk{showScore && ` (${score})`}
        </span>
    );
};
