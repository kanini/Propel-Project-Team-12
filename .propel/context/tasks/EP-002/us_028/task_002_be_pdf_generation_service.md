# Task - task_002_be_pdf_generation_service

## Requirement Reference
- User Story: US_028
- Story Location: .propel/context/tasks/EP-002/us_028/us_028.md
- Acceptance Criteria:
    - AC-1: Generate PDF with QuestPDF containing appointment date/time, provider name, specialty, location, visit reason, confirmation number
    - AC-2: PDF attached to email, email body includes summary
    - AC-3: Email sent within 2 minutes of booking with professional branding
    - AC-4: PDF downloadable from My Appointments

## Design References
| UI Impact | No |
| All fields | N/A |

## Applicable Technology Stack
| Backend | .NET 8.0, ASP.NET Core 8.0, EF Core 8.0 |
| Database | PostgreSQL 16.x |
| Library | QuestPDF 2024.x, MailKit 4.x |

## Task Overview
Implement appointment confirmation PDF generation using QuestPDF library. Create IpdfGenerationService with GenerateConfirmationPdfAsync method producing professional PDF with appointment details and platform branding. Integrate email delivery with MailKit attaching PDF. Store PDF path reference in database for re-download. Create GET /api/appointments/{id}/confirmation-pdf endpoint returning PDF stream. Add background job triggering PDF generation and email delivery within 2 minutes of booking.

## Dependent Tasks
- None (extends appointment booking functionality)

## Impacted Components
- `src/backend/PatientAccess.Business/Services/PdfGenerationService.cs` (NEW)
- `src/backend/PatientAccess.Business/Interfaces/IPdfGenerationService.cs` (NEW)
- `src/backend/PatientAccess.Business/Services/EmailService.cs` (UPDATE - attach PDF to email)
- `src/backend/PatientAccess.Web/Controllers/AppointmentsController.cs` (UPDATE - add GET confirmation-pdf endpoint)
- `src/backend/PatientAccess.Business/BackgroundJobs/ConfirmationEmailJob.cs` (NEW - Hangfire job)

## Implementation Plan
1. Install QuestPDF NuGet package: `dotnet add package QuestPDF --version 2024.x`
2. Create IPdfGenerationService: Task<byte[]> GenerateConfirmationPdfAsync(Appointment appointment)
3. Implement PdfGenerationService using QuestPDF DSL: Define document layout (header with logo, appointment details table, footer with confirmation number)
4. Include fields: Appointment Date/Time (formatted), Provider Name, Specialty, Location (placeholder: "123 Healthcare Ave"), Visit Reason, Confirmation Number (8-char alphanumeric)
5. Store PDF bytes in database: Add PdfFilePath column to Appointments table, save to file system OR store bytes in database
6. Create ConfirmationEmailJob Hangfire job: Triggered by POST /api/appointments success, generates PDF, sends email with attachment
7. Update EmailService: AddAttachment(byte[] pdfBytes, string filename)
8. Email body template: HTML with appointment summary + "Please find your confirmation attached"
9. Add GET /api/appointments/{id}/confirmation-pdf endpoint: Retrieve PDF bytes, return File(bytes, "application/pdf", filename)
10. Handle PDF generation failure: Retry job with exponential backoff (max 3 retries), log error, PDF remains downloadable after retry success

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/Services/PdfGenerationService.cs | QuestPDF-based confirmation PDF generator |
| CREATE | src/backend/PatientAccess.Business/Interfaces/IPdfGenerationService.cs | Service interface |
| CREATE | src/backend/PatientAccess.Business/BackgroundJobs/ConfirmationEmailJob.cs | Hangfire job for PDF + email delivery |
| UPDATE | src/backend/PatientAccess.Business/Services/EmailService.cs | Add attachment support |
| UPDATE | src/backend/PatientAccess.Business/Models/Appointment.cs | Add PdfFilePath property |
| UPDATE | src/backend/PatientAccess.Web/Controllers/AppointmentsController.cs | Add GET /api/appointments/{id}/confirmation-pdf |
| UPDATE | src/backend/PatientAccess.Web/Program.cs | Register IPdfGenerationService, configure Hangfire job |

## Implementation Checklist
- [x] Install QuestPDF: `dotnet add package QuestPDF --version 2024.x`
- [x] Create IPdfGenerationService with GenerateConfirmationPdfAsync(Appointment appointment)
- [x] Implement PdfGenerationService using QuestPDF Document DSL
- [x] Define PDF layout: Page(page => { page.Size(PageSizes.A4); page.Content().Column(...); })
- [x] Add header: Platform logo (SVG or PNG), platform name
- [x] Add appointment details table: Date/Time, Provider, Specialty, Location, Visit Reason
- [x] Add footer: Confirmation Number prominently displayed (large font)
- [x] Generate PDF bytes: return document.GeneratePdf()
- [x] Add PdfFilePath to Appointment.cs entity (VARCHAR(500), nullable)
- [x] Save PDF to file system: File.WriteAllBytesAsync($"pdfs/{appointmentId}.pdf", pdfBytes)
- [ ] OR Store in database: Add PdfContent BYTEA column
- [x] Create ConfirmationEmailJob Hangfire job
- [x] Queue job after appointment booking: BackgroundJob.Enqueue(() => job.SendConfirmationAsync(appointmentId))
- [x] Job logic: Fetch appointment, call GenerateConfirmationPdfAsync, call EmailService.SendWithAttachmentAsync
- [x] Update EmailService: Add AddAttachment(byte[] bytes, string filename, string mimeType)
- [x] Use MailKit AttachmentCollection.Add(new MimePart(...)
- [x] Email template: HTML body with summary + "Attached: Appointment_Confirmation_{number}.pdf"
- [x] Add GET /api/appointments/{id}/confirmation-pdf endpoint
- [x] Verify ownership: WHERE AppointmentId = {id} AND PatientId = {patientId}
- [x] Read PDF bytes from file system OR database
- [x] Return File(bytes, "application/pdf", $"Appointment_Confirmation_{confirmationNumber}.pdf")
- [x] Handle missing PDF (generation failed): Return 404 with message "PDF not available"
- [x] Add retry logic to ConfirmationEmailJob: [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 900 })]
- [x] Register IPdfGenerationService -> PdfGenerationService in DI
- [x] Configure Hangfire storage in Program.cs

Estimated effort: 8 hours.

