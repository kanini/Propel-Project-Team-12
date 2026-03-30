using System.Text.Json;

namespace PatientAccess.Business.Validators;

/// <summary>
/// Validates that cached data does not contain PHI (AC4 - US_056).
/// Prevents accidental caching of patient names, medical records, SSN, etc.
/// Epic: EP-010 - HIPAA Compliance & Security Hardening
/// Requirement: NFR-004 (zero-PHI caching strategy), AD-005 (design.md)
/// </summary>
public static class CachingPolicyValidator
{
    /// <summary>
    /// Prohibited field names (case-insensitive).
    /// Any object containing these fields CANNOT be cached.
    /// Covers PHI data as defined by HIPAA: identifiers, clinical data, demographics.
    /// </summary>
    private static readonly HashSet<string> ProhibitedFields = new(StringComparer.OrdinalIgnoreCase)
    {
        // Patient identifiers (HIPAA: 18 PHI identifiers)
        "patientid", "patient_id", "patientname", "patient_name",
        "firstname", "first_name", "lastname", "last_name", "fullname", "full_name",
        "email", "phone", "phonenumber", "phone_number", "mobile",

        // Medical identifiers
        "mrn", "medicicalrecordnumber", "medical_record_number",
        "ssn", "socialsecuritynumber", "social_security_number",
        "insuranceid", "insurance_id", "insurancenumber", "insurance_number",
        "membernumber", "member_number", "policyid", "policy_id",

        // Clinical data
        "diagnosis", "diagnosiscode", "diagnosis_code", "medication", "prescription",
        "allergy", "allergylist", "allergy_list",
        "vital", "vitalsign", "vital_sign", "vitals",
        "labresult", "lab_result", "testresult", "test_result",
        "clinicalnote", "clinical_note", "progressnote", "progress_note",
        "chiefcomplaint", "chief_complaint", "presentingillness", "presenting_illness",
        "medicalhistory", "medical_history", "surgicalhistory", "surgical_history",

        // Appointment details with PHI
        "visitreason", "visit_reason", "symptoms", "complaint",
        "appointmentnotes", "appointment_notes", "clinicalnotes", "clinical_notes",

        // Document references
        "documentcontent", "document_content", "fileontent", "file_content",
        "extracteddata", "extracted_data", "ocrtext", "ocr_text",

        // Geographic identifiers (smaller than state level)
        "address", "street", "streetaddress", "street_address",
        "city", "zipcode", "zip_code", "postalcode", "postal_code",
        "latitude", "longitude", "coordinates",

        // Dates (except year)
        "dateofbirth", "date_of_birth", "birthdate", "birth_date", "dob",
        "appointmentdate", "appointment_date", "visitdate", "visit_date",
        "admissiondate", "admission_date", "dischargedate", "discharge_date",

        // Biometric identifiers
        "fingerprint", "retinascan", "retina_scan", "facialimage", "facial_image",
        "voiceprint", "voice_print",

        // Unique identifiers
        "userid", "user_id", "accountnumber", "account_number",
        "certificatenumber", "certificate_number", "licensenumber", "license_number",
        "deviceid", "device_id", "serialnumber", "serial_number",

        // Web identifiers  
        "ipaddress", "ip_address", "url", "uri"
    };

    /// <summary>
    /// Validates that an object is safe to cache (contains no PHI).
    /// Throws InvalidOperationException if PHI detected.
    /// </summary>
    /// <typeparam name="T">Type of object to validate</typeparam>
    /// <param name="data">Object to validate for PHI content</param>
    /// <param name="cacheKey">Cache key for error reporting</param>
    /// <exception cref="InvalidOperationException">Thrown if PHI field detected</exception>
    public static void ValidateNoPHI<T>(T data, string cacheKey)
    {
        if (data == null)
            return;

        // Serialize to JSON and inspect property names
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var jsonDocument = JsonDocument.Parse(json);
        ValidateJsonElement(jsonDocument.RootElement, cacheKey);
    }

    /// <summary>
    /// Recursively validates JSON element for prohibited field names.
    /// </summary>
    private static void ValidateJsonElement(JsonElement element, string cacheKey)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                // Check if property name is prohibited
                if (ProhibitedFields.Contains(property.Name))
                {
                    throw new InvalidOperationException(
                        $"PHI caching violation detected: Field '{property.Name}' is prohibited " +
                        $"in cache key '{cacheKey}'. Zero-PHI caching policy enforced (AC4 - US_056). " +
                        $"PHI data must remain in encrypted PostgreSQL database only. " +
                        $"See docs/CACHING_POLICY.md for allowed cache types.");
                }

                // Recursively validate nested objects/arrays
                ValidateJsonElement(property.Value, cacheKey);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                ValidateJsonElement(item, cacheKey);
            }
        }
    }

    /// <summary>
    /// Validates that a cache key uses an allowed prefix from RedisKeyPrefix enum.
    /// Throws InvalidOperationException if prefix invalid.
    /// </summary>
    /// <param name="cacheKey">Cache key to validate</param>
    /// <exception cref="InvalidOperationException">Thrown if key prefix not in allowed list</exception>
    public static void ValidateKeyPrefix(string cacheKey)
    {
        if (string.IsNullOrWhiteSpace(cacheKey))
        {
            throw new ArgumentException("Cache key cannot be null or whitespace", nameof(cacheKey));
        }

        var allowedPrefixes = Enum.GetValues<Enums.RedisKeyPrefix>()
            .Select(p => ConvertEnumToPrefix(p));

        var hasValidPrefix = allowedPrefixes.Any(prefix =>
            cacheKey.StartsWith(prefix + ":", StringComparison.OrdinalIgnoreCase));

        if (!hasValidPrefix)
        {
            throw new InvalidOperationException(
                $"Invalid cache key prefix: '{cacheKey}'. " +
                $"Allowed prefixes: {string.Join(", ", allowedPrefixes)} (AC4 - US_056). " +
                $"See docs/CACHING_POLICY.md for zero-PHI caching policy.");
        }
    }

    /// <summary>
    /// Converts RedisKeyPrefix enum to lowercase prefix string.
    /// Example: RedisKeyPrefix.Session → "session"
    /// </summary>
    private static string ConvertEnumToPrefix(Enums.RedisKeyPrefix prefix)
    {
        // Convert PascalCase enum to lowercase (Session → session, AggregateAppointments → aggregate:appointments)
        var prefixString = prefix.ToString();

        // Handle multi-word prefixes (AggregateAppointments → aggregate:appointments)
        if (prefixString.StartsWith("Aggregate"))
        {
            var suffix = prefixString.Substring("Aggregate".Length).ToLowerInvariant();
            return $"aggregate:{suffix}";
        }
        else if (prefixString.Contains("Provider"))
        {
            return prefixString.ToLowerInvariant().Replace("provider", "");
        }

        return prefixString.ToLowerInvariant();
    }
}
