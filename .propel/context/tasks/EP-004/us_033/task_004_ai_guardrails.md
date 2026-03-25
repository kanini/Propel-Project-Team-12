# Task - task_004_ai_guardrails

## Requirement Reference

- User Story: us_033
- Story Location: .propel/context/tasks/EP-004/us_033/us_033.md
- Acceptance Criteria:
  - AC-4: When AI confidence drops below 70%, system suggests switching to manual form mode while preserving all entered data (AIR-S03)
  - AC-5: Token budget enforcement (AIR-O01) — each AI request does not exceed 4000 tokens
- Edge Cases:
  - AI cannot understand response: After 3 consecutive failures, offers manual form option
  - Patient provides minimal responses: Follow-up prompts while remaining patient-friendly

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

**Note**: All code, and libraries, MUST be compatible with versions above.

## AI References (AI Tasks Only)

| Reference Type | Value |
|----------------|-------|
| **AI Impact** | Yes |
| **AIR Requirements** | AIR-S03, AIR-O01, AIR-Q03, AIR-S02 |
| **AI Pattern** | Guardrails (validation, confidence thresholds, output schema enforcement) |
| **Prompt Template Path** | src/backend/PatientAccess.Business/Prompts/intake/ |
| **Guardrails Config** | src/backend/PatientAccess.Business/Prompts/intake/guardrails.json |
| **Model Provider** | Google Gemini |

### **CRITICAL: AI Implementation Requirement (AI Tasks Only)**

**IF AI Impact = Yes:**

- **MUST** reference prompt templates from Prompt Template Path during implementation
- **MUST** implement guardrails for input sanitization and output validation
- **MUST** enforce token budget limits per AIR-O01 requirements (4000 tokens max)
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

## Task Overview

Implement the AI guardrails layer for the conversational intake pipeline. This task creates a validation middleware that wraps the Gemini LLM calls to ensure: output JSON schema validation (AIR-Q03), confidence threshold evaluation with manual fallback trigger (AIR-S03), input sanitization to prevent prompt injection, content filtering to restrict AI responses to medical intake scope only, and structured audit logging of all AI interactions (AIR-S02). This layer sits between `IntakeService` and `GeminiIntakeService` as a decorator pattern.

## Dependent Tasks

- task_002_be_intake_api (defines `IAiIntakeService` interface)
- task_003_ai_llm_integration (provides core Gemini integration)

## Impacted Components

- **NEW** `src/backend/PatientAccess.Business/Services/GuardedAiIntakeService.cs` — Decorator wrapping `GeminiIntakeService` with guardrails
- **NEW** `src/backend/PatientAccess.Business/Validation/IntakeOutputValidator.cs` — JSON schema validation for AI extraction output
- **NEW** `src/backend/PatientAccess.Business/Validation/IntakeInputSanitizer.cs` — Input sanitization to prevent prompt injection
- **MODIFY** `src/backend/PatientAccess.Business/Prompts/intake/guardrails.json` — Populate guardrail configuration values
- **MODIFY** `src/backend/PatientAccess.Web/Program.cs` — Register `GuardedAiIntakeService` as decorator around `GeminiIntakeService`

## Implementation Plan

1. **Create guardrails configuration** (`guardrails.json`):
   ```json
   {
     "confidenceThreshold": 0.70,
     "maxConsecutiveFailures": 3,
     "maxTokensPerRequest": 4000,
     "maxInputLength": 2000,
     "allowedCategories": ["medications", "allergies", "symptoms", "medicalHistory", "visitConcerns"],
     "blockedPatterns": ["ignore previous", "system prompt", "forget instructions"],
     "requiredOutputFields": ["category", "extractedItems", "confidenceScore", "nextQuestion"]
   }
   ```

2. **Implement IntakeInputSanitizer**:
   - Strip HTML tags and script elements from patient input
   - Detect and block prompt injection patterns (e.g., "ignore previous instructions", "system prompt") using configurable blocked patterns from `guardrails.json`
   - Enforce maximum input length (2000 characters)
   - Log sanitization events for audit trail

3. **Implement IntakeOutputValidator**:
   - Validate AI response is valid JSON
   - Verify required fields exist (`category`, `extractedItems`, `confidenceScore`, `nextQuestion`) per `guardrails.json`
   - Validate `confidenceScore` is between 0.0 and 1.0
   - Validate `category` is in the allowed categories list
   - Validate `extractedItems` array elements have required structure (`name`, `value`)
   - Return validation result with specific failure reasons

4. **Implement GuardedAiIntakeService** (decorator pattern):
   - Implements `IAiIntakeService` interface
   - Constructor takes inner `GeminiIntakeService` instance, `IntakeInputSanitizer`, `IntakeOutputValidator`, `ILogger`, `IAuditLogService`
   - `ExtractStructuredDataAsync`:
     1. Sanitize input via `IntakeInputSanitizer`; reject if blocked patterns detected
     2. Delegate to inner `GeminiIntakeService.ExtractStructuredDataAsync`
     3. Validate output via `IntakeOutputValidator`; if invalid, return low-confidence fallback result
     4. Evaluate confidence threshold: if `confidenceScore < 0.70`, flag for fallback
     5. Log redacted prompt/response pair to audit service (AIR-S02)
     6. Return validated result
   - `GenerateNextQuestionAsync`: Apply same input sanitization, delegate to inner service, validate output is non-empty text

5. **Register decorator in DI**: Use Scrutor or manual decorator registration in `Program.cs`:
   ```csharp
   services.AddScoped<GeminiIntakeService>();
   services.AddScoped<IAiIntakeService>(sp =>
       new GuardedAiIntakeService(
           sp.GetRequiredService<GeminiIntakeService>(),
           sp.GetRequiredService<IntakeInputSanitizer>(),
           sp.GetRequiredService<IntakeOutputValidator>(),
           sp.GetRequiredService<ILogger<GuardedAiIntakeService>>(),
           sp.GetRequiredService<IAuditLogService>()));
   ```

6. **Implement structured audit logging** (AIR-S02):
   - Log each AI interaction with: timestamp, session ID, request category, token count estimate, confidence score, validation result
   - Redact all PII/PHI from logged prompt/response text
   - Use existing `AuditLogService` infrastructure with new action type `AiIntakeInteraction`

## Current Project State

```
src/backend/
├── PatientAccess.Business/
│   ├── Interfaces/
│   │   └── IAiIntakeService.cs         # From task_002
│   ├── Services/
│   │   ├── IntakeService.cs            # From task_002
│   │   └── GeminiIntakeService.cs      # From task_003
│   ├── Prompts/
│   │   └── intake/
│   │       ├── system-prompt.txt       # From task_003
│   │       ├── extraction-prompt.txt   # From task_003
│   │       ├── clarification-prompt.txt # From task_003
│   │       └── guardrails.json         # Skeleton from task_003
│   └── Validation/                     # (new directory)
```

## Expected Changes

| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/Services/GuardedAiIntakeService.cs | Decorator implementing IAiIntakeService with guardrails |
| CREATE | src/backend/PatientAccess.Business/Validation/IntakeOutputValidator.cs | JSON schema validation for AI extraction results |
| CREATE | src/backend/PatientAccess.Business/Validation/IntakeInputSanitizer.cs | Input sanitization and prompt injection prevention |
| MODIFY | src/backend/PatientAccess.Business/Prompts/intake/guardrails.json | Populate complete guardrail configuration |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register GuardedAiIntakeService as IAiIntakeService decorator |

## External References

- [OWASP Prompt Injection Prevention](https://owasp.org/www-project-top-10-for-large-language-model-applications/)
- [Decorator Pattern in ASP.NET Core DI](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-8.0)
- [JSON Schema Validation in .NET](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/overview)
- AIR-S03 Specification: .propel/context/docs/design.md#AI-Safety-Requirements
- AIR-O01 Specification: .propel/context/docs/design.md#AI-Operational-Requirements

## Build Commands

```bash
cd src/backend
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
- [ ] Prompt injection attempts blocked and logged
- [ ] Malformed AI output gracefully handled with fallback response
- [ ] Confidence < 70% correctly triggers `SuggestManualFallback` flag
- [ ] Audit logs contain redacted AI interactions with no raw PII

## Implementation Checklist

- [ ] Populate `guardrails.json` with confidence threshold (0.70), max tokens (4000), blocked patterns, and required output fields
- [ ] Implement `IntakeInputSanitizer` with HTML stripping, prompt injection detection, and length enforcement
- [ ] Implement `IntakeOutputValidator` with JSON schema validation for AI extraction output structure
- [ ] Implement `GuardedAiIntakeService` decorator wrapping GeminiIntakeService with sanitization, validation, and confidence evaluation
- [ ] Register decorator in DI container replacing direct IAiIntakeService binding
- [ ] Implement structured audit logging with PII redaction for all AI interactions
- **[AI Tasks - MANDATORY]** Reference prompt templates from AI References table during implementation
- **[AI Tasks - MANDATORY]** Implement and test guardrails before marking task complete
- **[AI Tasks - MANDATORY]** Verify AIR-XXX requirements are met (AIR-S03, AIR-O01, AIR-Q03, AIR-S02)
