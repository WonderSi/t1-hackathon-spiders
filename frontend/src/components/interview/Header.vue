<template>
    <header class="header">
        <h1 class="header__company-name">{{ companyName }}</h1>
        <div class="header__divider"></div>
        <h1 class="header__job-title-timer">
            {{ jobGrade }} {{ jobTitle }}
            <span class="header__timer"> — {{ timer }}</span>
        </h1>
        <div class="header__divider"></div>
        <div class="header_attachments">
            <AttachedFile type="resume" label="резюме" />
            <AttachedFile type="company" label="о нас" />
            <AttachedFile type="job" label="вакансия" />
        </div>
    </header>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import AttachedFile from '../AttachedFile.vue'
import { useAssessmentStore } from '@/stores/assessment'
import { useTasks } from '@/composables/useTasks'

const assessmentStore = useAssessmentStore()
const { currentTask } = useTasks()

const companyName = 'Т1'

const jobTitle = computed(() => {
  if (!assessmentStore.selectedSubject || !assessmentStore.selectedSubject.trim()) {
    return 'IT-специалист'
  }

  const subjectMap: Record<string, string> = {
    'Algorithms': 'Алгоритмы',
    'OOP': 'Объектно-ориентированное программирование',
    'Data Structures': 'Структура данных',
    'Databases': 'База данных',
    'System Design': 'Системный архитектор',
    'Testing': 'Тестировщик',
  }

  const prefix = subjectMap[assessmentStore.selectedSubject] || 'Software'
  return prefix
})

const jobGrade = computed<'Junior' | 'Middle' | 'Senior'>(() => {
  return assessmentStore.skillLevel ?? 'Junior'
})

const timer = ref('01:00:00')
let intervalId: number | null = null
let INITIAL_DURATION = 3600 

// Расчет оставшегося времени
function getRemainingSeconds(): number {
  if (!assessmentStore.timerStartTimestamp) {
    return INITIAL_DURATION
  }
  const elapsedMs = Date.now() - assessmentStore.timerStartTimestamp
  const elapsedSeconds = Math.floor(elapsedMs / 1000)
  return Math.max(0, INITIAL_DURATION - elapsedSeconds)
}

// Формат времени
function formatTime(sec: number) {
  const h = String(Math.floor(sec / 3600)).padStart(2, '0')
  const m = String(Math.floor((sec % 3600) / 60)).padStart(2, '0')
  const s = String(sec % 60).padStart(2, '0')
  return `${h}:${m}:${s}`
}

// Update таймера, вызывается каждую сек
function updateTimer(): void {
  const remaining = getRemainingSeconds()
  timer.value = formatTime(remaining)

  if (remaining <= 0) {
    if (intervalId) {
      clearInterval(intervalId)
      intervalId = null
      // endSession()
    }
  }
}

onMounted(() => {
  updateTimer()

  // Интвервал в секунду, обновленеи ui + проверка завершения 
  intervalId = window.setInterval(updateTimer, 1000)
})

onUnmounted(() => {
  if (intervalId) clearInterval(intervalId)
})

watch([currentTask, assessmentStore.timerStartTimestamp], updateTimer)
</script>

<style scoped lang="scss">
.header {
    display: flex;
    flex-direction: row;
    
    align-items: center;
    justify-content: space-between;
    gap: 28px;

    width: 100%;
    min-height: 50px;
    height: 100px;
    box-sizing: border-box;
    padding: 0 50px;
    background: $clr-light-main;
}

.header__company-name {
    font-family: $font-sans;
    font-size: clamp(1.5rem, 2.5vw, $font-size-header);
    font-weight: 200;
    color: $clr-light-header-font;
    line-height: 1.22;
    flex-shrink: 0;
    min-width: 0;
    margin: 0;
}

.header__divider {
    width: 3px;
    height: calc(100px - 2 * 16px);
    background: $clr-light-header-font;
    border-radius: $radius-1;
    flex-shrink: 0;
}

.header__job-title-timer {
    font-family: $font-sans;
    font-size: clamp(1.2rem, 2vw, $font-size-header);
    font-weight: 900;
    color: $clr-light-header-font;
    line-height: 1.22;
    margin: 0;
    text-align: center;
    min-width: 0;
    flex-grow: 1;
}

.header__timer {
    letter-spacing: 1px;
    font-weight: 100;
    font-variant-numeric: tabular-nums; 
    font-family: $font-mono;
    font-size: clamp(1.2rem, 2vw, $font-size-header);
}

.header_attachments {
    display: flex;
    flex-direction: row;
    gap: 16px;
    height: 62px;
    align-items: center;
}
</style>
