using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PatientAccess.Data;
using PatientAccess.Data.Models;
using Pgvector;

namespace PatientAccess.Tests.Data;

/// <summary>
/// Integration tests for pgvector knowledge base indices (DR-010, AIR-R04).
/// Tests ICD10Code, CPTCode, and ClinicalTerminology vector storage and retrieval.
/// 
/// Prerequisites:
/// - PostgreSQL 16+ with pgvector 0.5+ extension enabled
/// - Database connection configured in test environment
/// - AddVectorIndices migration applied
/// </summary>
public class VectorIndicesTests : IAsyncLifetime
{
    private PatientAccessDbContext _context = null!;
    private readonly string _connectionString;

    public VectorIndicesTests()
    {
        // Read connection string from environment or use test database
        _connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
            ?? "Host=aws-1-ap-northeast-2.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.dhbgcoscqujsfycytvns;Password=SET_VIA_ENV;SSL Mode=Require;Trust Server Certificate=true";
    }

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<PatientAccessDbContext>()
            .UseNpgsql(_connectionString, npgsqlOptions => npgsqlOptions.UseVector())
            .Options;

        _context = new PatientAccessDbContext(options);

        // Ensure database is created and migrations are applied
        await _context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        // Clean up test data
        if (_context != null)
        {
            await _context.DisposeAsync();
        }
    }

    #region ICD10Code Tests

    [Fact]
    public async Task ICD10Code_VectorEmbedding_StorageAndRetrieval()
    {
        // Arrange
        var testVector = CreateTestVector(1536);
        var icd10Code = new ICD10Code
        {
            Code = "E11.9",
            Description = "Type 2 diabetes mellitus without complications",
            Category = "Endocrine, nutritional and metabolic diseases",
            ChapterCode = "E00-E89",
            Embedding = testVector,
            ChunkText = "Type 2 diabetes mellitus without complications",
            Metadata = "{\"version\": \"ICD-10-CM-2024\", \"status\": \"active\"}",
            Version = "ICD-10-CM-2024"
        };

        // Act
        await _context.ICD10Codes.AddAsync(icd10Code);
        await _context.SaveChangesAsync();

        // Retrieve and verify
        var retrieved = await _context.ICD10Codes
            .FirstOrDefaultAsync(c => c.Code == "E11.9");

        // Assert
        retrieved.Should().NotBeNull("ICD-10 code should be stored");
        retrieved!.Description.Should().Be("Type 2 diabetes mellitus without complications");
        retrieved.Embedding.Should().NotBeNull("Embedding should be stored");
        retrieved.Embedding!.ToArray().Should().HaveCount(1536, "Embedding should be 1536 dimensions");

        // Cleanup
        _context.ICD10Codes.Remove(retrieved);
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task ICD10Code_CosineSimilarityQuery_ReturnsTopResults()
    {
        // Arrange - Insert test data with known vectors
        var queryVector = CreateTestVector(1536, baseValue: 1.0f);
        var similarVector = CreateTestVector(1536, baseValue: 1.0f); // Same as query
        var differentVector = CreateTestVector(1536, baseValue: 0.0f); // Different from query

        var code1 = new ICD10Code
        {
            Code = "E11.9",
            Description = "Type 2 diabetes mellitus without complications",
            Category = "Endocrine",
            ChapterCode = "E00-E89",
            Embedding = similarVector,
            ChunkText = "Diabetes type 2",
            Version = "ICD-10-CM-2024"
        };

        var code2 = new ICD10Code
        {
            Code = "I10",
            Description = "Essential primary hypertension",
            Category = "Circulatory",
            ChapterCode = "I00-I99",
            Embedding = differentVector,
            ChunkText = "High blood pressure",
            Version = "ICD-10-CM-2024"
        };

        await _context.ICD10Codes.AddRangeAsync(code1, code2);
        await _context.SaveChangesAsync();

        // Act - Query using cosine similarity
        var results = await _context.ICD10Codes
            .FromSqlRaw(@"
                SELECT * FROM ""ICD10Codes""
                WHERE ""IsActive"" = true
                ORDER BY ""Embedding"" <-> {0}::vector(1536)
                LIMIT 1", queryVector)
            .ToListAsync();

        // Assert
        results.Should().NotBeEmpty("Query should return results");
        results.First().Code.Should().Be("E11.9", "Most similar code should be returned first");

        // Cleanup
        _context.ICD10Codes.RemoveRange(code1, code2);
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task ICD10Code_UniqueCodeConstraint_ThrowsException()
    {
        // Arrange
        var code1 = new ICD10Code
        {
            Code = "E11.9",
            Description = "Description 1",
            Category = "Endocrine",
            ChapterCode = "E00-E89",
            ChunkText = "Test",
            Version = "ICD-10-CM-2024"
        };

        var code2 = new ICD10Code
        {
            Code = "E11.9", // Duplicate code
            Description = "Description 2",
            Category = "Endocrine",
            ChapterCode = "E00-E89",
            ChunkText = "Test",
            Version = "ICD-10-CM-2024"
        };

        await _context.ICD10Codes.AddAsync(code1);
        await _context.SaveChangesAsync();

        // Act
        Func<Task> act = async () =>
        {
            await _context.ICD10Codes.AddAsync(code2);
            await _context.SaveChangesAsync();
        };

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>("Duplicate Code should violate unique constraint");

        // Cleanup
        _context.ICD10Codes.Remove(code1);
        await _context.SaveChangesAsync();
    }

    #endregion

    #region CPTCode Tests

    [Fact]
    public async Task CPTCode_VectorEmbedding_StorageAndRetrieval()
    {
        // Arrange
        var testVector = CreateTestVector(1536);
        var cptCode = new CPTCode
        {
            Code = "99213",
            Description = "Office visit established patient",
            Category = "Evaluation and Management",
            Embedding = testVector,
            ChunkText = "E/M established patient visit",
            Metadata = "{\"version\": \"CPT-2024\", \"rvuValue\": 1.42}",
            Version = "CPT-2024"
        };

        // Act
        await _context.CPTCodes.AddAsync(cptCode);
        await _context.SaveChangesAsync();

        // Retrieve and verify
        var retrieved = await _context.CPTCodes
            .FirstOrDefaultAsync(c => c.Code == "99213");

        // Assert
        retrieved.Should().NotBeNull("CPT code should be stored");
        retrieved!.Embedding.Should().NotBeNull("Embedding should be stored");
        retrieved.Embedding!.ToArray().Should().HaveCount(1536, "Embedding should be 1536 dimensions");

        // Cleanup
        _context.CPTCodes.Remove(retrieved);
        await _context.SaveChangesAsync();
    }

    #endregion

    #region ClinicalTerminology Tests

    [Fact]
    public async Task ClinicalTerminology_WithSynonyms_StorageAndRetrieval()
    {
        // Arrange
        var testVector = CreateTestVector(1536);
        var term = new ClinicalTerminology
        {
            Term = "Type 2 Diabetes Mellitus",
            Category = "Diagnosis",
            Synonyms = "[\"T2DM\", \"NIDDM\", \"Adult-onset diabetes\"]",
            Embedding = testVector,
            ChunkText = "Type 2 Diabetes Mellitus chronic condition",
            Metadata = "{\"source\": \"SNOMED-CT\", \"mappedICD10Codes\": [\"E11.9\"]}",
            Source = "SNOMED-CT"
        };

        // Act
        await _context.ClinicalTerminology.AddAsync(term);
        await _context.SaveChangesAsync();

        // Retrieve and verify
        var retrieved = await _context.ClinicalTerminology
            .FirstOrDefaultAsync(t => t.Term == "Type 2 Diabetes Mellitus");

        // Assert
        retrieved.Should().NotBeNull("Clinical term should be stored");
        retrieved!.Synonyms.Should().Contain("T2DM", "Synonyms should be retrievable");
        retrieved.Embedding.Should().NotBeNull("Embedding should be stored");

        // Cleanup
        _context.ClinicalTerminology.Remove(retrieved);
        await _context.SaveChangesAsync();
    }

    #endregion

    #region Hybrid Search Tests

    [Fact]
    public async Task HybridSearch_CombinesVectorAndMetadataFiltering()
    {
        // Arrange
        var queryVector = CreateTestVector(1536, baseValue: 1.0f);
        var code = new ICD10Code
        {
            Code = "E11.65",
            Description = "Type 2 diabetes mellitus with hyperglycemia",
            Category = "Endocrine",
            ChapterCode = "E00-E89",
            Embedding = CreateTestVector(1536, baseValue: 1.0f),
            ChunkText = "Diabetes with high blood sugar",
            Metadata = "{\"status\": \"active\", \"subcategories\": [\"diabetes\", \"endocrine\"]}",
            Version = "ICD-10-CM-2024",
            IsActive = true
        };

        await _context.ICD10Codes.AddAsync(code);
        await _context.SaveChangesAsync();

        // Act - Hybrid query: vector similarity + JSONB metadata filtering
        var results = await _context.ICD10Codes
            .FromSqlRaw(@"
                SELECT * FROM ""ICD10Codes""
                WHERE ""IsActive"" = true
                  AND ""Metadata"" @> '{""subcategories"": [""diabetes""]}'::jsonb
                ORDER BY ""Embedding"" <-> {0}::vector(1536)
                LIMIT 5", queryVector)
            .ToListAsync();

        // Assert
        results.Should().NotBeEmpty("Hybrid query should return filtered results");
        results.First().Code.Should().Be("E11.65");

        // Cleanup
        _context.ICD10Codes.Remove(code);
        await _context.SaveChangesAsync();
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task VectorSearch_WithHNSWIndex_CompletesUnder100ms()
    {
        // Arrange
        var queryVector = CreateTestVector(1536);

        // Insert test data
        var codes = Enumerable.Range(1, 100).Select(i => new ICD10Code
        {
            Code = $"TEST{i:D4}",
            Description = $"Test code {i}",
            Category = "Test",
            ChapterCode = "T00-T99",
            Embedding = CreateTestVector(1536, baseValue: (float)i / 100),
            ChunkText = $"Test chunk {i}",
            Version = "TEST-2024"
        }).ToList();

        await _context.ICD10Codes.AddRangeAsync(codes);
        await _context.SaveChangesAsync();

        // Act - Measure query time
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var results = await _context.ICD10Codes
            .FromSqlRaw(@"
                SELECT * FROM ""ICD10Codes""
                WHERE ""IsActive"" = true
                ORDER BY ""Embedding"" <-> {0}::vector(1536)
                LIMIT 5", queryVector)
            .ToListAsync();
        stopwatch.Stop();

        // Assert
        results.Should().HaveCount(5, "Query should return top 5 results");
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100, "Query should complete in <100ms with HNSW index");

        // Cleanup
        _context.ICD10Codes.RemoveRange(codes);
        await _context.SaveChangesAsync();
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a test vector with specified dimensions.
    /// </summary>
    /// <param name="dimensions">Vector dimensions (default 1536)</param>
    /// <param name="baseValue">Base value for all elements (default 0.5f)</param>
    private static Vector CreateTestVector(int dimensions, float baseValue = 0.5f)
    {
        var array = Enumerable.Repeat(baseValue, dimensions).ToArray();
        return new Vector(array);
    }

    #endregion
}
