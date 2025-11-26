// Базовые типы
export type SkillLevel = 'Junior' | 'Middle' | 'Senior';
export type Performance = 'Correct' | 'Incorrect' | 'Partial';
export type Role = 'Frontend' | 'Backend' | 'AI';



// Task типы
export interface Task {
  id: string;
  subject: string;
  difficulty: number;
  description: string;
  exampleInput: string;
  exampleOutput: string;
}



// Scenario типы
export interface Scenario {
  id: string;
  name: string;
  role: Role;
  programmingLanguage: string;
  tasks: Task[];
}



// Request типы
export interface LoadScenarioRequest {
  scenarioId: string;
}

export interface GenerateTaskLLMRequest {
  generateTask: true;
  skillLevel: SkillLevel;
  programmingLanguage: string;
  subject: string;
  currentDifficulty: number;
  previousPerformance?: Performance;
}

export interface GenerateTaskScenarioRequest {
  generateTask: false;
  scenarioId: string;
  taskId?: string;
  programmingLanguage: string;
  currentDifficulty: number;
}

export type GenerateTaskRequest = GenerateTaskLLMRequest | GenerateTaskScenarioRequest;

export interface AssessSolutionRequest {
  taskId: string;
  taskDescription: string;
  solution: string;
  language: string;
}

export interface CalculateAdaptiveDifficultyRequest {
  sessionId: string;
  currentTaskId: string;
  submittedSolution: string;
  score: number;
  performance: Performance;
  currentDifficulty: number;
}

export interface DetermineGradeRequest {
  sessionId: string;
}

export interface DetectPlagiarismRequest {
  taskId: string;
  code: string;
}



// Response типы
export interface TaskResponse {
  taskId: string;
  description: string;
  exampleInput: string;
  exampleOutput: string;
  estimatedDifficulty: number;
  subject: string;
}

export interface AssessmentResponse {
  score: number;
}

export interface AdaptiveDifficultyResponse {
  newDifficulty: number;
}

export interface GradeResponse {
  grade: SkillLevel;
}

export interface PlagiarismResponse {
  isPlagiarized: boolean;
}
