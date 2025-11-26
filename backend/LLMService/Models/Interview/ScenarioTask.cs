namespace LLMService.Models.Interview;

public class ScenarioTask
{
    public string Id { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty; // "Algorithms", "OOP", etc.
    public float Difficulty { get; set; } = 2.5f;
    public string ExampleInput { get; set; } = string.Empty;
    public string ExampleOutput { get; set; } = string.Empty;
}