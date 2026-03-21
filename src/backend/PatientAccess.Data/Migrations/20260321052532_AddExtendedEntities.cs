using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientAccess.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExtendedEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop pre-existing EP-DATA-II tables from reference SQL script
            migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS ""NoShowHistory"" CASCADE;
                DROP TABLE IF EXISTS ""InsuranceRecords"" CASCADE;
                DROP TABLE IF EXISTS ""Notifications"" CASCADE;
                DROP TABLE IF EXISTS ""MedicalCodes"" CASCADE;
                DROP TABLE IF EXISTS ""IntakeRecords"" CASCADE;
                DROP TABLE IF EXISTS ""WaitlistEntries"" CASCADE;
            ");

            migrationBuilder.CreateTable(
                name: "InsuranceRecords",
                columns: table => new
                {
                    InsuranceRecordId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ProviderName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AcceptedIdPattern = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CoverageType = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsuranceRecords", x => x.InsuranceRecordId);
                });

            migrationBuilder.CreateTable(
                name: "MedicalCodes",
                columns: table => new
                {
                    MedicalCodeId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ExtractedDataId = table.Column<Guid>(type: "uuid", nullable: false),
                    CodeSystem = table.Column<int>(type: "integer", nullable: false),
                    CodeValue = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CodeDescription = table.Column<string>(type: "text", nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    VerificationStatus = table.Column<int>(type: "integer", nullable: false),
                    VerifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalCodes", x => x.MedicalCodeId);
                    table.ForeignKey(
                        name: "FK_MedicalCodes_ExtractedData",
                        column: x => x.ExtractedDataId,
                        principalTable: "ExtractedClinicalData",
                        principalColumn: "ExtractedDataId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicalCodes_VerifiedBy",
                        column: x => x.VerifiedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "NoShowHistory",
                columns: table => new
                {
                    NoShowHistoryId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalAppointments = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    NoShowCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ConfirmationResponseRate = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    AverageLeadTimeHours = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    LastCalculatedRiskScore = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    LastCalculatedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoShowHistory", x => x.NoShowHistoryId);
                    table.ForeignKey(
                        name: "FK_NoShowHistory_Patients",
                        column: x => x.PatientId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    RecipientId = table.Column<Guid>(type: "uuid", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ChannelType = table.Column<int>(type: "integer", nullable: false),
                    TemplateName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ScheduledTime = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    SentTime = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    DeliveryConfirmation = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LastErrorMessage = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_Notifications_Appointments",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "AppointmentId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Notifications_Recipients",
                        column: x => x.RecipientId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WaitlistEntries",
                columns: table => new
                {
                    WaitlistEntryId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreferredDateStart = table.Column<DateOnly>(type: "date", nullable: false),
                    PreferredDateEnd = table.Column<DateOnly>(type: "date", nullable: false),
                    PreferredTimeOfDay = table.Column<int>(type: "integer", nullable: true),
                    NotificationPreference = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WaitlistEntries", x => x.WaitlistEntryId);
                    table.ForeignKey(
                        name: "FK_WaitlistEntries_Patients",
                        column: x => x.PatientId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WaitlistEntries_Providers",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "ProviderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IntakeRecords",
                columns: table => new
                {
                    IntakeRecordId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    IntakeMode = table.Column<int>(type: "integer", nullable: false),
                    ChiefComplaint = table.Column<string>(type: "text", nullable: true),
                    SymptomHistory = table.Column<string>(type: "jsonb", nullable: true),
                    CurrentMedications = table.Column<string>(type: "jsonb", nullable: true),
                    KnownAllergies = table.Column<string>(type: "jsonb", nullable: true),
                    MedicalHistory = table.Column<string>(type: "jsonb", nullable: true),
                    InsuranceValidationStatus = table.Column<int>(type: "integer", nullable: true),
                    ValidatedInsuranceRecordId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntakeRecords", x => x.IntakeRecordId);
                    table.ForeignKey(
                        name: "FK_IntakeRecords_Appointments",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "AppointmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IntakeRecords_InsuranceRecords",
                        column: x => x.ValidatedInsuranceRecordId,
                        principalTable: "InsuranceRecords",
                        principalColumn: "InsuranceRecordId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_IntakeRecords_Patients",
                        column: x => x.PatientId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceRecords_IsActive",
                table: "InsuranceRecords",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceRecords_ProviderName_IsActive",
                table: "InsuranceRecords",
                columns: new[] { "ProviderName", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_IntakeRecords_AppointmentId",
                table: "IntakeRecords",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IntakeRecords_IsCompleted",
                table: "IntakeRecords",
                column: "IsCompleted");

            migrationBuilder.CreateIndex(
                name: "IX_IntakeRecords_PatientId",
                table: "IntakeRecords",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_IntakeRecords_ValidatedInsuranceRecordId",
                table: "IntakeRecords",
                column: "ValidatedInsuranceRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalCodes_CodeSystem",
                table: "MedicalCodes",
                column: "CodeSystem");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalCodes_CodeValue",
                table: "MedicalCodes",
                column: "CodeValue");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalCodes_ExtractedData_System_Value",
                table: "MedicalCodes",
                columns: new[] { "ExtractedDataId", "CodeSystem", "CodeValue" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicalCodes_ExtractedDataId",
                table: "MedicalCodes",
                column: "ExtractedDataId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalCodes_VerificationStatus",
                table: "MedicalCodes",
                column: "VerificationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalCodes_VerifiedBy",
                table: "MedicalCodes",
                column: "VerifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_NoShowHistory_PatientId",
                table: "NoShowHistory",
                column: "PatientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_AppointmentId",
                table: "Notifications",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientId",
                table: "Notifications",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ScheduledTime",
                table: "Notifications",
                column: "ScheduledTime");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Status",
                table: "Notifications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WaitlistEntries_Patient_Provider_Date",
                table: "WaitlistEntries",
                columns: new[] { "PatientId", "ProviderId", "PreferredDateStart" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WaitlistEntries_PatientId",
                table: "WaitlistEntries",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_WaitlistEntries_Priority_CreatedAt",
                table: "WaitlistEntries",
                columns: new[] { "Priority", "CreatedAt" },
                descending: new[] { true, false });

            migrationBuilder.CreateIndex(
                name: "IX_WaitlistEntries_ProviderId",
                table: "WaitlistEntries",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_WaitlistEntries_Status",
                table: "WaitlistEntries",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntakeRecords");

            migrationBuilder.DropTable(
                name: "MedicalCodes");

            migrationBuilder.DropTable(
                name: "NoShowHistory");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "WaitlistEntries");

            migrationBuilder.DropTable(
                name: "InsuranceRecords");
        }
    }
}
