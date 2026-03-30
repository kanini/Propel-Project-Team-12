using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Interfaces;

public interface IVectorSearchService
{
    Task StoreChunksAsync(Guid documentId, List<TextChunkDto> chunks, List<float[]> embeddings);
    Task<List<string>> SearchSimilarChunksAsync(Guid documentId, string query, int topK = 10);
    Task<List<string>> RetrieveAllChunksAsync(Guid documentId);
}
