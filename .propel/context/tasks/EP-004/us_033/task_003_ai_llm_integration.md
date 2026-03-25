# Task - task_003_ai_llm_integration

## Requirement Reference

- User Story: us_033
- Story Location: .propel/context/tasks/EP-004/us_033/us_033.md
- Acceptance Criteria:
  - AC-2: Patient responds in natural language (e.g., "I take metformin 500mg twice daily"); AI extracts structured data (medication: metformin, dose: 500mg, frequency: twice daily) and confirms understanding
  - AC-5: Token budget enforcement — each AI request does not exceed 4000 tokens (AIR-O01)
- Edge Cases:
  - AI cannot understand patient response: AI requests clarification with a rephrased question; after 3 consecutive failures, offers manual form option
  - Minimal responses: AI uses follow-up prompts to elicit more detail

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

## Applicable Technology Stack

| Layer | Technology | Version |
|-------|------------|---------|
| Backend | .NET 8 ASP.NET Core Web API | .NET 8.0 |
| AI/ML | Google Gemini (via Google.GenAI .NET SDK) | gemini-2.0-flash |
| AI Gateway | Custom ASP.NET Core middleware | N/A |

**Note**: All code, and libraries, MUST be compatible with versions above. The user has requested Google Gemini instead of Azure OpenAI for the LLM provider. The `Google.GenAI` NuGet package provides the official .NET SDK.

## AI References (AI Tasks Only)

| Reference Type | Value |
|----------------|-------|
| **AI Impact** | Yes |
| **AIR Requirements** | AIR-001, AIR-O01, AIR-O02, AIR-Q03, AIR-S01, AIR-S02 |
| **AI Pattern** | Conversational AI (NLU for structured data extraction) |
| **Prompt Template Path** | src/backend/PatientAccess.Business/Prompts/intake/ |
| **Guardrails Config** | src/backend/PatientAccess.Business/Prompts/intake/guardrails.json |
| **Model Provider** | Google Gemini |

### **CRITICAL: AI Implementation Requirement (AI Tasks Only)**

**IF AI Impact = Yes:**

- **MUST** reference prompt templates from Prompt Template Path during implementation
- **MUST** implement guardrails for input sanitization and output validation
- **MUST** enforce token budget limits per AIR-O01 requirements (4000 tokens max)
- **MUST** implement fallback logic for low-confidence responses
- **MUST** log all prompts/responses for audit (redact PII before logging)
- **MUST** handle model failures gracefully (timeout, rate limit, 5xx errors)

## Mobile References (Mobile Tasks Only)

| Reference Type | Value |
|----------------|-------|
| **Mobile Impact** | No |
| **Platform Target** | N/A |
| **Min OS Version** | N/A |
| **Mobile Framework** | N/A |

## Task Overview

Implement the `IAiIntakeService` interface using Google Gemini (gemini-2.0-flash model) for natural language understanding during conversational patient intake. This task covers: configuring the Gemini client, creating prompt templates for structured health data extraction, implementing the NLU extraction pipeline that parses patient free-text into structured categories (medications, allergies, symptoms, medical history, visit concerns), enforcing the 4000-token budget per request (AIR-O01), implementing circuit breaker pattern for provider failures (AIR-O02), and ensuring PII/PHI is redacted from audit logs (AIR-S01, AIR-S02).

## Dependent Tasks

- task_002_be_intake_api (defines `IAiIntakeService` interface that this task implements)

## Impacted Components

- **NEW** `src/backend/PatientAccess.Business/Services/GeminiIntakeService.cs` — Implementation of `IAiIntakeService` using Google Gemini
- **NEW** `src/backend/PatientAccess.Business/Prompts/intake/system-prompt.txt` — System prompt for intake conversation
- **NEW** `src/backend/PatientAccess.Business/Prompts/intake/extraction-prompt.txt` — Prompt template for structured data extraction
- **NEW** `src/backend/PatientAccess.Business/Prompts/intake/clarification-prompt.txt` — Prompt for generating clarification questions
- **NEW** `src/backend/PatientAccess.Business/Prompts/intake/guardrails.json` — Configuration: max tokens, confidence thresholds, retry limits
- **MODIFY** `src/backend/PatientAccess.Web/Program.cs` — Replace IAiIntakeService stub with GeminiIntakeService registration
- **MODIFY** `src/backend/PatientAccess.Business/PatientAccess.Business.csproj` — Add `Google.GenAI` NuGet package
- **MODIFY** `src/backend/PatientAccess.Web/appsettings.json` — Add Gemini API configuration section

## Implementation Plan

1. **Add NuGet package**: Add `Google.GenAI` package to `PatientAccess.Business.csproj`. This is the official Google Gen AI .NET SDK for Gemini models.

2. **Configure Gemini client**: Add configuration section in `appsettings.json`:
   ```json
   "Gemini": {
     "ApiKey": "",
     "Model": "gemini-2.0-flash",
     "MaxTokensPerRequest": 4000,
     "TimeoutSeconds": 30,
     "ConfidenceThreshold": 0.70
   }
   ```
   API key loaded from environment variable `GEMINI_API_KEY` (never committed to source).

3. **Create prompt templates**:
   - **system-prompt.txt**: Define the AI persona as a healthcare intake assistant. Instruct the model to extract structured data in JSON format with fields: `category`, `extractedItems[]` (each with `name`, `value`, `unit`, `frequency`), `confidenceScore` (0.0-1.0), `nextQuestion`. Constrain output to medical intake categories only.
   - **extraction-prompt.txt**: Template that inserts conversation history and current patient message. Instructs extraction of specific category (medications, allergies, symptoms, history, concerns). Requests JSON output schema.
   - **clarification-prompt.txt**: Template for generating follow-up questions when extraction confidence is low. Instructs rephrasing in patient-friendly language.

4. **Implement GeminiIntakeService**:
   - Constructor: Inject `IConfiguration`, `ILogger`, `IAuditLogService`. Initialize `Google.GenAI.Client` with API key. Load prompt templates from embedded resources or file paths.
   - `ExtractStructuredDataAsync(patientMessage, conversationHistory, category)`:
     1. Build prompt from template with conversation context
     2. Estimate token count; truncate conversation history if exceeding budget (AIR-O01)
     3. Call `client.Models.GenerateContentAsync` with model and config (`Temperature = 0.3`, `MaxOutputTokens = 1000`)
     4. Parse JSON response into structured extraction result
     5. Validate output schema (AIR-Q03) — reject if JSON is malformed
     6. Return extracted data with confidence score
   - `GenerateNextQuestionAsync(conversationHistory, currentCategory, completedCategories)`:
     1. Build prompt with context of completed vs remaining categories
     2. Call Gemini with low temperature (0.2) for consistent question generation
     3. Return next question text and target category

5. **Implement token budget enforcement** (AIR-O01):
   - Use a simple character-to-token ratio estimation (1 token ≈ 4 characters for English text)
   - Before each API call, calculate estimated input tokens (system prompt + conversation history + user message)
   - If estimated tokens > 4000, truncate older conversation history while preserving the last 3 exchanges
   - Log a warning when truncation occurs

6. **Implement circuit breaker** (AIR-O02):
   - Track consecutive failures to Gemini API
   - After 3 consecutive failures (timeout or 5xx), open circuit for 30 seconds
   - During open circuit, return a fallback response: `{ "error": "AI service temporarily unavailable", "suggestManualFallback": true }`
   - After 30s, allow one probe request (half-open state)
   - Use exponential backoff for retries: 1s, 2s, 4s

7. **Implement PII redaction for audit logging** (AIR-S01, AIR-S02):
   - Before logging prompts/responses, replace patterns matching SSN, phone numbers, email addresses, and dates of birth with `[REDACTED]`
   - Log redacted versions via `IAuditLogService`
   - Retain full (unredacted) data only in the `IntakeRecord` JSONB columns (encrypted at rest per NFR-003)

8. **Register in DI**: Update `Program.cs` to bind `IAiIntakeService` → `GeminiIntakeService` as scoped service.

## Current Project State

```
src/backend/
├── PatientAccess.Business/
│   ├── Interfaces/
│   │   └── IAiIntakeService.cs   # Created in task_002
│   ├── Services/
│   │   └── IntakeService.cs      # Created in task_002 (consumes IAiIntakeService)
│   └── Prompts/                  # (new directory)
├── PatientAccess.Web/
│   ├── appsettings.json
│   └── Program.cs
```

## Expected Changes

| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/Services/GeminiIntakeService.cs | IAiIntakeService implementation using Google Gemini SDK |
| CREATE | src/backend/PatientAccess.Business/Prompts/intake/system-prompt.txt | System prompt defining AI persona and JSON output schema |
| CREATE | src/backend/PatientAccess.Business/Prompts/intake/extraction-prompt.txt | Template for structured data extraction from patient text |
| CREATE | src/backend/PatientAccess.Business/Prompts/intake/clarification-prompt.txt | Template for clarification question generation |
| CREATE | src/backend/PatientAccess.Business/Prompts/intake/guardrails.json | Config: token limits, confidence thresholds, retry settings |
| MODIFY | src/backend/PatientAccess.Business/PatientAccess.Business.csproj | Add Google.GenAI NuGet package reference |
| MODIFY | src/backend/PatientAccess.Web/appsettings.json | Add Gemini configuration section |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register GeminiIntakeService as IAiIntakeService |

## External References

- [Google Gen AI .NET SDK (official)](https://github.com/googleapis/dotnet-genai)
- [Gemini API Documentation](https://ai.google.dev/gemini-api/docs)
- [Gemini Models — gemini-2.0-flash](https://ai.google.dev/gemini-api/docs/models#gemini-2.0-flash)
- [Circuit Breaker Pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/circuit-breaker)
- UC-007 Sequence Diagram: .propel/context/docs/models.md#UC-007

## Build Commands

```bash
cd src/backend
dotnet restore PatientAccess.Business/PatientAccess.Business.csproj
dotnet build PatientAccess.sln
dotnet test PatientAccess.Tests/PatientAccess.Tests.csproj
```

## Implementation Validation Strategy

- [ ] Unit tests pass
- [ ] **[AI Tasks]** Prompt templates validated with test inputs
- [ ] **[AI Tasks]** Guardrails tested for input sanitization and output validation
- [ ] **[AI Tasks]** Fallback logic tested with low-confidence/error scenarios
- [ ] **[AI Tasks]** Token budget enforcement verified
- [ ] **[AI Tasks]** Audit logging verified (no PII in logs)
- [ ] Gemini API call successful with test patient message
- [ ] Structured JSON extraction validated for all 5 categories
- [ ] Circuit breaker opens after 3 consecutive failures and recovers after timeout
- [ ] Token budget capped at 4000 tokens per request with history truncation

## Implementation Checklist

- [ ] Add `Google.GenAI` NuGet package to `PatientAccess.Business.csproj`
- [ ] Add Gemini configuration section to `appsettings.json` with API key from environment variable
- [ ] Create system prompt template defining healthcare intake persona and JSON output schema
- [ ] Create extraction and clarification prompt templates with placeholder tokens
- [ ] Implement `GeminiIntakeService` with `ExtractStructuredDataAsync` and `GenerateNextQuestionAsync`
- [ ] Implement token budget estimation and conversation history truncation (AIR-O01 — 4000 token cap)
- [ ] Implement circuit breaker with 30s timeout and exponential backoff (AIR-O02)
- [ ] Implement PII redaction for audit logging of prompts/responses (AIR-S01, AIR-S02)
- **[AI Tasks - MANDATORY]** Reference prompt templates from AI References table during implementation
- **[AI Tasks - MANDATORY]** Implement and test guardrails before marking task complete
- **[AI Tasks - MANDATORY]** Verify AIR-XXX requirements are met (AIR-001, AIR-O01, AIR-O02, AIR-Q03, AIR-S01, AIR-S02)
