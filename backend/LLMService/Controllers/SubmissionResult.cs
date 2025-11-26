namespace LLMService.Controllers;

public class SubmissionResult
{
    public float Score { get; set; }
    public float NewDifficulty { get; set; }
    public string Grade { get; set; } = "Unknown";
}