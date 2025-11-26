import { ref, onMounted, onUnmounted } from 'vue'

interface AntiCheatMetrics {
  pasteCount: number
  devToolsOpened: boolean
  suspiciousExtensions: string[]
  typingPatterns: number[]
  codeChangeTimestamps: number[]
}

export function useAntiCheat() {
  const metrics = ref<AntiCheatMetrics>({
    pasteCount: 0,
    devToolsOpened: false,
    suspiciousExtensions: [],
    typingPatterns: [],
    codeChangeTimestamps: []
  })

  const violations = ref<string[]>([])
  
  const detectDevTools = () => {
    const threshold = 160
    const widthThreshold = window.outerWidth - window.innerWidth > threshold
    const heightThreshold = window.outerHeight - window.innerHeight > threshold
    
    if (widthThreshold || heightThreshold) {
      if (!metrics.value.devToolsOpened) {
        metrics.value.devToolsOpened = true
        violations.value.push('DevTools открыты')
      }
    }
  }

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

  let devToolsInterval: number

  onMounted(() => {
    detectExtensions()
    devToolsInterval = window.setInterval(detectDevTools, 1000)
  })

  onUnmounted(() => {
    clearInterval(devToolsInterval)
  })

  return {
    metrics,
    violations
  }
}
