using LLMService.Models.Scibox;

namespace LLMService.Services;

public interface ISciboxClient
{
    Task<ChatCompletionResponse> ChatCompletionAsync(ChatCompletionRequest request);
    Task<EmbeddingResponse> EmbeddingAsync(EmbeddingRequest request);
}