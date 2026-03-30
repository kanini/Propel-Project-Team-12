using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Interfaces;

public interface ITextChunkingService
{
    List<TextChunkDto> ChunkText(string text, int maxTokens = 512, double overlapRatio = 0.125);
}
