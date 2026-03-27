using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientAccess.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentChunkStaging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentChunks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CodeSystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SourceText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    TokenCount = table.Column<int>(type: "integer", nullable: false),
                    ChunkIndex = table.Column<int>(type: "integer", nullable: false),
                    StartToken = table.Column<int>(type: "integer", nullable: false),
                    EndToken = table.Column<int>(type: "integer", nullable: false),
                    OverlapWithPrevious = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    TargetEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsProcessed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentChunks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentChunks_CodeSystem",
                table: "DocumentChunks",
                column: "CodeSystem");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentChunks_CodeSystem_IsProcessed",
                table: "DocumentChunks",
                columns: new[] { "CodeSystem", "IsProcessed" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentChunks_IsProcessed",
                table: "DocumentChunks",
                column: "IsProcessed");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentChunks_TargetEntityId",
                table: "DocumentChunks",
                column: "TargetEntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentChunks");
        }
    }
}
