namespace LLMService.Controllers;

public class FeedbackGenerationRequest
{
    public string TaskId { get; set; } = string.Empty;
    public string TaskDescription { get; set; } = string.Empty;
    public string Solution { get; set; } = string.Empty;
    public float Score { get; set; } = 3.0f;
    public string Language { get; set; } = "python";
}