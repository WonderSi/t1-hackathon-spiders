namespace LLMService.Models.Interview;

public class InterviewScenario
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // "Backend", "Frontend", "Data Science"
    public string ProgrammingLanguage { get; set; } = "C#";
    public List<ScenarioTask> Tasks { get; set; } = new(); // Загружается из "базы эксперта"
}