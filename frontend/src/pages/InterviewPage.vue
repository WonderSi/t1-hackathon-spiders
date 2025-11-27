<template>
  <Header />
  <div class="main-layout">
    <Chat />
    <CodeWindow 
      ref="codeWindowRef"
      :is-submitting-external="isProcessingSolution"
      @submit="handleCodeSubmit"
      @status-change="handleStatusChange"
    />
  </div>
</template>

<script setup lang="ts">
  import { ref } from 'vue';
  import Header from '@/components/interview/Header.vue';
  import Chat from '@/components/interview/Chat.vue';
  import CodeWindow from '@/components/interview/CodeWindow.vue';
  import { useTasks } from '@/composables/useTasks';
  import { useAssessment } from '@/composables/useAssessment';
  import { useAssessmentStore } from '@/stores/assessment'; 
  import type { Performance } from '@/types/types-api';

  const { currentTask, generateLLMTask } = useTasks();
  const { assessAndCheckPlagiarism } = useAssessment();
  const assessmentStore = useAssessmentStore();

  const codeWindowRef = ref<InstanceType<typeof CodeWindow> | null>(null);
  const isProcessingSolution = ref<boolean>(false);

  // Определить SkillLevel на основе текущей сложности
  const determineSkillLevel = (difficulty: number): 'Junior' | 'Middle' | 'Senior' => {
    if (difficulty <= 2.5) return 'Junior';
    if (difficulty <= 4.0) return 'Middle';
    return 'Senior';
  };

  // Обработка отправки решения из CodeWindow
  const handleCodeSubmit = async (payload: {
    code: string;
    language: string;
    solutionTime: string;
    antiCheatMetrics: any;
  }) => {
    if (!currentTask.value || isProcessingSolution.value) return;

    isProcessingSolution.value = true;

    try {
      console.log('📤 Отправка решения на оценку...');
      console.log('Anti-cheat метрики:', payload.antiCheatMetrics);

      // 1. Оценка + проверка плагиата параллельно
      const { score, isPlagiarized } = await assessAndCheckPlagiarism(
        currentTask.value.taskId,
        currentTask.value.description,
        payload.code,
        payload.language
      );

      if (score === null) {
        throw new Error('Не удалось получить оценку решения');
      }

      console.log(`📊 Оценка: ${score}/5.0 | Плагиат: ${isPlagiarized ? 'Да' : 'Нет'}`);

      // 2. Определение результата
      const performance: Performance = 
        score >= 4.0 ? 'Correct' : 
        score >= 2.0 ? 'Partially' : 
        'Incorrect';

      // 3. Обновляем статус в CodeWindow
      if (codeWindowRef.value) {
        if (performance === 'Correct') {
          codeWindowRef.value.updateStatus('Решено!');
        } else {
          codeWindowRef.value.updateStatus('Неправильно :(');
        }
      }

      // 4. Сохранение попытки в store
      assessmentStore.saveTaskAttempt(
        currentTask.value.taskId,
        currentTask.value.description,
        payload.code,
        score,
        performance
      );

      // 5. Адаптация сложности
      await assessmentStore.updateDifficulty(
        currentTask.value.taskId,
        payload.code,
        score,
        performance
      );

      // 6. Проверка: нужна ли следующая задача
      if (assessmentStore.totalTasks < 10) {
        console.log('🔄 Генерация следующей задачи...');

        await generateLLMTask({
          skillLevel: determineSkillLevel(assessmentStore.currentDifficulty),
          programmingLanguage: assessmentStore.programmingLanguage,
          subject: assessmentStore.selectedSubject as any,
          currentDifficulty: assessmentStore.currentDifficulty,
          previousPerformance: performance
        });

        // Сброс редактора для новой задачи
        if (codeWindowRef.value) {
          codeWindowRef.value.resetForNewTask();
        }
      } else {
        // 7. Интервью завершено
        console.log('🎉 Все задачи выполнены! Определяем итоговый грейд...');
        
        const grade = await assessmentStore.determineFinalGrade();
        assessmentStore.endSession();
        
        console.log(`🏆 Итоговый грейд: ${grade}`);
        // TODO: Показать модалку с результатами
      }

    } catch (err) {
      console.error('❌ Ошибка обработки решения:', err);
      
      if (codeWindowRef.value) {
        codeWindowRef.value.updateStatus('Ошибка сети');
      }
    } finally {
      isProcessingSolution.value = false;
    }
  };

  // 🟢 ДОБАВЛЕНО: обработчик изменения статуса (опционально)
  /**
   * Обработка изменения статуса (опционально для логирования)
   */
  const handleStatusChange = (status: string) => {
    console.log('📝 Статус изменился:', status);
  };
</script>

<style scoped lang="scss">
</style>
