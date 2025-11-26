namespace LLMService.Controllers;

public class SolutionAssessmentRequest
{
    public string TaskId { get; set; } = string.Empty;
    public string TaskDescription { get; set; } = string.Empty;
    public string Solution { get; set; } = string.Empty;
    public string Language { get; set; } = "C#";
}