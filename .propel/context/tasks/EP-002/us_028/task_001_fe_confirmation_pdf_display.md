# Task - task_001_fe_confirmation_pdf_display

## Requirement Reference
- User Story: US_028
- Story Location: .propel/context/tasks/EP-002/us_028/us_028.md
- Acceptance Criteria:
    - AC-4: Download confirmation PDF from appointment details view in My Appointments

## Design References
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | Yes |
| **Wireframe Status** | AVAILABLE |
| **Wireframe Type** | HTML |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-008-confirmation.html |
| **Screen Spec** | .propel/context/docs/figma_spec.md#SCR-008 |
| **UXR Requirements** | UXR-501 (200ms action feedback) |

### **CRITICAL: Wireframe Reference MANDATORY**
- MUST reference wireframe during implementation
- MUST validate UI matches wireframe before complete

## Applicable Technology Stack
| Frontend | React 18.x, TypeScript 5.x, Redux Toolkit 2.x, Tailwind CSS 3.x |

## Task Overview
Add "Download PDF" button to appointment confirmation screen and appointment detail view in My Appointments. Fetch PDF via GET /api/appointments/{id}/confirmation-pdf endpoint. Handle loading state during PDF generation. Display error message if PDF generation failed with retry option.

## Dependent Tasks
- task_002_be_pdf_generation_service.md - Backend PDF generation and email delivery
- task_001_fe_appointment_booking_calendar.md - Confirmation screen integration

## Impacted Components
- `src/frontend/src/components/appointments/ConfirmationDialog.tsx` (UPDATE - add Download PDF button)
- `src/frontend/src/pages/MyAppointments.tsx` (UPDATE - add PDF download to appointment detail)

## Implementation Plan
1. Add "Download PDF" button to ConfirmationDialog (post-booking success screen)
2. Implement downloadPDF action: GET /api/appointments/{id}/confirmation-pdf (returns PDF blob)
3. Handle loading state: Show spinner, disable button during download
4. Handle error: Display toast "PDF generation failed. Please try again."
5. On success: Trigger browser file download with filename "Appointment_Confirmation_{confirmationNumber}.pdf"
6. Add "View Confirmation" button to appointment detail in My Appointments listing same download logic

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| UPDATE | src/frontend/src/components/appointments/ConfirmationDialog.tsx | Add "Download PDF" button with loading state |
| UPDATE | src/frontend/src/pages/MyAppointments.tsx | Add "View Confirmation" action to appointment cards |
| UPDATE | src/frontend/src/store/slices/appointmentSlice.ts | Add downloadConfirmationPDF async thunk |

## Implementation Checklist
- [x] Add "Download PDF" button to ConfirmationDialog
- [x] **[UI Tasks - MANDATORY]** Reference wireframe during implementation
- [x] **[UI Tasks - MANDATORY]** Validate UI matches wireframe
- [x] Implement downloadConfirmationPDF thunk: GET /api/appointments/{id}/confirmation-pdf (responseType: 'blob')
- [x] Handle loading: Show spinner icon in button, disabled state
- [x] On success: Create Blob URL, trigger download with <a download href={blobUrl}>
- [x] On error: Display toast "PDF generation failed. Retry?"
- [x] Add "View Confirmation" button to AppointmentCard in My Appointments
- [x] Reuse downloadConfirmationPDF logic for detail view

Estimated effort: 3 hours.

