namespace LLMService.Controllers;

public class InterviewerChatRequest
{
    public string TaskId { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string TaskDescription { get; set; } = string.Empty;
    public string CurrentSolution { get; set; } = string.Empty;
}