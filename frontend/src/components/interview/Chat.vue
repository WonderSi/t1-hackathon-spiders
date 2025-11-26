<template>
    <div class="chat">
        <div class="chat__content">
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
import { ref, nextTick } from 'vue'
import MessagePanel from './MessagePanel.vue'
import MarkdownIt from 'markdown-it'
import hljs from 'highlight.js'
import 'highlight.js/styles/atom-one-dark.css'

interface ChatMessage {
  role: 'ai' | 'user';
  content: string;
}

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

const renderMarkdown = (text: string) => md.render(text)

const messages = ref<ChatMessage[]>([
    { 
        role: 'ai', 
        content: '# Привет!\nЯ готов провести **интервью**.\n\nВот пример кода на Vue:\n``````' 
    },
    { role: 'user', content: 'Привет, давай начнем.' },
    { role: 'ai', content: 'Отлично. Расскажи, чем `ref` отличается от `reactive`?' }
])

// --- Логика отправки с заглушкой ---
const addMessage = (text: string) => {
    // 1. Добавляем сообщение юзера
    messages.value.push({ role: 'user', content: text })
    scrollToBottom()

    // 2. Имитация задержки ответа AI
    setTimeout(() => {
        messages.value.push({
            role: 'ai',
            content: 'Это отличный вопрос! Давай я немного подумаю...\n\nА пока держи еще кусок кода:\n``````'
        })
        scrollToBottom()
    }, 1500)
}

const scrollToBottom = async () => {
    await nextTick()
    const container = document.querySelector('.chat__content')
    if (container) container.scrollTop = container.scrollHeight
}
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

    padding-left: 16px;
    padding-right: 16px;
    padding-bottom: 16px;
    padding-top: 0px; 

    border-radius: 12px;
    line-height: 1.5;
    font-size: $font-size-base;

    &--ai {
        align-self: flex-start;
        background-color: $clr-light-card;
        border-bottom: 1px solid $clr-light-accent;
        border-top: 1px solid $clr-light-accent;
    }

    &--user {
        padding-top: 16px; 
        align-self: flex-end;
        background-color: $clr-light-accent;
        color: $clr-light-card;
    }
}

.markdown-content :deep() {
    h1, h2, h3, h4 { margin: 0.5em 0; font-weight: 600; }
    h1 { font-size: 1.5em; }
    h2 { font-size: 1.3em; }
    p { margin-bottom: 0.8em; &:last-child { margin-bottom: 0; } }
    ul, ol { margin-bottom: 0.8em; padding-left: 1.2em; }
    li { margin-bottom: 0.2em; }
    
    code:not(pre code) {
        background-color: rgba(0, 0, 0, 0.06);
        padding: 2px 4px;
        border-radius: 4px;
        font-family: 'Fira Code', monospace;
        font-size: 0.9em;
        color: #d63384;
    }

    pre {
        margin: 10px 0;
        padding: 12px;
        border-radius: 8px;
        overflow-x: auto;
        background: #282c34;
        
        code {
            font-family: 'Fira Code', monospace;
            background: transparent;
            padding: 0;
            color: #abb2bf; // Светло-серый цвет фона кода
        }
    }
    
    a { color: $clr-light-accent; text-decoration: underline; }
}
</style>
