-- ============================================================================
-- Migration: 003_add_appointment_indexes
-- Date: 2026-03-22
-- Purpose: Add composite and expression indexes for appointment booking performance optimization
-- Target: Meet NFR-001 (500ms API response at P95)
-- ============================================================================
-- Description:
--   Adds optimized indexes to support high-performance availability queries and appointment
--   lookups. Many basic indexes already exist from EF Core configuration; this migration
--   adds specialized composite and expression indexes for complex query patterns.
--
-- Existing Indexes (from EF Core configuration):
--   - IX_TimeSlots_ProviderId (B-tree)
--   - IX_TimeSlots_StartTime (B-tree)
--   - IX_TimeSlots_IsBooked (B-tree)
--   - IX_Appointments_PatientId (B-tree)
--   - IX_Appointments_ProviderId (B-tree)
--   - IX_Appointments_TimeSlotId (B-tree)
--   - IX_Appointments_ScheduledDateTime (B-tree)
--   - IX_Appointments_Status (B-tree)
--   - IX_Appointments_ConfirmationNumber (B-tree, UNIQUE)
--
-- New Indexes (this migration):
--   - IX_TimeSlots_ProviderId_StartTime_IsBooked (Composite B-tree)
--     Purpose: Optimize monthly/daily availability queries with provider, date range, and booking status filters
--     Query pattern: WHERE ProviderId = ? AND StartTime BETWEEN ? AND ? [AND IsBooked = false]
--
--   - IX_TimeSlots_StartTime_Date (Expression B-tree)
--     Purpose: Optimize daily availability queries using DATE() function
--     Query pattern: WHERE ProviderId = ? AND DATE(StartTime) = ?
--
--   - IX_Appointments_PatientId_Status (Composite B-tree)
--     Purpose: Optimize "My Appointments" patient dashboard queries
--     Query pattern: WHERE PatientId = ? AND Status IN ('Scheduled', 'Confirmed', ...)
-- ============================================================================

BEGIN;

-- Composite index for availability queries (ProviderId + StartTime + IsBooked)
-- Replaces need for separate index lookups when filtering by all three columns
-- Supports query: SELECT * FROM "TimeSlots" WHERE "ProviderId" = ? AND "StartTime" BETWEEN ? AND ? AND "IsBooked" = false
CREATE INDEX IF NOT EXISTS "IX_TimeSlots_ProviderId_StartTime_IsBooked" 
ON "TimeSlots" ("ProviderId", "StartTime", "IsBooked");

COMMENT ON INDEX "IX_TimeSlots_ProviderId_StartTime_IsBooked" IS 
'Composite index for monthly/daily availability queries with provider, date range, and booking status filters. Targets <50ms query time for 1000+ time slots.';

-- Expression index for DATE-based filtering (daily availability)
-- Enables index usage for DATE(StartTime) = ? queries instead of sequential scan
-- Supports query: SELECT * FROM "TimeSlots" WHERE "ProviderId" = ? AND DATE("StartTime") = ?
CREATE INDEX IF NOT EXISTS "IX_TimeSlots_StartTime_Date" 
ON "TimeSlots" (DATE("StartTime"));

COMMENT ON INDEX "IX_TimeSlots_StartTime_Date" IS 
'Expression index for daily availability queries using DATE() function. Enables index scan instead of sequential scan for date-specific lookups.';

-- Composite index for patient appointment queries (PatientId + Status)
-- Optimizes "My Appointments" dashboard and appointment management queries
-- Supports query: SELECT * FROM "Appointments" WHERE "PatientId" = ? AND "Status" = 'Scheduled'
CREATE INDEX IF NOT EXISTS "IX_Appointments_PatientId_Status" 
ON "Appointments" ("PatientId", "Status");

COMMENT ON INDEX "IX_Appointments_PatientId_Status" IS 
'Composite index for patient appointment queries filtering by patient and status. Supports appointment dashboard and management features with <30ms query time for 50+ appointments.';

-- Analyze tables to update query planner statistics after index creation
ANALYZE "TimeSlots";
ANALYZE "Appointments";

COMMIT;

-- ============================================================================
-- Rollback Instructions (for development/testing only)
-- Execute these commands to remove the indexes created by this migration
-- ============================================================================
-- BEGIN;
-- DROP INDEX IF EXISTS "IX_TimeSlots_ProviderId_StartTime_IsBooked";
-- DROP INDEX IF EXISTS "IX_TimeSlots_StartTime_Date";
-- DROP INDEX IF EXISTS "IX_Appointments_PatientId_Status";
-- COMMIT;

-- ============================================================================
-- Performance Verification Queries
-- Run these EXPLAIN ANALYZE queries to verify index usage after migration
-- ============================================================================

-- Test monthly availability query (should use IX_TimeSlots_ProviderId_StartTime_IsBooked)
-- EXPLAIN ANALYZE SELECT * FROM "TimeSlots" 
-- WHERE "ProviderId" = '00000000-0000-0000-0000-000000000001' 
-- AND "StartTime" BETWEEN '2024-01-01' AND '2024-01-31' 
-- AND "IsBooked" = false
-- ORDER BY "StartTime";

-- Test daily availability with DATE function (should use IX_TimeSlots_StartTime_Date)
-- EXPLAIN ANALYZE SELECT * FROM "TimeSlots" 
-- WHERE "ProviderId" = '00000000-0000-0000-0000-000000000001' 
-- AND DATE("StartTime") = '2024-01-15'
-- ORDER BY "StartTime";

-- Test patient appointments query (should use IX_Appointments_PatientId_Status)
-- EXPLAIN ANALYZE SELECT * FROM "Appointments" 
-- WHERE "PatientId" = '00000000-0000-0000-0000-000000000001' 
-- AND "Status" IN (1, 2)  -- Scheduled = 1, Confirmed = 2
-- ORDER BY "ScheduledDateTime" DESC;

-- ============================================================================
-- Index Usage Monitoring
-- Query pg_stat_user_indexes to track index performance over time
-- ============================================================================
-- SELECT 
--     schemaname,
--     tablename,
--     indexname,
--     idx_scan AS index_scans,
--     idx_tup_read AS tuples_read,
--     idx_tup_fetch AS tuples_fetched
-- FROM pg_stat_user_indexes
-- WHERE schemaname = 'public' 
-- AND indexname IN (
--     'IX_TimeSlots_ProviderId_StartTime_IsBooked',
--     'IX_TimeSlots_StartTime_Date',
--     'IX_Appointments_PatientId_Status'
-- )
-- ORDER BY tablename, indexname;

-- ============================================================================
-- Index Maintenance Recommendations
-- ============================================================================
-- 1. Run VACUUM ANALYZE periodically (weekly) to update query planner statistics
-- 2. Monitor index bloat with pg_stat_user_indexes (idx_scan should increase over time)
-- 3. After bulk data imports, run ANALYZE on affected tables
-- 4. Consider REINDEX if query performance degrades significantly (rare with B-tree indexes)
-- 5. Monitor disk space usage - indexes increase storage requirements by ~30-40%
-- ============================================================================
