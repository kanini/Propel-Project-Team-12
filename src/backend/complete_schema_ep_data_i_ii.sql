-- ============================================================================
-- Complete Database Schema for EP-DATA-I and EP-DATA-II
-- Patient Access Platform - Healthcare Management System
-- PostgreSQL 16+ with pgvector extension
-- ============================================================================
-- Description: This script creates all database tables, constraints, indexes,
--              and enums for both EP-DATA-I (Core Schema) and EP-DATA-II
--              (Extended Entities). Safe to run multiple times (idempotent).
-- ============================================================================

-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "vector";

-- ============================================================================
-- PART 1: EP-DATA-I - CORE DATA SCHEMA & RELATIONSHIPS
-- ============================================================================

-- ----------------------------------------------------------------------------
-- Table: Users (DR-001)
-- Purpose: Authenticated system users (Patient, Staff, Admin)
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS "Users" (
    "UserId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "Email" VARCHAR(255) NOT NULL,
    "Name" VARCHAR(200) NOT NULL,
    "DateOfBirth" DATE,
    "Phone" VARCHAR(20),
    "PasswordHash" VARCHAR(255) NOT NULL,
    "Role" INTEGER NOT NULL, -- 1=Patient, 2=Staff, 3=Admin
    "Status" INTEGER NOT NULL, -- 1=Active, 2=Suspended, 3=Inactive
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Users_Email" ON "Users" ("Email");
CREATE INDEX IF NOT EXISTS "IX_Users_Role" ON "Users" ("Role");
CREATE INDEX IF NOT EXISTS "IX_Users_Status" ON "Users" ("Status");

COMMENT ON TABLE "Users" IS 'Core user entity for authentication and authorization';
COMMENT ON COLUMN "Users"."Role" IS '1=Patient, 2=Staff, 3=Admin';
COMMENT ON COLUMN "Users"."Status" IS '1=Active, 2=Suspended, 3=Inactive';

-- ----------------------------------------------------------------------------
-- Table: Patients (Extends User - DR-001)
-- Purpose: Patient-specific demographics and healthcare context
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS "Patients" (
    "PatientId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL,
    "EmergencyContactName" VARCHAR(200),
    "EmergencyContactPhone" VARCHAR(20),
    "InsuranceProvider" VARCHAR(200),
    "InsurancePolicyNumber" VARCHAR(100),
    "PrimaryPhysician" VARCHAR(200),
    "BloodType" VARCHAR(10),
    "Allergies" TEXT,
    "ChronicConditions" TEXT,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ,
    CONSTRAINT "FK_Patients_Users" FOREIGN KEY ("UserId") REFERENCES "Users"("UserId") ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Patients_UserId" ON "Patients" ("UserId");

COMMENT ON TABLE "Patients" IS 'Extended patient profile with healthcare-specific data';

-- ----------------------------------------------------------------------------
-- Table: Providers (DR-002)
-- Purpose: Healthcare provider reference (no login in Phase 1)
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS "Providers" (
    "ProviderId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "Name" VARCHAR(200) NOT NULL,
    "Specialty" VARCHAR(100) NOT NULL,
    "Email" VARCHAR(255),
    "Phone" VARCHAR(20),
    "LicenseNumber" VARCHAR(50),
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS "IX_Providers_Specialty" ON "Providers" ("Specialty");
CREATE INDEX IF NOT EXISTS "IX_Providers_IsActive" ON "Providers" ("IsActive");

COMMENT ON TABLE "Providers" IS 'Healthcare provider reference data';

-- ----------------------------------------------------------------------------
-- Table: Appointments (DR-002)
-- Purpose: Patient visit scheduling with status lifecycle
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS "Appointments" (
    "AppointmentId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "PatientId" UUID NOT NULL,
    "ProviderId" UUID NOT NULL,
    "ScheduledDateTime" TIMESTAMPTZ NOT NULL,
    "Status" INTEGER NOT NULL, -- 1=Scheduled, 2=Confirmed, 3=Arrived, 4=Completed, 5=Cancelled, 6=NoShow
    "VisitReason" TEXT NOT NULL,
    "IsWalkIn" BOOLEAN NOT NULL DEFAULT FALSE,
    "ConfirmationReceived" BOOLEAN NOT NULL DEFAULT FALSE,
    "NoShowRiskScore" DECIMAL(5,2),
    "CancellationNoticeHours" INTEGER DEFAULT 24,
    "Notes" TEXT,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ,
    CONSTRAINT "FK_Appointments_Patients" FOREIGN KEY ("PatientId") REFERENCES "Patients"("PatientId") ON DELETE CASCADE,
    CONSTRAINT "FK_Appointments_Providers" FOREIGN KEY ("ProviderId") REFERENCES "Providers"("ProviderId") ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS "IX_Appointments_PatientId" ON "Appointments" ("PatientId");
CREATE INDEX IF NOT EXISTS "IX_Appointments_ProviderId" ON "Appointments" ("ProviderId");
CREATE INDEX IF NOT EXISTS "IX_Appointments_ScheduledDateTime" ON "Appointments" ("ScheduledDateTime");
CREATE INDEX IF NOT EXISTS "IX_Appointments_Status" ON "Appointments" ("Status");

COMMENT ON TABLE "Appointments" IS 'Patient appointment scheduling and tracking';
COMMENT ON COLUMN "Appointments"."Status" IS '1=Scheduled, 2=Confirmed, 3=Arrived, 4=Completed, 5=Cancelled, 6=NoShow';

-- ----------------------------------------------------------------------------
-- Table: TimeSlots (DR-002)
-- Purpose: Provider availability windows
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS "TimeSlots" (
    "TimeSlotId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "ProviderId" UUID NOT NULL,
    "StartTime" TIMESTAMPTZ NOT NULL,
    "EndTime" TIMESTAMPTZ NOT NULL,
    "IsBooked" BOOLEAN NOT NULL DEFAULT FALSE,
    "AppointmentId" UUID,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ,
    CONSTRAINT "FK_TimeSlots_Providers" FOREIGN KEY ("ProviderId") REFERENCES "Providers"("ProviderId") ON DELETE CASCADE,
    CONSTRAINT "FK_TimeSlots_Appointments" FOREIGN KEY ("AppointmentId") REFERENCES "Appointments"("AppointmentId") ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS "IX_TimeSlots_ProviderId" ON "TimeSlots" ("ProviderId");
CREATE INDEX IF NOT EXISTS "IX_TimeSlots_StartTime" ON "TimeSlots" ("StartTime");
CREATE INDEX IF NOT EXISTS "IX_TimeSlots_IsBooked" ON "TimeSlots" ("IsBooked");

COMMENT ON TABLE "TimeSlots" IS 'Provider availability time slots';

-- ----------------------------------------------------------------------------
-- Table: ClinicalDocuments (DR-003)
-- Purpose: Uploaded patient documents with processing status
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS "ClinicalDocuments" (
    "DocumentId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "PatientId" UUID NOT NULL,
    "FileName" VARCHAR(500) NOT NULL,
    "FileSize" BIGINT NOT NULL,
    "FileType" VARCHAR(50) NOT NULL,
    "StoragePath" VARCHAR(1000) NOT NULL,
    "ProcessingStatus" INTEGER NOT NULL, -- 1=Uploaded, 2=Processing, 3=Completed, 4=Failed
    "UploadedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "ProcessedAt" TIMESTAMPTZ,
    "ErrorMessage" TEXT,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ,
    CONSTRAINT "FK_ClinicalDocuments_Patients" FOREIGN KEY ("PatientId") REFERENCES "Patients"("PatientId") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_ClinicalDocuments_PatientId" ON "ClinicalDocuments" ("PatientId");
CREATE INDEX IF NOT EXISTS "IX_ClinicalDocuments_ProcessingStatus" ON "ClinicalDocuments" ("ProcessingStatus");
CREATE INDEX IF NOT EXISTS "IX_ClinicalDocuments_UploadedAt" ON "ClinicalDocuments" ("UploadedAt");

COMMENT ON TABLE "ClinicalDocuments" IS 'Patient clinical document uploads (PDFs)';
COMMENT ON COLUMN "ClinicalDocuments"."ProcessingStatus" IS '1=Uploaded, 2=Processing, 3=Completed, 4=Failed';

-- ----------------------------------------------------------------------------
-- Table: ExtractedClinicalData (DR-004)
-- Purpose: AI-extracted data points from clinical documents
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS "ExtractedClinicalData" (
    "ExtractedDataId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "DocumentId" UUID NOT NULL,
    "PatientId" UUID NOT NULL,
    "DataType" INTEGER NOT NULL, -- 1=Vital, 2=Medication, 3=Allergy, 4=Diagnosis, 5=LabResult, 6=Procedure
    "DataKey" VARCHAR(200) NOT NULL,
    "DataValue" TEXT NOT NULL,
    "ConfidenceScore" DECIMAL(5,2) NOT NULL, -- 0.00 to 100.00
    "VerificationStatus" INTEGER NOT NULL DEFAULT 1, -- 1=Pending, 2=Verified, 3=Rejected
    "SourcePageNumber" INTEGER,
    "SourceTextExcerpt" TEXT,
    "VectorEmbedding" vector(1536), -- pgvector for semantic search
    "VerifiedBy" UUID,
    "VerifiedAt" TIMESTAMPTZ,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ,
    CONSTRAINT "FK_ExtractedData_Documents" FOREIGN KEY ("DocumentId") REFERENCES "ClinicalDocuments"("DocumentId") ON DELETE CASCADE,
    CONSTRAINT "FK_ExtractedData_Patients" FOREIGN KEY ("PatientId") REFERENCES "Patients"("PatientId") ON DELETE CASCADE,
    CONSTRAINT "FK_ExtractedData_VerifiedBy" FOREIGN KEY ("VerifiedBy") REFERENCES "Users"("UserId") ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS "IX_ExtractedData_DocumentId" ON "ExtractedClinicalData" ("DocumentId");
CREATE INDEX IF NOT EXISTS "IX_ExtractedData_PatientId" ON "ExtractedClinicalData" ("PatientId");
CREATE INDEX IF NOT EXISTS "IX_ExtractedData_DataType" ON "ExtractedClinicalData" ("DataType");
CREATE INDEX IF NOT EXISTS "IX_ExtractedData_VerificationStatus" ON "ExtractedClinicalData" ("VerificationStatus");

-- Vector similarity index for semantic search (DR-010)
CREATE INDEX IF NOT EXISTS "IX_ExtractedData_VectorEmbedding" ON "ExtractedClinicalData" USING ivfflat ("VectorEmbedding" vector_cosine_ops) WITH (lists = 100);

COMMENT ON TABLE "ExtractedClinicalData" IS 'AI-extracted clinical data points with confidence scores';
COMMENT ON COLUMN "ExtractedClinicalData"."DataType" IS '1=Vital, 2=Medication, 3=Allergy, 4=Diagnosis, 5=LabResult, 6=Procedure';
COMMENT ON COLUMN "ExtractedClinicalData"."VerificationStatus" IS '1=Pending, 2=Verified, 3=Rejected';

-- ----------------------------------------------------------------------------
-- Table: PatientProfile (DR-004, DR-005)
-- Purpose: Aggregated 360-degree patient view
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS "PatientProfiles" (
    "ProfileId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "PatientId" UUID NOT NULL,
    "AggregatedConditions" JSONB,
    "AggregatedMedications" JSONB,
    "AggregatedAllergies" JSONB,
    "AggregatedVitals" JSONB,
    "IdentifiedConflicts" JSONB,
    "LastAggregatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ,
    CONSTRAINT "FK_PatientProfiles_Patients" FOREIGN KEY ("PatientId") REFERENCES "Patients"("PatientId") ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_PatientProfiles_PatientId" ON "PatientProfiles" ("PatientId");

COMMENT ON TABLE "PatientProfiles" IS 'Aggregated 360-degree patient health view';

-- ----------------------------------------------------------------------------
-- Table: AuditLogs (DR-005, DR-007)
-- Purpose: Immutable audit trail for compliance
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS "AuditLogs" (
    "AuditLogId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL,
    "ActionType" VARCHAR(50) NOT NULL, -- CREATE, READ, UPDATE, DELETE, LOGIN, LOGOUT
    "ResourceType" VARCHAR(100) NOT NULL, -- User, Appointment, ClinicalDocument, etc.
    "ResourceId" UUID,
    "ActionDetails" JSONB NOT NULL,
    "IpAddress" VARCHAR(45),
    "UserAgent" VARCHAR(500),
    "Timestamp" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT "FK_AuditLogs_Users" FOREIGN KEY ("UserId") REFERENCES "Users"("UserId") ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS "IX_AuditLogs_UserId" ON "AuditLogs" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_AuditLogs_ResourceType" ON "AuditLogs" ("ResourceType");
CREATE INDEX IF NOT EXISTS "IX_AuditLogs_Timestamp" ON "AuditLogs" ("Timestamp");
CREATE INDEX IF NOT EXISTS "IX_AuditLogs_ActionType" ON "AuditLogs" ("ActionType");

COMMENT ON TABLE "AuditLogs" IS 'Immutable audit trail (7-year retention per HIPAA DR-007)';

-- ============================================================================
-- PART 2: EP-DATA-II - EXTENDED ENTITIES & REFERENCE DATA
-- ============================================================================

-- ----------------------------------------------------------------------------
-- Table: WaitlistEntries (DR-011)
-- Purpose: Patient preference records for unavailable slots
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS "WaitlistEntries" (
    "WaitlistEntryId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "PatientId" UUID NOT NULL,
    "ProviderId" UUID NOT NULL,
    "PreferredDateStart" DATE NOT NULL,
    "PreferredDateEnd" DATE NOT NULL,
    "PreferredTimeOfDay" INTEGER, -- 1=Morning, 2=Afternoon, 3=Evening, 4=Anytime
    "NotificationPreference" INTEGER NOT NULL, -- 1=Email, 2=SMS, 3=Both
    "Priority" INTEGER NOT NULL DEFAULT 1,
    "Status" INTEGER NOT NULL DEFAULT 1, -- 1=Active, 2=Notified, 3=Fulfilled, 4=Cancelled
    "Reason" TEXT,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ,
    CONSTRAINT "FK_WaitlistEntries_Patients" FOREIGN KEY ("PatientId") REFERENCES "Patients"("PatientId") ON DELETE CASCADE,
    CONSTRAINT "FK_WaitlistEntries_Providers" FOREIGN KEY ("ProviderId") REFERENCES "Providers"("ProviderId") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_WaitlistEntries_PatientId" ON "WaitlistEntries" ("PatientId");
CREATE INDEX IF NOT EXISTS "IX_WaitlistEntries_ProviderId" ON "WaitlistEntries" ("ProviderId");
CREATE INDEX IF NOT EXISTS "IX_WaitlistEntries_Status" ON "WaitlistEntries" ("Status");
CREATE INDEX IF NOT EXISTS "IX_WaitlistEntries_Priority_CreatedAt" ON "WaitlistEntries" ("Priority" DESC, "CreatedAt" ASC);

COMMENT ON TABLE "WaitlistEntries" IS 'Waitlist for unavailable appointment slots';
COMMENT ON COLUMN "WaitlistEntries"."PreferredTimeOfDay" IS '1=Morning, 2=Afternoon, 3=Evening, 4=Anytime';
COMMENT ON COLUMN "WaitlistEntries"."NotificationPreference" IS '1=Email, 2=SMS, 3=Both';
COMMENT ON COLUMN "WaitlistEntries"."Status" IS '1=Active, 2=Notified, 3=Fulfilled, 4=Cancelled';

-- ----------------------------------------------------------------------------
-- Table: IntakeRecords (DR-012)
-- Purpose: Pre-visit patient intake data
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS "IntakeRecords" (
    "IntakeRecordId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "AppointmentId" UUID NOT NULL,
    "PatientId" UUID NOT NULL,
    "IntakeMode" INTEGER NOT NULL, -- 1=AIConversational, 2=ManualForm
    "ChiefComplaint" TEXT,
    "SymptomHistory" JSONB,
    "CurrentMedications" JSONB,
    "KnownAllergies" JSONB,
    "MedicalHistory" JSONB,
    "InsuranceValidationStatus" INTEGER, -- 1=NotValidated, 2=Valid, 3=Invalid
    "ValidatedInsuranceRecordId" UUID,
    "IsCompleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "CompletedAt" TIMESTAMPTZ,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ,
    CONSTRAINT "FK_IntakeRecords_Appointments" FOREIGN KEY ("AppointmentId") REFERENCES "Appointments"("AppointmentId") ON DELETE CASCADE,
    CONSTRAINT "FK_IntakeRecords_Patients" FOREIGN KEY ("PatientId") REFERENCES "Patients"("PatientId") ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_IntakeRecords_AppointmentId" ON "IntakeRecords" ("AppointmentId");
CREATE INDEX IF NOT EXISTS "IX_IntakeRecords_PatientId" ON "IntakeRecords" ("PatientId");
CREATE INDEX IF NOT EXISTS "IX_IntakeRecords_IsCompleted" ON "IntakeRecords" ("IsCompleted");

COMMENT ON TABLE "IntakeRecords" IS 'Pre-visit patient intake data (AI or manual)';
COMMENT ON COLUMN "IntakeRecords"."IntakeMode" IS '1=AIConversational, 2=ManualForm';
COMMENT ON COLUMN "IntakeRecords"."InsuranceValidationStatus" IS '1=NotValidated, 2=Valid, 3=Invalid';

-- ----------------------------------------------------------------------------
-- Table: MedicalCodes (DR-013)
-- Purpose: ICD-10 and CPT code suggestions
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS "MedicalCodes" (
    "MedicalCodeId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "ExtractedDataId" UUID NOT NULL,
    "CodeSystem" VARCHAR(20) NOT NULL, -- ICD10, CPT
    "CodeValue" VARCHAR(20) NOT NULL,
    "CodeDescription" TEXT NOT NULL,
    "ConfidenceScore" DECIMAL(5,2) NOT NULL,
    "VerificationStatus" INTEGER NOT NULL DEFAULT 1, -- 1=Pending, 2=Verified, 3=Rejected
    "VerifiedBy" UUID,
    "VerifiedAt" TIMESTAMPTZ,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ,
    CONSTRAINT "FK_MedicalCodes_ExtractedData" FOREIGN KEY ("ExtractedDataId") REFERENCES "ExtractedClinicalData"("ExtractedDataId") ON DELETE CASCADE,
    CONSTRAINT "FK_MedicalCodes_VerifiedBy" FOREIGN KEY ("VerifiedBy") REFERENCES "Users"("UserId") ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS "IX_MedicalCodes_ExtractedDataId" ON "MedicalCodes" ("ExtractedDataId");
CREATE INDEX IF NOT EXISTS "IX_MedicalCodes_CodeSystem" ON "MedicalCodes" ("CodeSystem");
CREATE INDEX IF NOT EXISTS "IX_MedicalCodes_CodeValue" ON "MedicalCodes" ("CodeValue");
CREATE INDEX IF NOT EXISTS "IX_MedicalCodes_VerificationStatus" ON "MedicalCodes" ("VerificationStatus");

COMMENT ON TABLE "MedicalCodes" IS 'AI-suggested ICD-10 and CPT medical codes';
COMMENT ON COLUMN "MedicalCodes"."VerificationStatus" IS '1=Pending, 2=Verified, 3=Rejected';

-- ----------------------------------------------------------------------------
-- Table: Notifications (DR-014)
-- Purpose: Scheduled and delivered notifications
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS "Notifications" (
    "NotificationId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "PatientId" UUID NOT NULL,
    "AppointmentId" UUID,
    "ChannelType" INTEGER NOT NULL, -- 1=SMS, 2=Email, 3=Both
    "TemplateType" VARCHAR(100) NOT NULL, -- AppointmentReminder, WaitlistNotification, etc.
    "Subject" VARCHAR(500),
    "MessageBody" TEXT NOT NULL,
    "Status" INTEGER NOT NULL DEFAULT 1, -- 1=Scheduled, 2=Sent, 3=Delivered, 4=Failed
    "ScheduledAt" TIMESTAMPTZ NOT NULL,
    "SentAt" TIMESTAMPTZ,
    "DeliveredAt" TIMESTAMPTZ,
    "ErrorMessage" TEXT,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ,
    CONSTRAINT "FK_Notifications_Patients" FOREIGN KEY ("PatientId") REFERENCES "Patients"("PatientId") ON DELETE CASCADE,
    CONSTRAINT "FK_Notifications_Appointments" FOREIGN KEY ("AppointmentId") REFERENCES "Appointments"("AppointmentId") ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS "IX_Notifications_PatientId" ON "Notifications" ("PatientId");
CREATE INDEX IF NOT EXISTS "IX_Notifications_AppointmentId" ON "Notifications" ("AppointmentId");
CREATE INDEX IF NOT EXISTS "IX_Notifications_Status" ON "Notifications" ("Status");
CREATE INDEX IF NOT EXISTS "IX_Notifications_ScheduledAt" ON "Notifications" ("ScheduledAt");

COMMENT ON TABLE "Notifications" IS 'Notification delivery tracking';
COMMENT ON COLUMN "Notifications"."ChannelType" IS '1=SMS, 2=Email, 3=Both';
COMMENT ON COLUMN "Notifications"."Status" IS '1=Scheduled, 2=Sent, 3=Delivered, 4=Failed';

-- ----------------------------------------------------------------------------
-- Table: InsuranceRecords (DR-015)
-- Purpose: Insurance provider reference data for validation
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS "InsuranceRecords" (
    "InsuranceRecordId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "ProviderName" VARCHAR(200) NOT NULL,
    "AcceptedIdPattern" VARCHAR(500), -- Regex pattern for ID validation
    "CoverageType" VARCHAR(100) NOT NULL, -- HMO, PPO, Medicare, Medicaid, etc.
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS "IX_InsuranceRecords_ProviderName" ON "InsuranceRecords" ("ProviderName");
CREATE INDEX IF NOT EXISTS "IX_InsuranceRecords_IsActive" ON "InsuranceRecords" ("IsActive");

COMMENT ON TABLE "InsuranceRecords" IS 'Insurance provider reference data for validation';

-- ----------------------------------------------------------------------------
-- Table: NoShowHistory (DR-016)
-- Purpose: Patient no-show aggregated metrics
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS "NoShowHistory" (
    "NoShowHistoryId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "PatientId" UUID NOT NULL,
    "TotalAppointments" INTEGER NOT NULL DEFAULT 0,
    "NoShowCount" INTEGER NOT NULL DEFAULT 0,
    "ConfirmationResponseRate" DECIMAL(5,2), -- Percentage
    "AverageLeadTimeHours" DECIMAL(10,2),
    "LastCalculatedRiskScore" DECIMAL(5,2),
    "LastCalculatedAt" TIMESTAMPTZ,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ,
    CONSTRAINT "FK_NoShowHistory_Patients" FOREIGN KEY ("PatientId") REFERENCES "Patients"("PatientId") ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_NoShowHistory_PatientId" ON "NoShowHistory" ("PatientId");

COMMENT ON TABLE "NoShowHistory" IS 'Patient no-show risk metrics for FR-023';

-- ============================================================================
-- SUMMARY & VALIDATION QUERIES
-- ============================================================================

-- List all created tables
DO $$
DECLARE
    table_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO table_count 
    FROM information_schema.tables 
    WHERE table_schema = 'public' 
    AND table_type = 'BASE TABLE';
    
    RAISE NOTICE '========================================';
    RAISE NOTICE 'Schema Creation Complete!';
    RAISE NOTICE 'Total Tables Created: %', table_count;
    RAISE NOTICE '========================================';
    RAISE NOTICE 'EP-DATA-I Tables:';
    RAISE NOTICE '  - Users';
    RAISE NOTICE '  - Patients';
    RAISE NOTICE '  - Providers';
    RAISE NOTICE '  - Appointments';
    RAISE NOTICE '  - TimeSlots';
    RAISE NOTICE '  - ClinicalDocuments';
    RAISE NOTICE '  - ExtractedClinicalData';
    RAISE NOTICE '  - PatientProfiles';
    RAISE NOTICE '  - AuditLogs';
    RAISE NOTICE '';
    RAISE NOTICE 'EP-DATA-II Tables:';
    RAISE NOTICE '  - WaitlistEntries';
    RAISE NOTICE '  - IntakeRecords';
    RAISE NOTICE '  - MedicalCodes';
    RAISE NOTICE '  - Notifications';
    RAISE NOTICE '  - InsuranceRecords';
    RAISE NOTICE '  - NoShowHistory';
    RAISE NOTICE '========================================';
END $$;

-- Validation: List all tables with row counts
SELECT 
    schemaname,
    tablename,
    pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) AS size
FROM pg_tables
WHERE schemaname = 'public'
ORDER BY tablename;
