using LLMService.Models.Scibox;
using System.Text;
using System.Text.Json;
using LLMService.Models.Scibox;

namespace LLMService.Services;

public class SciboxClient : ISciboxClient
{
   private readonly HttpClient _httpClient;
   private readonly ILogger<SciboxClient> _logger;
   private readonly IConfiguration _configuration;

   public SciboxClient(HttpClient httpClient, ILogger<SciboxClient> logger, IConfiguration configuration)
   { 
       _httpClient = httpClient; 
       _logger = logger; 
       _configuration = configuration;
   }

    public async Task<ChatCompletionResponse> ChatCompletionAsync(ChatCompletionRequest request)
    {
        var apiKey = _configuration["Scibox:ApiKey"];
        
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/chat/completions", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Scibox API error: {StatusCode} - {Response}", response.StatusCode, responseContent);
            throw new HttpRequestException($"Scibox API error: {response.StatusCode}");
        }

        var result = JsonSerializer.Deserialize<ChatCompletionResponse>(responseContent);
        
        return result ?? new ChatCompletionResponse();
    }

    public async Task<EmbeddingResponse> EmbeddingAsync(EmbeddingRequest request)
    { 
        var apiKey = _configuration["Scibox:ApiKey"];
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/embeddings", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Scibox Embedding API error: {StatusCode} - {Response}", response.StatusCode, responseContent);
            throw new HttpRequestException($"Scibox Embedding API error: {response.StatusCode}");
        }

        var result = JsonSerializer.Deserialize<EmbeddingResponse>(responseContent);
        
        return result ?? new EmbeddingResponse();
    }
}