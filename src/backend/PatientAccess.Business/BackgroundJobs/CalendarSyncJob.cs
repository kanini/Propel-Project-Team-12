using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Exceptions;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;

namespace PatientAccess.Business.BackgroundJobs;

/// <summary>
/// Hangfire background job for asynchronous multi-provider calendar synchronization (US_039/US_040 - FR-024).
/// Supports Google Calendar and Microsoft Outlook Calendar via keyed service resolution.
/// Decouples calendar operations from appointment booking flow to ensure availability resilience (EC-1).
/// Implements exponential backoff retry (1min, 4min, 16min) for transient API failures.
/// </summary>
public class CalendarSyncJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly PatientAccessDbContext _context;
    private readonly ILogger<CalendarSyncJob> _logger;

    public CalendarSyncJob(
        IServiceProvider serviceProvider,
        PatientAccessDbContext context,
        ILogger<CalendarSyncJob> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates calendar events in all connected providers for newly booked appointment (AC-1).
    /// Persists provider-specific event IDs on Appointment for subsequent update/delete operations.
    /// Silently skips providers that are not connected (EC-2).
    /// </summary>
    /// <param name="appointmentId">Appointment unique identifier</param>
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 240, 960 })] // 1min, 4min, 16min
    public async Task CreateCalendarEventAsync(Guid appointmentId)
    {
        _logger.LogInformation("Creating calendar events for Appointment {AppointmentId}", appointmentId);

        try
        {
            var appointment = await _context.Appointments
                .Include(a => a.Provider)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment == null)
            {
                _logger.LogWarning("Appointment {AppointmentId} not found for calendar sync", appointmentId);
                return;
            }

            // Find all connected calendar providers for this patient
            var connectedProviders = await _context.CalendarIntegrations
                .Where(c => c.UserId == appointment.PatientId && c.IsConnected)
                .Select(c => c.Provider)
                .ToListAsync();

            if (connectedProviders.Count == 0)
            {
                _logger.LogInformation("User {PatientId} has no calendar providers connected, skipping sync",
                    appointment.PatientId);
                return;
            }

            // Build calendar event from appointment data
            var eventData = BuildCalendarEventDto(appointment);

            // Create event in each connected provider independently (EC-2)
            foreach (var provider in connectedProviders)
            {
                try
                {
                    var calendarService = _serviceProvider.GetRequiredKeyedService<ICalendarService>(provider);
                    var eventId = await calendarService.CreateEventAsync(appointment.PatientId, eventData);

                    if (!string.IsNullOrEmpty(eventId))
                    {
                        // Store event ID in provider-specific field
                        if (provider == "Google")
                            appointment.GoogleCalendarEventId = eventId;
                        else if (provider == "Outlook")
                            appointment.OutlookCalendarEventId = eventId;

                        _logger.LogInformation(
                            "Created {Provider} Calendar event {EventId} for Appointment {AppointmentId}",
                            provider, eventId, appointmentId);
                    }
                }
                catch (CalendarTokenExpiredException ex)
                {
                    _logger.LogWarning(ex, "{Provider} Calendar token expired for Appointment {AppointmentId}",
                        provider, appointmentId);
                    // Continue with other providers - don't block entire sync
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create {Provider} Calendar event for Appointment {AppointmentId}",
                        provider, appointmentId);
                    // Continue with other providers - partial failure is acceptable
                }
            }

            appointment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create calendar events for Appointment {AppointmentId}",
                appointmentId);
            throw; // Hangfire will retry with exponential backoff
        }
    }

    /// <summary>
    /// Updates calendar events in all providers when appointment is rescheduled (AC-2).
    /// Updates Google event if GoogleCalendarEventId exists, Outlook event if OutlookCalendarEventId exists.
    /// Each provider update is independent - failure in one doesn't block the other.
    /// </summary>
    /// <param name="appointmentId">Appointment unique identifier</param>
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 240, 960 })] // 1min, 4min, 16min
    public async Task UpdateCalendarEventAsync(Guid appointmentId)
    {
        _logger.LogInformation("Updating calendar events for Appointment {AppointmentId}", appointmentId);

        try
        {
            var appointment = await _context.Appointments
                .Include(a => a.Provider)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment == null)
            {
                _logger.LogWarning("Appointment {AppointmentId} not found for calendar update", appointmentId);
                return;
            }

            var eventData = BuildCalendarEventDto(appointment);

            // Update Google Calendar event if exists
            if (!string.IsNullOrEmpty(appointment.GoogleCalendarEventId))
            {
                try
                {
                    var googleService = _serviceProvider.GetRequiredKeyedService<ICalendarService>("Google");
                    if (await googleService.IsConnectedAsync(appointment.PatientId))
                    {
                        await googleService.UpdateEventAsync(
                            appointment.PatientId,
                            appointment.GoogleCalendarEventId,
                            eventData);

                        _logger.LogInformation(
                            "Updated Google Calendar event {EventId} for Appointment {AppointmentId}",
                            appointment.GoogleCalendarEventId, appointmentId);
                    }
                    else
                    {
                        _logger.LogInformation(
                            "User {PatientId} no longer has Google Calendar connected, skipping update",
                            appointment.PatientId);
                    }
                }
                catch (CalendarTokenExpiredException ex)
                {
                    _logger.LogWarning(ex, "Google Calendar token expired for Appointment {AppointmentId}",
                        appointmentId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update Google Calendar event for Appointment {AppointmentId}",
                        appointmentId);
                    // Continue with Outlook update
                }
            }

            // Update Outlook Calendar event if exists
            if (!string.IsNullOrEmpty(appointment.OutlookCalendarEventId))
            {
                try
                {
                    var outlookService = _serviceProvider.GetRequiredKeyedService<ICalendarService>("Outlook");
                    if (await outlookService.IsConnectedAsync(appointment.PatientId))
                    {
                        await outlookService.UpdateEventAsync(
                            appointment.PatientId,
                            appointment.OutlookCalendarEventId,
                            eventData);

                        _logger.LogInformation(
                            "Updated Outlook Calendar event {EventId} for Appointment {AppointmentId}",
                            appointment.OutlookCalendarEventId, appointmentId);
                    }
                    else
                    {
                        _logger.LogInformation(
                            "User {PatientId} no longer has Outlook Calendar connected, skipping update",
                            appointment.PatientId);
                    }
                }
                catch (CalendarTokenExpiredException ex)
                {
                    _logger.LogWarning(ex, "Outlook Calendar token expired for Appointment {AppointmentId}",
                        appointmentId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update Outlook Calendar event for Appointment {AppointmentId}",
                        appointmentId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update calendar events for Appointment {AppointmentId}",
                appointmentId);
            throw; // Hangfire will retry with exponential backoff
        }
    }

    /// <summary>
    /// Deletes calendar events from all providers when appointment is cancelled (AC-3).
    /// Event IDs and patient ID passed as parameters since appointment may be in cancelled state.
    /// Each provider deletion is independent - failure in one doesn't block the other.
    /// </summary>
    /// <param name="appointmentId">Appointment unique identifier (for logging)</param>
    /// <param name="googleCalendarEventId">Google Calendar event ID to delete (nullable)</param>
    /// <param name="outlookCalendarEventId">Outlook Calendar event ID to delete (nullable)</param>
    /// <param name="patientId">Patient unique identifier for calendar access</param>
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 240, 960 })] // 1min, 4min, 16min
    public async Task DeleteCalendarEventAsync(
        Guid appointmentId,
        string? googleCalendarEventId,
        string? outlookCalendarEventId,
        Guid patientId)
    {
        _logger.LogInformation("Deleting calendar events for Appointment {AppointmentId}", appointmentId);

        try
        {
            // Delete Google Calendar event if exists
            if (!string.IsNullOrEmpty(googleCalendarEventId))
            {
                try
                {
                    var googleService = _serviceProvider.GetRequiredKeyedService<ICalendarService>("Google");
                    if (await googleService.IsConnectedAsync(patientId))
                    {
                        await googleService.DeleteEventAsync(patientId, googleCalendarEventId);
                        _logger.LogInformation(
                            "Deleted Google Calendar event {EventId} for Appointment {AppointmentId}",
                            googleCalendarEventId, appointmentId);
                    }
                    else
                    {
                        _logger.LogInformation(
                            "User {PatientId} no longer has Google Calendar connected, skipping delete",
                            patientId);
                    }
                }
                catch (CalendarTokenExpiredException ex)
                {
                    _logger.LogWarning(ex, "Google Calendar token expired for Appointment {AppointmentId}",
                        appointmentId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete Google Calendar event for Appointment {AppointmentId}",
                        appointmentId);
                    // Continue with Outlook deletion
                }
            }

            // Delete Outlook Calendar event if exists
            if (!string.IsNullOrEmpty(outlookCalendarEventId))
            {
                try
                {
                    var outlookService = _serviceProvider.GetRequiredKeyedService<ICalendarService>("Outlook");
                    if (await outlookService.IsConnectedAsync(patientId))
                    {
                        await outlookService.DeleteEventAsync(patientId, outlookCalendarEventId);
                        _logger.LogInformation(
                            "Deleted Outlook Calendar event {EventId} for Appointment {AppointmentId}",
                            outlookCalendarEventId, appointmentId);
                    }
                    else
                    {
                        _logger.LogInformation(
                            "User {PatientId} no longer has Outlook Calendar connected, skipping delete",
                            patientId);
                    }
                }
                catch (CalendarTokenExpiredException ex)
                {
                    _logger.LogWarning(ex, "Outlook Calendar token expired for Appointment {AppointmentId}",
                        appointmentId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete Outlook Calendar event for Appointment {AppointmentId}",
                        appointmentId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete calendar events for Appointment {AppointmentId}",
                appointmentId);
            throw; // Hangfire will retry with exponential backoff
        }
    }

    /// <summary>
    /// Builds CalendarEventDto from Appointment entity.
    /// Reusable across create and update operations (DRY principle).
    /// </summary>
    private static CalendarEventDto BuildCalendarEventDto(Data.Models.Appointment appointment)
    {
        return new CalendarEventDto
        {
            Title = $"Appointment with {appointment.Provider.Name}",
            StartTime = appointment.ScheduledDateTime,
            EndTime = appointment.ScheduledDateTime.AddMinutes(30), // Default 30-minute appointment
            Description = $"Visit reason: {appointment.VisitReason}",
            Location = $"{appointment.Provider.Name} - {appointment.Provider.Specialty}"
        };
    }
}
