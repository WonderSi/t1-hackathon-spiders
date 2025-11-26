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
    display: flex;
    flex-direction: column;
    flex: 1;
    min-width: 0;
    height: 100%;
    background: $clr-light-card;
    border-radius: $radius-1;
    overflow: hidden;
}

.chat__content {
    padding-inline: 16px;
    overflow-y: auto;
    scrollbar-width: thin;
    scrollbar-color: $clr-light-accent transparent;
    color: $clr-light-main;
}
</style>
