<template>
    <div class="chat">
        <div class="chat__content">
            <!-- containing AI responses -->
            <!-- Эмуляция длинного контента для проверки скролла -->
            <p v-for="i in 50" :key="i">Сообщение {{ i }}</p> 
        </div>
        <MessagePanel />
    </div>
</template>

<script setup lang="ts">
import MessagePanel from './MessagePanel.vue'
</script>

<style lang="scss" scoped>
.chat {
    height: 100%;
    flex: 1;
    display: flex;
    flex-direction: column;
    min-width: 0;
    overflow: hidden;
    background: $clr-light-card;
    border-radius: $radius-1;
}

.chat__content {
    flex: 1 1 auto;
    overflow-y: auto;
    padding: 16px 16px 0px 16px;
    min-width: 0;
    min-height: 0;
    /* Messages будут рендериться тут */
    background: $clr-light-card;
}
</style>
