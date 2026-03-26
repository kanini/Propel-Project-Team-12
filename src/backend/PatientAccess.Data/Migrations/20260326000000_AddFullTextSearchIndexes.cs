using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientAccess.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFullTextSearchIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create GIN indexes for PostgreSQL Full-Text Search (FTS) on knowledge base entities (AIR-R03).
            // These indexes support hybrid retrieval combining semantic similarity (pgvector) with keyword matching (FTS).
            // GIN (Generalized Inverted Index) is optimized for full-text search queries using to_tsvector and ts_rank.

            // ICD-10 Description FTS index - supports keyword search on diagnosis descriptions
            // Example query: to_tsvector('english', "Description") @@ plainto_tsquery('english', 'diabetes')
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_icd10_description_fts 
                ON ""ICD10Codes"" 
                USING gin(to_tsvector('english', ""Description""));
            ");

            // CPT Description FTS index - supports keyword search on procedure descriptions
            // Example query: to_tsvector('english', "Description") @@ plainto_tsquery('english', 'knee replacement')
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_cpt_description_fts 
                ON ""CPTCodes"" 
                USING gin(to_tsvector('english', ""Description""));
            ");

            // Clinical Terminology Term FTS index - supports keyword search on clinical terms
            // Example query: to_tsvector('english', "Term") @@ plainto_tsquery('english', 'hypertension')
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_clinical_term_fts 
                ON ""ClinicalTerminology"" 
                USING gin(to_tsvector('english', ""Term""));
            ");

            // Optional: Add FTS indexes on Category fields for category-based filtering
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_icd10_category_fts 
                ON ""ICD10Codes"" 
                USING gin(to_tsvector('english', ""Category""));
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_cpt_category_fts 
                ON ""CPTCodes"" 
                USING gin(to_tsvector('english', ""Category""));
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_clinical_category_fts 
                ON ""ClinicalTerminology"" 
                USING gin(to_tsvector('english', ""Category""));
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop FTS indexes (reverse order for safety)
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_clinical_category_fts;");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_cpt_category_fts;");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_icd10_category_fts;");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_clinical_term_fts;");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_cpt_description_fts;");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_icd10_description_fts;");
        }
    }
}
