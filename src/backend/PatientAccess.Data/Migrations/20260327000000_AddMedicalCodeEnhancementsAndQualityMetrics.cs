using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientAccess.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicalCodeEnhancementsAndQualityMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add new columns to MedicalCodes table for enhanced code mapping (EP-008-US-051)
            migrationBuilder.AddColumn<string>(
                name: "Rationale",
                table: "MedicalCodes",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Rank",
                table: "MedicalCodes",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<bool>(
                name: "IsTopSuggestion",
                table: "MedicalCodes",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "RetrievedContext",
                table: "MedicalCodes",
                type: "character varying(5000)",
                maxLength: 5000,
                nullable: true);

            // Create index on IsTopSuggestion for filtering top suggestions
            migrationBuilder.CreateIndex(
                name: "IX_MedicalCodes_IsTopSuggestion",
                table: "MedicalCodes",
                column: "IsTopSuggestion");

            // Create QualityMetrics table for AIR-Q01 and AIR-Q03 tracking
            migrationBuilder.CreateTable(
                name: "QualityMetrics",
                columns: table => new
                {
                    QualityMetricId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    MetricType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MetricValue = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    SampleSize = table.Column<int>(type: "integer", nullable: false),
                    MeasurementPeriod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    Target = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityMetrics", x => x.QualityMetricId);
                });

            // Create indexes on QualityMetrics for time-series queries
            migrationBuilder.CreateIndex(
                name: "IX_QualityMetrics_MetricType",
                table: "QualityMetrics",
                column: "MetricType");

            migrationBuilder.CreateIndex(
                name: "IX_QualityMetrics_Period",
                table: "QualityMetrics",
                columns: new[] { "PeriodStart", "PeriodEnd" });

            migrationBuilder.CreateIndex(
                name: "IX_QualityMetrics_Status",
                table: "QualityMetrics",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop QualityMetrics table
            migrationBuilder.DropTable(name: "QualityMetrics");

            // Drop IsTopSuggestion index from MedicalCodes
            migrationBuilder.DropIndex(
                name: "IX_MedicalCodes_IsTopSuggestion",
                table: "MedicalCodes");

            // Drop new columns from MedicalCodes table
            migrationBuilder.DropColumn(
                name: "Rationale",
                table: "MedicalCodes");

            migrationBuilder.DropColumn(
                name: "Rank",
                table: "MedicalCodes");

            migrationBuilder.DropColumn(
                name: "IsTopSuggestion",
                table: "MedicalCodes");

            migrationBuilder.DropColumn(
                name: "RetrievedContext",
                table: "MedicalCodes");
        }
    }
}
