namespace CodeExecutionService.Services;

public class ProjectTemplateService : IProjectTemplateService
{
    public (string template, string fileName) GetProjectTemplateForLanguage(string language)
    {
        return language.ToLower() switch
        {
            "typescript" => (
                @"{
  ""compilerOptions"": {
    ""target"": ""ES2020"",
    ""module"": ""commonjs"",
    ""outDir"": ""./dist"",
    ""rootDir"": ""./"",
    ""strict"": true,
    ""esModuleInterop"": true
  }
}", "tsconfig.json"),
            "go" => ("", ""), // Go не требует проектного файла
            "python" => ("", ""), // Python — не нужен
            "javascript" => ("", ""), // JavaScript — не нужен
            "kotlin" => ("", ""), // Kotlin — не нужен
            "java" => ("", ""), // Java — не нужен
            "cpp" => ("", ""), // C++ — не нужен
            _ => ("", "") // Язык не поддерживает проектный файл
        };
    }

    public string GetCodeFileNameForLanguage(string language)
    {
        return language.ToLower() switch
        {
            "python" => "solution.py",
            "javascript" => "solution.js",
            "typescript" => "solution.ts",
            "kotlin" => "Solution.kt",
            "java" => "Solution.java",
            "cpp" => "solution.cpp",
            "go" => "main.go",
            _ => throw new ArgumentException($"Unsupported language: {language}")
        };
    }

    public string GetRunCommandForLanguage(string language)
    {
        return language.ToLower() switch
        {
            "python" => "python solution.py",
            "javascript" => "node solution.js",
            "typescript" => "bash -c 'npm install typescript && npx tsc solution.ts --outDir dist --target ES2020 --module CommonJS && node dist/solution.js'",
            "kotlin" => "bash -c 'kotlinc Solution.kt -include-runtime -d Solution.jar && java -jar Solution.jar'",
            "java" => "bash -c 'javac Solution.java && java Solution'",
            "cpp" => "bash -c 'g++ -o solution solution.cpp && ./solution'",
            "go" => "go run main.go",
            _ => throw new ArgumentException($"Unsupported language: {language}")
        };
    }
}