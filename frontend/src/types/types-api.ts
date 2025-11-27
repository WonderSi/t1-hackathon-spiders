// Уровень сложности задачи
export type SkillLevel = 'Junior' | 'Middle' | 'Senior';
// Грейд кандидата (включая Intern)
export type CandidateGrade = 'Intern' | 'Junior' | 'Middle' | 'Senior';
// Результат выполнения
export type Performance = 'Correct' | 'Partially' | 'Incorrect';
// Роли
export type Role = 'Frontend' | 'Backend' | 'AI';
// Темы задач
export type Subject = 
  | 'Algorithms' 
  | 'OOP' 
  | 'Data Structures' 
  | 'Design Patterns'
  | 'Databases'
  | 'System Design'
  | 'Testing';

// ============ TASK ============

// Объект задачи
export interface Task {
  id: string;
  subject: Subject;
  difficulty: number;
  description: string;
  exampleInput: string;
  exampleOutput: string;
}

// Ответ с сгенерированной задачей
export interface TaskResponse {
  taskId: string;
  description: string;
  exampleInput: string;
  exampleOutput: string;
  estimatedDifficulty: number;
  subject: Subject;
}

// ============ SCENARIO ============

// Scenario типы
export interface Scenario {
  id: string;
  name: string;
  role: Role;
  programmingLanguage: string;
  tasks: Task[];
}

export interface LoadScenarioRequest {
  scenarioId: string;
}

// ============ GENERATE TASK ============

// Генерация задачи через LLM
export interface GenerateTaskLLMRequest {
  generateTask: true;
  skillLevel: SkillLevel;
  programmingLanguage: string;
  subject: Subject;
  currentDifficulty: number;
  previousPerformance?: Performance;
}

// Получение задачи из сценария
export interface GenerateTaskScenarioRequest {
  generateTask: false;
  scenarioId: string;
  taskId?: string;
  programmingLanguage: string;
  currentDifficulty: number;
}

// Универсальный тип запроса генерации задачи
export type GenerateTaskRequest = GenerateTaskLLMRequest | GenerateTaskScenarioRequest;

// ============ ASSESSMENT ============

// Запрос на оценку решения задачи
export interface AssessSolutionRequest {
  taskId: string;
  taskDescription: string;
  solution: string;
  language: string;
}

// Ответ с оценкой решения
export interface AssessmentResponse {
  score: number;
}

// ============ ADAPTIVE DIFFICULTY ============

// Запрос на расчёт новой сложности
export interface CalculateAdaptiveDifficultyRequest {
  sessionId: string;
  currentTaskId: string;
  submittedSolution: string;
  score: number;
  performance: Performance;
  currentDifficulty: number;
}

// Ответ с новым уровнем сложности
export interface AdaptiveDifficultyResponse {
  newDifficulty: number;
}

// ============ GRADE DETERMINATION ============

// Запрос на определение итогового грейда
export interface DetermineGradeRequest {
  sessionId: string;
}

// Ответ с итоговым грейдом кандидата
export interface GradeResponse {
  grade: CandidateGrade;
}

// ============ PLAGIARISM DETECTION ============

// Запрос на проверку плагиата
export interface DetectPlagiarismRequest {
  taskId: string;
  code: string;
}

// Ответ с результатом проверки плагиата
export interface PlagiarismResponse {
  isPlagiarized: boolean;
}

// ============ SUBMIT ============

export interface SubmitSolutionRequest {
  sessionId: string;
  taskId: string;
  taskDescription: string;
  solution: string;
  language: string;
  taskDifficulty: number;  
}

export interface SubmitSolutionResponse {
  score: number;
  newDifficulty: number;
  grade: CandidateGrade;
}
