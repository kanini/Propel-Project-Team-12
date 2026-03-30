/**
 * RiskBadge Component for US_038 - No-Show Risk Indicator
 * Displays color-coded badge based on calculated risk score (FR-023)
 */

interface RiskBadgeProps {
    score: number; // 0-100
    riskLevel: 'Low' | 'Medium' | 'High';
    showScore?: boolean; // Optional: show numeric score alongside label
}

/**
 * Risk level color configuration aligned with design tokens
 */
const riskConfig = {
    Low: {
        color: 'bg-success/10 text-success',
        label: 'Low Risk',
    },
    Medium: {
        color: 'bg-warning/10 text-warning',
        label: 'Medium Risk',
    },
    High: {
        color: 'bg-error/10 text-error',
        label: 'High Risk',
    },
};

/**
 * Reusable risk indicator badge for Staff-facing appointment views
 * Displays green (Low < 40), amber (Medium 40-70), red (High > 70)
 * @param score - Risk score (0-100)
 * @param riskLevel - Derived risk level
 * @param showScore - Optional flag to display numeric score
 */
export function RiskBadge({ score, riskLevel, showScore = false }: RiskBadgeProps) {
    const config = riskConfig[riskLevel] || riskConfig.Low;

    return (
        <span
            className={`inline-flex items-center px-3 py-1 rounded-full text-sm font-medium ${config.color}`}
            aria-label={`No-show risk: ${riskLevel}, score ${score} out of 100`}
            role="status"
        >
            {config.label}
            {showScore && <span className="ml-1.5">({score})</span>}
        </span>
    );
}
