namespace LLMService.Controllers;

public class SubmissionRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string TaskId { get; set; } = string.Empty;
    public string TaskDescription { get; set; } = string.Empty;
    public string Solution { get; set; } = string.Empty;
    public string Language { get; set; } = "C#";
    public float TaskDifficulty { get; set; } = 2.0f;
}