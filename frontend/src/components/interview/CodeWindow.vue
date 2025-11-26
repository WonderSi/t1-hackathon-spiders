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
            </div>
        </div>

        <div class="code-window-content">
            <CodeEditor
                :key="selectedLanguage"
                v-model:value="code"
                :language="selectedLanguage"
                :theme="selectedTheme"
                :options="editorOptions"
                @change="onCodeChange" />
        </div>

        <div class="code-window-footer">
            <div class="status-info">
                <span class="attempt-counter">
                    Попытка: {{ submitionAttempts }}
                </span>

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
import { ref, computed } from 'vue'
import { CodeEditor, type EditorOptions } from 'monaco-editor-vue3'

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
    renderValidationDecorations: 'on'
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

    try {
        await new Promise(resolve => setTimeout(resolve, 1500)); // Имитация запроса к API (здесь будет fetch/axios)

        const isSuccess = Math.random() > 0.5; // заглушка для проверки статусов:

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

    label {
        display: flex;
        align-items: center;
        gap: 16px;
        font-size: 14px;
        font-family: $font-sans;
        color: $clr-light-main;
    }

    select {
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
</style>
