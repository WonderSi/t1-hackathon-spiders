namespace LLMService.Models.TaskGeneration;

public class CodingTaskRequest
{
    public string? SkillLevel { get; set; } = "Middle"; // "Junior", "Middle", "Senior"
    public string ProgrammingLanguage { get; set; } = "C#";
    public string Subject { get; set; } = "Algorithms";
    public float? CurrentDifficulty { get; set; } = 2.5f; // Уровень сложности
    public string? PreviousPerformance { get; set; } = null; // "Correct", "Partially", "Incorrect"
}