# Task - task_001_be_code_mapping_service

## Requirement Reference
- User Story: US_051
- Story Location: .propel/context/tasks/EP-008/us_051/us_051.md
- Acceptance Criteria:
    - **AC1**: Given clinical data is extracted, When the code mapping service runs, Then ICD-10 diagnosis codes are suggested for each diagnosis/condition with confidence scores (0-100%).
    - **AC2**: Given clinical data is extracted, When the code mapping service runs, Then CPT procedure codes are suggested for identified procedures with confidence scores (0-100%).
    - **AC3**: Given the RAG pipeline retrieves context, When code mapping uses Azure OpenAI with HIPAA BAA (TR-015), Then the LLM maps clinical text to codes grounded in retrieved knowledge base chunks, providing code value, description, and rationale.
    - **AC4**: Given quality requirements, When the mapping is evaluated against staff decisions, Then AI-Human Agreement Rate exceeds 98% (AIR-Q01) and output schema validity exceeds 99% (AIR-Q03).
- Edge Case:
    - What happens when clinical text is ambiguous and maps to multiple codes? All plausible codes are returned ranked by confidence, with the top suggestion highlighted.
    - How does the system handle clinical data not mappable to any code? System returns "No matching code found" with recommendation for manual coding.

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
| Library | Azure.AI.OpenAI | 1.0+ |
| Library | FluentValidation | 11.x |
| Library | Polly | 8.x |
| Database | PostgreSQL | 16.x |
| Vector Store | pgvector | 0.5+ |
| AI Gateway | Azure OpenAI Service | GPT-4o, text-embedding-3-small |
| Mobile | N/A | N/A |

**Note**: All code, and libraries, MUST be compatible with versions above.

## AI References (AI Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **AI Impact** | Yes |
| **AIR Requirements** | AIR-003 (ICD-10 mapping via RAG), AIR-004 (CPT mapping via RAG), AIR-Q01 (98% AI-Human Agreement), AIR-Q03 (99% output schema validity) |
| **AI Pattern** | RAG-based Code Mapping with LLM Inference |
| **Prompt Template Path** | .propel/prompts/ai/code-mapping-icd10.txt, .propel/prompts/ai/code-mapping-cpt.txt |
| **Guardrails Config** | AIR-S01 (HIPAA BAA requirement), TR-015 (Azure OpenAI only) |
| **Model Provider** | Azure OpenAI (GPT-4o for code mapping inference) |

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

Create code mapping service to suggest ICD-10 diagnosis codes and CPT procedure codes from extracted clinical data using RAG pattern with Azure OpenAI GPT-4o. This task implements the medical coding pipeline core logic: retrieves relevant knowledge base chunks (from US_050 hybrid retrieval), constructs prompts with clinical context and retrieved codes, invokes GPT-4o for code mapping inference, extracts confidence scores (0-100%), and validates output schema with FluentValidation per AIR-Q03 (>99% validity). The service handles ambiguous text (returns multiple ranked suggestions), unmappable data (returns "No matching code found"), and integrates quality metrics tracking for AIR-Q01 (>98% agreement with staff decisions).

**Key Capabilities:**
- CodeMappingService with ICD-10 and CPT mapping methods
- RAG integration: HybridRetrievalService for knowledge base chunks (top-5 per AIR-R02)
- Azure OpenAI GPT-4o integration with HIPAA BAA (TR-015)
- Prompt templates with system message, retrieved context, clinical text
- Confidence score extraction from LLM response (0-100% range)
- Multiple code ranking for ambiguous cases (sorted by confidence descending)
- "No matching code found" response for unmappable data
- FluentValidation schema validators (CodeMappingResponseValidator)
- Quality metrics tracking: agreement rate, schema validity rate
- Polly circuit breaker and retry logic for Azure OpenAI failures
- Logging and telemetry (Application Insights)

## Dependent Tasks
- EP-008: US_050: task_004_be_hybrid_retrieval_service (IHybridRetrievalService for RAG retrieval)
- EP-006-II: US_045: task_002_be_azure_document_intelligence (ExtractedClinicalData entity)

## Impacted Components
- **NEW**: `src/backend/PatientAccess.Business/Services/CodeMappingService.cs` - Core code mapping logic
- **NEW**: `src/backend/PatientAccess.Business/Interfaces/ICodeMappingService.cs` - Service interface
- **NEW**: `src/backend/PatientAccess.Business/DTOs/CodeMappingRequestDto.cs` - Request DTO
- **NEW**: `src/backend/PatientAccess.Business/DTOs/CodeMappingResponseDto.cs` - Response DTO
- **NEW**: `src/backend/PatientAccess.Business/DTOs/MedicalCodeSuggestionDto.cs` - Individual code suggestion DTO
- **NEW**: `src/backend/PatientAccess.Business/Validators/CodeMappingResponseValidator.cs` - FluentValidation schema validator
- **NEW**: `src/backend/PatientAccess.Business/Configuration/CodeMappingSettings.cs` - Configuration POCO
- **NEW**: `src/backend/PatientAccess.Data/Entities/MedicalCode.cs` - Medical code entity
- **NEW**: `src/backend/PatientAccess.Data/Entities/QualityMetric.cs` - Quality tracking entity
- **NEW**: `src/backend/PatientAccess.Data/Migrations/YYYYMMDDHHMMSS_AddMedicalCodeAndQualityMetrics.cs` - EF migration
- **NEW**: `.propel/prompts/ai/code-mapping-icd10.txt` - ICD-10 mapping prompt template
- **NEW**: `.propel/prompts/ai/code-mapping-cpt.txt` - CPT mapping prompt template
- **MODIFY**: `src/backend/PatientAccess.Data/ApplicationDbContext.cs` - Add MedicalCode, QualityMetric DbSets
- **MODIFY**: `src/backend/PatientAccess.Web/Program.cs` - Register ICodeMappingService, configure settings
- **MODIFY**: `src/backend/PatientAccess.Web/appsettings.json` - Add CodeMapping configuration

## Implementation Plan

1. **Create MedicalCode Entity**
   - File: `src/backend/PatientAccess.Data/Entities/MedicalCode.cs`
   - Properties:
     - Id (Guid, PK)
     - ExtractedClinicalDataId (Guid, FK to ExtractedClinicalData)
     - CodeSystem (string, 50 chars: "ICD10", "CPT")
     - Code (string, 20 chars: "E11.9", "99213")
     - Description (string, 1000 chars: full code description)
     - ConfidenceScore (decimal, 0-100%: LLM confidence in mapping)
     - Rationale (string, 2000 chars: LLM explanation for code selection)
     - Rank (int: 1 for top suggestion, 2-5 for alternatives)
     - IsTopSuggestion (bool: true if Rank == 1)
     - RetrievedContext (string, 5000 chars: RAG chunks used)
     - VerificationStatus (string, 50 chars: "Pending", "StaffVerified", "StaffRejected", "AutoAccepted")
     - VerifiedBy (int, FK to Users, nullable)
     - VerifiedAt (DateTime, nullable)
     - CreatedAt (DateTime, UTC)
   - Indexes:
     - ExtractedClinicalDataId (B-tree for FK lookup)
     - CodeSystem (B-tree for filtering)
     - VerificationStatus (B-tree for pending workflow)
     - IsTopSuggestion (B-tree for filtering primary suggestions)

2. **Create QualityMetric Entity**
   - File: `src/backend/PatientAccess.Data/Entities/QualityMetric.cs`
   - Properties:
     - Id (Guid, PK)
     - MetricType (string, 50 chars: "AIHumanAgreement", "SchemaValidity", "ConfidenceAccuracy")
     - MetricValue (decimal: percentage 0-100)
     - SampleSize (int: number of records evaluated)
     - MeasurementPeriod (string, 50 chars: "Daily", "Weekly", "Monthly")
     - PeriodStart (DateTime, UTC)
     - PeriodEnd (DateTime, UTC)
     - Target (decimal: target threshold, e.g., 98.0 for AIHumanAgreement)
     - Status (string, 20 chars: "BelowTarget", "MeetsTarget", "ExceedsTarget")
     - Notes (string, 2000 chars, nullable)
     - CreatedAt (DateTime, UTC)
   - Indexes:
     - MetricType (B-tree)
     - PeriodStart, PeriodEnd (B-tree for time-series queries)

3. **Create DTOs**
   - File: `src/backend/PatientAccess.Business/DTOs/CodeMappingRequestDto.cs`
     ```csharp
     public class CodeMappingRequestDto
     {
         [Required]
         public Guid ExtractedClinicalDataId { get; set; }
         
         [Required]
         [StringLength(5000, MinimumLength = 5)]
         public string ClinicalText { get; set; } // Extracted clinical text
         
         [Required]
         public string CodeSystem { get; set; } // "ICD10" or "CPT"
         
         public int MaxSuggestions { get; set; } = 5; // Default top-5
     }
     ```
   - File: `src/backend/PatientAccess.Business/DTOs/MedicalCodeSuggestionDto.cs`
     ```csharp
     public class MedicalCodeSuggestionDto
     {
         public string Code { get; set; } // "E11.9"
         public string Description { get; set; } // "Type 2 diabetes mellitus without complications"
         public decimal ConfidenceScore { get; set; } // 0-100
         public string Rationale { get; set; } // LLM explanation
         public int Rank { get; set; } // 1 = top suggestion
         public bool IsTopSuggestion { get; set; }
     }
     ```
   - File: `src/backend/PatientAccess.Business/DTOs/CodeMappingResponseDto.cs`
     ```csharp
     public class CodeMappingResponseDto
     {
         public Guid ExtractedClinicalDataId { get; set; }
         public string CodeSystem { get; set; }
         public List<MedicalCodeSuggestionDto> Suggestions { get; set; }
         public int SuggestionCount { get; set; }
         public string Message { get; set; } // "No matching code found" for edge case
         public bool IsAmbiguous { get; set; } // True if multiple codes with similar confidence
     }
     ```

4. **Create Prompt Templates**
   - File: `.propel/prompts/ai/code-mapping-icd10.txt`
     ```
     You are a medical coding expert specializing in ICD-10 diagnosis code assignment. Your task is to map clinical text to the most appropriate ICD-10 code(s) based on the provided context from the official ICD-10-CM coding manual.
     
     **Instructions:**
     1. Read the clinical text carefully.
     2. Review the retrieved ICD-10 code context below.
     3. Select the most appropriate ICD-10 code(s) that accurately represent the clinical condition.
     4. Provide a confidence score (0-100%) for each code suggestion.
     5. Explain your rationale for each code selection.
     6. If multiple codes are plausible, list them in descending order of confidence.
     7. If no code matches, respond with "No matching code found" and recommend manual coding.
     
     **Retrieved ICD-10 Context:**
     {retrieved_context}
     
     **Clinical Text:**
     {clinical_text}
     
     **Required Output Format (JSON):**
     {
       "suggestions": [
         {
           "code": "E11.9",
           "description": "Type 2 diabetes mellitus without complications",
           "confidence_score": 95,
           "rationale": "Clinical text explicitly mentions 'Type 2 Diabetes Mellitus' without mention of complications."
         }
       ]
     }
     
     **Guidelines:**
     - Use only the retrieved context for code selection (do not invent codes).
     - Confidence scores must reflect certainty based on clinical text specificity.
     - If clinical text is ambiguous, provide multiple codes with lower confidence.
     - Ensure JSON output is valid and parseable.
     ```
   - File: `.propel/prompts/ai/code-mapping-cpt.txt` (similar structure for CPT codes)

5. **Create CodeMappingResponseValidator**
   - File: `src/backend/PatientAccess.Business/Validators/CodeMappingResponseValidator.cs`
   - Validation rules per AIR-Q03 (>99% schema validity):
     ```csharp
     public class CodeMappingResponseValidator : AbstractValidator<CodeMappingResponseDto>
     {
         public CodeMappingResponseValidator()
         {
             RuleFor(r => r.ExtractedClinicalDataId).NotEmpty();
             RuleFor(r => r.CodeSystem).Must(cs => cs == "ICD10" || cs == "CPT")
                 .WithMessage("CodeSystem must be 'ICD10' or 'CPT'");
             
             RuleFor(r => r.Suggestions)
                 .NotNull()
                 .When(r => string.IsNullOrEmpty(r.Message)); // Only validate if not "No matching code found"
             
             RuleForEach(r => r.Suggestions).ChildRules(suggestion =>
             {
                 suggestion.RuleFor(s => s.Code).NotEmpty().MaximumLength(20);
                 suggestion.RuleFor(s => s.Description).NotEmpty().MaximumLength(1000);
                 suggestion.RuleFor(s => s.ConfidenceScore)
                     .InclusiveBetween(0, 100)
                     .WithMessage("Confidence score must be between 0 and 100");
                 suggestion.RuleFor(s => s.Rationale).NotEmpty().MaximumLength(2000);
                 suggestion.RuleFor(s => s.Rank).GreaterThan(0);
             });
         }
     }
     ```

6. **Implement ICodeMappingService Interface**
   - File: `src/backend/PatientAccess.Business/Interfaces/ICodeMappingService.cs`
   - Methods:
     ```csharp
     Task<CodeMappingResponseDto> MapToICD10Async(CodeMappingRequestDto request, CancellationToken cancellationToken);
     Task<CodeMappingResponseDto> MapToCPTAsync(CodeMappingRequestDto request, CancellationToken cancellationToken);
     Task<bool> ValidateOutputSchemaAsync(CodeMappingResponseDto response);
     Task<decimal> CalculateAgreementRateAsync(DateTime periodStart, DateTime periodEnd);
     ```

7. **Implement CodeMappingService**
   - File: `src/backend/PatientAccess.Business/Services/CodeMappingService.cs`
   - Constructor dependencies:
     - ILogger<CodeMappingService>
     - ApplicationDbContext
     - IHybridRetrievalService (from US_050 task_004)
     - OpenAIClient (Azure OpenAI SDK)
     - IOptions<CodeMappingSettings>
     - IValidator<CodeMappingResponseDto>
   - Implement MapToICD10Async:
     ```csharp
     public async Task<CodeMappingResponseDto> MapToICD10Async(
         CodeMappingRequestDto request, CancellationToken cancellationToken)
     {
         _logger.LogInformation("Mapping clinical text to ICD-10 codes for ExtractedDataId: {Id}", 
             request.ExtractedClinicalDataId);
         
         // 1. Retrieve relevant ICD-10 codes from knowledge base (RAG)
         var retrievalRequest = new CodeSearchRequestDto
         {
             Query = request.ClinicalText,
             CodeSystem = "ICD10",
             TopK = 5,
             MinSimilarityThreshold = 0.75
         };
         var retrievedCodes = await _hybridRetrievalService.SearchAsync(retrievalRequest, cancellationToken);
         
         // Build retrieved context for prompt
         var retrievedContext = string.Join("\n\n", retrievedCodes.Results.Select(r =>
             $"Code: {r.Code}\nDescription: {r.Description}\nCategory: {r.Category}\nSimilarity: {r.SimilarityScore:F2}"));
         
         // 2. Load prompt template
         var promptTemplate = await File.ReadAllTextAsync(".propel/prompts/ai/code-mapping-icd10.txt", cancellationToken);
         var prompt = promptTemplate
             .Replace("{retrieved_context}", retrievedContext)
             .Replace("{clinical_text}", request.ClinicalText);
         
         // 3. Invoke Azure OpenAI GPT-4o
         var chatMessages = new[]
         {
             new ChatMessage(ChatRole.System, "You are a medical coding expert specializing in ICD-10 diagnosis codes."),
             new ChatMessage(ChatRole.User, prompt)
         };
         
         var chatOptions = new ChatCompletionsOptions
         {
             DeploymentName = _settings.Value.Gpt4oDeploymentName,
             Messages = chatMessages,
             Temperature = 0.0f, // Deterministic for medical coding
             MaxTokens = 1000,
             ResponseFormat = ChatCompletionsResponseFormat.JsonObject // Force JSON output
         };
         
         var response = await _retryPolicy.ExecuteAsync(() =>
             _openAIClient.GetChatCompletionsAsync(chatOptions, cancellationToken));
         
         var llmOutput = response.Value.Choices[0].Message.Content;
         
         // 4. Parse JSON response
         var parsedResponse = JsonSerializer.Deserialize<LLMCodeMappingResponse>(llmOutput);
         
         // 5. Build response DTO
         var responseDto = new CodeMappingResponseDto
         {
             ExtractedClinicalDataId = request.ExtractedClinicalDataId,
             CodeSystem = "ICD10",
             Suggestions = parsedResponse.Suggestions.Select((s, index) => new MedicalCodeSuggestionDto
             {
                 Code = s.Code,
                 Description = s.Description,
                 ConfidenceScore = s.ConfidenceScore,
                 Rationale = s.Rationale,
                 Rank = index + 1,
                 IsTopSuggestion = index == 0
             }).ToList(),
             SuggestionCount = parsedResponse.Suggestions.Count,
             IsAmbiguous = parsedResponse.Suggestions.Count > 1 && 
                          Math.Abs(parsedResponse.Suggestions[0].ConfidenceScore - 
                                   parsedResponse.Suggestions[1].ConfidenceScore) < 10
         };
         
         // 6. Validate output schema (AIR-Q03: >99% validity)
         var isValid = await ValidateOutputSchemaAsync(responseDto);
         if (!isValid)
         {
             _logger.LogWarning("Invalid schema for ExtractedDataId: {Id}", request.ExtractedClinicalDataId);
             // Track schema validity failure
             await TrackSchemaValidityAsync(false, cancellationToken);
         }
         else
         {
             await TrackSchemaValidityAsync(true, cancellationToken);
         }
         
         // 7. Persist to database
         foreach (var suggestion in responseDto.Suggestions)
         {
             var medicalCode = new MedicalCode
             {
                 Id = Guid.NewGuid(),
                 ExtractedClinicalDataId = request.ExtractedClinicalDataId,
                 CodeSystem = "ICD10",
                 Code = suggestion.Code,
                 Description = suggestion.Description,
                 ConfidenceScore = suggestion.ConfidenceScore,
                 Rationale = suggestion.Rationale,
                 Rank = suggestion.Rank,
                 IsTopSuggestion = suggestion.IsTopSuggestion,
                 RetrievedContext = retrievedContext,
                 VerificationStatus = "Pending",
                 CreatedAt = DateTime.UtcNow
             };
             
             await _context.MedicalCodes.AddAsync(medicalCode, cancellationToken);
         }
         
         await _context.SaveChangesAsync(cancellationToken);
         
         _logger.LogInformation("Mapped {Count} ICD-10 codes for ExtractedDataId: {Id}", 
             responseDto.SuggestionCount, request.ExtractedClinicalDataId);
         
         return responseDto;
     }
     ```
   - Implement ValidateOutputSchemaAsync:
     ```csharp
     public async Task<bool> ValidateOutputSchemaAsync(CodeMappingResponseDto response)
     {
         var validationResult = await _validator.ValidateAsync(response);
         return validationResult.IsValid;
     }
     ```
   - Implement CalculateAgreementRateAsync (AIR-Q01: >98% agreement):
     ```csharp
     public async Task<decimal> CalculateAgreementRateAsync(DateTime periodStart, DateTime periodEnd)
     {
         var verifiedCodes = await _context.MedicalCodes
             .Where(mc => mc.VerifiedAt >= periodStart && mc.VerifiedAt <= periodEnd &&
                         (mc.VerificationStatus == "StaffVerified" || mc.VerificationStatus == "StaffRejected"))
             .ToListAsync();
         
         if (verifiedCodes.Count == 0) return 0;
         
         var agreementCount = verifiedCodes.Count(mc => mc.VerificationStatus == "StaffVerified" && mc.IsTopSuggestion);
         var agreementRate = (decimal)agreementCount / verifiedCodes.Count * 100;
         
         // Track metric
         var metric = new QualityMetric
         {
             Id = Guid.NewGuid(),
             MetricType = "AIHumanAgreement",
             MetricValue = agreementRate,
             SampleSize = verifiedCodes.Count,
             MeasurementPeriod = "Custom",
             PeriodStart = periodStart,
             PeriodEnd = periodEnd,
             Target = 98.0m,
             Status = agreementRate >= 98.0m ? "MeetsTarget" : "BelowTarget",
             CreatedAt = DateTime.UtcNow
         };
         
         await _context.QualityMetrics.AddAsync(metric);
         await _context.SaveChangesAsync();
         
         return agreementRate;
     }
     ```

8. **Add Polly Retry and Circuit Breaker**
   - Wrap Azure OpenAI calls in Polly policy (similar to US_050 task_003)
   - 3 retries with exponential backoff (2s, 4s, 8s)
   - Circuit breaker: 5 failures, 1-minute break

9. **Create CodeMappingSettings Configuration**
   - File: `src/backend/PatientAccess.Business/Configuration/CodeMappingSettings.cs`
     ```csharp
     public class CodeMappingSettings
     {
         public string Gpt4oDeploymentName { get; set; } // "gpt-4o"
         public int MaxSuggestions { get; set; } = 5;
         public decimal AmbiguityThreshold { get; set; } = 10.0m; // Confidence difference <10% = ambiguous
     }
     ```
   - Add to `appsettings.json`:
     ```json
     "CodeMapping": {
       "Gpt4oDeploymentName": "gpt-4o",
       "MaxSuggestions": 5,
       "AmbiguityThreshold": 10.0
     }
     ```

10. **Register Services in Program.cs**
    ```csharp
    builder.Services.Configure<CodeMappingSettings>(
        builder.Configuration.GetSection("CodeMapping"));
    
    builder.Services.AddScoped<IValidator<CodeMappingResponseDto>, CodeMappingResponseValidator>();
    builder.Services.AddScoped<ICodeMappingService, CodeMappingService>();
    ```

## Current Project State

```
src/backend/
├── PatientAccess.Business/
│   ├── Services/
│   │   ├── HybridRetrievalService.cs (from US_050 task_004)
│   │   └── EmbeddingGenerationService.cs (from US_050 task_003)
│   └── Interfaces/
│       ├── IHybridRetrievalService.cs (from US_050 task_004)
│       └── IEmbeddingGenerationService.cs (from US_050 task_003)
├── PatientAccess.Data/
│   ├── Entities/
│   │   ├── ExtractedClinicalData.cs (from EP-006-II)
│   │   ├── ICD10Code.cs (from US_050 task_001)
│   │   └── CPTCode.cs (from US_050 task_001)
│   └── ApplicationDbContext.cs
└── PatientAccess.Web/
    └── Program.cs
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/Services/CodeMappingService.cs | Core code mapping logic |
| CREATE | src/backend/PatientAccess.Business/Interfaces/ICodeMappingService.cs | Service interface |
| CREATE | src/backend/PatientAccess.Business/DTOs/CodeMappingRequestDto.cs | Request DTO |
| CREATE | src/backend/PatientAccess.Business/DTOs/CodeMappingResponseDto.cs | Response DTO |
| CREATE | src/backend/PatientAccess.Business/DTOs/MedicalCodeSuggestionDto.cs | Individual code DTO |
| CREATE | src/backend/PatientAccess.Business/Validators/CodeMappingResponseValidator.cs | FluentValidation validator |
| CREATE | src/backend/PatientAccess.Business/Configuration/CodeMappingSettings.cs | Configuration POCO |
| CREATE | src/backend/PatientAccess.Data/Entities/MedicalCode.cs | Medical code entity |
| CREATE | src/backend/PatientAccess.Data/Entities/QualityMetric.cs | Quality tracking entity |
| CREATE | src/backend/PatientAccess.Data/Migrations/YYYYMMDDHHMMSS_AddMedicalCodeAndQualityMetrics.cs | EF migration |
| CREATE | .propel/prompts/ai/code-mapping-icd10.txt | ICD-10 prompt template |
| CREATE | .propel/prompts/ai/code-mapping-cpt.txt | CPT prompt template |
| MODIFY | src/backend/PatientAccess.Data/ApplicationDbContext.cs | Add MedicalCode, QualityMetric DbSets |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register ICodeMappingService |
| MODIFY | src/backend/PatientAccess.Web/appsettings.json | Add CodeMapping config |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### Azure OpenAI Chat Completions
- **GPT-4o Documentation**: https://learn.microsoft.com/en-us/azure/ai-services/openai/concepts/models#gpt-4o-and-gpt-4-turbo
- **Chat Completions API**: https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/chatgpt
- **JSON Mode**: https://platform.openai.com/docs/guides/structured-outputs

### RAG Pattern
- **Retrieval-Augmented Generation**: https://arxiv.org/abs/2005.11401
- **Grounding LLM Responses**: https://learn.microsoft.com/en-us/azure/ai-services/openai/concepts/grounding

### FluentValidation Documentation
- **FluentValidation GitHub**: https://github.com/FluentValidation/FluentValidation
- **ASP.NET Core Integration**: https://docs.fluentvalidation.net/en/latest/aspnet.html

### Medical Coding Standards
- **ICD-10-CM Official Guidelines**: https://www.cdc.gov/nchs/icd/icd-10-cm.htm
- **CPT Coding**: https://www.ama-assn.org/practice-management/cpt

### Design Requirements
- **FR-034**: System MUST suggest ICD-10 diagnosis codes with confidence scores (spec.md)
- **FR-035**: System MUST suggest CPT procedure codes with confidence scores (spec.md)
- **AIR-003**: System MUST map extracted clinical data to appropriate ICD-10 diagnosis codes using RAG pattern (design.md)
- **AIR-004**: System MUST map to CPT procedure codes using RAG pattern (design.md)
- **AIR-Q01**: System MUST maintain AI-Human Agreement Rate above 98% (design.md)
- **AIR-Q03**: System MUST achieve output schema validity rate above 99% (design.md)
- **TR-015**: System MUST use Azure OpenAI Service with HIPAA BAA (design.md)

### Existing Codebase Patterns
- **RAG Retrieval**: `src/backend/PatientAccess.Business/Services/HybridRetrievalService.cs` (from US_050)
- **Azure OpenAI Integration**: `src/backend/PatientAccess.Business/Services/EmbeddingGenerationService.cs` (from US_050)

## Build Commands
```powershell
# Create migration
cd src/backend/PatientAccess.Data
dotnet ef migrations add AddMedicalCodeAndQualityMetrics --startup-project ../PatientAccess.Web

# Update database
dotnet ef database update --startup-project ../PatientAccess.Web

# Create prompt directories
New-Item -ItemType Directory -Path ".propel/prompts/ai" -Force

# Build solution
cd src/backend
dotnet build

# Run tests
cd PatientAccess.Tests
dotnet test
```

## Validation Strategy

### Unit Tests
- File: `src/backend/PatientAccess.Tests/Services/CodeMappingServiceTests.cs`
- Test cases:
  1. **Test_MapToICD10Async_ReturnsCodeWithConfidence**
     - Input: Clinical text "Type 2 Diabetes Mellitus"
     - Expected: ICD-10 code "E11.9" with confidence >80%
     - Assert: response.Suggestions[0].Code == "E11.9", ConfidenceScore > 80
  2. **Test_MapToICD10Async_ReturnsMultipleCodesForAmbiguousText**
     - Input: Ambiguous text mapping to multiple codes
     - Expected: Multiple suggestions ranked by confidence, IsAmbiguous = true
     - Assert: response.SuggestionCount > 1, response.IsAmbiguous == true
  3. **Test_MapToICD10Async_ReturnsNoMatchingCodeMessage**
     - Input: Clinical text with no mappable ICD-10 code
     - Expected: Message = "No matching code found"
     - Assert: response.Message == "No matching code found", response.SuggestionCount == 0
  4. **Test_ValidateOutputSchemaAsync_ReturnsTrue_ValidResponse**
     - Input: Valid CodeMappingResponseDto
     - Expected: Validation passes
     - Assert: isValid == true
  5. **Test_ValidateOutputSchemaAsync_ReturnsFalse_InvalidConfidenceScore**
     - Input: CodeMappingResponseDto with ConfidenceScore = 150 (>100)
     - Expected: Validation fails
     - Assert: isValid == false
  6. **Test_CalculateAgreementRateAsync_Returns98Percent**
     - Setup: Insert 100 MedicalCode records, 98 verified, 2 rejected
     - Execute: CalculateAgreementRateAsync(periodStart, periodEnd)
     - Assert: agreementRate == 98.0m

### Integration Tests
- File: `src/backend/PatientAccess.Tests/Services/CodeMappingServiceIntegrationTests.cs`
- Test cases:
  1. **Test_MapToICD10Async_RAGIntegration_RetrievesRelevantContext**
     - Setup: Populate ICD10Codes table with embeddings
     - Execute: MapToICD10Async("Type 2 Diabetes Mellitus")
     - Assert: Retrieved context contains E11.x codes
  2. **Test_MapToICD10Async_AzureOpenAI_ReturnsValidJSON**
     - Execute: MapToICD10Async with real Azure OpenAI call
     - Assert: Response is valid JSON, parseable to CodeMappingResponseDto
  3. **Test_SchemaValidity_Exceeds99Percent**
     - Execute: 100 code mapping requests
     - Track: Schema validity rate
     - Assert: validityRate > 99.0m

### Acceptance Criteria Validation
- **AC1**: ✅ ICD-10 codes suggested with confidence scores (0-100%)
- **AC2**: ✅ CPT codes suggested with confidence scores (0-100%)
- **AC3**: ✅ Azure OpenAI GPT-4o with HIPAA BAA, grounded in RAG context
- **AC4**: ✅ AI-Human Agreement Rate >98%, output schema validity >99%
- **Edge Case 1**: ✅ Ambiguous text returns multiple ranked suggestions
- **Edge Case 2**: ✅ Unmappable data returns "No matching code found"

## Success Criteria Checklist
- [X] [MANDATORY] MedicalCode entity created with CodeSystem, Code, ConfidenceScore, Rationale, Rank
- [X] [MANDATORY] QualityMetric entity created for AIHumanAgreement, SchemaValidity tracking
- [X] [MANDATORY] CodeMappingService implements ICodeMappingService interface
- [X] [MANDATORY] MapToICD10Async integrates HybridRetrievalService for RAG retrieval
- [X] [MANDATORY] MapToICD10Async invokes Azure OpenAI GPT-4o with HIPAA BAA (TR-015)
- [X] [MANDATORY] Prompt templates created (.propel/prompts/ai/code-mapping-icd10.txt, code-mapping-cpt.txt)
- [X] [MANDATORY] CodeMappingResponseValidator validates confidence scores (0-100 range)
- [X] [MANDATORY] MapToICD10Async returns multiple ranked suggestions for ambiguous text
- [X] [MANDATORY] MapToICD10Async returns "No matching code found" for unmappable data
- [X] [MANDATORY] CalculateAgreementRateAsync tracks AI-Human Agreement Rate (target >98%)
- [X] [MANDATORY] ValidateOutputSchemaAsync tracks schema validity (target >99%)
- [X] [MANDATORY] Polly retry policy: 3 retries with exponential backoff
- [MANDATORY] Unit test: Valid response passes schema validation
- [MANDATORY] Unit test: Invalid confidence score (>100) fails validation
- [MANDATORY] Integration test: RAG retrieval provides relevant context
- [MANDATORY] Integration test: Azure OpenAI returns valid JSON response
- [ ] [RECOMMENDED] Application Insights telemetry: "CodeMappingCompleted", "SchemaValidationFailed"
- [X] [RECOMMENDED] Temperature = 0.0 for deterministic medical coding

## Estimated Effort
**8 hours** (Service implementation + RAG integration + Azure OpenAI + FluentValidation + quality metrics + unit tests)
