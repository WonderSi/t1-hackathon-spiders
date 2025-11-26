import apiClient from '../index';
import type {
  AssessSolutionRequest,
  AssessmentResponse,
  CalculateAdaptiveDifficultyRequest,
  AdaptiveDifficultyResponse,
  DetermineGradeRequest,
  GradeResponse,
  DetectPlagiarismRequest,
  PlagiarismResponse,
} from '@/types/types-api';

export const assessmentApi = {
  // Evaluate the solution to the problem
  async assessSolution(request: AssessSolutionRequest): Promise<AssessmentResponse> {
    const { data } = await apiClient.post<AssessmentResponse>(
      '/api/llm/assess-solution',
      request
    );
    return data;
  },

  // Calculate adaptive complexity
  async calculateDifficulty(
    request: CalculateAdaptiveDifficultyRequest
  ): Promise<AdaptiveDifficultyResponse> {
    const { data } = await apiClient.post<AdaptiveDifficultyResponse>(
      '/api/llm/calculate-adaptive-difficulty',
      request
    );
    return data;
  },

  // Determine the candidate's grade
  async determineGrade(sessionId: string): Promise<GradeResponse> {
    const payload: DetermineGradeRequest = { sessionId };
    const { data } = await apiClient.post<GradeResponse>(
      '/api/llm/determine-grade',
      payload
    );
    return data;
  },

  // Check the code for plagiarism
  async detectPlagiarism(request: DetectPlagiarismRequest): Promise<PlagiarismResponse> {
    const { data } = await apiClient.post<PlagiarismResponse>(
      '/api/llm/detect-plagiarism',
      request
    );
    return data;
  },
};
