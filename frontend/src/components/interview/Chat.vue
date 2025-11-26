<template>
    <div class="chat">
        <div class="chat__content">
            <!-- containing AI responses -->
            <!-- Эмуляция длинного контента для проверки скролла -->
            <p v-for="i in 50" :key="i">Сообщение {{ i }}</p> 
        </div>
        <MessagePanel />
            <button 
      v-if="isDev" 
      @click="runQuickTest"
      class="test-button"
    >
      Тест API
    </button>
    </div>
</template>

<script setup lang="ts">
    import MessagePanel from './MessagePanel.vue'
    import { ref } from 'vue';
import { useTasks } from '@/composables/useTasks';
import { useAssessment } from '@/composables/useAssessment';

const isDev = import.meta.env.DEV;

const runQuickTest = async () => {
  console.log('Тест API...');
  
  const { generateLLMTask, currentTask } = useTasks();
  
  await generateLLMTask({
    skillLevel: 'Junior',
    programmingLanguage: 'C#',
    subject: 'OOP',
    currentDifficulty: 3,
    previousPerformance: "Correct"
  });
  
  console.log('Результат:', currentTask.value);
  alert(`Задача сгенерирована: ${currentTask.value?.taskId}`);
};
</script>

<style lang="scss" scoped>
.chat {
    display: flex;
    flex-direction: column;
    flex: 1;
    min-width: 0;
    height: 100%;
    background: $clr-light-card;
    border-radius: $radius-1;
    overflow: hidden;
}

.chat__content {
    padding-inline: 16px;
    overflow-y: auto;
    scrollbar-width: thin;
    scrollbar-color: $clr-light-accent transparent;
    color: $clr-light-main;
}
</style>
