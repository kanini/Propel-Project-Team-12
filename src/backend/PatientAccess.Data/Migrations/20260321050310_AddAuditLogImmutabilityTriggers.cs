using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientAccess.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogImmutabilityTriggers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // AD-007: Immutability trigger — prevent UPDATE on AuditLogs
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION prevent_audit_log_update()
                RETURNS TRIGGER AS $$
                BEGIN
                    RAISE EXCEPTION 'UPDATE operations are not permitted on AuditLogs table. Audit records are immutable.';
                    RETURN NULL;
                END;
                $$ LANGUAGE plpgsql;

                CREATE TRIGGER trg_audit_log_no_update
                BEFORE UPDATE ON ""AuditLogs""
                FOR EACH ROW
                EXECUTE FUNCTION prevent_audit_log_update();
            ");

            // AD-007: Immutability trigger — prevent DELETE on AuditLogs
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION prevent_audit_log_delete()
                RETURNS TRIGGER AS $$
                BEGIN
                    RAISE EXCEPTION 'DELETE operations are not permitted on AuditLogs table. Audit records are immutable.';
                    RETURN NULL;
                END;
                $$ LANGUAGE plpgsql;

                CREATE TRIGGER trg_audit_log_no_delete
                BEFORE DELETE ON ""AuditLogs""
                FOR EACH ROW
                EXECUTE FUNCTION prevent_audit_log_delete();
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP TRIGGER IF EXISTS trg_audit_log_no_update ON ""AuditLogs"";
                DROP FUNCTION IF EXISTS prevent_audit_log_update();
                DROP TRIGGER IF EXISTS trg_audit_log_no_delete ON ""AuditLogs"";
                DROP FUNCTION IF EXISTS prevent_audit_log_delete();
            ");
        }
    }
}
