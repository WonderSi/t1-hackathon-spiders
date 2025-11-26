<template>
  <Transition name="fade">
    <div v-if="violations.length > 0 && isVisible" class="anti-cheat-warning">
      <div class="warning-header">
        <span class="warning-icon">⚠️</span>
        <span>Обнаружена подозрительная активность</span>
      </div>
      <ul class="violations-list">
        <li v-for="(violation, index) in visibleViolations" :key="index">
          {{ violation }}
        </li>
      </ul>
      <div v-if="violations.length > 5" class="more-violations">
        ...и еще {{ violations.length - 5 }}
      </div>
    </div>
  </Transition>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue'

const props = defineProps<{
  violations: string[]
}>()

const visibleViolations = computed(() => {
  return props.violations.slice(-5)
})

const isVisible = ref(false)
let hideTimer: ReturnType<typeof setTimeout> | null = null

watch(() => props.violations, (newVal) => {
  if (newVal.length > 0) {
    isVisible.value = true
    
    if (hideTimer) clearTimeout(hideTimer)
    
    hideTimer = setTimeout(() => {
      isVisible.value = false
    }, 3000)
  }
}, { deep: true })
</script>

<style lang="scss">
.warning-icon {
  font-size: 14px;
  margin-bottom: 6px;
}

.more-violations {
    font-family: $font-mono;
    font-size: 12px;
    color: $clr-light-main;
    opacity: 0.7;
    margin-top: 8px;
    padding-left: 20px;
}

.anti-cheat-warning {
  position: fixed;
  top: 20px;
  right: 20px;
  background: #fff3cd;
  border-radius: 8px;
  padding: 24px;
  max-width: 400px;
  z-index: 1000;
  box-shadow: $shadow;

  .warning-header {
    display: flex;
    align-items: center;
    font-family: $font-sans;
    gap: 2px;
    font-weight: 600;
    margin-bottom: 4px;
    color: $clr-light-main;
  }

  .violations-list {
    font-family: $font-mono;
    margin: 0;
    padding-left: 20px;
    font-size: 14px;

    li {
      margin-bottom: 4px;
    }
  }
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.5s ease, transform 0.5s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
  transform: translateY(-20px);
}
</style>