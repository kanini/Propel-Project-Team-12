using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PatientAccess.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemSettingsAndReminderSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExtractedAt",
                table: "ExtractedClinicalData",
                type: "timestamptz",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<string>(
                name: "StructuredData",
                table: "ExtractedClinicalData",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresManualReview",
                table: "ClinicalDocuments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    SystemSettingId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Value = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.SystemSettingId);
                });

            migrationBuilder.InsertData(
                table: "SystemSettings",
                columns: new[] { "SystemSettingId", "CreatedAt", "Description", "Key", "UpdatedAt", "Value" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2026, 3, 26, 0, 0, 0, 0, DateTimeKind.Utc), "Reminder intervals in hours before appointment (e.g., 48h, 24h, 2h)", "Reminder.Intervals", null, "[48, 24, 2]" },
                    { new Guid("00000000-0000-0000-0000-000000000002"), new DateTime(2026, 3, 26, 0, 0, 0, 0, DateTimeKind.Utc), "Enable SMS reminders via Twilio", "Reminder.SmsEnabled", null, "true" },
                    { new Guid("00000000-0000-0000-0000-000000000003"), new DateTime(2026, 3, 26, 0, 0, 0, 0, DateTimeKind.Utc), "Enable email reminders via SendGrid", "Reminder.EmailEnabled", null, "true" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Status_ScheduledTime",
                table: "Notifications",
                columns: new[] { "Status", "ScheduledTime" });

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_Key",
                table: "SystemSettings",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_Status_ScheduledTime",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ExtractedAt",
                table: "ExtractedClinicalData");

            migrationBuilder.DropColumn(
                name: "StructuredData",
                table: "ExtractedClinicalData");

            migrationBuilder.DropColumn(
                name: "RequiresManualReview",
                table: "ClinicalDocuments");
        }
    }
}
