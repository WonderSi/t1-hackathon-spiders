import { ref, type Ref } from 'vue';
import { scenariosApi } from '@/api/endpoints';
import type { Scenario } from '@/types/types-api';

/**
 * Composable для работы со сценариями интервью
 * 
 * @description
 * Управляет загрузкой и кешированием сценариев из API.
 * Предоставляет методы для получения списка сценариев и загрузки конкретного.
 * 
 * @example
 * ```
 * const { scenarios, isLoading, error, fetchScenarios } = useScenarios();
 * 
 * onMounted(async () => {
 *   await fetchScenarios();
 * });
 * ```
 * 
 * @returns {Object} Объект с реактивным состоянием и методами
 */
export const useScenarios = () => {
  // ============ STATE ============
  
  // Список всех загруженных сценариев
  const scenarios: Ref<Scenario[]> = ref([]);

  // Текущий выбранный сценарий с полными данными
  const currentScenario: Ref<Scenario | null> = ref(null);

  // Флаг загрузки данных
  const isLoading = ref(false);

  // Объект ошибки (если произошла)
  const error: Ref<Error | null> = ref(null);

  // ============ ACTIONS ============

  /**
   * Получить список всех доступных сценариев
   * 
   * @async
   * @description
   * Отправляет GET запрос на /api/llm/scenarios
   * При успехе обновляет scenarios.value
   * При ошибке устанавливает error.value
   * 
   * @returns {Promise<void>}
   * 
   * @example
   * ```
   * try {
   *   await fetchScenarios();
   *   console.log('Загружено сценариев:', scenarios.value.length);
   * } catch (err) {
   *   console.error('Не удалось загрузить:', error.value);
   * }
   * ```
   */
  const fetchScenarios = async (): Promise<void> => {
    isLoading.value = true;
    error.value = null;

    try {
      scenarios.value = await scenariosApi.getAll();
      console.log(`Загружено ${scenarios.value.length} сценариев`);
    } catch (err) {
      error.value = err as Error;
      console.error('Ошибка загрузки сценариев:', err);
      throw err; // Пробрасываем для обработки в компоненте
    } finally {
      isLoading.value = false;
    }
  };

  /**
   * Загрузить конкретный сценарий по ID
   * 
   * @async
   * @param {string} scenarioId - ID сценария для загрузки
   * @returns {Promise<Scenario | null>} Загруженный сценарий или null при ошибке
   * 
   * @example
   * ```
   * const scenario = await loadScenario('scenario-1');
   * if (scenario) {
   *   console.log('Задач в сценарии:', scenario.tasks.length);
   * }
   * ```
   */
  const loadScenario = async (scenarioId: string): Promise<Scenario | null> => {
    if (!scenarioId || scenarioId.trim() === '') {
      console.warn('Пустой scenarioId передан в loadScenario');
      return null;
    }

    isLoading.value = true;
    error.value = null;

    try {
      currentScenario.value = await scenariosApi.loadById(scenarioId);
      console.log(`Сценарий "${currentScenario.value.name}" загружен`);
      return currentScenario.value;
    } catch (err) {
      error.value = err as Error;
      console.error('Ошибка загрузки сценария:', err);
      return null;
    } finally {
      isLoading.value = false;
    }
  };

  /**
   * Найти сценарий в уже загруженном списке (без запроса к API)
   * 
   * @param {string} id - ID сценария
   * @returns {Scenario | undefined}
   * 
   * @example
   * ```
   * const scenario = findScenarioById('scenario-1');
   * if (!scenario) {
   *   await loadScenario('scenario-1'); // Если не найден, загружаем
   * }
   * ```
   */
  const findScenarioById = (id: string): Scenario | undefined => {
    return scenarios.value.find(s => s.id === id);
  };

  /**
   * Фильтровать сценарии по роли
   * 
   * @param {string} role - Роль (Frontend, Backend, AI)
   * @returns {Scenario[]} Отфильтрованный массив сценариев
   * 
   * @example
   * ```
   * const backendScenarios = filterByRole('Backend');
   * ```
   */
  const filterByRole = (role: string): Scenario[] => {
    return scenarios.value.filter(s => s.role === role);
  };

  /**
   * Фильтровать сценарии по языку программирования
   * 
   * @param {string} language - Язык программирования (C#, Python, etc.)
   * @returns {Scenario[]}
   */
  const filterByLanguage = (language: string): Scenario[] => {
    return scenarios.value.filter(s => 
      s.programmingLanguage.toLowerCase() === language.toLowerCase()
    );
  };

  /**
   * Очистить текущий выбранный сценарий
   * 
   * @returns {void}
   */
  const clearCurrentScenario = (): void => {
    currentScenario.value = null;
  };

  /**
   * Очистить все данные (полный сброс)
   * 
   * @returns {void}
   */
  const reset = (): void => {
    scenarios.value = [];
    currentScenario.value = null;
    error.value = null;
    isLoading.value = false;
  };

  // ============ RETURN ============
  
  return {
    // State
    scenarios,
    currentScenario,
    isLoading,
    error,

    // Actions
    fetchScenarios,
    loadScenario,
    findScenarioById,
    filterByRole,
    filterByLanguage,
    clearCurrentScenario,
    reset,
  };
};
