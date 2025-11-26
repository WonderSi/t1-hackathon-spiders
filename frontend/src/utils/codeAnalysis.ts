export interface CodeAnalysisResult {
  originalityScore: number // 0-100
  suspiciousPatterns: string[]
  complexity: number
}

export function analyzeCodeOriginality(code: string): CodeAnalysisResult {
  const suspiciousPatterns: string[] = []
  let originalityScore = 100
  
  // 1. Проверка на типичные шаблоны из популярных решений
  const commonPatterns = [
    /function\s+solution\s*\(/gi,
    /leetcode/gi,
    /hackerrank/gi
  ]
  
  commonPatterns.forEach(pattern => {
    if (pattern.test(code)) {
      originalityScore -= 15
      suspiciousPatterns.push('Обнаружен типичный шаблон')
    }
  })
  
  // 2. Анализ сложности (слишком простой код подозрителен)
  const complexity = calculateComplexity(code)
  
  // 3. Проверка на последовательные большие вставки
  const lines = code.split('\n')
  if (lines.length > 50 && complexity < 5) {
    originalityScore -= 20
    suspiciousPatterns.push('Низкая сложность при большом объеме')
  }
  
  return {
    originalityScore: Math.max(0, originalityScore),
    suspiciousPatterns,
    complexity
  }
}

function calculateComplexity(code: string): number {
  let complexity = 0
  
  // Циклические конструкции
  complexity += (code.match(/for\s*\(/g) || []).length * 2
  complexity += (code.match(/while\s*\(/g) || []).length * 2
  
  // Условия
  complexity += (code.match(/if\s*\(/g) || []).length
  
  // Функции
  complexity += (code.match(/function\s+/g) || []).length * 3
  
  return complexity
}
