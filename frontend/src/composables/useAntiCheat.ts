import { ref, onMounted, onUnmounted } from 'vue'

interface AntiCheatMetrics {
    pasteCount: number
    devToolsOpened: boolean
    suspiciousExtensions: string[]
    typingPatterns: number[]
    codeChangeTimestamps: number[]
    tabSwitchCount: number
}

export function useAntiCheat() {
    const metrics = ref<AntiCheatMetrics>({
        pasteCount: 0,
        devToolsOpened: false,
        suspiciousExtensions: [],
        typingPatterns: [],
        codeChangeTimestamps: [],
        tabSwitchCount: 0
    })

    const violations = ref<string[]>([])
    const isDevToolsCurrentlyOpen = ref(false)
    let lastInputTime = Date.now()

    // копипаст
    const registerPaste = (textLength: number) => {
        metrics.value.pasteCount++
        metrics.value.codeChangeTimestamps.push(Date.now())

        if (metrics.value.pasteCount > 3) {
            const lastViolation = violations.value[violations.value.length - 1]
            if (!lastViolation?.startsWith('Подозрительное количество вставок')) {
                 violations.value.push(`Подозрительное количество вставок: ${metrics.value.pasteCount}`)
            }
        }

        // Правило: вставка огромного куска кода
        if (textLength > 500) {
            violations.value.push(`Массовая вставка кода (${textLength} символов)`)
        }
    }

    // скорость ввода (защита от макросов или очень быстрой вставки без Ctrl+V)
    const analyzeInputPattern = (changeLength: number) => {
        const now = Date.now()
        const timeDiff = now - lastInputTime
        
        metrics.value.codeChangeTimestamps.push(now)
        metrics.value.typingPatterns.push(timeDiff)

        if (timeDiff < 20 && changeLength > 10) {
            violations.value.push('Аномально быстрая генерация кода')
        }

        lastInputTime = now
    }

    // детектим девтулы
    const detectDevTools = () => {
        const threshold = 160
        const widthThreshold = window.outerWidth - window.innerWidth > threshold
        const heightThreshold = window.outerHeight - window.innerHeight > threshold

        const isOpenNow = widthThreshold || heightThreshold

        if (isOpenNow && !isDevToolsCurrentlyOpen.value) {
            violations.value.push(`DevTools открыты`)
            metrics.value.devToolsOpened = true
        }
        isDevToolsCurrentlyOpen.value = isOpenNow
    }

    // детектим расширения
    const detectExtensions = async () => {
        const suspiciousExtensions = [
            { id: 'fmkadmapgofadopljbjfkapdkoienihi', name: 'React DevTools' },
            { id: 'nhdogjmejiglipccpnnnanhbledajbpd', name: 'Vue DevTools' }
        ]

        for (const ext of suspiciousExtensions) {
            try {
                await fetch(`chrome-extension://${ext.id}/manifest.json`)
                metrics.value.suspiciousExtensions.push(ext.name)
            } catch {
                // Расширение не установлено
            }
        }
    }

    // --- 3. Уход с вкладки (Tab Switching) ---
    const handleVisibilityChange = () => {
        if (document.hidden) {
            metrics.value.tabSwitchCount++
            violations.value.push('Пользователь переключил вкладку')
        } else { }
    }

    // --- 4. Потеря фокуса окна
        const handleBlur = () => {
        if (!document.hidden) {
        violations.value.push('Клик вне браузера')
        }
    }

    let devToolsInterval: number
    onMounted(() => {
        detectExtensions()
        devToolsInterval = window.setInterval(detectDevTools, 1000)
        document.addEventListener('visibilitychange', handleVisibilityChange)
        window.addEventListener('blur', handleBlur)
    })

    onUnmounted(() => {
        clearInterval(devToolsInterval)
        document.removeEventListener('visibilitychange', handleVisibilityChange)
        window.removeEventListener('blur', handleBlur)
    })

    return {
        metrics,
        violations,
        registerPaste,
        analyzeInputPattern
    }
}
