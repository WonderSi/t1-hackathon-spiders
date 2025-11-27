using System.Text.Json.Serialization;

namespace CodeExecutionService.Models;

public class CodeExecutionRequest
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("language")]
    public string Language { get; set; } = "C#"; // "C#", "Python", "JavaScript"

    [JsonPropertyName("input")]
    public string Input { get; set; } = string.Empty; // stdin для программы

    [JsonPropertyName("timeoutMs")]
    public int TimeoutMs { get; set; } = 5000; // 5 секунд

    [JsonPropertyName("cpuQuota")]
    public double CpuQuota { get; set; } = 20000; // 20% от 1 ядра (100000)

    [JsonPropertyName("memoryLimitBytes")]
    public long MemoryLimitBytes { get; set; } = 256 * 1024 * 1024; // 256 MB
}