using Microsoft.Extensions.Logging;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PatientAccess.Business.Services;

/// <summary>
/// PDF generation service using QuestPDF (US_028 - FR-012, TR-019).
/// Generates professional appointment confirmation PDFs with platform branding.
/// </summary>
public class PdfGenerationService : IPdfGenerationService
{
    private readonly ILogger<PdfGenerationService> _logger;

    public PdfGenerationService(ILogger<PdfGenerationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Configure QuestPDF license (Community edition)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <summary>
    /// Generates appointment confirmation PDF with QuestPDF (AC-1).
    /// Includes appointment date/time, provider name, specialty, location,
    /// visit reason, and unique confirmation number.
    /// </summary>
    public async Task<byte[]> GenerateConfirmationPdfAsync(Appointment appointment)
    {
        try
        {
            _logger.LogInformation("Generating PDF for appointment {AppointmentId}", appointment.AppointmentId);

            // Generate PDF using QuestPDF DSL
            var pdfBytes = await Task.Run(() => Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    // Header
                    page.Header().Element(ComposeHeader);

                    // Content
                    page.Content().Element(c => ComposeContent(c, appointment));

                    // Footer
                    page.Footer().Element(c => ComposeFooter(c, appointment));
                });
            }).GeneratePdf());

            _logger.LogInformation(
                "Successfully generated PDF for appointment {AppointmentId}, size: {Size} bytes",
                appointment.AppointmentId, pdfBytes.Length);

            return pdfBytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate PDF for appointment {AppointmentId}", appointment.AppointmentId);
            throw;
        }
    }

    /// <summary>
    /// Composes PDF header with platform branding.
    /// </summary>
    private static void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("Patient Access Platform")
                    .FontSize(20)
                    .Bold()
                    .FontColor(Colors.Blue.Darken3);

                column.Item().Text("Appointment Confirmation")
                    .FontSize(14)
                    .FontColor(Colors.Grey.Darken2);
            });

            row.ConstantItem(100).AlignRight().Text(DateTime.UtcNow.ToString("MM/dd/yyyy"))
                .FontSize(10)
                .FontColor(Colors.Grey.Medium);
        });
    }

    /// <summary>
    /// Composes PDF content with appointment details.
    /// </summary>
    private static void ComposeContent(IContainer container, Appointment appointment)
    {
        container.PaddingVertical(20).Column(column =>
        {
            column.Spacing(15);

            // Confirmation Number (prominent display)
            column.Item().Background(Colors.Blue.Lighten4).Padding(15).Column(c =>
            {
                c.Item().Text("Confirmation Number")
                    .FontSize(12)
                    .FontColor(Colors.Grey.Darken2);

                c.Item().Text(appointment.ConfirmationNumber)
                    .FontSize(24)
                    .Bold()
                    .FontColor(Colors.Blue.Darken3);
            });

            // Appointment Details Table
            column.Item().Text("Appointment Details")
                .FontSize(14)
                .Bold()
                .FontColor(Colors.Blue.Darken3);

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(150);
                    columns.RelativeColumn();
                });

                // Date & Time
                table.Cell().Element(CellStyle).Text("Date & Time:");
                table.Cell().Element(CellStyle).Text(appointment.ScheduledDateTime.ToString("MMMM dd, yyyy 'at' h:mm tt"));

                // Provider
                table.Cell().Element(CellStyle).Text("Provider:");
                table.Cell().Element(CellStyle).Text(appointment.Provider?.Name ?? "Not specified");

                // Specialty
                table.Cell().Element(CellStyle).Text("Specialty:");
                table.Cell().Element(CellStyle).Text(appointment.Provider?.Specialty ?? "Not specified");

                // Location
                table.Cell().Element(CellStyle).Text("Location:");
                table.Cell().Element(CellStyle).Text("123 Healthcare Ave, Medical City, MC 12345");

                // Visit Reason
                table.Cell().Element(CellStyle).Text("Visit Reason:");
                table.Cell().Element(CellStyle).Text(appointment.VisitReason);

                // Patient Name
                table.Cell().Element(CellStyle).Text("Patient:");
                table.Cell().Element(CellStyle).Text(appointment.Patient?.Name ?? "Not specified");

                static IContainer CellStyle(IContainer container)
                {
                    return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8);
                }
            });

            // Important Notes Section
            column.Item().PaddingTop(20).Column(c =>
            {
                c.Item().Text("Important Notes")
                    .FontSize(12)
                    .Bold()
                    .FontColor(Colors.Blue.Darken3);

                c.Item().PaddingTop(5).Text("• Please arrive 15 minutes before your scheduled appointment time.")
                    .FontSize(10)
                    .FontColor(Colors.Grey.Darken1);

                c.Item().Text("• Bring a valid photo ID and insurance card.")
                    .FontSize(10)
                    .FontColor(Colors.Grey.Darken1);

                c.Item().Text($"• To cancel or reschedule, please provide at least {appointment.CancellationNoticeHours} hours notice.")
                    .FontSize(10)
                    .FontColor(Colors.Grey.Darken1);

                c.Item().Text("• Contact us: (555) 123-4567 or support@patientaccess.com")
                    .FontSize(10)
                    .FontColor(Colors.Grey.Darken1);
            });
        });
    }

    /// <summary>
    /// Composes PDF footer with confirmation number and page info.
    /// </summary>
    private static void ComposeFooter(IContainer container, Appointment appointment)
    {
        container.AlignCenter().Text(text =>
        {
            text.Span("Confirmation Code: ").FontSize(9).FontColor(Colors.Grey.Medium);
            text.Span(appointment.ConfirmationNumber).FontSize(9).Bold().FontColor(Colors.Grey.Darken1);
            text.Span(" | Generated on: ").FontSize(9).FontColor(Colors.Grey.Medium);
            text.Span(DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm UTC")).FontSize(9).FontColor(Colors.Grey.Darken1);
        });
    }
}
