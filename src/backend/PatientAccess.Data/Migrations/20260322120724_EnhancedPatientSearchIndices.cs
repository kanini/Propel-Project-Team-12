using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientAccess.Data.Migrations
{
    /// <inheritdoc />
    public partial class EnhancedPatientSearchIndices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Index on Users.Name for optimized patient name search (US_032, AC-3)
            // Supports LIKE '%name%' queries in SearchPatientsAsync
            migrationBuilder.CreateIndex(
                name: "IX_Users_Name",
                table: "Users",
                column: "Name");

            // Composite index on Appointments(PatientId, ScheduledDateTime) for optimized last appointment lookup (US_032, AC-3)
            // Supports ORDER BY ScheduledDateTime DESC queries when filtering by PatientId
            // This significantly improves performance of the subquery in SearchPatientsAsync that finds last appointment date
            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PatientId_ScheduledDateTime",
                table: "Appointments",
                columns: new[] { "PatientId", "ScheduledDateTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop composite index
            migrationBuilder.DropIndex(
                name: "IX_Appointments_PatientId_ScheduledDateTime",
                table: "Appointments");

            // Drop name index
            migrationBuilder.DropIndex(
                name: "IX_Users_Name",
                table: "Users");
        }
    }
}
