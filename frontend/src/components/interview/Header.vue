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
            <div class="attachment">
                <svg width="53" height="63" viewBox="0 0 53 63" fill="none" xmlns="http://www.w3.org/2000/svg">
                    <path d="M10.1133 0.500366L27.0957 0.702515C29.5609 0.731893 31.9177 1.7192 33.6689 3.45447L49.1641 18.807C50.9644 20.591 51.9766 23.0214 51.9766 25.556V52.3529C51.9764 57.5995 47.7232 61.8529 42.4766 61.8529H10C4.75338 61.8529 0.500136 57.5995 0.5 52.3529V10.0004C0.5 4.70944 4.82273 0.437383 10.1133 0.500366Z" fill="white" stroke="#55463F"/>
                    <path d="M42.8647 24.1329L28.5335 9.80177C28.5056 9.77406 28.4702 9.75521 28.4316 9.74759C28.3931 9.73997 28.3531 9.74392 28.3168 9.75893C28.2805 9.77395 28.2494 9.79937 28.2275 9.83199C28.2056 9.86461 28.1938 9.90298 28.1937 9.94228V22.8783C28.1937 23.3012 28.3617 23.7067 28.6607 24.0058C28.9597 24.3048 29.3653 24.4728 29.7881 24.4728H42.7242C42.7634 24.4726 42.8018 24.4608 42.8344 24.4389C42.8671 24.417 42.8925 24.3859 42.9075 24.3496C42.9225 24.3133 42.9265 24.2733 42.9188 24.2348C42.9112 24.1962 42.8924 24.1608 42.8647 24.1329Z" fill="#55463F"/>
                    <path d="M15.4381 10.5282H23.0045V22.8788C23.0046 24.6777 23.7198 26.4027 24.9918 27.6747C26.2639 28.9467 27.9888 29.6619 29.7877 29.662H42.1383V46.7948C42.1383 47.9558 41.677 49.0695 40.8561 49.8905C40.0351 50.7115 38.9214 51.1727 37.7604 51.1727H15.4381C14.277 51.1727 13.1634 50.7115 12.3424 49.8905C11.5215 49.0695 11.0602 47.9558 11.0602 46.7948V14.9061L11.066 14.6893C11.1196 13.6072 11.5726 12.5802 12.3424 11.8104C13.1634 10.9894 14.277 10.5282 15.4381 10.5282ZM38.3766 22.4725H30.194V14.2899L38.3766 22.4725Z" stroke="#55463F" stroke-width="4"/>
                </svg>
                <label>резюме</label>
            </div>
            <div class="attachment">
                <svg width="53" height="63" viewBox="0 0 53 63" fill="none" xmlns="http://www.w3.org/2000/svg">
                    <path d="M10.1133 0.500366L27.0957 0.702515C29.5609 0.731893 31.9177 1.7192 33.6689 3.45447L49.1641 18.807C50.9644 20.591 51.9766 23.0214 51.9766 25.556V52.3529C51.9764 57.5995 47.7232 61.8529 42.4766 61.8529H10C4.75338 61.8529 0.500136 57.5995 0.5 52.3529V10.0004C0.5 4.70944 4.82273 0.437383 10.1133 0.500366Z" fill="white" stroke="#55463F"/>
                    <path d="M42.8647 24.1329L28.5335 9.80177C28.5056 9.77406 28.4702 9.75521 28.4316 9.74759C28.3931 9.73997 28.3531 9.74392 28.3168 9.75893C28.2805 9.77395 28.2494 9.79937 28.2275 9.83199C28.2056 9.86461 28.1938 9.90298 28.1937 9.94228V22.8783C28.1937 23.3012 28.3617 23.7067 28.6607 24.0058C28.9597 24.3048 29.3653 24.4728 29.7881 24.4728H42.7242C42.7634 24.4726 42.8018 24.4608 42.8344 24.4389C42.8671 24.417 42.8925 24.3859 42.9075 24.3496C42.9225 24.3133 42.9265 24.2733 42.9188 24.2348C42.9112 24.1962 42.8924 24.1608 42.8647 24.1329Z" fill="#55463F"/>
                    <path d="M15.4381 10.5282H23.0045V22.8788C23.0046 24.6777 23.7198 26.4027 24.9918 27.6747C26.2639 28.9467 27.9888 29.6619 29.7877 29.662H42.1383V46.7948C42.1383 47.9558 41.677 49.0695 40.8561 49.8905C40.0351 50.7115 38.9214 51.1727 37.7604 51.1727H15.4381C14.277 51.1727 13.1634 50.7115 12.3424 49.8905C11.5215 49.0695 11.0602 47.9558 11.0602 46.7948V14.9061L11.066 14.6893C11.1196 13.6072 11.5726 12.5802 12.3424 11.8104C13.1634 10.9894 14.277 10.5282 15.4381 10.5282ZM38.3766 22.4725H30.194V14.2899L38.3766 22.4725Z" stroke="#55463F" stroke-width="4"/>
                </svg>
                <label>о нас</label>
            </div>
            <div class="attachment">
                <svg width="53" height="63" viewBox="0 0 53 63" fill="none" xmlns="http://www.w3.org/2000/svg">
                    <path d="M10.1133 0.500366L27.0957 0.702515C29.5609 0.731893 31.9177 1.7192 33.6689 3.45447L49.1641 18.807C50.9644 20.591 51.9766 23.0214 51.9766 25.556V52.3529C51.9764 57.5995 47.7232 61.8529 42.4766 61.8529H10C4.75338 61.8529 0.500136 57.5995 0.5 52.3529V10.0004C0.5 4.70944 4.82273 0.437383 10.1133 0.500366Z" fill="white" stroke="#55463F"/>
                    <path d="M42.8647 24.1329L28.5335 9.80177C28.5056 9.77406 28.4702 9.75521 28.4316 9.74759C28.3931 9.73997 28.3531 9.74392 28.3168 9.75893C28.2805 9.77395 28.2494 9.79937 28.2275 9.83199C28.2056 9.86461 28.1938 9.90298 28.1937 9.94228V22.8783C28.1937 23.3012 28.3617 23.7067 28.6607 24.0058C28.9597 24.3048 29.3653 24.4728 29.7881 24.4728H42.7242C42.7634 24.4726 42.8018 24.4608 42.8344 24.4389C42.8671 24.417 42.8925 24.3859 42.9075 24.3496C42.9225 24.3133 42.9265 24.2733 42.9188 24.2348C42.9112 24.1962 42.8924 24.1608 42.8647 24.1329Z" fill="#55463F"/>
                    <path d="M15.4381 10.5282H23.0045V22.8788C23.0046 24.6777 23.7198 26.4027 24.9918 27.6747C26.2639 28.9467 27.9888 29.6619 29.7877 29.662H42.1383V46.7948C42.1383 47.9558 41.677 49.0695 40.8561 49.8905C40.0351 50.7115 38.9214 51.1727 37.7604 51.1727H15.4381C14.277 51.1727 13.1634 50.7115 12.3424 49.8905C11.5215 49.0695 11.0602 47.9558 11.0602 46.7948V14.9061L11.066 14.6893C11.1196 13.6072 11.5726 12.5802 12.3424 11.8104C13.1634 10.9894 14.277 10.5282 15.4381 10.5282ZM38.3766 22.4725H30.194V14.2899L38.3766 22.4725Z" stroke="#55463F" stroke-width="4"/>
                </svg>
                <label>вакансия</label>
            </div>
        </div>
    </header>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'

const jobTitle = 'Frontend-разработчик'
const jobGrade = 'Junior'
const companyName = 'Т1'

const timer = ref('00:00:00')
let seconds = 0
let intervalId: number | null = null

function formatTime(sec: number) {
  const h = String(Math.floor(sec / 3600)).padStart(2, '0')
  const m = String(Math.floor((sec % 3600) / 60)).padStart(2, '0')
  const s = String(sec % 60).padStart(2, '0')
  return `${h}:${m}:${s}`
}

onMounted(() => {
  intervalId = window.setInterval(() => {
    seconds++
    timer.value = formatTime(seconds)
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
}

.header_attachments {
    display: flex;
    flex-direction: row;
    gap: 16px;
    height: 62px;
    align-items: center;
}

.attachment {
    width: 65px;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: flex-start;
}

.attachment svg {
    width: 100%;
    max-height: 63px;
    max-width: 53px;
    display: block;
    margin: 0 auto;
    border-radius: 10px;
    flex-shrink: 0;
}

.attachment label {
    font-family: $font-sans;
    font-size: $font-size-base;
    font-weight: 400;
    color: $clr-light-header-font;
    text-align: center;
    margin-top: 4px;
}
</style>
