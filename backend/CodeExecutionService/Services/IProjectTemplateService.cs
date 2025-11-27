namespace CodeExecutionService.Services;

public interface IProjectTemplateService
{
    (string template, string fileName) GetProjectTemplateForLanguage(string language);
    string GetRunCommandForLanguage(string language);
    string GetCodeFileNameForLanguage(string language);
}