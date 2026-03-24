using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientAccess.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUploadedByToClinicalDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UploadedBy",
                table: "ClinicalDocuments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalDocuments_UploadedBy",
                table: "ClinicalDocuments",
                column: "UploadedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_ClinicalDocuments_UploadedBy",
                table: "ClinicalDocuments",
                column: "UploadedBy",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClinicalDocuments_UploadedBy",
                table: "ClinicalDocuments");

            migrationBuilder.DropIndex(
                name: "IX_ClinicalDocuments_UploadedBy",
                table: "ClinicalDocuments");

            migrationBuilder.DropColumn(
                name: "UploadedBy",
                table: "ClinicalDocuments");
        }
    }
}
