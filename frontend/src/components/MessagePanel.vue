<template>
    <div class="chat__message-panel">
        <textarea
            class="chat__input-message"
            placeholder="Задайте ваш вопрос"
            v-model="userMessage"
            @input="resizeTextArea"
            :rows="1"
            ref="textareaRef"
            @keyup.enter="sendMessage"
        ></textarea>
        <button 
            class="chat__btn-send" 
            title="Send"
            @click="sendMessage"
            :disabled="loading || !userMessage.trim()"
            aria-label="Отправить сообщение">
            <svg class="chat__sent-button" width="18" height="15" viewBox="0 0 18 15" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path fill-rule="evenodd" clip-rule="evenodd" d="M0.888015 0.0500098C0.769779 0.000307048 0.639337 -0.0127245 0.513606 0.0126054C0.387874 0.0379352 0.272653 0.100458 0.182886 0.192065C0.0931184 0.283672 0.0329456 0.400137 0.010171 0.526356C-0.0126036 0.652576 0.00307091 0.782726 0.0551614 0.89993L2.69452 6.82633H9.17303C9.34277 6.82633 9.50555 6.89376 9.62558 7.01378C9.7456 7.1338 9.81303 7.29659 9.81303 7.46633C9.81303 7.63607 9.7456 7.79885 9.62558 7.91888C9.50555 8.0389 9.34277 8.10633 9.17303 8.10633H2.69452L0.0551614 14.0327C0.00307091 14.1499 -0.0126036 14.2801 0.010171 14.4063C0.0329456 14.5325 0.0931184 14.649 0.182886 14.7406C0.272653 14.8322 0.387874 14.8947 0.513606 14.9201C0.639337 14.9454 0.769779 14.9324 0.888015 14.8826L17.1013 8.05598C17.2173 8.00705 17.3163 7.92502 17.3858 7.82014C17.4554 7.71526 17.4925 7.59219 17.4925 7.46633C17.4925 7.34047 17.4554 7.2174 17.3858 7.11252C17.3163 7.00764 17.2173 6.92561 17.1013 6.87668L0.888015 0.0500098Z"/>
            </svg>
        </button>
        <div v-if="error" class="chat__error">{{ error }}</div>
    </div>
</template>


<script setup lang="ts">

import { ref, nextTick } from 'vue';

const userMessage = ref('');
const textareaRef = ref<HTMLTextAreaElement | null>(null);
const maxLines = 10;

const loading = ref(false);
const error = ref ('');

// adjusting for amount of lines
function resizeTextArea() {
  const textarea = textareaRef.value;
  if (textarea) {
    textarea.style.height = 'auto';
    const lineHeight = parseFloat(getComputedStyle(textarea).lineHeight) || 22; // px
    const lines = textarea.value.split('\n').length;
    // Ограничиваем максимальную высоту textarea
    const maxHeight = lineHeight * maxLines;
    textarea.style.height = `${Math.min(textarea.scrollHeight, maxHeight)}px`;
    // Если строк больше maxLines, появляется скролл
    textarea.style.overflowY = (textarea.scrollHeight > maxHeight) ? 'auto' : 'hidden';
  }
}

// send to AI
async function sendMessage() {
    const message = userMessage.value.trim();
    if (!message) return;
    loading.value = true;
    error.value='';
    try {
        await new Promise(res => setTimeout(res, 1000)); // demo
        userMessage.value = '';
    } catch (e) {
        error.value = 'Ошибка отправки сообщения';

    } finally {
        loading.value = false;
    }
    userMessage.value = '';
    await nextTick();
    resizeTextArea();
}

</script>

<style lang="scss" scoped>
@use "../scss/components/_messagePanel.scss";
</style>