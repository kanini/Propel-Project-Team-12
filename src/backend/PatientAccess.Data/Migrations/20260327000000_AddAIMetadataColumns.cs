using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientAccess.Data.Migrations
{
    /// <inheritdoc />
    /// <summary>
    /// Adds AI metadata tracking columns for US_058 - AI Safety & Operational Guardrails.
    /// Adds: IsAISuggested, RequiresManualReview, ModelVersion, token tracking, cost tracking fields.
    /// Supports: AIR-S01 (human-in-the-loop), AIR-S02 (confidence thresholds), AIR-O05 (cost tracking), NFR-015 (monitoring).
    /// </summary>
    public partial class AddAIMetadataColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ========================================
            // ExtractedClinicalData Table Updates
            // ========================================
            
            // Add IsAISuggested flag (AC1 - human-in-the-loop)
            migrationBuilder.AddColumn<bool>(
                name: "IsAISuggested",
                table: "ExtractedClinicalData",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                comment: "AI-suggested flag for human-in-the-loop verification (AC1 - US_058, AIR-S01)");

            // Add RequiresManualReview flag (AC2 - confidence thresholds)
            migrationBuilder.AddColumn<bool>(
                name: "RequiresManualReview",
                table: "ExtractedClinicalData",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Auto-flagged for mandatory manual review when confidence < threshold (AC2 - US_058, AIR-S02)");

            // Add ModelVersion for traceability (AC5 - monitoring)
            migrationBuilder.AddColumn<string>(
                name: "ModelVersion",
                table: "ExtractedClinicalData",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                comment: "AI model version used for extraction (AC5 - US_058, NFR-015)");

            // Add token tracking fields (AC4 - cost management)
            migrationBuilder.AddColumn<int>(
                name: "PromptTokens",
                table: "ExtractedClinicalData",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Prompt tokens consumed for this extraction (AC4 - US_058, AIR-O05)");

            migrationBuilder.AddColumn<int>(
                name: "CompletionTokens",
                table: "ExtractedClinicalData",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Completion tokens consumed for this extraction (AC4 - US_058, AIR-O05)");

            migrationBuilder.AddColumn<int>(
                name: "TotalTokens",
                table: "ExtractedClinicalData",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Total tokens consumed for this extraction (AC4 - US_058, AIR-O05)");

            // Add cost tracking field (AC4 - cost management)
            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedCost",
                table: "ExtractedClinicalData",
                type: "numeric(10,6)",
                precision: 10,
                scale: 6,
                nullable: false,
                defaultValue: 0.0m,
                comment: "Estimated cost in USD for this AI request (AC4 - US_058, AIR-O05)");

            // ========================================
            // ClinicalDocument Table Updates
            // ========================================
            
            // Add ModelVersion for document-level tracking
            migrationBuilder.AddColumn<string>(
                name: "ModelVersion",
                table: "ClinicalDocuments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                comment: "AI model version used for document processing (AC5 - US_058, NFR-015)");

            // Add aggregated token tracking fields
            migrationBuilder.AddColumn<int>(
                name: "TotalPromptTokens",
                table: "ClinicalDocuments",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Total prompt tokens consumed for this document (AC4 - US_058, AIR-O05)");

            migrationBuilder.AddColumn<int>(
                name: "TotalCompletionTokens",
                table: "ClinicalDocuments",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Total completion tokens consumed for this document (AC4 - US_058, AIR-O05)");

            migrationBuilder.AddColumn<int>(
                name: "TotalTokens",
                table: "ClinicalDocuments",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Total tokens consumed for this document (AC4 - US_058, AIR-O05)");

            // Add aggregated cost tracking field
            migrationBuilder.AddColumn<decimal>(
                name: "TotalEstimatedCost",
                table: "ClinicalDocuments",
                type: "numeric(10,6)",
                precision: 10,
                scale: 6,
                nullable: false,
                defaultValue: 0.0m,
                comment: "Total estimated cost in USD for this document (AC4 - US_058, AIR-O05)");

            // ========================================
            // Performance Indexes
            // ========================================
            
            // Index for querying low-confidence extractions requiring manual review
            migrationBuilder.CreateIndex(
                name: "IX_ExtractedClinicalData_RequiresManualReview_ConfidenceScore",
                table: "ExtractedClinicalData",
                columns: new[] { "RequiresManualReview", "ConfidenceScore" },
                filter: "\"RequiresManualReview\" = true");

            // Index for querying unverified AI suggestions
            migrationBuilder.CreateIndex(
                name: "IX_ExtractedClinicalData_VerificationStatus_IsAISuggested",
                table: "ExtractedClinicalData",
                columns: new[] { "VerificationStatus", "IsAISuggested" },
                filter: "\"VerificationStatus\" = 0 AND \"IsAISuggested\" = true");

            // Index for cost tracking and monitoring queries
            migrationBuilder.CreateIndex(
                name: "IX_ExtractedClinicalData_ExtractedAt_TotalTokens",
                table: "ExtractedClinicalData",
                columns: new[] { "ExtractedAt", "TotalTokens" });

            // Index for document-level cost queries
            migrationBuilder.CreateIndex(
                name: "IX_ClinicalDocuments_ProcessedAt_TotalEstimatedCost",
                table: "ClinicalDocuments",
                columns: new[] { "ProcessedAt", "TotalEstimatedCost" },
                filter: "\"ProcessedAt\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ========================================
            // Drop Indexes
            // ========================================
            
            migrationBuilder.DropIndex(
                name: "IX_ExtractedClinicalData_RequiresManualReview_ConfidenceScore",
                table: "ExtractedClinicalData");

            migrationBuilder.DropIndex(
                name: "IX_ExtractedClinicalData_VerificationStatus_IsAISuggested",
                table: "ExtractedClinicalData");

            migrationBuilder.DropIndex(
                name: "IX_ExtractedClinicalData_ExtractedAt_TotalTokens",
                table: "ExtractedClinicalData");

            migrationBuilder.DropIndex(
                name: "IX_ClinicalDocuments_ProcessedAt_TotalEstimatedCost",
                table: "ClinicalDocuments");

            // ========================================
            // Drop ExtractedClinicalData Columns
            // ========================================
            
            migrationBuilder.DropColumn(
                name: "IsAISuggested",
                table: "ExtractedClinicalData");

            migrationBuilder.DropColumn(
                name: "RequiresManualReview",
                table: "ExtractedClinicalData");

            migrationBuilder.DropColumn(
                name: "ModelVersion",
                table: "ExtractedClinicalData");

            migrationBuilder.DropColumn(
                name: "PromptTokens",
                table: "ExtractedClinicalData");

            migrationBuilder.DropColumn(
                name: "CompletionTokens",
                table: "ExtractedClinicalData");

            migrationBuilder.DropColumn(
                name: "TotalTokens",
                table: "ExtractedClinicalData");

            migrationBuilder.DropColumn(
                name: "EstimatedCost",
                table: "ExtractedClinicalData");

            // ========================================
            // Drop ClinicalDocument Columns
            // ========================================
            
            migrationBuilder.DropColumn(
                name: "ModelVersion",
                table: "ClinicalDocuments");

            migrationBuilder.DropColumn(
                name: "TotalPromptTokens",
                table: "ClinicalDocuments");

            migrationBuilder.DropColumn(
                name: "TotalCompletionTokens",
                table: "ClinicalDocuments");

            migrationBuilder.DropColumn(
                name: "TotalTokens",
                table: "ClinicalDocuments");

            migrationBuilder.DropColumn(
                name: "TotalEstimatedCost",
                table: "ClinicalDocuments");
        }
    }
}
