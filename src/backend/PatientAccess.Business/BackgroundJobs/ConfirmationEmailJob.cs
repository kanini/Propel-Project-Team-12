using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.Interfaces;
using PatientAccess.Business.Services;
using PatientAccess.Data;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PatientAccess.Business.BackgroundJobs;

/// <summary>
/// Hangfire background job for sending appointment confirmation emails with PDF attachments (US_028 - FR-012, AC-2, AC-3).
/// Triggered after successful appointment booking with retry logic for failures.
/// </summary>
public class ConfirmationEmailJob
{
    private readonly PatientAccessDbContext _context;
    private readonly IPdfGenerationService _pdfService;
    private readonly IEmailService _emailService;
    private readonly ILogger<ConfirmationEmailJob> _logger;

    public ConfirmationEmailJob(
        PatientAccessDbContext context,
        IPdfGenerationService pdfService,
        IEmailService emailService,
        ILogger<ConfirmationEmailJob> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _pdfService = pdfService ?? throw new ArgumentNullException(nameof(pdfService));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Sends appointment confirmation email with PDF attachment (AC-2, AC-3).
    /// Implements automatic retry on failure (max 3 attempts with exponential backoff).
    /// </summary>
    /// <param name="appointmentId">Appointment GUID</param>
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 900 })]
    public async Task SendConfirmationAsync(Guid appointmentId)
    {
        try
        {
            _logger.LogInformation("========== STARTING confirmation email job for appointment {AppointmentId} ==========", appointmentId);

            // Fetch appointment with navigation properties
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Provider)
                .Include(a => a.TimeSlot)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment == null)
            {
                _logger.LogError("Appointment {AppointmentId} not found for confirmation email", appointmentId);
                throw new InvalidOperationException($"Appointment {appointmentId} not found");
            }

            _logger.LogInformation(
                "Appointment loaded - Patient: {PatientName} ({PatientEmail}), Provider: {ProviderName}, DateTime: {DateTime}",
                appointment.Patient.Name, appointment.Patient.Email, appointment.Provider.Name, appointment.ScheduledDateTime);

            // Generate PDF (AC-1)
            _logger.LogInformation("Generating PDF for appointment {AppointmentId}", appointmentId);
            var pdfBytes = await _pdfService.GenerateConfirmationPdfAsync(appointment);
            _logger.LogInformation("PDF generated successfully. Size: {Size} bytes", pdfBytes.Length);

            // Save PDF to file system
            var pdfDirectory = Path.Combine("pdfs");
            if (!Directory.Exists(pdfDirectory))
            {
                Directory.CreateDirectory(pdfDirectory);
            }

            var pdfFileName = $"Appointment_Confirmation_{appointment.ConfirmationNumber}.pdf";
            var pdfFilePath = Path.Combine(pdfDirectory, pdfFileName);
            await File.WriteAllBytesAsync(pdfFilePath, pdfBytes);

            // Update appointment with PDF file path
            appointment.PdfFilePath = pdfFilePath;
            appointment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "PDF saved to {FilePath} for appointment {AppointmentId}",
                pdfFilePath, appointmentId);

            // Send email with PDF attachment (AC-2, AC-3)
            var patientEmail = appointment.Patient.Email;
            var patientName = appointment.Patient.Name;

            _logger.LogInformation(
                "Sending confirmation email to {Email} for appointment {AppointmentId}",
                patientEmail, appointmentId);

            var emailSent = await _emailService.SendAppointmentConfirmationAsync(
                patientEmail,
                patientName,
                appointment.Provider.Name,
                appointment.ScheduledDateTime,
                appointment.ConfirmationNumber,
                pdfBytes,
                pdfFileName);

            if (!emailSent)
            {
                _logger.LogWarning(
                    "Failed to send confirmation email for appointment {AppointmentId}. Will retry.",
                    appointmentId);
                throw new InvalidOperationException($"Email delivery failed for appointment {appointmentId}");
            }

            _logger.LogInformation(
                "========== SUCCESS: Confirmation email sent for appointment {AppointmentId} ==========",
                appointmentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "========== ERROR: Processing confirmation email job for appointment {AppointmentId} ==========",
                appointmentId);
            throw; // Rethrow to trigger Hangfire retry
        }
    }
}
