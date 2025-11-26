<template>
    <div class="code-window">
        <div class="code-window-header">
            <div class="header-controls">
                <label>
                    Язык:
                    <select v-model="selectedLanguage" @change="onLanguageChange">
                        <option value="javascript">JavaScript</option>
                        <option value="typescript">TypeScript</option>
                        <option value="kotlin">Kotlin</option>
                        <option value="java">Java</option>
                        <option value="python">Python</option>
                        <option value="cpp">C++</option>
                        <option value="go">Go</option>
                    </select>
                </label>
                <div class="timer">
                    <div v-if="startTime" class="timer-display" :class="{ 'timer-active': isTracking }">
                    <svg width="18" height="21" viewBox="0 0 18 21" fill="none" xmlns="http://www.w3.org/2000/svg">
                        <path
                            d="M6 2V0H12V2H6ZM8 13H10V7H8V13ZM9 21C7.76667 21 6.604 20.7627 5.512 20.288C4.42 19.8133 3.466 19.1673 2.65 18.35C1.834 17.5327 1.18833 16.5783 0.713 15.487C0.237667 14.3957 0 13.2333 0 12C0 10.7667 0.237667 9.604 0.713 8.512C1.18833 7.42 1.834 6.466 2.65 5.65C3.466 4.834 4.42033 4.18833 5.513 3.713C6.60567 3.23767 7.768 3 9 3C10.0333 3 11.025 3.16667 11.975 3.5C12.925 3.83333 13.8167 4.31667 14.65 4.95L16.05 3.55L17.45 4.95L16.05 6.35C16.6833 7.18333 17.1667 8.075 17.5 9.025C17.8333 9.975 18 10.9667 18 12C18 13.2333 17.7623 14.396 17.287 15.488C16.8117 16.58 16.166 17.534 15.35 18.35C14.534 19.166 13.5797 19.812 12.487 20.288C11.3943 20.764 10.232 21.0013 9 21ZM9 19C10.9333 19 12.5833 18.3167 13.95 16.95C15.3167 15.5833 16 13.9333 16 12C16 10.0667 15.3167 8.41667 13.95 7.05C12.5833 5.68333 10.9333 5 9 5C7.06667 5 5.41667 5.68333 4.05 7.05C2.68333 8.41667 2 10.0667 2 12C2 13.9333 2.68333 15.5833 4.05 16.95C5.41667 18.3167 7.06667 19 9 19Z"
                            fill="#55463F" />
                    </svg>
                    {{ formattedTime }}
                </div>
                </div>
            </div>
        </div>

        <!-- вынести в хедер? -->
        <AntiCheatWarning :violations="violations" />

        <div class="code-window-content">
            <CodeEditor
                :key="selectedLanguage"
                v-model:value="code"
                :language="selectedLanguage"
                :theme="selectedTheme"
                :options="editorOptions"
                @editorDidMount="onEditorMount"
                @change="onCodeChange"
            />
        </div>

        <div class="code-window-footer">
            <div class="status-info">
                <span class="attempt-counter">Попытка: {{ submitionAttempts }}</span>
                <span class="integrity-score">Оригинальность: {{ integrityScore }}%</span>

                <span v-if="passedStatus" :class="['status-badge', passedStatusClass]">
                    {{ passedStatus }}
                </span>
            </div>

            <button class="code-window-submit-btn" @click="handleSubmit" :disabled="isSubmitting">{{ isSubmitting ?
                'Отправка...' : 'Отправить' }}</button>
        </div>
    </div>
</template>

<script setup lang="ts">
// Monaco Editor (IDE) imports:
import * as monaco from 'monaco-editor'
import editorWorker from 'monaco-editor/esm/vs/editor/editor.worker?worker'
import jsonWorker from 'monaco-editor/esm/vs/language/json/json.worker?worker'
import cssWorker from 'monaco-editor/esm/vs/language/css/css.worker?worker'
import htmlWorker from 'monaco-editor/esm/vs/language/html/html.worker?worker'
import tsWorker from 'monaco-editor/esm/vs/language/typescript/ts.worker?worker'

import { ref, computed } from 'vue'
import { CodeEditor, type EditorOptions } from 'monaco-editor-vue3'

import { useAntiCheat } from '@/composables/useAntiCheat'
import { useSolutionTimer } from '@/composables/useSolutionTimer'
import AntiCheatWarning from './AntiCheatWarning.vue'
import { analyzeCodeOriginality } from '@/utils/codeAnalysis'

self.MonacoEnvironment = {
  getWorker(_, label) {
    if (label === 'json') {
      return new jsonWorker()
    }
    if (label === 'css' || label === 'scss' || label === 'less') {
      return new cssWorker()
    }
    if (label === 'html' || label === 'handlebars' || label === 'razor') {
      return new htmlWorker()
    }
    if (label === 'typescript' || label === 'javascript') {
      return new tsWorker()
    }
    return new editorWorker()
  }
}

const { metrics, violations } = useAntiCheat()

const { startTimer, stopTimer, resetTimer, startTime, isTracking, formattedTime } = useSolutionTimer()

const finalSolutionTime = ref<string | null>(null);

const editorInstance = ref<any>(null)
let lastChangeTime = Date.now()
const typingSpeed = ref<number[]>([])

const integrityScore = computed(() => {
    if (!code.value) return 100
    return analyzeCodeOriginality(code.value).originalityScore
})

const onEditorMount = (editor: any) => {
    editorInstance.value = editor

    // Отслеживание вставки из буфера обмена
    editor.onDidPaste((e: any) => {
        metrics.value.pasteCount++
        metrics.value.codeChangeTimestamps.push(Date.now())

        if (metrics.value.pasteCount > 3) {
            violations.value.push(`Подозрительное количество вставок: ${metrics.value.pasteCount}`)
        }
    })

    // Отслеживание изменений кода
    editor.onDidChangeModelContent((e: any) => {
        const now = Date.now()
        const timeDiff = now - lastChangeTime

        typingSpeed.value.push(timeDiff)
        metrics.value.codeChangeTimestamps.push(now)

        const lineCount = editor.getModel()?.getLineCount() || 0
        if (lineCount >= 10 && !startTime.value) {
            startTimer()
        }

        if (timeDiff < 10 && e.changes[0]?.text.length > 20) {
            violations.value.push('Обнаружена аномально быстрая вставка кода')
        }

        lastChangeTime = now
    })

    editor.onKeyDown((e: any) => {
        const isCtrlV = (e.ctrlKey || e.metaKey) && e.code === 'KeyV'
        if (isCtrlV) {
            console.warn('Попытка вставки из буфера обмена')
        }
    })
}

type Language = 'javascript' | 'typescript' | 'kotlin' | 'java' | 'kotlin' | 'python' | 'cpp' | 'go';

const STATUS_MESSAGES = {
    IDLE: '',
    CHECKING: 'Проверка...',
    SUCCESS: 'Засчитано!',
    FAILED: 'Не засчитано',
    ERROR: 'Ошибка сети'
} as const;

type StatusMessage = typeof STATUS_MESSAGES[keyof typeof STATUS_MESSAGES];

const code = ref<string>('print("Hello World")');

const selectedLanguage = ref<Language>('python');
const selectedTheme = ref<string>('vs'); // по факту const, можем добавить больше тем в будущем если также внедрим их для всей страницы

const submitionAttempts = ref<number>(0);
const isSubmitting = ref<boolean>(false);

const passedStatus = ref<StatusMessage>(STATUS_MESSAGES.IDLE);

const editorOptions = {
    automaticLayout: true,        // автоподстройка под размер контейнера
    fontSize: 14,
    minimap: { enabled: false },  // убрать миникарту справа
    scrollBeyondLastLine: false,
    wordWrap: 'on',               // перенос длинных строк
    renderValidationDecorations: 'on',
}

const templates: Record<Language, string> = {
    javascript: `console.log('Hello JS');`,
    typescript: `const greeting: string = 'Hello TS';\nconsole.log(greeting);`,
    python: `print("Hello Python")`,
    java: `public class Main {\n    public static void main(String[] args) {\n        System.out.println("Hello Java");\n    }\n}`,
    cpp: `#include <iostream>\n\nint main() {\n    std::cout << "Hello C++" << std::endl;\n    return 0;\n}`,
    go: `package main\n\nimport "fmt"\n\nfunc main() {\n    fmt.Println("Hello Go")\n}`,
    kotlin: `fun main() {\n    println("Hello Kotlin")\n}`
};

const passedStatusClass = computed((): string => {
    switch (passedStatus.value) {
        case STATUS_MESSAGES.SUCCESS: return 'status-success';
        case STATUS_MESSAGES.FAILED: return 'status-failed';
        case STATUS_MESSAGES.ERROR: return 'status-error'
        default: return '';
    }
});

const onLanguageChange = (): void => {
    passedStatus.value = STATUS_MESSAGES.IDLE;

    const currentCodeIsTemplate = Object.values(templates).includes(code.value);
    const codeIsEmpty = !code.value || !code.value.trim();

    if (codeIsEmpty || currentCodeIsTemplate) {
        code.value = templates[selectedLanguage.value || ''];
    }
}

const onCodeChange = (value?: string): void => {
    if (passedStatus.value !== STATUS_MESSAGES.IDLE) {
        passedStatus.value = STATUS_MESSAGES.IDLE
    }
}

const handleSubmit = async (): Promise<void> => {
    if (isSubmitting.value) return;

    submitionAttempts.value++;
    isSubmitting.value = true;
    passedStatus.value = STATUS_MESSAGES.CHECKING;

    finalSolutionTime.value = null;

    const antiCheatData = {
        metrics: metrics.value,
        violations: violations.value,
        codeAnalysis: analyzeCodeOriginality(code.value),
        solutionTime: formattedTime.value
    }

    try {
        await new Promise(resolve => setTimeout(resolve, 1500)); // Имитация запроса к API (здесь будет fetch/axios)

        // ВАЖНО: isSuccess придет с бэкенда НУЖНО ОБНОВИТЬ
        const isSuccess = Math.random() > 0.5; // заглушка для проверки статусов:

        if (isSuccess) {
            passedStatus.value = STATUS_MESSAGES.SUCCESS;
            
            finalSolutionTime.value = formattedTime.value;
            stopTimer();
    
            resetTimer();
        } else {
            passedStatus.value = STATUS_MESSAGES.FAILED;
        }


        passedStatus.value = isSuccess ? STATUS_MESSAGES.SUCCESS : STATUS_MESSAGES.FAILED;
    } catch (e: unknown) {
        console.error(e);
        passedStatus.value = STATUS_MESSAGES.ERROR;
    } finally {
        isSubmitting.value = false;
    }

}
</script>

<style lang="scss">
@keyframes pulse {
    0% { opacity: 1; }
    50% { opacity: 0.7; }
    100% { opacity: 1; }
}

.code-window {
    flex: 2;
    box-sizing: border-box;
    height: 100%;
    min-width: 0;
    background: $clr-light-card;
    border-radius: $radius-1;
    overflow: hidden;
    display: flex;
    flex-direction: column;
}

.code-window-header {
    padding: 16px;
    border-bottom: 2px solid $clr-light-accent;
    display: flex;
    align-items: center;
    justify-content: space-between;
}

.header-controls {
    display: flex;
    gap: 32px;
    
    width: 100%;
    justify-content: space-between;
    align-items: center;

    label {
        display: flex;
        align-items: center;
        gap: 16px;
        font-size: 14px;
        font-family: $font-sans;
        color: $clr-light-main;
    }

    select {
        outline: none;

        padding: 4px 4px;
        border-radius: 4px;
        border: none;
        background: $clr-light-card;
        cursor: pointer;
        font-family: $font-sans;
        font-weight: 900;
        box-shadow: $shadow;
        color: $clr-light-main;
    }
}

.timer-display {
    font-family: $font-mono;
    font-size: 16px;
    color: $clr-light-main;
    padding: 0px;
    align-items: center;
    border-radius: 4px;
    background: transparent;
    transition: all 0.3s ease;
    display: flex;
    gap: 8px;

    &.timer-active {
        color: darken($clr-light-main, 10%);
        animation: pulse 2s infinite;
    }
}

.code-window-content {
    flex: 1 1 auto;
    padding: 16px;
    overflow: hidden; // Monaco сам рендерит скролл
    min-height: 0;
}

.code-window-footer {
    padding: $space-16px;
    border-top: 2px solid $clr-light-accent;

    display: flex;
    align-items: center;
    justify-content: space-between;
    background: $clr-light-card;

    .status-info {
        display: flex;
        align-items: center;
        gap: 16px;
        font-family: $font-sans;
        font-size: $font-size-base;

        .status-badge {
            font-size: $font-size-base;
            font-weight: 400;
            font-family: $font-sans;
            border-left: 2px solid $clr-light-accent;
            padding: 0 0 0 16px;

            color: $clr-light-main;

            &.status-success {
                color: $clr-light-ui-code-passed-text;
            }

            &.status-error {
                color: $clr-light-ui-code-error-text;
            }

            &.status-failed {
                color: $clr-light-ui-code-error-text;
            }
        }

        .attempt-counter {
            font-family: $font-sans;
            font-size: $font-size-base;
            color: $clr-light-main;
        }

        .integrity-score {
            font-family: $font-sans;
            font-size: $font-size-base;
            color: $clr-light-main;
        }
    }

    .code-window-submit-btn {
        padding: 8px 16px;
        background-color: $clr-light-main;
        color: $clr-light-card;
        border: none;
        border-radius: 6px;
        cursor: pointer;
        font-weight: 400;
        font-family: $font-sans;
        transition: opacity 0.2s;

        &:hover {
            opacity: 0.9;
        }

        &:disabled {
            background-color: $clr-light-accent;
            cursor: not-allowed;
        }
    }
}

.integrity-score {}
</style>
