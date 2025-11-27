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
            var score = await llmService.AssessSolutionAsync(request.TaskDescription, request.Solution, request.Language);
            
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
            var score = await llmService.AssessSolutionAsync(request.TaskDescription, request.Solution, request.Language);

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
    
    [HttpPost("generate-feedback")]
    public async Task<ActionResult<string>> GenerateFeedback([FromBody] FeedbackGenerationRequest request)
    {
        try
        {
            var feedback = await llmService.GenerateFeedbackAsync(request.TaskDescription, request.Solution, request.Score, request.Language);
            return Ok(feedback);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating feedback for task: {TaskId}", request.TaskId);
            return StatusCode(500, new { error = "Internal server error during feedback generation." });
        }
    }

    [HttpGet("next-task-difficulty/{sessionId}")]
    public async Task<ActionResult<float>> GetNextTaskDifficulty(string sessionId)
    {
        try
        {
            var difficulty = await llmService.GetNextTaskDifficultyAsync(sessionId);
            return Ok(difficulty);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting next task difficulty for session: {SessionId}", sessionId);
            return StatusCode(500, new { error = "Internal server error during difficulty retrieval." });
        }
    }

    [HttpPost("interviewer-chat")]
    public async Task<ActionResult<string>> InterviewerChat([FromBody] InterviewerChatRequest request)
    {
        try
        {
            var answer = await llmService.RespondToCandidateQuestionAsync(request.Question, request.TaskDescription, request.CurrentSolution);
            return Ok(answer);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error responding to candidate question for task: {TaskId}", request.TaskId);
            return StatusCode(500, new { error = "Internal server error during chat response." });
        }
    }
}