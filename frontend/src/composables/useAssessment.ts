import { ref, type Ref } from 'vue';
import { assessmentApi } from '@/api/endpoints';
import type {
  AssessSolutionRequest,
  AssessmentResponse,
  DetectPlagiarismRequest,
  PlagiarismResponse,
} from '@/types/types-api';

/**
 * Composable для оценки решений и проверки плагиата
 * 
 * @description
 * Управляет оценкой пользовательских решений и проверкой кода на плагиат.
 * Предоставляет методы для параллельного выполнения обеих проверок.
 * 
 * @example
 * ```
 * const { assessSolution, checkPlagiarism } = useAssessment();
 * 
 * const score = await assessSolution(
 *   'task-123',
 *   'Write a function...',
 *   'public int Add(...) { ... }',
 *   'C#'
 * );
 * ```
 */
export const useAssessment = () => {
  // ============ STATE ============

  // Оценка последнего решения (от 0 до 5)
  const lastScore: Ref<number | null> = ref(null);

  // Результат проверки на плагиат
  const isPlagiarized: Ref<boolean | null> = ref(null);

  // Флаг процесса оценки
  const isAssessing = ref(false);

  // Флаг процесса проверки плагиата
  const isCheckingPlagiarism = ref(false);

  // Объект ошибки
  const error: Ref<Error | null> = ref(null);

  // ============ ACTIONS ============

  /**
   * Оценить решение задачи
   * 
   * @async
   * @param {string} taskId - ID задачи
   * @param {string} taskDescription - Описание задачи
   * @param {string} solution - Код решения пользователя
   * @param {string} language - Язык программирования (C#, Python, etc.)
   * @returns {Promise<number | null>} Оценка от 0 до 5 или null при ошибке
   * 
   * @example
   * ```
   * const score = await assessSolution(
   *   'task-abc',
   *   'Reverse a string',
   *   'public string Reverse(string s) { ... }',
   *   'C#'
   * );
   * 
   * if (score !== null && score >= 4.0) {
   *   console.log('Отлично!');
   * }
   * ```
   */
  const assessSolution = async (
    taskId: string,
    taskDescription: string,
    solution: string,
    language: string
  ): Promise<number | null> => {
    // Валидация входных данных
    if (!taskId || !solution || !language) {
      console.error('Обязательные параметры отсутствуют');
      return null;
    }

    isAssessing.value = true;
    error.value = null;

    try {
      const request: AssessSolutionRequest = {
        taskId,
        taskDescription,
        solution,
        language,
      };

      const response: AssessmentResponse = await assessmentApi.assessSolution(request);
      lastScore.value = response.score;

      console.log(`Оценка получена: ${response.score}/5.0`);
      return response.score;
    } catch (err) {
      error.value = err as Error;
      console.error('Ошибка оценки решения:', err);
      return null;
    } finally {
      isAssessing.value = false;
    }
  };

  /**
   * Проверить код на плагиат
   * 
   * @async
   * @param {string} taskId - ID задачи
   * @param {string} code - Код для проверки
   * @returns {Promise<boolean | null>} true если плагиат, false если нет, null при ошибке
   * 
   * @example
   * ```
   * const isPlagiarism = await checkPlagiarism('task-1', userCode);
   * if (isPlagiarism) {
   *   alert('Обнаружен плагиат!');
   * }
   * ```
   */
  const checkPlagiarism = async (
    taskId: string,
    code: string
  ): Promise<boolean | null> => {
    if (!taskId || !code) {
      console.error('taskId и code обязательны');
      return null;
    }

    isCheckingPlagiarism.value = true;
    error.value = null;

    try {
      const request: DetectPlagiarismRequest = {
        taskId,
        code,
      };

      const response: PlagiarismResponse = await assessmentApi.detectPlagiarism(request);
      isPlagiarized.value = response.isPlagiarized;

      const status = response.isPlagiarized ? 'Плагиат' : 'Оригинальный код';
      console.log(status);

      return response.isPlagiarized;
    } catch (err) {
      error.value = err as Error;
      console.error('Ошибка проверки плагиата:', err);
      return null;
    } finally {
      isCheckingPlagiarism.value = false;
    }
  };

  /**
   * Комбинированный метод: оценить решение + проверить плагиат
   * Запускает обе проверки параллельно для ускорения
   * 
   * @async
   * @param {string} taskId
   * @param {string} taskDescription
   * @param {string} solution
   * @param {string} language
   * @returns {Promise<{ score: number | null; isPlagiarized: boolean | null }>}
   * 
   * @example
   * ```
   * const { score, isPlagiarized } = await assessAndCheckPlagiarism(
   *   'task-1',
   *   'Implement binary search',
   *   userCode,
   *   'Python'
   * );
   * 
   * if (isPlagiarized) {
   *   console.log('Плагиат обнаружен');
   * } else if (score && score >= 4) {
   *   console.log('Отличное решение!');
   * }
   * ```
   */
  const assessAndCheckPlagiarism = async (
    taskId: string,
    taskDescription: string,
    solution: string,
    language: string
  ): Promise<{ score: number | null; isPlagiarized: boolean | null }> => {
    console.log('Запуск параллельных проверок...');

    // Запускаем обе проверки одновременно
    const [score, plagiarism] = await Promise.all([
      assessSolution(taskId, taskDescription, solution, language),
      checkPlagiarism(taskId, solution),
    ]);

    return { 
      score, 
      isPlagiarized: plagiarism 
    };
  };

  /**
   * Сбросить результаты оценки
   * 
   * @returns {void}
   */
  const resetAssessment = (): void => {
    lastScore.value = null;
    isPlagiarized.value = null;
    error.value = null;
  };

  /**
   * Полный сброс composable
   * 
   * @returns {void}
   */
  const reset = (): void => {
    lastScore.value = null;
    isPlagiarized.value = null;
    error.value = null;
    isAssessing.value = false;
    isCheckingPlagiarism.value = false;
  };

  // ============ RETURN ============

  return {
    // State
    lastScore,
    isPlagiarized,
    isAssessing,
    isCheckingPlagiarism,
    error,

    // Actions
    assessSolution,
    checkPlagiarism,
    assessAndCheckPlagiarism,
    resetAssessment,
    reset,
  };
};
