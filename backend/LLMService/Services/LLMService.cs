using LLMService.Models.Adaptive;
using LLMService.Models.Interview;
using LLMService.Models.Scibox;
using LLMService.Models.TaskGeneration;
using Microsoft.Extensions.Caching.Memory;

namespace LLMService.Services;

public class LLMService : ILLMService
{
   private readonly ISciboxClient _sciboxClient;
   private readonly IMemoryCache _cache;
   private readonly ILogger<LLMService> _logger;
   private readonly IConfiguration _configuration;

        // Для MVP: храним сессии адаптивности и грейды в памяти
    private static readonly Dictionary<string, float> _sessionDifficulty = new();
    private static readonly Dictionary<string, List<float>> _sessionScores = new();
    private static readonly Dictionary<string, List<List<double>>> _codeEmbeddings = new(); // taskId -> List<Embedding>

    public LLMService(ISciboxClient sciboxClient, IMemoryCache cache, ILogger<LLMService> logger, IConfiguration configuration)
    {
        _sciboxClient = sciboxClient;
        _cache = cache;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<CodingTaskResponse> GenerateCodingTaskAsync(CodingTaskRequest request)
        {
            var cacheKey = $"task_{request.SkillLevel}_{request.ProgrammingLanguage}_{request.Subject}_{request.CurrentDifficulty}";
            if (_cache.TryGetValue(cacheKey, out CodingTaskResponse cached))
            {
                _logger.LogInformation("Task retrieved from cache: {CacheKey}", cacheKey);
                return cached;
            }

            var model = _configuration["Scibox:Models:Coding"];
            var messages = new List<ChatMessage>
            {
                new ChatMessage("system", "Ты — технический интервьюер, который генерирует задачи для программистов. Сгенерируй задачу по программированию на указанном языке и уровне сложности. Включи пример ввода и вывода. Задача должна быть выполнима за 15-20 минут."),
                new ChatMessage("user", $"Сгенерируй задачу по {request.ProgrammingLanguage} для уровня {request.SkillLevel} по теме {request.Subject}. Уровень сложности: {request.CurrentDifficulty}. Предыдущий результат: {request.PreviousPerformance}.")
            };

            var sciboxRequest = new ChatCompletionRequest
            {
                Model = model,
                Messages = messages,
                Temperature = 0.7,
                MaxTokens = 500
            };

            var response = await _sciboxClient.ChatCompletionAsync(sciboxRequest);

            if (response.Choices.Count == 0 || string.IsNullOrEmpty(response.Choices[0].Message.Content))
            {
                throw new InvalidOperationException("Scibox returned empty response for task generation.");
            }

            var taskDescription = response.Choices[0].Message.Content;

            // Попробуем извлечь примеры из ответа LLM с помощью простого поиска
            var exampleInput = ExtractExample(taskDescription, "Пример ввода", "Input");
            var exampleOutput = ExtractExample(taskDescription, "Пример вывода", "Output");

            var result = new CodingTaskResponse
            {
                TaskId = Guid.NewGuid().ToString(), // Генерируем ID для задачи
                Description = taskDescription,
                ExampleInput = exampleInput,
                ExampleOutput = exampleOutput,
                EstimatedDifficulty = (float)request.CurrentDifficulty,
                Subject = request.Subject
            };

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(60));
            return result;
        }

        public async Task<float> AssessSolutionAsync(string taskDescription, string solution, string language)
        {
            var model = _configuration["Scibox:Models:General"];
            var messages = new List<ChatMessage>
            {
                new ChatMessage("system", "Ты — технический эксперт, который оценивает решения программистов по шкале от 1 до 5. Оцени правильность, эффективность и читаемость кода. Ответь только числом от 1 до 5."),
                new ChatMessage("user", $"Задача: {taskDescription}\n\nРешение на {language}:\n{solution}\n\nОцени решение по шкале от 1 до 5, только число.")
            };

            var sciboxRequest = new ChatCompletionRequest
            {
                Model = model,
                Messages = messages,
                Temperature = 0.2,
                MaxTokens = 10
            };

            var response = await _sciboxClient.ChatCompletionAsync(sciboxRequest);

            var assessmentText = response.Choices[0].Message.Content;
            // Теперь парсим только число
            if (float.TryParse(System.Text.RegularExpressions.Regex.Match(assessmentText, @"\b[1-5]\.?\d?\b").Value, out var score))
            {
                return score;
            }

            return 3.0f; // Значение по умолчанию
        }

        public async Task<float> CalculateAdaptiveDifficultyAsync(AdaptiveDifficultyRequest request)
        {
            // Логика адаптации сложности на основе предыдущего результата
            var newDifficulty = request.CurrentDifficulty;

            if (request.Score >= 4.0f)
            {
                newDifficulty += 0.3f; // Повысить сложность
            }
            else if (request.Score <= 2.0f)
            {
                newDifficulty -= 0.3f; // Понизить сложность
            }

            // Ограничить диапазон
            newDifficulty = Math.Max(1.0f, Math.Min(5.0f, newDifficulty));

            // Сохраняем новую сложность для сессии
            _sessionDifficulty[request.SessionId] = newDifficulty;

            // Сохраняем оценку для определения грейда
            if (!_sessionScores.ContainsKey(request.SessionId))
                _sessionScores[request.SessionId] = new List<float>();
            _sessionScores[request.SessionId].Add(request.Score);

            _logger.LogInformation("Adaptive difficulty calculated: {OldDifficulty} -> {NewDifficulty} for session {SessionId}", request.CurrentDifficulty, newDifficulty, request.SessionId);

            return newDifficulty;
        }

        public async Task<string> DetermineCandidateGradeAsync(string sessionId)
        {
            if (!_sessionScores.TryGetValue(sessionId, out var scores) || scores.Count == 0)
                return "Unknown";

            var averageScore = scores.Average();

            return averageScore switch
            {
                >= (float)4.0 => "Senior",
                >= (float)3.0 => "Middle",
                >= (float)2.0 => "Junior",
                _ => "Intern"
            };
        }

        public async Task<bool> DetectPlagiarismAsync(string code, string taskId)
        {
            var model = _configuration["Scibox:Models:Embedding"];
            var embeddingRequest = new EmbeddingRequest
            {
                Model = model,
                Input = code
            };

            var embeddingResponse = await _sciboxClient.EmbeddingAsync(embeddingRequest);

            if (embeddingResponse.Data.Count == 0)
                return false;

            var currentEmbedding = embeddingResponse.Data[0].Embedding;

            // Получаем известные эмбеддинги для этой задачи
            var knownEmbeddings = await GetKnownEmbeddingsForTask(taskId);

            foreach (var knownEmbedding in knownEmbeddings)
            {
                var similarity = CalculateCosineSimilarity(currentEmbedding, knownEmbedding);
                if (similarity > 0.85) // Порог схожести
                {
                    _logger.LogWarning("Plagiarism detected for task {TaskId} (similarity: {Similarity})", taskId, similarity);
                    return true;
                }
            }

            // Если плагиат не найден, сохраняем текущий эмбеддинг для будущих проверок
            await StoreCodeEmbedding(taskId, currentEmbedding);

            return false;
        }

        // --- Вспомогательные методы ---

        private string ExtractExample(string text, params string[] keywords)
        {
            foreach (var keyword in keywords)
            {
                var start = text.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
                if (start != -1)
                {
                    var end = text.IndexOf('\n', start);
                    if (end == -1) end = text.Length;
                    return text.Substring(start, end - start).Replace(keyword, "").Trim();
                }
            }
            return string.Empty;
        }

        private async Task<List<List<double>>> GetKnownEmbeddingsForTask(string taskId)
        {
            // Для MVP: возвращаем список из памяти
            if (_codeEmbeddings.TryGetValue(taskId, out var embeddings))
                return embeddings;
            return new List<List<double>>();
        }

        private async Task StoreCodeEmbedding(string taskId, List<double> embedding)
        {
            // Для MVP: сохраняем в память
            if (!_codeEmbeddings.ContainsKey(taskId))
                _codeEmbeddings[taskId] = new List<List<double>>();
            
            _codeEmbeddings[taskId].Add(embedding);
        }

        private double CalculateCosineSimilarity(List<double> a, List<double> b)
        {
            if (a.Count != b.Count) return 0.0;

            double dotProduct = 0.0, magnitudeA = 0.0, magnitudeB = 0.0;
            for (int i = 0; i < a.Count; i++)
            {
                dotProduct += a[i] * b[i];
                magnitudeA += Math.Pow(a[i], 2);
                magnitudeB += Math.Pow(b[i], 2);
            }

            magnitudeA = Math.Sqrt(magnitudeA);
            magnitudeB = Math.Sqrt(magnitudeB);

            if (magnitudeA == 0 || magnitudeB == 0) return 0.0;

            return dotProduct / (magnitudeA * magnitudeB);
        }
}