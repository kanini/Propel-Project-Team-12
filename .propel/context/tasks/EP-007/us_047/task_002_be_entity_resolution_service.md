# Task - task_002_be_entity_resolution_service

## Requirement Reference
- User Story: US_047
- Story Location: .propel/context/tasks/EP-007/us_047/us_047.md
- Acceptance Criteria:
    - **AC1**: Given a patient has multiple extracted documents, When aggregation runs, Then identical or equivalent data points (same medication, same dose) are de-duplicated into a single consolidated record using entity resolution.
    - **AC2**: Given different documents contain the same medication with different doses, When aggregation detects the difference, Then both entries are retained with source document references and flagged as a potential conflict.
- Edge Case:
    - What happens when entity resolution cannot determine if two entries are duplicates? Items are merged with both source references and flagged for staff review.

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
| Library | FuzzySharp | 2.x |
| AI/ML | N/A | N/A |
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

Implement entity resolution service to detect duplicate clinical data points across multiple documents using fuzzy string matching, structural comparison, and rule-based logic. This task creates deterministic algorithms for identifying identical or equivalent medications, conditions, allergies, and encounters, applying similarity thresholds (exact match, high similarity >90%, potential match 70-90%, distinct <70%), conflict detection for same entity with different attributes (e.g., medication with different doses), and ambiguity flagging when resolution confidence is low. The service enables accurate de-duplication while preserving data lineage and surfacing conflicts for staff verification.

**Key Capabilities:**
- Fuzzy string matching using Levenshtein distance (FuzzySharp library)
- Medication matching (drug name + dosage + frequency)
- Condition matching (condition name + ICD-10 code)
- Allergy matching (allergen name + severity)
- Encounter matching (date + type + provider + facility)
- Conflict detection (same entity, different critical attributes)
- Ambiguity threshold (70-90% similarity = flag for review)
- Source document tracking for merged duplicates
- Batch processing for efficiency

## Dependent Tasks
- EP-007: US_047: task_001_db_patient_profile_schema (PatientProfile entities)
- EP-006-II: US_045: task_001_db_extracted_data_schema (ExtractedClinicalData table)

## Impacted Components
- **NEW**: `src/backend/PatientAccess.Business/Services/IEntityResolutionService.cs` - Entity resolution interface
- **NEW**: `src/backend/PatientAccess.Business/Services/EntityResolutionService.cs` - Entity resolution implementation
- **NEW**: `src/backend/PatientAccess.Business/Models/EntityMatchResult.cs` - Match result model
- **NEW**: `src/backend/PatientAccess.Business/Models/EntitySimilarity.cs` - Similarity calculation model
- **NEW**: `src/backend/PatientAccess.Business/Utilities/FuzzyMatcher.cs` - Fuzzy string matching utility
- **NEW**: `src/backend/PatientAccess.Tests/Services/EntityResolutionServiceTests.cs` - Unit tests
- **MODIFY**: `src/backend/PatientAccess.Web/Program.cs` - Register entity resolution service

## Implementation Plan

1. **Add FuzzySharp NuGet Package**
   - Install `FuzzySharp` version 2.x for Levenshtein distance calculation
   - Configure in `PatientAccess.Business.csproj`

2. **Create FuzzyMatcher Utility**
   - Implement static methods:
     - `CalculateSimilarity(string str1, string str2)` - Returns 0-100 similarity score using Levenshtein distance
     - `NormalizeString(string input)` - Lowercase, trim, remove punctuation for comparison
     - `IsExactMatch(string str1, string str2)` - Case-insensitive exact match
     - `IsHighSimilarity(string str1, string str2, int threshold = 90)` - Similarity >= 90%
     - `IsPotentialMatch(string str1, string str2, int minThreshold = 70)` - Similarity 70-90%
   - Use FuzzySharp's `Fuzz.Ratio()` for basic similarity, `Fuzz.PartialRatio()` for substring matching

3. **Create EntityMatchResult Model**
   - Properties:
     - IsMatch (bool)
     - MatchType (enum: ExactMatch, HighSimilarity, PotentialMatch, NoMatch)
     - SimilarityScore (int, 0-100)
     - HasConflict (bool, same entity with different critical attributes)
     - ConflictDetails (string, description of conflict)
     - RequiresManualReview (bool, ambiguous cases)
     - SourceEntityIds (List<Guid>, entities being compared)

4. **Create EntitySimilarity Model**
   - Properties:
     - EntityId (Guid)
     - ComparisonEntityId (Guid)
     - SimilarityScore (int, 0-100)
     - MatchedFields (List<string>, fields that matched)
     - ConflictingFields (List<string>, fields that differ)

5. **Implement Medication Matching Logic**
   - Method: `ResolveMedicationDuplicatesAsync(List<ExtractedClinicalData> medications)`
   - Algorithm:
     - Normalize drug names (remove brand variations, dosage forms)
     - Compare drug names using fuzzy matching (threshold: 85% for potential match)
     - If drug names match (>85%):
       * Compare dosage: exact match = duplicate, mismatch = conflict
       * Compare frequency: exact match = duplicate, mismatch = conflict
       * If dosage/frequency conflict: flag as conflict, retain both
       * If dosage/frequency match: mark as duplicate, merge with source references
     - Return EntityMatchResult with match type and conflict details

6. **Implement Condition Matching Logic**
   - Method: `ResolveConditionDuplicatesAsync(List<ExtractedClinicalData> conditions)`
   - Algorithm:
     - If ICD-10 codes present: exact ICD-10 match = duplicate (highest confidence)
     - If no ICD-10 or mismatch: compare condition names using fuzzy matching (threshold: 90%)
     - Handle synonyms: "diabetes mellitus" = "diabetes", "hypertension" = "high blood pressure"
     - If condition name matches but ICD-10 differs: flag as potential conflict
     - Return merged condition with all source references

7. **Implement Allergy Matching Logic**
   - Method: `ResolveAllergyDuplicatesAsync(List<ExtractedClinicalData> allergies)`
   - Algorithm:
     - Normalize allergen names (remove formatting variations)
     - Compare allergen names using fuzzy matching (threshold: 85%)
     - If allergen names match:
       * Compare severity: if different, flag as conflict (e.g., "Moderate" vs "Severe")
       * Compare reaction: ignore minor differences, flag major conflicts
     - Return merged allergy with source references

8. **Implement Encounter Matching Logic**
   - Method: `ResolveEncounterDuplicatesAsync(List<ExtractedClinicalData> encounters)`
   - Algorithm:
     - Compare encounter dates (same day = potential duplicate)
     - Compare encounter types (exact match required)
     - Compare provider and facility (fuzzy match, threshold: 80%)
     - If date + type + provider/facility match: mark as duplicate
     - Return merged encounter with source references

## Current Project State

```
src/backend/
├── PatientAccess.Business/
│   ├── Services/
│   │   ├── ClinicalDataExtractionService.cs (from EP-006-II)
│   │   └── DocumentProcessingService.cs (from EP-006-I)
│   └── DTOs/
└── PatientAccess.Data/
    ├── Entities/
    │   ├── ExtractedClinicalData.cs (from EP-006-II)
    │   └── PatientProfile.cs (from task_001)
    └── ApplicationDbContext.cs
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/Services/IEntityResolutionService.cs | Entity resolution interface |
| CREATE | src/backend/PatientAccess.Business/Services/EntityResolutionService.cs | Entity resolution implementation |
| CREATE | src/backend/PatientAccess.Business/Models/EntityMatchResult.cs | Match result model |
| CREATE | src/backend/PatientAccess.Business/Models/EntitySimilarity.cs | Similarity calculation model |
| CREATE | src/backend/PatientAccess.Business/Utilities/FuzzyMatcher.cs | Fuzzy string matching utility |
| CREATE | src/backend/PatientAccess.Tests/Services/EntityResolutionServiceTests.cs | Unit tests |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register entity resolution service |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### FuzzySharp Documentation
- **GitHub**: https://github.com/JakeBayer/FuzzySharp
- **Levenshtein Distance**: https://en.wikipedia.org/wiki/Levenshtein_distance
- **String Similarity Algorithms**: https://www.dotnetperls.com/levenshtein

### Entity Resolution Patterns
- **Microsoft Docs**: https://learn.microsoft.com/en-us/azure/architecture/data-guide/scenarios/search#entity-resolution
- **Data Deduplication**: https://en.wikipedia.org/wiki/Data_deduplication

### Design Requirements
- **FR-030**: System MUST aggregate extracted data into a de-duplicated, consolidated patient profile (spec.md)
- **FR-031**: System MUST explicitly highlight critical data conflicts requiring staff verification (spec.md)
- **AIR-005**: System MUST use entity resolution pattern for data consolidation (design.md)
- **AG-003**: Reduce clinical preparation time through automated data aggregation (design.md)

### Existing Codebase Patterns
- **Service Pattern**: `src/backend/PatientAccess.Business/Services/ClinicalDataExtractionService.cs`
- **DTO Pattern**: `src/backend/PatientAccess.Business/DTOs/ExtractedDataPointDto.cs`

## Build Commands
```powershell
# Add FuzzySharp NuGet package
cd src/backend
dotnet add PatientAccess.Business package FuzzySharp

# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test --filter "FullyQualifiedName~EntityResolutionServiceTests"

# Run application
cd PatientAccess.Web
dotnet run
```

## Implementation Validation Strategy
- [ ] Unit tests pass (medication, condition, allergy, encounter matching)
- [ ] Exact matches detected correctly (100% similarity)
- [ ] High similarity matches detected (>90% threshold)
- [ ] Potential matches flagged correctly (70-90% threshold)
- [ ] Conflicts detected when same entity has different critical attributes
- [ ] Medication dosage mismatches flagged as conflicts
- [ ] Condition name variations matched correctly (synonyms)
- [ ] Allergy severity differences flagged as conflicts
- [ ] Encounter duplicates on same day detected
- [ ] Ambiguous cases flagged for manual review
- [ ] Source document references preserved for merged duplicates
- [ ] Performance acceptable for batch processing (100+ items <1 second)

## Implementation Checklist
- [ ] Add FuzzySharp NuGet package to PatientAccess.Business
- [ ] Create FuzzyMatcher utility with CalculateSimilarity and NormalizeString methods
- [ ] Create EntityMatchResult model with MatchType enum
- [ ] Create EntitySimilarity model with SimilarityScore property
- [ ] Implement ResolveMedicationDuplicatesAsync with dosage/frequency conflict detection
- [ ] Implement ResolveConditionDuplicatesAsync with ICD-10 code matching
- [ ] Implement ResolveAllergyDuplicatesAsync with severity conflict detection
- [ ] Implement ResolveEncounterDuplicatesAsync with date + type + provider matching
- [ ] Register EntityResolutionService in Program.cs dependency injection
- [ ] Write unit tests for each matching algorithm with edge cases
- [ ] Validate similarity thresholds with real-world test data
- [ ] Test performance with batch processing (100+ items)
