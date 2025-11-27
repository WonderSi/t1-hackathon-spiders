using CodeExecutionService.Models;

namespace CodeExecutionService.Services;

public interface ICodeExecutionService
{
    Task<CodeExecutionResponse> ExecuteCodeAsync(CodeExecutionRequest request);
}