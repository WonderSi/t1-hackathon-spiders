import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { assessmentApi } from '@/api/endpoints';
import type { 
  SkillLevel, 
  Performance,
  CandidateGrade,
  TaskResponse,
} from '@/types/types-api';

// История решения одной задачи
interface TaskAttempt {
  // ID задачи
  taskId: string;
  // Markdown-описание задачи
  taskDescription: string;
  // Код решения пользователя
  solution: string;
  // Оценка от 0 до 5
  score: number;
  // Результат: Correct | Partial | Incorrect
  performance: Performance;
  // Уровень сложности на момент решения
  difficulty: number;
  // Время отправки решения
  timestamp: Date;
}

/**
 * Pinia Store для управления сессией интервью
 * 
 * @description
 * Глобальное хранилище для:
 * - Отслеживания прогресса интервью
 * - Хранения истории всех решений
 * - Адаптации сложности задач
 * - Определения итогового грейда
 * 
 * @example
 * ```
 * import { useAssessmentStore } from '@/stores/assessment';
 * 
 * const store = useAssessmentStore();
 * store.startSession('C#', 'Algorithms');
 * ```
 */
export const useAssessmentStore = defineStore('assessment', () => {
  // ============ STATE ============

  // Уникальный ID текущей сессии (UUID v4)
  const sessionId = ref<string>('');

  // Текущий уровень сложности (от 1.0 до 5.0)
  const currentDifficulty = ref<number>(2.0);

  // История всех попыток решения задач
  const taskAttempts = ref<TaskAttempt[]>([]);

  // Итоговый грейд кандидата (определяется в конце)
  const finalGrade = ref<CandidateGrade | null>(null);

  // Выбранный язык программирования
  const programmingLanguage = ref<string>('C#');

  // Выбранная тема (Algorithms, Data Structures, etc.)
  const selectedSubject = ref<string>('Algorithms');

  // Режим интервью: 'llm' (адаптивный) или 'scenario' (фиксированный)
  const interviewMode = ref<'llm' | 'scenario'>('llm');

  // ID выбранного сценария (если режим = 'scenario')
  const selectedScenarioId = ref<string | null>(null);

  const skillLevel = ref<SkillLevel | null>(null);

  const timerStartTimestamp = ref<number | null>(null)

  // ============ GETTERS ============

  // Общее количество решённых задач
  const totalTasks = computed(() => taskAttempts.value.length);

  // Средний балл по всем задачам
  const averageScore = computed(() => {
    if (taskAttempts.value.length === 0) return 0;

    const sum = taskAttempts.value.reduce((acc, attempt) => acc + attempt.score, 0);
    return Number((sum / taskAttempts.value.length).toFixed(2));
  });

  // Количество правильных решений (score >= 4.0)
  const correctSolutions = computed(() => {
    return taskAttempts.value.filter(a => a.performance === 'Correct').length;
  });

  // Количество частично правильных решений (2.0 <= score < 4.0)
  const partialSolutions = computed(() => {
    return taskAttempts.value.filter(a => a.performance === 'Partially').length;
  });

  // Количество неправильных решений (score < 2.0)
  const incorrectSolutions = computed(() => {
    return taskAttempts.value.filter(a => a.performance === 'Incorrect').length;
  });

  // Процент правильных ответов
  const successRate = computed(() => {
    if (totalTasks.value === 0) return 0;
    return Number(((correctSolutions.value / totalTasks.value) * 100).toFixed(1));
  });

  // Последняя решённая задача
  const lastAttempt = computed(() => {
    return taskAttempts.value[taskAttempts.value.length - 1] || null;
  });

  // Есть ли активная сессия
  const hasActiveSession = computed(() => {
    return sessionId.value !== '';
  });

  /**
   * Прогресс в процентах (для UI прогресс-бара)
   * Предположим, что максимум 10 задач
   * @returns {number} От 0 до 100
   */
  const progressPercentage = computed(() => {
    const maxTasks = 10;
    return Math.min((totalTasks.value / maxTasks) * 100, 100);
  });

  // ============ ACTIONS ============

  /**
   * Начать новую сессию интервью
   * 
   * @param {string} language - Язык программирования (C#, Python, etc.)
   * @param {string} subject - Тема (Algorithms, Design Patterns, etc.)
   * @param {'llm' | 'scenario'} mode - Режим интервью
   * @param {string | null} scenarioId - ID сценария (если mode = 'scenario')
   * 
   * @example
   * ```
   * store.startSession('TypeScript', 'Design Patterns', 'llm');
   * ```
   */
  const startSession = (
    language: string,
    subject: string,
    skillLevelParam: SkillLevel,
    mode: 'llm' | 'scenario' = 'llm',
    scenarioId: string | null = null
  ): void => {
    // Генерируем UUID v4 для сессии
    sessionId.value = crypto.randomUUID();
    programmingLanguage.value = language;
    selectedSubject.value = subject;
    skillLevel.value = skillLevelParam;
    interviewMode.value = mode;
    selectedScenarioId.value = scenarioId;
    currentDifficulty.value = 2.0; // Начальная сложность
    taskAttempts.value = [];
    finalGrade.value = null;

    console.log(`Сессия начата: ${sessionId.value}`);
    console.log(`Язык: ${language}, Тема: ${subject}, Режим: ${mode}`);

    // Сохраняем в localStorage для восстановления после перезагрузки
    startTimer()
    saveSessionToLocalStorage();
  };

  /**
   * Сохранить результат решения задачи
   * 
   * @param {string} taskId - ID задачи
   * @param {string} taskDescription - Описание задачи
   * @param {string} solution - Код решения
   * @param {number} score - Оценка (0-5)
   * @param {Performance} performance - Результат (Correct/Partial/Incorrect)
   * 
   * @example
   * ```
   * store.saveTaskAttempt(
   *   'task-123',
   *   'Write a binary search function',
   *   userCode,
   *   4.5,
   *   'Correct'
   * );
   * ```
   */
  const saveTaskAttempt = (
    taskId: string,
    taskDescription: string,
    solution: string,
    score: number,
    performance: Performance
  ): void => {
    const attempt: TaskAttempt = {
      taskId,
      taskDescription,
      solution,
      score,
      performance,
      difficulty: currentDifficulty.value,
      timestamp: new Date(),
    };

    taskAttempts.value.push(attempt);
    console.log(`Сохранено решение: ${performance} (${score}/5.0)`);

    // Обновляем localStorage
    saveSessionToLocalStorage();
  };

  /**
   * Рассчитать и обновить сложность на основе результата
   * 
   * @async
   * @param {string} taskId - ID задачи
   * @param {string} solution - Код решения
   * @param {number} score - Оценка
   * @param {Performance} performance - Результат
   * @returns {Promise<number>} Новый уровень сложности
   * 
   * @example
   * ```
   * const newDifficulty = await store.updateDifficulty(
   *   'task-1',
   *   userCode,
   *   4.5,
   *   'Correct'
   * );
   * console.log('Новая сложность:', newDifficulty);
   * ```
   */
  const updateDifficulty = async (
    taskId: string,
    solution: string,
    score: number,
    performance: Performance
  ): Promise<number> => {
    if (!sessionId.value) {
      console.error('Нет активной сессии');
      return currentDifficulty.value;
    }

    try {
      const response = await assessmentApi.calculateDifficulty({
        sessionId: sessionId.value,
        currentTaskId: taskId,
        submittedSolution: solution,
        score,
        performance,
        currentDifficulty: currentDifficulty.value,
      });

      const oldDifficulty = currentDifficulty.value;
      currentDifficulty.value = response.newDifficulty;

      const direction = response.newDifficulty > oldDifficulty ? '📈' : '📉';
      console.log(
        `${direction} Сложность: ${oldDifficulty.toFixed(1)} → ${response.newDifficulty.toFixed(1)}`
      );

      saveSessionToLocalStorage();
      return response.newDifficulty;
    } catch (err) {
      console.error('Ошибка обновления сложности:', err);
      return currentDifficulty.value;
    }
  };

/**
 * Определить итоговый грейд кандидата
 * 
 * @async
 * @returns {Promise<CandidateGrade | null>}
 * 
 * @example
 * ```
 * const grade = await store.determineFinalGrade();
 * if (grade) {
 *   alert(`Ваш уровень: ${grade}`);
 * }
 * ```
 */
const determineFinalGrade = async (): Promise<CandidateGrade | null> => {
  if (!sessionId.value) {
    console.error('Нет активной сессии');
    return null;
  }

  try {
    const response = await assessmentApi.determineGrade(sessionId.value);
    finalGrade.value = response.grade;

    console.log(`Итоговый грейд: ${response.grade}`);
    saveSessionToLocalStorage();

    return response.grade;
  } catch (err) {
    console.error('Ошибка определения грейда:', err);
    return null;
  }
};


  // Завершить сессию и сохранить в историю
  const endSession = (): void => {
    if (!sessionId.value) {
      console.warn('Нет активной сессии для завершения');
      stopTimer()
      return;
    }

    console.log(`Сессия завершена: ${sessionId.value}`);

    // Сохраняем в localStorage как завершённую
    const sessionData = {
      sessionId: sessionId.value,
      attempts: taskAttempts.value,
      finalGrade: finalGrade.value,
      language: programmingLanguage.value,
      subject: selectedSubject.value,
      mode: interviewMode.value,
      averageScore: averageScore.value,
      successRate: successRate.value,
      endTime: new Date().toISOString(),
    };

    // Сохраняем в массив завершённых сессий
    const completedSessions = JSON.parse(
      localStorage.getItem('completed_sessions') || '[]'
    );
    completedSessions.push(sessionData);
    localStorage.setItem('completed_sessions', JSON.stringify(completedSessions));

    // Удаляем текущую сессию
    localStorage.removeItem('current_session');
  };

  // Сбросить store и начать заново
  const resetStore = (): void => {
    sessionId.value = '';
    currentDifficulty.value = 2.0;
    taskAttempts.value = [];
    finalGrade.value = null;
    programmingLanguage.value = 'C#';
    selectedSubject.value = 'Algorithms';
    interviewMode.value = 'llm';
    selectedScenarioId.value = null;
    skillLevel.value = null;

    localStorage.removeItem('current_session');
    console.log('  Store сброшен');
  };

  // Сохранить текущую сессию в localStorage
  const saveSessionToLocalStorage = (): void => {
    const sessionData = {
      sessionId: sessionId.value,
      currentDifficulty: currentDifficulty.value,
      taskAttempts: taskAttempts.value,
      finalGrade: finalGrade.value,
      programmingLanguage: programmingLanguage.value,
      selectedSubject: selectedSubject.value,
      interviewMode: interviewMode.value,
      selectedScenarioId: selectedScenarioId.value,
      skillLevel: skillLevel.value,
      timerStartTimestamp: timerStartTimestamp.value,
    };

    localStorage.setItem('current_session', JSON.stringify(sessionData));
  };

  // Восстановить сессию из localStorage
  const restoreSession = (): boolean => {
    const savedSession = localStorage.getItem('current_session');
    if (!savedSession) return false;

    try {
      const data = JSON.parse(savedSession);
      sessionId.value = data.sessionId;
      currentDifficulty.value = data.currentDifficulty;
      taskAttempts.value = data.taskAttempts.map((a: TaskAttempt) => ({
        ...a,
        timestamp: new Date(a.timestamp),
      }));
      finalGrade.value = data.finalGrade;
      programmingLanguage.value = data.programmingLanguage;
      selectedSubject.value = data.selectedSubject;
      interviewMode.value = data.interviewMode;
      selectedScenarioId.value = data.selectedScenarioId;
      skillLevel.value = data.skillLevel;
      timerStartTimestamp.value = data.timerStartTimestamp || null,

      console.log('Сессия восстановлена:', sessionId.value);
      return true;
    } catch (err) {
      console.error('Ошибка восстановления сессии:', err);
      return false;
    }
  };

  const startTimer = (): void => {
    if (!timerStartTimestamp.value && hasActiveSession.value) {
      timerStartTimestamp.value = Date.now()  // Timestamp в ms
      console.log('Таймер запущен:', new Date(timerStartTimestamp.value).toLocaleTimeString())
      saveSessionToLocalStorage()
    }
  }

  const stopTimer = (): void => {
    if (timerStartTimestamp.value) {
      console.log('Таймер остановлен')
      timerStartTimestamp.value = null
      saveSessionToLocalStorage()
    }
  }

  // ============ RETURN ============

  return {
    // State
    sessionId,
    currentDifficulty,
    taskAttempts,
    finalGrade,
    programmingLanguage,
    selectedSubject,
    interviewMode,
    selectedScenarioId,
    skillLevel,
    timerStartTimestamp,

    // Getters
    totalTasks,
    averageScore,
    correctSolutions,
    partialSolutions,
    incorrectSolutions,
    successRate,
    lastAttempt,
    hasActiveSession,
    progressPercentage,

    // Actions
    startSession,
    saveTaskAttempt,
    updateDifficulty,
    determineFinalGrade,
    endSession,
    resetStore,
    restoreSession,
    startTimer,
    stopTimer
  };
});
