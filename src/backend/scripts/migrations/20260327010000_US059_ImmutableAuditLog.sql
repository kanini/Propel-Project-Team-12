-- ============================================================================
-- US_059: Immutable Audit Log Enhancements
-- ============================================================================
-- Description: Adds immutability enforcement, archive table, and performance
--              optimizations for HIPAA-compliant audit logging (DR-005, DR-007)
-- Requirements:
--   - AC1: Result tracking (Success/Failure/PartialSuccess)
--   - AC2: Immutability via PostgreSQL RLS policies and triggers
--   - AC3: Performance indexes for <2s query times (NFR-007)
--   - Edge Case: 7-year retention with AuditLogArchive table
-- ============================================================================

BEGIN;

-- ============================================================================
-- 1. Add Result column to AuditLogs (AC1 - US_059)
-- ============================================================================

ALTER TABLE "AuditLogs"
ADD COLUMN IF NOT EXISTS "Result" VARCHAR(50) NOT NULL DEFAULT 'Success';

COMMENT ON COLUMN "AuditLogs"."Result" IS 'Result of the action: Success, Failure, PartialSuccess (AC1 - US_059)';

-- ============================================================================
-- 2. Create AuditLogArchive table for 7-year retention (DR-007, Edge Case)
-- ============================================================================

CREATE TABLE IF NOT EXISTS "AuditLogArchives" (
    "AuditLogId" UUID NOT NULL PRIMARY KEY,
    "UserId" UUID NULL,
    "Timestamp" TIMESTAMPTZ NOT NULL,
    "ActionType" VARCHAR(50) NOT NULL,
    "ResourceType" VARCHAR(100) NOT NULL,
    "ResourceId" UUID NULL,
    "ActionDetails" JSONB NOT NULL DEFAULT '{}'::jsonb,
    "IpAddress" VARCHAR(45) NULL,
    "UserAgent" VARCHAR(500) NULL,
    "Result" VARCHAR(50) NOT NULL DEFAULT 'Success',
    "ArchivedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "ArchiveReason" VARCHAR(200) NOT NULL DEFAULT 'Retention policy',
    
    -- Foreign key to Users (preserve audit trail)
    CONSTRAINT "FK_AuditLogArchives_Users" FOREIGN KEY ("UserId")
        REFERENCES "Users"("UserId")
        ON DELETE RESTRICT
);

COMMENT ON TABLE "AuditLogArchives" IS 'Archive table for audit logs older than retention period (DR-007, US_059 Edge Case). Supports 7-year HIPAA retention without deleting data.';

-- Indexes for AuditLogArchive queries
CREATE INDEX IF NOT EXISTS "IX_AuditLogArchives_UserId" ON "AuditLogArchives"("UserId");
CREATE INDEX IF NOT EXISTS "IX_AuditLogArchives_Timestamp" ON "AuditLogArchives"("Timestamp" DESC);
CREATE INDEX IF NOT EXISTS "IX_AuditLogArchives_ArchivedAt" ON "AuditLogArchives"("ArchivedAt" DESC);

-- ============================================================================
-- 3. Create composite indexes for query performance (AC3, NFR-007: <2s)
-- ============================================================================

-- Composite index for date range queries (most common)
CREATE INDEX IF NOT EXISTS "IX_AuditLogs_Timestamp_UserId" 
ON "AuditLogs"("Timestamp" DESC, "UserId");

-- Composite index for user+action queries
CREATE INDEX IF NOT EXISTS "IX_AuditLogs_UserId_ActionType_Timestamp" 
ON "AuditLogs"("UserId", "ActionType", "Timestamp" DESC);

-- Composite index for resource queries
CREATE INDEX IF NOT EXISTS "IX_AuditLogs_ResourceType_ResourceId_Timestamp" 
ON "AuditLogs"("ResourceType", "ResourceId", "Timestamp" DESC);

-- Index for result filtering (Success/Failure queries)
CREATE INDEX IF NOT EXISTS "IX_AuditLogs_Result_Timestamp"
ON "AuditLogs"("Result", "Timestamp" DESC);

-- ============================================================================
-- 4. Enable Row Level Security (RLS) for immutability enforcement (AC2)
-- ============================================================================

-- Enable RLS on AuditLogs table
ALTER TABLE "AuditLogs" ENABLE ROW LEVEL SECURITY;

-- Policy: Allow SELECT for all authenticated users with proper roles
-- (Admin role can read all audit logs, Staff can read their own)
CREATE POLICY IF NOT EXISTS "audit_logs_select_policy"
ON "AuditLogs"
FOR SELECT
USING (true); -- Application-level auth via ASP.NET Core authorization

-- Policy: Allow INSERT for all (append-only)
CREATE POLICY IF NOT EXISTS "audit_logs_insert_policy"
ON "AuditLogs" 
FOR INSERT
WITH CHECK (true); -- All inserts allowed (application handles validation)

-- Policy: DENY UPDATE - immutability enforcement (AC2)
CREATE POLICY IF NOT EXISTS "audit_logs_no_updates"
ON "AuditLogs"
FOR UPDATE
USING (false); -- No updates allowed under any circumstances

-- Policy: DENY DELETE - immutability enforcement (AC2)
CREATE POLICY IF NOT EXISTS "audit_logs_no_deletes"
ON "AuditLogs"
FOR DELETE
USING (false); -- No deletes allowed under any circumstances

-- Same RLS policies for AuditLogArchive
ALTER TABLE "AuditLogArchives" ENABLE ROW LEVEL SECURITY;

CREATE POLICY IF NOT EXISTS "audit_archive_select_policy"
ON "AuditLogArchives"
FOR SELECT
USING (true);

CREATE POLICY IF NOT EXISTS "audit_archive_insert_policy"
ON "AuditLogArchives"
FOR INSERT
WITH CHECK (true);

CREATE POLICY IF NOT EXISTS "audit_archive_no_updates"
ON "AuditLogArchives"
FOR UPDATE
USING (false);

CREATE POLICY IF NOT EXISTS "audit_archive_no_deletes"
ON "AuditLogArchives"
FOR DELETE
USING (false);

-- ============================================================================
-- 5. Create trigger function to block UPDATE/DELETE (double enforcement - AC2)
-- ============================================================================

CREATE OR REPLACE FUNCTION prevent_audit_log_modification()
RETURNS TRIGGER AS $$
BEGIN
    RAISE EXCEPTION 'Audit logs are immutable. UPDATE and DELETE operations are not permitted (AC2 - US_059).';
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

-- Drop existing triggers if they exist (idempotent)
DROP TRIGGER IF EXISTS prevent_audit_log_updates ON "AuditLogs";
DROP TRIGGER IF EXISTS prevent_audit_log_deletes ON "AuditLogs";
DROP TRIGGER IF EXISTS prevent_audit_archive_updates ON "AuditLogArchives";
DROP TRIGGER IF EXISTS prevent_audit_archive_deletes ON "AuditLogArchives";

-- Create triggers for AuditLogs
CREATE TRIGGER prevent_audit_log_updates
    BEFORE UPDATE ON "AuditLogs"
    FOR EACH ROW
    EXECUTE FUNCTION prevent_audit_log_modification();

CREATE TRIGGER prevent_audit_log_deletes
    BEFORE DELETE ON "AuditLogs"
    FOR EACH ROW
    EXECUTE FUNCTION prevent_audit_log_modification();

-- Create triggers for AuditLogArchives
CREATE TRIGGER prevent_audit_archive_updates
    BEFORE UPDATE ON "AuditLogArchives"
    FOR EACH ROW
    EXECUTE FUNCTION prevent_audit_log_modification();

CREATE TRIGGER prevent_audit_archive_deletes
    BEFORE DELETE ON "AuditLogArchives"
    FOR EACH ROW
    EXECUTE FUNCTION prevent_audit_log_modification();

-- ============================================================================
-- 6. Add function for archiving old audit logs (Edge Case)
-- ============================================================================

CREATE OR REPLACE FUNCTION archive_old_audit_logs(retention_days INT DEFAULT 90)
RETURNS TABLE(archived_count BIGINT) AS $$
DECLARE
    cutoff_date TIMESTAMPTZ;
    rows_archived BIGINT;
BEGIN
    -- Calculate cutoff date
    cutoff_date := NOW() - (retention_days || ' days')::INTERVAL;
    
    -- Insert old logs into archive (temporarily bypass RLS for this operation)
    WITH archived AS (
        INSERT INTO "AuditLogArchives" (
            "AuditLogId", "UserId", "Timestamp", "ActionType", "ResourceType",
            "ResourceId", "ActionDetails", "IpAddress", "UserAgent", "Result",
            "ArchivedAt", "ArchiveReason"
        )
        SELECT 
            "AuditLogId", "UserId", "Timestamp", "ActionType", "ResourceType",
            "ResourceId", "ActionDetails", "IpAddress", "UserAgent", "Result",
            NOW() AS "ArchivedAt",
            'Retention policy: >' || retention_days || ' days' AS "ArchiveReason"
        FROM "AuditLogs"
        WHERE "Timestamp" < cutoff_date
        ON CONFLICT ("AuditLogId") DO NOTHING
        RETURNING 1
    )
    SELECT COUNT(*) INTO rows_archived FROM archived;
    
    -- NOTE: We do NOT delete from AuditLogs here due to immutability constraint
    -- Instead, application can mark logs as archived or use separate cold storage
    
    RETURN QUERY SELECT rows_archived;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION archive_old_audit_logs IS 'Archives audit logs older than retention_days to AuditLogArchives table. Does not delete from AuditLogs per immutability requirement (AC2).';

COMMIT;

-- ============================================================================
-- Verification Queries (for testing)
-- ============================================================================

-- Test immutability (should fail):
-- UPDATE "AuditLogs" SET "ActionType" = 'Test' WHERE "AuditLogId" = '00000000-0000-0000-0000-000000000000';
-- DELETE FROM "AuditLogs" WHERE "AuditLogId" = '00000000-0000-0000-0000-000000000000';

-- Verify RLS policies:
-- SELECT * FROM pg_policies WHERE schemaname = 'public' AND tablename = 'AuditLogs';

-- Verify triggers:
-- SELECT tgname, tgtype, tgenabled FROM pg_trigger WHERE tgrelid = 'AuditLogs'::regclass;

-- Test archive function:
-- SELECT * FROM archive_old_audit_logs(90);
