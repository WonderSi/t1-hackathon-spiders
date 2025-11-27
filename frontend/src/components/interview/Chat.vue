<template>
    <div class="chat">
        <div class="chat__content" ref="chatContentRef">
             <!-- containing AI responses -->
            <div 
                v-for="(msg, index) in messages" 
                :key="index"
                class="chat__message"
                :class="{
                    'chat__message--ai': msg.role === 'ai',
                    'chat__message--user': msg.role === 'user' }"
            >
                <!-- Если это AI, рендерим Markdown -->
                <div v-if="msg.role === 'ai'" class="markdown-content" v-html="renderMarkdown(msg.content)"></div>
                <div v-else>{{ msg.content }}</div>
            </div>
        </div>
        <MessagePanel @send="addMessage" />
    </div>
</template>

<script setup lang="ts">
  
import { ref, nextTick, watch, onMounted, onUnmounted } from 'vue'
import MessagePanel from './MessagePanel.vue'
import MarkdownIt from 'markdown-it'
import hljs from 'highlight.js'
import 'highlight.js/styles/atom-one-dark.css'
import { useTasks } from '@/composables/useTasks'
import { useAssessmentStore } from '@/stores/assessment'

interface ChatMessage {
  role: 'ai' | 'user';
  content: string;
}

const { currentTask } = useTasks()
const assessmentStore = useAssessmentStore()

const chatContentRef = ref<HTMLElement | null>(null)

const md = new MarkdownIt({
    html: false, // защита от исполнения кода через скрипты
    linkify: true,
    typographer: true,
})

md.options.highlight = (str: string, lang: string): string => {
    if (lang && hljs.getLanguage(lang)) {
        try {
            return `<pre class="hljs"><code>${hljs.highlight(str, { language: lang, ignoreIllegals: true }).value}</code></pre>`
        } catch (__) {}
    }
    return `<pre class="hljs"><code>${md.utils.escapeHtml(str)}</code></pre>`
}

const renderMarkdown = (text: string): string => md.render(text)

// const messages = ref<ChatMessage[]>([
//     { 
//         role: 'ai', 
//         content: `# Привет!
// Я готов провести **интервью**.

// Вот пример многострочного кода на Vue 3 (Composition API):

// \`\`\`vue
// <${""}script setup lang="ts">
// import { ref } from 'vue';

// const count = ref<number>(0);

// function increment() {
//   count.value++;
// }
// <${""}/script>

// <${""}template>
//   <button @click="increment">
//     Счетчик: {{ count }}
//   </button>
// <${""}/template>

// <${""}style lang="scss" scoped>
// button {
//   background-color: #42b883;
//   color: white;
//   padding: 10px 20px;
//   border: none;
//   border-radius: 4px;
//   cursor: pointer;

//   &:hover {
//     opacity: 0.9;
//   }
// }
// <${""}/style>
// \`\`\`` 
//     },
//     { role: 'user', content: 'Привет, давай начнем.' },
//     { role: 'ai', content: 'Отлично. Расскажи, чем \`ref\` отличается от \`reactive\`?' }
// ])

// Ключ для сохранения сообщений в localStorage
const CHAT_STORAGE_KEY = 'interview_chat_messages' 

// Инициализация сообщений с восстановлением из localStorage
const messages = ref<ChatMessage[]>([])

// ============ LOCALSTORAGE HELPERS ============

// Сохранение state в localStorage
const saveMessagesToLocalStorage  = (): void => {
  try {
    localStorage.setItem(CHAT_STORAGE_KEY, JSON.stringify(messages.value));
    console.log('State задач сохранен в localStorage');
  } catch (error) {
    console.error('Ошибка сохранения state задач:', error);
  }
};

// Восстановление state из localStorage
const restoreMessagesFromLocalStorage  = (): boolean => {
  try {
    const savedMessages = localStorage.getItem(CHAT_STORAGE_KEY);
    if (!savedMessages) {
      console.log('Нет сохраненного state задач');
      return false;
    }

    messages.value = JSON.parse(savedMessages);
    console.log('Сообщения чата восстановлены из localStorage:', messages.value.length, 'сообщений')
    return true
  } catch (error) {
    console.error('Ошибка восстановления state задач:', error);
    return false;
  }
};

// Очистка localStorage
const clearChatLocalStorage  = (): void => {
  localStorage.removeItem(CHAT_STORAGE_KEY);
  console.log('State задач очищен из localStorage');
};

const formatTaskMessage = (task: any): string => {
    return `# 📋 Задача: ${task.subject}

**Сложность:** ${task.estimatedDifficulty}/5  
**Язык:** ${assessmentStore.programmingLanguage}

---

## 📝 Описание

${task.description}

---

Напиши своё решение в редакторе кода справа и отправь на проверку! 🚀`
}


const addMessage = (text: string) => {
    messages.value.push({ role: 'user', content: text })
    scrollToBottom()

    saveMessagesToLocalStorage()
    // ОТПРАВКА РЕШЕНИЯ / ВОПРОСА НА БЭК
    // ПОЛУЧЕНИЕ ОЦЕНКИ / НОВОЙ ЗАДАЧИ

    setTimeout(() => {
        messages.value.push({
            role: 'ai',
            content: 'Это отличный вопрос! Давай я немного подумаю...'
        })
        scrollToBottom()
        saveMessagesToLocalStorage()
    }, 1500)
}

watch(currentTask, (newTask) => {
    if (newTask) {
        console.log('Новая задача получена:', newTask.taskId)
        
        // Добавляем задачу в чат как AI сообщение
        const taskMessage: ChatMessage = {
            role: 'ai',
            content: formatTaskMessage(newTask)
        }
        
        messages.value.push(taskMessage)
        scrollToBottom()
        saveMessagesToLocalStorage()
    }
}, { immediate: true })

// Инициализация при монтировании
onMounted(() => {
    const messagesRestored = restoreMessagesFromLocalStorage()
    
    if (!messagesRestored) {
        // Если сообщений не было сохранено, инициализируем начальное состояние
        if (currentTask.value) {
            // Если есть текущая задача, но нет сохраненных сообщений
            messages.value.push({
                role: 'ai',
                content: formatTaskMessage(currentTask.value)
            })
            saveMessagesToLocalStorage()
        } 
//         else if (assessmentStore.hasActiveSession) {
//             // Если сессия активна, но задачи нет - показываем приветствие
//             messages.value.push({
//                 role: 'ai',
//                 content: `# 👋 Привет!

// Я готов провести техническое интервью.

// **Язык программирования:** ${assessmentStore.programmingLanguage}  
// **Тема:** ${assessmentStore.selectedSubject}  
// **Текущая сложность:** ${assessmentStore.currentDifficulty.toFixed(1)}/5.0

// Жду загрузки первой задачи... ⏳`
//             })
//             saveMessagesToLocalStorage()
//         }
    }
    scrollToBottom()

    const handleFeedback = (e: CustomEvent) => {
    const msg: ChatMessage = { role: 'ai', content: e.detail.feedback }
    messages.value.push(msg)
    scrollToBottom()
    saveMessagesToLocalStorage()
  }

  window.addEventListener('solution-feedback', handleFeedback as EventListener)
  window.addEventListener('interview-complete', handleFeedback as EventListener)

  // Cleanup
  onUnmounted(() => {
    window.removeEventListener('solution-feedback', handleFeedback as EventListener)
    window.removeEventListener('interview-complete', handleFeedback as EventListener)
  })
})

const scrollToBottom = async () => {
    await nextTick()
    const container = document.querySelector('.chat__content')
    if (container) container.scrollTop = container.scrollHeight
}

defineExpose({
    clearChatLocalStorage,
    saveMessagesToLocalStorage,
    restoreMessagesFromLocalStorage
})

</script>

<style lang="scss" scoped>
.chat {
    display: flex;
    flex-direction: column;
    flex: 1;
    min-width: 0;
    height: 100%;
    background: $clr-light-card;
    border-radius: $radius-1;
    overflow: hidden;
    position: relative;
}

.chat__content {
    display: flex;
    flex-direction: column;
    flex: 1; 
    
    gap: 16px;
    padding-inline: 16px;
    padding-top: 20px;
    overflow-y: auto;
    scrollbar-width: thin;
    scrollbar-color: $clr-light-accent transparent;
    color: $clr-light-main; 
}

.chat__message {
    max-width: 85%;
    font-family: $font-sans;

    padding: 16px;

    border-radius: 12px;
    line-height: 1.5;
    font-size: $font-size-base;

    white-space: pre-wrap;
    word-break: break-word;

    &--ai {
        align-self: flex-start;
        background-color: $clr-light-card;

        border-left: 0px solid $clr-light-accent;
        border-right: 1px solid $clr-light-accent;
        border-top: 1px solid $clr-light-accent;
        border-bottom: 1px solid $clr-light-accent;
    }

    &--user {
        align-self: flex-end;
        background-color: $clr-light-accent;
        color: $clr-light-card;
    }
}

.markdown-content {
    white-space: normal;
    
    :deep() { 
        h1, h2, h3, h4 {
            margin: 0.5em 0;
            font-weight: 600;
            line-height: 1.3;
        }
        h1 { font-size: 1.5em; }
        h2 { font-size: 1.3em; }
        
        p { 
            margin-bottom: 0.8em; 
            &:last-child { margin-bottom: 0; } 
        }
        
        ul, ol { 
            margin-bottom: 0.8em; 
            padding-left: 1.2em;
             &:last-child { margin-bottom: 0; }
        }
        li { margin-bottom: 0.2em; }

        code:not(pre code) {
            background-color: rgba($clr-light-accent, 0.15);
            padding: 2px 4px;
            border-radius: 4px;
            font-family: 'Fira Code', monospace;
            font-size: 0.9em;
            color: $clr-light-code-md;
        }

        pre {
            margin: 10px 0;
            padding: 12px;
            border-radius: 8px;
            overflow-x: auto;
            background: $clr-light-main;
            
            code {
                font-family: 'Fira Code', monospace;
                background: transparent;
                padding: 0;
                color: $clr-light-card;
            }
        }
        
        a { color: $clr-light-accent; text-decoration: underline; }

        & > *:first-child {
            margin-top: 0;
        }
        & > *:last-child {
            margin-bottom: 0;
        }
    }
}
</style>
