namespace LLMService.Controllers;

public class PlagiarismCheckRequest
{
    public string TaskId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}