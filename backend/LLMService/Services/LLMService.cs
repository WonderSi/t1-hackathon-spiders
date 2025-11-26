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
    private static readonly Dictionary<string, List<(float Score, float TaskDifficulty)>> _sessionScoresWithDifficulty = new();
    private static readonly Dictionary<string, List<List<double>>> _codeEmbeddings = new();

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
                new ChatMessage("system", "/no_think Ты — технический эксперт, который оценивает решения программистов по шкале от 1 до 5. Оцени правильность, эффективность и читаемость кода. Ответь ТОЛЬКО числом от 1 до 5, без объяснений, без слов, без тегов. Если код не является решением задачи, ответь 1."),
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
            
            _logger.LogInformation("LLM Assessment Raw Response: {RawResponse}", assessmentText);
            
            float score = 1.0f; // Значение по умолчанию
            bool parsed = false;
            
            var match = System.Text.RegularExpressions.Regex.Match(assessmentText, @"\b([1-5])(\.\d+)?\b");
            if (match.Success && float.TryParse(match.Value, out var extractedScore))
            {
                score = extractedScore;
                parsed = true;
            }

            if (!parsed)
            {
                _logger.LogWarning("Could not parse assessment score from LLM response: {RawResponse}", assessmentText);
                // Оставляем score = 1.0f или другое значение по умолчанию
            }

            return score;
        }

        public async Task<float> CalculateAdaptiveDifficultyAsync(AdaptiveDifficultyRequest request)
        {
            var newDifficulty = request.CurrentDifficulty;

            switch (request.Score)
            {
                case >= 4.0f:
                    newDifficulty += 0.3f; // Повысить сложность
                    break;
                case <= 2.0f:
                    newDifficulty -= 0.3f; // Понизить сложность
                    break;
            }

            // Ограничить диапазон
            newDifficulty = Math.Max(1.0f, Math.Min(5.0f, newDifficulty));

            // Сохраняем новую сложность для сессии
            _sessionDifficulty[request.SessionId] = newDifficulty;

            if (!_sessionScoresWithDifficulty.ContainsKey(request.SessionId))
                _sessionScoresWithDifficulty[request.SessionId] = new List<(float Score, float TaskDifficulty)>();

            _sessionScoresWithDifficulty[request.SessionId].Add((request.Score, request.CurrentDifficulty));

            _logger.LogInformation("Adaptive difficulty calculated: {OldDifficulty} -> {NewDifficulty} for session {SessionId}", request.CurrentDifficulty, newDifficulty, request.SessionId);

            return newDifficulty;
        }

        public async Task<string> DetermineCandidateGradeAsync(string sessionId)
        {
            if (!_sessionScoresWithDifficulty.TryGetValue(sessionId, out var scoresAndDifficulties) || scoresAndDifficulties.Count == 0)
            {
                _logger.LogWarning("No scores found for session {SessionId} when determining grade.", sessionId);
                return "Unknown";
            }

            var totalWeightedScore = 0.0f;
            var totalDifficultyWeight = 0.0f;

            foreach (var (Score, TaskDifficulty) in scoresAndDifficulties)
            {
                // Используем сложность задачи как вес для оценки
                // Например: задача сложности 4.0 и оценка 4.5 -> 4.5 * 4.0
                totalWeightedScore += Score * TaskDifficulty;
                totalDifficultyWeight += TaskDifficulty;
            }

            if (totalDifficultyWeight == 0)
            {
                _logger.LogWarning("Total difficulty weight is zero for session {SessionId}. Cannot calculate grade.", sessionId);
                return "Unknown";
            }

            var weightedAverageScore = totalWeightedScore / totalDifficultyWeight;

            _logger.LogInformation("Calculated weighted average score for session {SessionId}: {WeightedAverageScore}", sessionId, weightedAverageScore);

            return weightedAverageScore switch
            {
                >= 4.7f => "Expert",
                >= 4.0f => "Senior",
                >= 3.3f => "Senior-Junior",
                >= 2.7f => "Middle-Senior",
                >= 2.0f => "Middle",
                >= 1.3f => "Junior-Middle",
                >= 1.0f => "Junior",
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
            return _codeEmbeddings.TryGetValue(taskId, out var embeddings) ? embeddings : new List<List<double>>();
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