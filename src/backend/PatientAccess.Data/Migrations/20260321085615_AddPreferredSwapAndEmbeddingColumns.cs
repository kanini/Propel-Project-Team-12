using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace PatientAccess.Data.Migrations
{
    /// <summary>
    /// Adds missing columns to close EP-DATA-I gaps:
    ///   - Appointments.PreferredSwapReference (Guid?, nullable) — US_010 AC1 dynamic slot swap
    ///   - ExtractedClinicalData.Embedding (vector(1536), nullable) — US_011 AC4 pgvector semantic search
    ///
    /// ZERO-DOWNTIME: Both columns are added as NULLABLE, requiring no backfill.
    /// This follows the non-breaking schema change pattern documented in DR-009.
    /// </summary>
    public partial class AddPreferredSwapAndEmbeddingColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Vector>(
                name: "Embedding",
                table: "ExtractedClinicalData",
                type: "vector(1536)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PreferredSwapReference",
                table: "Appointments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PreferredSwapReference",
                table: "Appointments",
                column: "PreferredSwapReference");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_PreferredSwapReference",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "Embedding",
                table: "ExtractedClinicalData");

            migrationBuilder.DropColumn(
                name: "PreferredSwapReference",
                table: "Appointments");
        }
    }
}
