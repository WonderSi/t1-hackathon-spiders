using System.Text.Json.Serialization;

namespace LLMService.Models.Scibox;

public class Choice
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("message")]
    public ChatMessage Message { get; set; } = new("", "");

    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; } = string.Empty;
}