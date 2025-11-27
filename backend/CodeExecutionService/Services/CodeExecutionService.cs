using System.Net;
using System.Text;
using CodeExecutionService.Models;
using Docker.DotNet;
using Docker.DotNet.Models;
using SharpCompress.Common;
using SharpCompress.Writers.Tar;

namespace CodeExecutionService.Services;

public class CodeExecutionService : ICodeExecutionService
{
    private readonly DockerClient _dockerClient;
    private readonly ILogger<CodeExecutionService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IProjectTemplateService _projectTemplateService;

    public CodeExecutionService(
        ILogger<CodeExecutionService> logger,
        IConfiguration configuration,
        IProjectTemplateService projectTemplateService)
    {
        var dockerEndpoint = configuration["Docker:Endpoint"] ?? "npipe://./pipe/docker_engine";
        var uri = new Uri(dockerEndpoint);
        var config = new DockerClientConfiguration(
            endpoint: uri,
            defaultTimeout: TimeSpan.FromSeconds(int.Parse(configuration["Docker:TimeoutSeconds"] ?? "60"))
        );
        _dockerClient = config.CreateClient();
        _logger = logger;
        _configuration = configuration;
        _projectTemplateService = projectTemplateService;
    }

    private async Task EnsureImageExistsAsync(string imageName)
    {
        var filters = new Dictionary<string, IDictionary<string, bool>>
        {
            ["reference"] = new Dictionary<string, bool> { [imageName] = true }
        };

        var images = await _dockerClient.Images.ListImagesAsync(new ImagesListParameters { Filters = filters });

        if (images.Count > 0)
        {
            _logger.LogDebug("Image {ImageName} already exists locally.", imageName);
            return;
        }

        _logger.LogInformation("Image {ImageName} not found locally. Pulling...", imageName);

        try
        {
            await _dockerClient.Images.CreateImageAsync(
                new ImagesCreateParameters { FromImage = imageName },
                authConfig: null,
                new Progress<JSONMessage>(msg => _logger.LogDebug("Pulling image: {Status}", msg.Status)),
                cancellationToken: default
            );

            _logger.LogInformation("Image {ImageName} pulled successfully.", imageName);
        }
        catch (DockerApiException ex)
        {
            _logger.LogError(ex, "Failed to pull image {ImageName}", imageName);
            throw;
        }
    }

    public async Task<CodeExecutionResponse> ExecuteCodeAsync(CodeExecutionRequest request)
    {
        var language = request.Language.ToLower();
        var image = GetDockerImage(language);
        var codeFileName = GetCodeFileName(language);
        var runCommand = GetRunCommand(language);

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // 1. Сохраняем код кандидата в файл
            var codeFilePath = Path.Combine(tempDir, codeFileName);
            await File.WriteAllTextAsync(codeFilePath, request.Code);

            // 2. Создаём TAR-архив
            var tarStream = CreateTarArchive(tempDir);

            // 3. Создаём контейнер
            var createResponse = await _dockerClient.Containers.CreateContainerAsync(new CreateContainerParameters
            {
                Image = image,
                Cmd = runCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList(),
                HostConfig = new HostConfig
                {
                    Binds = new List<string> { $"{tempDir}:/app/code:ro" },
                    NanoCPUs = (long)(request.CpuQuota * 1_000_000_000),
                    Memory = request.MemoryLimitBytes,
                    NetworkMode = "none",
                    AutoRemove = true
                },
                WorkingDir = "/app/code"
            });

            var containerId = createResponse.ID;

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            await _dockerClient.Containers.StartContainerAsync(containerId, new ContainerStartParameters());

            // 4. Ждём выполнения с таймаутом
            var waitTask = _dockerClient.Containers.WaitContainerAsync(containerId);

            var logsTask = _dockerClient.Containers.GetContainerLogsAsync(containerId, new ContainerLogsParameters
            {
                ShowStdout = true,
                ShowStderr = true,
                Follow = false
            });

            var completedTask = await Task.WhenAny(waitTask, Task.Delay(request.TimeoutMs));

            stopwatch.Stop();

            if (completedTask != waitTask)
            {
                await _dockerClient.Containers.KillContainerAsync(containerId, new ContainerKillParameters());
                return new CodeExecutionResponse
                {
                    Success = false,
                    Error = "Execution timed out.",
                    ExecutionTimeMs = request.TimeoutMs
                };
            }

            var waitResponse = await waitTask;
            var logsResponse = await logsTask;

            string output = "";
            string error = "";

            using (var memoryStream = new MemoryStream())
            {
                await logsResponse.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                output = ProcessDockerStream(memoryStream);
            }

            return new CodeExecutionResponse
            {
                Success = waitResponse.StatusCode == 0,
                Output = output,
                Error = error,
                ExitCode = (int)waitResponse.StatusCode,
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                MemoryUsedBytes = 0
            };
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    private string ProcessDockerStream(Stream stream)
    {
        using var reader = new BinaryReader(stream);
        var output = new StringBuilder();

        while (true)
        {
            try
            {
                var header = reader.ReadBytes(8);

                if (header.Length < 8)
                    break; // Конец потока

                var streamType = header[0]; // 1 = stdout, 2 = stderr
                var length = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(
                    new byte[] { header[7], header[6], header[5], header[4] }, 0)); // Big-endian

                if (length <= 0)
                    continue;

                var frameBody = reader.ReadBytes(length);

                if (streamType == 1) // stdout
                {
                    output.Append(Encoding.UTF8.GetString(frameBody));
                }
                else if (streamType == 2) // stderr
                {
                    // В MVP: добавляем к output, но можно отдельно
                    output.Append(Encoding.UTF8.GetString(frameBody));
                }
            }
            catch (EndOfStreamException)
            {
                break;
            }
        }

        return output.ToString();
    }
    
    private string GetDockerImage(string language)
    {
        return language.ToLower() switch
        {
            "python" => "python:3.11-slim",
            "javascript" => "node:18-alpine",
            "typescript" => "node:18-alpine",
            "kotlin" => "eclipse-temurin:17-jdk-alpine",
            "java" => "eclipse-temurin:17-jdk-alpine",
            "cpp" => "gcc:latest",
            "go" => "golang:1.21-alpine",
            _ => throw new ArgumentException($"Unsupported language: {language}")
        };
    }
    
    private string GetCodeFileName(string language)
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

    private string GetRunCommand(string language)
    {
        return language.ToLower() switch
        {
            "python" => "python solution.py",
            "javascript" => "node solution.js",
            "typescript" => "sh -c 'npm install typescript && npx tsc solution.ts --outDir dist --target ES2020 --module CommonJS && node dist/solution.js'",
            "kotlin" => "sh -c 'kotlinc Solution.kt -include-runtime -d Solution.jar && java -jar Solution.jar'", // ✅ Запускает файл Solution.kt
            "java" => "sh -c 'javac Solution.java && java Solution'", // ✅ Запускает файл Solution.java
            "cpp" => "sh -c 'g++ -o solution solution.cpp && ./solution'", // ✅ Запускает файл solution.cpp
            "go" => "go run main.go", // ✅ Запускает файл main.go
            _ => throw new ArgumentException($"Unsupported language: {language}")
        };
    }

    private Stream CreateTarArchive(string directoryPath)
    {
        var memoryStream = new MemoryStream();

        using (var writer = new TarWriter(memoryStream, new TarWriterOptions(CompressionType.None, true)))
        {
            var dirInfo = new DirectoryInfo(directoryPath);
                
            foreach (var fileInfo in dirInfo.GetFiles())
            {
                using (var fileStream = fileInfo.OpenRead())
                {
                    writer.Write(fileInfo.Name, fileStream, fileInfo.LastWriteTimeUtc);
                }
            }
        }

        memoryStream.Position = 0;
            
        return memoryStream;
    }
}