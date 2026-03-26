using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientAccess.Data.Migrations
{
    /// <summary>
    /// Migration to remove ProcessingNotes column from ClinicalDocuments table.
    /// The ProcessingNotes field is no longer needed as manual review flagging is handled through RequiresManualReview boolean.
    /// </summary>
    public partial class RemoveProcessingNotesFromClinicalDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProcessingNotes",
                table: "ClinicalDocuments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProcessingNotes",
                table: "ClinicalDocuments",
                type: "text",
                nullable: true);
        }
    }
}
