using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;

namespace PatientAccess.Business.Services;

public class TextChunkingService : ITextChunkingService
{
    // Approximate tokens ≈ words * 1.3 (English), so ~512 tokens ≈ 394 words ≈ 2000 chars
    private const int CharsPerToken = 4;

    public List<TextChunkDto> ChunkText(string text, int maxTokens = 512, double overlapRatio = 0.125)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new List<TextChunkDto>();

        var maxChars = maxTokens * CharsPerToken;
        var overlapChars = (int)(maxChars * overlapRatio);
        var chunks = new List<TextChunkDto>();
        var index = 0;
        var position = 0;

        while (position < text.Length)
        {
            var end = Math.Min(position + maxChars, text.Length);
            var chunkText = text[position..end];

            // Try to break at a sentence boundary
            if (end < text.Length)
            {
                var lastPeriod = chunkText.LastIndexOf(". ", StringComparison.Ordinal);
                var lastNewline = chunkText.LastIndexOf('\n');
                var breakPoint = Math.Max(lastPeriod, lastNewline);

                if (breakPoint > maxChars / 2)
                {
                    chunkText = chunkText[..(breakPoint + 1)];
                    end = position + breakPoint + 1;
                }
            }

            chunks.Add(new TextChunkDto
            {
                Index = index,
                Text = chunkText.Trim(),
                TokenCount = (int)Math.Ceiling((double)chunkText.Length / CharsPerToken)
            });

            var newPosition = end - overlapChars;
            if (newPosition <= position)
                newPosition = end; // Ensure forward progress — prevent infinite loop
            position = newPosition;

            index++;
        }

        return chunks;
    }
}
