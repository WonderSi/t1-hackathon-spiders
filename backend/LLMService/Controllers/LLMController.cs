using Microsoft.AspNetCore.Mvc;
using LLMService.Services;
using LLMService.Models.TaskGeneration;
using LLMService.Models.Adaptive;
using LLMService.Models.Interview;

namespace LLMService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LLMController(ILLMService llmService, ILogger<LLMController> logger) : ControllerBase
{
    [HttpPost("generate-task")]
    public async Task<ActionResult<CodingTaskResponse>> GenerateTask([FromBody] CodingTaskRequest request)
    {
        try
        {
            var response = await llmService.GenerateCodingTaskAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating coding task for request: {@Request}", request);
            return StatusCode(500, new { error = "Internal server error during task generation." });
        }
    }

    [HttpPost("assess-solution")]
    public async Task<ActionResult<float>> AssessSolution([FromBody] SolutionAssessmentRequest request)
    {
        try
        {
            // ✅ Теперь передаём и taskId, и сложность задачи (если известна)
            // В реальном сценарии, сложность задачи должна быть передана от InterviewService
            // или извлекаться из сессии.
            // Для упрощения, предположим, что сложность передаётся.
            var score = await llmService.AssessSolutionAsync(request.TaskDescription, request.Solution, request.Language);
            // НЕТУ БОЛЬШЕ вызова CalculateAdaptiveDifficultyAsync здесь, если вы хотите отделить оценку и адаптацию
            return Ok(score);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error assessing solution for task: {TaskId}", request.TaskId);
            return StatusCode(500, new { error = "Internal server error during solution assessment." });
        }
    }

    [HttpPost("submit-solution")]
    public async Task<ActionResult<SubmissionResult>> SubmitSolution([FromBody] SubmissionRequest request)
    {
        try
        {
            // 1. Оценить решение
            var score = await llmService.AssessSolutionAsync(request.TaskDescription, request.Solution, request.Language);

            // 2. Обновить адаптивность (передаём сложность задачи, на которой получили оценку)
            var adaptiveRequest = new AdaptiveDifficultyRequest
            {
                SessionId = request.SessionId,
                CurrentTaskId = request.TaskId,
                SubmittedSolution = request.Solution,
                Score = score,
                Performance = score >= 4.0 ? "Correct" : score >= 2.0 ? "Partially" : "Incorrect",
                CurrentDifficulty = request.TaskDifficulty // ✅ Передаём сложность задачи
            };

            var newDifficulty = await llmService.CalculateAdaptiveDifficultyAsync(adaptiveRequest);

            // 3. Определить грейд
            var grade = await llmService.DetermineCandidateGradeAsync(request.SessionId);

            return Ok(new SubmissionResult
            {
                Score = score,
                NewDifficulty = newDifficulty,
                Grade = grade
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error submitting solution for task: {TaskId}", request.TaskId);
            return StatusCode(500, new { error = "Internal server error during solution submission." });
        }
    }
    
    [HttpPost("calculate-adaptive-difficulty")]
    public async Task<ActionResult<float>> CalculateAdaptiveDifficulty([FromBody] AdaptiveDifficultyRequest request)
    {
        try
        {
            var newDifficulty = await llmService.CalculateAdaptiveDifficultyAsync(request);
            return Ok(newDifficulty);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calculating adaptive difficulty for session: {SessionId}", request.SessionId);
            return StatusCode(500, new { error = "Internal server error during adaptive difficulty calculation." });
        }
    }

    [HttpPost("determine-grade")]
    public async Task<ActionResult<string>> DetermineGrade([FromBody] DetermineGradeRequest request)
    {
        try
        {
            var grade = await llmService.DetermineCandidateGradeAsync(request.SessionId);
            return Ok(grade);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error determining grade for session: {SessionId}", request.SessionId);
            return StatusCode(500, new { error = "Internal server error during grade determination." });
        }
    }
    
    [HttpPost("detect-plagiarism")]
    public async Task<ActionResult<bool>> DetectPlagiarism([FromBody] PlagiarismCheckRequest request)
    {
        try
        {
            var isPlagiarized = await llmService.DetectPlagiarismAsync(request.Code, request.TaskId);
            return Ok(isPlagiarized);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error detecting plagiarism for task: {TaskId}", request.TaskId);
            return StatusCode(500, new { error = "Internal server error during plagiarism check." });
        }
    }
}