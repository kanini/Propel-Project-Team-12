# Task - task_003_fe_risk_indicator_badge

## Requirement Reference

- User Story: us_038
- Story Location: .propel/context/tasks/EP-005/us_038/us_038.md
- Acceptance Criteria:
  - AC-3: A high-risk score (>70) displays a visual risk indicator (red/amber/green badge) alongside appointment details when Staff views the appointment
- Edge Cases:
  - None directly applicable (UI rendering only; scoring logic handled in backend tasks)

## Design References (Frontend Tasks Only)

| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Figma URL** | N/A |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-003-patient-dashboard.html |
| **Screen Spec** | figma_spec.md#SCR-003 |
| **UXR Requirements** | UXR-001, UXR-002, UXR-003, UXR-301, UXR-303, UXR-502 |
| **Design Tokens** | designsystem.md#colors, designsystem.md#typography |

### **CRITICAL: Wireframe Implementation Requirement (UI Tasks Only)**

**IF Wireframe Status = AVAILABLE or EXTERNAL:**

- **MUST** open and reference the wireframe file/URL during UI implementation
- **MUST** match layout, spacing, typography, and colors from the wireframe
- **MUST** implement all states shown in wireframe (default, hover, focus, error, loading)
- **MUST** validate implementation against wireframe at breakpoints: 375px, 768px, 1440px
- Run `/analyze-ux` after implementation to verify pixel-perfect alignment

## Applicable Technology Stack

| Layer | Technology | Version |
|-------|------------|---------|
| Frontend | React + TypeScript + Redux Toolkit + Tailwind CSS | React 18.x, TypeScript 5.x, Redux Toolkit 2.x |

**Note**: All code, and libraries, MUST be compatible with versions above.

## AI References (AI Tasks Only)

| Reference Type | Value |
|----------------|-------|
| **AI Impact** | No |
| **AIR Requirements** | N/A |
| **AI Pattern** | N/A |
| **Prompt Template Path** | N/A |
| **Guardrails Config** | N/A |
| **Model Provider** | N/A |

## Mobile References (Mobile Tasks Only)

| Reference Type | Value |
|----------------|-------|
| **Mobile Impact** | No |
| **Platform Target** | N/A |
| **Min OS Version** | N/A |
| **Mobile Framework** | N/A |

## Task Overview

Add a visual no-show risk indicator badge to Staff-facing appointment views. This creates a reusable `RiskBadge` component that displays a color-coded badge (green for Low <40, amber for Medium 40-70, red for High >70) alongside appointment details. The `Appointment` TypeScript interface is extended with `noShowRiskScore` and `riskLevel` fields to consume the updated API response. The `RiskBadge` is integrated into the Staff `AppointmentCard` component and conditionally rendered only when a risk score is present. The badge includes accessible aria-labels for screen readers.

## Dependent Tasks

- EP-005/us_038/task_002_be_risk_scoring_integration — Provides `NoShowRiskScore` and `RiskLevel` in AppointmentResponseDto API response

## Impacted Components

- **NEW** `src/frontend/src/components/common/RiskBadge.tsx` — Reusable risk indicator badge (red/amber/green)
- **MODIFY** `src/frontend/src/types/appointment.ts` — Add `noShowRiskScore` and `riskLevel` optional fields to `Appointment` interface
- **MODIFY** `src/frontend/src/features/staff/components/AppointmentCard.tsx` — Integrate RiskBadge next to status badge
- **MODIFY** `src/frontend/src/components/appointments/AppointmentCard.tsx` — Integrate RiskBadge for Staff role context (conditional)

## Implementation Plan

1. **Extend Appointment TypeScript interface**:
   ```typescript
   // Add to existing Appointment interface in types/appointment.ts
   noShowRiskScore?: number;  // 0-100, from API response
   riskLevel?: 'Low' | 'Medium' | 'High'; // Derived from score
   ```
   Both fields are optional since legacy appointments may not have scores.

2. **Create RiskBadge component**:
   ```typescript
   // src/frontend/src/components/common/RiskBadge.tsx
   interface RiskBadgeProps {
       score: number;           // 0-100
       riskLevel: string;       // "Low", "Medium", "High"
       showScore?: boolean;     // Show numeric score alongside label (default: false)
   }
   ```
   - Color mapping using existing Tailwind design tokens:
     - **Low** (<40): `bg-success/10 text-success` (green)
     - **Medium** (40-70): `bg-warning/10 text-warning` (amber)
     - **High** (>70): `bg-error/10 text-error` (red)
   - Badge layout: pill-shaped, matching existing `statusConfig` badge pattern in Staff AppointmentCard
   - Content: "Low Risk" / "Medium Risk" / "High Risk" label, optional numeric score
   - Accessible: `aria-label="No-show risk: {RiskLevel}, score {Score} out of 100"`

3. **Integrate RiskBadge into Staff AppointmentCard**:
   - Import `RiskBadge` component
   - Render next to the existing status badge in the header section:
     ```tsx
     {/* Header with Status Badge and Risk Badge */}
     <div className="flex justify-between items-start mb-4">
         <div>...</div>
         <div className="flex items-center gap-2">
             {appointment.noShowRiskScore != null && appointment.riskLevel && (
                 <RiskBadge
                     score={appointment.noShowRiskScore}
                     riskLevel={appointment.riskLevel}
                 />
             )}
             <span className={`...status badge...`}>...</span>
         </div>
     </div>
     ```
   - Only renders when `noShowRiskScore` is not null (backward-compatible)

4. **Integrate RiskBadge into patient-facing AppointmentCard (Staff context)**:
   - The patient AppointmentCard at `src/frontend/src/components/appointments/AppointmentCard.tsx` may also display in Staff-visible views
   - Add conditional rendering: show RiskBadge only when score is present
   - The badge is informational for Staff; patients do not need to see their own risk score
   - If the component is patient-only context, skip this integration (Staff uses the separate Staff AppointmentCard)

5. **Ensure ArrivalAppointment type also includes risk fields**:
   - Check if `src/frontend/src/types/arrival.ts` `ArrivalAppointment` interface needs `noShowRiskScore` and `riskLevel`
   - If Staff arrival management views show appointments, extend the type accordingly

6. **Accessibility compliance**:
   - RiskBadge uses `aria-label` with full risk description
   - Color is NOT the only indicator — text label ("High Risk") accompanies color (WCAG 2.2 AA)
   - Focus is not needed (badge is informational, not interactive)

## Current Project State

```
src/frontend/src/
├── components/
│   ├── common/                          # No RiskBadge component
│   └── appointments/
│       └── AppointmentCard.tsx          # EXISTS — patient appointment card with status badge
├── features/
│   └── staff/
│       └── components/
│           └── AppointmentCard.tsx      # EXISTS — staff card with statusConfig badge pattern
├── types/
│   ├── appointment.ts                   # EXISTS — Appointment interface without risk score
│   └── arrival.ts                       # EXISTS — ArrivalAppointment interface
└── store/
    └── slices/
        └── appointmentSlice.ts          # EXISTS — appointment state management
```

## Expected Changes

| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/frontend/src/components/common/RiskBadge.tsx | Reusable risk badge with green/amber/red color coding |
| MODIFY | src/frontend/src/types/appointment.ts | Add noShowRiskScore and riskLevel optional fields |
| MODIFY | src/frontend/src/features/staff/components/AppointmentCard.tsx | Render RiskBadge in header alongside status badge |
| MODIFY | src/frontend/src/components/appointments/AppointmentCard.tsx | Conditionally render RiskBadge when score present |

## External References

- Tailwind CSS Colors: https://tailwindcss.com/docs/customizing-colors
- WCAG 2.2 AA Use of Color: https://www.w3.org/WAI/WCAG22/Understanding/use-of-color
- React Component Composition: https://react.dev/learn/passing-props-to-a-component

## Build Commands

```bash
cd src/frontend
npm run build
npm run lint
```

## Implementation Validation Strategy

- [ ] Unit tests pass
- [ ] **[UI Tasks]** Visual comparison against wireframe completed at 375px, 768px, 1440px
- [ ] **[UI Tasks]** Run `/analyze-ux` to validate wireframe alignment
- [ ] RiskBadge renders green for Low (<40), amber for Medium (40-70), red for High (>70)
- [ ] RiskBadge has accessible aria-label with score description
- [ ] Badge only appears when noShowRiskScore is not null (backward-compatible)
- [ ] Text label accompanies color (WCAG 2.2 AA — color is not sole indicator)
- [ ] Staff AppointmentCard displays risk badge alongside status badge

## Implementation Checklist

- [ ] Add `noShowRiskScore` and `riskLevel` optional fields to `Appointment` TypeScript interface
- [ ] Create `RiskBadge` component with green/amber/red color mapping and accessible aria-label
- [ ] Integrate `RiskBadge` into Staff `AppointmentCard` header alongside status badge
- [ ] Add conditional rendering in patient `AppointmentCard` for Staff-context views
- [ ] Ensure ArrivalAppointment type includes risk fields if used in Staff arrival views
- [ ] Verify WCAG accessibility: text label + color, aria-label with score description
- **[UI Tasks - MANDATORY]** Reference wireframe from Design References table during implementation
- **[UI Tasks - MANDATORY]** Validate UI matches wireframe before marking task complete
