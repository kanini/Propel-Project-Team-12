using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PatientAccess.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientProfileAggregation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add missing columns to existing tables
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

            // Create PatientProfiles table
            migrationBuilder.CreateTable(
                name: "PatientProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastAggregatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    TotalDocumentsProcessed = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    HasUnresolvedConflicts = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ProfileCompleteness = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 0m),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientProfiles_Patients",
                        column: x => x.PatientId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.CheckConstraint("CK_PatientProfiles_ProfileCompleteness", "\"ProfileCompleteness\" >= 0 AND \"ProfileCompleteness\" <= 100");
                });

            // Create ConsolidatedConditions table
            migrationBuilder.CreateTable(
                name: "ConsolidatedConditions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    PatientProfileId = table.Column<int>(type: "integer", nullable: false),
                    ConditionName = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    ICD10Code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    DiagnosisDate = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    Status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Severity = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    SourceDocumentIds = table.Column<List<Guid>>(type: "jsonb", nullable: false),
                    SourceDataIds = table.Column<List<Guid>>(type: "jsonb", nullable: false),
                    IsDuplicate = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DuplicateCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    FirstRecordedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsolidatedConditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsolidatedConditions_PatientProfiles",
                        column: x => x.PatientProfileId,
                        principalTable: "PatientProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create ConsolidatedMedications table
            migrationBuilder.CreateTable(
                name: "ConsolidatedMedications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    PatientProfileId = table.Column<int>(type: "integer", nullable: false),
                    DrugName = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    Dosage = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Frequency = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    RouteOfAdministration = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    Status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    SourceDocumentIds = table.Column<List<Guid>>(type: "jsonb", nullable: false),
                    SourceDataIds = table.Column<List<Guid>>(type: "jsonb", nullable: false),
                    IsDuplicate = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DuplicateCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    HasConflict = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    FirstRecordedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsolidatedMedications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsolidatedMedications_PatientProfiles",
                        column: x => x.PatientProfileId,
                        principalTable: "PatientProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create ConsolidatedAllergies table
            migrationBuilder.CreateTable(
                name: "ConsolidatedAllergies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    PatientProfileId = table.Column<int>(type: "integer", nullable: false),
                    AllergenName = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    Reaction = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    Severity = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    OnsetDate = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    Status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    SourceDocumentIds = table.Column<List<Guid>>(type: "jsonb", nullable: false),
                    SourceDataIds = table.Column<List<Guid>>(type: "jsonb", nullable: false),
                    IsDuplicate = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DuplicateCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    FirstRecordedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsolidatedAllergies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsolidatedAllergies_PatientProfiles",
                        column: x => x.PatientProfileId,
                        principalTable: "PatientProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create VitalTrends table
            migrationBuilder.CreateTable(
                name: "VitalTrends",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    PatientProfileId = table.Column<int>(type: "integer", nullable: false),
                    VitalType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Unit = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    SourceDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceDataId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VitalTrends", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VitalTrends_PatientProfiles",
                        column: x => x.PatientProfileId,
                        principalTable: "PatientProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create ConsolidatedEncounters table
            migrationBuilder.CreateTable(
                name: "ConsolidatedEncounters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    PatientProfileId = table.Column<int>(type: "integer", nullable: false),
                    EncounterDate = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    EncounterType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Provider = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    Facility = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    ChiefComplaint = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    SourceDocumentIds = table.Column<List<Guid>>(type: "jsonb", nullable: false),
                    SourceDataIds = table.Column<List<Guid>>(type: "jsonb", nullable: false),
                    IsDuplicate = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DuplicateCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsolidatedEncounters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsolidatedEncounters_PatientProfiles",
                        column: x => x.PatientProfileId,
                        principalTable: "PatientProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create DataConflicts table
            migrationBuilder.CreateTable(
                name: "DataConflicts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    PatientProfileId = table.Column<int>(type: "integer", nullable: false),
                    ConflictType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                    SourceDataIds = table.Column<List<Guid>>(type: "jsonb", nullable: false),
                    ResolutionStatus = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    ResolvedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataConflicts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataConflicts_PatientProfiles",
                        column: x => x.PatientProfileId,
                        principalTable: "PatientProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DataConflicts_ResolvedBy",
                        column: x => x.ResolvedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            // Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_PatientProfiles_PatientId",
                table: "PatientProfiles",
                column: "PatientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConsolidatedConditions_PatientProfileId",
                table: "ConsolidatedConditions",
                column: "PatientProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsolidatedConditions_ConditionName",
                table: "ConsolidatedConditions",
                column: "ConditionName");

            migrationBuilder.CreateIndex(
                name: "IX_ConsolidatedConditions_ICD10Code",
                table: "ConsolidatedConditions",
                column: "ICD10Code");

            migrationBuilder.CreateIndex(
                name: "IX_ConsolidatedConditions_Status",
                table: "ConsolidatedConditions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ConsolidatedMedications_PatientProfileId",
                table: "ConsolidatedMedications",
                column: "PatientProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsolidatedMedications_DrugName",
                table: "ConsolidatedMedications",
                column: "DrugName");

            migrationBuilder.CreateIndex(
                name: "IX_ConsolidatedMedications_Status",
                table: "ConsolidatedMedications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ConsolidatedMedications_HasConflict",
                table: "ConsolidatedMedications",
                column: "HasConflict");

            migrationBuilder.CreateIndex(
                name: "IX_ConsolidatedAllergies_PatientProfileId",
                table: "ConsolidatedAllergies",
                column: "PatientProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsolidatedAllergies_AllergenName",
                table: "ConsolidatedAllergies",
                column: "AllergenName");

            migrationBuilder.CreateIndex(
                name: "IX_ConsolidatedAllergies_Severity",
                table: "ConsolidatedAllergies",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_VitalTrends_PatientProfile_VitalType_RecordedAt",
                table: "VitalTrends",
                columns: new[] { "PatientProfileId", "VitalType", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsolidatedEncounters_PatientProfileId",
                table: "ConsolidatedEncounters",
                column: "PatientProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsolidatedEncounters_EncounterDate",
                table: "ConsolidatedEncounters",
                column: "EncounterDate");

            migrationBuilder.CreateIndex(
                name: "IX_DataConflicts_PatientProfileId",
                table: "DataConflicts",
                column: "PatientProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_DataConflicts_ResolutionStatus",
                table: "DataConflicts",
                column: "ResolutionStatus");

            migrationBuilder.CreateIndex(
                name: "IX_DataConflicts_EntityType",
                table: "DataConflicts",
                column: "EntityType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop tables in reverse order
            migrationBuilder.DropTable(name: "DataConflicts");
            migrationBuilder.DropTable(name: "ConsolidatedEncounters");
            migrationBuilder.DropTable(name: "VitalTrends");
            migrationBuilder.DropTable(name: "ConsolidatedAllergies");
            migrationBuilder.DropTable(name: "ConsolidatedMedications");
            migrationBuilder.DropTable(name: "ConsolidatedConditions");
            migrationBuilder.DropTable(name: "PatientProfiles");

            // Remove columns from existing tables
            migrationBuilder.DropColumn(name: "ExtractedAt", table: "ExtractedClinicalData");
            migrationBuilder.DropColumn(name: "StructuredData", table: "ExtractedClinicalData");
            migrationBuilder.DropColumn(name: "RequiresManualReview", table: "ClinicalDocuments");
        }
    }
}
