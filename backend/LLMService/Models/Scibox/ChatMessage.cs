using System.Text.Json.Serialization;

namespace LLMService.Models.Scibox;

public class ChatMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    public ChatMessage(string role, string content)
    {
        Role = role;
        Content = content;
    }
}