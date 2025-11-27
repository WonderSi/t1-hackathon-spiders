<template>
  <Teleport to="body">
    <div v-if="isOpen" class="modal-overlay">
      <div class="modal-content">

        <h2 class="modal-title">Настройка интервью</h2>
        <p class="modal-subtitle">
          Выберите параметры для генерации персонализированных задач
        </p>

        <form class="modal-form" @submit.prevent="handleSubmit">
          <!-- Skill Level -->
          <div class="form-group">
            <label for="skillLevel" class="form-label">
              Уровень навыков <span class="required">*</span>
            </label>
            <select 
              id="skillLevel" 
              v-model="formData.skillLevel" 
              class="form-select"
              required
            >
              <option value="" disabled>Выберите уровень</option>
              <option value="Junior">Junior</option>
              <option value="Middle">Middle</option>
              <option value="Senior">Senior</option>
            </select>
          </div>

          <!-- Programming Language -->
          <div class="form-group">
            <label for="programmingLanguage" class="form-label">
              Язык программирования <span class="required">*</span>
            </label>
            <select 
              id="programmingLanguage" 
              v-model="formData.programmingLanguage" 
              class="form-select"
              required
            >
              <option value="" disabled>Выберите язык</option>
              <option value="JavaScript">JavaScript</option>
              <option value="TypeScript">TypeScript</option>
              <option value="Python">Python</option>
              <option value="Java">Java</option>
              <option value="C++">C++</option>
              <option value="Go">Go</option>
              <option value="Kotlin">Kotlin</option>
            </select>
          </div>

          <!-- Subject -->
          <div class="form-group">
            <label for="subject" class="form-label">
              Тема задач <span class="required">*</span>
            </label>
            <select 
              id="subject" 
              v-model="formData.subject" 
              class="form-select"
              required
            >
              <option value="" disabled>Выберите тему</option>
              <option value="Algorithms">Algorithms</option>
              <option value="OOP">OOP</option>
              <option value="Data Structures">Data Structures</option>
              <option value="Databases">Databases</option>
              <option value="System Design">System Design</option>
              <option value="Testing">Testing</option>
            </select>
          </div>

          <!-- Current Difficulty -->
          <div class="form-group">
            <label for="currentDifficulty" class="form-label">
              Начальная сложность <span class="required">*</span>
            </label>
            <select 
              id="currentDifficulty" 
              v-model.number="formData.currentDifficulty" 
              class="form-select"
              required
            >
              <option value="" disabled>Выберите сложность</option>
              <option :value="1">1 - Очень легко</option>
              <option :value="2">2 - Легко</option>
              <option :value="3">3 - Средне</option>
              <option :value="4">4 - Сложно</option>
              <option :value="5">5 - Очень сложно</option>
            </select>
          </div>

          <!-- Loading State -->
          <div v-if="isLoading" class="loading-indicator">
            <div class="spinner"></div>
            <p>Генерация первой задачи...</p>
          </div>

          <!-- Error Message -->
          <div v-if="errorMessage" class="error-message">
            {{ errorMessage }}
          </div>

          <!-- Submit Button -->
          <div class="modal-actions">
            <button 
              type="submit" 
              class="btn-submit" 
              :disabled="!isFormValid || isLoading"
            >
              {{ isLoading ? 'Загрузка...' : 'Начать интервью' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import { useAssessmentStore } from '@/stores/assessment';
import { useTasks } from '@/composables/useTasks';
import type { SkillLevel, Subject } from '@/types/types-api';

interface Props {
  isOpen: boolean;
}

interface FormData {
  skillLevel: SkillLevel | '';
  programmingLanguage: string;
  subject: Subject | '';
  currentDifficulty: number | '';
}

const props = defineProps<Props>();
const emit = defineEmits<{
  close: [];
  success: [];
}>();

// Stores and composables
const assessmentStore = useAssessmentStore();
const { generateLLMTask, currentTask } = useTasks();

// Form state
const formData = ref<FormData>({
  skillLevel: '',
  programmingLanguage: '',
  subject: '',
  currentDifficulty: ''
});

const isLoading = ref<boolean>(false);
const errorMessage = ref<string>('');

// Validation
const isFormValid = computed(() => {
  return (
    formData.value.skillLevel !== '' &&
    formData.value.programmingLanguage !== '' &&
    formData.value.subject !== '' &&
    formData.value.currentDifficulty !== ''
  );
});

// Submit handler
const handleSubmit = async (): Promise<void> => {
  if (!isFormValid.value || isLoading.value) {
    return;
  }

  isLoading.value = true;
  errorMessage.value = '';

  try {
    // 1. Стартуем сессию в store
    assessmentStore.startSession(
      formData.value.programmingLanguage,
      formData.value.subject as Subject,
      formData.value.skillLevel as SkillLevel,
      'llm' // Режим адаптивной генерации
    );

    // 2. Генерируем первую задачу
    await generateLLMTask({
      skillLevel: formData.value.skillLevel as SkillLevel,
      programmingLanguage: formData.value.programmingLanguage,
      subject: formData.value.subject as Subject,
      currentDifficulty: formData.value.currentDifficulty as number
    });

    if (currentTask.value) {
      console.log('Первая задача сгенерирована:', currentTask.value.taskId);
      
      // 3. Успешно - закрываем модальное окно
      emit('success');
      resetForm();
    } else {
      throw new Error('Задача не была сгенерирована');
    }
  } catch (err) {
    console.error('Ошибка инициализации:', err);
    errorMessage.value = 'Не удалось начать интервью. Попробуйте снова.';
    
    // Откатываем сессию при ошибке
    assessmentStore.resetStore();
  } finally {
    isLoading.value = false;
  }
};

// Close modal
const closeModal = (): void => {
  if (!isLoading.value) {
    emit('close');
    resetForm();
  }
};

// Reset form
const resetForm = (): void => {
  formData.value = {
    skillLevel: '',
    programmingLanguage: '',
    subject: '',
    currentDifficulty: ''
  };
  errorMessage.value = '';
};
</script>

<style lang="scss" scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(41, 34, 30, 0.9);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 9999;
  backdrop-filter: blur(6px);
  animation: fadeIn 0.3s ease;
}

@keyframes fadeIn {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}

.modal-content {
  background: $clr-light-card;
  padding: 2.5rem;
  border-radius: 16px;
  width: 540px;
  max-width: 90%;
  max-height: 90vh;
  overflow-y: auto;
  position: relative;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.4);
  animation: slideUp 0.3s ease;
}

@keyframes slideUp {
  from {
    transform: translateY(30px);
    opacity: 0;
  }
  to {
    transform: translateY(0);
    opacity: 1;
  }
}

.modal-title {
  margin: 0 0 0.5rem 0;
  font-family: $font-sans;
  font-size: 1.75rem;
  font-weight: 700;
  color: $clr-light-main;
  text-align: center;
}

.modal-subtitle {
  margin: 0 0 2rem 0;
  font-family: $font-sans;
  font-size: 0.95rem;
  color: $clr-light-accent;
  text-align: center;
  line-height: 1.5;
}

.modal-form {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.form-label {
  font-family: $font-sans;
  font-size: $font-size-base;
  font-weight: 600;
  color: $clr-light-main;

  .required {
    color: #e74c3c;
    margin-left: 3px;
  }
}

.form-select {
  padding: 0.875rem 1rem;
  font-family: $font-sans;
  font-size: $font-size-base;
  color: $clr-light-main;
  background: white;
  border: 2px solid $clr-light-accent;
  border-radius: $radius-1;
  cursor: pointer;
  transition: all 0.2s;
  appearance: none;
  background-repeat: no-repeat;
  background-position: right 1rem center;
  padding-right: 2.75rem;

  &:hover {
    border-color: $clr-light-main;
    box-shadow: 0 2px 8px rgba($clr-light-main, 0.1);
  }

  &:focus {
    outline: none;
    border-color: $clr-light-main;
    box-shadow: 0 0 0 3px rgba($clr-light-main, 0.15);
  }

  option:disabled {
    color: $clr-light-accent;
  }
}

.loading-indicator {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
  padding: 1rem;
  background: rgba($clr-light-accent, 0.1);
  border-radius: $radius-1;

  p {
    margin: 0;
    font-family: $font-sans;
    color: $clr-light-main;
    font-size: 0.95rem;
  }
}

.spinner {
  width: 32px;
  height: 32px;
  border: 3px solid rgba($clr-light-main, 0.2);
  border-top-color: $clr-light-main;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.error-message {
  padding: 1rem;
  background: rgba(#e74c3c, 0.1);
  border: 1px solid #e74c3c;
  border-radius: $radius-1;
  color: #c0392b;
  font-family: $font-sans;
  font-size: 0.9rem;
  text-align: center;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  margin-top: 0.5rem;

  button {
    padding: 0.875rem 1.75rem;
    border-radius: $radius-1;
    cursor: pointer;
    font-family: $font-sans;
    font-size: $font-size-base;
    font-weight: 600;
    transition: all 0.2s;
    border: none;

    &.btn-submit {
      background: $clr-light-main;
      color: $clr-light-card;

      &:hover:not(:disabled) {
        background: $clr-light-accent;
        transform: translateY(-2px);
        box-shadow: 0 6px 16px rgba($clr-light-main, 0.4);
      }

      &:disabled {
        background: $clr-light-accent-hover;
        cursor: not-allowed;
        opacity: 0.6;
        transform: none;
      }
    }
  }
}

// Scrollbar
.modal-content::-webkit-scrollbar {
  width: 8px;
}

.modal-content::-webkit-scrollbar-track {
  background: rgba($clr-light-accent, 0.1);
  border-radius: 4px;
}

.modal-content::-webkit-scrollbar-thumb {
  background: $clr-light-accent;
  border-radius: 4px;

  &:hover {
    background: $clr-light-main;
  }
}
</style>
