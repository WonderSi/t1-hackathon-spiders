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
import { ref, onMounted, onUnmounted } from 'vue'
import AttachedFile from '../AttachedFile.vue'

const jobTitle = 'Frontend-разработчик'
const jobGrade = 'Junior'
const companyName = 'Т1'

const timer = ref('01:00:00')
let remainingSeconds = 3600 
let intervalId: number | null = null

function formatTime(sec: number) {
  const h = String(Math.floor(sec / 3600)).padStart(2, '0')
  const m = String(Math.floor((sec % 3600) / 60)).padStart(2, '0')
  const s = String(sec % 60).padStart(2, '0')
  return `${h}:${m}:${s}`
}

onMounted(() => {
    timer.value = formatTime(remainingSeconds)

    intervalId = window.setInterval(() => {
        if (remainingSeconds > 0) {
            remainingSeconds--
            timer.value = formatTime(remainingSeconds)
        } else {
            if (intervalId) clearInterval(intervalId)
        }
    }, 1000)
})

onUnmounted(() => {
  if (intervalId) clearInterval(intervalId)
})
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
    letter-spacing: 2px;
    font-weight: 100;
    font-variant-numeric: tabular-nums; 
}

.header_attachments {
    display: flex;
    flex-direction: row;
    gap: 16px;
    height: 62px;
    align-items: center;
}
</style>
