using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientAccess.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditRetentionPolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // DR-007: HIPAA 7-year audit log retention enforcement.
            // Prevents deletion of audit log entries younger than 7 years.
            // Records older than 7 years may be archived externally.
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION enforce_audit_retention_policy()
                RETURNS TRIGGER AS $$
                BEGIN
                    IF OLD.""Timestamp"" > (NOW() - INTERVAL '7 years') THEN
                        RAISE EXCEPTION 'Cannot delete audit log entries younger than 7 years (HIPAA DR-007). Record timestamp: %', OLD.""Timestamp"";
                    END IF;
                    RETURN OLD;
                END;
                $$ LANGUAGE plpgsql;

                -- Drop the unconditional delete trigger and replace with retention-aware one
                DROP TRIGGER IF EXISTS trg_audit_log_no_delete ON ""AuditLogs"";

                CREATE TRIGGER trg_audit_log_retention_delete
                BEFORE DELETE ON ""AuditLogs""
                FOR EACH ROW
                EXECUTE FUNCTION enforce_audit_retention_policy();
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP TRIGGER IF EXISTS trg_audit_log_retention_delete ON ""AuditLogs"";
                DROP FUNCTION IF EXISTS enforce_audit_retention_policy();

                -- Restore the unconditional delete prevention trigger
                CREATE TRIGGER trg_audit_log_no_delete
                BEFORE DELETE ON ""AuditLogs""
                FOR EACH ROW
                EXECUTE FUNCTION prevent_audit_log_delete();
            ");
        }
    }
}
