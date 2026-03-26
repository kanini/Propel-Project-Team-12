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
| AI/ML | Google Gemini API (Free Tier) | 1.5 |
| OCR | Tesseract OCR | 5.x |
| Storage | Supabase Storage | Latest |
| Library | Tesseract (NuGet) | 5.x |
| Library | Google.Cloud.AIPlatform.V1 | Latest |
| Vector Store | N/A | N/A |
| AI Gateway | N/A | N/A |
| Mobile | N/A | N/A |

**Note**: All code, and libraries, MUST be compatible with versions above.

## AI References (AI Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **AI Impact** | Yes |
| **AIR Requirements** | AIR-002, AIR-008, AIR-009, AIR-Q02, AIR-Q04, AIR-Q05 |
| **AI Pattern** | Document Intelligence Pattern (OCR + LLM Extraction) |
| **Prompt Template Path** | .propel/prompts/clinical-data-extraction-prompt.md |
| **Guardrails Config** | Confidence threshold: 50% (flag for manual review below), Language filter: English only, Max tokens: 8000 |
| **Model Provider** | Google Gemini (gemini-1.5-flash-latest - Free Tier) |

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

Integrate Google Gemini (free tier) with Tesseract OCR to extract structured clinical data from PDF documents stored in Supabase Storage. This task implements a two-stage AI extraction pipeline: (1) Tesseract OCR extracts raw text from PDF pages stored in Supabase, (2) Gemini LLM analyzes extracted text to identify and structure vitals, medications, allergies, diagnoses, and lab results. The system assigns confidence scores, maintains source document traceability, and stores results in ExtractedClinicalData table. The implementation ensures 5-second per-page performance (AIR-Q02), hallucination rate <2% (AIR-Q04), and extraction recall >95% for critical elements (AIR-Q05).

**Key Capabilities:**
- Supabase Storage integration for PDF file retrieval
- Tesseract OCR for text extraction from PDF pages
- Google Gemini API integration (free tier) for clinical data structuring
- Prompt engineering with medical domain templates
- Page-by-page document processing with performance monitoring
- Clinical data type classification (Vital, Medication, Allergy, Diagnosis, LabResult)
- Confidence score calculation and low-confidence flagging (<50%)
- Source document reference extraction (page number, text excerpt)
- Structured data storage with traceability
- Error handling for poor quality scans and non-English content
- Audit logging for all AI operations (AIR-S02)
- Token usage optimization (stay within free tier limits)

## Dependent Tasks
- task_001_db_extracted_data_schema (ExtractedClinicalData table must exist)
- EP-006-I: US_043: task_001_be_hangfire_processing_pipeline (DocumentProcessingService must exist)

## Impacted Components
- **NEW**: `src/backend/PatientAccess.Business/Services/SupabaseStorageService.cs` - Supabase Storage service for file retrieval
- **NEW**: `src/backend/PatientAccess.Business/Services/TesseractOcrService.cs` - Tesseract OCR service wrapper
- **NEW**: `src/backend/PatientAccess.Business/Services/GeminiAiService.cs` - Google Gemini API service wrapper
- **NEW**: `src/backend/PatientAccess.Business/Services/IClinicalDataExtractionService.cs` - Extraction service interface
- **NEW**: `src/backend/PatientAccess.Business/Services/ClinicalDataExtractionService.cs` - Extraction orchestration (OCR + LLM)
- **NEW**: `src/backend/PatientAccess.Business/DTOs/ExtractedDataPointDto.cs` - Extracted data DTO
- **NEW**: `src/backend/PatientAccess.Business/DTOs/ExtractionResultDto.cs` - Extraction result summary DTO
- **NEW**: `src/backend/PatientAccess.Business/DTOs/OcrResultDto.cs` - OCR text extraction result
- **NEW**: `.propel/prompts/clinical-data-extraction-prompt.md` - Gemini prompt template
- **MODIFY**: `src/backend/PatientAccess.Business/Services/DocumentProcessingService.cs` - Call extraction service
- **MODIFY**: `src/backend/PatientAccess.Web/Program.cs` - Register services, configure credentials
- **MODIFY**: `src/backend/PatientAccess.Web/appsettings.json` - Add Gemini API, Supabase, Tesseract configuration

## Implementation Plan

1. **Configure Services (Gemini, Supabase, Tesseract)**
   - Add configuration to `appsettings.json`:
     - GeminiAI:ApiKey (from Google AI Studio - free tier)
     - GeminiAI:Model (gemini-1.5-flash-latest)
     - GeminiAI:MaxTokens (8000)
     - Supabase:Url (Supabase project URL)
     - Supabase:ApiKey (Supabase anon/service key)
     - Supabase:BucketName (clinical-documents)
     - Tesseract:DataPath (tessdata folder path)
     - Tesseract:Language (eng)
   - Store API keys in environment variables or User Secrets (development)
   - Register configuration in `Program.cs`

2. **Create SupabaseStorageService**
   - Add NuGet package: `supabase-csharp` (Supabase .NET SDK)
   - Inject `IConfiguration`, `ILogger`
   - Implement `DownloadDocumentAsync(Guid documentId)` method:
     - Initialize Supabase client with URL and API key
     - Retrieve file from Supabase Storage bucket using document path
     - Download file to temp directory for processing
     - Return local file path for OCR processing
   - Implement `GetDocumentStreamAsync(Guid documentId)` for streaming
   - Implement retry logic with exponential backoff (3 attempts)
   - Handle errors: network timeout, file not found, storage permission errors

3. **Create TesseractOcrService**
   - Add NuGet package: `Tesseract` version 5.x
   - Inject `IConfiguration`, `ILogger`
   - Implement `ExtractTextFromPdfAsync(string pdfPath)` method:
     - Convert PDF pages to images using PdfiumViewer or similar library
     - Initialize Tesseract engine with English language data
     - Process each page image with OCR
     - Extract text with bounding box coordinates
     - Return `OcrResultDto` with page number, extracted text, confidence score
   - Measure OCR processing time per page (log warning if >3 seconds)
   - Handle low-quality images: log warning, set confidence score
   - Clean up temporary image files after processing

4. **Create GeminiAiService**
   - Add NuGet package: `Google.Cloud.AIPlatform.V1` or use REST API via HttpClient
   - Inject `IConfiguration`, `ILogger`, `IHttpClientFactory`
   - Implement `ExtractClinicalDataAsync(string ocrText, string promptTemplate)` method:
     - Load prompt template from `.propel/prompts/clinical-data-extraction-prompt.md`
     - Inject OCR text into prompt with medical data extraction instructions
     - Call Gemini API (gemini-1.5-flash-latest) with structured output request
     - Parse JSON response into clinical data points
     - Extract confidence scores from Gemini response metadata
     - Return structured `ExtractedDataPointDto` list
   - Implement token counting and rate limiting (stay within free tier: 15 RPM, 1M TPM)
   - Implement retry logic with exponential backoff (3 attempts)
   - Handle errors: rate limiting (429), model errors, malformed JSON responses
   - Cache parsed prompt templates for performance

5. **Create ClinicalDataExtractionService**
   - Inject `SupabaseStorageService`, `TesseractOcrService`, `GeminiAiService`, `ApplicationDbContext`, `ILogger`
   - Implement `ExtractClinicalDataAsync(Guid documentId)` method:
     - Load ClinicalDocument entity from database
     - Download PDF from Supabase Storage using `SupabaseStorageService`
     - Extract text from PDF using `TesseractOcrService` (page-by-page)
     - For each page, call `GeminiAiService.ExtractClinicalDataAsync(ocrText)`
     - Combine OCR confidence and Gemini confidence scores (weighted average: 30% OCR, 70% Gemini)
     - Classify extracted fields by type (Vital, Medication, Allergy, Diagnosis, LabResult)
     - Map each field to `ExtractedClinicalData` entity with source references
     - Flag data points with combined confidence <50% for manual review
     - Clean up temporary files after processing
     - Save extracted data to database in batch
     - Return `ExtractionResultDto` with summary (total extracted, flagged for review)
   - Measure total processing time (OCR + LLM) per page

6. **Implement Data Type Classification Logic**
   - Design Gemini prompt to return structured JSON with data type classification
   - Prompt should instruct Gemini to categorize each extracted field:
     - Vitals: blood pressure, heart rate, temperature, weight, height, BMI, O2 saturation
     - Medications: drug name, dosage, frequency, start date, prescriber
     - Allergies: allergen, reaction, severity, onset date
     - Diagnoses: ICD code, description, diagnosis date, provider
     - Lab Results: test name, value, unit, reference range, collection date
   - Parse Gemini JSON response and validate data types
   - Store structured fields in StructuredData JSON column
   - Handle ambiguous classifications (default to manual review flag)
   - Validate medical terminology against known medical ontologies (optional)

7. **Implement Source Reference Extraction**
   - Extract page number from OCR result metadata
   - Extract text excerpt (surrounding context) from OCR output
   - Store in SourcePageNumber and SourceTextExcerpt fields
   - Limit excerpt to 1000 characters (database constraint)
   - Ensure traceability: every data point links to source page and text location

8. **Enhance DocumentProcessingService**
   - Modify `ProcessDocumentAsync` to call `ClinicalDataExtractionService`
   - Measure OCR time and Gemini inference time separately
   - Log warning if combined processing time per page >5 seconds
   - Update document status based on extraction result
   - Handle extraction errors: log details, update status to "Failed", trigger Pusher event
   - Store extraction summary in ClinicalDocument.ProcessingNotes (optional JSON field)
   - Monitor Gemini API quota usage (log warnings approaching free tier limits)

9. **Implement Quality Guardrails**
   - Combined confidence threshold: OCR confidence (30%) + Gemini confidence (70%)
   - Flag data points with combined confidence <50% for manual review
   - Language detection: Tesseract language detection, skip non-English content
   - Schema validation: validate Gemini JSON output matches expected structure
   - Hallucination detection: validate against medical terminology patterns in prompt
   - Set RequiresManualReview = true if >20% data points flagged
   - Token budget enforcement: reject documents exceeding 8000 tokens per page
   - Rate limiting: implement exponential backoff if approaching Gemini free tier limits (15 RPM)

10. **Create Gemini Prompt Template**
   - Create `.propel/prompts/clinical-data-extraction-prompt.md`
   - Design prompt with:
     - System instructions for medical data extraction
     - Expected JSON output schema (data type, value, confidence, source)
     - Few-shot examples for each data type (Vital, Medication, Allergy, Diagnosis, LabResult)
     - Instructions to avoid hallucination (only extract explicit data)
     - Instructions to provide confidence scores (0-100%)
     - Handling ambiguous or unclear data
   - Include validation instructions (medical terminology, unit consistency)

11. **Add Comprehensive Audit Logging**
   - Log OCR service invocation with document ID and page count (AIR-S02)
   - Log Gemini API calls with prompt tokens and completion tokens
   - Log extraction results summary (total extracted, confidence distribution)
   - Log performance metrics (OCR time, Gemini inference time per page)
   - Log Gemini API quota usage (remaining requests, token usage)
   - Log flagged items for manual review
   - Use structured logging with correlation IDs
   - Redact PII from logs (patient name, SSN, medical record numbers)

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
| CREATE | src/backend/PatientAccess.Business/Services/SupabaseStorageService.cs | Supabase Storage service for file retrieval |
| CREATE | src/backend/PatientAccess.Business/Services/TesseractOcrService.cs | Tesseract OCR service wrapper |
| CREATE | src/backend/PatientAccess.Business/Services/GeminiAiService.cs | Google Gemini API service wrapper |
| CREATE | src/backend/PatientAccess.Business/Services/IClinicalDataExtractionService.cs | Extraction service interface |
| CREATE | src/backend/PatientAccess.Business/Services/ClinicalDataExtractionService.cs | Extraction orchestration (Supabase + OCR + Gemini) |
| CREATE | src/backend/PatientAccess.Business/DTOs/ExtractedDataPointDto.cs | Extracted data point DTO |
| CREATE | src/backend/PatientAccess.Business/DTOs/ExtractionResultDto.cs | Extraction summary DTO |
| CREATE | src/backend/PatientAccess.Business/DTOs/OcrResultDto.cs | OCR text extraction result |
| CREATE | .propel/prompts/clinical-data-extraction-prompt.md | Gemini prompt template |
| MODIFY | src/backend/PatientAccess.Business/Services/DocumentProcessingService.cs | Call extraction service in ProcessDocumentAsync |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register all services (Supabase, Tesseract, Gemini) |
| MODIFY | src/backend/PatientAccess.Web/appsettings.json | Add Gemini, Supabase, Tesseract configuration |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### Google Gemini Documentation
- **Gemini API Quickstart**: https://ai.google.dev/tutorials/dotnet_quickstart
- **Gemini Free Tier Limits**: https://ai.google.dev/pricing
- **Structured Output**: https://ai.google.dev/docs/structured_output
- **Prompt Engineering**: https://ai.google.dev/docs/prompting_intro
- **Safety Settings**: https://ai.google.dev/docs/safety_setting_gemini

### Tesseract OCR Documentation
- **Tesseract .NET Wrapper**: https://github.com/charlesw/tesseract
- **Tesseract Documentation**: https://github.com/tesseract-ocr/tesseract/wiki
- **Image Preprocessing**: https://tesseract-ocr.github.io/tessdoc/ImproveQuality.html

### Supabase Documentation
- **Supabase C# SDK**: https://github.com/supabase-community/supabase-csharp
- **Storage API**: https://supabase.com/docs/guides/storage
- **Storage Best Practices**: https://supabase.com/docs/guides/storage/uploads

### Design Requirements
- **FR-028**: System MUST extract clinical data (vitals, medical history, medications, allergies, lab results, diagnoses) from uploaded unstructured PDF reports using document intelligence pattern (spec.md)
- **TR-016**: System MUST use Tesseract OCR + Google Gemini for PDF clinical document extraction (design.md)
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
# Add required NuGet packages
cd src/backend
dotnet add PatientAccess.Business package Tesseract --version 5.2.0
dotnet add PatientAccess.Business package supabase-csharp
dotnet add PatientAccess.Business package Google.Cloud.AIPlatform.V1
dotnet add PatientAccess.Business package PdfiumViewer  # For PDF to image conversion

# Download Tesseract language data
# Download from https://github.com/tesseract-ocr/tessdata
# Place eng.traineddata in src/backend/PatientAccess.Web/tessdata/

# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test

# Set environment variables (development)
$env:GeminiAI__ApiKey="your-gemini-api-key"
$env:Supabase__Url="your-supabase-url"
$env:Supabase__ApiKey="your-supabase-key"

# Run application
cd PatientAccess.Web
dotnet run
```

## Implementation Validation Strategy
- [ ] Unit tests pass (data classification, confidence calculation, source reference extraction)
- [ ] Integration tests pass (Supabase download, Tesseract OCR, Gemini API call, database storage)
- [ ] Gemini API credentials configured correctly (free tier)
- [ ] Supabase Storage credentials configured correctly
- [ ] Tesseract language data installed in correct directory
- [ ] Document analysis completes within 5 seconds per page (95th percentile)
- [ ] OCR extracts text with >90% accuracy on clear documents
- [ ] Gemini structured output returns valid JSON with expected schema
- [ ] Combined confidence scores (OCR + Gemini) calculated correctly
- [ ] Extracted data includes confidence scores (0-100%)
- [ ] Source references captured (page number, text excerpt)
- [ ] Data type classification accuracy >90% (manual validation on test set)
- [ ] Low-confidence data points (<50%) flagged for manual review
- [ ] Non-English content skipped with appropriate logging
- [ ] Error handling works (timeout, rate limit, service unavailable)
- [ ] Gemini free tier limits enforced (15 RPM, 1M TPM)
- [ ] Temporary files cleaned up after processing
- [ ] Audit logging captures all AI operations without PII
- [ ] Performance monitoring logs OCR time and Gemini inference time separately
- [ ] Prompt template loads correctly from .propel/prompts/ directory

## Implementation Checklist
- [ ] Add Tesseract, supabase-csharp, Google.Cloud.AIPlatform.V1, PdfiumViewer NuGet packages
- [ ] Download and install Tesseract language data (eng.traineddata) in tessdata/ directory
- [ ] Configure credentials in appsettings.json (Gemini API key, Supabase URL/key, Tesseract data path)
- [ ] Create SupabaseStorageService with DownloadDocumentAsync method
- [ ] Create TesseractOcrService with ExtractTextFromPdfAsync method (PDF to image to text)
- [ ] Create GeminiAiService with ExtractClinicalDataAsync method (structured JSON output)
- [ ] Design and create Gemini prompt template in .propel/prompts/clinical-data-extraction-prompt.md
- [ ] Implement retry logic with exponential backoff for all external services
- [ ] Create ClinicalDataExtractionService orchestrating Supabase + Tesseract + Gemini
- [ ] Implement combined confidence score calculation (30% OCR + 70% Gemini)
- [ ] Implement field mapping to ExtractedClinicalData entities with structured data
- [ ] Extract source references (page number, text excerpt) from OCR results
- [ ] Enhance DocumentProcessingService to call extraction and measure performance per stage
- [ ] Implement quality guardrails (confidence threshold, token limits, rate limiting)
- [ ] Implement temporary file cleanup after processing
- [ ] Add comprehensive audit logging (OCR, Gemini API calls, quota usage)
- **[AI Tasks - MANDATORY]** Reference prompt templates from AI References table during implementation
- **[AI Tasks - MANDATORY]** Implement and test guardrails before marking task complete
- **[AI Tasks - MANDATORY]** Verify AIR-XXX requirements are met (quality, safety, operational)
- **[AI Tasks - MANDATORY]** Ensure Gemini free tier limits are enforced (15 RPM, 1M TPM)
