using System.Text.Json.Serialization;

namespace LLMService.Models.Scibox;

public class EmbeddingRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("input")]
    public object Input { get; set; } = string.Empty; // Can be string or string[]
}