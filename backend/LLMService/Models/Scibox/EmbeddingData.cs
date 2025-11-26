using System.Text.Json.Serialization;

namespace LLMService.Models.Scibox;

public class EmbeddingData
{
    [JsonPropertyName("embedding")]
    public List<double> Embedding { get; set; } = new();

    [JsonPropertyName("index")]
    public int Index { get; set; }
}