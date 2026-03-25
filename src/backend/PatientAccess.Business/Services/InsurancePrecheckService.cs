using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;

namespace PatientAccess.Business.Services;

/// <summary>
/// Insurance precheck service implementation (US_036)
/// Simulates insurance verification - in production would integrate with clearinghouse API
/// </summary>
public class InsurancePrecheckService : IInsurancePrecheckService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<InsurancePrecheckService> _logger;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    private static readonly Dictionary<string, string> ProviderNames = new()
    {
        ["bcbs"] = "Blue Cross Blue Shield",
        ["aetna"] = "Aetna",
        ["cigna"] = "Cigna",
        ["unitedhealthcare"] = "UnitedHealthcare",
        ["humana"] = "Humana",
        ["kaiser"] = "Kaiser Permanente",
        ["anthem"] = "Anthem",
        ["medicare"] = "Medicare",
        ["medicaid"] = "Medicaid",
        ["other"] = "Other"
    };

    public InsurancePrecheckService(IMemoryCache cache, ILogger<InsurancePrecheckService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<InsurancePrecheckResponseDto> VerifyInsuranceAsync(
        InsurancePrecheckRequestDto request, 
        CancellationToken ct = default)
    {
        var cacheKey = $"insurance-precheck:{request.AppointmentId}";

        // Check cache first
        if (_cache.TryGetValue<InsurancePrecheckResponseDto>(cacheKey, out var cached) && cached != null)
        {
            _logger.LogDebug("Returning cached insurance precheck for appointment {AppointmentId}", request.AppointmentId);
            return cached;
        }

        _logger.LogInformation(
            "Verifying insurance for appointment {AppointmentId}, provider {ProviderId}", 
            request.AppointmentId, 
            request.ProviderId);

        // Simulate API call delay
        await Task.Delay(TimeSpan.FromSeconds(1), ct);

        // In production, this would call a clearinghouse API (e.g., Availity, Change Healthcare)
        // For now, simulate verification based on provider

        InsurancePrecheckResponseDto result;

        if (string.IsNullOrEmpty(request.MemberId) || request.MemberId.Length < 5)
        {
            result = new InsurancePrecheckResponseDto
            {
                Status = "not_found",
                Message = "Insurance information could not be located. Please verify your member ID."
            };
        }
        else if (request.ProviderId == "other")
        {
            result = new InsurancePrecheckResponseDto
            {
                Status = "pending",
                Message = "Manual verification required. Our team will contact you."
            };
        }
        else
        {
            // Simulate successful verification
            var providerName = ProviderNames.GetValueOrDefault(request.ProviderId, "Unknown");
            
            result = new InsurancePrecheckResponseDto
            {
                Status = "verified",
                ProviderName = providerName,
                MemberId = request.MemberId,
                EffectiveDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)).ToString("yyyy-MM-dd"),
                ExpirationDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)).ToString("yyyy-MM-dd"),
                CopayAmount = request.ProviderId switch
                {
                    "medicare" => 0,
                    "medicaid" => 0,
                    "kaiser" => 20,
                    _ => 25
                },
                DeductibleRemaining = request.ProviderId switch
                {
                    "medicare" => 226,
                    "medicaid" => 0,
                    _ => Random.Shared.Next(0, 1500)
                }
            };
        }

        // Cache the result
        _cache.Set(cacheKey, result, CacheDuration);

        return result;
    }

    public Task<InsurancePrecheckResponseDto?> GetPrecheckResultAsync(int appointmentId, CancellationToken ct = default)
    {
        var cacheKey = $"insurance-precheck:{appointmentId}";
        
        if (_cache.TryGetValue<InsurancePrecheckResponseDto>(cacheKey, out var cached))
        {
            return Task.FromResult<InsurancePrecheckResponseDto?>(cached);
        }

        return Task.FromResult<InsurancePrecheckResponseDto?>(null);
    }
}
