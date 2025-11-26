import apiClient from '../index';
import type { Scenario, LoadScenarioRequest } from '@/types/types-api';

export const scenariosApi = {
  // Get a list of all scenarios
  async getAll(): Promise<Scenario[]> {
    const { data } = await apiClient.get<Scenario[]>('/api/llm/scenarios');
    return data;
  },

  // Upload a specific script by ID
  async loadById(scenarioId: string): Promise<Scenario> {
    const payload: LoadScenarioRequest = { scenarioId };
    const { data } = await apiClient.post<Scenario>('/api/llm/load-scenario', payload);
    return data;
  },
};
