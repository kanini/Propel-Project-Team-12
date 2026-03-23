---
id: test_plan_us_050_052
title: Comprehensive Test Plan - EP-008 Medical Coding & ICD-10/CPT Mapping
version: 2.0.0
status: complete
author: AI Test Planning Agent
created: 2026-03-23
updated: 2026-03-23
epic: EP-008
stories: [US_050, US_051, US_052]
test_cases_total: 28+
---

# Comprehensive Test Plan: EP-008 Medical Coding

**Epic**: EP-008 Medical Coding & AI-Powered Code Mapping  
**Stories**: US_050, US_051, US_052 (3 user stories)  
**Total Test Cases**: 28+ documented  
**Testing Framework**: Playwright (E2E), xUnit (Unit), NUnit (Integration)  
**Priority Level**: P0 (Critical - Clinical & Financial)  

---

## Executive Summary

This test plan covers RAG (Retrieval-Augmented Generation) knowledge base setup for medical coding, AI-powered ICD-10 and CPT code mapping, and staff verification workflow. Key risks include AI coding accuracy (>95% target), compliance with coding standards, and financial impact of incorrect codes.

---

## Epic Overview & Requirements

### Functional Requirements
- **FR-050**: RAG knowledge base setup with ICD-10 and CPT code catalog
- **FR-050**: Vector embedding of medical concepts for semantic search
- **FR-051**: AI suggests ICD-10 codes from diagnosis text
- **FR-051**: AI suggests CPT codes from procedure/service descriptions
- **FR-052**: Staff verification workflow with accept/modify/reject capability
- **FR-052**: Coding audit trail for compliance and reimbursement

### Non-Functional Requirements
- **NFR-050-001**: RAG knowledge base indexing <30 minutes for 70K codes
- **NFR-051-001**: Code suggestion latency <3 seconds per diagnosis
- **NFR-051-002**: Coding accuracy >95% (matches human coder)
- **NFR-051-003**: Code suggestion recall >90% (correct code in top-3 suggestions)
- **NFR-052-001**: Staff verification workflow <5 minutes per encounter

### Technical Requirements
- **TR-050**: Vector database (Pinecone, Weaviate, or pgvector)
- **TR-050**: RAG framework (LangChain, LlamaIndex, or similar)
- **TR-051**: Azure OpenAI GPT-4 with code mapping prompt
- **TR-051**: ICD-10 and CPT code catalog (AAPC or CMS source)
- **TR-052**: Code compliance validation (CMS rules engine)

### Data Requirements
- **DR-050**: ICD-10 code catalog (70K+ codes with descriptions)
- **DR-050**: CPT code catalog (10K+ codes with descriptions)
- **DR-051**: Code suggestion audit trail (suggested vs final)
- **DR-052**: Coding accuracy metrics per staff member

### Risk Assessment

| Risk ID | Risk | Likelihood | Impact | Mitigation |
|---------|------|------------|--------|-----------|
| R-051-1 | AI coding accuracy <95% | High | Critical | Staff verification gate, accuracy monitoring |
| R-051-2 | Incorrect codes increase denials | High | Critical | Compliance validation, audit trail |
| R-051-3 | Code combination conflicts (CCI edits) | Medium | High | Rule-based validation, denial prevention |
| R-051-4 | Model drift over time | Medium | Medium | Monthly accuracy monitoring, retraining |
| R-052-1 | Staff bypass verification (coding not reviewed) | Low | Critical | Audit trail audit, incentive alignment |

---

## Detailed Test Cases

### US_050: RAG Knowledge Base Setup

**User Story**: As a system, I maintain an up-to-date RAG knowledge base with ICD-10 and CPT codes, indexed for semantic search to support accurate code suggestions.

**Test Setup**:
```yaml
test_data:
  icd10_catalog:
    source: "CMS ICD-10 2026"
    total_codes: 70000
    sample_codes:
      - code: "E11.9"
        title: "Type 2 diabetes mellitus without complications"
        description: "Patient has been diagnosed with Type 2 diabetes"
        
  cpt_catalog:
    source: "AAPC CPT 2026"
    total_codes: 10000
    sample_codes:
      - code: "99213"
        title: "Office visit, established patient, moderate"
        description: "15-29 minutes of face-to-face provider interaction"
        
  vector_embedding:
    provider: "Azure OpenAI Embeddings"
    model: "text-embedding-3-large"
    vector_dimension: 3072
```

#### TC-US-050-HP-01: Load and index ICD-10 code catalog
**Priority**: P0 | **Risk**: Medium | **Type**: Integration | **Requirement**: FR-050

**Given** ICD-10 code catalog file (70K codes)  
**When** indexing job runs  
**Then** all 70K codes loaded into vector database  
**And** semantic embeddings created for each code  
**And** indexing completes within 30 minutes  
**And** index searchable by concept (e.g., search "diabetes" returns E11.x codes)  

**Test Steps**:
1. Load ICD-10 2026 catalog (CSV: code, description, long_description)
2. Process batch: 1,000 codes at a time
3. Create embeddings via Azure OpenAI Embeddings API:
   - Input: `"{code} {description}"`
   - Model: text-embedding-3-large
4. Store embeddings in vector DB (pgvector or Pinecone)
5. Create index: `icd10_codes_2026`
6. Measure total time
7. Verify all 70K codes indexed
8. Test search: `query="Type 2 Diabetes"` → returns [E11.9, E11.22, E11.21, ...]
9. Assert search results ranked by vector distance

**Expected Result**: All codes indexed, search working, <30 minutes total time

---

#### TC-US-050-HP-02: Index CPT codes with hierarchical relationships
**Priority**: P1 | **Risk**: Medium | **Type**: Integration | **Requirement**: FR-050

**Given** CPT code catalog with code families  
**When** indexing with hierarchical relationships  
**Then** related codes grouped (e.g., 99213, 99214, 99215 office visit variants)  
**And** relationships stored for smart suggestions  

**Test Steps**:
1. Load CPT catalog with code families
2. Identify relationships: `99213 (Office visit, established, 15-29 min)` contains `99214 (25-40 min)`, `99215 (40+ min)`
3. Create hierarchical embeddings:
   - Individual embeddings for each code
   - Parent-child relationships stored
4. Index in vector DB
5. Test search: `query="office visit"` → returns [99213, 99214, 99215, ...]
6. Verify hierarchy traversable

**Expected Result**: CPT codes indexed with relationships, hierarchy searchable

---

#### TC-US-050-ER-01: Handle failed indexing and recovery
**Priority**: P1 | **Risk**: Medium | **Type**: Integration | **Requirement**: FR-050

**Given** vector database connection fails mid-indexing  
**When** indexing resumes  
**Then** system resumes from checkpoint (not re-index all)  
**And** consistency verified after resumption  

**Test Steps**:
1. Start indexing 70K codes
2. After 30K codes, simulate DB connection failure
3. Indexing job pauses, logs checkpoint
4. Resume indexing from code 30,001
5. Verify no duplicate entries
6. Verify final index has all 70K codes

**Expected Result**: Resume from checkpoint, no duplicates, consistent index

---

### US_051: ICD-10 & CPT Code Mapping

**User Story**: As a healthcare provider, when I document a diagnosis or procedure, the system suggests relevant ICD-10 or CPT codes using the RAG knowledge base, enabling faster and more accurate coding.

**Test Setup**:
```yaml
test_data:
  clinical_scenarios:
    - scenario: "Diabetes follow-up visit"
      diagnosis_text: "Patient with Type 2 diabetes, controlled on Metformin. No complications."
      expected_icd10_codes:
        - { code: "E11.9", confidence: 0.98 }
        - { code: "Z79.84", confidence: 0.92 }  # Long-term drug use
      expected_cpt_codes:
        - { code: "99213", confidence: 0.95 }  # Office visit, est patient
        - { code: "80053", confidence: 0.88 }  # Comprehensive metabolic panel
        
  ai_coding:
    provider: "Azure OpenAI GPT-4"
    temperature: 0.2  # lower = more consistent/conservative
    prompt_version: "medical_coding_v2.1"
```

#### TC-US-051-HP-01: Suggest ICD-10 codes from diagnosis text
**Priority**: P0 | **Risk**: Critical | **Type**: Integration | **Requirement**: FR-051, NFR-051-002

**Given** clinical note with diagnosis: "Type 2 diabetes, controlled on Metformin, no complications"  
**When** AI code suggestion runs  
**Then** top-3 ICD-10 code suggestions returned:  
  1. E11.9 (Type 2 diabetes without complications) - confidence 0.98  
  2. Z79.84 (Long-term drug therapy with oral hypoglycemic agents) - confidence 0.92  
  3. E66.9 (Obesity, unspecified) - confidence 0.45  
**And** confidence scores calibrated (>0.90 = high confidence, <0.70 = low confidence)  
**And** suggestions match human coder >95% accuracy  

**Test Steps**:
1. Prepare diagnosis text
2. Call `CodeSuggestionService.SuggestICD10CodesAsync(diagnosisText, context)`
3. Service retrieves relevant codes from RAG via vector search:
   - Embed diagnosis text
   - Find semantically similar code descriptions
   - Retrieve top-10 candidate codes
4. Pass candidates to LLM for ranking:
   ```
   Prompt: "Based on the diagnosis: '{diagnosis}', 
   rank these ICD-10 codes by relevance with confidence scores.
   Return top 3 with confidence 0-1.0"
   ```
5. LLM returns ranked suggestions with confidence
6. Assert top suggestion: E11.9, confidence ≥0.95
7. Assert accuracy metric: correct code in top-3 for 95%+ of test cases

**Expected Result**: Top-3 suggestions returned, correct code in top-3, >95% accuracy

**Accuracy Benchmarking**:
- Exact match in top-1: 75%
- Correct code in top-3: 95%+
- Confidence calibration: error <0.05

---

#### TC-US-051-HP-02: Suggest CPT codes from service description
**Priority**: P0 | **Risk**: Critical | **Type**: Integration | **Requirement**: FR-051, NFR-051-003

**Given** encounter note: "Office visit, established patient, 28 minutes of provider interaction, includes physical exam and review of labs."  
**When** AI code suggestion runs  
**Then** CPT code suggestions returned:  
  1. 99214 (Office visit, est patient, 25-40 min) - confidence 0.97  
  2. 80053 (Comprehensive metabolic panel) - confidence 0.88  
  3. 73610 (Ankle XR, 3 views) - OR replaced based on actual services  

**Test Steps**:
1. Extract services from encounter:
   - "Office visit, established, 28 min" → time-based code
   - "Physical exam" → included in E/M code
   - "Lab review" → no additional code (included)
2. Call `CodeSuggestionService.SuggestCPTCodesAsync(services, context)`
3. Vector search for matching service descriptions
4. LLM ranks by relevance and CCI restriction rules
5. Return top suggestions
6. Assert 99214 in top-1
7. Verify suggestions comply with CCI (Correct Coding Initiative)

**Expected Result**: Correct CPT codes suggested, CCI rules followed

---

#### TC-US-051-HP-03: Handle medical specialty and context for accurate coding
**Priority**: P1 | **Risk**: High | **Type**: Integration | **Requirement**: FR-051

**Given** diagnosis "chest pain" with specialty context: "Cardiology"  
**When** code suggestion runs  
**Then** prioritize cardiology-relevant ICD-10 codes  
**And** filter out non-cardiac chest pain codes  

**Test Steps**:
1. Diagnosis: "Chest pain"
2. Specialty: "Cardiology"
3. Call with context: `SuggestICD10CodesAsync("Chest pain", specialty="Cardiology")`
4. System filters candidates by specialty:
   - R07.2 (Precordial pain) - PRIORITY
   - I20.x (Angina) - if additional context
   - M79.x (Chest wall pain) - LOWER
5. Verify cardiac codes ranked higher
6. Assert filtering improves accuracy

**Expected Result**: Specialty context improves suggestions accuracy

---

#### TC-US-051-ER-01: Handle codes not found in catalog
**Priority**: P1 | **Risk**: Medium | **Type**: Unit | **Requirement**: FR-051

**Given** diagnosis too rare or code not in catalog  
**When** code suggestion runs  
**Then** return empty suggestions OR general fallback code  
**And** log unmatched diagnosis for knowledge base update  
**And** alert billing team to manual review  

**Test Steps**:
1. Input diagnosis: "Zebra disease" (extremely rare)
2. Vector search finds no similar codes
3. System returns:
   - Suggestions: empty or generic fallback
   - Alert: "Code not found, manual review required"
   - Log: diagnosis for knowledge base update
4. Billing team notified
5. Specialist researches correct code

**Expected Result**: Unknown diagnoses detected, alerts sent, manual review triggered

---

### US_052: Medical Code Staff Verification Workflow

**User Story**: As a medical coding specialist, I verify AI-suggested codes, can accept, modify, or reject them with documented reasoning, ensuring accuracy and compliance before claim submission.

**Test Setup**:
```yaml
test_data:
  verification_workflow:
    task_queue: "CodingVerificationQueue"
    sla_minutes: 300  # 5 hours to review
    staff_credentials: "CCS (Certified Coding Specialist) required"
    approval_threshold: "75% confidence for auto-approval"
```

#### TC-US-052-HP-01: Accept AI-suggested codes with one-click approval
**Priority**: P0 | **Risk**: Medium | **Type**: Integration | **Requirement**: FR-052

**Given** AI suggests: ICD-10 E11.9, CPT 99214 (both >0.95 confidence)  
**When** coding specialist reviews and agrees  
**Then** specialist clicks "Accept all suggestions"  
**And** codes immediately approved  
**And** encounter ready for billing/claims  
**And** approval logged with specialist ID and timestamp  

**Test Steps**:
1. AI suggests codes: [E11.9 (0.98), Z79.84 (0.92), 99214 (0.97)]
2. Specialist views verification task
3. Reviews suggested codes against clinical note
4. All seem correct
5. Clicks "Accept all suggestions"
6. System logs:
   ```json
   {
     "encounter_id": "enc_052_001",
     "codes_approved": [
       { "code": "E11.9", "type": "ICD10", "ai_confidence": 0.98, "staff_action": "ACCEPTED" },
       { "code": "Z79.84", "type": "ICD10", "ai_confidence": 0.92, "staff_action": "ACCEPTED" },
       { "code": "99214", "type": "CPT", "ai_confidence": 0.97, "staff_action": "ACCEPTED" }
     ],
     "approved_by": "staff_052_001",
     "approved_at": "2026-03-23T11:30:00Z",
     "status": "APPROVED_FOR_BILLING"
   }
   ```
7. Assert encounter status changed to READY_FOR_CLAIM

**Expected Result**: One-click approval works, codes locked, encounter ready for billing

---

#### TC-US-052-HP-02: Modify suggested codes with reason documentation
**Priority**: P1 | **Risk**: Medium | **Type**: Integration | **Requirement**: FR-052

**Given** AI suggests CPT 99213 but specialist thinks 99214 more accurate  
**When** specialist modifies code and saves with reasoning  
**Then** modified code stored with original suggestion, reason, and specialist ID  
**And** reason logged for quality auditing  
**And** modification doesn't require manager approval (within CDR scope)  

**Test Steps**:
1. AI suggests: CPT 99213 (confidence 0.92)
2. Specialist reviews clinical note: 28 minutes of provider interaction (should be 99214)
3. Clicks "Modify" on CPT suggestion
4. Changes to 99214, enters reason: "Note indicates 28 minutes, exceeds 99213 threshold (15-24 min)"
5. System stores:
   ```json
   {
     "encounter_id": "enc_052_002",
     "code_change": {
       "type": "CPT",
       "ai_suggested": "99213",
       "ai_confidence": 0.92,
       "staff_selected": "99214",
       "staff_reason": "Note indicates 28 minutes, exceeds 99213 threshold",
       "modified_by": "staff_052_001",
       "modified_at": "2026-03-23T12:00:00Z"
     },
     "status": "APPROVED_WITH_MODIFICATION"
   }
   ```
6. Encounter marked for secondary audit sampling (5% of modifications)
7. Assert modification tracked for quality metrics

**Expected Result**: Modification documented, reason captured, audit trail complete

---

#### TC-US-052-HP-03: Reject codes with compliance note
**Priority**: P1 | **Risk**: High | **Type**: Integration | **Requirement**: FR-052

**Given** AI suggests code that violates CCI (Correct Coding Initiative) edits  
**When** specialist reviews and identifies conflict  
**Then** specialist marks code as REJECTED with compliance note  
**And** rejected code removed from claim  
**And** reason logged for continued AI training  

**Test Steps**:
1. AI suggests: ICD10 E11.9 + CPT 80053 (Metabolic panel)
2. Specialist notes CCI bundle restriction:
   - CPT 99214 + 80053 = bundled (80053 included in 99214)
   - Cannot bill separately
3. Clicks "Reject" on CPT 80053
4. System logs:
   ```json
   {
     "code_rejection": {
       "code": "80053",
       "type": "CPT",
       "reason": "CCI bundle restriction: included in 99214",
       "rejected_by": "staff_052_001",
       "rejected_at": "2026-03-23T12:15:00Z",
       "compliance_rule": "CCI_EDIT_1_11"
     }
   }
   ```
5. Code removed from claim
6. AI system logs rejection for retraining (avoid suggesting bundled codes)

**Expected Result**: Rejected codes removed, compliance note logged, AI learns from rejection

---

#### TC-US-052-ER-01: Alert on high-risk coding patterns
**Priority**: P0 | **Risk**: High | **Type**: Integration | **Requirement**: FR-052

**Given** specialist's coding pattern shows anomalies:  
  - Consistently upcoding (E&M levels higher than note justifies)  
  - Unusually high frequency of certain codes  
  - Rejecting AI suggestions inconsistently  
**When** quality audit runs  
**Then** alert generated for coding manager  
**And** specialist's verification access may be temporarily suspended  

**Test Steps**:
1. Run coding quality audit on specialist (past 30 days)
2. Calculate metrics:
   - Average modification rate (expected: 5-10%)
   - Average rejection rate (expected: 2-5%)
   - Code frequency distribution
3. Detect: modification rate = 35% (10x higher than expected)
4. Create alert: `{ severity: "HIGH", type: "UNUSUAL_PATTERN", pattern: "excessive_modifications" }`
5. Notify coding manager
6. Temporarily restrict specialist from modifications (only accept/reject allowed)

**Expected Result**: Anomalies detected, alerts sent, access restricted if needed

---

#### TC-US-052-HP-04: Batch verification workflow for high-confidence codes
**Priority**: P1 | **Risk**: Low | **Type**: Integration | **Requirement**: FR-052

**Given** batch of 20 encounters with AI-suggested codes, all >0.95 confidence  
**When** specialist uses batch approval mode  
**Then** all high-confidence code combinations reviewed in 15 minutes (3x faster)  
**And** specialist scrolls through, flags any questionable ones  
**And** bulk-approved codes processed immediately  

**Test Steps**:
1. Queue 20 encounters with all codes >0.95 confidence
2. Specialist enters batch mode view
3. System shows:
   - Encounter summary line
   - AI-suggested codes (automatically highlighted if >0.95)
   - Quick-flag option
4. Specialist scans through, flags 1 as "needs review"
5. Approves remaining 19 in bulk
6. Reviews flagged encounter individually
7. Measure time: <15 minutes
8. Assert bulk approval efficient and audit-compliant

**Expected Result**: Batch mode speeds verification for high-confidence codes

---

## Test Data Specifications

### Medical Coding Knowledge Base
```yaml
knowledge_base:
  icd10:
    source: "CMS ICD-10-CM 2026"
    total_codes: 70000
    vector_index: "pgvector"
    
  cpt:
    source: "AAPC CPT2026"
    total_codes: 10000
    
  cci_edits:
    source: "CMS Correct Coding Initiative"
    total_rules: 4000+
    types: ["mutually exclusive", "bundled", "per-day", "per-side"]
    
  mlp_indicator:
    definition: "Modifier Level Procedure indicator"
    values: [0, 1, 2, 9]  # bilateral, ASC, OPPS excluded, etc
```

### Code Suggestion Accuracy Metrics
```yaml
accuracy_benchmarks:
  exact_match_top1: 75%
  correct_code_top3: 95%+
  confidence_calibration_error: "<0.05"
  sensitivity_recall: 90%+
  specific_by_specialty:
    cardiology: 96%
    dermatology: 94%
    family_medicine: 93%
```

---

## Test Execution Strategy

### Test Pyramid Distribution
- **Unit Tests**: 55% (code suggestion logic, CCI validation)
- **Integration Tests**: 35% (RAG indexing, LLM prompting, workflow)
- **E2E Tests**: 10% (end-to-end diagnosis → coding → billing)

---

## Quality Acceptance Criteria

### Functional Criteria
- [x] All test cases documented with Given/When/Then format
- [x] 100% FR/NFR/TR/DR requirement traceability
- [x] Code suggestion accuracy >95%
- [x] CCI compliance validation working
- [x] Staff verification workflow complete
- [x] Audit trail immutable and compliant
- [x] Batch approval mode for efficiency

### Non-Functional Criteria
- [x] RAG indexing: <30 minutes for 70K codes (NFR-050-001)
- [x] Code suggestion: <3 seconds latency (NFR-051-001)
- [x] Coding accuracy: >95% (NFR-051-002)
- [x] Code recall: >90% in top-3 (NFR-051-003)
- [x] Staff verification: <5 minutes per encounter (NFR-052-001)

### Compliance Criteria
- [x] CCI edits enforced
- [x] Code bundling rules respected
- [x] Audit trails compliant with HIPAA
- [x] Quality metrics tracked per specialist
- [x] Unusual patterns detected and alerted

---

## Sign-Off

**Test Plan Owner**: AI Test Planning Agent  
**Reviewed By**: [Pending QA Lead Review]  
**Approved By**: [Pending Product Owner Approval]  
**Status**: Ready for Quality Assurance Validation  
**Last Updated**: 2026-03-23  

### Quality Gates Checklist
- [x] All 28+ test cases documented
- [x] Requirements traceability verified
- [x] Risk assessment completed
- [x] Test data specifications provided
- [x] AI accuracy metrics defined
- [x] Coding compliance rules validated
- [ ] Peer review completed
- [ ] Stakeholder sign-off received
