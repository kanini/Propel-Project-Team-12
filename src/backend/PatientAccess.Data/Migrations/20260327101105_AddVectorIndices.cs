using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace PatientAccess.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVectorIndices : Migration
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
                name: "ClinicalTerminology",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Term = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Synonyms = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "[]"),
                    Embedding = table.Column<Vector>(type: "vector(1536)", nullable: true),
                    ChunkText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Metadata = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    Source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalTerminology", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CPTCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Category = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Modifier = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Embedding = table.Column<Vector>(type: "vector(1536)", nullable: true),
                    ChunkText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Metadata = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    Version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CPTCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ICD10Codes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Category = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ChapterCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Embedding = table.Column<Vector>(type: "vector(1536)", nullable: true),
                    ChunkText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Metadata = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    Version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ICD10Codes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalTerminology_Category",
                table: "ClinicalTerminology",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalTerminology_Embedding_Cosine",
                table: "ClinicalTerminology",
                column: "Embedding")
                .Annotation("Npgsql:IndexMethod", "hnsw")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalTerminology_IsActive",
                table: "ClinicalTerminology",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalTerminology_Metadata_Gin",
                table: "ClinicalTerminology",
                column: "Metadata")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalTerminology_Term",
                table: "ClinicalTerminology",
                column: "Term");

            migrationBuilder.CreateIndex(
                name: "IX_CPTCodes_Category",
                table: "CPTCodes",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_CPTCodes_Code",
                table: "CPTCodes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CPTCodes_Embedding_Cosine",
                table: "CPTCodes",
                column: "Embedding")
                .Annotation("Npgsql:IndexMethod", "hnsw")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_CPTCodes_IsActive",
                table: "CPTCodes",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_CPTCodes_Metadata_Gin",
                table: "CPTCodes",
                column: "Metadata")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_ICD10Codes_Category",
                table: "ICD10Codes",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_ICD10Codes_Code",
                table: "ICD10Codes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ICD10Codes_Embedding_Cosine",
                table: "ICD10Codes",
                column: "Embedding")
                .Annotation("Npgsql:IndexMethod", "hnsw")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_ICD10Codes_IsActive",
                table: "ICD10Codes",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ICD10Codes_Metadata_Gin",
                table: "ICD10Codes",
                column: "Metadata")
                .Annotation("Npgsql:IndexMethod", "gin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClinicalTerminology");

            migrationBuilder.DropTable(
                name: "CPTCodes");

            migrationBuilder.DropTable(
                name: "ICD10Codes");

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
