START TRANSACTION;

ALTER TABLE "ClinicalDocuments" ADD "UploadedBy" uuid;

CREATE INDEX "IX_ClinicalDocuments_UploadedBy" ON "ClinicalDocuments" ("UploadedBy");

ALTER TABLE "ClinicalDocuments" ADD CONSTRAINT "FK_ClinicalDocuments_UploadedBy" FOREIGN KEY ("UploadedBy") REFERENCES "Users" ("UserId") ON DELETE SET NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260324064122_AddUploadedByToClinicalDocument', '8.0.0');

COMMIT;

START TRANSACTION;

ALTER TABLE "ExtractedClinicalData" ADD "ExtractedAt" timestamptz NOT NULL DEFAULT (NOW());

ALTER TABLE "ExtractedClinicalData" ADD "StructuredData" jsonb;

ALTER TABLE "ClinicalDocuments" ADD "RequiresManualReview" boolean NOT NULL DEFAULT FALSE;

CREATE TABLE "SystemSettings" (
    "SystemSettingId" uuid NOT NULL DEFAULT (gen_random_uuid()),
    "Key" character varying(200) NOT NULL,
    "Value" character varying(2000) NOT NULL,
    "Description" text,
    "CreatedAt" timestamptz NOT NULL DEFAULT (NOW()),
    "UpdatedAt" timestamptz,
    CONSTRAINT "PK_SystemSettings" PRIMARY KEY ("SystemSettingId")
);

INSERT INTO "SystemSettings" ("SystemSettingId", "CreatedAt", "Description", "Key", "UpdatedAt", "Value")
VALUES ('00000000-0000-0000-0000-000000000001', TIMESTAMPTZ '2026-03-26T00:00:00Z', 'Reminder intervals in hours before appointment (e.g., 48h, 24h, 2h)', 'Reminder.Intervals', NULL, '[48, 24, 2]');
INSERT INTO "SystemSettings" ("SystemSettingId", "CreatedAt", "Description", "Key", "UpdatedAt", "Value")
VALUES ('00000000-0000-0000-0000-000000000002', TIMESTAMPTZ '2026-03-26T00:00:00Z', 'Enable SMS reminders via Twilio', 'Reminder.SmsEnabled', NULL, 'true');
INSERT INTO "SystemSettings" ("SystemSettingId", "CreatedAt", "Description", "Key", "UpdatedAt", "Value")
VALUES ('00000000-0000-0000-0000-000000000003', TIMESTAMPTZ '2026-03-26T00:00:00Z', 'Enable email reminders via SendGrid', 'Reminder.EmailEnabled', NULL, 'true');

CREATE INDEX "IX_Notifications_Status_ScheduledTime" ON "Notifications" ("Status", "ScheduledTime");

CREATE UNIQUE INDEX "IX_SystemSettings_Key" ON "SystemSettings" ("Key");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260326120237_AddSystemSettingsAndReminderSupport', '8.0.0');

COMMIT;

START TRANSACTION;

ALTER TABLE "WaitlistEntries" ADD "NotifiedAt" timestamptz;

ALTER TABLE "WaitlistEntries" ADD "NotifiedSlotId" uuid;

ALTER TABLE "WaitlistEntries" ADD "ResponseDeadline" timestamptz;

ALTER TABLE "WaitlistEntries" ADD "ResponseToken" character varying(64);

CREATE INDEX "IX_WaitlistEntries_NotifiedSlotId" ON "WaitlistEntries" ("NotifiedSlotId");

CREATE UNIQUE INDEX "IX_WaitlistEntries_ResponseToken" ON "WaitlistEntries" ("ResponseToken") WHERE "ResponseToken" IS NOT NULL;

CREATE INDEX "IX_WaitlistEntries_Status_ResponseDeadline" ON "WaitlistEntries" ("Status", "ResponseDeadline") WHERE "Status" = 2;

ALTER TABLE "WaitlistEntries" ADD CONSTRAINT "FK_WaitlistEntries_NotifiedSlot" FOREIGN KEY ("NotifiedSlotId") REFERENCES "TimeSlots" ("TimeSlotId") ON DELETE SET NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260326173132_AddWaitlistNotificationFields', '8.0.0');

COMMIT;

