using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientAccess.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCalendarEventIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NotifiedAt",
                table: "WaitlistEntries",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "NotifiedSlotId",
                table: "WaitlistEntries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResponseDeadline",
                table: "WaitlistEntries",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponseToken",
                table: "WaitlistEntries",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OutlookCalendarEventId",
                table: "Appointments",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WaitlistEntries_NotifiedSlotId",
                table: "WaitlistEntries",
                column: "NotifiedSlotId");

            migrationBuilder.CreateIndex(
                name: "IX_WaitlistEntries_ResponseToken",
                table: "WaitlistEntries",
                column: "ResponseToken",
                unique: true,
                filter: "\"ResponseToken\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WaitlistEntries_Status_ResponseDeadline",
                table: "WaitlistEntries",
                columns: new[] { "Status", "ResponseDeadline" },
                filter: "\"Status\" = 2");

            migrationBuilder.AddForeignKey(
                name: "FK_WaitlistEntries_NotifiedSlot",
                table: "WaitlistEntries",
                column: "NotifiedSlotId",
                principalTable: "TimeSlots",
                principalColumn: "TimeSlotId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WaitlistEntries_NotifiedSlot",
                table: "WaitlistEntries");

            migrationBuilder.DropIndex(
                name: "IX_WaitlistEntries_NotifiedSlotId",
                table: "WaitlistEntries");

            migrationBuilder.DropIndex(
                name: "IX_WaitlistEntries_ResponseToken",
                table: "WaitlistEntries");

            migrationBuilder.DropIndex(
                name: "IX_WaitlistEntries_Status_ResponseDeadline",
                table: "WaitlistEntries");

            migrationBuilder.DropColumn(
                name: "NotifiedAt",
                table: "WaitlistEntries");

            migrationBuilder.DropColumn(
                name: "NotifiedSlotId",
                table: "WaitlistEntries");

            migrationBuilder.DropColumn(
                name: "ResponseDeadline",
                table: "WaitlistEntries");

            migrationBuilder.DropColumn(
                name: "ResponseToken",
                table: "WaitlistEntries");

            migrationBuilder.DropColumn(
                name: "OutlookCalendarEventId",
                table: "Appointments");
        }
    }
}
