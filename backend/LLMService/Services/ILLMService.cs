using LLMService.Models.Adaptive;
using LLMService.Models.Interview;
using LLMService.Models.TaskGeneration;

namespace LLMService.Services;

public interface ILLMService
{
    Task<CodingTaskResponse> GenerateCodingTaskAsync(CodingTaskRequest request); // Обновлённый метод
    Task<float> AssessSolutionAsync(string taskDescription, string solution, string language);
    Task<float> CalculateAdaptiveDifficultyAsync(AdaptiveDifficultyRequest request);
    Task<string> DetermineCandidateGradeAsync(string sessionId);
    Task<bool> DetectPlagiarismAsync(string code, string taskId);
}