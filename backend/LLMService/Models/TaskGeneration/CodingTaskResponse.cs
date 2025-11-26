namespace LLMService.Models.TaskGeneration;

public class CodingTaskResponse
{
    public string TaskId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ExampleInput { get; set; } = string.Empty;
    public string ExampleOutput { get; set; } = string.Empty;
    public float EstimatedDifficulty { get; set; } // 1.0 - 5.0
    public string Subject { get; set; } = string.Empty;
}