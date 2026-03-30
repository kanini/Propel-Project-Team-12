-- Migration: Add DocumentChunks table for RAG vector storage
-- Requires: pgvector extension (already enabled)

CREATE TABLE IF NOT EXISTS "DocumentChunks" (
    "ChunkId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "DocumentId" UUID NOT NULL,
    "ChunkIndex" INTEGER NOT NULL,
    "ChunkText" TEXT NOT NULL,
    "TokenCount" INTEGER NOT NULL,
    "Embedding" vector(768),
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT "FK_DocumentChunks_Documents" FOREIGN KEY ("DocumentId")
        REFERENCES "ClinicalDocuments"("DocumentId") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_DocumentChunks_DocumentId"
    ON "DocumentChunks" ("DocumentId");

CREATE INDEX IF NOT EXISTS "IX_DocumentChunks_Embedding"
    ON "DocumentChunks" USING ivfflat ("Embedding" vector_cosine_ops) WITH (lists = 100);
