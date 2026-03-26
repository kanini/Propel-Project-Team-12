using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientAccess.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExtractionWorkflowFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add fields to ClinicalDocuments table
            migrationBuilder.AddColumn<bool>(
                name: "RequiresManualReview",
                table: "ClinicalDocuments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ProcessingNotes",
                table: "ClinicalDocuments",
                type: "text",
                nullable: true);

            // Add fields to ExtractedClinicalData table
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequiresManualReview",
                table: "ClinicalDocuments");

            migrationBuilder.DropColumn(
                name: "ProcessingNotes",
                table: "ClinicalDocuments");

            migrationBuilder.DropColumn(
                name: "ExtractedAt",
                table: "ExtractedClinicalData");

            migrationBuilder.DropColumn(
                name: "StructuredData",
                table: "ExtractedClinicalData");
        }
    }
}
