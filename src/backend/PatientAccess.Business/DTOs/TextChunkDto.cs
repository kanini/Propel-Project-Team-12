namespace PatientAccess.Business.DTOs;

public class TextChunkDto
{
    public int Index { get; set; }
    public string Text { get; set; } = string.Empty;
    public int TokenCount { get; set; }
}
