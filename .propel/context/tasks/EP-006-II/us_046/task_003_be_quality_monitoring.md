# Task - task_003_be_quality_monitoring

## Requirement Reference
- User Story: US_046
- Story Location: .propel/context/tasks/EP-006-II/us_046/us_046.md
- Acceptance Criteria:
    - **AC4**: Given extraction quality is monitored, When the system evaluates outputs, Then output schema validity rate exceeds 99% for structured responses, ensuring all extracted data conforms to the expected schema.
- Edge Case:
    - N/A (quality monitoring is observational, not user-facing)

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | No |
| **Figma URL** | N/A |
| **Wireframe Status** | N/A |
| **Wireframe Type** | N/A |
| **Wireframe Path/URL** | N/A |
| **Screen Spec** | N/A |
| **UXR Requirements** | N/A |
| **Design Tokens** | N/A |

> If UI Impact = No, all design references should be N/A

## Applicable Technology Stack
| Layer | Technology | Version |
|-------|------------|---------|
| Backend | .NET | 8.0 |
| Backend | ASP.NET Core Web API | 8.0 |
| Database | PostgreSQL | 16.x |
| Library | Entity Framework Core | 8.0 |
| Library | FluentValidation | 11.x |
| Monitoring | Application Insights | Latest |
| Background Jobs | Hangfire | 1.8.x |
| Vector Store | N/A | N/A |
| AI Gateway | N/A | N/A |
| Mobile | N/A | N/A |

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

> **AI Impact Legend:**
> - **Yes**: Task involves LLM integration, RAG pipeline, prompt engineering, or AI infrastructure
> - **No**: Task is deterministic (FE/BE/DB only)
>
> If AI Impact = No, all AI references should be N/A

## Mobile References (Mobile Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **Mobile Impact** | No |
| **Platform Target** | N/A |
| **Min OS Version** | N/A |
| **Mobile Framework** | N/A |

> If Mobile Impact = No, all Mobile references should be N/A

## Task Overview

Implement quality monitoring and validation for AI extraction pipeline to ensure output schema validity (AIR-Q03: >99%), track hallucination rate (AIR-Q04: <2%), and monitor extraction recall (AIR-Q05: >95% for critical elements). This task adds FluentValidation-based schema validators for ExtractedClinicalData entities, implements quality metrics tracking (schema validity, confidence distribution, extraction completeness), creates background jobs for quality reporting, adds Application Insights dashboards for real-time quality monitoring, and implements automated alerting when quality thresholds are breached. The implementation enables proactive quality assurance and provides transparency into AI extraction performance.

**Key Capabilities:**
- FluentValidation schema validators for extracted data types
- Real-time schema validation during extraction persistence
- Quality metrics tracking per document and aggregate
- Confidence score distribution analysis
- Extraction recall monitoring (critical vs. optional fields)
- Hallucination detection indicators (future enhancement placeholder)
- Automated quality reports (daily aggregation)
- Application Insights custom metrics and alerts
- Quality degradation alerts (schema validity <99%, low confidence rate >20%)

## Dependent Tasks
- EP-006-II: US_045: task_001_db_extracted_data_schema (ExtractedClinicalData table)
- EP-006-II: US_045: task_003_be_verification_workflow_integration (Data persistence)
- EP-006-II: US_046: task_002_be_queue_burst_handling (QueueMonitoringJob pattern)

## Impacted Components
- **NEW**: `src/backend/PatientAccess.Business/Validators/ExtractedDataValidator.cs` - Schema validation rules
- **NEW**: `src/backend/PatientAccess.Business/Validators/VitalDataValidator.cs` - Vital-specific validation
- **NEW**: `src/backend/PatientAccess.Business/Validators/MedicationDataValidator.cs` - Medication-specific validation
- **NEW**: `src/backend/PatientAccess.Business/Services/IQualityMonitoringService.cs` - Quality metrics interface
- **NEW**: `src/backend/PatientAccess.Business/Services/QualityMonitoringService.cs` - Quality tracking and reporting
- **NEW**: `src/backend/PatientAccess.Business/BackgroundJobs/QualityReportingJob.cs` - Daily quality aggregation
- **NEW**: `src/backend/PatientAccess.Business/DTOs/QualityMetricsDto.cs` - Quality report DTO
- **MODIFY**: `src/backend/PatientAccess.Business/Services/ClinicalDataExtractionService.cs` - Add validation calls
- **MODIFY**: `src/backend/PatientAccess.Web/Program.cs` - Register validators and quality service
- **NEW**: `src/backend/PatientAccess.Tests/Validators/ExtractedDataValidatorTests.cs` - Unit tests

## Implementation Plan

1. **Add FluentValidation NuGet Package**
   - Install `FluentValidation` version 11.x
   - Install `FluentValidation.DependencyInjectionExtensions` for DI integration

2. **Create Base ExtractedDataValidator**
   - Create `ExtractedDataValidator.cs` with common rules:
     - DocumentId: Required, must be valid GUID
     - PatientId: Required, must be valid integer
     - DataType: Required, must be valid enum value (Vital, Medication, Allergy, Diagnosis, LabResult)
     - Value: Required, MaxLength(1000)
     - ConfidenceScore: Range(0, 100)
     - SourcePageNumber: Range(1, 10000)
     - SourceTextExcerpt: MaxLength(1000)
     - VerificationStatus: Required, must be valid enum value
     - StructuredData: Required (must be valid JSON if present)
   - Use FluentValidation `RuleFor` syntax

3. **Create Data Type-Specific Validators**
   - **VitalDataValidator**: Extends ExtractedDataValidator
     - Validate StructuredData contains required fields: type, value, unit
     - Validate vital types: "blood_pressure", "heart_rate", "temperature", "weight", "height", "bmi", "o2_saturation"
     - Validate value ranges (e.g., heart_rate: 30-250, temperature: 95-106°F)
   - **MedicationDataValidator**: Extends ExtractedDataValidator
     - Validate StructuredData contains: drug_name, dosage, frequency
     - Validate drug_name non-empty
     - Validate dosage format (e.g., "10mg", "1 tablet")
   - **AllergyDataValidator**, **DiagnosisDataValidator**, **LabResultValidator**: Similar patterns

4. **Integrate Validation in ClinicalDataExtractionService**
   - Inject `IValidator<ExtractedClinicalData>` array (all validators)
   - Before persisting extracted data:
     - Select appropriate validator based on DataType
     - Call `validator.ValidateAsync(entity)`
     - If validation fails:
       * Log validation errors with correlation ID
       * Track schema validity failure in quality metrics
       * Set entity RequiresManualReview = true (store despite failure)
       * Continue processing remaining data points
   - Track validation results: pass count, fail count

5. **Create QualityMonitoringService**
   - Implement `IQualityMonitoringService` with methods:
     - `TrackSchemaValidationAsync(Guid documentId, bool isValid, string errors = null)` - Track per-document validation
     - `TrackConfidenceDistributionAsync(Guid documentId, List<int> confidenceScores)` - Analyze confidence spread
     - `TrackExtractionRecallAsync(Guid documentId, Dictionary<string, int> criticalFieldCounts)` - Monitor recall for critical fields
     - `GetQualityMetricsAsync(DateTime fromDate, DateTime toDate)` - Return aggregate quality report
   - Store metrics in Application Insights custom events and metrics

6. **Implement Quality Metrics Tracking**
   - Track custom Application Insights metrics:
     - `extraction_schema_validity_rate` (percentage, target >99%)
     - `extraction_confidence_avg` (average confidence score across all extractions)
     - `extraction_low_confidence_rate` (percentage of data points <50% confidence)
     - `extraction_recall_medications` (percentage of documents with medications extracted)
     - `extraction_recall_allergies` (percentage of documents with allergies extracted)
   - Track custom events:
     - `SchemaValidationFailed` (document ID, data type, validation errors)
     - `LowConfidenceExtraction` (document ID, data type, confidence score)
     - `QualityThresholdBreached` (metric name, actual value, threshold)

7. **Create QualityReportingJob**
   - Implement background job that runs daily (midnight UTC)
   - Tasks:
     - Query ExtractedClinicalData for last 24 hours
     - Calculate aggregate quality metrics:
       * Schema validity rate (valid records / total records)
       * Average confidence score
       * Low confidence rate (records <50% / total)
       * Extraction recall per data type
     - If schema validity < 99%: trigger Application Insights alert
     - If low confidence rate > 20%: trigger alert
     - Store daily report in database table (QualityReports)
     - Log summary to console and Application Insights
   - Schedule via Hangfire: `RecurringJob.AddOrUpdate("quality-reporting", () => job.GenerateDailyReportAsync(), Cron.Daily(0))`

8. **Add Quality Metrics API Endpoint**
   - Add controller endpoint: `GET /api/admin/quality/metrics?fromDate={date}&toDate={date}`
   - Requires Admin role authorization
   - Return JSON:
     ```json
     {
       "period": { "from": "2026-03-20", "to": "2026-03-23" },
       "schemaValidityRate": 99.3,
       "averageConfidence": 87.5,
       "lowConfidenceRate": 8.2,
       "extractionRecall": {
         "medications": 96.7,
         "allergies": 94.1,
         "vitals": 98.3
       },
       "totalDocumentsProcessed": 142
     }
     ```
   - Use QualityMonitoringService for data retrieval

## Current Project State

```
src/backend/
├── PatientAccess.Business/
│   ├── Services/
│   │   ├── ClinicalDataExtractionService.cs (from US_045, to be enhanced)
│   │   └── DocumentProcessingService.cs (from US_043)
│   ├── BackgroundJobs/
│   │   ├── DocumentProcessingJob.cs (from US_043)
│   │   └── QueueMonitoringJob.cs (from task_002)
│   └── DTOs/
│       └── ExtractedDataPointDto.cs (from US_045)
├── PatientAccess.Data/
│   └── Entities/
│       └── ExtractedClinicalData.cs (from US_045)
└── PatientAccess.Web/
    ├── Program.cs (to be modified)
    └── Controllers/
        └── AdminController.cs (from task_002)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/Validators/ExtractedDataValidator.cs | Base validation rules |
| CREATE | src/backend/PatientAccess.Business/Validators/VitalDataValidator.cs | Vital-specific validation |
| CREATE | src/backend/PatientAccess.Business/Validators/MedicationDataValidator.cs | Medication-specific validation |
| CREATE | src/backend/PatientAccess.Business/Services/IQualityMonitoringService.cs | Quality metrics interface |
| CREATE | src/backend/PatientAccess.Business/Services/QualityMonitoringService.cs | Quality tracking implementation |
| CREATE | src/backend/PatientAccess.Business/BackgroundJobs/QualityReportingJob.cs | Daily quality report generation |
| CREATE | src/backend/PatientAccess.Business/DTOs/QualityMetricsDto.cs | Quality report DTO |
| CREATE | src/backend/PatientAccess.Tests/Validators/ExtractedDataValidatorTests.cs | Validator unit tests |
| MODIFY | src/backend/PatientAccess.Business/Services/ClinicalDataExtractionService.cs | Add validation logic |
| MODIFY | src/backend/PatientAccess.Web/Controllers/AdminController.cs | Add quality metrics endpoint |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register validators and quality service |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### FluentValidation Documentation
- **Getting Started**: https://docs.fluentvalidation.net/en/latest/
- **Built-in Validators**: https://docs.fluentvalidation.net/en/latest/built-in-validators.html
- **Custom Validators**: https://docs.fluentvalidation.net/en/latest/custom-validators.html
- **Dependency Injection**: https://docs.fluentvalidation.net/en/latest/di.html
- **Testing**: https://docs.fluentvalidation.net/en/latest/testing.html

### Application Insights Metrics
- **Custom Metrics**: https://learn.microsoft.com/en-us/azure/azure-monitor/app/api-custom-events-metrics
- **Alerts**: https://learn.microsoft.com/en-us/azure/azure-monitor/alerts/alerts-overview
- **Telemetry**: https://learn.microsoft.com/en-us/azure/azure-monitor/app/api-filtering-sampling

### Design Requirements
- **AIR-Q02**: System MUST complete AI inference operations within 5 seconds per document page for 95th percentile requests (design.md)
- **AIR-Q03**: System MUST achieve output schema validity rate above 99% for structured AI responses (design.md)
- **AIR-Q04**: System MUST maintain hallucination rate below 2% on clinical document extraction evaluation set (design.md)
- **AIR-Q05**: System MUST achieve extraction recall above 95% for critical clinical data elements (medications, allergies) (design.md)
- **NFR-007**: System MUST log all data access, changes, and authentication events for audit compliance (design.md)
- **NFR-011**: System MUST maintain application error rate below 0.1% of requests (design.md)

### Existing Codebase Patterns
- **Background Job Pattern**: `src/backend/PatientAccess.Business/BackgroundJobs/QueueMonitoringJob.cs`
- **Service Pattern**: `src/backend/PatientAccess.Business/Services/ClinicalDataExtractionService.cs`
- **DTO Pattern**: `src/backend/PatientAccess.Business/DTOs/ExtractedDataPointDto.cs`

## Build Commands
```powershell
# Add FluentValidation NuGet packages
cd src/backend
dotnet add PatientAccess.Business package FluentValidation
dotnet add PatientAccess.Business package FluentValidation.DependencyInjectionExtensions

# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test

# Run application
cd PatientAccess.Web
dotnet run

# Test quality metrics endpoint (requires Admin auth)
$token = "your-admin-jwt-token"
$fromDate = "2026-03-20"
$toDate = "2026-03-23"
Invoke-WebRequest -Uri "http://localhost:5000/api/admin/quality/metrics?fromDate=$fromDate&toDate=$toDate" -Method Get -Headers @{ Authorization = "Bearer $token" }
```

## Implementation Validation Strategy
- [ ] Unit tests pass (validators, quality metrics calculation)
- [ ] Integration tests pass (end-to-end validation during extraction)
- [ ] FluentValidation rules correctly validate ExtractedClinicalData
- [ ] Data type-specific validators enforce correct structured data schemas
- [ ] Schema validation failures tracked in Application Insights
- [ ] Invalid data still persisted with RequiresManualReview = true
- [ ] Quality metrics endpoint returns correct aggregate statistics
- [ ] QualityReportingJob runs daily and generates reports
- [ ] Application Insights alerts triggered when quality thresholds breached
- [ ] Schema validity rate calculation correct (valid / total)
- [ ] Confidence distribution analysis accurate
- [ ] Extraction recall tracked per data type (medications, allergies)
- [ ] Quality degradation detected and alerted (validity <99%, low confidence >20%)

## Implementation Checklist
- [ ] Add FluentValidation and FluentValidation.DependencyInjectionExtensions packages
- [ ] Create ExtractedDataValidator with common validation rules
- [ ] Create data type-specific validators (VitalDataValidator, MedicationDataValidator, etc.)
- [ ] Integrate validation in ClinicalDataExtractionService.ExtractClinicalDataAsync
- [ ] Create IQualityMonitoringService interface
- [ ] Implement QualityMonitoringService with metrics tracking methods
- [ ] Create QualityReportingJob for daily quality aggregation
- [ ] Add GET /api/admin/quality/metrics endpoint with Admin authorization
- [ ] Register validators and quality service in Program.cs
- [ ] Schedule QualityReportingJob as Hangfire recurring job
- [ ] Track custom Application Insights metrics (schema validity, confidence avg, recall)
- [ ] Write unit tests for validators and quality metrics calculation
