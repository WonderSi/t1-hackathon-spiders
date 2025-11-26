import { ref, computed, onUnmounted } from 'vue'

export function useSolutionTimer() {
    const startTime = ref<number | null>(null)
    const endTime = ref<number | null>(null)
    const isTracking = ref(false)

    const currentTime = ref(Date.now())
    let timerInterval: ReturnType<typeof setInterval> | null = null
    
    const formattedTime = computed(() => {
        if (!startTime.value) return '00:00'
        
        const targetTime = isTracking.value ? currentTime.value : (endTime.value || Date.now())
        const diffSeconds = Math.max(0, Math.floor((targetTime - startTime.value) / 1000))
        const minutes = Math.floor(diffSeconds / 60).toString().padStart(2, '0')
        const seconds = (diffSeconds % 60).toString().padStart(2, '0')

        return `${minutes}:${seconds}`
    })

    const startTimer = () => {
        if (!isTracking.value) {
            startTime.value = Date.now()
            isTracking.value = true

            timerInterval = setInterval(() => {
                currentTime.value = Date.now()
            }, 1000)

            console.log('Таймер решения запущен')
        }
    }

    const stopTimer = () => {
        if (isTracking.value) {
            endTime.value = Date.now()
            isTracking.value = false

            if (timerInterval) {
                clearInterval(timerInterval)
                timerInterval = null
            }

            console.log('Таймер остановлен. Время:', formattedTime.value)
        }
    }

    const resetTimer = () => {
        startTime.value = null
        endTime.value = null
        isTracking.value = false
        if (timerInterval) {
            clearInterval(timerInterval)
            timerInterval = null
        }
    }

    onUnmounted(() => {
        if (timerInterval) clearInterval(timerInterval)
    })

    return {
        startTime,
        endTime,
        isTracking,
        formattedTime,
        startTimer,
        stopTimer,
        resetTimer
    }
}
