using Microsoft.Extensions.Logging;
using PatientAccess.Business.Interfaces;

namespace PatientAccess.Business.BackgroundJobs;

/// <summary>
/// Background job for monitoring slot availability and processing automatic swaps (FR-010).
/// Runs periodically (recommended every 5 minutes) to detect when preferred slots become available.
/// Executes swaps in FIFO order based on appointment creation time.
/// </summary>
public class SlotAvailabilityMonitor
{
    private readonly ISlotSwapService _slotSwapService;
    private readonly ILogger<SlotAvailabilityMonitor> _logger;

    public SlotAvailabilityMonitor(
        ISlotSwapService slotSwapService,
        ILogger<SlotAvailabilityMonitor> logger)
    {
        _slotSwapService = slotSwapService ?? throw new ArgumentNullException(nameof(slotSwapService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes the slot availability monitoring and swap processing job.
    /// Should be called by background job scheduler (Hangfire, Quartz, or hosted service).
    /// </summary>
    public async Task RunAsync()
    {
        try
        {
            _logger.LogInformation("SlotAvailabilityMonitor job started");

            var swapCount = await _slotSwapService.ProcessPendingSwapsAsync();

            _logger.LogInformation(
                "SlotAvailabilityMonitor job completed. Processed {Count} successful swaps",
                swapCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SlotAvailabilityMonitor job failed");
            // Don't throw - background job should not fail the application
            // Logging and monitoring should alert on errors
        }
    }
}
