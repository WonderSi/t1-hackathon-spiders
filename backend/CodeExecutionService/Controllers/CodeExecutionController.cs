using CodeExecutionService.Models;
using CodeExecutionService.Services;
using Microsoft.AspNetCore.Mvc;

namespace CodeExecutionService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CodeExecutionController : ControllerBase
{
    private readonly ICodeExecutionService _codeExecutionService;
    private readonly ILogger<CodeExecutionController> _logger;

    public CodeExecutionController(ICodeExecutionService codeExecutionService, ILogger<CodeExecutionController> logger)
    {
        _codeExecutionService = codeExecutionService;
        _logger = logger;
    }

    [HttpPost("execute")]
    public async Task<ActionResult<CodeExecutionResponse>> Execute([FromBody] CodeExecutionRequest? request)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }
        
        try
        {
            var response = await _codeExecutionService.ExecuteCodeAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing code for request: {@Request}", request);
            return StatusCode(500, new { error = "Internal server error during code execution." });
        }
    }
}