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
                v-model:value="code" 
                :language="selectedLanguage" 
                :theme="selectedTheme"
                :options="editorOptions" 
            />
        </div>

        <div class="code-window-footer">
            <div class="status-info">
                <span class="attempt-counter">
                    Попытка: {{ submitionAttempts }}
                </span>

                <span v-if="passedStatus"
                    :class="['status-badge', passedStatusClass]">
                    {{ passedStatus }}
                </span>
            </div>

            <button 
                class="code-window-submit-btn"
                @click="handleSubmit"
                :disabled="isSubmitting"
            >{{ isSubmitting ? 'Отправка...' : 'Отправить' }}</button>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { CodeEditor, type EditorOptions } from 'monaco-editor-vue3'

const code = ref<string>('');
const selectedLanguage = ref<string>('python');
const selectedTheme = ref<string>('vs');

const submitionAttempts = ref<number>(0);
const isSubmitting = ref<boolean>(false);
const passedStatus = ref<string>('');

const editorOptions = {
    automaticLayout: true,        // автоподстройка под размер контейнера
    fontSize: 14,
    minimap: { enabled: false },  // убрать миникарту справа
    scrollBeyondLastLine: false,
    wordWrap: 'on',               // перенос длинных строк
}

const passedStatusClass = computed (() => {
    if (passedStatus.value === 'Засчитано!') return 'status-success';
    if (passedStatus.value === 'Не засчитано') return 'status-error';
    return '';
});

const onLanguageChange = () => {
    passedStatus.value = '';
}

const onCodeChange = () => {
    if (passedStatus.value) passedStatus.value = '';
}

const handleSubmit = async () => {
    if (isSubmitting.value) return;

    submitionAttempts.value++;
    isSubmitting.value = true;
    passedStatus.value = 'Проверка...';

    try {
        // Имитация запроса к API (здесь будет fetch/axios)
        await new Promise(resolve => setTimeout(resolve, 1500));
        // заглушка для проверки статусов:
        const isSuccess = Math.random() > 0.5;
        passedStatus.value = isSuccess ? 'Засчитано!' : 'Не засчитано';
    } catch (e) {
        console.error(e);
        passedStatus.value = 'Не засчитано';
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
.status-success { color: $clr-light-ui-code-passed-text; }
.status-error { color: $clr-light-ui-code-error-text; }
</style>
