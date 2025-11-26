import { ref, type Ref } from 'vue';
import { tasksApi } from '@/api/endpoints';
import type { 
  TaskResponse, 
  GenerateTaskRequest,
  GenerateTaskLLMRequest,
  GenerateTaskScenarioRequest,
} from '@/types/types-api';

  // ============ GLOBAL STATE (SINGLETON) ============

  // Текущая активная задача
  const currentTask: Ref<TaskResponse | null> = ref(null);
  // История всех сгенерированных задач в текущей сессии
  const taskHistory: Ref<TaskResponse[]> = ref([]);
  // Флаг процесса генерации задачи
  const isGenerating = ref(false);
  // Объект ошибки
  const error: Ref<Error | null> = ref(null);

/**
 * Composable для генерации и управления задачами
 * 
 * @description
 * Предоставляет методы для генерации задач через LLM или из сценария.
 * Ведёт историю всех сгенерированных задач.
 * 
 * @example
 * ```
 * const { currentTask, generateLLMTask } = useTasks();
 * 
 * await generateLLMTask({
 *   skillLevel: 'Middle',
 *   programmingLanguage: 'C#',
 *   subject: 'Algorithms',
 *   currentDifficulty: 2.5
 * });
 * ```
 */
export const useTasks = () => {
  // ============ ACTIONS ============

  /**
   * Универсальный метод генерации задачи
   * 
   * @async
   * @private
   * @param {GenerateTaskRequest} request - Параметры генерации
   * @returns {Promise<TaskResponse | null>}
   */
  const generateTask = async (
    request: GenerateTaskRequest
  ): Promise<TaskResponse | null> => {
    isGenerating.value = true;
    error.value = null;

    try {
      const task = await tasksApi.generate(request);
      currentTask.value = task;

      // Добавляем в историю
      taskHistory.value.push(task);

      const mode = request.generateTask ? 'LLM' : 'сценария';
      console.log(`Задача сгенерирована через ${mode}:`, task.subject);

      return task;
    } catch (err) {
      error.value = err as Error;
      console.error('Ошибка генерации задачи:', err);
      return null;
    } finally {
      isGenerating.value = false;
    }
  };

  /**
   * Сгенерировать задачу через LLM (адаптивная сложность)
   * 
   * @async
   * @param {Omit<GenerateTaskLLMRequest, 'generateTask'>} params - Параметры для LLM
   * @returns {Promise<TaskResponse | null>}
   * 
   * @example
   * ```
   * const task = await generateLLMTask({
   *   skillLevel: 'Senior',
   *   programmingLanguage: 'TypeScript',
   *   subject: 'Design Patterns',
   *   currentDifficulty: 4.0,
   *   previousPerformance: 'Correct'
   * });
   * ```
   */
  const generateLLMTask = async (
    params: Omit<GenerateTaskLLMRequest, 'generateTask'>
  ): Promise<TaskResponse | null> => {
    const request: GenerateTaskLLMRequest = {
      generateTask: true,
      ...params,
    };
    return generateTask(request);
  };

  /**
   * Получить задачу из предзаготовленного сценария
   * 
   * @async
   * @param {string} scenarioId - ID сценария
   * @param {string} taskId - ID задачи в сценарии (опционально)
   * @param {string} programmingLanguage - Язык программирования
   * @param {number} currentDifficulty - Текущий уровень сложности
   * @returns {Promise<TaskResponse | null>}
   * 
   * @example
   * ```
   * const task = await getScenarioTask(
   *   'scenario-1',
   *   'task-5',
   *   'Python',
   *   3.0
   * );
   * ```
   */
  const getScenarioTask = async (
    scenarioId: string,
    taskId: string,
    programmingLanguage: string,
    currentDifficulty: number
  ): Promise<TaskResponse | null> => {
    if (!scenarioId) {
      console.error('scenarioId обязателен');
      return null;
    }

    const request: GenerateTaskScenarioRequest = {
      generateTask: false,
      scenarioId,
      taskId,
      programmingLanguage,
      currentDifficulty,
    };
    return generateTask(request);
  };

  /**
   * Получить следующую задачу из сценария (без указания taskId)
   * Бэкенд сам выберет задачу на основе currentDifficulty
   * 
   * @async
   * @param {string} scenarioId
   * @param {string} programmingLanguage
   * @param {number} currentDifficulty
   * @returns {Promise<TaskResponse | null>}
   */
  const getNextScenarioTask = async (
    scenarioId: string,
    programmingLanguage: string,
    currentDifficulty: number
  ): Promise<TaskResponse | null> => {
    const request: GenerateTaskScenarioRequest = {
      generateTask: false,
      scenarioId,
      programmingLanguage,
      currentDifficulty,
    };
    return generateTask(request);
  };

  /**
   * Очистить текущую задачу
   * 
   * @returns {void}
   */
  const clearCurrentTask = (): void => {
    currentTask.value = null;
  };

  /**
   * Очистить всю историю задач
   * 
   * @returns {void}
   */
  const clearHistory = (): void => {
    taskHistory.value = [];
    currentTask.value = null;
  };

  /**
   * Получить количество решённых задач
   * 
   * @returns {number}
   */
  const getCompletedTasksCount = (): number => {
    return taskHistory.value.length;
  };

  /**
   * Получить последнюю задачу из истории
   * 
   * @returns {TaskResponse | null}
   */
  const getLastTask = (): TaskResponse | null => {
    return taskHistory.value[taskHistory.value.length - 1] || null;
  };

  /**
   * Полный сброс composable
   * 
   * @returns {void}
   */
  const reset = (): void => {
    currentTask.value = null;
    taskHistory.value = [];
    error.value = null;
    isGenerating.value = false;
  };

  // ============ RETURN ============

  return {
    // State
    currentTask,
    taskHistory,
    isGenerating,
    error,

    // Actions
    generateLLMTask,
    getScenarioTask,
    getNextScenarioTask,
    clearCurrentTask,
    clearHistory,
    getCompletedTasksCount,
    getLastTask,
    reset,
  };
};
