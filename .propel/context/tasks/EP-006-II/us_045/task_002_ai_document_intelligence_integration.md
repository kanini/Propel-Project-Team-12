# Task - task_002_ai_document_intelligence_integration

## Requirement Reference
- User Story: US_045
- Story Location: .propel/context/tasks/EP-006-II/us_045/us_045.md
- Acceptance Criteria:
    - **AC1**: Given a PDF document is queued for processing, When the extraction job runs, Then Azure AI Document Intelligence processes the document and extracts vitals, medical history, medications, allergies, lab results, and diagnoses as structured data points.
    - **AC2**: Given data is extracted, When each data point is stored, Then it includes a confidence score (0-100%) and a source document reference (page number, text excerpt) enabling traceability to the original document.
    - **AC3**: Given the extraction is running, When processing a single page, Then AI inference completes within 5 seconds at the 95th percentile (AIR-Q02, NFR-013).
    - **AC4**: Given the AI extraction targets quality, When evaluated against a test set, Then hallucination rate is below 2% (AIR-Q04) and extraction recall is above 95% for critical elements like medications and allergies (AIR-Q05).
    - **AC5**: Given multiple data types exist in a document, When extraction completes, Then each element is classified by type (Vital, Medication, Allergy, Diagnosis, LabResult) with appropriate structured fields.
- Edge Case:
    - What happens when a PDF page is a scanned image with poor quality? AI returns lower confidence scores; data points below 50% confidence are flagged for manual review.
    - How does the system handle multi-language content in a document? English content is extracted; non-English sections are skipped with a note in the extraction results.

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
| AI/ML | Azure AI Document Intelligence | 4.0 |
| AI/ML | Azure.AI.FormRecognizer | 4.1.x |
| Vector Store | N/A | N/A |
| AI Gateway | N/A | N/A |
| Mobile | N/A | N/A |

**Note**: All code, and libraries, MUST be compatible with versions above.

## AI References (AI Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **AI Impact** | Yes |
| **AIR Requirements** | AIR-002, AIR-008, AIR-009, AIR-Q02, AIR-Q04, AIR-Q05 |
| **AI Pattern** | Document Intelligence Pattern |
| **Prompt Template Path** | N/A (uses Azure AI Document Intelligence pre-trained models) |
| **Guardrails Config** | Confidence threshold: 50% (flag for manual review below), Language filter: English only |
| **Model Provider** | Azure AI Document Intelligence |

> **AI Impact Legend:**
> - **Yes**: Task involves LLM integration, RAG pipeline, prompt engineering, or AI infrastructure
> - **No**: Task is deterministic (FE/BE/DB only)
>
> If AI Impact = No, all AI references should be N/A

### **CRITICAL: AI Implementation Requirement (AI Tasks Only)**
**IF AI Impact = Yes:**
- **MUST** reference prompt templates from Prompt Template Path during implementation
- **MUST** implement guardrails for input sanitization and output validation
- **MUST** enforce token budget limits per AIR-O01 requirements
- **MUST** implement fallback logic for low-confidence responses
- **MUST** log all prompts/responses for audit (redact PII)
- **MUST** handle model failures gracefully (timeout, rate limit, 5xx errors)

## Mobile References (Mobile Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **Mobile Impact** | No |
| **Platform Target** | N/A |
| **Min OS Version** | N/A |
| **Mobile Framework** | N/A |

> If Mobile Impact = No, all Mobile references should be N/A

## Task Overview

Integrate Azure AI Document Intelligence to extract structured clinical data from uploaded PDF documents. This task implements the core AI extraction pipeline that processes clinical documents page-by-page, identifies and extracts vitals, medications, allergies, diagnoses, and lab results, assigns confidence scores to each data point, maintains source document traceability (page number, text excerpt), and stores results in the ExtractedClinicalData table. The implementation ensures 5-second per-page performance (AIR-Q02), hallucination rate <2% (AIR-Q04), and extraction recall >95% for critical elements (AIR-Q05).

**Key Capabilities:**
- Azure AI Document Intelligence SDK integration
- Pre-trained model for medical document analysis
- Page-by-page document processing with performance monitoring
- Clinical data type classification (Vital, Medication, Allergy, Diagnosis, LabResult)
- Confidence score calculation and low-confidence flagging (<50%)
- Source document reference extraction (page number, text excerpt)
- Structured data storage with traceability
- Error handling for poor quality scans and non-English content
- Audit logging for all AI operations (AIR-S02)

## Dependent Tasks
- task_001_db_extracted_data_schema (ExtractedClinicalData table must exist)
- EP-006-I: US_043: task_001_be_hangfire_processing_pipeline (DocumentProcessingService must exist)

## Impacted Components
- **NEW**: `src/backend/PatientAccess.Business/Services/AzureDocumentIntelligenceService.cs` - Azure AI service wrapper
- **NEW**: `src/backend/PatientAccess.Business/Services/IClinicalDataExtractionService.cs` - Extraction service interface
- **NEW**: `src/backend/PatientAccess.Business/Services/ClinicalDataExtractionService.cs` - Extraction orchestration
- **NEW**: `src/backend/PatientAccess.Business/DTOs/ExtractedDataPointDto.cs` - Extracted data DTO
- **NEW**: `src/backend/PatientAccess.Business/DTOs/ExtractionResultDto.cs` - Extraction result summary DTO
- **MODIFY**: `src/backend/PatientAccess.Business/Services/DocumentProcessingService.cs` - Call extraction service
- **MODIFY**: `src/backend/PatientAccess.Web/Program.cs` - Register Azure AI service, configure credentials
- **MODIFY**: `src/backend/PatientAccess.Web/appsettings.json` - Add Azure AI Document Intelligence configuration

## Implementation Plan

1. **Configure Azure AI Document Intelligence Credentials**
   - Add configuration to `appsettings.json`:
     - AzureAIDocumentIntelligence:Endpoint (Azure resource endpoint)
     - AzureAIDocumentIntelligence:ApiKey (subscription key)
     - AzureAIDocumentIntelligence:Model (prebuilt-healthInsuranceCard.us or custom model ID)
   - Use Azure Key Vault for API key storage (production)
   - Register configuration in `Program.cs`

2. **Create AzureDocumentIntelligenceService**
   - Add NuGet package: `Azure.AI.FormRecognizer` version 4.1.x
   - Inject `IConfiguration`, `ILogger`
   - Implement `AnalyzeDocumentAsync(string filePath)` method
   - Create `DocumentAnalysisClient` with endpoint and API key
   - Use prebuilt-healthInsuranceCard model or custom trained model
   - Process document page-by-page with performance monitoring
   - Return raw `AnalyzeResult` with extracted fields and confidence scores
   - Implement retry logic with exponential backoff (3 attempts)
   - Handle errors: timeout (30s), rate limiting (429), service unavailable (503)

3. **Create ClinicalDataExtractionService**
   - Inject `AzureDocumentIntelligenceService`, `ApplicationDbContext`, `ILogger`
   - Implement `ExtractClinicalDataAsync(Guid documentId)` method:
     - Load ClinicalDocument entity from database
     - Call `AzureDocumentIntelligenceService.AnalyzeDocumentAsync(filePath)`
     - Parse `AnalyzeResult` to identify clinical data types
     - Classify extracted fields by type (Vital, Medication, Allergy, Diagnosis, LabResult)
     - Map each field to `ExtractedClinicalData` entity with source references
     - Calculate confidence score from Azure AI confidence values
     - Flag data points with confidence <50% for manual review
     - Save extracted data to database in batch
     - Return `ExtractionResultDto` with summary (total extracted, flagged for review)

4. **Implement Data Type Classification Logic**
   - Use field name patterns to classify data types:
     - Vitals: blood pressure, heart rate, temperature, weight, height, BMI, O2 saturation
     - Medications: drug name, dosage, frequency, start date, prescriber
     - Allergies: allergen, reaction, severity, onset date
     - Diagnoses: ICD code, description, diagnosis date, provider
     - Lab Results: test name, value, unit, reference range, collection date
   - Store structured fields in StructuredData JSON column
   - Handle ambiguous classifications (default to manual review flag)

5. **Implement Source Reference Extraction**
   - Extract page number from Azure AI result
   - Extract text excerpt (surrounding context) from document
   - Store in SourcePageNumber and SourceTextExcerpt fields
   - Limit excerpt to 1000 characters (database constraint)
   - Ensure traceability: every data point links to source

6. **Enhance DocumentProcessingService**
   - Modify `ProcessDocumentAsync` to call `ClinicalDataExtractionService`
   - Measure processing time per page (log warning if >5 seconds)
   - Update document status based on extraction result
   - Handle extraction errors: log details, update status to "Failed", trigger Pusher event
   - Store extraction summary in ClinicalDocument.ProcessingNotes (optional JSON field)

7. **Implement Quality Guardrails**
   - Confidence threshold: flag data points <50% for manual review
   - Language detection: skip non-English content, log skipped sections
   - Schema validation: validate extracted data matches expected structure
   - Hallucination detection: cross-reference with medical terminology database (future enhancement)
   - Set RequiresManualReview = true if >20% data points flagged

8. **Add Comprehensive Audit Logging**
   - Log AI service invocation with document ID and model version (AIR-S02)
   - Log extraction results summary (total extracted, confidence distribution)
   - Log performance metrics (processing time per page)
   - Log flagged items for manual review
   - Use structured logging with correlation IDs
   - Redact PII from logs (patient name, SSN)

## Current Project State

```
src/backend/
├── PatientAccess.Business/
│   ├── Services/
│   │   ├── DocumentProcessingService.cs (from EP-006-I)
│   │   └── DocumentUploadService.cs (from EP-006-I)
│   └── DTOs/
│       └── DocumentUploadResponseDto.cs (from EP-006-I)
├── PatientAccess.Data/
│   └── Entities/
│       ├── ClinicalDocument.cs (from EP-006-I)
│       └── ExtractedClinicalData.cs (from task_001)
└── PatientAccess.Web/
    ├── Program.cs
    └── appsettings.json
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/Services/AzureDocumentIntelligenceService.cs | Azure AI SDK wrapper |
| CREATE | src/backend/PatientAccess.Business/Services/IClinicalDataExtractionService.cs | Extraction service interface |
| CREATE | src/backend/PatientAccess.Business/Services/ClinicalDataExtractionService.cs | Extraction orchestration and classification |
| CREATE | src/backend/PatientAccess.Business/DTOs/ExtractedDataPointDto.cs | Extracted data point DTO |
| CREATE | src/backend/PatientAccess.Business/DTOs/ExtractionResultDto.cs | Extraction summary DTO |
| MODIFY | src/backend/PatientAccess.Business/Services/DocumentProcessingService.cs | Call extraction service in ProcessDocumentAsync |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register Azure AI services |
| MODIFY | src/backend/PatientAccess.Web/appsettings.json | Add Azure AI configuration |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### Azure AI Document Intelligence Documentation
- **Quickstart**: https://learn.microsoft.com/en-us/azure/ai-services/document-intelligence/quickstarts/get-started-sdks-rest-api
- **Prebuilt Models**: https://learn.microsoft.com/en-us/azure/ai-services/document-intelligence/concept-model-overview
- **Health Insurance Card Model**: https://learn.microsoft.com/en-us/azure/ai-services/document-intelligence/concept-health-insurance-card
- **.NET SDK**: https://learn.microsoft.com/en-us/dotnet/api/overview/azure/ai.formrecognizer-readme
- **Performance Optimization**: https://learn.microsoft.com/en-us/azure/ai-services/document-intelligence/concept-best-practices

### Design Requirements
- **FR-028**: System MUST extract clinical data (vitals, medical history, medications, allergies, lab results, diagnoses) from uploaded unstructured PDF reports using document intelligence pattern (spec.md)
- **TR-016**: System MUST use Azure AI Document Intelligence for PDF clinical document extraction (design.md)
- **AIR-002**: System MUST extract clinical data from unstructured PDFs using document intelligence pattern (design.md)
- **AIR-008**: System MUST provide confidence scores (0-100%) for all AI-suggested clinical data (design.md)
- **AIR-009**: System MUST provide source document references (page number, text excerpt) for all AI-extracted data points (design.md)
- **AIR-Q02**: System MUST complete AI inference operations within 5 seconds per document page for 95th percentile requests (design.md)
- **AIR-Q04**: System MUST maintain hallucination rate below 2% on clinical document extraction evaluation set (design.md)
- **AIR-Q05**: System MUST achieve extraction recall above 95% for critical clinical data elements (medications, allergies) (design.md)
- **AIR-S02**: System MUST log all AI prompts and responses for audit purposes with configurable retention period (minimum 1 year) (design.md)

### Existing Codebase Patterns
- **Service Pattern**: `src/backend/PatientAccess.Business/Services/DocumentUploadService.cs`
- **Entity Pattern**: `src/backend/PatientAccess.Data/Entities/ClinicalDocument.cs`

## Build Commands
```powershell
# Add Azure AI NuGet package
cd src/backend
dotnet add PatientAccess.Business package Azure.AI.FormRecognizer

# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test

# Run application
cd PatientAccess.Web
dotnet run
```

## Implementation Validation Strategy
- [ ] Unit tests pass (data classification, confidence calculation, source reference extraction)
- [ ] Integration tests pass (Azure AI service call, database storage, end-to-end extraction)
- [ ] Azure AI Document Intelligence credentials configured correctly
- [ ] Document analysis completes within 5 seconds per page (95th percentile)
- [ ] Extracted data includes confidence scores (0-100%)
- [ ] Source references captured (page number, text excerpt)
- [ ] Data type classification accuracy >90% (manual validation on test set)
- [ ] Low-confidence data points (<50%) flagged for manual review
- [ ] Non-English content skipped with appropriate logging
- [ ] Error handling works (timeout, rate limit, service unavailable)
- [ ] Audit logging captures all AI operations without PII
- [ ] Performance monitoring logs processing time per page

## Implementation Checklist
- [ ] Add Azure.AI.FormRecognizer NuGet package and configure credentials in appsettings.json
- [ ] Create AzureDocumentIntelligenceService with AnalyzeDocumentAsync method
- [ ] Implement retry logic with exponential backoff for Azure AI service calls
- [ ] Create ClinicalDataExtractionService with data type classification logic
- [ ] Implement field mapping to ExtractedClinicalData entities with structured data
- [ ] Extract source references (page number, text excerpt) from Azure AI results
- [ ] Enhance DocumentProcessingService to call extraction and measure performance
- [ ] Implement confidence threshold guardrails (flag <50%, reject <20%)
- **[AI Tasks - MANDATORY]** Reference prompt templates from AI References table during implementation
- **[AI Tasks - MANDATORY]** Implement and test guardrails before marking task complete
- **[AI Tasks - MANDATORY]** Verify AIR-XXX requirements are met (quality, safety, operational)
