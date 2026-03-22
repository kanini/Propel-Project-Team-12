-- Migration: AddAppointmentBookingFields
-- Date: 2026-03-22
-- Description: Add TimeSlotId, PreferredSlotId, and ConfirmationNumber to Appointments table
--              Remove AppointmentId from TimeSlots table to reflect new relationship design

-- Step 1: Add new columns to Appointments table
ALTER TABLE "Appointments" 
ADD COLUMN "TimeSlotId" UUID,
ADD COLUMN "PreferredSlotId" UUID,
ADD COLUMN "ConfirmationNumber" VARCHAR(8);

-- Step 2: Create indexes for new columns
CREATE INDEX "IX_Appointments_TimeSlotId" ON "Appointments" ("TimeSlotId");
CREATE UNIQUE INDEX "IX_Appointments_ConfirmationNumber" ON "Appointments" ("ConfirmationNumber");

-- Step 3: Add foreign key constraints
ALTER TABLE "Appointments"
ADD CONSTRAINT "FK_Appointments_TimeSlot" 
FOREIGN KEY ("TimeSlotId") REFERENCES "TimeSlots"("TimeSlotId") 
ON DELETE RESTRICT;

ALTER TABLE "Appointments"
ADD CONSTRAINT "FK_Appointments_PreferredSlot" 
FOREIGN KEY ("PreferredSlotId") REFERENCES "TimeSlots"("TimeSlotId") 
ON DELETE SET NULL;

-- Step 4: Update VisitReason column to have max length constraint
ALTER TABLE "Appointments" 
ALTER COLUMN "VisitReason" TYPE VARCHAR(500);

-- Step 5: Drop old AppointmentId column and constraint from TimeSlots (if exists)
-- Note: This should only be run after ensuring no data depends on this relationship
-- ALTER TABLE "TimeSlots" DROP CONSTRAINT IF EXISTS "FK_TimeSlots_Appointments";
-- ALTER TABLE "TimeSlots" DROP COLUMN IF EXISTS "AppointmentId";

-- Step 6: For existing data migration (if any appointments exist), you would need to:
-- 1. Create TimeSlots for each existing appointment
-- 2. Update Appointments.TimeSlotId to reference the new TimeSlots
-- 3. Generate ConfirmationNumbers for existing appointments
-- Example (only run if there is existing data):
-- UPDATE "Appointments" SET "ConfirmationNumber" = UPPER(SUBSTRING(MD5(RANDOM()::TEXT) FOR 8))
-- WHERE "ConfirmationNumber" IS NULL;

-- Step 7: Make TimeSlotId and ConfirmationNumber required (NOT NULL)
-- This should be done after data migration is complete
-- ALTER TABLE "Appointments" ALTER COLUMN "TimeSlotId" SET NOT NULL;
-- ALTER TABLE "Appointments" ALTER COLUMN "ConfirmationNumber" SET NOT NULL;
