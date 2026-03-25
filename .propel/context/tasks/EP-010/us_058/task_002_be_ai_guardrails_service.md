# Task - task_002_be_ai_guardrails_service

## Requirement Reference
- User Story: US_058
- Story Location: .propel/context/tasks/EP-010/us_058/us_058.md
- Acceptance Criteria:
    - **AC1**: Given the human-in-the-loop requirement (AIR-S01), When AI generates any clinical suggestion, Then the output is flagged as "AI-Suggested" and cannot be committed to the patient record without explicit staff verification.
    - **AC2**: Given confidence thresholds (AIR-S02), When an AI extraction has a confidence score below the defined threshold, Then it is auto-flagged for mandatory manual review and highlighted with a warning indicator.

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
| Frontend | N/A | N/A |
| Backend | ASP.NET Core | 8.0 |
| Backend | C# | 12.0 |
| Backend | Azure.AI.OpenAI | 2.0+ |
| Database | N/A | N/A |
| Caching | N/A | N/A |
| Vector Store | N/A | N/A |
| AI Gateway | Azure OpenAI | GPT-4o |
| Mobile | N/A | N/A |

**Note**: All code, and libraries, MUST be compatible with versions above.

## AI References (AI Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **AI Impact** | Yes |
| **AIR Requirements** | AIR-S01, AIR-S02, AIR-S03 |
| **AI Pattern** | Human-in-the-loop verification, Confidence thresholds, Fallback to manual |
| **Prompt Template Path** | N/A |
| **Guardrails Config** | appsettings.json (ConfidenceThreshold: 0.7) |
| **Model Provider** | Azure OpenAI (GPT-4o), Azure AI Document Intelligence |

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

Implement AI guardrails service to enforce human-in-the-loop verification and confidence threshold validation for all AI-generated clinical suggestions. This task creates AIGuardrailsService with methods to evaluate confidence scores, auto-flag low-confidence extractions for mandatory manual review (AC2: threshold 0.7 per AIR-S03), and enforce that AI suggestions cannot be committed to patient records without explicit staff verification (AC1). Service integrates with ExtractedClinicalData entities from task_001, sets IsAISuggested flag, calculates RequiresManualReview based on configurable threshold, and provides validation before database persistence. Includes ConfidenceThresholdEvaluator for threshold logic, VerificationWorkflowService for staff verification state management, and comprehensive unit tests.

**Key Capabilities:**
- AIGuardrailsService for guardrail enforcement (AC1, AC2)
- EvaluateConfidence() method checks threshold (default 0.7 per AIR-S03)
- Auto-flag RequiresManualReview when confidence < threshold (AC2)
- ValidateBeforeCommit() prevents uncommitted AI suggestions (AC1)
- ConfidenceThresholdEvaluator with configurable thresholds per data type
- VerificationWorkflowService manages staff verification state
- Integration with ExtractedClinicalData entities (task_001)
- Configurable thresholds in appsettings.json
- Comprehensive logging for audit trail

## Dependent Tasks
- EP-010: US_058: task_001 (AI metadata schema - VerificationStatus, ConfidenceScore fields)

## Impacted Components
- **NEW**: `src/backend/PatientAccess.Business/Services/AIGuardrailsService.cs` - Guardrails enforcement
- **NEW**: `src/backend/PatientAccess.Business/Interfaces/IAIGuardrailsService.cs` - Service interface
- **NEW**: `src/backend/PatientAccess.Business/Services/ConfidenceThresholdEvaluator.cs` - Threshold logic
- **NEW**: `src/backend/PatientAccess.Business/Services/VerificationWorkflowService.cs` - Verification state management
- **NEW**: `docs/AI_GUARDRAILS.md` - Guardrails documentation
- **MODIFY**: `src/backend/PatientAccess.Web/Program.cs` - Register guardrails service
- **MODIFY**: `src/backend/PatientAccess.Web/appsettings.json` - Add confidence threshold configuration

## Implementation Plan

1. **Create IAIGuardrailsService Interface**
   - File: `src/backend/PatientAccess.Business/Interfaces/IAIGuardrailsService.cs`
   - Service interface:
     ```csharp
     namespace PatientAccess.Business.Interfaces
     {
         /// <summary>
         /// AI guardrails service for confidence threshold evaluation and human-in-the-loop enforcement (AC1, AC2 - US_058).
         /// </summary>
         public interface IAIGuardrailsService
         {
             /// <summary>
             /// Evaluates AI extraction confidence and auto-flags for manual review if below threshold (AC2).
             /// </summary>
             Task<ExtractedClinicalData> EvaluateAndFlagAsync(
                 ExtractedClinicalData extraction,
                 CancellationToken cancellationToken = default);
             
             /// <summary>
             /// Validates that AI-suggested data cannot be committed without staff verification (AC1).
             /// Throws InvalidOperationException if validation fails.
             /// </summary>
             void ValidateBeforeCommit(ExtractedClinicalData extraction);
             
             /// <summary>
             /// Records staff verification for AI-suggested data (AC1).
             /// </summary>
             Task<ExtractedClinicalData> RecordVerificationAsync(
                 int extractionId,
                 int staffUserId,
                 VerificationStatus status,
                 string? correctedValue = null,
                 CancellationToken cancellationToken = default);
         }
     }
     ```

2. **Create ConfidenceThresholdEvaluator**
   - File: `src/backend/PatientAccess.Business/Services/ConfidenceThresholdEvaluator.cs`
   - Threshold evaluation logic:
     ```csharp
     using Microsoft.Extensions.Configuration;
     
     namespace PatientAccess.Business.Services
     {
         /// <summary>
         /// Evaluates confidence scores against configurable thresholds (AC2 - US_058).
         /// </summary>
         public class ConfidenceThresholdEvaluator
         {
             private readonly IConfiguration _configuration;
             private readonly ILogger<ConfidenceThresholdEvaluator> _logger;
             
             // Default threshold from AIR-S03: 70%
             private const decimal DefaultConfidenceThreshold = 0.7m;
             
             public ConfidenceThresholdEvaluator(
                 IConfiguration configuration,
                 ILogger<ConfidenceThresholdEvaluator> logger)
             {
                 _configuration = configuration;
                 _logger = logger;
             }
             
             /// <summary>
             /// Determines if extraction requires manual review based on confidence threshold (AC2).
             /// </summary>
             public bool RequiresManualReview(string dataType, decimal? confidenceScore)
             {
                 if (!confidenceScore.HasValue)
                 {
                     // No confidence score = requires manual review
                     _logger.LogWarning(
                         "No confidence score provided for {DataType}. Flagging for manual review.",
                         dataType);
                     return true;
                 }
                 
                 var threshold = GetThresholdForDataType(dataType);
                 var requiresReview = confidenceScore.Value < threshold;
                 
                 if (requiresReview)
                 {
                     _logger.LogInformation(
                         "Confidence score {Score} below threshold {Threshold} for {DataType}. Flagging for manual review (AC2 - US_058).",
                         confidenceScore.Value, threshold, dataType);
                 }
                 
                 return requiresReview;
             }
             
             /// <summary>
             /// Gets confidence threshold for specific data type.
             /// Allows per-data-type thresholds (e.g., stricter for medications).
             /// </summary>
             private decimal GetThresholdForDataType(string dataType)
             {
                 // Check for data type-specific threshold
                 var specificThreshold = _configuration.GetValue<decimal?>(
                     $"AIGuardrails:ConfidenceThresholds:{dataType}");
                 
                 if (specificThreshold.HasValue)
                 {
                     return specificThreshold.Value;
                 }
                 
                 // Fallback to general threshold
                 var generalThreshold = _configuration.GetValue<decimal?>(
                     "AIGuardrails:ConfidenceThresholds:Default");
                 
                 return generalThreshold ?? DefaultConfidenceThreshold;
             }
             
             /// <summary>
             /// Gets warning level for confidence score.
             /// </summary>
             public string GetWarningLevel(decimal? confidenceScore)
             {
                 if (!confidenceScore.HasValue)
                     return "Critical"; // No score = critical
                 
                 if (confidenceScore.Value < 0.5m)
                     return "Critical"; // < 50%
                 
                 if (confidenceScore.Value < 0.7m)
                     return "Warning"; // 50-70%
                 
                 if (confidenceScore.Value < 0.9m)
                     return "Info"; // 70-90%
                 
                 return "None"; // >= 90%
             }
         }
     }
     ```

3. **Create VerificationWorkflowService**
   - File: `src/backend/PatientAccess.Business/Services/VerificationWorkflowService.cs`
   - Staff verification state management:
     ```csharp
     using PatientAccess.Data;
     using Microsoft.EntityFrameworkCore;
     
     namespace PatientAccess.Business.Services
     {
         /// <summary>
         /// Manages staff verification workflow for AI-suggested data (AC1 - US_058).
         /// </summary>
         public class VerificationWorkflowService
         {
             private readonly AppDbContext _context;
             private readonly ILogger<VerificationWorkflowService> _logger;
             
             public VerificationWorkflowService(
                 AppDbContext context,
                 ILogger<VerificationWorkflowService> logger)
             {
                 _context = context;
                 _logger = logger;
             }
             
             /// <summary>
             /// Records staff verification for AI extraction (AC1).
             /// </summary>
             public async Task<ExtractedClinicalData> RecordVerificationAsync(
                 int extractionId,
                 int staffUserId,
                 VerificationStatus status,
                 string? correctedValue = null,
                 CancellationToken cancellationToken = default)
             {
                 var extraction = await _context.ExtractedClinicalData
                     .FirstOrDefaultAsync(e => e.Id == extractionId, cancellationToken);
                 
                 if (extraction == null)
                 {
                     throw new InvalidOperationException($"Extraction {extractionId} not found.");
                 }
                 
                 // Update verification fields (AC1)
                 extraction.VerificationStatus = status;
                 extraction.VerifiedBy = staffUserId;
                 extraction.VerifiedAt = DateTime.UtcNow;
                 
                 // If manually edited, update value
                 if (status == VerificationStatus.ManuallyEdited && !string.IsNullOrEmpty(correctedValue))
                 {
                     extraction.Value = correctedValue;
                 }
                 
                 await _context.SaveChangesAsync(cancellationToken);
                 
                 _logger.LogInformation(
                     "Staff user {StaffUserId} verified extraction {ExtractionId} with status {Status} (AC1 - US_058).",
                     staffUserId, extractionId, status);
                 
                 return extraction;
             }
             
             /// <summary>
             /// Gets all extractions pending staff verification.
             /// </summary>
             public async Task<List<ExtractedClinicalData>> GetPendingVerificationsAsync(
                 int? patientId = null,
                 CancellationToken cancellationToken = default)
             {
                 var query = _context.ExtractedClinicalData
                     .Where(e => e.IsAISuggested && e.VerificationStatus == VerificationStatus.Pending);
                 
                 if (patientId.HasValue)
                 {
                     query = query.Where(e => e.ClinicalDocument.PatientId == patientId.Value);
                 }
                 
                 return await query
                     .Include(e => e.ClinicalDocument)
                     .OrderBy(e => e.RequiresManualReview ? 0 : 1) // Priority: manual review first
                     .ThenByDescending(e => e.ConfidenceScore) // Then by confidence (lowest first)
                     .ToListAsync(cancellationToken);
             }
         }
     }
     ```

4. **Create AIGuardrailsService**
   - File: `src/backend/PatientAccess.Business/Services/AIGuardrailsService.cs`
   - Main guardrails enforcement service:
     ```csharp
     using PatientAccess.Business.Interfaces;
     using PatientAccess.Data.Entities;
     using PatientAccess.Business.Enums;
     
     namespace PatientAccess.Business.Services
     {
         /// <summary>
         /// AI guardrails service enforcing human-in-the-loop and confidence thresholds (AC1, AC2 - US_058).
         /// </summary>
         public class AIGuardrailsService : IAIGuardrailsService
         {
             private readonly ConfidenceThresholdEvaluator _thresholdEvaluator;
             private readonly VerificationWorkflowService _verificationWorkflow;
             private readonly ILogger<AIGuardrailsService> _logger;
             
             public AIGuardrailsService(
                 ConfidenceThresholdEvaluator thresholdEvaluator,
                 VerificationWorkflowService verificationWorkflow,
                 ILogger<AIGuardrailsService> logger)
             {
                 _thresholdEvaluator = thresholdEvaluator;
                 _verificationWorkflow = verificationWorkflow;
                 _logger = logger;
             }
             
             /// <summary>
             /// Evaluates AI extraction confidence and auto-flags for manual review (AC2 - US_058).
             /// </summary>
             public Task<ExtractedClinicalData> EvaluateAndFlagAsync(
                 ExtractedClinicalData extraction,
                 CancellationToken cancellationToken = default)
             {
                 // Set AI-suggested flag (AC1)
                 extraction.IsAISuggested = true;
                 
                 // Evaluate confidence threshold (AC2)
                 var requiresReview = _thresholdEvaluator.RequiresManualReview(
                     extraction.DataType,
                     extraction.ConfidenceScore);
                 
                 extraction.RequiresManualReview = requiresReview;
                 
                 // Set initial verification status
                 extraction.VerificationStatus = VerificationStatus.Pending;
                 
                 // Log guardrail evaluation
                 var warningLevel = _thresholdEvaluator.GetWarningLevel(extraction.ConfidenceScore);
                 
                 _logger.LogInformation(
                     "AI extraction evaluated: DataType={DataType}, Confidence={Confidence}, " +
                     "RequiresManualReview={RequiresReview}, WarningLevel={WarningLevel} (AC2 - US_058)",
                     extraction.DataType,
                     extraction.ConfidenceScore,
                     requiresReview,
                     warningLevel);
                 
                 return Task.FromResult(extraction);
             }
             
             /// <summary>
             /// Validates that AI-suggested data cannot be committed without verification (AC1 - US_058).
             /// </summary>
             public void ValidateBeforeCommit(ExtractedClinicalData extraction)
             {
                 // Check if AI-suggested data is unverified (AC1)
                 if (extraction.IsAISuggested && extraction.VerificationStatus == VerificationStatus.Pending)
                 {
                     throw new InvalidOperationException(
                         $"AI-suggested data (ID: {extraction.Id}) cannot be committed to patient record " +
                         $"without explicit staff verification (AC1 - US_058).");
                 }
                 
                 _logger.LogDebug(
                     "Validation passed: Extraction {Id} verified by staff (AC1 - US_058).",
                     extraction.Id);
             }
             
             /// <summary>
             /// Records staff verification for AI-suggested data (AC1 - US_058).
             /// </summary>
             public async Task<ExtractedClinicalData> RecordVerificationAsync(
                 int extractionId,
                 int staffUserId,
                 VerificationStatus status,
                 string? correctedValue = null,
                 CancellationToken cancellationToken = default)
             {
                 return await _verificationWorkflow.RecordVerificationAsync(
                     extractionId,
                     staffUserId,
                     status,
                     correctedValue,
                     cancellationToken);
             }
         }
     }
     ```

5. **Configure Confidence Thresholds**
   - File: `src/backend/PatientAccess.Web/appsettings.json`
   - Add guardrails configuration:
     ```json
     {
       "AIGuardrails": {
         "ConfidenceThresholds": {
           "Default": 0.7,
           "Medication": 0.8,
           "Allergy": 0.8,
           "Vital": 0.7,
           "Diagnosis": 0.75,
           "LabResult": 0.7
         },
         "EnableStrictMode": false
       }
     }
     ```

6. **Register Services in Program.cs**
   - File: `src/backend/PatientAccess.Web/Program.cs`
   - Service registration:
     ```csharp
     // Register AI Guardrails Services (AC1, AC2 - US_058)
     builder.Services.AddScoped<ConfidenceThresholdEvaluator>();
     builder.Services.AddScoped<VerificationWorkflowService>();
     builder.Services.AddScoped<IAIGuardrailsService, AIGuardrailsService>();
     ```

7. **Document AI Guardrails**
   - File: `docs/AI_GUARDRAILS.md`
   - Comprehensive guardrails documentation:
     ```markdown
     # AI Guardrails (AC1, AC2 - US_058)
     
     ## Overview
     All AI-generated clinical suggestions are subject to mandatory guardrails ensuring human-in-the-loop verification and confidence threshold validation before commitment to patient records.
     
     ## Human-in-the-Loop (AC1 - AIR-S01)
     
     ### Requirement
     AI-generated clinical data CANNOT be committed to patient records without explicit staff verification.
     
     ### Implementation
     - **IsAISuggested Flag**: All AI extractions marked with `IsAISuggested = true`
     - **VerificationStatus**: Tracks verification lifecycle (Pending → Verified/ManuallyEdited/Rejected)
     - **ValidateBeforeCommit()**: Throws exception if unverified AI data attempted for commit
     - **Staff Verification**: Requires explicit staff action to approve, edit, or reject AI suggestions
     
     ### Example Workflow
     ```
     1. AI extracts medication: "Lisinopril 10mg daily"
        - IsAISuggested = true
        - ConfidenceScore = 0.92
        - VerificationStatus = Pending
     
     2. Staff reviews suggestion:
        - Option A: Verify as correct → VerificationStatus = Verified
        - Option B: Edit value → VerificationStatus = ManuallyEdited
        - Option C: Reject and re-enter → VerificationStatus = Rejected
     
     3. Only after staff action can data be committed to patient record
     ```
     
     ## Confidence Thresholds (AC2 - AIR-S02)
     
     ### Requirement
     Extractions with confidence scores below defined thresholds are auto-flagged for mandatory manual review.
     
     ### Default Thresholds (AIR-S03)
     - **Default**: 0.7 (70%)
     - **Medication**: 0.8 (80%) - stricter due to clinical risk
     - **Allergy**: 0.8 (80%) - stricter due to clinical risk
     - **Vital**: 0.7 (70%)
     - **Diagnosis**: 0.75 (75%)
     - **LabResult**: 0.7 (70%)
     
     ### Auto-Flagging Logic
     ```csharp
     if (confidenceScore < threshold)
     {
         extraction.RequiresManualReview = true;
         // Highlighted with warning indicator in UI
     }
     ```
     
     ### Warning Levels
     - **Critical** (< 50%): Red indicator, mandatory review
     - **Warning** (50-70%): Amber indicator, requires attention
     - **Info** (70-90%): Blue indicator, review recommended
     - **None** (>= 90%): Green indicator, high confidence
     
     ## Configuration
     
     ### appsettings.json
     ```json
     {
       "AIGuardrails": {
         "ConfidenceThresholds": {
           "Default": 0.7,
           "Medication": 0.8
         }
       }
     }
     ```
     
     ### Customization
     Confidence thresholds can be adjusted per data type based on clinical risk assessment and validation accuracy metrics.
     
     ## API Usage
     
     ### Evaluate AI Ex traction
     ```csharp
     var extraction = new ExtractedClinicalData
     {
         DataType = "Medication",
         Value = "Lisinopril 10mg",
         ConfidenceScore = 0.75m
     };
     
     // Apply guardrails
     extraction = await _guardrailsService.EvaluateAndFlagAsync(extraction);
     
     // Result:
     // - IsAISuggested = true
     // - RequiresManualReview = true (0.75 < 0.8 for Medication)
     // - VerificationStatus = Pending
     ```
     
     ### Record Staff Verification
     ```csharp
     var verified = await _guardrailsService.RecordVerificationAsync(
         extractionId: 123,
         staffUserId: 456,
         status: VerificationStatus.Verified
     );
     
     // Result: VerificationStatus = Verified, VerifiedBy = 456, VerifiedAt = <timestamp>
     ```
     
     ### Validate Before Commit
     ```csharp
     try
     {
         _guardrailsService.ValidateBeforeCommit(extraction);
         // Validation passed - proceed with commit
     }
     catch (InvalidOperationException ex)
     {
         // AC1 violation: AI-suggested data not yet verified
         // Cannot commit to patient record
     }
     ```
     
     ## Compliance
     - **AIR-S01**: Human-in-the-loop enforced (AC1)
     - **AIR-S02**: Confidence threshold validation (AC2)
     - **AIR-S03**: 70% threshold for fallback to manual (AC2)
     ```

## Current Project State

```
src/backend/
├── PatientAccess.Business/
│   ├── Services/
│   ├── Interfaces/
│   └── Enums/
│       └── VerificationStatus.cs (from task_001)
├── PatientAccess.Data/
│   └── Entities/
│       └── ExtractedClinicalData.cs (from task_001 with AI metadata)
└── PatientAccess.Web/
    ├── Program.cs
    └── appsettings.json
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/Services/AIGuardrailsService.cs | Guardrails enforcement |
| CREATE | src/backend/PatientAccess.Business/Interfaces/IAIGuardrailsService.cs | Service interface |
| CREATE | src/backend/PatientAccess.Business/Services/ConfidenceThresholdEvaluator.cs | Threshold evaluation |
| CREATE | src/backend/PatientAccess.Business/Services/VerificationWorkflowService.cs | Verification workflow |
| CREATE | docs/AI_GUARDRAILS.md | Guardrails documentation |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register guardrails services |
| MODIFY | src/backend/PatientAccess.Web/appsettings.json | Add confidence thresholds |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### AI Safety Best Practices
- **Human-in-the-Loop Systems**: https://en.wikipedia.org/wiki/Human-in-the-loop
- **Confidence Thresholds**: https://machinelearningmastery.com/threshold-moving-for-imbalanced-classification/

### Design Requirements
- **AIR-S01**: Human-in-the-loop for clinical AI (design.md)
- **AIR-S02**: Confidence threshold flagging (design.md)
- **AIR-S03**: 70% threshold for fallback to manual (design.md)

## Build Commands
```powershell
# Build solution
cd src/backend
dotnet build PatientAccess.sln

# Run tests
dotnet test PatientAccess.Tests/PatientAccess.Tests.csproj

# Run backend
cd PatientAccess.Web
dotnet run
```

## Validation Strategy

### Unit Tests
- File: `src/backend/PatientAccess.Tests/Services/AIGuardrailsServiceTests.cs`
- Test cases:
  1. **Test_EvaluateAndFlag_SetsIsAISuggested**
     - Input: ExtractedClinicalData with confidence=0.85
     - Call: EvaluateAndFlagAsync()
     - Assert: IsAISuggested = true, VerificationStatus = Pending
  2. **Test_EvaluateAndFlag_AutoFlagsLowConfidence**
     - Input: Medication extraction with confidence=0.65 (threshold=0.8)
     - Call: EvaluateAndFlagAsync()
     - Assert: RequiresManualReview = true (AC2)
  3. **Test_EvaluateAndFlag_DoesNotFlagHighConfidence**
     - Input: Vital extraction with confidence=0.85 (threshold=0.7)
     - Call: EvaluateAndFlagAsync()
     - Assert: RequiresManualReview = false
  4. **Test_ValidateBeforeCommit_ThrowsForUnverified**
     - Input: AI-suggested extraction with VerificationStatus=Pending
     - Call: ValidateBeforeCommit()
     - Assert: Throws InvalidOperationException (AC1)
  5. **Test_ValidateBeforeCommit_AllowsVerified**
     - Input: AI-suggested extraction with VerificationStatus=Verified
     - Call: ValidateBeforeCommit()
     - Assert: No exception thrown
  6. **Test_RecordVerification_UpdatesVerificationFields**
     - Setup: Pending AI extraction
     - Call: RecordVerificationAsync(extractionId, staffId, Verified)
     - Assert: VerificationStatus=Verified, VerifiedBy=staffId, VerifiedAt!=null

### Integration Tests
- File: `src/backend/PatientAccess.Tests/Integration/AIGuardrailsIntegrationTests.cs`
- Test cases:
  1. **Test_EvaluateAndSave_LowConfidence_FlagsForReview**
     - Create extraction with confidence=0.6
     - Call EvaluateAndFlagAsync() and save to DB
     - Query DB: Assert RequiresManualReview=true
  2. **Test_VerificationWorkflow_EndToEnd**
     - Create pending AI extraction
     - Record staff verification
     - Query DB: Assert VerificationStatus updated, VerifiedBy populated

### Acceptance Criteria Validation
- **AC1**: ✅ AI suggestions flagged, cannot commit without staff verification, ValidateBeforeCommit enforces
- **AC2**: ✅ Low-confidence extractions auto-flagged for manual review based on configurable thresholds

## Success Criteria Checklist
- [MANDATORY] IAIGuardrailsService interface created
- [MANDATORY] AIGuardrailsService implemented with EvaluateAndFlagAsync() (AC2)
- [MANDATORY] ValidateBeforeCommit() prevents unverified AI commits (AC1)
- [MANDATORY] RecordVerificationAsync() manages staff verification (AC1)
- [MANDATORY] ConfidenceThresholdEvaluator with per-data-type thresholds
- [MANDATORY] RequiresManualReview() logic using configurable thresholds (AC2)
- [MANDATORY] GetWarningLevel() returns Critical/Warning/Info/None
- [MANDATORY] VerificationWorkflowService manages verification state
- [MANDATORY] RecordVerificationAsync() updates VerifiedBy/VerifiedAt fields
- [MANDATORY] GetPendingVerificationsAsync() queries unverified extractions
- [MANDATORY] Confidence thresholds configured in appsettings.json (Default=0.7, Medication=0.8)
- [MANDATORY] Services registered in Program.cs
- [MANDATORY] AI_GUARDRAILS.md documentation with workflow examples
- [MANDATORY] Unit test: EvaluateAndFlag sets IsAISuggested and RequiresManualReview
- [MANDATORY] Unit test: ValidateBeforeCommit throws for unverified AI data
- [MANDATORY] Unit test: RecordVerification updates verification fields
- [RECOMMENDED] Integration test: End-to-end verification workflow
- [RECOMMENDED] Configurable strict mode for additional guardrails

## Estimated Effort
**4 hours** (AIGuardrailsService + ConfidenceThresholdEvaluator + VerificationWorkflowService + configuration + docs + tests)
