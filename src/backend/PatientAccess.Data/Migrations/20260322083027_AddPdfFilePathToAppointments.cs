using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientAccess.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPdfFilePathToAppointments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeSlots_Appointments",
                table: "TimeSlots");

            migrationBuilder.DropIndex(
                name: "IX_TimeSlots_AppointmentId",
                table: "TimeSlots");

            migrationBuilder.DropColumn(
                name: "AppointmentId",
                table: "TimeSlots");

            migrationBuilder.AlterColumn<string>(
                name: "VisitReason",
                table: "Appointments",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "ConfirmationNumber",
                table: "Appointments",
                type: "varchar(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PdfFilePath",
                table: "Appointments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PreferredSlotId",
                table: "Appointments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TimeSlotId",
                table: "Appointments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ConfirmationNumber",
                table: "Appointments",
                column: "ConfirmationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PreferredSlotId",
                table: "Appointments",
                column: "PreferredSlotId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_TimeSlotId",
                table: "Appointments",
                column: "TimeSlotId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_PreferredSlot",
                table: "Appointments",
                column: "PreferredSlotId",
                principalTable: "TimeSlots",
                principalColumn: "TimeSlotId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_TimeSlot",
                table: "Appointments",
                column: "TimeSlotId",
                principalTable: "TimeSlots",
                principalColumn: "TimeSlotId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_PreferredSlot",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_TimeSlot",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_ConfirmationNumber",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_PreferredSlotId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_TimeSlotId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ConfirmationNumber",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "PdfFilePath",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "PreferredSlotId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "TimeSlotId",
                table: "Appointments");

            migrationBuilder.AddColumn<Guid>(
                name: "AppointmentId",
                table: "TimeSlots",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "VisitReason",
                table: "Appointments",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500);

            migrationBuilder.CreateIndex(
                name: "IX_TimeSlots_AppointmentId",
                table: "TimeSlots",
                column: "AppointmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeSlots_Appointments",
                table: "TimeSlots",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "AppointmentId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
