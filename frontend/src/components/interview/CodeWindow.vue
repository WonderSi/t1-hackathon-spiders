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
                <span v-if="passedStatus"
                :class="['status-badge', passedStatusClass]">
                {{ passedStatus }}
            </span>

            <span class="attempt-counter">
                Попытка: {{ submitionAttempts }}
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
import { ref } from 'vue'
import { CodeEditor, type EditorOptions } from 'monaco-editor-vue3'

const code = ref<string>('');
const selectedLanguage = ref<string>('python');
const selectedTheme = ref<string>('vs');

const submitionAttempts = ref<string>('');
const passedStatus = ref<string>('');

const editorOptions = {
    automaticLayout: true,        // автоподстройка под размер контейнера
    fontSize: 14,
    minimap: { enabled: false },  // убрать миникарту справа
    scrollBeyondLastLine: false,
    wordWrap: 'on',               // перенос длинных строк
}

const onLanguageChange = () => {
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

.code-window-content {
    flex: 1 1 auto;
    padding: 16px;
    overflow-y: auto;
    min-height: 0;

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
        gap: 2px;
        font-size: 14px;
    }

    select {
        padding: 4px 8px;
        border-radius: 4px;
        border: none;
        background: $clr-light-card;
        cursor: pointer;
    }
}

.code-window-content {
    flex: 1 1 auto;
    min-height: 0; // важно для корректного overflow
    overflow: hidden; // Monaco сам рендерит скролл
}

.submit-btn {

}
</style>
