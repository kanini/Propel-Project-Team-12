using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.Interfaces;

namespace PatientAccess.Business.Services;

public class EmbeddingService : IEmbeddingService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EmbeddingService> _logger;
    private readonly string _apiKey;
    private const string EmbeddingEndpoint = "gemini-embedding-001";

    public EmbeddingService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<EmbeddingService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _apiKey = configuration["GeminiAI:ApiKey"] ?? string.Empty;
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(_apiKey) || _apiKey.StartsWith("SET_VIA_ENV", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Gemini API key not configured. Returning empty embedding.");
            return Array.Empty<float>();
        }

        var client = _httpClientFactory.CreateClient();
       
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{EmbeddingEndpoint}:embedContent?key={_apiKey}";
        var payload = new
        {
            model = "models/gemini-embedding-001",
            content = new { parts = new[] { new { text } } },
            outputDimensionality = 768
        };

        var response = await client.PostAsJsonAsync(url, payload);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var values = json.GetProperty("embedding").GetProperty("values");



        var embedding = new float[values.GetArrayLength()];
        var i = 0;
        foreach (var val in values.EnumerateArray())
        {
            embedding[i++] = val.GetSingle();
        }

        return embedding;
    }

    public async Task<List<float[]>> GenerateBatchEmbeddingsAsync(List<string> texts)
    {
        var results = new List<float[]>();

        // Gemini doesn't have a native batch embedding endpoint, so we process sequentially
        // with a small delay to respect rate limits (1500 RPM for embedding)
        foreach (var text in texts)
        {
            var embedding = await GenerateEmbeddingAsync(text);
            results.Add(embedding);

            // Small delay to avoid rate limiting
            if (texts.Count > 10)
                await Task.Delay(50);
        }

        return results;
    }
}
