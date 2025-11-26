namespace LLMService.Models.Adaptive;

public class AdaptiveDifficultyRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string CurrentTaskId { get; set; } = string.Empty;
    public string SubmittedSolution { get; set; } = string.Empty;
    public float Score { get; set; } // 1.0 - 5.0
    public string Performance { get; set; } = "Correct"; // "Correct", "Partially", "Incorrect"
    public float CurrentDifficulty { get; set; } = 2.5f;
}