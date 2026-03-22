-- ============================================================================
-- Migration: 004_add_preferred_slot_swap
-- Date: 2026-03-22
-- Purpose: Add PreferredSlotId and TimeSlotId columns to Appointments table for US_026 (Dynamic Preferred Slot Swap)
-- Target: Support FR-010 (Dynamic preferred slot swap)
-- ============================================================================
-- Description:
--   Extends Appointments table with:
--   - TimeSlotId: Foreign key to TimeSlots table (replaces implicit relationship)
--   - PreferredSlotId: Optional foreign key for dynamic slot swap feature
--   Adds indexes for optimal swap detection query performance
--
-- Dependencies:
--   - Existing Appointments table structure
--   - Existing TimeSlots table structure
--
-- Rollback:
--   - DROP CONSTRAINT "FK_Appointments_PreferredSlot";
--   - DROP CONSTRAINT "FK_Appointments_TimeSlot";
--   - DROP INDEX "IX_Appointments_TimeSlotId";
--   - DROP INDEX "IX_Appointments_PreferredSlotId";
--   - ALTER TABLE "Appointments" DROP COLUMN "TimeSlotId";
--   - ALTER TABLE "Appointments" DROP COLUMN "PreferredSlotId";
-- ============================================================================

BEGIN;

-- Add TimeSlotId column (NOT NULL) - links appointment to booked time slot
-- This makes the relationship explicit for query optimization
ALTER TABLE "Appointments" 
ADD COLUMN IF NOT EXISTS "TimeSlotId" UUID NOT NULL;

-- Add PreferredSlotId column (NULLABLE) - optional preferred slot for swap
-- NULL means no swap preference set for this appointment
ALTER TABLE "Appointments" 
ADD COLUMN IF NOT EXISTS "PreferredSlotId" UUID;

-- Add foreign key constraint for TimeSlotId
ALTER TABLE "Appointments"
ADD CONSTRAINT "FK_Appointments_TimeSlot" 
FOREIGN KEY ("TimeSlotId") 
REFERENCES "TimeSlots"("TimeSlotId") 
ON DELETE RESTRICT;

-- Add foreign key constraint for PreferredSlotId
ALTER TABLE "Appointments"
ADD CONSTRAINT "FK_Appointments_PreferredSlot" 
FOREIGN KEY ("PreferredSlotId") 
REFERENCES "TimeSlots"("TimeSlotId") 
ON DELETE SET NULL;

-- Index for TimeSlotId lookups (reverse navigation from appointment to slot)
CREATE INDEX IF NOT EXISTS "IX_Appointments_TimeSlotId" 
ON "Appointments" ("TimeSlotId");

-- Composite index for swap detection queries
-- Supports: SELECT * FROM "Appointments" WHERE "PreferredSlotId" = ? AND "Status" = 'Scheduled' ORDER BY "CreatedAt"
CREATE INDEX IF NOT EXISTS "IX_Appointments_PreferredSlotId_Status_CreatedAt" 
ON "Appointments" ("PreferredSlotId", "Status", "CreatedAt") 
WHERE "PreferredSlotId" IS NOT NULL;

COMMENT ON COLUMN "Appointments"."TimeSlotId" IS 'Foreign key to TimeSlots table - the booked time slot for this appointment';
COMMENT ON COLUMN "Appointments"."PreferredSlotId" IS 'Optional foreign key to TimeSlots table - preferred slot for dynamic swap (FR-010). NULL means no swap preference.';
COMMENT ON INDEX "IX_Appointments_PreferredSlotId_Status_CreatedAt" IS 'Partial index for swap detection - only indexes appointments with swap preferences. Supports FIFO swap execution.';

COMMIT;
