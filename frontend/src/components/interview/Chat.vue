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
        content: `# Привет!
Я готов провести **интервью**.

Вот пример многострочного кода на Vue 3 (Composition API):

\`\`\`vue
<${""}script setup lang="ts">
import { ref } from 'vue';

const count = ref<number>(0);

function increment() {
  count.value++;
}
<${""}/script>

<${""}template>
  <button @click="increment">
    Счетчик: {{ count }}
  </button>
<${""}/template>

<${""}style lang="scss" scoped>
button {
  background-color: #42b883;
  color: white;
  padding: 10px 20px;
  border: none;
  border-radius: 4px;
  cursor: pointer;

  &:hover {
    opacity: 0.9;
  }
}
<${""}/style>
\`\`\`` 
    },
    { role: 'user', content: 'Привет, давай начнем.' },
    { role: 'ai', content: 'Отлично. Расскажи, чем \`ref\` отличается от \`reactive\`?' }
])

const addMessage = (text: string) => {
    messages.value.push({ role: 'user', content: text })
    scrollToBottom()

    setTimeout(() => {
        messages.value.push({
            role: 'ai',
            content: 'Это отличный вопрос! Давай я немного подумаю...\n\nА пока держи еще кусок кода:\n```\n'
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
        border-left: 0px solid $clr-light-accent;
        border-right: 1px solid $clr-light-accent;
        border-top: 1px solid $clr-light-accent;
        border-bottom: 1px solid $clr-light-accent;
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
        background-color: rgba($clr-light-accent, 0.2);
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
}
</style>