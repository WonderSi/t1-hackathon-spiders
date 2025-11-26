import apiClient from '../index';
import type { GenerateTaskRequest, TaskResponse } from '@/types/types-api';

export const tasksApi = {
  // Task generation (via LLM or from a script)
  async generate(request: GenerateTaskRequest): Promise<TaskResponse> {
    const { data } = await apiClient.post<TaskResponse>(
      '/api/llm/generate-task',
      request
    );
    return data;
  },
};
